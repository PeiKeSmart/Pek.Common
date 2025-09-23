using System;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace Pek.Common.Tests.Security
{
    public class HMACRIPEMD160Tests
    {
        [Fact]
        public void HexMatchesManualImplementation()
        {
            var data = "The quick brown fox jumps over the lazy dog";
            var key = "secret";

            var expectedRaw = new HMACRIPEMD160(Encoding.UTF8.GetBytes(key)).ComputeHash(Encoding.UTF8.GetBytes(data));
            var expectedHex = ToLowerHex(expectedRaw);

            var apiHex = HMACRIPEMD160.ComputeHex(data, key);
            var apiRaw = HMACRIPEMD160.ComputeRaw(data, key);

            Assert.Equal(expectedHex, apiHex);
            Assert.Equal(expectedRaw, apiRaw);
            Assert.Equal(20, apiRaw.Length); // 160 bits
        }

        [Fact]
        public void DeterministicForSameInput()
        {
            var data = "测试中文+symbols!*";
            var key = "密钥Key123";

            var h1 = HMACRIPEMD160.ComputeHex(data, key);
            var h2 = HMACRIPEMD160.ComputeHex(data, key);
            Assert.Equal(h1, h2);
        }

        private static string ToLowerHex(byte[] bytes)
        {
            char Get(int v) => (char)(v < 10 ? '0' + v : 'a' + (v - 10));
            var chars = new char[bytes.Length * 2];
            int i = 0;
            foreach (var b in bytes)
            {
                chars[i++] = Get(b >> 4);
                chars[i++] = Get(b & 0xF);
            }
            return new string(chars);
        }
    }
}
