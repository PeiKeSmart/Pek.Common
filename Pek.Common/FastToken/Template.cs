using System.Text.RegularExpressions;

namespace Pek.FastToken;

/// <summary>
/// 模版引擎
/// </summary>
/// <remarks>
/// 模版引擎
/// </remarks>
/// <param name="content"></param>
public class Template(String content)
{
    private String Content { get; set; } = content;

    /// <summary>
    /// 创建模板
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public static Template Create(String content) => new(content);

    /// <summary>
    /// 设置变量
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public Template Set(String key, String value)
    {
        Content = Content.Replace("{{" + key + "}}", value);
        return this;
    }

    /// <summary>
    /// 渲染模板
    /// </summary>
    /// <param name="check">是否检查未使用的模板变量</param>
    /// <returns></returns>
    public String Render(Boolean check = false)
    {
        if (check)
        {
            var mc = Regex.Matches(Content, @"\{\{.+?\}\}");
            foreach (Match m in mc)
            {
                throw new ArgumentException($"模版变量{m.Value}未被使用");
            }
        }

        return Content;
    }
}