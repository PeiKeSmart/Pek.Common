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

    /// <summary>
    /// 翻译内容
    /// </summary>
    /// <param name="key">要翻译的内容</param>
    /// <param name="Lng">语言</param>
    /// <returns></returns>
    String? Translate(String key, String? Lng);
}
