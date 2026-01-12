using System;
using System.Reflection;
using System.Runtime.Loader;

namespace RevitAI.Isolation
{
    /// <summary>
    /// Custom AssemblyLoadContext to isolate plugin dependencies.
    /// </summary>
    public class PluginLoadContext : AssemblyLoadContext
    {
        private readonly string _pluginPath;

        public PluginLoadContext(string pluginPath) : base(isCollectible: true)
        {
            _pluginPath = pluginPath;
        }
        /// <summary>
        /// 尝试从插件目录加载程序集
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <returns></returns>
        protected override Assembly? Load(AssemblyName assemblyName)
        {
            // 尝试从插件目录加载
            string assemblyPath = System.IO.Path.Combine(_pluginPath, $"{assemblyName.Name}.dll");
            if (System.IO.File.Exists(assemblyPath))
            {
                // 使用 LoadFromAssemblyPath 加载到此 Context
                return LoadFromAssemblyPath(assemblyPath);
            }
            
            // 默认回退到默认上下文 (System libs 等)
            return null;
        }
    }
}
