using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Blam.Utilities
{
    public static class AesHelper
    {
        public static byte[] DecryptAes(byte[] source, string key)
        {
            return DecryptAes(source, key, Encoding.UTF8);
        }

        public static byte[] DecryptAes(byte[] source, string key, Encoding keyEncoding)
        {
            return DecryptAes(source, keyEncoding.GetBytes(key));
        }

        public static byte[] DecryptAes(byte[] source, byte[] key)
        {
            if (source.Length % 16 > 0)
                Array.Resize(ref source, source.Length + 16 - source.Length % 16);

            var xor = key.Select(b => (byte)(b ^ 0xFFA5)).ToArray();
            var iv = xor.Select(b => (byte)(b ^ 0x3C)).ToArray();
            var aes = new AesManaged
            {
                Mode = CipherMode.CBC,
                Key = xor,
                IV = iv,
                Padding = PaddingMode.Zeros
            };

            return aes.CreateDecryptor().TransformFinalBlock(source, 0, source.Length);
        }

        public static byte[] ReadAesBytes(this BinaryReader reader, int count, string key)
        {
            if (count % 16 > 0) count += 16 - count % 16;
            return DecryptAes(reader.ReadBytes(count), key);
        }
    }
}
