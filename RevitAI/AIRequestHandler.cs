using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using RevitAI.Services;
using RevitAI.Models;
using System;
using System.Collections.Generic;

namespace RevitAI
{
    /// <summary>
    /// 处理 Revit 外部事件，确保在主线程上下文中修改模型
    /// </summary>
    public class AIRequestHandler : IExternalEventHandler
    {
        // 用于传递 AI 解析出的任务列表
        public List<RevitTask>? Tasks { get; set; }

        public void Execute(UIApplication app)
        {
            try
            {
                if (Tasks == null || Tasks.Count == 0) return;

                UIDocument uidoc = app.ActiveUIDocument;
                if (uidoc == null) return;

                Document doc = uidoc.Document;

                // 使用 RevitTaskProcessor 处理任务
                var processor = new RevitTaskProcessor(doc);
                processor.ProcessTasks(Tasks);

                TaskDialog.Show("成功", $"已成功处理 {Tasks.Count} 个任务。");
            }
            catch (Exception ex)
            {
                TaskDialog.Show("错误", $"任务执行失败: {ex.Message}");
            }
        }

        public string GetName()
        {
            return "AI Task Handler";
        }
    }
}
