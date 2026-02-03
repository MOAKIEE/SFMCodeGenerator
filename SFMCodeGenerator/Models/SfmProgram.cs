using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Text;

namespace SFMCodeGenerator.Models
{
    /// <summary>
    /// SFM程序
    /// </summary>
    public partial class SfmProgram : ObservableObject
    {
        /// <summary>
        /// 名称变更事件
        /// </summary>
        public event Action? NameChanged;

        [ObservableProperty]
        private string _name = "我的程序";

        partial void OnNameChanged(string value)
        {
            NameChanged?.Invoke();
        }

        [ObservableProperty]
        private ObservableCollection<SfmTrigger> _triggers = new();

        /// <summary>
        /// 生成完整的SFM代码
        /// </summary>
        public string GenerateCode()
        {
            var sb = new StringBuilder();

            // 程序名称
            if (!string.IsNullOrWhiteSpace(Name))
            {
                sb.AppendLine($"name \"{Name}\"");
                sb.AppendLine();
            }

            // 触发器
            foreach (var trigger in Triggers)
            {
                sb.AppendLine(trigger.GenerateCode());
                sb.AppendLine();
            }

            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// 获取代码字符数
        /// </summary>
        public int GetCharacterCount()
        {
            return GenerateCode().Length;
        }
    }

    /// <summary>
    /// SFM触发器
    /// </summary>
    public partial class SfmTrigger : ObservableObject
    {
        [ObservableProperty]
        private TriggerType _triggerType = TriggerType.Every;

        [ObservableProperty]
        private int _interval = 20;

        [ObservableProperty]
        private TimeUnit _timeUnit = TimeUnit.Ticks;

        [ObservableProperty]
        private bool _isGlobal;

        [ObservableProperty]
        private int? _offset;

        [ObservableProperty]
        private ObservableCollection<SfmStatement> _statements = new();

        /// <summary>
        /// 生成触发器代码
        /// </summary>
        public string GenerateCode()
        {
            var sb = new StringBuilder();

            if (TriggerType == TriggerType.Every)
            {
                var parts = new List<string> { "every" };

                if (Interval != 1)
                {
                    parts.Add(Interval.ToString());
                }

                if (IsGlobal)
                {
                    parts.Add("global");
                }

                if (Offset.HasValue && Offset.Value > 0)
                {
                    parts.Add($"+ {Offset.Value}");
                }

                parts.Add(TimeUnit == TimeUnit.Ticks ? "ticks" : "seconds");
                parts.Add("do");

                sb.AppendLine(string.Join(" ", parts));
            }
            else // RedstoneP ulse
            {
                sb.AppendLine("every redstone pulse do");
            }

            // 语句
            foreach (var statement in Statements)
            {
                sb.AppendLine(statement.GenerateCode(1));
            }

            sb.Append("end");
            return sb.ToString();
        }
    }
}
