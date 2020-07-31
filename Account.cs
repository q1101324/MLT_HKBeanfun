using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace 麻辣烫新枫之谷登陆器
{
    public partial class Account : Form
    {
        private BFServiceX bfService;
        private Main main;
        MLTClient.ServiceAccount account;
        public Account(Main main, MLTClient.ServiceAccount account)
        {
            this.main = main;
            this.account = account;
            InitializeComponent();
        }

        private void Account_Load(object sender, EventArgs e)
        {
            this.lb_state.Text = account.isEnable ? "正常" : "封号";
            this.lb_name.Text = account.sname;
            this.tb_user.Text = account.sid;

            IniFile ini = new IniFile("D://MLTconfig.ini");
            textBox1.Text = ini.IniReadValue("Config", "filePath");
        }

        private void Account_FormClosing(object sender, FormClosingEventArgs e)
        {
            main.Show();
            main.mltClient.LoginOut();
            main.enableBtn();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            tb_pwd.Text = main.mltClient.GetOTP(account, "610074", "T9");
        }



        //一键登录
        private void OnkeyLogin(string sid, string pwd)
        {
            IntPtr foregroundWindow = User32API.FindWindow("MapleStoryClass", "MapleStory");
            object dataObject = System.Windows.Clipboard.GetDataObject();
            System.Windows.Clipboard.SetText(sid);
            User32API.SetForegroundWindow(foregroundWindow);
            Thread.Sleep(10);
            User32API.keybd_event(17, 157, 1, 0);
            User32API.SetForegroundWindow(foregroundWindow);
            Thread.Sleep(10);
            User32API.keybd_event(86, 158, 0, 0);
            Thread.Sleep(100);
            User32API.SetForegroundWindow(foregroundWindow);
            Thread.Sleep(10);
            User32API.keybd_event(86, 158, 2, 0);
            User32API.SetForegroundWindow(foregroundWindow);
            Thread.Sleep(10);
            User32API.keybd_event(17, 157, 3, 0);
            User32API.SetForegroundWindow(foregroundWindow);
            Thread.Sleep(10);
            User32API.keybd_event(9, 0, 1, 0);
            User32API.SetForegroundWindow(foregroundWindow);
            Thread.Sleep(10);
            User32API.keybd_event(9, 0, 2, 0);
            Thread.Sleep(250);
            System.Windows.Clipboard.SetText(pwd);
            User32API.SetForegroundWindow(foregroundWindow);
            Thread.Sleep(10);
            User32API.keybd_event(17, 157, 1, 0);
            User32API.SetForegroundWindow(foregroundWindow);
            Thread.Sleep(10);
            User32API.keybd_event(86, 158, 0, 0);
            Thread.Sleep(100);
            User32API.SetForegroundWindow(foregroundWindow);
            Thread.Sleep(10);
            User32API.keybd_event(86, 158, 2, 0);
            User32API.SetForegroundWindow(foregroundWindow);
            Thread.Sleep(10);
            User32API.keybd_event(17, 157, 3, 0);
            User32API.SetForegroundWindow(foregroundWindow);
            Thread.Sleep(10);
            User32API.keybd_event(13, 0, 0, 0);
            User32API.SetForegroundWindow(foregroundWindow);
            Thread.Sleep(10);
            User32API.keybd_event(13, 0, 2, 0);
            System.Windows.Clipboard.SetDataObject(dataObject);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            tb_pwd.Text = main.mltClient.GetOTP(account, "610074", "T9");
            OnkeyLogin(tb_user.Text, tb_pwd.Text);
        }

        private string game_commandLine = "tw.login.maplestory.gamania.com 8484 BeanFun %s %s";
        private DateTime NowTime = DateTime.Now.AddMinutes(1);
        private void button3_Click(object sender, EventArgs e)
        {
            string processName = "MapleStory", fileName = textBox1.Text;
            bool flag = false;
            //游戏是否打开
            foreach (Process process in Process.GetProcessesByName(processName))
            {
                try
                {
                    if (process.MainModule.FileName == fileName)
                    {
                        flag = true;
                        break;
                    }
                }
                catch
                {
                }
            }
 
            //询问是否关闭游戏
            if (flag && System.Windows.MessageBox.Show("游戏已经运行,可能是客戶端问题导致未完全关闭,是否要结束游戏?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                foreach (Process process2 in Process.GetProcessesByName(processName))
                {
                    try
                    {
                        if (process2.MainModule.FileName == fileName)
                        {
                            process2.Kill();
                        }
                    }
                    catch
                    {
                    }
                }
            }

            tb_pwd.Text = main.mltClient.GetOTP(account, "610074", "T9");


            string param = game_commandLine;
            Regex regex2 = new Regex("%s");
            param = regex2.Replace(param, tb_user .Text, 1);
            param = regex2.Replace(param, tb_pwd.Text, 1);


            TextInfo textInfo = CultureInfo.GetCultureInfo("zh-HK").TextInfo;
            String text = (textBox1.Text.StartsWith("\"") ? (textBox1.Text + " ") : ("\"" + textBox1.Text + "\" "));
            text += param;

            LoaderWrapper loader = new LoaderWrapper();
            loader.ApplicationName = textBox1.Text;
            loader.CommandLine = text;
            loader.CurrentDirectory = Path.GetDirectoryName(textBox1.Text);
            loader.AnsiCodePage = (uint)textInfo.ANSICodePage;
            loader.OemCodePage = (uint)textInfo.OEMCodePage;
            loader.LocaleID = (uint)textInfo.LCID;
            loader.DefaultCharset = (uint)136;
            loader.HookUILanguageAPI = 0U;
            loader.Timezone = "China Standard Time";
            loader.NumberOfRegistryRedirectionEntries = 0;
            loader.DebugMode = false;
            loader.Start();

            NowTime = DateTime.Now;
            NowTime.AddMinutes(1);
            IniFile ini = new IniFile("D://MLTconfig.ini");
            ini.IniWriteValue("Config", "filePath", textBox1.Text);

            Thread.Sleep(1500);

            IntPtr startHwnd = User32API.FindWindow("StartUpDlgClass", "MapleStory");
            if (startHwnd != IntPtr.Zero)
            {
                User32API.PostMessage(startHwnd, 16U, 0, 0);
            }

        }
    }
}
