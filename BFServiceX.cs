using BFService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 麻辣烫新枫之谷登陆器
{
	public class BFServiceX 
    {
		private BFServiceXClass bfServiceXClass;
		private string token;

		public BFServiceX()
        {
            this.bfServiceXClass = new BFServiceXClass();
        }

		public uint Initialize2()
		{
			uint result;
			try
			{
				uint num = this.bfServiceXClass.Initialize2("HK;Production", "", "", 0U, "");
				if (num != 0U)
				{
					result = this.Initialize2();
				}
				else
				{
					result = num;
				}
			}
			catch
			{
				result = this.Initialize2();
			}
			return result;
		}

		public string Token
		{
			get
			{
				string result;
				try
				{
					this.bfServiceXClass.SaveData("Seed", "0");
					this.bfServiceXClass.SaveData("Token", this.token);
					string text = this.bfServiceXClass.LoadData("Token");
					if (text == "Failure")
					{
						this.Initialize2();
						result = this.Token;
					}
					else
					{
						result = text;
					}
				}
				catch
				{
					result = this.Token;
				}
				return result;
			}
			set
			{
				this.token = value;
			}
		}
	}
}
