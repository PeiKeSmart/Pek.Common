﻿using System.Text.RegularExpressions;

namespace Pek.SyntaxHighlighing;

public class Highlighter
{
    public List<HighlighterRule> Rules { get; set; }

    public Highlighter() => Rules = [new HighlighterRule(@"^.")];

    public Highlighter(IEnumerable<HighlighterRule> rules)
    {
        Rules = new List<HighlighterRule>(rules)
        {
            new(@"^.")
        };
    }

    public Highlighter(params HighlighterRule[] rules) : this((IEnumerable<HighlighterRule>)rules) { }

    public IEnumerable<HighlighterElement> Highlight(String text)
    {
        var i = 0;
        var str = text;
        var length = str.Length;

        Match match;
        while (i < length)
        {
            foreach (var rule in Rules)
            {
                match = rule.Pattern.Match(str);

                if (match.Success)
                {
                    if (match.Length == 0)
                        throw new Exception($"Rule {rule.Pattern} produced zero length result. Please modify it to not allow zero length results.");

                    yield return new HighlighterElement(match.Value, rule.Attributes);
                    i += match.Length;

                    str = str.Remove(0, match.Length);
                    break;
                }
            }
        }
    }

    public static Highlighter FromExtension(String extension)
    {
        return extension switch
        {
            ".cs" or ".java" => CSharpHighlighter,
            _ => NoHighlighter,
        };
    }

    public static Highlighter NoHighlighter => _noHighlighter;
    private static readonly Highlighter _noHighlighter = new();

    public static Highlighter CSharpHighlighter => _cSharpHighlighter;
    private static readonly Highlighter _cSharpHighlighter = new(
        new HighlighterRule(@"^\/\/.*?\n") { { "class", "comment" } },                      //Line comments
        new HighlighterRule(@"^\/\*.*?\*\/") { { "class", "comment" } },                    //Muliline comments
        new HighlighterRule(@"^""[^""\\]*(\\.[^""\\]*)*""") { { "class", "string" } },      //String literals
        new HighlighterRule(@"^\s"),                                                        //Whitespace
        new HighlighterRule(@"^\b(bool|byte|sbyte|char|decimal|double|float|int|uint|long|ulong|new|object|short|ushort|string|base|this|void)\b") { { "class", "keyword" } },  //Keywords
        new HighlighterRule(@"^\b(as|break|case|catch|checked|continue|default|do|else|finally|fixed|for|foreach|goto|if|is|lock|return|switch|throw|try|unchecked|while|yield)\b") { { "class", "keyword" } }, //More keywords
        new HighlighterRule(@"^\b(abstract|class|const|delegate|enum|event|explicit|extern|get|implicit|in|internal|interface|nameof|namespace|operator|out|override|params|partial|private|protected|public|readonly|ref|sealed|set|sizeof|static|struct|typeof|using|virtual|volatile)\b") { { "class", "keyword" } } //Even more keywords
    );
}