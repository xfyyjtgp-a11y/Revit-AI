using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitAI.Views;
using System;
using System.IO;
using System.Reflection;

namespace RevitAI
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        // ⚠️ 请在此处替换为您自己的 OpenAI API Key
        // 或者配置环境变量 "OPENAI_API_KEY"
        private const string OPENAI_API_KEY = "sk-wiwex9n08EZK9GwacRy4u4s3vJNykHRnc17vTLjxkrJ7kSBf";
        
        // 保持对 Window 的静态引用，避免重复打开或被 GC 回收
        public static InputWindow? _inputWindow;

        static Command()
        {
            // 注册程序集解析器，帮助找到同一目录下的依赖项 (如 Semantic Kernel)
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

                // 2. 检查窗口是否已打开
                if (_inputWindow != null && _inputWindow.IsLoaded)
                {
                    _inputWindow.Activate();
                    return Result.Succeeded;
                }

                // 3. 初始化外部事件
                // 创建 Handler (处理模型修改)
                AIRequestHandler handler = new AIRequestHandler();
                // 创建 ExternalEvent (注册到 Revit)
                ExternalEvent exEvent = ExternalEvent.Create(handler);

                // 4. 显示非模态窗口 (Modeless)
                // 窗口不再阻塞 Revit 主线程，用户可以继续操作 Revit
                // 当点击生成时，窗口会在后台运行 AI，然后通过 exEvent 通知 Revit 修改模型
                _inputWindow = new InputWindow(exEvent, handler, apiKey);
                
                // 设置窗口 Owner 为 Revit 主窗口 (可选，防止窗口跑到 Revit 后面)
                // 这里简单使用 .Show()
                _inputWindow.Show();

                // 5. 立即返回 Succeeded
                // Revit 认为命令已完成，恢复响应。实际的 AI 任务在后台继续。
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
