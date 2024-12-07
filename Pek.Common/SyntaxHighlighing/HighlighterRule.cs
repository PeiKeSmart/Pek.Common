using System.Collections;
using System.Text.RegularExpressions;

namespace Pek.SyntaxHighlighing;

public class HighlighterRule : IDictionary<String, String>
{
    public Regex Pattern { get; set; }
    public Dictionary<String, String> Attributes { get; set; }

    public ICollection<String> Keys => ((IDictionary<String, String>)Attributes).Keys;

    public ICollection<String> Values => ((IDictionary<String, String>)Attributes).Values;

    public Int32 Count => ((IDictionary<String, String>)Attributes).Count;

    public Boolean IsReadOnly => ((IDictionary<String, String>)Attributes).IsReadOnly;

    public String this[String key] { get => ((IDictionary<String, String>)Attributes)[key]; set => ((IDictionary<String, String>)Attributes)[key] = value; }

    public HighlighterRule(Regex pattern)
    {
        Pattern = pattern;
        Attributes = [];
    }

    public HighlighterRule(String patternString) : this(new Regex(patternString, RegexOptions.Compiled | RegexOptions.Singleline)) { }

    public HighlighterRule(Regex pattern, IDictionary<String, String> attributes)
    {
        Pattern = pattern;
        Attributes = new Dictionary<String, String>(attributes);
    }

    public HighlighterRule(String patternString, IDictionary<String, String> attributes)
        : this(new Regex(patternString, RegexOptions.Compiled | RegexOptions.Singleline), attributes) { }

    public void Add(String key, String value) => ((IDictionary<String, String>)Attributes).Add(key, value);

    public Boolean ContainsKey(String key) => ((IDictionary<String, String>)Attributes).ContainsKey(key);

    public Boolean Remove(String key) => ((IDictionary<String, String>)Attributes).Remove(key);

    public Boolean TryGetValue(String key, out String value) => ((IDictionary<String, String>)Attributes).TryGetValue(key, out value!);

    public void Add(KeyValuePair<String, String> item) => ((IDictionary<String, String>)Attributes).Add(item);

    public void Clear() => ((IDictionary<String, String>)Attributes).Clear();

    public Boolean Contains(KeyValuePair<String, String> item) => ((IDictionary<String, String>)Attributes).Contains(item);

    public void CopyTo(KeyValuePair<String, String>[] array, Int32 arrayIndex) => ((IDictionary<String, String>)Attributes).CopyTo(array, arrayIndex);

    public Boolean Remove(KeyValuePair<String, String> item) => ((IDictionary<String, String>)Attributes).Remove(item);

    public IEnumerator<KeyValuePair<String, String>> GetEnumerator() => ((IDictionary<String, String>)Attributes).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IDictionary<String, String>)Attributes).GetEnumerator();
}