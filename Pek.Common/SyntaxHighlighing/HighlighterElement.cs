namespace Pek.SyntaxHighlighing;

public class HighlighterElement
{
    public string Value { get; set; }
    public Dictionary<String, String> Attributes { get; set; }

    public HighlighterElement(String value, IDictionary<String, String> attributes)
    {
        Value = value;
        Attributes = new Dictionary<String, String>(attributes);
    }
}