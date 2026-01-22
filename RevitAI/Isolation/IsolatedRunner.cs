using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace RevitAI.Isolation
{
    public static class IsolatedRunner
    {
        /// <summary>
        /// 在隔离的 AssemblyLoadContext 中运行 AI 逻辑，避免依赖冲突。
        /// </summary>
        public static async Task<string?> RunProcessRequestAsync(string apiKey, string userInput)
        {
            // 获取当前程序集路径 (RevitAI.dll)
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            // 获取插件目录路径 (RevitAI.Plugins.dll)
            string pluginPath = Path.GetDirectoryName(assemblyPath)!;

            // 创建隔离上下文
            // 原理说明：
            // 当我们在该上下文中加载程序集或执行代码时，如果遇到未加载的依赖项（如 SemanticKernel.dll），
            // CLR 会自动调用 PluginLoadContext.Load(AssemblyName) 方法。
            // 我们不需要手动调用 Load 方法，它是由运行时在解析依赖时自动触发的"钩子"。
            var context = new PluginLoadContext(pluginPath);

            try
            {
                // 1. 在隔离上下文中加载当前程序集
                // 注意：必须重新加载 RevitAI.dll，因为我们需要其中的 AIService 类型
                // 该类型将绑定到隔离上下文中的 Microsoft.SemanticKernel
                Assembly isolatedAssembly = context.LoadFromAssemblyPath(assemblyPath);

                // 2. 通过反射获取 AIService 类型
                Type? aiServiceType = isolatedAssembly.GetType("RevitAI.Services.AIService");
                if (aiServiceType == null) throw new InvalidOperationException("Could not find AIService type in isolated assembly.");

                // 3. 创建实例
                object? instance = Activator.CreateInstance(aiServiceType, new object[] { apiKey, "gpt-4o-mini" });

                // 4. 获取方法 ProcessRequestJsonAsync
                MethodInfo? method = aiServiceType.GetMethod("ProcessRequestJsonAsync");
                
                if (method == null)
                {
                     throw new InvalidOperationException("Method ProcessRequestJsonAsync not found.");
                }

                // 5. 调用方法
                Task<string?> task = (Task<string?>)method.Invoke(instance, new object[] { userInput })!;
                
                return await task.ConfigureAwait(false);
            }
            finally
            {
                // 卸载上下文
                context.Unload();
            }
        }
    }
}
