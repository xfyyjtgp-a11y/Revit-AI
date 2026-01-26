using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using RevitAI.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace RevitAI.ViewModels
{
    public partial class RevitFamilyViewModel : ObservableObject
    {
        private ObservableCollection<RevitFamily> _families = new ObservableCollection<RevitFamily>();
        public ObservableCollection<RevitFamily> Families
        {
            get => _families;
            set => SetProperty(ref _families, value);
        }

        public IAsyncRelayCommand OpenFileCommand { get; }

        public RevitFamilyViewModel()
        {
            OpenFileCommand = new AsyncRelayCommand(OpenFile);
          
        }
        
        private async Task OpenFile()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Revit Files (*.rvt)|*.rvt|All Files (*.*)|*.*",
                Title = "选择 Revit 项目文件"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                await ParseRevitFile(openFileDialog.FileName);
            }
        }

        private async Task ParseRevitFile(string filePath)
        {
            // 清空现有数据
            Families.Clear();

            if (ConstantParameter.CommandData == null)
            {
                // 如果没有 Revit 上下文（例如独立运行时），则无法读取
                return;
            }

            try
            {
                // 获取 Revit Application 对象
                var app = ConstantParameter.CommandData.Application.Application;
                
                // 在内存中打开文档 (Open document in memory)
                // 注意：此操作在主线程执行，大文件可能会短暂阻塞 UI
                Document doc = app.OpenDocumentFile(filePath);

                try
                {
                    // 使用 FilteredElementCollector 获取所有族
                    var collector = new FilteredElementCollector(doc).OfClass(typeof(Family));
                    var families = collector.ToElements();

                    foreach (Element elem in families)
                    {
                        if (elem is Family family)
                        {
                            // 过滤掉不可编辑的系统族（视需求而定，通常用户加载的族是可编辑的）
                            if (!family.IsEditable) continue;

                            var revitFamily = new RevitFamily
                            {
                                Name = family.Name
                            };

                            // 获取该族下的所有类型 (FamilySymbol)
                            var symbolIds = family.GetFamilySymbolIds();
                            foreach (ElementId id in symbolIds)
                            {
                                Element symbol = doc.GetElement(id);
                                if (symbol != null)
                                {
                                    revitFamily.Types.Add(new RevitFamilyType
                                    {
                                        Name = symbol.Name
                                    });
                                }
                            }

                            // 只有当族包含类型或者确实是有效族时才添加
                            Families.Add(revitFamily);
                        }
                    }
                }
                finally
                {
                    // 务必关闭文档，释放内存，false 表示不保存修改
                    doc.Close(false);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error parsing Revit file: {ex.Message}");
            }
        }

    }
}
