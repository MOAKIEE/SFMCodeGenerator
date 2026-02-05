using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SFMCodeGenerator.Models;
using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.Win32;
using System.IO;

namespace SFMCodeGenerator.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private const int MaxCharacters = 32300;

        [ObservableProperty]
        private SfmProgram _program = new();

        [ObservableProperty]
        private SfmTrigger? _selectedTrigger;

        [ObservableProperty]
        private SfmStatement? _selectedStatement;

        [ObservableProperty]
        private string _generatedCode = "";

        [ObservableProperty]
        private int _characterCount;

        [ObservableProperty]
        private string _characterCountText = "0 / 32300";

        [ObservableProperty]
        private bool _isCharacterCountWarning;

        // 触发器设置
        [ObservableProperty]
        private TriggerType _triggerType = TriggerType.Every;

        [ObservableProperty]
        private int _triggerInterval = 20;

        [ObservableProperty]
        private TimeUnit _triggerTimeUnit = TimeUnit.Ticks;

        [ObservableProperty]
        private bool _triggerIsGlobal;

        [ObservableProperty]
        private int _triggerOffset;

        // INPUT设置
        [ObservableProperty]
        private string _inputLabel = "a";

        [ObservableProperty]
        private string _inputQuantity = "";

        [ObservableProperty]
        private string _inputRetention = "";

        [ObservableProperty]
        private bool _inputRetentionEach;

        [ObservableProperty]
        private string _inputResourceType = "物品";

        [ObservableProperty]
        private string _inputResourceId = "";

        [ObservableProperty]
        private bool _inputUseEach;

        [ObservableProperty]
        private Side _inputSide = Side.Null;

        [ObservableProperty]
        private string _inputSlots = "";

        // OUTPUT设置
        [ObservableProperty]
        private string _outputLabel = "b";

        [ObservableProperty]
        private string _outputQuantity = "";

        [ObservableProperty]
        private string _outputRetention = "";

        [ObservableProperty]
        private bool _outputRetentionEach;

        [ObservableProperty]
        private string _outputResourceType = "物品";

        [ObservableProperty]
        private string _outputResourceId = "";

        [ObservableProperty]
        private bool _outputUseEach;

        [ObservableProperty]
        private Side _outputSide = Side.Null;

        [ObservableProperty]
        private string _outputSlots = "";

        // IF条件设置
        [ObservableProperty]
        private string _ifConditionLabel = "a";

        [ObservableProperty]
        private SetOperator _ifSetOperator = SetOperator.Overall;

        [ObservableProperty]
        private ComparisonOperator _ifComparisonOperator = ComparisonOperator.GreaterThan;

        [ObservableProperty]
        private string _ifQuantity = "0";

        [ObservableProperty]
        private string _ifResourceId = "iron_ingot";

        // 下拉选项
        public ObservableCollection<TriggerType> TriggerTypes { get; } = new(Enum.GetValues<TriggerType>());
        public ObservableCollection<TimeUnit> TimeUnits { get; } = new(Enum.GetValues<TimeUnit>());
        public ObservableCollection<Side> Sides { get; } = new(Enum.GetValues<Side>());
        public ObservableCollection<SetOperator> SetOperators { get; } = new(Enum.GetValues<SetOperator>());
        public ObservableCollection<ComparisonOperator> ComparisonOperators { get; } = new(Enum.GetValues<ComparisonOperator>());

        // 资源类型列表
        public ObservableCollection<string> ResourceTypes { get; } = new()
        {
            "物品",      // 默认，不需要前缀
            "流体",      // fluid::
            "能量",      // fe::
            "气体"       // gas::
        };

        // 常用标签列表（用户可自定义）
        public ObservableCollection<string> CommonLabels { get; } = new();

        // 按类型分类的常用资源ID列表
        public ObservableCollection<string> CommonItemIds { get; } = new();
        public ObservableCollection<string> CommonFluidIds { get; } = new();
        public ObservableCollection<string> CommonEnergyIds { get; } = new();
        public ObservableCollection<string> CommonGasIds { get; } = new();

        // 当前显示的资源ID列表（根据选择的类型动态切换）
        [ObservableProperty]
        private ObservableCollection<string> _currentInputResourceIds = new();

        [ObservableProperty]
        private ObservableCollection<string> _currentOutputResourceIds = new();

        // 配置文件路径
        private static readonly string ConfigPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SFMCodeGenerator", "config.json");


        public MainViewModel()
        {
            // 加载保存的常用列表
            LoadCommonLists();
            
            // 初始化当前资源ID列表
            UpdateCurrentInputResourceIds();
            UpdateCurrentOutputResourceIds();
            
            // 监听程序名称变化
            Program.NameChanged += UpdateGeneratedCode;
            
            // 初始化默认触发器
            AddNewTrigger();
            UpdateGeneratedCode();
        }

        // 根据资源类型获取对应的常用ID列表
        private ObservableCollection<string> GetResourceIdsByType(string resourceType)
        {
            return resourceType switch
            {
                "流体" => CommonFluidIds,
                "能量" => CommonEnergyIds,
                "气体" => CommonGasIds,
                _ => CommonItemIds
            };
        }

        private void UpdateCurrentInputResourceIds()
        {
            CurrentInputResourceIds = GetResourceIdsByType(InputResourceType);
        }

        private void UpdateCurrentOutputResourceIds()
        {
            CurrentOutputResourceIds = GetResourceIdsByType(OutputResourceType);
        }

        // 当 InputResourceType 变化时更新列表
        partial void OnInputResourceTypeChanged(string value)
        {
            UpdateCurrentInputResourceIds();
        }

        // 当 OutputResourceType 变化时更新列表
        partial void OnOutputResourceTypeChanged(string value)
        {
            UpdateCurrentOutputResourceIds();
        }


        [RelayCommand]
        private void AddNewTrigger()
        {
            var trigger = new SfmTrigger
            {
                TriggerType = TriggerType,
                Interval = TriggerInterval,
                TimeUnit = TriggerTimeUnit,
                IsGlobal = TriggerIsGlobal,
                Offset = TriggerOffset > 0 ? TriggerOffset : null
            };

            Program.Triggers.Add(trigger);
            SelectedTrigger = trigger;
            UpdateGeneratedCode();
        }

        [RelayCommand]
        private void RemoveTrigger()
        {
            if (SelectedTrigger != null)
            {
                Program.Triggers.Remove(SelectedTrigger);
                SelectedTrigger = Program.Triggers.FirstOrDefault();
                UpdateGeneratedCode();
            }
        }

        [RelayCommand]
        private void AddInputStatement()
        {
            if (SelectedTrigger == null)
            {
                MessageBox.Show("请先选择一个触发器", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // 根据资源类型拼接前缀
            var resourceId = GetResourceIdWithPrefix(InputResourceType, InputResourceId);

            var statement = new InputStatement
            {
                Label = InputLabel,
                Quantity = string.IsNullOrEmpty(InputQuantity) ? null : int.Parse(InputQuantity),
                Retention = string.IsNullOrEmpty(InputRetention) ? null : int.Parse(InputRetention),
                RetentionEach = InputRetentionEach,
                ResourceId = resourceId,
                UseEach = InputUseEach,
                Side = InputSide == Side.Null ? null : InputSide,
                Slots = string.IsNullOrEmpty(InputSlots) ? null : InputSlots
            };

            SelectedTrigger.Statements.Add(statement);
            UpdateGeneratedCode();
        }

        [RelayCommand]
        private void AddOutputStatement()
        {
            if (SelectedTrigger == null)
            {
                MessageBox.Show("请先选择一个触发器", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // 根据资源类型拼接前缀
            var resourceId = GetResourceIdWithPrefix(OutputResourceType, OutputResourceId);

            var statement = new OutputStatement
            {
                Label = OutputLabel,
                Quantity = string.IsNullOrEmpty(OutputQuantity) ? null : int.Parse(OutputQuantity),
                Retention = string.IsNullOrEmpty(OutputRetention) ? null : int.Parse(OutputRetention),
                RetentionEach = OutputRetentionEach,
                ResourceId = resourceId,
                UseEach = OutputUseEach,
                Side = OutputSide == Side.Null ? null : OutputSide,
                Slots = string.IsNullOrEmpty(OutputSlots) ? null : OutputSlots
            };

            SelectedTrigger.Statements.Add(statement);
            UpdateGeneratedCode();
        }

        [RelayCommand]
        private void AddIfStatement()
        {
            if (SelectedTrigger == null)
            {
                MessageBox.Show("请先选择一个触发器", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var opStr = IfSetOperator == SetOperator.Overall ? "" : IfSetOperator.ToString().ToLower() + " ";
            var compStr = IfComparisonOperator switch
            {
                ComparisonOperator.GreaterThan => ">",
                ComparisonOperator.LessThan => "<",
                ComparisonOperator.Equal => "=",
                ComparisonOperator.GreaterOrEqual => ">=",
                ComparisonOperator.LessOrEqual => "<=",
                _ => ">"
            };

            var statement = new IfStatement
            {
                Condition = $"{opStr}{IfConditionLabel} has {compStr} {IfQuantity} {IfResourceId}"
            };

            SelectedTrigger.Statements.Add(statement);
            UpdateGeneratedCode();
        }

        [RelayCommand]
        private void RemoveStatement()
        {
            if (SelectedTrigger != null && SelectedStatement != null)
            {
                SelectedTrigger.Statements.Remove(SelectedStatement);
                SelectedStatement = null;
                UpdateGeneratedCode();
            }
        }

        [RelayCommand]
        private void MoveStatementUp()
        {
            if (SelectedTrigger != null && SelectedStatement != null)
            {
                var index = SelectedTrigger.Statements.IndexOf(SelectedStatement);
                if (index > 0)
                {
                    SelectedTrigger.Statements.Move(index, index - 1);
                    UpdateGeneratedCode();
                }
            }
        }

        [RelayCommand]
        private void MoveStatementDown()
        {
            if (SelectedTrigger != null && SelectedStatement != null)
            {
                var index = SelectedTrigger.Statements.IndexOf(SelectedStatement);
                if (index < SelectedTrigger.Statements.Count - 1)
                {
                    SelectedTrigger.Statements.Move(index, index + 1);
                    UpdateGeneratedCode();
                }
            }
        }

        [RelayCommand]
        private void CopyCode()
        {
            try
            {
                Clipboard.SetText(GeneratedCode);
                MessageBox.Show("代码已复制到剪贴板！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"复制失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void SaveToFile()
        {
            var dialog = new SaveFileDialog
            {
                Filter = "SFM程序文件 (*.sfm)|*.sfm|文本文件 (*.txt)|*.txt|所有文件 (*.*)|*.*",
                DefaultExt = ".sfm",
                FileName = string.IsNullOrEmpty(Program.Name) ? "program" : Program.Name
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    File.WriteAllText(dialog.FileName, GeneratedCode);
                    MessageBox.Show("文件保存成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"保存失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private void LoadTemplate(string templateName)
        {
            Program = new SfmProgram();

            switch (templateName)
            {
                case "simple_move":
                    Program.Name = "简单物品移动";
                    var t1 = new SfmTrigger { Interval = 20 };
                    t1.Statements.Add(new InputStatement { Label = "a" });
                    t1.Statements.Add(new OutputStatement { Label = "b" });
                    Program.Triggers.Add(t1);
                    break;

                case "smelting":
                    Program.Name = "自动熔炼系统";
                    var t2 = new SfmTrigger { Interval = 20 };
                    t2.Statements.Add(new InputStatement { Label = "chest", ResourceId = "iron_ore" });
                    t2.Statements.Add(new OutputStatement { Label = "furnace", Side = Side.Top });
                    Program.Triggers.Add(t2);
                    var t3 = new SfmTrigger { Interval = 20 };
                    t3.Statements.Add(new InputStatement { Label = "furnace", Side = Side.Bottom });
                    t3.Statements.Add(new OutputStatement { Label = "output" });
                    Program.Triggers.Add(t3);
                    break;

                case "sorting":
                    Program.Name = "自动分类系统";
                    var t4 = new SfmTrigger { Interval = 20 };
                    var if1 = new IfStatement { Condition = "input has > 0 iron_ingot" };
                    if1.ThenStatements.Add(new InputStatement { Label = "input", ResourceId = "iron_ingot" });
                    if1.ThenStatements.Add(new OutputStatement { Label = "iron_storage" });
                    t4.Statements.Add(if1);
                    var if2 = new IfStatement { Condition = "input has > 0 gold_ingot" };
                    if2.ThenStatements.Add(new InputStatement { Label = "input", ResourceId = "gold_ingot" });
                    if2.ThenStatements.Add(new OutputStatement { Label = "gold_storage" });
                    t4.Statements.Add(if2);
                    Program.Triggers.Add(t4);
                    break;

                case "fluid":
                    Program.Name = "流体传输";
                    var t5 = new SfmTrigger { Interval = 20 };
                    t5.Statements.Add(new InputStatement { Label = "tank_a", ResourceId = "fluid::", Side = Side.Bottom });
                    t5.Statements.Add(new OutputStatement { Label = "tank_b", ResourceId = "fluid::", Side = Side.Top });
                    Program.Triggers.Add(t5);
                    break;
            }

            SelectedTrigger = Program.Triggers.FirstOrDefault();
            UpdateGeneratedCode();
        }

        [RelayCommand]
        private void ClearAll()
        {
            if (MessageBox.Show("确定要清空所有内容吗？", "确认", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Program = new SfmProgram();
                SelectedTrigger = null;
                SelectedStatement = null;
                UpdateGeneratedCode();
            }
        }

        /// <summary>
        /// 根据资源类型获取带前缀的资源ID
        /// </summary>
        private string GetResourceIdWithPrefix(string resourceType, string resourceId)
        {
            if (string.IsNullOrEmpty(resourceId))
            {
                // 如果没有填写具体ID，根据类型返回通配符
                return resourceType switch
                {
                    "流体" => "fluid::",
                    "能量" => "fe::",
                    "气体" => "gas::",
                    _ => "*"  // 物品默认匹配所有
                };
            }

            // 如果填写了具体ID，根据类型添加前缀
            return resourceType switch
            {
                "流体" => $"fluid::{resourceId}",
                "能量" => $"fe::{resourceId}",
                "气体" => $"gas::{resourceId}",
                _ => resourceId  // 物品直接使用ID
            };
        }

        private void UpdateGeneratedCode()
        {
            GeneratedCode = Program.GenerateCode();
            CharacterCount = GeneratedCode.Length;
            CharacterCountText = $"{CharacterCount} / {MaxCharacters}";
            IsCharacterCountWarning = CharacterCount > MaxCharacters * 0.9;
        }

        partial void OnProgramChanged(SfmProgram? oldValue, SfmProgram newValue)
        {
            // 取消订阅旧程序的事件
            if (oldValue != null)
            {
                oldValue.NameChanged -= UpdateGeneratedCode;
            }
            // 订阅新程序的事件
            newValue.NameChanged += UpdateGeneratedCode;
            UpdateGeneratedCode();
        }

        // 打开常用列表管理窗口
        [RelayCommand]
        private void ManageCommonItems()
        {
            var window = new Views.ManageCommonItemsWindow(
                CommonLabels, 
                CommonItemIds, 
                CommonFluidIds, 
                CommonEnergyIds, 
                CommonGasIds, 
                SaveCommonLists);
            window.Owner = System.Windows.Application.Current.MainWindow;
            window.ShowDialog();
        }


        // 保存常用列表到文件
        private void SaveCommonLists()
        {
            try
            {
                var dir = System.IO.Path.GetDirectoryName(ConfigPath);
                if (!string.IsNullOrEmpty(dir) && !System.IO.Directory.Exists(dir))
                {
                    System.IO.Directory.CreateDirectory(dir);
                }

                var config = new
                {
                    Labels = CommonLabels.ToList(),
                    ItemIds = CommonItemIds.ToList(),
                    FluidIds = CommonFluidIds.ToList(),
                    EnergyIds = CommonEnergyIds.ToList(),
                    GasIds = CommonGasIds.ToList()
                };

                var json = System.Text.Json.JsonSerializer.Serialize(config);
                System.IO.File.WriteAllText(ConfigPath, json);
            }
            catch { /* 忽略保存错误 */ }
        }

        // 加载常用列表
        private void LoadCommonLists()
        {
            try
            {
                if (System.IO.File.Exists(ConfigPath))
                {
                    var json = System.IO.File.ReadAllText(ConfigPath);
                    var doc = System.Text.Json.JsonDocument.Parse(json);
                    
                    LoadListFromJson(doc, "Labels", CommonLabels);
                    LoadListFromJson(doc, "ItemIds", CommonItemIds);
                    LoadListFromJson(doc, "FluidIds", CommonFluidIds);
                    LoadListFromJson(doc, "EnergyIds", CommonEnergyIds);
                    LoadListFromJson(doc, "GasIds", CommonGasIds);
                }
            }
            catch { /* 忽略加载错误 */ }
        }

        private void LoadListFromJson(System.Text.Json.JsonDocument doc, string propertyName, ObservableCollection<string> list)
        {
            if (doc.RootElement.TryGetProperty(propertyName, out var items))
            {
                foreach (var item in items.EnumerateArray())
                {
                    var value = item.GetString();
                    if (value != null && !list.Contains(value))
                    {
                        list.Add(value);
                    }
                }
            }
        }
    }
}
