using System;
using System.Security.Cryptography;

namespace Lib_K_Relay.Crypto
{
	class RSACipher
	{

		private string PublicKey = "MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDCKFctVrhfF3m2Kes0FBL/JFeO" +
			"cmNg9eJz8k/hQy1kadD+XFUpluRqa//Uxp2s9W2qE0EoUCu59ugcf/p7lGuL99Uo" +
			"SGmQEynkBvZct+/M40L0E0rZ4BVgzLOJmIbXMp0J4PnPcb6VLZvxazGcmSfjauC7" +
			"F3yWYqUbZd/HCBtawwIDAQAB";
		private static RSAParameters RSAKey;

		public RSACipher()
		{
			var sr = new System.IO.StringReader(PublicKey);
			var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
			RSAKey = (RSAParameters)xs.Deserialize(sr);
		}

		public static string Cipher(string data)
		{
			RSACryptoServiceProvider csp = new RSACryptoServiceProvider();
			csp.ImportParameters(RSAKey);
			byte[] bytesPlainTextData = System.Text.Encoding.Unicode.GetBytes(data);
			var bytesCypherText = csp.Encrypt(bytesPlainTextData, false);
			return Convert.ToBase64String(bytesCypherText);
		}

		public static string Cipher(byte[] data)
		{
			RSACryptoServiceProvider csp = new RSACryptoServiceProvider();
			csp.ImportParameters(RSAKey);
			var bytesCypherText = csp.Encrypt(data, false);
			return Convert.ToBase64String(bytesCypherText);
		}
	}
}