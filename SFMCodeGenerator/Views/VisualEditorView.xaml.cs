using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SFMCodeGenerator.ViewModels;

namespace SFMCodeGenerator.Views
{
    public partial class VisualEditorView : UserControl
    {
        private bool _isDragging;
        private Point _dragStart;
        private VisualNode? _draggedNode;
        
        // 连接模式
        private bool _isConnecting;
        private VisualNode? _connectionSource;
        private Button? _connectionButton;

        public VisualEditorView()
        {
            InitializeComponent();
            Loaded += VisualEditorView_Loaded;
        }

        private void VisualEditorView_Loaded(object sender, RoutedEventArgs e)
        {
            // 尝试从主窗口获取常用列表
            if (Window.GetWindow(this)?.DataContext is MainViewModel mainVm &&
                DataContext is VisualEditorViewModel vm)
            {
                vm.CommonLabels = mainVm.CommonLabels;
                vm.CommonItemIds = mainVm.CommonItemIds;
                vm.CommonFluidIds = mainVm.CommonFluidIds;
                vm.CommonEnergyIds = mainVm.CommonEnergyIds;
                vm.CommonGasIds = mainVm.CommonGasIds;
            }
        }

        private void Node_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 如果正在连接模式，处理连接
            if (_isConnecting && _connectionSource != null)
            {
                if (sender is FrameworkElement element && element.DataContext is VisualNode targetNode)
                {
                    CompleteConnection(targetNode);
                    e.Handled = true;
                    return;
                }
            }
            
            // 否则处理拖拽
            if (sender is FrameworkElement elem && elem.DataContext is VisualNode node)
            {
                _isDragging = true;
                _draggedNode = node;
                _dragStart = e.GetPosition(EditorCanvas);
                elem.CaptureMouse();
                e.Handled = true;
            }
        }

        private void Node_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && _draggedNode != null && sender is FrameworkElement)
            {
                var currentPos = e.GetPosition(EditorCanvas);
                var deltaX = currentPos.X - _dragStart.X;
                var deltaY = currentPos.Y - _dragStart.Y;

                _draggedNode.X += deltaX;
                _draggedNode.Y += deltaY;

                // 限制在画布范围内
                _draggedNode.X = Math.Max(0, Math.Min(_draggedNode.X, EditorCanvas.ActualWidth - 200));
                _draggedNode.Y = Math.Max(0, Math.Min(_draggedNode.Y, EditorCanvas.ActualHeight - 200));

                _dragStart = currentPos;
            }
        }

        private void Node_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                _isDragging = false;
                _draggedNode = null;
                element.ReleaseMouseCapture();
            }
        }

        private void StartConnection_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is VisualNode node)
            {
                if (!_isConnecting)
                {
                    // 开始连接
                    _isConnecting = true;
                    _connectionSource = node;
                    _connectionButton = btn;
                    
                    // 显示临时连接线
                    TempConnectionLine.Visibility = Visibility.Visible;
                    TempConnectionLine.X1 = node.X + 90;
                    TempConnectionLine.Y1 = node.Y + 70;
                    TempConnectionLine.X2 = node.X + 90;
                    TempConnectionLine.Y2 = node.Y + 70;
                    
                    btn.Content = "选择目标...";
                }
                else
                {
                    // 完成连接
                    CompleteConnection(node);
                }
            }
        }

        private void CompleteConnection(VisualNode targetNode)
        {
            if (_connectionSource != null && DataContext is VisualEditorViewModel vm)
            {
                if (targetNode != _connectionSource)
                {
                    vm.Connect(_connectionSource, targetNode);
                }
            }
            CancelConnection();
        }

        private void CancelConnection()
        {
            _isConnecting = false;
            _connectionSource = null;
            TempConnectionLine.Visibility = Visibility.Collapsed;
            
            if (_connectionButton != null)
            {
                _connectionButton.Content = "连接";
                _connectionButton = null;
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isConnecting && _connectionSource != null)
            {
                var pos = e.GetPosition(EditorCanvas);
                TempConnectionLine.X2 = pos.X;
                TempConnectionLine.Y2 = pos.Y;
            }
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // 点击空白处取消连接
            if (_isConnecting)
            {
                CancelConnection();
            }
        }

        private void ReverseConnection_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is NodeConnection connection && 
                DataContext is VisualEditorViewModel vm)
            {
                vm.ReverseConnection(connection);
            }
        }

        private void AddTrigger_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is VisualEditorViewModel vm)
            {
                vm.AddTriggerCommand.Execute(null);
            }
        }

        private void RemoveTrigger_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is VisualEditorViewModel vm && vm.SelectedTrigger != null)
            {
                vm.RemoveTriggerCommand.Execute(vm.SelectedTrigger);
            }
        }
    }
}
