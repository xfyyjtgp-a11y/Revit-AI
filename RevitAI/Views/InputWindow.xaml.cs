using System;
using System.Windows;
using Autodesk.Revit.UI;
using RevitAI.ViewModels;

namespace RevitAI.Views
{
    public partial class InputWindow : Window
    {
        public InputWindow(ExternalEvent externalEvent, AIRequestHandler requestHandler, string apiKey)
        {
            InitializeComponent();
            DataContext = new InputWindowViewModel(externalEvent, requestHandler, apiKey);
        }
    }
}
