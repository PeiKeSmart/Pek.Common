using System.Text;

namespace Pek.FastToken;

internal class FastReplacerSnippet
{
    private class InnerSnippet
    {
        public FastReplacerSnippet? Snippet;
        public Int32 Start; // Position of the snippet in parent snippet's Text.
        public Int32 End; // Position of the snippet in parent snippet's Text.
        public Int32 Order1; // Order of snippets with a same Start position in their parent.
        public Int32 Order2; // Order of snippets with a same Start position and Order1 in their parent.

        public override String ToString() => "InnerSnippet: " + Snippet?.Text;
    }

    public readonly String Text;
    private readonly List<InnerSnippet> InnerSnippets;

    public FastReplacerSnippet(String text)
    {
        Text = text;
        InnerSnippets = [];
    }

    public override String ToString() => "Snippet: " + Text;

    public void Append(FastReplacerSnippet snippet)
    {
        InnerSnippets.Add(new InnerSnippet
        {
            Snippet = snippet,
            Start = Text.Length,
            End = Text.Length,
            Order1 = 1,
            Order2 = InnerSnippets.Count
        });
    }

    public void Replace(Int32 start, Int32 end, FastReplacerSnippet snippet)
    {
        InnerSnippets.Add(new InnerSnippet
        {
            Snippet = snippet,
            Start = start,
            End = end,
            Order1 = 0,
            Order2 = 0
        });
    }

    public void InsertBefore(Int32 start, FastReplacerSnippet snippet)
    {
        InnerSnippets.Add(new InnerSnippet
        {
            Snippet = snippet,
            Start = start,
            End = start,
            Order1 = 2,
            Order2 = InnerSnippets.Count
        });
    }

    public void InsertAfter(Int32 end, FastReplacerSnippet snippet)
    {
        InnerSnippets.Add(new InnerSnippet
        {
            Snippet = snippet,
            Start = end,
            End = end,
            Order1 = 1,
            Order2 = InnerSnippets.Count
        });
    }

    public void ToString(StringBuilder sb)
    {
        InnerSnippets.Sort(delegate (InnerSnippet a, InnerSnippet b)
        {
            if (a == b) return 0;
            if (a.Start != b.Start) return a.Start - b.Start;
            if (a.End != b.End) return a.End - b.End; // Disambiguation if there are inner snippets inserted before a token (they have End==Start) go before inner snippets inserted instead of a token (End>Start).
            if (a.Order1 != b.Order1) return a.Order1 - b.Order1;
            if (a.Order2 != b.Order2) return a.Order2 - b.Order2;
            throw new InvalidOperationException(String.Format(
                "Internal error: Two snippets have ambigous order. At position from {0} to {1}, order1 is {2}, order2 is {3}. First snippet is \"{4}\", second snippet is \"{5}\".",
                a.Start, a.End, a.Order1, a.Order2, a.Snippet?.Text, b.Snippet?.Text));
        });
        var lastPosition = 0;
        foreach (var innerSnippet in InnerSnippets)
        {
            if (innerSnippet.Start < lastPosition)
                throw new InvalidOperationException(String.Format(
                    "Internal error: Token is overlapping with a previous token. Overlapping token is from position {0} to {1}, previous token ends at position {2} in snippet \"{3}\".",
                    innerSnippet.Start, innerSnippet.End, lastPosition, Text));
            sb.Append(Text, lastPosition, innerSnippet.Start - lastPosition);
            innerSnippet.Snippet?.ToString(sb);
            lastPosition = innerSnippet.End;
        }
        sb.Append(Text, lastPosition, Text.Length - lastPosition);
    }

    public Int32 GetLength()
    {
        var len = Text.Length;
        foreach (var innerSnippet in InnerSnippets)
        {
            len -= innerSnippet.End - innerSnippet.Start;
            len += innerSnippet.Snippet?.GetLength() ?? 0;
        }
        return len;
    }
}