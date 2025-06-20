﻿namespace Pek.Helpers;

/// <summary>
/// 32位64位整数解压缩
/// </summary>
public class CompresTo
{
    public static String IntToi32(Int64 xx)
    {
        var a = "";
        while (xx >= 1)
        {
            Int32 index = Convert.ToInt16(xx - (xx / 32) * 32);
            a = Base64Code[index] + a;
            xx /= 32;
        }
        return a;
    }

    public static Int64 I32ToInt(String xx)
    {
        Int64 a = 0;
        var power = xx.Length - 1;

        for (var i = 0; i <= power; i++)
        {
            a += Base64CodeR[xx[power - i].ToString()] * Convert.ToInt64(Math.Pow(32, i));
        }

        return a;
    }


    public static String IntToi64(Int64 xx)
    {
        var a = "";
        while (xx >= 1)
        {
            var index = Convert.ToInt16(xx - (xx / 64) * 64);
            a = Base64Code[index] + a;
            xx /= 64;
        }
        return a;
    }

    public static Int64 I64ToInt(String xx)
    {
        Int64 a = 0;
        var power = xx.Length - 1;

        for (var i = 0; i <= power; i++)
        {
            a += Base64CodeR[xx[power - i].ToString()] * Convert.ToInt64(Math.Pow(64, i));
        }

        return a;
    }

    public static Dictionary<Int32, String> Base64Code = new() {
            {   0  ,"z"}, {   1  ,"1"}, {   2  ,"2"}, {   3  ,"3"}, {   4  ,"4"}, {   5  ,"5"}, {   6  ,"6"}, {   7  ,"7"}, {   8  ,"8"}, {   9  ,"9"},
            {   10  ,"a"}, {   11  ,"b"}, {   12  ,"c"}, {   13  ,"d"}, {   14  ,"e"}, {   15  ,"f"}, {   16  ,"g"}, {   17  ,"h"}, {   18  ,"i"}, {   19  ,"j"},
            {   20  ,"k"}, {   21  ,"x"}, {   22  ,"m"}, {   23  ,"n"}, {   24  ,"y"}, {   25  ,"p"}, {   26  ,"q"}, {   27  ,"r"}, {   28  ,"s"}, {   29  ,"t"},
            {   30  ,"u"}, {   31  ,"v"}, {   32  ,"w"}, {   33  ,"x"}, {   34  ,"y"}, {   35  ,"z"}, {   36  ,"A"}, {   37  ,"B"}, {   38  ,"C"}, {   39  ,"D"},
            {   40  ,"E"}, {   41  ,"F"}, {   42  ,"G"}, {   43  ,"H"}, {   44  ,"I"}, {   45  ,"J"}, {   46  ,"K"}, {   47  ,"L"}, {   48  ,"M"}, {   49  ,"N"},
            {   50  ,"O"}, {   51  ,"P"}, {   52  ,"Q"}, {   53  ,"R"}, {   54  ,"S"}, {   55  ,"T"}, {   56  ,"U"}, {   57  ,"V"}, {   58  ,"W"}, {   59  ,"X"},
            {   60  ,"Y"}, {   61  ,"Z"}, {   62  ,"-"}, {   63  ,"_"},
        };

    public static Dictionary<String, Int32> Base64CodeR => Enumerable.Range(0, Base64Code.Count).ToDictionary(i => Base64Code[i], i => i);
}