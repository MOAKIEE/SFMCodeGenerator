using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace SFMCodeGenerator.Models
{
    /// <summary>
    /// SFM语句基类
    /// </summary>
    public abstract partial class SfmStatement : ObservableObject
    {
        public abstract StatementType Type { get; }
        public abstract string GenerateCode(int indentLevel = 0);
        
        protected string GetIndent(int level) => new string(' ', level * 4);
    }

    /// <summary>
    /// INPUT语句
    /// </summary>
    public partial class InputStatement : SfmStatement
    {
        public override StatementType Type => StatementType.Input;

        [ObservableProperty]
        private int? _quantity;

        [ObservableProperty]
        private int? _retention;

        [ObservableProperty]
        private bool _retentionEach;

        [ObservableProperty]
        private string _resourceId = "*";

        [ObservableProperty]
        private string _label = "a";

        [ObservableProperty]
        private bool _useEach;

        [ObservableProperty]
        private Side? _side;

        [ObservableProperty]
        private string? _slots;

        [ObservableProperty]
        private string? _exceptResources;

        public override string GenerateCode(int indentLevel = 0)
        {
            var indent = GetIndent(indentLevel);
            var parts = new List<string> { "input" };

            // 数量
            if (Quantity.HasValue)
            {
                parts.Add(Quantity.Value.ToString());
            }

            // 保留
            if (Retention.HasValue)
            {
                parts.Add($"retain {Retention.Value}" + (RetentionEach ? " each" : ""));
            }

            // 资源ID
            if (!string.IsNullOrEmpty(ResourceId) && ResourceId != "*")
            {
                parts.Add(ResourceId);
            }

            // 排除
            if (!string.IsNullOrEmpty(ExceptResources))
            {
                parts.Add($"except {ExceptResources}");
            }

            // from
            parts.Add("from");
            if (UseEach) parts.Add("each");
            parts.Add(Label);

            // 方向
            if (Side.HasValue && Side.Value != Models.Side.Null)
            {
                parts.Add(Side.Value.ToString().ToLower());
                parts.Add("side");
            }

            // 槽位
            if (!string.IsNullOrEmpty(Slots))
            {
                parts.Add($"slots {Slots}");
            }

            return indent + string.Join(" ", parts);
        }
    }

    /// <summary>
    /// OUTPUT语句
    /// </summary>
    public partial class OutputStatement : SfmStatement
    {
        public override StatementType Type => StatementType.Output;

        [ObservableProperty]
        private int? _quantity;

        [ObservableProperty]
        private int? _retention;

        [ObservableProperty]
        private bool _retentionEach;

        [ObservableProperty]
        private string _resourceId = "*";

        [ObservableProperty]
        private string _label = "b";

        [ObservableProperty]
        private bool _useEach;

        [ObservableProperty]
        private Side? _side;

        [ObservableProperty]
        private string? _slots;

        [ObservableProperty]
        private string? _exceptResources;

        public override string GenerateCode(int indentLevel = 0)
        {
            var indent = GetIndent(indentLevel);
            var parts = new List<string> { "output" };

            // 数量
            if (Quantity.HasValue)
            {
                parts.Add(Quantity.Value.ToString());
            }

            // 保留
            if (Retention.HasValue)
            {
                parts.Add($"retain {Retention.Value}" + (RetentionEach ? " each" : ""));
            }

            // 资源ID
            if (!string.IsNullOrEmpty(ResourceId) && ResourceId != "*")
            {
                parts.Add(ResourceId);
            }

            // 排除
            if (!string.IsNullOrEmpty(ExceptResources))
            {
                parts.Add($"except {ExceptResources}");
            }

            // to
            parts.Add("to");
            if (UseEach) parts.Add("each");
            parts.Add(Label);

            // 方向
            if (Side.HasValue && Side.Value != Models.Side.Null)
            {
                parts.Add(Side.Value.ToString().ToLower());
                parts.Add("side");
            }

            // 槽位
            if (!string.IsNullOrEmpty(Slots))
            {
                parts.Add($"slots {Slots}");
            }

            return indent + string.Join(" ", parts);
        }
    }

    /// <summary>
    /// IF条件语句
    /// </summary>
    public partial class IfStatement : SfmStatement
    {
        public override StatementType Type => StatementType.If;

        [ObservableProperty]
        private string _condition = "a has > 0 iron_ingot";

        [ObservableProperty]
        private ObservableCollection<SfmStatement> _thenStatements = new();

        [ObservableProperty]
        private ObservableCollection<SfmStatement>? _elseStatements;

        public override string GenerateCode(int indentLevel = 0)
        {
            var indent = GetIndent(indentLevel);
            var lines = new List<string>
            {
                $"{indent}if {Condition} then"
            };

            foreach (var stmt in ThenStatements)
            {
                lines.Add(stmt.GenerateCode(indentLevel + 1));
            }

            if (ElseStatements != null && ElseStatements.Count > 0)
            {
                lines.Add($"{indent}else");
                foreach (var stmt in ElseStatements)
                {
                    lines.Add(stmt.GenerateCode(indentLevel + 1));
                }
            }

            lines.Add($"{indent}end");
            return string.Join("\n", lines);
        }
    }

    /// <summary>
    /// FORGET语句
    /// </summary>
    public partial class ForgetStatement : SfmStatement
    {
        public override StatementType Type => StatementType.Forget;

        [ObservableProperty]
        private string? _labels;

        public override string GenerateCode(int indentLevel = 0)
        {
            var indent = GetIndent(indentLevel);
            if (string.IsNullOrEmpty(Labels))
            {
                return $"{indent}forget";
            }
            return $"{indent}forget {Labels}";
        }
    }
}
