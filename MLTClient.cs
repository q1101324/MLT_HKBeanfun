
using FluorineFx;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace 麻辣烫新枫之谷登陆器
{
    public partial class MLTClient : WebClient
    {
        #region 声明参数
        private int remainPoint;
        private string skey;
        public string errmsg;
        private string sessionkey;
        private string service_code = "610074";
        private string service_region = "T9";
        private BFServiceX bfServiceX;
        public List<MLTClient.ServiceAccount> accountList;
        public GameServerAccountListApp gameServAccListApp;
        private CookieContainer cookieContainer;
        public string accountAmountLimitNotice;
        #endregion

        //初始化
        public MLTClient()
        {
            this.AllowAutoRedirect = true;
            this.ResponseUri = null;
            this.cookieContainer = new CookieContainer();
            base.Headers.Set("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36");
            base.Headers.Set("Accept-Encoding", "identity");
            base.Encoding = Encoding.UTF8;
            this.errmsg = null;
            this.accountList = new List<MLTClient.ServiceAccount>();
            this.accountAmountLimitNotice = "";
            InitializeComponent();
        }

        #region  登陆
        //界面登陆
        public void Login(string id, string pass, int loginMethod, string service_code = "610074", string service_region = "T9")
        {
            this.skey = null;
            try
            {

                if (this.bfServiceX == null)
                {
                    try
                    {
                        this.bfServiceX = new BFServiceX();
                    }
                    catch (Exception ex)
                    {
                        this.errmsg = "缤放组件初始化错误 ";
                        return;
                    }
                }
                this.bfServiceX.Initialize2();
                if (this.gameServAccListApp == null)
                {
                    this.gameServAccListApp = new GameServerAccountListApp();
                }
                this.sessionkey = this.GetSessionkey();

                string isLogin;
                switch (loginMethod)
                {
                    case 0:
                        isLogin = this.login(id, pass, this.sessionkey);
                        break;

                    default:
                        this.errmsg = "暂时不支持台号登录";
                        return;
                }
                this.getAccount(isLogin, service_code, service_region);
                Ping();
            }
            catch (Exception ex)
            {
                if (ex is WebException)
                {
                    this.errmsg = "网络出现问题。" + ex.Message;
                }
                else
                {
                    this.errmsg = "登入未知错误\n\n" + ex.Message + "\n" + ex.StackTrace;
                }
            }
        }

        //黑菊登陆
        private string login(string user, string pwd, string sesionkey)
        {
            string result;
            try
            {
                string input = this.DownloadString("http://hk.beanfun.com/beanfun_block/login/id-pass_form.aspx?otp1=" + sesionkey + "&seed=0");
                Regex regex = new Regex("id=\"__VIEWSTATE\" value=\"(.*)\" />");
                if (!regex.IsMatch(input))
                {
                    this.errmsg = "sesionkey出错";
                    result = null;
                }
                else
                {
                    string value = regex.Match(input).Groups[1].Value;
                    regex = new Regex("id=\"__EVENTVALIDATION\" value=\"(.*)\" />");
                    if (!regex.IsMatch(input))
                    {
                        this.errmsg = "sesionkey出错";
                        result = null;
                    }
                    else
                    {
                        string value2 = regex.Match(input).Groups[1].Value;
                        regex = new Regex("id=\"__VIEWSTATEGENERATOR\" value=\"(.*)\" />");
                        if (!regex.IsMatch(input))
                        {
                            this.errmsg = "sesionkey出错";
                            result = null;
                        }
                        else
                        {
                            string value3 = regex.Match(input).Groups[1].Value;
                            NameValueCollection nameValueCollection = new NameValueCollection();
                            nameValueCollection.Add("__EVENTTARGET", "");
                            nameValueCollection.Add("__EVENTARGUMENT", "");
                            nameValueCollection.Add("__VIEWSTATE", value);
                            nameValueCollection.Add("__VIEWSTATEGENERATOR", value3);
                            nameValueCollection.Add("__EVENTVALIDATION", value2);
                            nameValueCollection.Add("t_AccountID", user);
                            nameValueCollection.Add("t_Password", pwd);
                            nameValueCollection.Add("btn_login.x", "0");
                            nameValueCollection.Add("btn_login.y", "0");
                            nameValueCollection.Add("recaptcha_response_field", "manual_challenge");
                            input = this.UploadString("http://hk.beanfun.com/beanfun_block/login/id-pass_form.aspx?otp1=" + sesionkey + "&seed=0", nameValueCollection);
                            regex = new Regex("ProcessLoginV2\\((.*)\\);\\\"");
                            if (!regex.IsMatch(input))
                            {
                                this.errmsg = "账号或者密码错误";
                                result = null;
                            }
                            else
                            {
                                string text = regex.Match(input).Groups[1].Value.Replace("\\", "");
                                this.bfServiceX.Token = (string)JObject.Parse(text)["token"];
                                result = "true";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.errmsg = "登录未知错误：\n\n" + ex.Message + "\n" + ex.StackTrace;
                result = null;
            }
            return result;
        }
        #endregion

        #region   帐号操作
        //添加帐号
        public bool AddServiceAccount(string name, string service_code, string service_region)
        {
            if (name == null || name == "")
            {
                return false;
            }
            ASObject asobject = this.gameServAccListApp.AddServiceAccount(null, null, service_code, service_region, null, name,this.GetCookies());
            return asobject != null && asobject["result"] != null && !((string)asobject["result"] != "1");
        }

        //获取OPT
        public string GetOTP(MLTClient.ServiceAccount acc, string service_code = "610074", string service_region = "T9")
        {
            string result;
            try
            {
                string text;

                text = this.DownloadString(string.Concat(new string[]
                {
                        "http://hk.beanfun.com/beanfun_block/auth.aspx?channel=game_zone&page_and_query=game_start_step2.aspx?service_code=",
                        service_code,
                        "%26service_region=",
                        service_region,
                        "%26service_account_sn=",
                        acc.ssn,
                        "&token=",
                        this.bfServiceX.Token
                }));
                if (text == "")
                {
                    this.errmsg = "OTPNoResponse";
                    return null;
                }

                Regex regex2 = new Regex("var MyAccountData = (.*);");
                if (!regex2.IsMatch(text))
                {
                    this.errmsg = "OTPNoMyAccountData";
                    return null;
                }
                JObject jobject = JObject.Parse(regex2.Match(text).Groups[1].Value);
                acc.sname = (string)jobject["ServiceAccountDisplayName"];
                acc.screatetime = (string)jobject["ServiceAccountCreateTime"];
                NameValueCollection nameValueCollection2 = new NameValueCollection();
                nameValueCollection2.Add("service_code", service_code);
                nameValueCollection2.Add("service_region", service_region);
                nameValueCollection2.Add("service_account_id", acc.sid);
                nameValueCollection2.Add("service_sotp", acc.ssn);
                nameValueCollection2.Add("service_display_name", acc.sname);
                nameValueCollection2.Add("service_create_time", acc.screatetime);
                ServicePointManager.Expect100Continue = false;
                this.UploadString("http://hk.beanfun.com/beanfun_block/generic_handlers/record_service_start.ashx", nameValueCollection2);
                text = this.DownloadString(string.Concat(new object[]
                {
                        "http://hk.beanfun.com/beanfun_block/generic_handlers/get_otp.ashx?ppppp=&token=",
                        this.bfServiceX.Token,
                        "&account_service_code=",
                        service_code,
                        "&account_service_region=",
                        service_region,
                        "&service_account_id=",
                        acc.sid,
                        "&create_time=",
                        acc.screatetime.Replace(" ", "%20"),
                        "&d=",
                        Environment.TickCount
                }));

                text = text.Substring(2);
                string a_ = text.Substring(0, 8);
                string text2 = DES.Decrypt(text.Substring(8), a_);
                if (text2 != null)
                {
                    text2 = text2.Trim(new char[1]);
                    this.errmsg = null;
                }
                else
                {
                    this.errmsg = "DecryptOTPError";
                }
                result = text2;
            }
            catch (Exception ex)
            {
                this.errmsg = "獲取密碼失敗，請嘗試重新登入。\n\n" + ex.Message + "\n" + ex.StackTrace;
                result = null;
            }
            Ping();
            return result;
        }

        //获取帐号信息
        private void getAccount(string loginState, string service_code = "610074", string service_region = "T9")
        {
            if (this.sessionkey == null || loginState == null)
            {
                return;
            }
            if (!this.DownloadString(string.Concat(new string[]
            {
                    "http://hk.beanfun.com/beanfun_block/auth.aspx?channel=game_zone&page_and_query=game_start.aspx%3Fservice_code_and_region%3D",
                    service_code,
                    "_",
                    service_region,
                    "&token=",
                    this.bfServiceX.Token
            })).Contains("document.location = \"http://hk.beanfun.com/beanfun_block/game_zone/game_server_account_list.aspx"))
            {
                this.errmsg = "LoginAuthErr"; return;
            }
            this.GetAccounts_HK(service_code, service_region, false);

            if (this.errmsg != null)
            {
                return;
            }
            this.remainPoint = this.getRemainPoint();
            this.errmsg = null;
        }

        //修改账户别名
        public bool ChangeServiceAccountDisplayName(string newName)
        {

            if (newName == null || newName == "")
            {
                return false;
            }
            string gameCode = this.service_code + "_" + this.service_region;
            return true;
        }

        public bool ChangeServiceAccountDisplayName(string newName, string gameCode, string ssid)
        {
            if (newName == null || newName == "" || ssid == null)
            {
                return false;
            }

            return this.gameServAccListApp.ChangeServiceAccountDisplayName(gameCode, ssid, newName, this.GetCookies()) == "";
        }

        public void GetAccounts_HK(string service_code, string service_region, bool fatal = true)
        {
            string input = this.DownloadString("http://hk.beanfun.com/beanfun_block/game_zone/game_server_account_list.aspx?service_code=" + service_code + "&service_region=" + service_region);
            IList serviceAccounts = this.gameServAccListApp.GetServiceAccounts(service_code, service_region,this.GetCookies());
            if (serviceAccounts == null)
            {
                this.errmsg = "LoginGetAccountErr";
                return;
            }
            this.accountList.Clear();
            try
            {
                foreach (object obj in serviceAccounts)
                {
                    ASObject asobject = (ASObject)obj;
                    if (!(asobject["service_code"].ToString() != service_code) && !(asobject["service_region"].ToString() != service_region))
                    {
                        this.accountList.Add(new MLTClient.ServiceAccount(!bool.Parse(asobject["lock_flag"].ToString()), asobject["visible"].ToString() == "1", bool.Parse(asobject["is_inherited"].ToString()), asobject["service_account_id"].ToString(), asobject["service_account_sn"].ToString(), asobject["service_account_display_name"].ToString(), asobject["create_time"].ToString(), asobject["last_used_time"].ToString(), asobject["auth_type"].ToString()));
                    }
                }
            }
            catch
            {
                this.errmsg = "LoginUpdateAccountListErr";
                return;
            }
            Regex regex = new Regex("<service_account_amount_limit>(.*)</service_account_amount_limit>");
            if (regex.IsMatch(input))
            {
                this.accountAmountLimitNotice = "帳號上限：" + regex.Match(input).Groups[1].Value;
            }
            else
            {
                this.accountAmountLimitNotice = "";
            }
            this.errmsg = null;
        }

        //登出
        public void LoginOut()
        {
            this.DownloadString("http://hk.beanfun.com/beanfun_block/generic_handlers/remove_login_session.ashx");
            this.DownloadString("http://hk.beanfun.com/beanfun_web_ap/remove_login_session.ashx");
            if (this.bfServiceX != null)
            {
                this.DownloadString("http://hk.beanfun.com/beanfun_block/generic_handlers/erase_token.ashx?token=" + this.bfServiceX.Token);
            }
        }
        #endregion

        //获取乐豆数量
        public int getRemainPoint()
        {
            string uri = "http://hk.beanfun.com/beanfun_block/generic_handlers/get_remain_point.ashx?token=" + this.bfServiceX.Token;

            string input = this.DownloadString(uri);
            int result;
            try
            {
                Regex regex = new Regex("\"RemainPoint\" : \"(.*)\" }");
                if (regex.IsMatch(input))
                {
                    result = int.Parse(regex.Match(input).Groups[1].Value);
                }
                else
                {
                    result = 0;
                }
            }
            catch
            {
                result = 0;
            }
            return result;
        }

        private string seed = "";
        //获取key
        public string GetSessionkey()
        {

            string text2 = this.DownloadString("http://hk.beanfun.com/beanfun_block/login/index.aspx?service=999999_T0");
            if (text2 == null)
            {
                this.errmsg = "LoginNoResponse";
                return null;
            }
            Regex regex2 = new Regex("otp1 = \"(.*)\";");
            if (!regex2.IsMatch(text2))
            {
                this.errmsg = "LoginNoOTP1";
                return null;
            }
            return regex2.Match(text2).Groups[1].Value;

            Regex regex3 = new Regex("seed = \"(.*)\";");
            if (!regex3.IsMatch(text2))
            {
                this.errmsg = "LoginNoOTP1";
                return null;
            }
            else
            {
                seed = regex3.Match(text2).Groups[1].Value;
            }
            return regex2.Match(text2).Groups[1].Value;
        }

        //心跳
        public void Ping()
        {
            try
            {

                this.DownloadString("http://hk.beanfun.com/beanfun_block/generic_handlers/echo_token.ashx?token=" + this.bfServiceX.Token);

            }
            catch
            {
            }
        }

        public class QRCodeClass
        {
            public string skey;

            public string value;

            public string viewstate;

            public string eventvalidation;

            public string bitmapUrl;

            public bool oldAppQRCode;
        }


        public class ServiceAccount
        {
            // Token: 0x17000015 RID: 21
            // (get) Token: 0x0600018E RID: 398 RVA: 0x0000C600 File Offset: 0x0000A800
            // (set) Token: 0x0600018F RID: 399 RVA: 0x0000C608 File Offset: 0x0000A808
            public bool isEnable { get; set; }

            // Token: 0x17000016 RID: 22
            // (get) Token: 0x06000190 RID: 400 RVA: 0x0000C611 File Offset: 0x0000A811
            // (set) Token: 0x06000191 RID: 401 RVA: 0x0000C619 File Offset: 0x0000A819
            public bool visible { get; set; }

            // Token: 0x17000017 RID: 23
            // (get) Token: 0x06000192 RID: 402 RVA: 0x0000C622 File Offset: 0x0000A822
            // (set) Token: 0x06000193 RID: 403 RVA: 0x0000C62A File Offset: 0x0000A82A
            public bool isinherited { get; set; }

            // Token: 0x17000018 RID: 24
            // (get) Token: 0x06000194 RID: 404 RVA: 0x0000C633 File Offset: 0x0000A833
            // (set) Token: 0x06000195 RID: 405 RVA: 0x0000C63B File Offset: 0x0000A83B
            public string sid { get; set; }

            // Token: 0x17000019 RID: 25
            // (get) Token: 0x06000196 RID: 406 RVA: 0x0000C644 File Offset: 0x0000A844
            // (set) Token: 0x06000197 RID: 407 RVA: 0x0000C64C File Offset: 0x0000A84C
            public string ssn { get; set; }

            // Token: 0x1700001A RID: 26
            // (get) Token: 0x06000198 RID: 408 RVA: 0x0000C655 File Offset: 0x0000A855
            // (set) Token: 0x06000199 RID: 409 RVA: 0x0000C65D File Offset: 0x0000A85D
            public string sname { get; set; }

            // Token: 0x1700001B RID: 27
            // (get) Token: 0x0600019A RID: 410 RVA: 0x0000C666 File Offset: 0x0000A866
            // (set) Token: 0x0600019B RID: 411 RVA: 0x0000C66E File Offset: 0x0000A86E
            public string screatetime { get; set; }

            // Token: 0x1700001C RID: 28
            // (get) Token: 0x0600019C RID: 412 RVA: 0x0000C677 File Offset: 0x0000A877
            // (set) Token: 0x0600019D RID: 413 RVA: 0x0000C67F File Offset: 0x0000A87F
            public string slastusedtime { get; set; }

            // Token: 0x1700001D RID: 29
            // (get) Token: 0x0600019E RID: 414 RVA: 0x0000C688 File Offset: 0x0000A888
            // (set) Token: 0x0600019F RID: 415 RVA: 0x0000C690 File Offset: 0x0000A890
            public string sauthtype { get; set; }

            // Token: 0x060001A0 RID: 416 RVA: 0x0000C69C File Offset: 0x0000A89C
            public ServiceAccount(bool isEnable, string sid, string ssn, string sname, string screatetime)
            {
                this.isEnable = isEnable;
                this.visible = true;
                this.isinherited = false;
                this.sid = sid;
                this.ssn = ssn;
                this.sname = sname;
                this.screatetime = screatetime;
                this.slastusedtime = null;
                this.sauthtype = null;
            }

            // Token: 0x060001A1 RID: 417 RVA: 0x0000C6F0 File Offset: 0x0000A8F0
            public ServiceAccount(bool isEnable, bool visible, bool isinherited, string sid, string ssn, string sname, string screatetime, string slastusedtime, string sauthtype)
            {
                this.isEnable = isEnable;
                this.visible = visible;
                this.isinherited = isinherited;
                this.sid = sid;
                this.ssn = ssn;
                this.sname = sname;
                this.screatetime = screatetime;
                this.slastusedtime = slastusedtime;
                this.sauthtype = sauthtype;
            }

            // Token: 0x0400012F RID: 303
            [CompilerGenerated]
            private bool a;

            // Token: 0x04000130 RID: 304
            [CompilerGenerated]
            private bool b;

            // Token: 0x04000131 RID: 305
            [CompilerGenerated]
            private bool c;

            // Token: 0x04000132 RID: 306
            [CompilerGenerated]
            private string d;

            // Token: 0x04000133 RID: 307
            [CompilerGenerated]
            private string e;

            // Token: 0x04000134 RID: 308
            [CompilerGenerated]
            private string f;

            // Token: 0x04000135 RID: 309
            [CompilerGenerated]
            private string g;

            // Token: 0x04000136 RID: 310
            [CompilerGenerated]
            private string h;

            // Token: 0x04000137 RID: 311
            [CompilerGenerated]
            private string i;
        }

        public MLTClient(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        #region 重写方法WebClient
        public string UploadString(string skey, NameValueCollection payload)
        {
            base.Headers.Set("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36");
            base.Headers.Set("Accept-Encoding", "identity");
            return Encoding.UTF8.GetString(base.UploadValues(skey, payload));
        }

        public new string DownloadString(string Uri)
        {
            base.Headers.Set("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36");
            base.Headers.Set("Accept-Encoding", "identity");
            return base.DownloadString(Uri);
        }

        public string DownloadString(string Uri, Encoding Encoding)
        {
            base.Headers.Set("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36");
            base.Headers.Set("Accept-Encoding", "identity");
            return Encoding.GetString(base.DownloadData(Uri));
        }

        public string UploadStringGZip(string skey, NameValueCollection payload)
        {
            base.Headers.Set("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36");
            base.Headers.Set("Accept-Encoding", "gzip, deflate, br");
            byte[] array = base.UploadValues(skey, payload);
            if (array[0] == 31 && array[1] == 139)
            {
                Stream stream = new MemoryStream(array);
                MemoryStream memoryStream = new MemoryStream();
                GZipStream gzipStream = new GZipStream(stream, CompressionMode.Decompress);
                byte[] array2 = new byte[1000];
                int count;
                while ((count = gzipStream.Read(array2, 0, array2.Length)) > 0)
                {
                    memoryStream.Write(array2, 0, count);
                }
                array = memoryStream.ToArray();
            }
            return Encoding.UTF8.GetString(array);
        }
        private bool AllowAutoRedirect;
        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest httpWebRequest = base.GetWebRequest(address) as HttpWebRequest;
            if (httpWebRequest != null)
            {
                httpWebRequest.CookieContainer = this.cookieContainer;
                httpWebRequest.AllowAutoRedirect = this.AllowAutoRedirect;
            }
            return httpWebRequest;
        }

        private Uri ResponseUri;
        protected override WebResponse GetWebResponse(WebRequest request)
        {
            WebResponse webResponse = base.GetWebResponse(request);
            this.ResponseUri = webResponse.ResponseUri;
            return webResponse;
        }

        //获取cookie
        public CookieCollection GetCookies()
        {
            return cookieContainer.GetCookies(new Uri("https://" + "HK".ToLower() + ".beanfun.com/"));
        }
        #endregion
    }
}
