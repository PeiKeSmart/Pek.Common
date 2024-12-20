namespace Pek.Helpers;

/// <summary>
/// 邮件帮助类
/// </summary>
public class MailHelper
{
    /// <summary>
    /// 获取邮箱后缀
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static String GetEmailSuffix(String email)
    {
        if (String.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email address cannot be null or empty.", nameof(email));
        }

        var atIndex = email.LastIndexOf('@');
        if (atIndex < 0 || atIndex == email.Length - 1)
        {
            throw new ArgumentException("Invalid email address format.", nameof(email));
        }

        return email[(atIndex + 1)..];
    }
}
