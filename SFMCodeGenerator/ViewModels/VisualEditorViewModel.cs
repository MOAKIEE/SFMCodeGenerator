using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;

namespace SFMCodeGenerator.ViewModels
{
    public partial class VisualEditorViewModel : ObservableObject
    {
        // 触发器列表
        public ObservableCollection<VisualTrigger> Triggers { get; } = new();

        // 当前选中的触发器
        [ObservableProperty]
        private VisualTrigger? _selectedTrigger;

        // 生成的代码
        [ObservableProperty]
        private string _generatedCode = "";

        // 引用主ViewModel的常用列表（在View中设置）
        [ObservableProperty]
        private ObservableCollection<string> _commonLabels = new();

        [ObservableProperty]
        private ObservableCollection<string> _commonItemIds = new();

        [ObservableProperty]
        private ObservableCollection<string> _commonFluidIds = new();

        [ObservableProperty]
        private ObservableCollection<string> _commonEnergyIds = new();

        [ObservableProperty]
        private ObservableCollection<string> _commonGasIds = new();

        // 触发器类型选项（和高级模式一致：只有一种红石触发）
        public static string[] TriggerTypes { get; } = { "定时", "红石脉冲" };

        public VisualEditorViewModel()
        {
            // 添加默认触发器
            AddTrigger();
        }

        [RelayCommand]
        private void AddTrigger()
        {
            var trigger = new VisualTrigger
            {
                Name = $"触发器 {Triggers.Count + 1}",
                TickInterval = 20,
                TriggerType = "定时"
            };
            trigger.PropertyChanged += (_, _) => UpdateCode();
            
            // 添加两个默认节点
            var nodeA = new VisualNode { Label = "a", X = 50, Y = 80 };
            var nodeB = new VisualNode { Label = "b", X = 320, Y = 80 };
            nodeA.PropertyChanged += (_, _) => UpdateCode();
            nodeB.PropertyChanged += (_, _) => UpdateCode();
            
            trigger.Nodes.Add(nodeA);
            trigger.Nodes.Add(nodeB);
            trigger.Connections.Add(new NodeConnection { Source = nodeA, Target = nodeB });
            
            Triggers.Add(trigger);
            SelectedTrigger = trigger;
            UpdateCode();
        }

        [RelayCommand]
        private void RemoveTrigger(VisualTrigger trigger)
        {
            Triggers.Remove(trigger);
            if (SelectedTrigger == trigger)
            {
                SelectedTrigger = Triggers.FirstOrDefault();
            }
            UpdateCode();
        }

        [RelayCommand]
        private void AddNode()
        {
            if (SelectedTrigger == null) return;
            
            var node = new VisualNode
            {
                Label = GetNextLabel(),
                X = 50 + (SelectedTrigger.Nodes.Count % 4) * 200,
                Y = 80 + (SelectedTrigger.Nodes.Count / 4) * 180
            };
            node.PropertyChanged += (_, _) => UpdateCode();
            SelectedTrigger.Nodes.Add(node);
            UpdateCode();
        }

        [RelayCommand]
        private void RemoveNode(VisualNode node)
        {
            if (SelectedTrigger == null) return;
            
            var connectionsToRemove = SelectedTrigger.Connections
                .Where(c => c.Source == node || c.Target == node).ToList();
            foreach (var conn in connectionsToRemove)
            {
                SelectedTrigger.Connections.Remove(conn);
            }
            SelectedTrigger.Nodes.Remove(node);
            UpdateCode();
        }

        public void Connect(VisualNode source, VisualNode target)
        {
            if (SelectedTrigger == null || source == target) return;
            
            if (!SelectedTrigger.Connections.Any(c => c.Source == source && c.Target == target))
            {
                var conn = new NodeConnection { Source = source, Target = target };
                conn.PropertyChanged += (_, _) => UpdateCode();
                SelectedTrigger.Connections.Add(conn);
                UpdateCode();
            }
        }

        public void ReverseConnection(NodeConnection connection)
        {
            (connection.Source, connection.Target) = (connection.Target, connection.Source);
            OnPropertyChanged(nameof(SelectedTrigger));
            UpdateCode();
        }

        [RelayCommand]
        private void RemoveConnection(NodeConnection connection)
        {
            if (SelectedTrigger == null) return;
            SelectedTrigger.Connections.Remove(connection);
            UpdateCode();
        }

        [RelayCommand]
        private void CopyCode()
        {
            if (!string.IsNullOrEmpty(GeneratedCode))
            {
                Clipboard.SetText(GeneratedCode);
            }
        }

        private void UpdateCode()
        {
            if (Triggers.Count == 0)
            {
                GeneratedCode = "// 请添加触发器";
                return;
            }

            var sb = new StringBuilder();
            
            foreach (var trigger in Triggers)
            {
                if (trigger.Connections.Count == 0) continue;
                
                // 根据触发器类型生成不同的代码
                var triggerCode = trigger.TriggerType == "红石脉冲"
                    ? "every redstone pulse do"
                    : $"every {trigger.TickInterval} ticks do";
                
                sb.AppendLine(triggerCode);

                var sourceGroups = trigger.Connections.GroupBy(c => c.Source);
                foreach (var group in sourceGroups)
                {
                    var source = group.Key;
                    sb.AppendLine($"  {GenerateInputCode(source)}");
                    
                    foreach (var conn in group)
                    {
                        sb.AppendLine($"  {GenerateOutputCode(conn.Target)}");
                    }
                }

                sb.AppendLine("end");
                sb.AppendLine();
            }
            
            GeneratedCode = sb.ToString().TrimEnd();
        }

        private string GenerateInputCode(VisualNode node)
        {
            var sb = new StringBuilder("input");

            var typePrefix = node.ResourceType switch
            {
                "流体" => "fluid::",
                "能量" => "fe::",
                "气体" => "gas::",
                _ => ""
            };

            if (!string.IsNullOrWhiteSpace(node.Quantity))
                sb.Append($" {node.Quantity}");

            if (!string.IsNullOrEmpty(typePrefix))
            {
                sb.Append($" {(string.IsNullOrWhiteSpace(node.ResourceId) ? typePrefix : $"{typePrefix}{node.ResourceId}")}");
            }
            else if (!string.IsNullOrWhiteSpace(node.ResourceId))
            {
                sb.Append($" {node.ResourceId}");
            }

            sb.Append($" from {node.Label}");
            AppendSide(sb, node.Side);

            return sb.ToString();
        }

        private string GenerateOutputCode(VisualNode node)
        {
            var sb = new StringBuilder("output");

            var typePrefix = node.ResourceType switch
            {
                "流体" => "fluid::",
                "能量" => "fe::",
                "气体" => "gas::",
                _ => ""
            };

            if (!string.IsNullOrWhiteSpace(node.Quantity))
                sb.Append($" {node.Quantity}");

            if (!string.IsNullOrEmpty(typePrefix))
            {
                sb.Append($" {(string.IsNullOrWhiteSpace(node.ResourceId) ? typePrefix : $"{typePrefix}{node.ResourceId}")}");
            }
            else if (!string.IsNullOrWhiteSpace(node.ResourceId))
            {
                sb.Append($" {node.ResourceId}");
            }

            sb.Append($" to {node.Label}");
            AppendSide(sb, node.Side);

            return sb.ToString();
        }

        private void AppendSide(StringBuilder sb, string side)
        {
            if (side != "任意")
            {
                var sideCode = side switch
                {
                    "上" => "top", "下" => "bottom",
                    "北" => "north", "南" => "south",
                    "东" => "east", "西" => "west",
                    _ => ""
                };
                if (!string.IsNullOrEmpty(sideCode))
                    sb.Append($" {sideCode} side");
            }
        }

        private string GetNextLabel()
        {
            if (SelectedTrigger == null) return "x";
            
            var existingLabels = SelectedTrigger.Nodes.Select(n => n.Label).ToList();
            foreach (var c in "abcdefghijklmnopqrstuvwxyz")
            {
                var label = c.ToString();
                if (!existingLabels.Contains(label))
                    return label;
            }
            return "x";
        }
    }

    // 触发器
    public partial class VisualTrigger : ObservableObject
    {
        [ObservableProperty]
        private string _name = "";

        [ObservableProperty]
        private string _triggerType = "定时";

        [ObservableProperty]
        private int _tickInterval = 20;

        public ObservableCollection<VisualNode> Nodes { get; } = new();
        public ObservableCollection<NodeConnection> Connections { get; } = new();
    }

    // 节点
    public partial class VisualNode : ObservableObject
    {
        [ObservableProperty]
        private string _label = "";

        [ObservableProperty]
        private string _side = "任意";

        [ObservableProperty]
        private string _resourceType = "物品";

        [ObservableProperty]
        private string _resourceId = "";

        [ObservableProperty]
        private string _quantity = "";

        [ObservableProperty]
        private double _x;

        [ObservableProperty]
        private double _y;

        public string DisplayName => $"[{Label}]";

        public static string[] SideOptions { get; } = { "任意", "上", "下", "北", "南", "东", "西" };
        public static string[] TypeOptions { get; } = { "物品", "流体", "能量", "气体" };
    }

    // 连接
    public partial class NodeConnection : ObservableObject
    {
        [ObservableProperty]
        private VisualNode _source = null!;
        
        [ObservableProperty]
        private VisualNode _target = null!;
    }

    // 中点计算转换器
    public class CenterConverter : System.Windows.Data.IMultiValueConverter
    {
        public double Offset { get; set; }

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is double v1 && values[1] is double v2)
            {
                return (v1 + v2) / 2 + Offset;
            }
            return 0.0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // 偏移转换器
    public class OffsetConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is double v && parameter != null && double.TryParse(parameter.ToString(), out var offset))
            {
                return v + offset;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // 计算连线起止点（对齐节点边框）
    public class LineEndpointConverter : System.Windows.Data.IMultiValueConverter
    {
        private const double HalfWidth = 90;
        private const double HalfHeight = 70;

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length < 4) return 0.0;
            if (values[0] is not double sx || values[1] is not double sy ||
                values[2] is not double tx || values[3] is not double ty)
            {
                return 0.0;
            }

            var sourceCenterX = sx + HalfWidth;
            var sourceCenterY = sy + HalfHeight;
            var targetCenterX = tx + HalfWidth;
            var targetCenterY = ty + HalfHeight;

            var dx = targetCenterX - sourceCenterX;
            var dy = targetCenterY - sourceCenterY;
            var len = Math.Sqrt(dx * dx + dy * dy);

            if (len < 0.001)
            {
                return parameter?.ToString() switch
                {
                    "startX" => sourceCenterX,
                    "startY" => sourceCenterY,
                    "endX" => targetCenterX,
                    "endY" => targetCenterY,
                    _ => 0.0
                };
            }

            var ux = dx / len;
            var uy = dy / len;

            var sourceScale = GetEdgeScale(ux, uy);
            var targetScale = GetEdgeScale(ux, uy);

            var startX = sourceCenterX + ux * sourceScale;
            var startY = sourceCenterY + uy * sourceScale;
            var endX = targetCenterX - ux * targetScale;
            var endY = targetCenterY - uy * targetScale;

            return parameter?.ToString() switch
            {
                "startX" => startX,
                "startY" => startY,
                "endX" => endX,
                "endY" => endY,
                _ => 0.0
            };
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static double GetEdgeScale(double ux, double uy)
        {
            var ax = Math.Abs(ux);
            var ay = Math.Abs(uy);
            var scaleX = ax < 0.0001 ? double.PositiveInfinity : HalfWidth / ax;
            var scaleY = ay < 0.0001 ? double.PositiveInfinity : HalfHeight / ay;
            return Math.Min(scaleX, scaleY);
        }
    }

    // 计算箭头角度
    public class LineAngleConverter : System.Windows.Data.IMultiValueConverter
    {
        private const double HalfWidth = 90;
        private const double HalfHeight = 70;

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length < 4) return 0.0;
            if (values[0] is not double sx || values[1] is not double sy ||
                values[2] is not double tx || values[3] is not double ty)
            {
                return 0.0;
            }

            var sourceCenterX = sx + HalfWidth;
            var sourceCenterY = sy + HalfHeight;
            var targetCenterX = tx + HalfWidth;
            var targetCenterY = ty + HalfHeight;

            var dx = targetCenterX - sourceCenterX;
            var dy = targetCenterY - sourceCenterY;
            var angle = Math.Atan2(dy, dx) * 180.0 / Math.PI;
            return angle;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // 根据资源类型选择常用ID列表
    public class ResourceIdListConverter : System.Windows.Data.IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length < 5) return Array.Empty<string>();

            var resourceType = values[0] as string ?? "物品";
            var items = values[1] as ObservableCollection<string> ?? new ObservableCollection<string>();
            var fluids = values[2] as ObservableCollection<string> ?? new ObservableCollection<string>();
            var energy = values[3] as ObservableCollection<string> ?? new ObservableCollection<string>();
            var gas = values[4] as ObservableCollection<string> ?? new ObservableCollection<string>();

            return resourceType switch
            {
                "流体" => fluids,
                "能量" => energy,
                "气体" => gas,
                _ => items
            };
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
