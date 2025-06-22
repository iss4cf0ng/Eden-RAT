using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Eden
{
    public class EZCrypto
    {
        public class EZAES
        {
            private Aes m_AES;

            public byte[] m_abKey { get { return m_AES.Key; } }
            public byte[] m_abIV { get { return m_AES.IV; } }

            public EZAES(int nKeySize = 256, int nBlockSize = 128, byte[] abKey = null, byte[] abIV = null)
            {
                m_AES = Aes.Create();

                m_AES.Mode = CipherMode.CBC;
                m_AES.KeySize = nKeySize;
                m_AES.BlockSize = nBlockSize;

                if (abKey != null)
                    m_AES.Key = abKey;
                else
                    m_AES.GenerateKey();

                if (abIV != null)
                    m_AES.IV = abIV;
                else
                    m_AES.GenerateIV();
            }

            public byte[] Encrypt(byte[] abPlain)
            {
                byte[] abCipher = new byte[abPlain.Length];
                using (ICryptoTransform encryptor = m_AES.CreateEncryptor(m_abKey, m_abIV))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            cs.Write(abPlain, 0, abPlain.Length);
                            cs.FlushFinalBlock();
                        }

                        abCipher = ms.ToArray();
                    }
                }

                return abCipher;
            }

            public byte[] Decrypt(byte[] abCipher)
            {
                byte[] abPlain = null;
                using (ICryptoTransform decryptor = m_AES.CreateDecryptor(m_abKey, m_abIV))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
                        {
                            cs.Write(abCipher, 0, abCipher.Length);
                            cs.FlushFinalBlock();
                        }

                        abPlain = ms.ToArray();
                    }
                }

                return abPlain;
            }
        }

        public class EZRSA
        {
            public int m_nKeySize { get; set; }

            public string m_szXmlPublicKey { get; set; }
            public string m_szXmlPrivateKey { get; set; }

            public EZRSA(int nKeySize = 4096)
            {
                m_nKeySize = nKeySize;

                (m_szXmlPublicKey, m_szXmlPrivateKey) = CreateKey();
            }

            public (string szXmlPublicKey, string szXmlPrivateKey) CreateKey()
            {
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                rsa.KeySize = m_nKeySize;

                string szXmlPublicKey = rsa.ToXmlString(false);
                string szXmlPrivateKey = rsa.ToXmlString(true);

                return (szXmlPublicKey, szXmlPrivateKey);
            }

            public byte[] Encrypt(byte[] abData, string szXmlPublicKey = null)
            {
                szXmlPublicKey = szXmlPublicKey ?? m_szXmlPublicKey;

                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                rsa.KeySize = m_nKeySize;
                rsa.FromXmlString(szXmlPublicKey);

                byte[] abCipher = rsa.Encrypt(abData, false);

                return abCipher;
            }

            public byte[] Decrypt(byte[] abData, string szXmlPrivateKey = null)
            {
                szXmlPrivateKey = szXmlPrivateKey ?? m_szXmlPrivateKey;

                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                rsa.KeySize = m_nKeySize;
                rsa.FromXmlString(szXmlPrivateKey);

                byte[] abPlain = rsa.Decrypt(abData, false);

                return abPlain;
            }
        }

        public class Encoder
        {
            public static string stre2b64(string szData)
            {
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(szData));
            }

            public static string b64d2str(string szData)
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(szData));
            }

            public static byte[] b64d2bytes(string szData)
            {
                return Convert.FromBase64String(szData);
            }

            public static string bytese2b64(byte[] abData)
            {
                return Convert.ToBase64String(abData);
            }
        }

        public class Hash
        {
            public static string ComputeSha512(string szInput)
            {
                using (SHA512 sha512 = SHA512.Create())
                {
                    byte[] abSha512Hash = sha512.ComputeHash(Encoding.UTF8.GetBytes(szInput));
                    return BitConverter.ToString(abSha512Hash).Replace("-", string.Empty);
                }
            }
        }
    }
}
