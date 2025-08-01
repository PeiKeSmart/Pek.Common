using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace Pek.Helpers;

public static class RandomUtilities
{
    private static readonly Random random = new();

    public static Int32 GetRandomInt(Int32 min, Int32 max, Int32[]? excludeValues = null)
    {
        if (min == max)
            return min;
        if (max < min)
            ConvertUtilities.Switch(ref min, ref max);
        var value = random.Next(min, max + 1);
        if (excludeValues != null && excludeValues.Contains(value))
            return GetRandomInt(max, excludeValues);
        return value;
    }

    public static Int32 GetRandomIndex(Int32 maxCount) => random.Next(0, maxCount);

    public static Int32 GetRandomPositiveInt(Int32 maxCount) => random.Next(1, maxCount);

    public static Int32 GetRandomInt(Int32 max, Int32[]? excludeValues = null)
    {
        var value = random.Next(-1 * max, max + 1);
        if (excludeValues != null && excludeValues.Contains(value))
            return GetRandomInt(max, excludeValues);
        return value;
    }

    public static Char GetRandomChar(this String text) => text[GetRandomIndex(text.Length)];

    public static T GetRandomValue<T>(this IReadOnlyList<T> items) => items[GetRandomIndex(items.Count)];

    public static T GetRandomEnum<T>()
    {
        var enumValues = Enum.GetValues(typeof(T)).Cast<Int32>().ToArray();
        return (T)(Object)enumValues.GetRandomValue();
    }

    public static Boolean GenerateBool() => GetRandomInt(1, 2) == 1;

    public static IEnumerable<T> OrderByRandom<T>(this IEnumerable<T> items) => items.OrderBy(x => GetRandomIndex(100000));
}

public static class ArrayUtilities
{
    public static Boolean IsNullOrEmpty(this Array array)
    {
        if (array == null || array.Length == 0)
            return true;
        else
            return false;
    }
}

public static class XmlUtilities
{
    public static T? FromXml<T>(this String xml)
        where T : class, new()
    {
        if (String.IsNullOrEmpty(xml))
            return null;
        using var memoryStream = new MemoryStream();
        memoryStream.Write(Encoding.UTF8.GetBytes(xml));
        memoryStream.Position = 0;
        var serializer = new XmlSerializer(typeof(T));
        return (T?)serializer.Deserialize(memoryStream);
    }

    public static String ToXml<T>(this T obj)
    {
        using var memoryStream = new MemoryStream();
        var serializer = new XmlSerializer(typeof(T));
        serializer.Serialize(memoryStream, obj);
        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }
}

public static class ConvertUtilities
{
    public static Object ParseTo(this String str, Type toType) => Convert.ChangeType(str, toType, CultureInfo.InvariantCulture);

    public static void Switch<T>(ref T obj1, ref T obj2) => (obj2, obj1) = (obj1, obj2);

    public static Guid GenerateGuid(this String text)
    {
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(Encoding.Default.GetBytes(text));
        return new Guid(hash);
    }

    public static String To1CString(this Double d) => d.ToString("N1");

    public static String To1CString(this Single f) => f.ToString("N1");

    public static IEnumerable<IEnumerable<T>> SplitByCount<T>(this IEnumerable<T> items, Int32 countLimit) => items.SplitByLimit(countLimit, x => 1);

    public static IEnumerable<IEnumerable<T>> SplitByLimit<T>(this IEnumerable<T> items, Int32 lengthLimit, Func<T, Int32> getLength)
    {
        var result = new List<T>();
        var totalLength = 0;
        foreach (var item in items)
        {
            var length = getLength(item);
            if (totalLength + length > lengthLimit && result.Count != 0)
            {
                yield return result.ToArray();
                result.Clear();
                totalLength = 0;
            }
            totalLength += length;
            result.Add(item);
        }
        if (result.Count != 0)
            yield return result;
    }

    public static IEnumerable<IEnumerable<T>> SplitByEqualLimit<T>(this IEnumerable<T> items, Int32 lengthLimit, Func<T, Int32> getLength)
    {
        var result = new List<T>();
        var maxLength = 0;
        foreach (var item in items)
        {
            var length = getLength(item);
            if (maxLength < length)
                maxLength = length;
            if (maxLength * (result.Count + 1) > lengthLimit && result.Count != 0)
            {
                yield return result.ToArray();
                result.Clear();
                maxLength = 0;
            }
            result.Add(item);
        }
        if (result.Count != 0)
            yield return result;
    }

    public static IEnumerable<T> SequenceElements<T>(Int32 count, IEnumerable<T> initialPreviousValues, Func<T[], T> getNextValue)
    {
        var preriousItems = initialPreviousValues.ToList();
        for (var i = 0; i < count; ++i)
        {
            var value = getNextValue([.. preriousItems]);
            preriousItems.Add(value);
            yield return value;
        }
    }
}

public static class LogUtilities
{
    private static readonly Object _lock = new();
    public static void Log(String filePath, Int32 maxSizeInKb, Exception exception) => Log(filePath, maxSizeInKb, exception.ToString());

    public static void Log(String filePath, Int32 maxSizeInKb, String message)
    {
        var dateTime = DateTime.Now.ToString("dd'.'MM'.'yy' 'HH':'mm':'ss");
        var logContent = $"{dateTime}{Environment.NewLine}{message}";
        lock (_lock)
        {
            if (File.Exists(filePath) && maxSizeInKb > 0 && new FileInfo(filePath).Length > maxSizeInKb * 1024)
            {
                var lines = File.ReadAllLines(filePath);
                File.WriteAllLines(filePath, lines.Skip(lines.Length / 2));
            }
            File.AppendAllText(filePath, Environment.NewLine + Environment.NewLine + logContent);
        }
    }
}

public static class FileUtilities
{
    public static void SimpleWatchFiles(this FileSystemWatcher watcher, Action action)
    {
        watcher.EnableRaisingEvents = true;
        watcher.NotifyFilter = NotifyFilters.LastWrite;
        watcher.Changed += delegate { action(); };
        watcher.Created += delegate { action(); };
        watcher.Deleted += delegate { action(); };
        watcher.Renamed += delegate { action(); };
    }
}