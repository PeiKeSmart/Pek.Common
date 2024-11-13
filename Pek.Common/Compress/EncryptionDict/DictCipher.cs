using System.IO.Compression;
using System.Text;

namespace Pek.Compress.EncryptionDict;

public class DictCipher
{
    private readonly Dictionary<char, char> _encryptionDict;
    private readonly Dictionary<char, char> _decryptionDict;

    public DictCipher(Dictionary<char, char> encryptionDict, Dictionary<char, char> decryptionDict)
    {
        if (encryptionDict == null)
        {
            // 定义加密字典，包括字母、数字和符号
            encryptionDict = new Dictionary<char, char>
        {
            {'A', 'X'}, {'B', 'Y'}, {'C', 'Z'}, {'D', 'A'},
            {'E', 'B'}, {'F', 'C'}, {'G', 'D'}, {'H', 'E'},
            {'I', 'F'}, {'J', 'G'}, {'K', 'H'}, {'L', 'I'},
            {'M', 'J'}, {'N', 'K'}, {'O', 'L'}, {'P', 'M'},
            {'Q', 'N'}, {'R', 'O'}, {'S', 'P'}, {'T', 'Q'},
            {'U', 'R'}, {'V', 'S'}, {'W', 'T'}, {'X', 'U'},
            {'Y', 'V'}, {'Z', 'W'}, {' ', '_'},
            {'0', '9'}, {'1', '8'}, {'2', '7'}, {'3', '6'},
            {'4', '5'}, {'5', '4'}, {'6', '3'}, {'7', '2'},
            {'8', '1'}, {'9', '0'},
            {'!', '@'}, {'@', '#'}, {'#', '$'}, {'$', '%'},
            {'%', '^'}, {'^', '&'}, {'&', '*'}, {'*', '('},
            {'(', ')'}, {')', '-'}, {'-', '='}, {'=', '+'}
        };
            // 创建解密字典
            decryptionDict = encryptionDict.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
        }

        _encryptionDict = encryptionDict;
        _decryptionDict = decryptionDict;
    }

    public string Encrypt(string plaintext)
    {
        return new string(plaintext.Select(c => _encryptionDict.ContainsKey(c) ? _encryptionDict[c] : c).ToArray());
    }

    public byte[] Compress(string data)
    {
        var bytes = Encoding.UTF8.GetBytes(data);
        using (var ms = new MemoryStream())
        using (var gzip = new GZipStream(ms, CompressionMode.Compress))
        {
            gzip.Write(bytes, 0, bytes.Length);
            gzip.Close();
            return ms.ToArray();
        }
    }

    public string Decrypt(string ciphertext)
    {
        return new string(ciphertext.Select(c => _decryptionDict.ContainsKey(c) ? _decryptionDict[c] : c).ToArray());
    }
}
