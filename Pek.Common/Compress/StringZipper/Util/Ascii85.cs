using System.Text;

namespace Pek.Compress.StringZipper.Util;

/// <summary>
/// C# implementation of ASCII85 encoding. 
/// Based on C code from http://www.stillhq.com/cgi-bin/cvsweb/ascii85/
/// </summary>
/// <remarks>
/// Jeff Atwood
/// http://www.codinghorror.com/blog/archives/000410.html
/// </remarks>
public class Ascii85
{
    public static byte[] FromAscii85String(string str)
    {
        return new Ascii85().Decode(str);
    }

    public static string ToAscii85String(byte[] data)
    {
        return new Ascii85
        {
            LineLength = 0
        }.Encode(data);
    }

    /// <summary>
    /// Decodes an ASCII85 encoded string into the original binary data
    /// </summary>
    /// <param name="s">ASCII85 encoded string</param>
    /// <returns>byte array of decoded binary data</returns>
    public byte[] Decode(string s)
    {
        if (EnforceMarks && (!s.StartsWith(PrefixMark) | !s.EndsWith(SuffixMark)))
        {
            throw new Exception(string.Concat(new string[]
            {
                    "ASCII85 encoded data should begin with '",
                    PrefixMark,
                    "' and end with '",
                    SuffixMark,
                    "'"
            }));
        }
        if (s.StartsWith(PrefixMark))
        {
            s = s.Substring(PrefixMark.Length);
        }
        if (s.EndsWith(SuffixMark))
        {
            s = s.Substring(0, s.Length - SuffixMark.Length);
        }
        var ms = new MemoryStream();
        var count = 0;
        var text = s;
        var j = 0;
        while (j < text.Length)
        {
            var c = text[j];
            if (c == '\0')
            {
                goto IL_13F;
            }
            bool processChar;
            switch (c)
            {
                case '\b':
                case '\t':
                case '\n':
                case '\f':
                case '\r':
                    goto IL_13F;
                case '\v':
                    break;
                default:
                    if (c == 'z')
                    {
                        if (count != 0)
                        {
                            throw new Exception("The character 'z' is invalid inside an ASCII85 block.");
                        }
                        _decodedBlock[0] = 0;
                        _decodedBlock[1] = 0;
                        _decodedBlock[2] = 0;
                        _decodedBlock[3] = 0;
                        ms.Write(_decodedBlock, 0, _decodedBlock.Length);
                        processChar = false;
                        goto IL_16F;
                    }
                    break;
            }
            if (c < '!' || c > 'u')
            {
                throw new Exception("Bad character '" + c.ToString() + "' found. ASCII85 only allows characters '!' to 'u'.");
            }
            processChar = true;
IL_16F:
            if (processChar)
            {
                _tuple += (uint)(c - '!') * pow85[count];
                count++;
                if (count == _encodedBlock.Length)
                {
                    DecodeBlock();
                    ms.Write(_decodedBlock, 0, _decodedBlock.Length);
                    _tuple = 0U;
                    count = 0;
                }
            }
            j++;
            continue;
IL_13F:
            processChar = false;
            goto IL_16F;
        }
        if (count != 0)
        {
            if (count == 1)
            {
                throw new Exception("The last block of ASCII85 data cannot be a single byte.");
            }
            count--;
            _tuple += pow85[count];
            DecodeBlock(count);
            for (var i = 0; i < count; i++)
            {
                ms.WriteByte(_decodedBlock[i]);
            }
        }
        return ms.ToArray();
    }

    /// <summary>
    /// Encodes binary data into a plaintext ASCII85 format string
    /// </summary>
    /// <param name="ba">binary data to encode</param>
    /// <returns>ASCII85 encoded string</returns>
    public string Encode(byte[] ba)
    {
        var sb = new StringBuilder(ba.Length * (_encodedBlock.Length / _decodedBlock.Length));
        _linePos = 0;
        if (EnforceMarks)
        {
            AppendString(sb, PrefixMark);
        }
        var count = 0;
        _tuple = 0U;
        foreach (var b in ba)
        {
            if (count >= _decodedBlock.Length - 1)
            {
                _tuple |= (uint)b;
                if (_tuple == 0U)
                {
                    AppendChar(sb, 'z');
                }
                else
                {
                    EncodeBlock(sb);
                }
                _tuple = 0U;
                count = 0;
            }
            else
            {
                _tuple |= (uint)((uint)b << 24 - count * 8);
                count++;
            }
        }
        if (count > 0)
        {
            EncodeBlock(count + 1, sb);
        }
        if (EnforceMarks)
        {
            AppendString(sb, SuffixMark);
        }
        return sb.ToString();
    }

    private void EncodeBlock(StringBuilder sb)
    {
        EncodeBlock(_encodedBlock.Length, sb);
    }

    private void EncodeBlock(int count, StringBuilder sb)
    {
        for (var i = _encodedBlock.Length - 1; i >= 0; i--)
        {
            _encodedBlock[i] = (byte)(_tuple % 85U + 33U);
            _tuple /= 85U;
        }
        for (var j = 0; j < count; j++)
        {
            var c = (char)_encodedBlock[j];
            AppendChar(sb, c);
        }
    }

    private void DecodeBlock()
    {
        DecodeBlock(_decodedBlock.Length);
    }

    private void DecodeBlock(int bytes)
    {
        for (var i = 0; i < bytes; i++)
        {
            _decodedBlock[i] = (byte)(_tuple >> 24 - i * 8);
        }
    }

    private void AppendString(StringBuilder sb, string s)
    {
        if (LineLength > 0 && _linePos + s.Length > LineLength)
        {
            _linePos = 0;
            sb.Append('\n');
        }
        else
        {
            _linePos += s.Length;
        }
        sb.Append(s);
    }

    private void AppendChar(StringBuilder sb, char c)
    {
        sb.Append(c);
        _linePos++;
        if (LineLength > 0 && _linePos >= LineLength)
        {
            _linePos = 0;
            sb.Append('\n');
        }
    }

    public string PrefixMark = "<~";

    /// <summary>
    /// Suffix mark that identifies an encoded ASCII85 string, traditionally '~&gt;'
    /// </summary>
    public string SuffixMark = "~>";

    /// <summary>
    /// Maximum line length for encoded ASCII85 string; 
    /// set to zero for one unbroken line.
    /// </summary>
    public int LineLength = 75;

    /// <summary>
    /// Add the Prefix and Suffix marks when encoding, and enforce their presence for decoding
    /// </summary>
    public bool EnforceMarks = true;

    private const int _asciiOffset = 33;

    private byte[] _encodedBlock = new byte[5];

    private byte[] _decodedBlock = new byte[4];

    private uint _tuple;

    private int _linePos;

    private uint[] pow85 = new uint[]
    {
            52200625U,
            614125U,
            7225U,
            85U,
            1U
    };
}