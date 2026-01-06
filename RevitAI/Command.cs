using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitAI.Services;
using RevitAI.Views;
using RevitAI.Isolation;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

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
                    WallRequest? request = null;
                    
                    try
                    {
                        // 使用 IsolatedRunner 在独立上下文中运行 AI
                        string? jsonResult = Task.Run(async () => 
                        {
                            return await IsolatedRunner.RunParseWallRequestAsync(apiKey, inputWindow.PromptText);
                        }).GetAwaiter().GetResult();

                        if (!string.IsNullOrEmpty(jsonResult))
                        {
                            request = System.Text.Json.JsonSerializer.Deserialize<WallRequest>(jsonResult);
                        }
                    }
                    catch (Exception ex)
                    {
                        TaskDialog.Show("AI Error", $"AI 服务调用失败: {ex.Message}\n\nStack: {ex.StackTrace}\nInner: {ex.InnerException?.Message}");
                        return Result.Failed;
                    }

                    if (request != null)
                    {
                        // 4. 执行 Revit 模型创建
                        var creator = new RevitModelCreator(commandData.Application.ActiveUIDocument.Document);
                        creator.CreateWall(request);
                        
                        TaskDialog.Show("成功", $"已成功创建墙体：\n长度: {request.Length}m\n高度: {request.Height}m\n标高: {request.LevelName}");
                    }
                    else
                    {
                        TaskDialog.Show("AI Error", "AI 无法解析您的请求，请重试。");
                        return Result.Failed;
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
