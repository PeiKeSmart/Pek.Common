using System.Dynamic;
using System.Text.Json;

namespace Pek;

/// <summary>
/// 通过Dynamic访问System.Text.Json对象
/// 参考：https://blog.51cto.com/shanyou/3048736
/// </summary>
public class JTextAccessor(JsonElement content) : DynamicObject
{
    private readonly JsonElement _content = content;

    public override Boolean TryGetMember(GetMemberBinder binder, out Object? result)
    {
        result = null;
        if (_content.TryGetProperty(binder.Name, out var value))
        {
            result = JTextAccessor.Obtain(value);
        }
        else return false;
        return true;
    }

    private static Object? Obtain(in JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String: return element.GetString();
            case JsonValueKind.Null: return null;
            case JsonValueKind.False: return false;
            case JsonValueKind.True: return true;
            case JsonValueKind.Number: return element.GetDouble();
            default: break;
        }

        if (element.ValueKind == JsonValueKind.Array)
        {
            var list = new List<Object>();
            foreach (var item in element.EnumerateArray())
            {
                var m = Obtain(item);
                if (m == null) continue;

                list.Add(m);
            }

            return list;
        }
        // Undefine、Object
        else return new JTextAccessor(element);
    }
}