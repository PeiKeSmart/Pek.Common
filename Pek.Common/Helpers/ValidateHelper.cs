using System.Text.RegularExpressions;

using NewLife;

namespace Pek.Helpers;

/// <summary>
/// 验证帮助类
/// </summary>
public class ValidateHelper
{
    //邮件正则表达式
    //private static Regex _emailregex = new Regex(@"^[a-z0-9]([a-z0-9]*[-_]?[a-z0-9]+)*@([a-z0-9]*[-_]?[a-z0-9]+)+[\.][a-z]{2,3}([\.][a-z]{2})?$", RegexOptions.IgnoreCase);
    private static Regex _emailregex = new(@"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$", RegexOptions.IgnoreCase);
    //手机号正则表达式
    private static Regex _mobileregex = new("^(13|14|15|16|17|18|19)[0-9]{9}$");
    //固话号正则表达式
    private static Regex _phoneregex = new(@"^(\d{3,4}-?)?\d{7,8}$");
    //IP正则表达式
    private static Regex _ipregex = new(@"^(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])$");
    //日期正则表达式
    private static Regex _dateregex = new(@"(\d{4})-(\d{1,2})-(\d{1,2})");
    //数值(包括整数和小数)正则表达式
    private static Regex _numericregex = new(@"^[-]?[0-9]+(\.[0-9]+)?$");
    //邮政编码正则表达式
    private static Regex _zipcoderegex = new(@"^\d{6}$");

    /// <summary>
    /// 是否为邮箱名
    /// </summary>
    public static Boolean IsEmail(String s)
    {
        if (String.IsNullOrEmpty(s))
            return true;
        return _emailregex.IsMatch(s);
    }

    /// <summary>
    /// 是否为手机号
    /// </summary>
    public static Boolean IsMobile(String s)
    {
        if (String.IsNullOrEmpty(s))
            return true;
        return _mobileregex.IsMatch(s);
    }

    /// <summary>
    /// 判断当前时间是否在指定的时间段内
    /// </summary>
    /// <param name="periodList">指定时间段</param>
    /// <returns></returns>
    public static Boolean BetweenPeriod(String periodList) => BetweenPeriod(periodList, out _);

    /// <summary>
    /// 判断当前时间是否在指定的时间段内
    /// </summary>
    /// <param name="periodStr">指定时间段</param>
    /// <param name="liePeriod">所处时间段</param>
    /// <returns></returns>
    public static Boolean BetweenPeriod(String periodStr, out String liePeriod)
    {
        var periodList = periodStr.SplitString("\n");
        return BetweenPeriod(periodList, out liePeriod);
    }

    /// <summary>
    /// 判断当前时间是否在指定的时间段内
    /// </summary>
    /// <param name="periodList">指定时间段</param>
    /// <param name="liePeriod">所处时间段</param>
    /// <returns></returns>
    public static Boolean BetweenPeriod(String[] periodList, out String liePeriod)
    {
        if (periodList != null && periodList.Length > 0)
        {
            DateTime startTime;
            DateTime endTime;
            var nowTime = DateTime.Now;
            var nowDate = nowTime.Date;

            foreach (var period in periodList)
            {
                var index = period.IndexOf('-');
                startTime = period[..index].ToDateTime();
                endTime = period[(index + 1)..].ToDateTime();

                if (startTime < endTime)
                {
                    if (nowTime > startTime && nowTime < endTime)
                    {
                        liePeriod = period;
                        return true;
                    }
                }
                else
                {
                    if ((nowTime > startTime && nowTime < nowDate.AddDays(1)) || (nowTime < endTime))
                    {
                        liePeriod = period;
                        return true;
                    }
                }
            }
        }
        liePeriod = String.Empty;
        return false;
    }

    /// <summary>
    /// 判断一个ip是否在另一个ip内
    /// </summary>
    /// <param name="sourceIP">检测ip</param>
    /// <param name="targetIP">匹配ip</param>
    /// <returns></returns>
    public static Boolean InIP(String sourceIP, String targetIP)
    {
        if (String.IsNullOrEmpty(sourceIP) || String.IsNullOrEmpty(targetIP))
            return false;

        var sourceIPBlockList = sourceIP.SplitString(@".");
        var targetIPBlockList = targetIP.SplitString(@".");

        var sourceIPBlockListLength = sourceIPBlockList.Length;

        for (var i = 0; i < sourceIPBlockListLength; i++)
        {
            if (targetIPBlockList[i] == "*")
                return true;

            if (sourceIPBlockList[i] != targetIPBlockList[i])
            {
                return false;
            }
            else
            {
                if (i == 3)
                    return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 判断一个ip是否在另一个ip内
    /// </summary>
    /// <param name="sourceIP">检测ip</param>
    /// <param name="targetIPList">匹配ip列表</param>
    /// <returns></returns>
    public static Boolean InIPList(String sourceIP, String[] targetIPList)
    {
        if (targetIPList != null && targetIPList.Length > 0)
        {
            foreach (var targetIP in targetIPList)
            {
                if (InIP(sourceIP, targetIP))
                    return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 判断一个ip是否在另一个ip内
    /// </summary>
    /// <param name="sourceIP">检测ip</param>
    /// <param name="targetIPStr">匹配ip</param>
    /// <returns></returns>
    public static Boolean InIPList(String sourceIP, String targetIPStr)
    {
        var targetIPList = targetIPStr.SplitString("\n");
        return InIPList(sourceIP, targetIPList);
    }

    /// <summary>
    /// 是否为日期
    /// </summary>
    public static Boolean IsDate(String s) => _dateregex.IsMatch(s);

    /// <summary>
    /// 是否是身份证号
    /// </summary>
    public static Boolean IsIdCard(String id)
    {
        if (String.IsNullOrEmpty(id))
            return true;
        if (id.Length == 18)
            return CheckIDCard18(id);
        else if (id.Length == 15)
            return CheckIDCard15(id);
        else
            return false;
    }

    /// <summary>
    /// 是否为18位身份证号
    /// </summary>
    private static Boolean CheckIDCard18(String Id)
    {
        if (Int64.TryParse(Id.Remove(17), out var n) == false || n < Math.Pow(10, 16) || Int64.TryParse(Id.Replace('x', '0').Replace('X', '0'), out n) == false)
            return false;//数字验证

        var address = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";
        if (address.IndexOf(Id.Remove(2)) == -1)
            return false;//省份验证

        var birth = Id.Substring(6, 8).Insert(6, "-").Insert(4, "-");
        var time = new DateTime();
        if (DateTime.TryParse(birth, out time) == false)
            return false;//生日验证

        var arrVarifyCode = ("1,0,x,9,8,7,6,5,4,3,2").Split(',');
        var Wi = ("7,9,10,5,8,4,2,1,6,3,7,9,10,5,8,4,2").Split(',');
        var Ai = Id.Remove(17).ToCharArray();
        var sum = 0;
        for (var i = 0; i < 17; i++)
            sum += Int32.Parse(Wi[i]) * Int32.Parse(Ai[i].ToString());

        var y = -1;
        y = sum % 11;
        //Math.DivRem(sum, 11, out y);
        if (arrVarifyCode[y] != Id.Substring(17, 1).ToLower())
            return false;//校验码验证

        return true;//符合GB11643-1999标准
    }

    /// <summary>
    /// 是否为15位身份证号
    /// </summary>
    private static Boolean CheckIDCard15(String Id)
    {
        if (Int64.TryParse(Id, out var n) == false || n < Math.Pow(10, 14))
            return false;//数字验证

        var address = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";
        if (address.IndexOf(Id.Remove(2)) == -1)
            return false;//省份验证

        var birth = Id.Substring(6, 6).Insert(4, "-").Insert(2, "-");
        var time = new DateTime();
        if (DateTime.TryParse(birth, out time) == false)
            return false;//生日验证

        return true;//符合15位身份证标准
    }

}