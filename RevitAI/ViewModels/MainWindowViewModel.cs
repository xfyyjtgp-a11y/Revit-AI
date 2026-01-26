using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Autodesk.Revit.UI;
using RevitAI.Views;
using System.Windows;

namespace RevitAI.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly ExternalEvent _externalEvent;
        private readonly AIRequestHandler _requestHandler;
        private readonly string _apiKey;

        private InputWindow? _inputWindow;
        private RevitFamilyView? _revitFamilyView;

        public MainWindowViewModel(ExternalEvent externalEvent, AIRequestHandler requestHandler, string apiKey)
        {
            _externalEvent = externalEvent;
            _requestHandler = requestHandler;
            _apiKey = apiKey;
        }

        [RelayCommand]
        private void OpenInputWindow()
        {
            if (_inputWindow == null)
            {
                _inputWindow = new InputWindow(_externalEvent, _requestHandler, _apiKey);
                _inputWindow.Closed += (s, e) => _inputWindow = null;
                _inputWindow.Show();
            }
            else
            {
                _inputWindow.Activate();
                if (_inputWindow.WindowState == WindowState.Minimized)
                {
                    _inputWindow.WindowState = WindowState.Normal;
                }
            }
        }

        [RelayCommand]
        private void OpenRevitFamilyView()
        {
            if (_revitFamilyView == null)
            {
                _revitFamilyView = new RevitFamilyView();
                _revitFamilyView.Closed += (s, e) => _revitFamilyView = null;
                _revitFamilyView.Show();
            }
            else
            {
                _revitFamilyView.Activate();
                if (_revitFamilyView.WindowState == WindowState.Minimized)
                {
                    _revitFamilyView.WindowState = WindowState.Normal;
                }
            }
        }
    }
}
