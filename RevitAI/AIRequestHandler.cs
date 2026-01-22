using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using RevitAI.Services;
using System;

namespace RevitAI
{
    /// <summary>
    /// 处理 Revit 外部事件，确保在主线程上下文中修改模型
    /// </summary>
    public class AIRequestHandler : IExternalEventHandler
    {
        // 用于传递 AI 解析出的请求数据
        public WallRequest? Request { get; set; }

        public void Execute(UIApplication app)
        {
            try
            {
                if (Request == null) return;

                UIDocument uidoc = app.ActiveUIDocument;
                if (uidoc == null) return;

                Document doc = uidoc.Document;

                // 使用 RevitModelCreator 创建墙体
                var creator = new RevitModelCreator(doc);
                creator.CreateWall(Request);

                TaskDialog.Show("成功", $"已成功创建墙体：\n长度: {Request.Length}m\n高度: {Request.Height}m\n标高: {Request.LevelName}");
            }
            catch (Exception ex)
            {
                TaskDialog.Show("错误", $"创建墙体失败: {ex.Message}");
            }
        }

        public string GetName()
        {
            return "AI Wall Creator Handler";
        }
    }
}
