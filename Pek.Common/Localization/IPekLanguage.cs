namespace Pek;

/// <summary>
/// 翻译语言接口
/// </summary>
public interface IPekLanguage
{
    /// <summary>
    /// 翻译内容
    /// </summary>
    /// <param name="key">要翻译的内容</param>
    /// <returns></returns>
    String? Translate(String key);
}
