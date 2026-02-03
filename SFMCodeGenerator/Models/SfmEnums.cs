namespace SFMCodeGenerator.Models
{
    /// <summary>
    /// 触发器类型
    /// </summary>
    public enum TriggerType
    {
        /// <summary>时间触发器</summary>
        Every,
        /// <summary>红石脉冲触发器</summary>
        RedstonePulse
    }

    /// <summary>
    /// 时间单位
    /// </summary>
    public enum TimeUnit
    {
        Ticks,
        Seconds
    }

    /// <summary>
    /// 资源类型
    /// </summary>
    public enum ResourceType
    {
        /// <summary>物品</summary>
        Item,
        /// <summary>流体</summary>
        Fluid,
        /// <summary>能量</summary>
        Energy,
        /// <summary>气体（通用机械）</summary>
        Gas
    }

    /// <summary>
    /// 方向/面
    /// </summary>
    public enum Side
    {
        Top,
        Bottom,
        North,
        South,
        East,
        West,
        Left,
        Right,
        Front,
        Back,
        Null
    }

    /// <summary>
    /// 比较运算符
    /// </summary>
    public enum ComparisonOperator
    {
        /// <summary>大于 GT ></summary>
        GreaterThan,
        /// <summary>小于 LT <</summary>
        LessThan,
        /// <summary>等于 EQ =</summary>
        Equal,
        /// <summary>大于等于 GE >=</summary>
        GreaterOrEqual,
        /// <summary>小于等于 LE <=</summary>
        LessOrEqual
    }

    /// <summary>
    /// 集合操作符
    /// </summary>
    public enum SetOperator
    {
        /// <summary>总计（默认）</summary>
        Overall,
        /// <summary>部分</summary>
        Some,
        /// <summary>每个</summary>
        Every,
        /// <summary>各自</summary>
        Each,
        /// <summary>恰好一个</summary>
        One,
        /// <summary>至多一个</summary>
        Lone
    }

    /// <summary>
    /// 语句类型
    /// </summary>
    public enum StatementType
    {
        Input,
        Output,
        If,
        Forget
    }
}
