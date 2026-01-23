using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.UI;
using RevitAI.Isolation;
using RevitAI.Services;
using RevitAI.Models;

namespace RevitAI.Views
{
    public partial class InputWindow : Window
    {
        private readonly ExternalEvent _externalEvent;
        private readonly AIRequestHandler _requestHandler;
        private readonly string _apiKey;

        public InputWindow(ExternalEvent externalEvent, AIRequestHandler handler, string apiKey)
        {
            InitializeComponent();
            _externalEvent = externalEvent;
            _requestHandler = handler;
            _apiKey = apiKey;
        }

        public string PromptText => InputTextBox.Text;

        private async void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            string promptText = InputTextBox.Text;
            if (string.IsNullOrWhiteSpace(promptText))
            {
                MessageBox.Show("请输入有效的指令。", "提示");
                return;
            }

            // 更新 UI 状态
            StatusText.Text = "正在思考中 (AI Processing)...";
            GenerateButton.IsEnabled = false;
            InputTextBox.IsEnabled = false;

            try
            {
                // 在后台线程运行 AI 逻辑，不阻塞 UI 线程
                List<RevitTask>? tasks = await Task.Run(async () =>
                {
                    string? jsonResult = await IsolatedRunner.RunProcessRequestAsync(_apiKey, promptText);
                    if (!string.IsNullOrEmpty(jsonResult))
                    {
                        return System.Text.Json.JsonSerializer.Deserialize<List<RevitTask>>(jsonResult);
                    }
                    return null;
                });

                if (tasks != null && tasks.Count > 0)
                {
                    StatusText.Text = "AI 解析成功，正在生成模型...";
                    
                    // 将数据传递给 Handler
                    _requestHandler.Tasks = tasks;
                    
                    // 触发外部事件，通知 Revit 在主线程执行
                    _externalEvent.Raise();

                    // 可以在这里关闭窗口，或者等待完成后手动关闭
                    // 为了演示流程，我们保持窗口开启，直到用户手动关闭
                    StatusText.Text = "指令已发送给 Revit，请检查模型。";
                    GenerateButton.IsEnabled = true;
                    InputTextBox.IsEnabled = true;
                }
                else
                {
                    StatusText.Text = "AI 无法理解该指令，请重试。";
                    GenerateButton.IsEnabled = true;
                    InputTextBox.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = "发生错误";
                MessageBox.Show($"AI 服务调用失败: {ex.Message}", "错误");
                GenerateButton.IsEnabled = true;
                InputTextBox.IsEnabled = true;
            }
        }
    }
}
