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

        // 常用物品列表
        public ObservableCollection<string> CommonItems { get; } = new()
        {
            "", "iron_ingot", "gold_ingot", "diamond", "emerald", "copper_ingot",
            "iron_ore", "gold_ore", "diamond_ore", "coal", "redstone",
            "stone", "cobblestone", "dirt", "sand", "gravel",
            "oak_log", "birch_log", "spruce_log", "jungle_log",
            "wheat", "carrot", "potato", "beetroot",
            "*"
        };

        public MainViewModel()
        {
            // 监听程序名称变化
            Program.NameChanged += UpdateGeneratedCode;
            
            // 初始化默认触发器
            AddNewTrigger();
            UpdateGeneratedCode();
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

            var statement = new InputStatement
            {
                Label = InputLabel,
                Quantity = string.IsNullOrEmpty(InputQuantity) ? null : int.Parse(InputQuantity),
                Retention = string.IsNullOrEmpty(InputRetention) ? null : int.Parse(InputRetention),
                RetentionEach = InputRetentionEach,
                ResourceId = string.IsNullOrEmpty(InputResourceId) ? "*" : InputResourceId,
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

            var statement = new OutputStatement
            {
                Label = OutputLabel,
                Quantity = string.IsNullOrEmpty(OutputQuantity) ? null : int.Parse(OutputQuantity),
                Retention = string.IsNullOrEmpty(OutputRetention) ? null : int.Parse(OutputRetention),
                RetentionEach = OutputRetentionEach,
                ResourceId = string.IsNullOrEmpty(OutputResourceId) ? "*" : OutputResourceId,
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
                    t5.Statements.Add(new InputStatement { Label = "tank_a", ResourceId = "fluid:minecraft:water" });
                    t5.Statements.Add(new OutputStatement { Label = "tank_b" });
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
    }
}
