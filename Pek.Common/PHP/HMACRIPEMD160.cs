using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Security.Cryptography;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace System.Security.Cryptography
{
    [ComVisible(true)]
    public class HMACRIPEMD160 : IDisposable
    {
        public const int BlockSize = 64;
        public const int HashSizeBytes = 20;

    private readonly byte[] _opad;
    private readonly byte[] _ipad;
    private readonly RIPEMD160Managed _inner; // 复用
    private readonly RIPEMD160Managed _outer; // 复用
    private bool _disposed;
    // Streaming state
    private bool _streamingActive;
    private readonly RIPEMD160Managed _streamInner;
    private readonly RIPEMD160Managed _streamOuter;
    private byte[]? _streamInnerDigestBuffer; // cached 20 bytes

        public HMACRIPEMD160() : this(GenerateRandomKey()) { }

        public HMACRIPEMD160(byte[] key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            _inner = new RIPEMD160Managed();
            _outer = new RIPEMD160Managed();
            _streamInner = new RIPEMD160Managed();
            _streamOuter = new RIPEMD160Managed();
            if (key.Length > BlockSize)
            {
                key = _inner.ComputeHash(key); // 先压缩
            }
            var k0 = new byte[BlockSize];
            Buffer.BlockCopy(key, 0, k0, 0, key.Length);
            _ipad = new byte[BlockSize];
            _opad = new byte[BlockSize];
            for (int i = 0; i < BlockSize; i++)
            {
                _ipad[i] = (byte)(k0[i] ^ 0x36);
                _opad[i] = (byte)(k0[i] ^ 0x5c);
            }
        }

        public byte[] ComputeRaw(byte[] data)
        {
            Ensure();
            if (data == null) throw new ArgumentNullException(nameof(data));
            Span<byte> innerDigest = stackalloc byte[HashSizeBytes];
            _inner.Initialize();
            _inner.Append(_ipad);
            _inner.Append(data);
            _inner.Finish(innerDigest, out _);

            Span<byte> final = stackalloc byte[HashSizeBytes];
            _outer.Initialize();
            _outer.Append(_opad);
            _outer.Append(innerDigest);
            _outer.Finish(final, out _);
            return final.ToArray();
        }

        public bool TryComputeRaw(ReadOnlySpan<byte> data, Span<byte> destination, out int written)
        {
            Ensure();
            if (destination.Length < HashSizeBytes) { written = 0; return false; }
            _inner.Initialize();
            _inner.Append(_ipad);
            _inner.Append(data);
            Span<byte> inner = stackalloc byte[HashSizeBytes];
            _inner.Finish(inner, out _);

            _outer.Initialize();
            _outer.Append(_opad);
            _outer.Append(inner);
            _outer.Finish(destination, out written);
            return true;
        }

        public bool TryComputeHex(ReadOnlySpan<byte> data, Span<char> destination, out int written)
        {
            if (destination.Length < HashSizeBytes * 2) { written = 0; return false; }
            Span<byte> tmp = stackalloc byte[HashSizeBytes];
            if (!TryComputeRaw(data, tmp, out _)) { written = 0; return false; }
            for (var i = 0; i < tmp.Length; i++)
            {
                var b = tmp[i];
                destination[2 * i] = HexNib(b >> 4);
                destination[2 * i + 1] = HexNib(b & 0xF);
            }
            written = HashSizeBytes * 2;
            return true;
        }

        public static bool TryParseHex(ReadOnlySpan<char> hex, Span<byte> destination, out int written)
        {
            if ((hex.Length & 1) != 0) { written = 0; return false; }
            var byteLen = hex.Length / 2;
            if (destination.Length < byteLen) { written = 0; return false; }
            for (int i = 0; i < byteLen; i++)
            {
                var hi = FromHex(hex[2 * i]);
                var lo = FromHex(hex[2 * i + 1]);
                if (hi < 0 || lo < 0) { written = 0; return false; }
                destination[i] = (byte)((hi << 4) | lo);
            }
            written = byteLen;
            return true;
        }

        private static int FromHex(char c)
        {
            if (c >= '0' && c <= '9') return c - '0';
            if (c >= 'a' && c <= 'f') return c - 'a' + 10;
            if (c >= 'A' && c <= 'F') return c - 'A' + 10;
            return -1;
        }

        // Streaming API
        public void Begin()
        {
            Ensure();
            if (_streamingActive) throw new InvalidOperationException("Streaming already active. Call FinishRaw/FinishHex first.");
            _streamingActive = true;
            _streamInner.Initialize();
            _streamInner.Append(_ipad);
        }

        public void Append(ReadOnlySpan<byte> data)
        {
            Ensure();
            if (!_streamingActive) throw new InvalidOperationException("Call Begin() before Append().");
            if (!data.IsEmpty)
            {
                _streamInner.Append(data);
            }
        }

        public bool FinishRaw(Span<byte> destination, out int written)
        {
            Ensure();
            if (!_streamingActive) throw new InvalidOperationException("Streaming not active.");
            if (destination.Length < HashSizeBytes) { written = 0; return false; }
            Span<byte> inner = stackalloc byte[HashSizeBytes];
            _streamInner.Finish(inner, out _);
            _streamOuter.Initialize();
            _streamOuter.Append(_opad);
            _streamOuter.Append(inner);
            if (!_streamOuter.Finish(destination, out written)) return false;
            _streamingActive = false;
            return true;
        }

        public bool FinishHex(Span<char> destination, out int written)
        {
            if (destination.Length < HashSizeBytes * 2) { written = 0; return false; }
            Span<byte> raw = stackalloc byte[HashSizeBytes];
            if (!FinishRaw(raw, out _)) { written = 0; return false; }
            for (var i = 0; i < raw.Length; i++)
            {
                var b = raw[i];
                destination[2 * i] = HexNib(b >> 4);
                destination[2 * i + 1] = HexNib(b & 0xF);
            }
            written = HashSizeBytes * 2;
            return true;
        }

        public void Append(Stream stream, int bufferSize = 81920)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (!_streamingActive) throw new InvalidOperationException("Call Begin() before Append(stream)");
            if (bufferSize <= 0) bufferSize = 4096;
            var rent = ArrayPool<byte>.Shared.Rent(bufferSize);
            try
            {
                int read;
                while ((read = stream.Read(rent, 0, bufferSize)) > 0)
                {
                    _streamInner.Append(rent.AsSpan(0, read));
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(rent, clearArray: true);
            }
        }

        public async ValueTask AppendAsync(Stream stream, int bufferSize = 81920, CancellationToken cancellationToken = default)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (!_streamingActive) throw new InvalidOperationException("Call Begin() before AppendAsync(stream)");
            if (bufferSize <= 0) bufferSize = 4096;
            var rent = ArrayPool<byte>.Shared.Rent(bufferSize);
            try
            {
                int read;
#if NETSTANDARD2_0 || NET461 || NET462 || NET472
                // 同步回退
                while ((read = stream.Read(rent, 0, bufferSize)) > 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    _streamInner.Append(rent.AsSpan(0, read));
                }
#else
                while ((read = await stream.ReadAsync(rent.AsMemory(0, bufferSize), cancellationToken).ConfigureAwait(false)) > 0)
                {
                    _streamInner.Append(rent.AsSpan(0, read));
                }
#endif
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(rent, clearArray: true);
            }
        }

        // Verification helpers
        public static bool VerifyHex(string expectedHex, byte[] data, byte[] key, Encoding? encoding = null)
        {
            if (expectedHex == null) throw new ArgumentNullException(nameof(expectedHex));
            var chars = expectedHex.AsSpan();
            Span<byte> expected = stackalloc byte[HashSizeBytes];
            if (chars.Length != HashSizeBytes * 2) return false;
            if (!TryParseHex(chars, expected, out var w) || w != HashSizeBytes) return false;
            Span<byte> actual = stackalloc byte[HashSizeBytes];
            using var h = new HMACRIPEMD160(key);
            if (!h.TryComputeRaw(data, actual, out _)) return false;
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }

        public static bool VerifyHex(string expectedHex, string data, string key, Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;
            return VerifyHex(expectedHex, encoding.GetBytes(data), encoding.GetBytes(key), encoding);
        }

        // Raw verification (constant time)
        public static bool VerifyRaw(ReadOnlySpan<byte> expectedRaw, byte[] data, byte[] key)
        {
            if (expectedRaw.Length != HashSizeBytes) return false;
            Span<byte> actual = stackalloc byte[HashSizeBytes];
            using var h = new HMACRIPEMD160(key);
            if (!h.TryComputeRaw(data, actual, out _)) return false;
            return CryptographicOperations.FixedTimeEquals(actual, expectedRaw);
        }
        public static bool VerifyRaw(ReadOnlySpan<byte> expectedRaw, ReadOnlySpan<byte> data, ReadOnlySpan<byte> key)
        {
            if (expectedRaw.Length != HashSizeBytes) return false;
            Span<byte> actual = stackalloc byte[HashSizeBytes];
            using var h = new HMACRIPEMD160(key.ToArray());
            if (!h.TryComputeRaw(data, actual, out _)) return false;
            return CryptographicOperations.FixedTimeEquals(actual, expectedRaw);
        }
        public static bool VerifyRaw(byte[] expectedRaw, string data, string key, Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;
            return VerifyRaw(expectedRaw, encoding.GetBytes(data), encoding.GetBytes(key));
        }
        public static bool VerifyRaw(byte[] expectedRaw, byte[] data, byte[] key)
            => VerifyRaw(expectedRaw.AsSpan(), data, key);

        // Async enumerable streaming Append
        public async ValueTask AppendAsync(IAsyncEnumerable<ReadOnlyMemory<byte>> source, CancellationToken cancellationToken = default)
        {
            if (!_streamingActive) throw new InvalidOperationException("Call Begin() before AppendAsync(enumerable)");
            await foreach (var chunk in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                if (!chunk.IsEmpty)
                {
                    _streamInner.Append(chunk.Span);
                }
            }
        }
        public string ComputeHex(byte[] data) => ToLowerHex(ComputeRaw(data));

        public static byte[] Compute(byte[] data, byte[] key) { using var h = new HMACRIPEMD160(key); return h.ComputeRaw(data); }
        public static string ComputeHex(byte[] data, byte[] key) => ToLowerHex(Compute(data, key));
        public static string ComputeHex(string data, string key, Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8; return ComputeHex(encoding.GetBytes(data), encoding.GetBytes(key));
        }
        public static byte[] ComputeRaw(string data, string key, Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8; return Compute(encoding.GetBytes(data), encoding.GetBytes(key));
        }

        private static byte[] GenerateRandomKey()
        {
            var key = new byte[BlockSize];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(key);
            return key;
        }

        // 由于已改为增量 Append，不再需要 Concat

        private static string ToLowerHex(byte[] bytes)
        {
            var c = new char[bytes.Length * 2]; int i = 0; foreach (var b in bytes) { c[i++] = HexNib(b >> 4); c[i++] = HexNib(b & 0xF); } return new string(c);
        }
        private static char HexNib(int v) => (char)(v < 10 ? '0' + v : 'a' + v - 10);

        private void Ensure() { if (_disposed) throw new ObjectDisposedException(nameof(HMACRIPEMD160)); }
    public void Dispose() { if (!_disposed){ _disposed = true; CryptographicOperations.ZeroMemory(_ipad); CryptographicOperations.ZeroMemory(_opad);} }
    }
}