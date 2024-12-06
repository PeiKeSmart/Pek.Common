using System.Text;

namespace Pek.FastToken;

/// <summary>
/// FastReplacer是类似于StringBuilder的实用程序类，具有快速替换功能。
/// FastReplacer仅限于替换格式正确的令牌。
/// 使用ToString()函数获取最终文本。
/// </summary>
public class FastReplacer
{
    private readonly String TokenOpen;
    private readonly String TokenClose;

    /// <summary>
    /// All tokens that will be replaced must have same opening and closing delimiters, such as "{" and "}".
    /// </summary>
    /// <param name="tokenOpen">Opening delimiter for tokens.</param>
    /// <param name="tokenClose">Closing delimiter for tokens.</param>
    /// <param name="caseSensitive">Set caseSensitive to false to use case-insensitive search when replacing tokens.</param>
    public FastReplacer(String tokenOpen, String tokenClose, Boolean caseSensitive = true)
    {
        if (String.IsNullOrEmpty(tokenOpen) || String.IsNullOrEmpty(tokenClose))
            throw new ArgumentException("Token must have opening and closing delimiters, such as \"{\" and \"}\".");

        TokenOpen = tokenOpen;
        TokenClose = tokenClose;

        var stringComparer = caseSensitive ? StringComparer.Ordinal : StringComparer.InvariantCultureIgnoreCase;
        OccurrencesOfToken = new Dictionary<String, List<TokenOccurrence>>(stringComparer);
    }

    private readonly FastReplacerSnippet RootSnippet = new("");

    private class TokenOccurrence
    {
        public FastReplacerSnippet? Snippet;
        public Int32 Start; // Position of a token in the snippet.
        public Int32 End; // Position of a token in the snippet.
    }

    private readonly Dictionary<String, List<TokenOccurrence>> OccurrencesOfToken;

    public void Append(String text)
    {
        var snippet = new FastReplacerSnippet(text);
        RootSnippet.Append(snippet);
        ExtractTokens(snippet);
    }

    /// <returns>Returns true if the token was found, false if nothing was replaced.</returns>
    public Boolean Replace(String token, String text)
    {
        ValidateToken(token, text, false);
        if (OccurrencesOfToken.TryGetValue(token, out var occurrences) && occurrences.Count > 0)
        {
            OccurrencesOfToken.Remove(token);
            var snippet = new FastReplacerSnippet(text);
            foreach (var occurrence in occurrences)
                occurrence.Snippet?.Replace(occurrence.Start, occurrence.End, snippet);
            ExtractTokens(snippet);
            return true;
        }
        return false;
    }

    /// <returns>Returns true if the token was found, false if nothing was replaced.</returns>
    public Boolean InsertBefore(String token, String text)
    {
        ValidateToken(token, text, false);
        if (OccurrencesOfToken.TryGetValue(token, out var occurrences) && occurrences.Count > 0)
        {
            var snippet = new FastReplacerSnippet(text);
            foreach (var occurrence in occurrences)
                occurrence.Snippet?.InsertBefore(occurrence.Start, snippet);
            ExtractTokens(snippet);
            return true;
        }
        return false;
    }

    /// <returns>Returns true if the token was found, false if nothing was replaced.</returns>
    public Boolean InsertAfter(String token, String text)
    {
        ValidateToken(token, text, false);
        if (OccurrencesOfToken.TryGetValue(token, out var occurrences) && occurrences.Count > 0)
        {
            var snippet = new FastReplacerSnippet(text);
            foreach (var occurrence in occurrences)
                occurrence.Snippet?.InsertAfter(occurrence.End, snippet);
            ExtractTokens(snippet);
            return true;
        }
        return false;
    }

    public Boolean Contains(String token)
    {
        ValidateToken(token, token, false);
        if (OccurrencesOfToken.TryGetValue(token, out var occurrences))
            return occurrences.Count > 0;
        return false;
    }

    private void ExtractTokens(FastReplacerSnippet snippet)
    {
        var last = 0;
        while (last < snippet.Text.Length)
        {
            // Find next token position in snippet.Text:
            var start = snippet.Text.IndexOf(TokenOpen, last, StringComparison.InvariantCultureIgnoreCase);
            if (start == -1)
                return;
            var end = snippet.Text.IndexOf(TokenClose, start + TokenOpen.Length, StringComparison.InvariantCultureIgnoreCase);
            if (end == -1)
                throw new ArgumentException(String.Format("Token is opened but not closed in text \"{0}\".", snippet.Text));
            var eol = snippet.Text.IndexOf('\n', start + TokenOpen.Length);
            if (eol != -1 && eol < end)
            {
                last = eol + 1;
                continue;
            }

            // Take the token from snippet.Text:
            end += TokenClose.Length;
            var token = snippet.Text[start..end];
            var context = snippet.Text;
            ValidateToken(token, context, true);

            // Add the token to the dictionary:
            var tokenOccurrence = new TokenOccurrence { Snippet = snippet, Start = start, End = end };
            if (OccurrencesOfToken.TryGetValue(token, out var occurrences))
                occurrences.Add(tokenOccurrence);
            else
                OccurrencesOfToken.Add(token, [tokenOccurrence]);

            last = end;
        }
    }

    private void ValidateToken(String token, String context, Boolean alreadyValidatedStartAndEnd)
    {
        if (!alreadyValidatedStartAndEnd)
        {
            if (!token.StartsWith(TokenOpen, StringComparison.InvariantCultureIgnoreCase))
                throw new ArgumentException(String.Format("Token \"{0}\" shoud start with \"{1}\". Used with text \"{2}\".", token, TokenOpen, context));
            var closePosition = token.IndexOf(TokenClose, StringComparison.InvariantCultureIgnoreCase);
            if (closePosition == -1)
                throw new ArgumentException(String.Format("Token \"{0}\" should end with \"{1}\". Used with text \"{2}\".", token, TokenClose, context));
            if (closePosition != token.Length - TokenClose.Length)
                throw new ArgumentException(String.Format("Token \"{0}\" is closed before the end of the token. Used with text \"{1}\".", token, context));
        }

        if (token.Length == TokenOpen.Length + TokenClose.Length)
            throw new ArgumentException(String.Format("Token has no body. Used with text \"{0}\".", context));
        if (token.Contains('\n'))
            throw new ArgumentException(String.Format("Unexpected end-of-line within a token. Used with text \"{0}\".", context));
        if (token.IndexOf(TokenOpen, TokenOpen.Length, StringComparison.InvariantCultureIgnoreCase) != -1)
            throw new ArgumentException(String.Format("Next token is opened before a previous token was closed in token \"{0}\". Used with text \"{1}\".", token, context));
    }

    public override String ToString()
    {
        var totalTextLength = RootSnippet.GetLength();

        var sb = new StringBuilder(totalTextLength);
        RootSnippet.ToString(sb);
        if (sb.Length != totalTextLength)
            throw new InvalidOperationException(String.Format(
                "Internal error: Calculated total text length ({0}) is different from actual ({1}).",
                totalTextLength, sb.Length));
        return sb.ToString();
    }
}