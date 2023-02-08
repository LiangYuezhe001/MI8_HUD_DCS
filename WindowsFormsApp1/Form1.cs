using System;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Reflection;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Diagnostics;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        static Socket server_r;
        //接收socket
        static float currentSimTime, TAS;
        static double simtime, vvx, vvy, vvz, MAGyaw, altBar, altRad, bvx, bvy, obvx, obvy, oVerticalVelocity, odVerticalVelocity, dVerticalVelocity,
            VerticalVelocity, pitch, bank, yaw, SBP, lRPM, rRPM, oTAS, dTAS, odTAS,camx,camy,camz, visyaw,magdiff;
        static int flag_puase = 0, flag_hide = 0, flag_blind = 0,flag_timeout=0;
        static int pix_bvx, pix_bvy;
        static int mywidth, myheight;
        static int pic_size = 500;

        KeyboardHook kh;

        static string dis_yaw,dis_visyaw;
        //static int gradations_yaw;
        static Bitmap bmp = new Bitmap(pic_size, pic_size);

        Graphics g = Graphics.FromImage(bmp);

        Pen p = new Pen(Color.LimeGreen);
        Pen p2 = new Pen(Color.LimeGreen);

        public class Win32Api

        {

            #region 常数和结构

            public const int WM_KEYDOWN = 0x100;

            public const int WM_KEYUP = 0x101;

            public const int WM_SYSKEYDOWN = 0x104;

            public const int WM_SYSKEYUP = 0x105;

            public const int WH_KEYBOARD_LL = 13;



            [StructLayout(LayoutKind.Sequential)] //声明键盘钩子的封送结构类型 

            public class KeyboardHookStruct

            {

                public int vkCode; //表示一个在1到254间的虚似键盘码 

                public int scanCode; //表示硬件扫描码 

                public int flags;

                public int time;

                public int dwExtraInfo;

            }

            #endregion

            #region Api

            public delegate int HookProc(int nCode, Int32 wParam, IntPtr lParam);

            //安装钩子的函数 

            [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]

            public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

            //卸下钩子的函数 

            [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]

            public static extern bool UnhookWindowsHookEx(int idHook);

            //下一个钩挂的函数 

            [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]

            public static extern int CallNextHookEx(int idHook, int nCode, Int32 wParam, IntPtr lParam);

            [DllImport("user32")]

            public static extern int ToAscii(int uVirtKey, int uScanCode, byte[] lpbKeyState, byte[] lpwTransKey, int fuState);

            [DllImport("user32")]

            public static extern int GetKeyboardState(byte[] pbKeyState);

            [DllImport("kernel32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]

            public static extern IntPtr GetModuleHandle(string lpModuleName);

            #endregion

        }
        public class KeyboardHook

        {

            int hHook;

            Win32Api.HookProc KeyboardHookDelegate;

            public event KeyEventHandler OnKeyDownEvent;

            public event KeyEventHandler OnKeyUpEvent;

            public event KeyPressEventHandler OnKeyPressEvent;

            public KeyboardHook() { }

            public void SetHook()

            {

                KeyboardHookDelegate = new Win32Api.HookProc(KeyboardHookProc);

                Process cProcess = Process.GetCurrentProcess();

                ProcessModule cModule = cProcess.MainModule;

                var mh = Win32Api.GetModuleHandle(cModule.ModuleName);

                hHook = Win32Api.SetWindowsHookEx(Win32Api.WH_KEYBOARD_LL, KeyboardHookDelegate, mh, 0);

            }

            public void UnHook()

            {

                Win32Api.UnhookWindowsHookEx(hHook);

            }

            private List<Keys> preKeysList = new List<Keys>();//存放被按下的控制键，用来生成具体的键

            private int KeyboardHookProc(int nCode, Int32 wParam, IntPtr lParam)

            {

                //如果该消息被丢弃（nCode<0）或者没有事件绑定处理程序则不会触发事件

                if ((nCode >= 0) && (OnKeyDownEvent != null || OnKeyUpEvent != null || OnKeyPressEvent != null))

                {

                    Win32Api.KeyboardHookStruct KeyDataFromHook = (Win32Api.KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(Win32Api.KeyboardHookStruct));

                    Keys keyData = (Keys)KeyDataFromHook.vkCode;

                    //按下控制键

                    if ((OnKeyDownEvent != null || OnKeyPressEvent != null) && (wParam == Win32Api.WM_KEYDOWN || wParam == Win32Api.WM_SYSKEYDOWN))

                    {

                        if (IsCtrlAltShiftKeys(keyData) && preKeysList.IndexOf(keyData) == -1)

                        {

                            preKeysList.Add(keyData);

                        }

                    }

                    //WM_KEYDOWN和WM_SYSKEYDOWN消息，将会引发OnKeyDownEvent事件

                    if (OnKeyDownEvent != null && (wParam == Win32Api.WM_KEYDOWN || wParam == Win32Api.WM_SYSKEYDOWN))

                    {

                        KeyEventArgs e = new KeyEventArgs(GetDownKeys(keyData));



                        OnKeyDownEvent(this, e);

                    }

                    //WM_KEYDOWN消息将引发OnKeyPressEvent 

                    if (OnKeyPressEvent != null && wParam == Win32Api.WM_KEYDOWN)

                    {

                        byte[] keyState = new byte[256];

                        Win32Api.GetKeyboardState(keyState);

                        byte[] inBuffer = new byte[2];

                        if (Win32Api.ToAscii(KeyDataFromHook.vkCode, KeyDataFromHook.scanCode, keyState, inBuffer, KeyDataFromHook.flags) == 1)

                        {

                            KeyPressEventArgs e = new KeyPressEventArgs((char)inBuffer[0]);

                            OnKeyPressEvent(this, e);

                        }

                    }

                    //松开控制键

                    if ((OnKeyDownEvent != null || OnKeyPressEvent != null) && (wParam == Win32Api.WM_KEYUP || wParam == Win32Api.WM_SYSKEYUP))

                    {

                        if (IsCtrlAltShiftKeys(keyData))

                        {

                            for (int i = preKeysList.Count - 1; i >= 0; i--)

                            {

                                if (preKeysList[i] == keyData) { preKeysList.RemoveAt(i); }

                            }

                        }

                    }

                    //WM_KEYUP和WM_SYSKEYUP消息，将引发OnKeyUpEvent事件 

                    if (OnKeyUpEvent != null && (wParam == Win32Api.WM_KEYUP || wParam == Win32Api.WM_SYSKEYUP))

                    {

                        KeyEventArgs e = new KeyEventArgs(GetDownKeys(keyData));

                        OnKeyUpEvent(this, e);

                    }

                }

                return Win32Api.CallNextHookEx(hHook, nCode, wParam, lParam);

            }

            //根据已经按下的控制键生成key

            private Keys GetDownKeys(Keys key)

            {

                Keys rtnKey = Keys.None;

                foreach (Keys i in preKeysList)

                {

                    if (i == Keys.LControlKey || i == Keys.RControlKey) { rtnKey = rtnKey | Keys.Control; }

                    if (i == Keys.LMenu || i == Keys.RMenu) { rtnKey = rtnKey | Keys.Alt; }

                    if (i == Keys.LShiftKey || i == Keys.RShiftKey) { rtnKey = rtnKey | Keys.Shift; }

                }

                return rtnKey | key;

            }

            private Boolean IsCtrlAltShiftKeys(Keys key)

            {

                if (key == Keys.LControlKey || key == Keys.RControlKey || key == Keys.LMenu || key == Keys.RMenu || key == Keys.LShiftKey || key == Keys.RShiftKey) { return true; }

                return false;

            }

        }



        private void recive()
        {
            int length = 0;
            double filter_par=0.05;
            //接受
            byte[] buffer = new byte[1024];
            EndPoint point2 = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345);
            try
            {
                length = server_r.ReceiveFrom(buffer, ref point2);//接收数据报  

            }
            catch (SocketException)
            {

                pause();
                flag_timeout = 1;
            }
            if (flag_timeout == 0)
            {
                timer1.Start();
                string message = Encoding.UTF8.GetString(buffer, 0, length);

                if (message == "quit")
                {
                    flag_puase = 0;
                    timer1.Stop();
                    g.Clear(Color.White);
                    //this.Hide();

                }

                string head = message.Substring(0, 2);
                string body = message.Substring(2);



                if (head == "tm")
                {
                    currentSimTime = float.Parse(body);

                }
                else if (head == "va")
                {
                    TAS = float.Parse(body);
                    TAS = TAS * (float)3.6;
                    dTAS = (TAS - oTAS) * filter_par + odTAS * (1 - filter_par);
                    odTAS = dTAS;
                    oTAS = TAS;


                }
                else
                {

                    if (head == "ms")
                    {



                        string[] rawData = body.Split(';');
                        string[] STData = rawData[0].Split(',');

                        simtime = double.Parse(STData[0]);
                        vvx = double.Parse(STData[1]);
                        vvy = double.Parse(STData[2]);
                        vvz = double.Parse(STData[3]);
                        MAGyaw = double.Parse(STData[4]);
                        altBar = double.Parse(STData[5]);
                        altRad = double.Parse(STData[6]);
                        VerticalVelocity = double.Parse(STData[7]);
                        dVerticalVelocity = (VerticalVelocity - oVerticalVelocity)* filter_par+odVerticalVelocity*(1- filter_par);
                        odVerticalVelocity = dVerticalVelocity;
                        oVerticalVelocity = VerticalVelocity;
                        pitch = double.Parse(STData[8]);
                        bank = double.Parse(STData[9]);
                        yaw = double.Parse(STData[10]);
                        lRPM = double.Parse(STData[11]);
                        rRPM = double.Parse(STData[12]);
                        camx= double.Parse(STData[13]);
                        camz = double.Parse(STData[15]);
                     

                    }
                }
            }
        }
        private void startup()
        {
            if (flag_blind == 0)
            {
                //接受网络设置
                server_r = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                server_r.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345));//绑定端口号和IP  
                server_r.ReceiveTimeout = 1000;
                server_r.SendTimeout = 1000;

                timer1.Start();
                button1.BackColor = Color.White;
                button1.Enabled = false;
                flag_blind = 1;

            }

            while (true)
            {

                recive();
                if (flag_puase == 0) { break; }
                if (flag_timeout == 1) 
                {
                    //g.DrawString("TIMEOUT", new Font("Lucida Console", 20), Brushes.LimeGreen, new PointF(190, 200));
                   // this.CreateGraphics().DrawImage(bmp, (int)((mywidth - pic_size) / 2), (int)((myheight - pic_size) / 2));
                    timer1.Stop();
                    flag_puase = 0;
                    break; 
                }
                Application.DoEvents();

            }

        }

        private void pause()
        {
            flag_puase = 0;
            timer1.Stop();
            g.Clear(Color.White);
            this.Hide();
            flag_hide = 1;
        }
        

        private void go_continue()
        {
            flag_puase = 1;
            startup();
            this.Show();
            flag_hide = 0;
            flag_timeout = 0;
        }

        private void draw_magyaw_indicator()
        {
            int a = 10; //上缘 
            int b; //短下缘
            int c = 30; //长下缘
            int j = 1;
            int gap, pos, num = 30;
            int gradations_yaw;

            gap = (400 - 100) / num;
            gradations_yaw = -(int)(((MAGyaw / Math.PI * 90) % 10) * 10);            
            dis_yaw = string.Format("{0:d3}", (int)(MAGyaw / Math.PI * 180));
            for (int i = 100 + gap; i <= 500; i += gap)
            {

                if (j < 5)
                {
                    j++;
                    b = 20;
                }
                else
                {
                    j = 1;
                    b = c;
                }
                pos = i + gradations_yaw;



                if (pos >= 100 && pos <= 400)
                {
                    g.DrawLine(p, new Point(pos, a), new Point(pos, b));
                }
            }



            if (MAGyaw <= Math.PI / 6)
            {
                g.DrawString("N", new Font("Lucida Console", 10), Brushes.LimeGreen, new PointF(245 - (int)(MAGyaw / Math.PI * 900), 0));
            }
            if (MAGyaw >= Math.PI / 6 * 11)
            {
                g.DrawString("N", new Font("Lucida Console", 10), Brushes.LimeGreen, new PointF(2045 - (int)(MAGyaw / Math.PI * 900), 0));
            }

            if (MAGyaw >= Math.PI / 3 && MAGyaw <= Math.PI / 6 * 4)
            {
                g.DrawString("E", new Font("Lucida Console", 10), Brushes.LimeGreen, new PointF(695 - (int)(MAGyaw / Math.PI * 900), 0));
            }

            if (MAGyaw >= Math.PI / 6 * 5 && MAGyaw <= Math.PI / 6 * 7)
            {
                g.DrawString("S", new Font("Lucida Console", 10), Brushes.LimeGreen, new PointF(1145 - (int)(MAGyaw / Math.PI * 900), 0));
            }

            if (MAGyaw >= Math.PI / 6 * 8 && MAGyaw <= Math.PI / 6 * 10)
            {
                g.DrawString("W", new Font("Lucida Console", 10), Brushes.LimeGreen, new PointF(1595 - (int)(MAGyaw / Math.PI * 900), 0));
            }

            g.DrawString(dis_yaw, new Font("Lucida Console", 16), Brushes.LimeGreen, new PointF(226, 30));

            g.DrawRectangle(p, 247, 10, 6, 20);

        }

        private void draw_visyaw_indicator()
        {
            int a = 500; //上缘 
            int b; //短下缘
            int c = 480; //长下缘
            int j = 1;
            int gap, pos, num = 30;
            int gradations_yaw;
            magdiff = MAGyaw - yaw;
            if (camz >= 0)
            {
                visyaw = (camx + 1) * 90 + 180 + magdiff;
                dis_visyaw = string.Format("{0:d3}", (int)visyaw);
                
            }
            else
            {
                visyaw = -(camx + 1) * 90 + 180 + magdiff;
                dis_visyaw = string.Format("{0:d3}", (int)visyaw);
            }

            gap = (400 - 100) / num;
            gradations_yaw = -(int)((visyaw % 10) * 5);

            for (int i = 100 ; i <= 500; i += gap)
            {

                if (j < 5)
                {
                    j++;
                    b = 490;
                }
                else
                {
                    j = 1;
                    b = c;
                }
                pos = i + gradations_yaw;



                if (pos >= 100 && pos <= 400)
                {
                    g.DrawLine(p, new Point(pos, a), new Point(pos, b));
                }
            }


           // dis_visyaw = string.Format("{0:d3}", dis_visyaw);
            g.DrawString(dis_visyaw, new Font("Lucida Console", 16), Brushes.LimeGreen, new PointF(226, 460));

            g.DrawRectangle(p, 247, 480, 6, 20);

        }

        private void draw_vectorspeed_indicator()
        {
            bvx = (vvx * Math.Cos(yaw) + vvz * Math.Sin(yaw)) * 0.5 + obvx * 0.5;
            bvy = (-vvx * Math.Sin(yaw) + vvz * Math.Cos(yaw)) * 0.5 + obvy * 0.5;
            obvx = bvx;
            obvy = bvy;
            if (TAS <= 25)
            {
                pix_bvx = (int)(bvx * 20);
                pix_bvy = (int)(bvy * 20);
            }
            else
            {
                pix_bvx = (int)(bvx * 10);
                pix_bvy = (int)(bvy * 10);
            }

            if (TAS <= 50)
            {
                g.DrawLine(p, new Point(250, 250), new Point(250 + pix_bvy, 250 - pix_bvx));

                g.DrawEllipse(p, 247 + pix_bvy, 247 - pix_bvx, 6, 6);
            }
        }

        private void tas_indicator()
        {
            g.DrawString(Convert.ToString((int)TAS), new Font("Lucida Console", 14), Brushes.LimeGreen, new PointF(66, 78));
            g.DrawLine(p2, new Point(85, 250), new Point(85, 250 - (int)(Math.Atan(dTAS * 30)/Math.PI*300)));


            if (TAS <= 50)
            {
                g.DrawRectangle(p, 70, 100, 10, 300);
                p2.Width = 10;
                g.DrawLine(p2, new Point(75, 400), new Point(75, 400 - (int)(TAS * 6)));
                g.DrawString("50", new Font("Lucida Console", 12), Brushes.LimeGreen, new PointF(40, 100));

            }
            else
            {
                g.DrawRectangle(p, 70, 100, 10, 300);
                p2.Width = 10;
                g.DrawLine(p2, new Point(75, 400), new Point(75, 400 - (int)(TAS)));
                g.DrawString("300", new Font("Lucida Console", 10), Brushes.LimeGreen, new PointF(30, 100));
            }
        }

        private void height_indicator()
        {
            int a = 400, dis_VerticalVelocity;
            
            g.DrawString(Convert.ToString((int)altBar) + "B", new Font("Lucida Console", 14), Brushes.LimeGreen, new PointF(412, 58));
            g.DrawString(Convert.ToString((int)altRad) + "R", new Font("Lucida Console", 14), Brushes.LimeGreen, new PointF(412, 78));


            g.DrawLine(p, new Point(a, 100), new Point(a + 30, 100));
            g.DrawLine(p, new Point(a, 150), new Point(a + 30, 150));
            g.DrawLine(p, new Point(a + 10, 170), new Point(a + 20, 170));
            g.DrawLine(p, new Point(a + 10, 190), new Point(a + 20, 190));
            g.DrawLine(p, new Point(a + 10, 210), new Point(a + 20, 210));
            g.DrawLine(p, new Point(a + 10, 230), new Point(a + 20, 230));
            //g.DrawLine(p, new Point(a, 250), new Point(a+20, 250));
            g.DrawLine(p, new Point(a, 250), new Point(a + 30, 250));
            g.DrawLine(p, new Point(a + 10, 270), new Point(a + 20, 270));
            g.DrawLine(p, new Point(a + 10, 290), new Point(a + 20, 290));
            g.DrawLine(p, new Point(a + 10, 310), new Point(a + 20, 310));
            g.DrawLine(p, new Point(a + 10, 330), new Point(a + 20, 330));
            g.DrawLine(p, new Point(a, 350), new Point(a + 30, 350));
            g.DrawLine(p, new Point(a, 400), new Point(a + 30, 400));

            if (Math.Abs(VerticalVelocity) <= 5)
            {
                dis_VerticalVelocity = (int)(VerticalVelocity * 20);
                g.DrawLine(p, new Point(398, 244 - dis_VerticalVelocity), new Point(398, 256 - dis_VerticalVelocity));
                g.DrawLine(p, new Point(398, 244 - dis_VerticalVelocity), new Point(410, 250 - dis_VerticalVelocity));
                g.DrawLine(p, new Point(398, 256 - dis_VerticalVelocity), new Point(410, 250 - dis_VerticalVelocity));
                g.DrawString(Convert.ToString((int)Math.Abs( VerticalVelocity)), new Font("Lucida Console", 12), Brushes.LimeGreen,
                    new PointF(385, 244 - dis_VerticalVelocity));
            }
            else
            {
                if (VerticalVelocity > 0)
                {
                    dis_VerticalVelocity = (int)(Math.Atan(VerticalVelocity - 5) / Math.PI * 100 + 100);
                    g.DrawLine(p, new Point(398, 244 - dis_VerticalVelocity), new Point(398, 256 - dis_VerticalVelocity));
                    g.DrawLine(p, new Point(398, 244 - dis_VerticalVelocity), new Point(410, 250 - dis_VerticalVelocity));
                    g.DrawLine(p, new Point(398, 256 - dis_VerticalVelocity), new Point(410, 250 - dis_VerticalVelocity));
                    g.DrawString(Convert.ToString((int)Math.Abs(VerticalVelocity)), new Font("Lucida Console", 12), Brushes.LimeGreen,
                      new PointF(385, 244 - dis_VerticalVelocity));
                }
                else
                {
                    dis_VerticalVelocity = (int)(Math.Atan(VerticalVelocity + 5) / Math.PI * 100 - 100);
                    g.DrawLine(p, new Point(398, 244 - dis_VerticalVelocity), new Point(398, 256 - dis_VerticalVelocity));
                    g.DrawLine(p, new Point(398, 244 - dis_VerticalVelocity), new Point(410, 250 - dis_VerticalVelocity));
                    g.DrawLine(p, new Point(398, 256 - dis_VerticalVelocity), new Point(410, 250 - dis_VerticalVelocity));
                    g.DrawString(Convert.ToString((int)Math.Abs(VerticalVelocity)), new Font("Lucida Console", 12), Brushes.LimeGreen,
                    new PointF(385, 244 - dis_VerticalVelocity));
                }
            }
            g.DrawLine(p2, new Point(435, 250), new Point(435, 250 - (int)(Math.Atan(dVerticalVelocity * 100)/Math.PI*300)));

            if (altRad <= 50)
            {
                g.DrawRectangle(p, 420, 100, 10, 300);
                p2.Width = 10;
                g.DrawLine(p2, new Point(425, 400), new Point(425, 400 - (int)(altRad * 6)));
                g.DrawString("50", new Font("Lucida Console", 12), Brushes.LimeGreen, new PointF(440, 100));
            }
            else
            {
                g.DrawRectangle(p, 420, 100, 10, 300);
                p2.Width = 10;
                g.DrawLine(p2, new Point(425, 400), new Point(425, 400 - (int)(altRad * 6 / 20)));
                g.DrawString("1000", new Font("Lucida Console", 12), Brushes.LimeGreen, new PointF(440, 100));
            }

            if (TAS <= 60 && VerticalVelocity <= -4)
            {
                g.DrawString("VRS", new Font("Lucida Console", 34), Brushes.LimeGreen, new PointF(200, 150));
                g.DrawRectangle(p, 200, 154, 100, 35);
            }

            
        }

        private void draw_ADI()
        {
            int mid = (int)pic_size / 2;
            int cross_size = 5;
            int long_line = 100, short_line = 50, gap = 50, acc;
            double CB, SB, CTB, STB, LSB, LCB, SSB, SCB, SCTB, SSTB;

            CB = Math.Cos(bank);
            SB = Math.Sin(bank);
            CTB = Math.Cos(Math.PI / 2 + bank);
            STB = Math.Sin(Math.PI / 2 + bank);
            LSB = long_line * SB;
            LCB = long_line * CB;
            acc = (int)(pitch / Math.PI * 900);

            g.DrawLine(p, new Point(mid - cross_size, mid), new Point(mid + cross_size, mid));
            g.DrawLine(p, new Point(mid, mid - cross_size), new Point(mid, mid + cross_size));

            g.DrawLine(p, new Point(180, mid), new Point(230, mid));
            g.DrawLine(p, new Point(230, mid), new Point(230, 260));

            g.DrawLine(p, new Point(270, mid), new Point(320, mid));
            g.DrawLine(p, new Point(270, mid), new Point(270, 260));


            //CTB = 0;
            //STB = 0;
            if (TAS >= 25 && TAS <= 60)
            {
                g.DrawLine(p, new Point((int)(mid - LCB), (int)(mid + LSB) + acc),
                    new Point((int)(mid + LCB), (int)(mid - LSB) + acc));

            }
            if (TAS > 60)
            {
                SSB = short_line * SB;
                SCB = short_line * CB;
                SSTB = gap * STB;
                SCTB = gap * CTB;

                g.DrawLine(p, new Point((int)(mid - LCB), (int)(mid + LSB) + acc),
                    new Point((int)(mid + LCB), (int)(mid - LSB) + acc));

                g.DrawLine(p, new Point((int)(mid - SCB + SCTB), (int)(mid + SSB - SSTB) + acc),
                    new Point((int)(mid + SCB + SCTB), (int)(mid - SSB - SSTB) + acc));
                g.DrawLine(p, new Point((int)(mid - SCB - SCTB), (int)(mid + SSB + SSTB) + acc),
                 new Point((int)(mid + SCB - SCTB), (int)(mid - SSB + SSTB) + acc));
            }

        }

        private void draw_others()
        {
            ////////////////////////RPM/////////////
            ///
            if (lRPM <= 70) { lRPM = 70; }
            if (rRPM <= 70) { rRPM = 70; }
            g.DrawString(Convert.ToString((int)(lRPM)), new Font("Lucida Console", 10), Brushes.LimeGreen, new PointF(60, 410));
            g.DrawString(Convert.ToString((int)(rRPM)), new Font("Lucida Console", 10), Brushes.LimeGreen, new PointF(80, 410));
            g.DrawLine(p, new Point(69, 400), new Point(69, 400 - (int)((rRPM - 70) * 10)));
            g.DrawLine(p, new Point(67, 400), new Point(67, 400 - (int)((lRPM - 70) * 10)));
            ///////////////////////////////////////////////////////////
            ///////////////////SBP////////////////////////
            if (TAS < 1)
            {
                SBP = 0;
            }
            else
            {
                SBP = Math.Sin(Math.Atan2(obvy, obvx));
            }
            g.DrawLine(p, new Point(150, 450), new Point(350, 450));
            g.DrawRectangle(p, 235, 430, 20, 20);
            g.DrawEllipse(p, 235 + (int)(SBP * 100), 430, 20, 20);
            ////////////////////////////////////////
            ///time
            ///
            g.DrawString(Convert.ToString((int)currentSimTime), new Font("Lucida Console", 12), Brushes.LimeGreen, new PointF(410, 410));
            /////////////////////////////////////
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            p.Width = 2;
            g.Clear(Color.White);
            draw_magyaw_indicator();
            draw_vectorspeed_indicator();
            tas_indicator();
            height_indicator();
            draw_ADI();
            draw_others();
            draw_visyaw_indicator();

            this.CreateGraphics().DrawImage(bmp, (int)((mywidth - pic_size) / 2), (int)((myheight - pic_size) / 2));
        }




        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.BackColor = Color.White;

            this.TransparencyKey = Color.White;
            kh = new KeyboardHook();

            kh.SetHook();

            kh.OnKeyDownEvent += kh_OnKeyDownEvent;
            mywidth = this.Width;
            myheight = this.Height;

        }

        void kh_OnKeyDownEvent(object sender, KeyEventArgs e)

        {

            if (e.KeyData == (Keys.H | Keys.Control))
            {
                if (flag_hide == 0)
                {
                    this.Hide();
                    flag_hide = 1;
                }
                else
                {
                    this.Show();
                    flag_hide = 0;
                }

            }

            if (e.KeyData == (Keys.H ))
            {
                if (flag_puase == 0)
                {
                    go_continue();
                   
                }
                else
                {
                    pause();
                }
            }
            if (e.KeyData == (Keys.Q | Keys.Control)) { System.Environment.Exit(0); }//Ctrl+C 关闭窗口 


        }



        private void From1_paint(object sender, PaintEventArgs e)
        {

        }

        private void b1click(object sender, EventArgs e)
        {

            ////接受网络设置
            //server_r = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //server_r.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345));//绑定端口号和IP  
            //server_r.ReceiveTimeout = 1000000;
            //server_r.SendTimeout = 1000000;
            //EndPoint point2 = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345);
            //timer1.Start();
            //button1.BackColor = Color.White;
            //button1.Enabled = false;




            //while (true)
            //{


            //    //接受
            //    byte[] buffer = new byte[10240];
            //    int length = server_r.ReceiveFrom(buffer, ref point2);//接收数据报  
            //    string message = Encoding.UTF8.GetString(buffer, 0, length);
            //    if (message == "quit") { break; }

            //    string head = message.Substring(0, 2);
            //    string body = message.Substring(2);



            //    if (head == "tm")
            //    {
            //        currentSimTime = float.Parse(body);

            //    }
            //    else if (head == "va")
            //    {
            //        TAS = float.Parse(body);
            //        TAS = TAS * (float)3.6;


            //    }
            //    else
            //    {

            //        if (head == "ms")
            //        {



            //            string[] rawData = body.Split(';');
            //            string[] STData = rawData[0].Split(',');

            //            simtime = double.Parse(STData[0]);
            //            vvx = double.Parse(STData[1]);
            //            vvy = double.Parse(STData[2]);
            //            vvz = double.Parse(STData[3]);
            //            MAGyaw = double.Parse(STData[4]);
            //            altBar = double.Parse(STData[5]);
            //            altRad = double.Parse(STData[6]);
            //            VerticalVelocity = double.Parse(STData[7]);
            //            dVerticalVelocity = VerticalVelocity - oVerticalVelocity;
            //            oVerticalVelocity = VerticalVelocity;
            //            pitch = double.Parse(STData[8]);
            //            bank = double.Parse(STData[9]);
            //            yaw = double.Parse(STData[10]);
            //            lRPM = double.Parse(STData[11]);
            //            rRPM = double.Parse(STData[12]);
            //            bvx = (vvx * Math.Cos(yaw) + vvz * Math.Sin(yaw)) * 0.5 + obvx * 0.5;
            //            bvy = (-vvx * Math.Sin(yaw) + vvz * Math.Cos(yaw)) * 0.5 + obvy * 0.5;
            //            obvx = bvx;
            //            obvy = bvy;
            //            SBP = Math.Sin(Math.Atan2(obvy, obvx));
            //            dis_yaw = Convert.ToString((int)(MAGyaw / Math.PI * 180));
            //            //gradations_yaw = -(int)(((MAGyaw / Math.PI * 90) % 10) * 10);
            //        }
            //    }





            //    Application.DoEvents();



            //}
        }




        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {


        }
        private void progressBar1_Click(object sender, EventArgs e)
        {

        }

        private void button1_KeyDown(object sender, KeyEventArgs e)
        {

        }
    }
}
