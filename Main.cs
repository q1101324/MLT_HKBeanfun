using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 麻辣烫新枫之谷登陆器
{
    public partial class Main : Form
    {
        public MLTClient mltClient;
        public Main()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;

            Func<int> fun = login;
            fun.BeginInvoke(TakesAWhileCallBack, fun);//主线程调用子线程开始执行TakeAWhile方法,并给它传递了参数
        }


        private int login()
        {
            mltClient.Login(textBox1.Text, textBox2.Text, 0, "610074", "T9");

            IniFile ini = new IniFile("D://MLTconfig.ini");
            ini.IniWriteValue("Config", "user", textBox1.Text);
            ini.IniWriteValue("Config", "pwd", textBox2.Text);
            if (mltClient.errmsg != "" && mltClient.errmsg!= null)
            {
                button1.Enabled = true;

                if (mltClient.errmsg.Contains("BFServiceXNotFound"))
                {
                    MessageBox.Show("缤放组件没有安装，点击确定下载。");
                    HttpDownloadFile("http://hk.download.beanfun.com/beanfun20/beanfun_2_0_93_170_hk.exe", "c:/bf.exe");
                }
            }
            return 0;
        }

        /// <summary>
        /// 回调函数
        /// </summary>
        /// <param name="fun">调用的委托</param>
        void TakesAWhileCallBack(IAsyncResult fun)
        {
            if (mltClient.accountList.Count>0)
            {
                MessageBox.Show("登录成功。");
                this.Hide();
                Account account = new Account(this, mltClient.accountList[0]);
                account.ShowDialog();
               
            }
            else
            {
                MessageBox.Show(mltClient.errmsg);
            }
        }

        public void enableBtn()
        {
            this.button1.Enabled = true;
        }

        private void Main_Load(object sender, EventArgs e)
        {
            //加载时 取消跨线程检查
            Control.CheckForIllegalCrossThreadCalls = false;
            this.mltClient = new MLTClient();
            IniFile ini = new IniFile("D://MLTconfig.ini");
            textBox1.Text = ini.IniReadValue("Config", "user");
            textBox2.Text = ini.IniReadValue("Config", "pwd");
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            mltClient.Ping();
        }


        /// <summary>
        /// Http下载文件
        /// </summary>
        public static string HttpDownloadFile(string url, string path)
        {
            // 设置参数
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            //发送请求并获取相应回应数据
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            //直到request.GetResponse()程序才开始向目标网页发送Post请求
            Stream responseStream = response.GetResponseStream();
            //创建本地文件写入流
            Stream stream = new FileStream(path, FileMode.Create);
            byte[] bArr = new byte[1024];
            int size = responseStream.Read(bArr, 0, (int)bArr.Length);
            while (size > 0)
            {
                stream.Write(bArr, 0, size);
                size = responseStream.Read(bArr, 0, (int)bArr.Length);
            }
            stream.Close();
            responseStream.Close();
            return path;
        }
    }
}
