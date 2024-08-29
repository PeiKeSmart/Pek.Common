using System.Text;

namespace Pek.Compress.StringZipper.Util;

public class LzString
{
    private static int GetBaseValue(string alphabet, char character)
    {
        if (!LzString.baseReverseDic.ContainsKey(alphabet))
        {
            LzString.baseReverseDic[alphabet] = new Dictionary<char, int>();
            for (var i = 0; i < alphabet.Length; i++)
            {
                LzString.baseReverseDic[alphabet][alphabet[i]] = i;
            }
        }
        return LzString.baseReverseDic[alphabet][character];
    }

    public static string CompressToBase64(string input)
    {
        if (input == null)
        {
            return string.Empty;
        }
        var res = LzString.Compress(input, 6, (int a) => LzString.keyStrBase64[a]);
        switch (res.Length % 4)
        {
            case 0:
                return res;
            case 1:
                return res + "===";
            case 2:
                return res + "==";
            case 3:
                return res + "=";
            default:
                return null;
        }
    }

    public static string DecompressFromBase64(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }
        return LzString.Decompress(input.Length, 32, (int index) => LzString.GetBaseValue(LzString.keyStrBase64, input[index]));
    }

    public static string CompressToUTF16(string input)
    {
        if (input == null)
        {
            return string.Empty;
        }
        return LzString.Compress(input, 15, (int a) => LzString.f(a + 32)) + " ";
    }

    public static string DecompressFromUTF16(string compressed)
    {
        if (string.IsNullOrWhiteSpace(compressed))
        {
            return string.Empty;
        }
        return LzString.Decompress(compressed.Length, 16384, (int index) => Convert.ToInt32(compressed[index]) - 32);
    }

    public static byte[] CompressToUint8Array(string uncompressed)
    {
        var compressed = LzString.Compress(uncompressed);
        var buf = new byte[compressed.Length * 2];
        var i = 0;
        var TotalLen = compressed.Length;
        while (i < TotalLen)
        {
            var current_value = Convert.ToInt32(compressed[i]);
            buf[i * 2] = (byte)((uint)current_value >> 8);
            buf[i * 2 + 1] = (byte)(current_value % 256);
            i++;
        }
        return buf;
    }

    public static string DecompressFromUint8Array(byte[] compressed)
    {
        if (compressed == null)
        {
            return string.Empty;
        }
        var buf = new int[compressed.Length / 2];
        var i = 0;
        var TotalLen = buf.Length;
        while (i < TotalLen)
        {
            buf[i] = (int)compressed[i * 2] * 256 + (int)compressed[i * 2 + 1];
            i++;
        }
        var result = new char[buf.Length];
        for (var j = 0; j < buf.Length; j++)
        {
            result[j] = LzString.f(buf[j]);
        }
        return LzString.Decompress(new string(result));
    }

    public static string CompressToEncodedURIComponent(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }
        return LzString.Compress(input, 6, (int a) => LzString.keyStrUriSafe[a]);
    }

    public static string DecompressFromEncodedURIComponent(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }
        input = input.Replace(' ', '+');
        return LzString.Decompress(input.Length, 32, (int index) => LzString.GetBaseValue(LzString.keyStrUriSafe, input[index]));
    }

    public static string Compress(string uncompressed)
    {
        return LzString.Compress(uncompressed, 16, LzString.f);
    }

    private static string Compress(string uncompressed, int bitsPerChar, LzString.GetCharFromInt getCharFromInt)
    {
        if (uncompressed == null)
        {
            return string.Empty;
        }
        var context_enlargeIn = 2;
        var context_dictSize = 3;
        var context_numBits = 2;
        var context_data_val = 0;
        var context_data_position = 0;
        var context_dictionaryToCreate = new Dictionary<string, bool>();
        var context_dictionary = new Dictionary<string, int>();
        var context_data = new StringBuilder();
        var context_c = string.Empty;
        var context_wc = string.Empty;
        var context_w = string.Empty;
        int value;
        for (var ii = 0; ii < uncompressed.Length; ii++)
        {
            context_c = uncompressed[ii].ToString();
            if (!context_dictionary.ContainsKey(context_c))
            {
                context_dictionary[context_c] = context_dictSize++;
                context_dictionaryToCreate[context_c] = true;
            }
            context_wc = context_w + context_c;
            if (context_dictionary.ContainsKey(context_wc))
            {
                context_w = context_wc;
            }
            else
            {
                if (context_dictionaryToCreate.ContainsKey(context_w))
                {
                    if (Convert.ToInt32(context_w[0]) < 256)
                    {
                        for (var i = 0; i < context_numBits; i++)
                        {
                            context_data_val <<= 1;
                            if (context_data_position == bitsPerChar - 1)
                            {
                                context_data_position = 0;
                                context_data.Append(getCharFromInt(context_data_val));
                                context_data_val = 0;
                            }
                            else
                            {
                                context_data_position++;
                            }
                        }
                        value = Convert.ToInt32(context_w[0]);
                        for (var i = 0; i < 8; i++)
                        {
                            context_data_val = (context_data_val << 1 | (value & 1));
                            if (context_data_position == bitsPerChar - 1)
                            {
                                context_data_position = 0;
                                context_data.Append(getCharFromInt(context_data_val));
                                context_data_val = 0;
                            }
                            else
                            {
                                context_data_position++;
                            }
                            value >>= 1;
                        }
                    }
                    else
                    {
                        value = 1;
                        for (var i = 0; i < context_numBits; i++)
                        {
                            context_data_val = (context_data_val << 1 | value);
                            if (context_data_position == bitsPerChar - 1)
                            {
                                context_data_position = 0;
                                context_data.Append(getCharFromInt(context_data_val));
                                context_data_val = 0;
                            }
                            else
                            {
                                context_data_position++;
                            }
                            value = 0;
                        }
                        value = Convert.ToInt32(context_w[0]);
                        for (var i = 0; i < 16; i++)
                        {
                            context_data_val = (context_data_val << 1 | (value & 1));
                            if (context_data_position == bitsPerChar - 1)
                            {
                                context_data_position = 0;
                                context_data.Append(getCharFromInt(context_data_val));
                                context_data_val = 0;
                            }
                            else
                            {
                                context_data_position++;
                            }
                            value >>= 1;
                        }
                    }
                    context_enlargeIn--;
                    if (context_enlargeIn == 0)
                    {
                        context_enlargeIn = (int)Math.Pow(2.0, (double)context_numBits);
                        context_numBits++;
                    }
                    context_dictionaryToCreate.Remove(context_w);
                }
                else
                {
                    value = context_dictionary[context_w];
                    for (var i = 0; i < context_numBits; i++)
                    {
                        context_data_val = (context_data_val << 1 | (value & 1));
                        if (context_data_position == bitsPerChar - 1)
                        {
                            context_data_position = 0;
                            context_data.Append(getCharFromInt(context_data_val));
                            context_data_val = 0;
                        }
                        else
                        {
                            context_data_position++;
                        }
                        value >>= 1;
                    }
                }
                context_enlargeIn--;
                if (context_enlargeIn == 0)
                {
                    context_enlargeIn = (int)Math.Pow(2.0, (double)context_numBits);
                    context_numBits++;
                }
                context_dictionary[context_wc] = context_dictSize++;
                context_w = context_c;
            }
        }
        if (context_w != string.Empty)
        {
            if (context_dictionaryToCreate.ContainsKey(context_w))
            {
                if (Convert.ToInt32(context_w[0]) < 256)
                {
                    for (var i = 0; i < context_numBits; i++)
                    {
                        context_data_val <<= 1;
                        if (context_data_position == bitsPerChar - 1)
                        {
                            context_data_position = 0;
                            context_data.Append(getCharFromInt(context_data_val));
                            context_data_val = 0;
                        }
                        else
                        {
                            context_data_position++;
                        }
                    }
                    value = Convert.ToInt32(context_w[0]);
                    for (var i = 0; i < 8; i++)
                    {
                        context_data_val = (context_data_val << 1 | (value & 1));
                        if (context_data_position == bitsPerChar - 1)
                        {
                            context_data_position = 0;
                            context_data.Append(getCharFromInt(context_data_val));
                            context_data_val = 0;
                        }
                        else
                        {
                            context_data_position++;
                        }
                        value >>= 1;
                    }
                }
                else
                {
                    value = 1;
                    for (var i = 0; i < context_numBits; i++)
                    {
                        context_data_val = (context_data_val << 1 | value);
                        if (context_data_position == bitsPerChar - 1)
                        {
                            context_data_position = 0;
                            context_data.Append(getCharFromInt(context_data_val));
                            context_data_val = 0;
                        }
                        else
                        {
                            context_data_position++;
                        }
                        value = 0;
                    }
                    value = Convert.ToInt32(context_w[0]);
                    for (var i = 0; i < 16; i++)
                    {
                        context_data_val = (context_data_val << 1 | (value & 1));
                        if (context_data_position == bitsPerChar - 1)
                        {
                            context_data_position = 0;
                            context_data.Append(getCharFromInt(context_data_val));
                            context_data_val = 0;
                        }
                        else
                        {
                            context_data_position++;
                        }
                        value >>= 1;
                    }
                }
                context_enlargeIn--;
                if (context_enlargeIn == 0)
                {
                    context_enlargeIn = (int)Math.Pow(2.0, (double)context_numBits);
                    context_numBits++;
                }
                context_dictionaryToCreate.Remove(context_w);
            }
            else
            {
                value = context_dictionary[context_w];
                for (var i = 0; i < context_numBits; i++)
                {
                    context_data_val = (context_data_val << 1 | (value & 1));
                    if (context_data_position == bitsPerChar - 1)
                    {
                        context_data_position = 0;
                        context_data.Append(getCharFromInt(context_data_val));
                        context_data_val = 0;
                    }
                    else
                    {
                        context_data_position++;
                    }
                    value >>= 1;
                }
            }
            if (context_enlargeIn - 1 == 0)
            {
                context_enlargeIn = (int)Math.Pow(2.0, (double)context_numBits);
                context_numBits++;
            }
        }
        value = 2;
        for (var i = 0; i < context_numBits; i++)
        {
            context_data_val = (context_data_val << 1 | (value & 1));
            if (context_data_position == bitsPerChar - 1)
            {
                context_data_position = 0;
                context_data.Append(getCharFromInt(context_data_val));
                context_data_val = 0;
            }
            else
            {
                context_data_position++;
            }
            value >>= 1;
        }
        for (; ; )
        {
            context_data_val <<= 1;
            if (context_data_position == bitsPerChar - 1)
            {
                break;
            }
            context_data_position++;
        }
        context_data.Append(getCharFromInt(context_data_val));
        return context_data.ToString();
    }

    public static string Decompress(string compressed)
    {
        if (string.IsNullOrWhiteSpace(compressed))
        {
            return string.Empty;
        }
        return LzString.Decompress(compressed.Length, 32768, (int index) => Convert.ToInt32(compressed[index]));
    }

    private static string Decompress(int length, int resetValue, LzString.GetNextValue getNextValue)
    {
        var dictionary = new Dictionary<int, string>();
        var enlargeIn = 4;
        var dictSize = 4;
        var numBits = 3;
        var c = 0;
        var entry = string.Empty;
        var result = new StringBuilder();
        var data = new LzString.DataStruct
        {
            val = getNextValue(0),
            position = resetValue,
            index = 1
        };
        for (var i = 0; i < 3; i++)
        {
            dictionary[i] = Convert.ToChar(i).ToString();
        }
        var bits = 0;
        var maxpower = (int)Math.Pow(2.0, 2.0);
        for (var power = 1; power != maxpower; power <<= 1)
        {
            var resb = data.val & data.position;
            data.position >>= 1;
            if (data.position == 0)
            {
                data.position = resetValue;
                var index = data.index;
                data.index = index + 1;
                data.val = getNextValue(index);
            }
            bits |= ((resb > 0) ? 1 : 0) * power;
        }
        switch (bits)
        {
            case 0:
                bits = 0;
                maxpower = (int)Math.Pow(2.0, 8.0);
                for (var power = 1; power != maxpower; power <<= 1)
                {
                    var resb = data.val & data.position;
                    data.position >>= 1;
                    if (data.position == 0)
                    {
                        data.position = resetValue;
                        var index2 = data.index;
                        data.index = index2 + 1;
                        data.val = getNextValue(index2);
                    }
                    bits |= ((resb > 0) ? 1 : 0) * power;
                }
                c = Convert.ToInt32(LzString.f(bits));
                break;
            case 1:
                bits = 0;
                maxpower = (int)Math.Pow(2.0, 16.0);
                for (var power = 1; power != maxpower; power <<= 1)
                {
                    var resb = data.val & data.position;
                    data.position >>= 1;
                    if (data.position == 0)
                    {
                        data.position = resetValue;
                        var index2 = data.index;
                        data.index = index2 + 1;
                        data.val = getNextValue(index2);
                    }
                    bits |= ((resb > 0) ? 1 : 0) * power;
                }
                c = Convert.ToInt32(LzString.f(bits));
                break;
            case 2:
                return string.Empty;
        }
        dictionary[3] = Convert.ToChar(c).ToString();
        var w = Convert.ToChar(c).ToString();
        result.Append(Convert.ToChar(c));
        while (data.index <= length)
        {
            bits = 0;
            maxpower = (int)Math.Pow(2.0, (double)numBits);
            for (var power = 1; power != maxpower; power <<= 1)
            {
                var resb = data.val & data.position;
                data.position >>= 1;
                if (data.position == 0)
                {
                    data.position = resetValue;
                    var index = data.index;
                    data.index = index + 1;
                    data.val = getNextValue(index);
                }
                bits |= ((resb > 0) ? 1 : 0) * power;
            }
            switch (c = bits)
            {
                case 0:
                    bits = 0;
                    maxpower = (int)Math.Pow(2.0, 8.0);
                    for (var power = 1; power != maxpower; power <<= 1)
                    {
                        var resb = data.val & data.position;
                        data.position >>= 1;
                        if (data.position == 0)
                        {
                            data.position = resetValue;
                            var index2 = data.index;
                            data.index = index2 + 1;
                            data.val = getNextValue(index2);
                        }
                        bits |= ((resb > 0) ? 1 : 0) * power;
                    }
                    dictionary[dictSize++] = LzString.f(bits).ToString();
                    c = dictSize - 1;
                    enlargeIn--;
                    break;
                case 1:
                    bits = 0;
                    maxpower = (int)Math.Pow(2.0, 16.0);
                    for (var power = 1; power != maxpower; power <<= 1)
                    {
                        var resb = data.val & data.position;
                        data.position >>= 1;
                        if (data.position == 0)
                        {
                            data.position = resetValue;
                            var index2 = data.index;
                            data.index = index2 + 1;
                            data.val = getNextValue(index2);
                        }
                        bits |= ((resb > 0) ? 1 : 0) * power;
                    }
                    dictionary[dictSize++] = LzString.f(bits).ToString();
                    c = dictSize - 1;
                    enlargeIn--;
                    break;
                case 2:
                    return result.ToString();
            }
            if (enlargeIn == 0)
            {
                enlargeIn = (int)Math.Pow(2.0, (double)numBits);
                numBits++;
            }
            if (dictionary.ContainsKey(c))
            {
                entry = dictionary[c];
            }
            else
            {
                if (c != dictSize)
                {
                    return null;
                }
                entry = w + w[0].ToString();
            }
            result.Append(entry);
            dictionary[dictSize++] = w + entry[0].ToString();
            enlargeIn--;
            w = entry;
            if (enlargeIn == 0)
            {
                enlargeIn = (int)Math.Pow(2.0, (double)numBits);
                numBits++;
            }
        }
        return string.Empty;
    }

    private static readonly string keyStrBase64 = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";

    private static readonly string keyStrUriSafe = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-$";

    private static readonly Dictionary<string, Dictionary<char, int>> baseReverseDic = new Dictionary<string, Dictionary<char, int>>();

    private static readonly LzString.GetCharFromInt f = (int a) => Convert.ToChar(a);

    private delegate char GetCharFromInt(int a);

    private delegate int GetNextValue(int index);

    private struct DataStruct
    {
        public int val;

        public int position;

        public int index;
    }
}