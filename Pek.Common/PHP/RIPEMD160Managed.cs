using System;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;

namespace System.Security.Cryptography
{
    /// <summary>
    /// 纯托管 RIPEMD160 实现（精简版）。
    /// 说明：本实现依据 RIPEMD160 规范；为了篇幅精简，此版本未做极限性能优化。
    /// 可用于 HMAC-RIPEMD160 兼容 PHP。
    /// </summary>
    internal sealed class RIPEMD160Managed : HashAlgorithm
    {
        private const int BlockSizeBytes = 64; // 512 bit
        private readonly uint[] _state = new uint[5];
        private readonly uint[] _bufferWords = new uint[16];
        private readonly byte[] _buffer = new byte[BlockSizeBytes];
        private long _count; // total bytes
        private int _bufferFilled;

        public RIPEMD160Managed()
        {
            HashSizeValue = 160;
            Initialize();
        }

        public override void Initialize()
        {
            _count = 0;
            _bufferFilled = 0;
            // Initial magic constants
            _state[0] = 0x67452301;
            _state[1] = 0xEFCDAB89;
            _state[2] = 0x98BADCFE;
            _state[3] = 0x10325476;
            _state[4] = 0xC3D2E1F0;
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            var offset = ibStart;
            var remaining = cbSize;
            _count += cbSize;
            while (remaining > 0)
            {
                var toCopy = Math.Min(remaining, BlockSizeBytes - _bufferFilled);
                Buffer.BlockCopy(array, offset, _buffer, _bufferFilled, toCopy);
                _bufferFilled += toCopy;
                offset += toCopy;
                remaining -= toCopy;
                if (_bufferFilled == BlockSizeBytes)
                {
                    ProcessBlock(_buffer, 0);
                    _bufferFilled = 0;
                }
            }
        }

        public void Append(byte[] data, int offset, int count)
        {
            HashCore(data, offset, count);
        }

        public void Append(ReadOnlySpan<byte> data)
        {
            // 快速路径：如果有剩余填充块
            var span = data;
            while (!span.IsEmpty)
            {
                var toCopy = Math.Min(span.Length, BlockSizeBytes - _bufferFilled);
                span.Slice(0, toCopy).CopyTo(_buffer.AsSpan(_bufferFilled));
                _bufferFilled += toCopy;
                _count += toCopy;
                span = span.Slice(toCopy);
                if (_bufferFilled == BlockSizeBytes)
                {
                    ProcessBlock(_buffer, 0);
                    _bufferFilled = 0;
                }
            }
        }

        public bool Finish(Span<byte> destination, out int written)
        {
            if (destination.Length < 20) { written = 0; return false; }
            var hash = HashFinal();
            hash.AsSpan().CopyTo(destination);
            written = 20;
            Initialize();
            return true;
        }

        public byte[] FinishAndGetHash()
        {
            var final = HashFinal();
            // Reset for potential reuse
            Initialize();
            return final;
        }

        protected override byte[] HashFinal()
        {
            // Padding
            var paddingLength = (int)((_count % BlockSizeBytes) < 56 ? (56 - (_count % BlockSizeBytes)) : (56 + (64 - (_count % BlockSizeBytes))));
            var padding = new byte[paddingLength + 8];
            padding[0] = 0x80;
            var bitLength = _count * 8;
            for (var i = 0; i < 8; i++)
            {
                padding[paddingLength + i] = (byte)(bitLength >> (8 * i));
            }
            HashCore(padding, 0, padding.Length);

            var result = new byte[20];
            for (var i = 0; i < 5; i++)
            {
                var v = _state[i];
                result[4 * i] = (byte)(v & 0xFF);
                result[4 * i + 1] = (byte)((v >> 8) & 0xFF);
                result[4 * i + 2] = (byte)((v >> 16) & 0xFF);
                result[4 * i + 3] = (byte)((v >> 24) & 0xFF);
            }
            return result;
        }

        private static uint ROL(uint x, int n) => (x << n) | (x >> (32 - n));

        private static uint F(int j, uint x, uint y, uint z)
        {
            if (j < 16) return x ^ y ^ z;
            if (j < 32) return (x & y) | (~x & z);
            if (j < 48) return (x | ~y) ^ z;
            if (j < 64) return (x & z) | (y & ~z);
            return x ^ (y | ~z);
        }

        private static readonly int[] R0 = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
        private static readonly int[] R1 = { 7, 4, 13, 1, 10, 6, 15, 3, 12, 0, 9, 5, 2, 14, 11, 8 };
        private static readonly int[] R2 = { 3, 10, 14, 4, 9, 15, 8, 1, 2, 7, 0, 6, 13, 11, 5, 12 };
        private static readonly int[] R3 = { 1, 9, 11, 10, 0, 8, 12, 4, 13, 3, 7, 15, 14, 5, 6, 2 };
        private static readonly int[] R4 = { 4, 0, 5, 9, 7, 12, 2, 10, 14, 1, 3, 8, 11, 6, 15, 13 };
        private static readonly int[] S0 = { 11, 14, 15, 12, 5, 8, 7, 9, 11, 13, 14, 15, 6, 7, 9, 8 };
        private static readonly int[] S1 = { 7, 6, 8, 13, 11, 9, 7, 15, 7, 12, 15, 9, 11, 7, 13, 12 };
        private static readonly int[] S2 = { 11, 13, 6, 7, 14, 9, 13, 15, 14, 8, 13, 6, 5, 12, 7, 5 };
        private static readonly int[] S3 = { 11, 12, 14, 15, 14, 15, 9, 8, 9, 14, 5, 6, 8, 6, 5, 12 };
        private static readonly int[] S4 = { 9, 15, 5, 11, 6, 8, 13, 12, 5, 12, 13, 14, 11, 8, 5, 6 };
        private static readonly int[] Rp0 = { 5, 14, 7, 0, 9, 2, 11, 4, 13, 6, 15, 8, 1, 10, 3, 12 };
        private static readonly int[] Rp1 = { 6, 11, 3, 7, 0, 13, 5, 10, 14, 15, 8, 12, 4, 9, 1, 2 };
        private static readonly int[] Rp2 = { 15, 5, 1, 3, 7, 14, 6, 9, 11, 8, 12, 2, 10, 0, 4, 13 };
        private static readonly int[] Rp3 = { 8, 6, 4, 1, 3, 11, 15, 0, 5, 12, 2, 13, 9, 7, 10, 14 };
        private static readonly int[] Rp4 = { 12, 15, 10, 4, 1, 5, 8, 7, 6, 2, 13, 14, 0, 3, 9, 11 };
        private static readonly int[] Sp0 = { 8, 9, 9, 11, 13, 15, 15, 5, 7, 7, 8, 11, 14, 14, 12, 6 };
        private static readonly int[] Sp1 = { 9, 13, 15, 7, 12, 8, 9, 11, 7, 7, 12, 7, 6, 15, 13, 11 };
        private static readonly int[] Sp2 = { 9, 7, 15, 11, 8, 6, 6, 14, 12, 13, 5, 14, 13, 13, 7, 5 };
        private static readonly int[] Sp3 = { 15, 5, 8, 11, 14, 14, 6, 14, 6, 9, 12, 9, 12, 5, 15, 8 };
        private static readonly int[] Sp4 = { 8, 5, 12, 9, 12, 5, 14, 6, 8, 13, 6, 5, 15, 13, 11, 11 };

        private static int R(int j) => j < 16 ? R0[j] : j < 32 ? R1[j - 16] : j < 48 ? R2[j - 32] : j < 64 ? R3[j - 48] : R4[j - 64];
        private static int S(int j) => j < 16 ? S0[j] : j < 32 ? S1[j - 16] : j < 48 ? S2[j - 32] : j < 64 ? S3[j - 48] : S4[j - 64];
        private static int Rp(int j) => j < 16 ? Rp0[j] : j < 32 ? Rp1[j - 16] : j < 48 ? Rp2[j - 32] : j < 64 ? Rp3[j - 48] : Rp4[j - 64];
        private static int Sp(int j) => j < 16 ? Sp0[j] : j < 32 ? Sp1[j - 16] : j < 48 ? Sp2[j - 32] : j < 64 ? Sp3[j - 48] : Sp4[j - 64];

        private static uint K(int j) => j switch
        {
            < 16 => 0x00000000U,
            < 32 => 0x5A827999U,
            < 48 => 0x6ED9EBA1U,
            < 64 => 0x8F1BBCDCU,
            _ => 0xA953FD4EU
        };

        private static uint Kp(int j) => j switch
        {
            < 16 => 0x50A28BE6U,
            < 32 => 0x5C4DD124U,
            < 48 => 0x6D703EF3U,
            < 64 => 0x7A6D76E9U,
            _ => 0x00000000U
        };

        private void ProcessBlock(byte[] input, int offset)
        {
            for (var i = 0; i < 16; i++)
            {
                _bufferWords[i] = (uint)(input[offset + 4 * i] | (input[offset + 4 * i + 1] << 8) | (input[offset + 4 * i + 2] << 16) | (input[offset + 4 * i + 3] << 24));
            }
            uint a = _state[0], b = _state[1], c = _state[2], d = _state[3], e = _state[4];
            uint ap = a, bp = b, cp = c, dp = d, ep = e;

            for (var j = 0; j < 80; j++)
            {
                var t = ROL(a + F(j, b, c, d) + _bufferWords[R(j)] + K(j), S(j)) + e;
                a = e; e = d; d = ROL(c, 10); c = b; b = t;

                var tp = ROL(ap + F(79 - j, bp, cp, dp) + _bufferWords[Rp(j)] + Kp(j), Sp(j)) + ep;
                ap = ep; ep = dp; dp = ROL(cp, 10); cp = bp; bp = tp;
            }

            var temp = _state[1] + c + dp;
            _state[1] = _state[2] + d + ep;
            _state[2] = _state[3] + e + ap;
            _state[3] = _state[4] + a + bp;
            _state[4] = _state[0] + b + cp;
            _state[0] = temp;
        }
    }
}
