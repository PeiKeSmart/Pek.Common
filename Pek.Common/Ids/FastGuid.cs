#if NETSTANDARD2_1_OR_GREATER || NET8_0_OR_GREATER

namespace Pek.Ids;

public sealed class FastGuid
{
    // Base32 encoding - in ascii sort order for easy text based sorting
    private static readonly Char[] s_encode32Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUV".ToCharArray();
    // Global ID
    private static Int64 NextId = InitializeNextId();

    // Instance components
    private String? _idString;

    internal Int64 IdValue { get; }

    public String IdString
    {
        get
        {
            if (_idString == null)
            {
                _idString = GenerateGuidString(this);
            }
            return _idString;
        }
    }

    // Static constructor to initialize global components
    private static Int64 InitializeNextId()
    {
        var guidBytes = Guid.NewGuid().ToByteArray();

        // Use the first 4 bytes from the Guid to initialize global ID
        return
            guidBytes[0] << 32 |
            guidBytes[1] << 40 |
            guidBytes[2] << 48 |
            guidBytes[3] << 56;
    }

    internal FastGuid(long id)
    {
        IdValue = id;
    }

    public static FastGuid NewGuid()
    {
        return new FastGuid(Interlocked.Increment(ref NextId));
    }

    private static String GenerateGuidString(FastGuid guid)
    {
        return String.Create(13, guid.IdValue, (buffer, value) =>
        {
            var encode32Chars = s_encode32Chars;
            buffer[12] = encode32Chars[value & 31];
            buffer[11] = encode32Chars[(value >> 5) & 31];
            buffer[10] = encode32Chars[(value >> 10) & 31];
            buffer[9] = encode32Chars[(value >> 15) & 31];
            buffer[8] = encode32Chars[(value >> 20) & 31];
            buffer[7] = encode32Chars[(value >> 25) & 31];
            buffer[6] = encode32Chars[(value >> 30) & 31];
            buffer[5] = encode32Chars[(value >> 35) & 31];
            buffer[4] = encode32Chars[(value >> 40) & 31];
            buffer[3] = encode32Chars[(value >> 45) & 31];
            buffer[2] = encode32Chars[(value >> 50) & 31];
            buffer[1] = encode32Chars[(value >> 55) & 31];
            buffer[0] = encode32Chars[(value >> 60) & 31];
        });
    }
}
#endif