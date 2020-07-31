using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace 麻辣烫新枫之谷登陆器
{
    class User32API
    {
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, uint lParam);
        [DllImport("user32.dll")]
        public static extern int PostMessage(IntPtr hWnd, uint Msg, int wParam, uint lParam);
        [DllImport("user32.dll")]
        public static extern int SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);
        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        public extern static IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowThreadProcessId(IntPtr hwnd, out int ID);

        [DllImport("kernel32.dll")]
        public static extern int GetCurrentThreadId(); //取得当前线程编号的API 

        [DllImport("User32.dll")]
        internal extern static void UnhookWindowsHookEx(IntPtr handle); //取消Hook的API 

        [DllImport("User32.dll")]
        internal extern static IntPtr SetWindowsHookEx(int idHook, [MarshalAs(UnmanagedType.FunctionPtr)] HookProc lpfn, IntPtr hinstance, int threadID); //设置Hook的API 

        [DllImport("User32.dll")]
        internal extern static IntPtr CallNextHookEx(IntPtr handle, int code, IntPtr wparam, IntPtr lparam); //取得下一个Hook的API 
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string name);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left; //最左坐标
            public int Top; //最上坐标
            public int Right; //最右坐标
            public int Bottom; //最下坐标
        }

        public delegate int Hookproc(int code, IntPtr wparam, ref cwpstruct cwp);
        internal delegate IntPtr HookProc(int code, IntPtr wparam, ref cwpstruct cwp);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int GetLastError();

        private IntPtr hookhandle = IntPtr.Zero;
        private static HookProc hookproc;
        static IntPtr _nextHookPtr; //记录Hook编号
        [StructLayout(LayoutKind.Sequential)]
        public struct cwpstruct
        {
            public IntPtr lparam;
            public IntPtr wparam;
            public int message;
            public IntPtr hwnd;
        }
        //以下为HOOK内容
        internal enum HookType //枚举，钩子的类型 
        {
            //MsgFilter = -1, 
            //JournalRecord = 0, 
            //JournalPlayback = 1, 
            //Keyboard = 2,
            //GetMessage = 3, 
            CallWndProc = 4,
            WM_MOVE = 0x03,
            //CBT = 5, 
            //SysMsgFilter = 6, 

            //Mouse = 7, 
            //Hardware = 8, 
            //Debug = 9, 
            //Shell = 10, 
            //ForegroundIdle = 11, 
            //CallWndProcRet = 12, 
            //KeyboardLL = 13, 
            //MouseLL = 14, 
        };


        public static void SetHook(int threadID, string name)
        {
            if (_nextHookPtr != IntPtr.Zero) //已经勾过了 
                return;

            // HOOKPROC为委托
            hookproc = new HookProc(myhookproc);
            IntPtr moudle = GetModuleHandle(name);
            _nextHookPtr = User32API.SetWindowsHookEx((int)HookType.WM_MOVE, hookproc, moudle, threadID); //加到Hook链中 

            if (_nextHookPtr == IntPtr.Zero)
            {
                int errorCode = GetLastError();
                System.Console.WriteLine("SetWindowsHookEx failed.error code:" + errorCode);

            }

        }

        public static IntPtr myhookproc(int code, IntPtr wparam, ref cwpstruct cwp)
        {
            switch (code)
            {
                case 0:
                    switch (cwp.message)
                    {
                        case 0x0000f://wm_paint，拦截wm_paint消息
                            //MessageBox.Show("已经钩挂", "HOOK提示");//do something
                            break;
                    }
                    break;
            }
            return User32API.CallNextHookEx(_nextHookPtr, code, wparam, cwp.lparam);
        }

        public static void UnHook()
        {
            if (_nextHookPtr != IntPtr.Zero)
            {
                User32API.UnhookWindowsHookEx(_nextHookPtr); //从Hook链中取消 
                _nextHookPtr = IntPtr.Zero;
            }
        }
    }
}
