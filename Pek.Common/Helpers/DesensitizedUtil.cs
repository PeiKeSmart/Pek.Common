using System.Text.RegularExpressions;

using NewLife;
using NewLife.Log;

using Pek.Log;

namespace Pek.Helpers;

/// <summary>
/// 信息脱敏工具类
/// https://github.com/dotnet-easy/easytool/blob/main/EasyTool.Core/ToolCategory/DesensitizedUtil.cs
/// </summary>
public static partial class DesensitizedUtil
{
    private static readonly Regex IdcardRegex = new(@"^\d{15}(\d{2}[0-9xX])?$");
    private static readonly Regex MobileRegex = new(@"^(13\d|14[5-9]|15[^4\D]|16\d|17[0-8]|18\d|19[0-3,5-9])\d{8}$");
    private static readonly Regex TelRegex = new(@"^(\d{3,4}-?)?\d{7,8}$");
    private static readonly Regex EmailRegex = new(@"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$");
    private static readonly Regex BankcardRegex = new(@"^\d{12,19}$");
    private static readonly String[] AreaCodes = [
        "11", "12", "13", "14", "15", "21", "22", "23", "31", "32",
        "33", "34", "35", "36", "37", "41", "42", "43", "44", "45",
        "46", "50", "51", "52", "53", "54", "61", "62", "63", "64",
        "65"
    ];

    /// <summary>
    /// 脱敏用户ID，只保留前两位和后两位
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>脱敏后的用户ID</returns>
    public static String UserId(String userId)
    {
        if (String.IsNullOrWhiteSpace(userId) || userId.Length <= 4)
        {
            return userId;
        }

        var prefix = userId[..2];
        var suffix = userId.Substring(userId.Length - 2, 2);
        return prefix + new String('*', userId.Length - 4) + suffix;
    }

    /// <summary>
    /// 脱敏中文姓名，只保留第一个汉字和最后一个汉字，其他用*代替
    /// </summary>
    /// <param name="name">中文姓名</param>
    /// <returns>脱敏后的中文姓名</returns>
    public static String ChineseName(String name)
    {
        if (String.IsNullOrWhiteSpace(name) || name.Length <= 2)
        {
            return name;
        }

        var firstChar = name[..1];
        var lastChar = name.Substring(name.Length - 1, 1);
        return firstChar + new String('*', name.Length - 2) + lastChar;
    }

    /// <summary>
    /// 脱敏身份证号码，只保留前两位和后四位
    /// </summary>
    /// <param name="idcard">身份证号码</param>
    /// <returns>脱敏后的身份证号码</returns>
    public static String Idcard(String idcard)
    {
        if (String.IsNullOrWhiteSpace(idcard) || !IdcardRegex.IsMatch(idcard))
        {
            return idcard;
        }

        var prefix = idcard[..4];
        var suffix = idcard.Substring(idcard.Length - 4, 4);
        return prefix + new String('*', idcard.Length - 8) + suffix;
    }

    /// <summary>
    /// 脱敏座机号码，只保留前三位和后四位
    /// </summary>
    /// <param name="tel">座机号码</param>
    /// <returns>脱敏后的座机号码</returns>
    public static String Tel(String tel)
    {
        if (String.IsNullOrWhiteSpace(tel) || !TelRegex.IsMatch(tel))
        {
            return tel;
        }

        var startIndex = tel.Length - 4 > 0 ? tel.Length - 4 : 0;
        var suffix = tel.Substring(startIndex, 4);
        return tel[..3] + new String('*', startIndex) + suffix;
    }

    /// <summary>
    /// 脱敏手机号码，只保留前三位和后四位
    /// </summary>
    /// <param name="mobile">手机号码</param>
    /// <returns>脱敏后的手机号码</returns>
    public static String Mobile(String mobile)
    {
        if (String.IsNullOrWhiteSpace(mobile) || !MobileRegex.IsMatch(mobile))
        {
            return mobile;
        }

        var prefix = mobile[..3];
        var suffix = mobile.Substring(mobile.Length - 4, 4);
        return prefix + new String('*', mobile.Length - 7) + suffix;
    }

    /// <summary>
    /// 脱敏地址信息，只保留前五个字符和后三个字符
    /// </summary>
    /// <param name="address">地址信息</param>
    /// <returns>脱敏后的地址信息</returns>
    public static String Address(String address)
    {
        if (String.IsNullOrWhiteSpace(address) || address.Length <= 8)
        {
            return address;
        }

        var prefix = address[..5];
        var suffix = address.Substring(address.Length - 3, 3);
        return prefix + new String('*', address.Length - 8) + suffix;
    }

    /// <summary>
    /// 脱敏电子邮件，只保留邮箱前缀的前三个字符和后两个字符
    /// </summary>
    /// <param name="email">电子邮件</param>
    /// <returns>脱敏后的电子邮件</returns>
    public static String Email(String email)
    {
        if (String.IsNullOrWhiteSpace(email) || !EmailRegex.IsMatch(email))
        {
            return email;
        }

        var atIndex = email.IndexOf('@');
        if (atIndex <= 3)
        {
            return email;
        }

        var prefix = email[..3];
        var suffix = email.Substring(atIndex - 2, 2);
        return prefix + new String('*', atIndex - 5) + suffix + email[atIndex..];
    }

    /// <summary>
    /// 脱敏密码，只保留前两个字符和后两个字符
    /// </summary>
    /// <param name="password">密码</param>
    /// <returns>脱敏后的密码</returns>
    public static String Password(String password)
    {
        if (String.IsNullOrWhiteSpace(password) || password.Length <= 4)
        {
            return password;
        }
        var prefix = password[..2];
        var suffix = password.Substring(password.Length - 2, 2);
        return prefix + new String('*', password.Length - 4) + suffix;
    }

    /// <summary>
    /// 脱敏中国大陆车牌号，只保留前两个字符和最后一个字符
    /// </summary>
    /// <param name="plateNumber">车牌号</param>
    /// <returns>脱敏后的车牌号</returns>
    public static String PlateNumber(String plateNumber)
    {
        if (String.IsNullOrWhiteSpace(plateNumber) || plateNumber.Length <= 3)
        {
            return plateNumber;
        }

        var prefix = plateNumber[..2];
        var suffix = plateNumber.Substring(plateNumber.Length - 1, 1);
        return prefix + new String('*', plateNumber.Length - 3) + suffix;
    }

    /// <summary>
    /// 脱敏银行卡号，只保留前四位和后四位
    /// </summary>
    /// <param name="bankcard">银行卡号</param>
    /// <returns>脱敏后的银行卡号</returns>
    public static String Bankcard(String bankcard)
    {
        if (String.IsNullOrWhiteSpace(bankcard) || !BankcardRegex.IsMatch(bankcard))
        {
            return bankcard;
        }

        var prefix = bankcard[..4];
        var suffix = bankcard.Substring(bankcard.Length - 4, 4);
        return prefix + new String('*', bankcard.Length - 8) + suffix;
    }

    /// <summary>
    /// 加密手机号码
    /// </summary>
    /// <param name="phone">手机号码</param>
    public static String EncryptPhoneOfChina(String phone) =>
        String.IsNullOrWhiteSpace(phone)
            ? String.Empty
            : $"{phone[..3]}******{phone.Substring(phone.Length - 2, 2)}";

    /// <summary>
    /// 加密车牌号
    /// </summary>
    /// <param name="plateNumber">车牌号</param>
    public static String EncryptPlateNumberOfChina(String plateNumber) =>
        String.IsNullOrWhiteSpace(plateNumber)
            ? String.Empty
            : $"{plateNumber[..2]}***{plateNumber.Substring(plateNumber.Length - 2, 2)}";

    /// <summary>
    /// 加密汽车VIN
    /// </summary>
    /// <param name="vinCode">汽车VIN</param>
    public static String EncryptVinCode(String vinCode) =>
        String.IsNullOrWhiteSpace(vinCode)
            ? String.Empty
            : $"{vinCode[..3]}***********{vinCode.Substring(vinCode.Length - 3, 3)}";

    /// <summary>
    /// 格式化金额
    /// </summary>
    /// <param name="money">金额</param>
    /// <param name="isEncrypt">是否加密。默认：false</param>
    public static String FormatMoney(Decimal money, Boolean isEncrypt = false) => isEncrypt ? "***" : $"{money:N2}";

    /// <summary>
    /// 将传入的字符串中间部分字符替换成特殊字符
    /// </summary>
    /// <param name="value">需要替换的字符串</param>
    /// <param name="startLen">前保留长度</param>
    /// <param name="endLen">尾保留长度</param>
    /// <param name="specialChar">特殊字符</param>
    /// <returns>被特殊字符替换的字符串</returns>
    public static String ReplaceWithSpecialChar(String value, Int32 startLen = 2, Int32 endLen = 2, Char specialChar = '*')
    {
        try
        {
            if (value.IsNullOrWhiteSpace())
            {
                return String.Empty;
            }
            var lenth = value.Length - startLen - endLen;

            if (lenth <= 0)
            {
                return value;
            }

            var replaceStr = value.Substring(startLen, lenth);

            var specialStr = String.Empty;

            for (var i = 0; i < replaceStr.Length; i++)
            {
                specialStr += specialChar;
            }

            value = value.Replace(replaceStr, specialStr);
        }
        catch (Exception ex)
        {
            DTrace.WriteException(ex);
        }

        return value;
    }
}