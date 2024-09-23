using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace G_Mobile_Android_WMS
{
    public static class Encryption
    {
        private const string PublicKey =
        "MIGeMA0GCSqGSIb3DQEBAQUAA4GMADCBiAKBgF5C2tffNhghgyv4LeIUzKoqXSs/" +
        "QzcCLtLMAfnhFPWjQIRhs+8f9hQOPzpqfkz/WKHdqTOajTuV7cozWrZT733i5Rdk" +
        "Bkp7xgNyMOy+lYx2SqaVUrCs/4aw9AuTmnvWOOAvvpfy0T806yH0Q1FZ3T1Oan2p" +
        "cOFVQBWs2aKC3YZFAgMBAAE=";

        public static string RSAEncrypt(string DataToEncrypt)
        {
            try
            {
                string encryptedData;

                using (RSA RSA = CreateRsaProviderFromPublicKey(PublicKey))
                {
                    RSACryptoServiceProvider RSAC = new RSACryptoServiceProvider();
                    RSAC.ImportParameters(RSA.ExportParameters(false));
                    encryptedData = Convert.ToBase64String(RSAC.Encrypt(Encoding.UTF8.GetBytes(DataToEncrypt), false));
                }

                return encryptedData;
            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        private static RSA CreateRsaProviderFromPrivateKey(string privateKey)
        {
            var privateKeyBits = Convert.FromBase64String(privateKey);

            var rsa = System.Security.Cryptography.RSA.Create();
            var rsaParameters = new RSAParameters();

            using (BinaryReader binr = new BinaryReader(new MemoryStream(privateKeyBits)))
            {
                byte bt = 0;
                ushort twobytes = 0;
                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130)
                    binr.ReadByte();
                else if (twobytes == 0x8230)
                    binr.ReadInt16();
                else
                    throw new Exception("Unexpected value read binr.ReadUInt16()");

                twobytes = binr.ReadUInt16();
                if (twobytes != 0x0102)
                    throw new Exception("Unexpected version");

                bt = binr.ReadByte();
                if (bt != 0x00)
                    throw new Exception("Unexpected value read binr.ReadByte()");

                rsaParameters.Modulus = binr.ReadBytes(GetIntegerSize(binr));
                rsaParameters.Exponent = binr.ReadBytes(GetIntegerSize(binr));
                rsaParameters.D = binr.ReadBytes(GetIntegerSize(binr));
                rsaParameters.P = binr.ReadBytes(GetIntegerSize(binr));
                rsaParameters.Q = binr.ReadBytes(GetIntegerSize(binr));
                rsaParameters.DP = binr.ReadBytes(GetIntegerSize(binr));
                rsaParameters.DQ = binr.ReadBytes(GetIntegerSize(binr));
                rsaParameters.InverseQ = binr.ReadBytes(GetIntegerSize(binr));
            }

            rsa.ImportParameters(rsaParameters);
            return rsa;
        }

        private static RSA CreateRsaProviderFromPublicKey(string publicKeyString)
        {
            // encoded OID sequence for  PKCS #1 rsaEncryption szOID_RSA_RSA = "1.2.840.113549.1.1.1"
            byte[] seqOid = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
            byte[] seq;

            var x509Key = Convert.FromBase64String(publicKeyString);

            // ---------  Set up stream to read the asn.1 encoded SubjectPublicKeyInfo blob  ------
            using (MemoryStream mem = new MemoryStream(x509Key))
            {
                using (BinaryReader binr = new BinaryReader(mem))  //wrap Memory Stream with BinaryReader for easy reading
                {
                    byte bt = 0;
                    ushort twobytes = 0;

                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                        binr.ReadByte();    //advance 1 byte
                    else if (twobytes == 0x8230)
                        binr.ReadInt16();   //advance 2 bytes
                    else
                        return null;

                    seq = binr.ReadBytes(15);       //read the Sequence OID
                    if (!CompareBytearrays(seq, seqOid))    //make sure Sequence for OID is correct
                        return null;

                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8103) //data read as little endian order (actual data order for Bit String is 03 81)
                        binr.ReadByte();    //advance 1 byte
                    else if (twobytes == 0x8203)
                        binr.ReadInt16();   //advance 2 bytes
                    else
                        return null;

                    bt = binr.ReadByte();
                    if (bt != 0x00)     //expect null byte next
                        return null;

                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                        binr.ReadByte();    //advance 1 byte
                    else if (twobytes == 0x8230)
                        binr.ReadInt16();   //advance 2 bytes
                    else
                        return null;

                    twobytes = binr.ReadUInt16();
                    byte lowbyte = 0x00;
                    byte highbyte = 0x00;

                    if (twobytes == 0x8102) //data read as little endian order (actual data order for Integer is 02 81)
                        lowbyte = binr.ReadByte();  // read next bytes which is bytes in modulus
                    else if (twobytes == 0x8202)
                    {
                        highbyte = binr.ReadByte(); //advance 2 bytes
                        lowbyte = binr.ReadByte();
                    }
                    else
                        return null;
                    byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };   //reverse byte order since asn.1 key uses big endian order
                    int modsize = BitConverter.ToInt32(modint, 0);

                    int firstbyte = binr.PeekChar();
                    if (firstbyte == 0x00)
                    {   //if first byte (highest order) of modulus is zero, don't include it
                        binr.ReadByte();    //skip this null byte
                        modsize -= 1;   //reduce modulus buffer size by 1
                    }

                    byte[] modulus = binr.ReadBytes(modsize);   //read the modulus bytes

                    if (binr.ReadByte() != 0x02)            //expect an Integer for the exponent data
                        return null;
                    int expbytes = (int)binr.ReadByte();        // should only need one byte for actual exponent data (for all useful values)
                    byte[] exponent = binr.ReadBytes(expbytes);

                    // ------- create RSACryptoServiceProvider instance and initialize with public key -----
                    var rsa = System.Security.Cryptography.RSA.Create();
                    RSAParameters rsaKeyInfo = new RSAParameters
                    {
                        Modulus = modulus,
                        Exponent = exponent
                    };
                    rsa.ImportParameters(rsaKeyInfo);

                    return rsa;
                }

            }
        }

        private static int GetIntegerSize(BinaryReader binr)
        {
            byte bt;
            int count;
            bt = binr.ReadByte();
            if (bt != 0x02)
                return 0;
            bt = binr.ReadByte();

            if (bt == 0x81)
                count = binr.ReadByte();
            else
                if (bt == 0x82)
            {
                var highbyte = binr.ReadByte();
                var lowbyte = binr.ReadByte();
                byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                count = BitConverter.ToInt32(modint, 0);
            }
            else
            {
                count = bt;
            }

            while (binr.ReadByte() == 0x00)
            {
                count -= 1;
            }
            binr.BaseStream.Seek(-1, SeekOrigin.Current);
            return count;
        }

        private static bool CompareBytearrays(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;
            int i = 0;
            foreach (byte c in a)
            {
                if (c != b[i])
                    return false;
                i++;
            }
            return true;
        }

        #region AES

        private const string Klucz = "WMSDESKTOP2019NetFramework4.5";

        public static string AESEncrypt(string Data)
        {
            try
            {
                byte[] Bytes = Encoding.Unicode.GetBytes(Data);
                using (Aes Enc = Aes.Create())
                {
                    Rfc2898DeriveBytes PDB = new Rfc2898DeriveBytes(Klucz, new byte[] { 0x02, 0x12, 0x94, 0x11, 0x44, 0x88, 0x01, 0x00, 0xFE, 0xEC, 0xDD, 0x32, 0x95 });
                    Enc.Key = PDB.GetBytes(32);
                    Enc.IV = PDB.GetBytes(16);
                    using (MemoryStream MS = new MemoryStream())
                    {
                        using (CryptoStream CS = new CryptoStream(MS, Enc.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            CS.Write(Bytes, 0, Bytes.Length);
                            CS.Close();
                        }
                        Data = Convert.ToBase64String(MS.ToArray());
                    }
                }
                return Data;
            }
            catch (Exception)
            {
                return "";
            }
        }
        public static string AESDecrypt(string Data)
        {
            try
            {
                Data = Data.Replace(" ", "+");
                byte[] Bytes = Convert.FromBase64String(Data);
                using (Aes Dec = Aes.Create())
                {
                    Rfc2898DeriveBytes PDB = new Rfc2898DeriveBytes(Klucz, new byte[] { 0x02, 0x12, 0x94, 0x11, 0x44, 0x88, 0x01, 0x00, 0xFE, 0xEC, 0xDD, 0x32, 0x95 });
                    Dec.Key = PDB.GetBytes(32);
                    Dec.IV = PDB.GetBytes(16);
                    using (MemoryStream MS = new MemoryStream())
                    {
                        using (CryptoStream CS = new CryptoStream(MS, Dec.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            CS.Write(Bytes, 0, Bytes.Length);
                            CS.Close();
                        }
                        Data = Encoding.Unicode.GetString(MS.ToArray());
                    }
                }
                return Data;
            }
            catch (Exception)
            {
                return "";
            }
        }

        #endregion
    }
}