using System;
using System.Collections;
using System.Net;
using System.Threading;
using FluorineFx;
using FluorineFx.AMF3;
using FluorineFx.Messaging.Api.Service;
using FluorineFx.Net;
namespace 麻辣烫新枫之谷登陆器
{
	public class GameServerAccountListApp
    {
		private NetConnection netConnection;

		public void Connect(CookieCollection cookie)
		{
			this.netConnection = new NetConnection();
			this.netConnection.ObjectEncoding = ObjectEncoding.AMF3;
			this.netConnection.NetStatus += this.NetStatus;
			this.netConnection.CookieContainer.Add(new Uri("http://hk.beanfun.com/"), cookie);
			this.netConnection.Connect("http://hk.beanfun.com/Gateway.aspx", new object[0]);
		}

		private void NetStatus(object A_0, NetStatusEventArgs A_1)
		{
			string text = A_1.Info["level"] as string;
			if (text == "error")
			{
				Console.WriteLine("Error: " + A_1.Info["code"]);
			}
			if (text == "status")
			{
				Console.WriteLine("Status: " + A_1.Info["code"]);
			}
		}

		public IList GetServiceAccounts(string service_code, string service_region, CookieCollection cookie)
		{
			if (this.netConnection == null || !this.netConnection.Connected)
			{
				this.Connect(cookie);
			}
			GameServerAccountListApp.Account account = new GameServerAccountListApp.Account(service_code, service_region);
			this.netConnection.Call("BeanFunBlock.GameZone.GetServiceAccounts", account, new object[]
			{
				service_code,
				service_region
			});
			int num = 0;
			while (num < 60 && account.list == null)
			{
				Thread.Sleep(1000);
				num++;
			}
			return account.list;
		}

		public string GetServiceContract(string service_code, string service_region, CookieCollection cookie)
		{
			if (this.netConnection == null || !this.netConnection.Connected)
			{
				this.Connect(cookie);
			}
			GameServerAccountListApp.Contract c = new GameServerAccountListApp.Contract();
			this.netConnection.Call("BeanFunBlock.GameZone.GetServiceContract", c, new object[]
			{
				service_code,
				service_region
			});
			int num = 0;
			while (num < 60 && c.Result == null)
			{
				Thread.Sleep(1000);
				num++;
			}
			return c.Result;
		}

		public ASObject AddServiceAccount(string parent_service_code, string parent_service_region, string service_code, string service_region, string service_account_id, string service_account_display_name, CookieCollection cookie)
		{
			if (this.netConnection == null || !this.netConnection.Connected)
			{
				this.Connect(cookie);
			}
			GameServerAccountListApp.add addname = new GameServerAccountListApp.add();
			this.netConnection.Call("BeanFunBlock.GameZone.AddServiceAccount", addname, new object[]
			{
				parent_service_code,
				parent_service_region,
				service_code,
				service_region,
				service_account_id,
				service_account_display_name,
				""
			});
			int num = 0;
			while (num < 60 && addname.Result == null)
			{
				Thread.Sleep(1000);
				num++;
			}
			return addname.Result;
		}

		public string ChangeServiceAccountDisplayName(string gameCode, string service_account_id, string service_account_display_name, CookieCollection cookie)
		{
			if (this.netConnection == null || !this.netConnection.Connected)
			{
				this.Connect(cookie);
			}
			GameServerAccountListApp.name n = new GameServerAccountListApp.name();
			this.netConnection.Call("BeanFunBlock.GameZone.ChangeServiceAccountDisplayName", n, new object[]
			{
				gameCode,
				service_account_id,
				service_account_display_name
			});
			int num = 0;
			while (num < 60 && n.result == null)
			{
				Thread.Sleep(1000);
				num++;
			}
			return n.result;
		}

		public void UpdateServiceAccount(string service_code, string service_region, MLTClient.ServiceAccount account, CookieCollection cookie)
		{
			if (this.netConnection == null || !this.netConnection.Connected)
			{
				this.Connect(cookie);
			}
			this.netConnection.Call("BeanFunBlock.GameZone.UpdateServiceAccount", new GameServerAccountListApp.Update(), new object[]
			{
				service_code
			});
		}



		private class Account : IPendingServiceCallback
		{
			public Account(string service_code, string service_region)
			{
				this.service_code = service_code;
				this.service_region = service_region;
			}

			public void ResultReceived(IPendingServiceCall call)
			{
				ArrayCollection arrayCollection = (ArrayCollection)call.Result;
				this.list = arrayCollection.List;
			}

			private string service_code { get;  set; }

			private string service_region { get;  set; }

			// Token: 0x04000073 RID: 115
			public IList list { get; protected set; }
		}

		private class Contract : IPendingServiceCallback
		{
			// Token: 0x060000A7 RID: 167 RVA: 0x00004C19 File Offset: 0x00002E19
			public void ResultReceived(IPendingServiceCall call)
			{
				this.Result = (string)call.Result;
			}

			// Token: 0x04000074 RID: 116
			public string Result;
		}

		private class add : IPendingServiceCallback
		{
			// Token: 0x060000A9 RID: 169 RVA: 0x00004C34 File Offset: 0x00002E34
			public void ResultReceived(IPendingServiceCall call)
			{
				this.Result = (ASObject)call.Result;
			}

			// Token: 0x04000075 RID: 117
			public ASObject Result;
		}

		private class name : IPendingServiceCallback
		{
			public void ResultReceived(IPendingServiceCall call)
			{
				this.result = (string)call.Result;
			}

			public string result;
		}

		private class Update : IPendingServiceCallback
		{
			// Token: 0x060000AD RID: 173 RVA: 0x00004C6A File Offset: 0x00002E6A
			public void ResultReceived(IPendingServiceCall call)
			{
				object result = call.Result;
			}
		}
	}
}
