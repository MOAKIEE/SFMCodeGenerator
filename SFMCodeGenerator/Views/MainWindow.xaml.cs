using System.ComponentModel;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using SFMCodeGenerator.ViewModels;

namespace SFMCodeGenerator.Views
{
    public partial class MainWindow : Window
    {
        private MainViewModel? _viewModel;
        
        // 高亮颜色
        private static readonly SolidColorBrush HighlightBrush = new(Color.FromRgb(255, 255, 100)); // 亮黄色
        private static readonly SolidColorBrush TextBrush = new(Color.FromRgb(26, 26, 26)); // 深色文本

        public MainWindow()
        {
            InitializeComponent();
            
            // 窗口加载后初始化
            Loaded += OnWindowLoaded;
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            _viewModel = DataContext as MainViewModel;
            
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged += OnViewModelPropertyChanged;
                UpdateCodePreview();
            }
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.GeneratedCode) || 
                e.PropertyName == nameof(MainViewModel.SelectedStatement))
            {
                // 使用 Dispatcher 确保在 UI 线程上更新
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(UpdateCodePreview));
            }
        }

        private void UpdateCodePreview()
        {
            if (_viewModel == null) return;

            var code = _viewModel.GeneratedCode;
            var selectedStatement = _viewModel.SelectedStatement;
            
            // 获取选中语句的代码（用于匹配高亮）
            string? selectedCode = selectedStatement?.GenerateCode(0)?.Trim();

            // 创建新文档
            var document = new FlowDocument { PageWidth = 2000 };
            var paragraph = new Paragraph 
            { 
                Margin = new Thickness(0),
                FontFamily = new FontFamily("Consolas, Microsoft YaHei UI"),
                FontSize = 14
            };

            if (!string.IsNullOrEmpty(code))
            {
                // 分行处理代码
                var lines = code.Split('\n');

                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i].TrimEnd('\r');
                    var run = new Run(line) { Foreground = TextBrush };

                    // 检查是否需要高亮
                    if (selectedCode != null && !string.IsNullOrEmpty(line.Trim()))
                    {
                        var trimmedLine = line.Trim();
                        if (trimmedLine.Equals(selectedCode, StringComparison.OrdinalIgnoreCase) ||
                            selectedCode.Contains(trimmedLine) ||
                            trimmedLine.Contains(selectedCode))
                        {
                            run.Background = HighlightBrush;
                            run.Foreground = Brushes.Black;
                        }
                    }

                    paragraph.Inlines.Add(run);
                    
                    // 添加换行符（除了最后一行）
                    if (i < lines.Length - 1)
                    {
                        paragraph.Inlines.Add(new LineBreak());
                    }
                }
            }

            document.Blocks.Add(paragraph);
            CodePreview.Document = document;
        }
    }
}
