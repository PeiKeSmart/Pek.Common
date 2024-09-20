namespace Pek;

/// <summary>
/// 系统扩展 - 公共扩展
/// </summary>
public static partial class DHExtensions
{
    #region Value(获取枚举值)

    /// <summary>
    /// 获取枚举值
    /// </summary>
    /// <param name="instance">枚举实例</param>
    public static int Value(this Enum instance) => Helpers.Enum.GetValue(instance.GetType(), instance);

    /// <summary>
    /// 获取枚举值
    /// </summary>
    /// <typeparam name="TResult">返回值类型</typeparam>
    /// <param name="instance">枚举实例</param>
    public static TResult Value<TResult>(this Enum instance) => Helpers.Conv.To<TResult>(instance.Value());

    #endregion
}
