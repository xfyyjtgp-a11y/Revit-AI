using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitAI.Views;
using RevitAI.Isolation;
using RevitAI.Models;
using RevitAI.Services;
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
        
        // 保持对 Window 的静态引用，避免重复打开或被 GC 回收
        public static MainWindow? _mainWindow;

        static Command()
        {
            // 注册程序集解析器，帮助找到同一目录下的依赖项 (如 Semantic Kernel)
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
                if (_mainWindow == null || !_mainWindow.IsLoaded)
                {
                    // Initialize Revit Context
                    ConstantParameter.CommandData = commandData;

                    var handler = new AIRequestHandler();
                    var exEvent = ExternalEvent.Create(handler);
                    _mainWindow = new MainWindow(exEvent, handler, OPENAI_API_KEY);
                }
                
                _mainWindow.Show();
                _mainWindow.Activate();
                
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
