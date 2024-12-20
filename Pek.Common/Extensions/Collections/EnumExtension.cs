﻿using System.Collections.Concurrent;

namespace Pek;

/// <summary>
/// 枚举扩展属性
/// </summary>
public static class EnumExtension
{
    private static ConcurrentDictionary<string, Dictionary<string, string>> enumCache;

    private static ConcurrentDictionary<string, Dictionary<string, string>> EnumCache
    {
        get
        {
            if (enumCache == null)
            {
                enumCache = new ConcurrentDictionary<string, Dictionary<string, string>>();
            }
            return enumCache;
        }
        set { enumCache = value; }
    }

    /// <summary>
    /// 获得枚举提示文本
    /// </summary>
    /// <param name="en"></param>
    /// <returns></returns>
    public static string GetEnumText(this Enum en)
    {
        var enString = string.Empty;
        if (null == en) return enString;
        var type = en.GetType();
        enString = en.ToString();
        if (!EnumCache.ContainsKey(type.FullName))
        {
            var fields = type.GetFields();
            var temp = new Dictionary<string, string>();
            foreach (var item in fields)
            {
                var attrs = item.GetCustomAttributes(typeof(TextAttribute), false);
                if (attrs.Length == 1)
                {
                    var v = ((TextAttribute)attrs[0]).Value;
                    temp.Add(item.Name, v);
                }
            }
            EnumCache.TryAdd(type.FullName, temp);
        }
        if (EnumCache[type.FullName].ContainsKey(enString))
        {
            return EnumCache[type.FullName][enString];
        }
        return enString;
    }
}

public class TextAttribute : Attribute
{
    public TextAttribute(string value)
    {
        Value = value;
    }

    public string Value { get; set; }
}