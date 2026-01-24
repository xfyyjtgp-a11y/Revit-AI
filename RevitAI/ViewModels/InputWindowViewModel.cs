using System;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RevitAI.Services;
using RevitAI.Isolation;
using System.Text.Json;
using System.Collections.Generic;
using RevitAI.Models;

namespace RevitAI.ViewModels
{
    public partial class InputWindowViewModel : ObservableObject
    {
        private readonly ExternalEvent _externalEvent;
        private readonly AIRequestHandler _requestHandler;
        private readonly string _apiKey;

        [ObservableProperty]
        private string _inputText = "帮我创建一堵长5米，高3米的墙。";

        [ObservableProperty]
        private string _statusText = "准备就绪";

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(GenerateCommand))]
        private bool _isBusy;

        public InputWindowViewModel(ExternalEvent externalEvent, AIRequestHandler requestHandler, string apiKey)
        {
            _externalEvent = externalEvent;
            _requestHandler = requestHandler;
            _apiKey = apiKey;
        }

        private bool CanGenerate() => !IsBusy;

        [RelayCommand(CanExecute = nameof(CanGenerate))]
        private async Task GenerateAsync()
        {
            try
            {
                IsBusy = true;
                StatusText = "正在思考...";

                // 使用隔离环境运行 AI 服务
                string? jsonResult = await IsolatedRunner.RunProcessRequestAsync(_apiKey, InputText);

                if (!string.IsNullOrEmpty(jsonResult))
                {
                    var tasks = JsonSerializer.Deserialize<List<RevitTask>>(jsonResult);

                    if (tasks != null && tasks.Count > 0)
                    {
                        StatusText = $"解析出 {tasks.Count} 个任务，正在发送到 Revit...";
                        _requestHandler.Tasks = tasks;
                        _externalEvent.Raise();
                        StatusText = "指令已发送，等待 Revit 执行。";
                    }
                    else
                    {
                        StatusText = "未能解析出有效的 Revit 任务。";
                    }
                }
                else
                {
                    StatusText = "未能解析出有效的 Revit 任务。";
                }
            }
            catch (Exception ex)
            {
                StatusText = $"错误: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
