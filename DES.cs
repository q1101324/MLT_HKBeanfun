using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace 麻辣烫新枫之谷登陆器
{
    class DES
    {
		public static string Encrypt(string A_0, string A_1)
		{
			string result;
			try
			{
				DESCryptoServiceProvider descryptoServiceProvider = new DESCryptoServiceProvider();
				descryptoServiceProvider.Mode = CipherMode.ECB;
				descryptoServiceProvider.Padding = PaddingMode.None;
				descryptoServiceProvider.Key = Encoding.ASCII.GetBytes(A_1);
				byte[] bytes = Encoding.ASCII.GetBytes(A_0);
				result = BitConverter.ToString(descryptoServiceProvider.CreateEncryptor().TransformFinalBlock(bytes, 0, bytes.Length)).Replace("-", "");
			}
			catch (Exception ex)
			{
				Console.WriteLine("EncryptDESError:" + ex.Message + "\n" + ex.StackTrace);
				result = null;
			}
			return result;
		}

		// Token: 0x0600004F RID: 79 RVA: 0x000039D0 File Offset: 0x00001BD0
		public static string Decrypt(string A_0, string A_1)
		{
			string result;
			try
			{
				DESCryptoServiceProvider descryptoServiceProvider = new DESCryptoServiceProvider();
				descryptoServiceProvider.Mode = CipherMode.ECB;
				descryptoServiceProvider.Padding = PaddingMode.None;
				descryptoServiceProvider.Key = Encoding.ASCII.GetBytes(A_1);
				byte[] array = new byte[A_0.Length / 2];
				for (int i = 0; i < A_0.Length; i += 2)
				{
					array[i / 2] = Convert.ToByte(A_0.Substring(i, 2), 16);
				}
				ICryptoTransform cryptoTransform = descryptoServiceProvider.CreateDecryptor();
				result = Encoding.ASCII.GetString(cryptoTransform.TransformFinalBlock(array, 0, array.Length));
			}
			catch (Exception ex)
			{
				Console.WriteLine("DecryptDESError:" + ex.Message + "\n" + ex.StackTrace);
				result = null;
			}
			return result;
		}
	}
}
