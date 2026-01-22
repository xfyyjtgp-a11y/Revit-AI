using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitAI.Services;
using RevitAI.Views;
using RevitAI.Isolation;
using RevitAI.Models;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace RevitAI
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        // ⚠️ 请在此处替换为您自己的 OpenAI API Key
        // 或者配置环境变量 "OPENAI_API_KEY"
        private const string OPENAI_API_KEY = "sk-wiwex9n08EZK9GwacRy4u4s3vJNykHRnc17vTLjxkrJ7kSBf";

        static Command()
        {
            // 注册程序集解析器，帮助找到同一目录下的依赖项 (如 Semantic Kernel)
            // 虽然有了 Isolation，但保留这个以防万一
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }
        /// <summary>
        /// 程序集解析器，用于在当前域中解析依赖项
        /// </summary>
        /// <param name="sender">当前域对象，用于解析程序集</param>
        /// <param name="args">解析事件参数，包含要解析的程序集名称</param>
        /// <returns>解析后的程序集 如果解析成功；否则为 null</returns>
        private static Assembly? CurrentDomain_AssemblyResolve(object? sender, ResolveEventArgs args)
        {
            try
            {
                string folderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
                string assemblyName = new AssemblyName(args.Name).Name + ".dll";
                string assemblyPath = Path.Combine(folderPath, assemblyName);

                if (File.Exists(assemblyPath))
                {
                    return Assembly.LoadFrom(assemblyPath);
                }
            }
            catch (Exception) { }
            return null;
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                // 1. 获取 API Key
                string apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? OPENAI_API_KEY;
                
                if (apiKey.Contains("YOUR_OPENAI_API_KEY"))
                {
                    TaskDialog.Show("配置错误", "请配置有效的 OpenAI API Key。");
                    return Result.Cancelled;
                }

                // 2. 显示 UI
                var inputWindow = new InputWindow();
                bool? dialogResult = inputWindow.ShowDialog();

                if (inputWindow.IsConfirmed && !string.IsNullOrWhiteSpace(inputWindow.PromptText))
                {
                    // 3. 调用 AI 解析意图 (使用隔离上下文)
                    List<RevitTask>? tasks = null;
                    
                    try
                    {
                        // 使用 IsolatedRunner 在独立上下文中运行 AI
                        string? jsonResult = Task.Run(async () => 
                        {
                            return await IsolatedRunner.RunProcessRequestAsync(apiKey, inputWindow.PromptText);
                        }).GetAwaiter().GetResult();

                        if (!string.IsNullOrEmpty(jsonResult))
                        {
                            tasks = System.Text.Json.JsonSerializer.Deserialize<List<RevitTask>>(jsonResult);
                        }
                    }
                    catch (Exception ex)
                    {
                        TaskDialog.Show("AI Error", $"AI 服务调用失败: {ex.Message}\n\nStack: {ex.StackTrace}\nInner: {ex.InnerException?.Message}");
                        return Result.Failed;
                    }

                    if (tasks != null && tasks.Count > 0)
                    {
                        // 4. 执行 Revit 模型创建
                        var processor = new RevitTaskProcessor(commandData.Application.ActiveUIDocument.Document);
                        processor.ProcessTasks(tasks);
                        
                        TaskDialog.Show("成功", $"已成功处理 {tasks.Count} 个任务。");
                    }
                    else
                    {
                        TaskDialog.Show("提示", "AI 未能识别出有效的建模指令。");
                    }
                }

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}
