using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Text;

namespace SFMCodeGenerator.ViewModels
{
    public partial class WizardViewModel : ObservableObject
    {
        // 执行频率
        [ObservableProperty]
        private int _tickInterval = 20;

        // 来源列表
        public ObservableCollection<WizardSource> Sources { get; } = new();

        // 目标列表
        public ObservableCollection<WizardTarget> Targets { get; } = new();

        // 生成的代码
        [ObservableProperty]
        private string _generatedCode = "";

        // 方向选项
        public string[] SideOptions { get; } = { "任意", "上", "下", "北", "南", "东", "西" };

        // 类型选项
        public string[] TypeOptions { get; } = { "物品", "流体", "能量", "气体" };

        public WizardViewModel()
        {
            // 添加默认来源和目标
            Sources.Add(new WizardSource());
            Targets.Add(new WizardTarget());

            // 监听集合变化
            Sources.CollectionChanged += (_, _) => UpdateCode();
            Targets.CollectionChanged += (_, _) => UpdateCode();

            UpdateCode();
        }

        [RelayCommand]
        private void AddSource()
        {
            var source = new WizardSource();
            source.PropertyChanged += (_, _) => UpdateCode();
            Sources.Add(source);
            UpdateCode();
        }

        [RelayCommand]
        private void RemoveSource(WizardSource source)
        {
            if (Sources.Count > 1)
            {
                Sources.Remove(source);
                UpdateCode();
            }
        }

        [RelayCommand]
        private void AddTarget()
        {
            var target = new WizardTarget();
            target.PropertyChanged += (_, _) => UpdateCode();
            Targets.Add(target);
            UpdateCode();
        }

        [RelayCommand]
        private void RemoveTarget(WizardTarget target)
        {
            if (Targets.Count > 1)
            {
                Targets.Remove(target);
                UpdateCode();
            }
        }

        [RelayCommand]
        private void CopyCode()
        {
            if (!string.IsNullOrEmpty(GeneratedCode))
            {
                System.Windows.Clipboard.SetText(GeneratedCode);
            }
        }

        partial void OnTickIntervalChanged(int value) => UpdateCode();

        private void UpdateCode()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"EVERY {TickInterval} TICKS DO");

            foreach (var source in Sources)
            {
                sb.AppendLine($"  {source.GenerateCode()}");
            }

            foreach (var target in Targets)
            {
                sb.AppendLine($"  {target.GenerateCode()}");
            }

            sb.AppendLine("END");

            GeneratedCode = sb.ToString();
        }

        public void SubscribeToChanges()
        {
            foreach (var source in Sources)
            {
                source.PropertyChanged += (_, _) => UpdateCode();
            }
            foreach (var target in Targets)
            {
                target.PropertyChanged += (_, _) => UpdateCode();
            }
        }
    }

    public partial class WizardSource : ObservableObject
    {
        [ObservableProperty]
        private string _label = "a";

        [ObservableProperty]
        private string _side = "任意";

        [ObservableProperty]
        private string _resourceType = "物品";

        [ObservableProperty]
        private string _resourceId = "";

        [ObservableProperty]
        private string _quantity = "";

        public string GenerateCode()
        {
            var sb = new StringBuilder("INPUT");

            // 资源类型前缀
            var typePrefix = ResourceType switch
            {
                "流体" => "fluid::",
                "能量" => "fe::",
                "气体" => "gas::",
                _ => ""
            };

            // 资源限制
            if (!string.IsNullOrWhiteSpace(ResourceId))
            {
                sb.Append($" {typePrefix}{ResourceId}");
            }

            // 数量
            if (!string.IsNullOrWhiteSpace(Quantity))
            {
                sb.Append($" {Quantity}");
            }

            sb.Append($" FROM {Label}");

            // 方向
            if (Side != "任意")
            {
                var sideCode = Side switch
                {
                    "上" => "TOP",
                    "下" => "BOTTOM",
                    "北" => "NORTH",
                    "南" => "SOUTH",
                    "东" => "EAST",
                    "西" => "WEST",
                    _ => ""
                };
                if (!string.IsNullOrEmpty(sideCode))
                {
                    sb.Append($" SIDE {sideCode}");
                }
            }

            return sb.ToString();
        }
    }

    public partial class WizardTarget : ObservableObject
    {
        [ObservableProperty]
        private string _label = "b";

        [ObservableProperty]
        private string _side = "任意";

        [ObservableProperty]
        private string _resourceType = "物品";

        [ObservableProperty]
        private string _resourceId = "";

        [ObservableProperty]
        private string _quantity = "";

        public string GenerateCode()
        {
            var sb = new StringBuilder("OUTPUT");

            // 资源类型前缀
            var typePrefix = ResourceType switch
            {
                "流体" => "fluid::",
                "能量" => "fe::",
                "气体" => "gas::",
                _ => ""
            };

            // 资源限制
            if (!string.IsNullOrWhiteSpace(ResourceId))
            {
                sb.Append($" {typePrefix}{ResourceId}");
            }

            // 数量
            if (!string.IsNullOrWhiteSpace(Quantity))
            {
                sb.Append($" {Quantity}");
            }

            sb.Append($" TO {Label}");

            // 方向
            if (Side != "任意")
            {
                var sideCode = Side switch
                {
                    "上" => "TOP",
                    "下" => "BOTTOM",
                    "北" => "NORTH",
                    "南" => "SOUTH",
                    "东" => "EAST",
                    "西" => "WEST",
                    _ => ""
                };
                if (!string.IsNullOrEmpty(sideCode))
                {
                    sb.Append($" SIDE {sideCode}");
                }
            }

            return sb.ToString();
        }
    }
}
