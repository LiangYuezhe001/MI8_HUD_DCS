using System;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Reflection;


namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        static Socket server_r;
        //接收socket
        static float currentSimTime, TAS;
        static double simtime, vvx, vvy, vvz, MAGyaw, altBar, altRad, bvx, bvy, obvx, obvy, oVerticalVelocity,dVerticalVelocity, VerticalVelocity, pitch, bank, yaw,SBP,lRPM,rRPM ,CB,SB,CTB,STB;
        static int flag=0;
        
        private void startup()
        {
            //接受网络设置
            server_r = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            server_r.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345));//绑定端口号和IP  
            server_r.ReceiveTimeout = 1000000;
            server_r.SendTimeout = 1000000;
            EndPoint point2 = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345);
            timer1.Start();
            button1.BackColor = Color.White;
            button1.Enabled = false;




            while (true)
            {

                
                //接受
                byte[] buffer = new byte[10240];
                int length = server_r.ReceiveFrom(buffer, ref point2);//接收数据报  
                string message = Encoding.UTF8.GetString(buffer, 0, length);
                if (message == "quit") { break; }

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
                        dVerticalVelocity = VerticalVelocity - oVerticalVelocity;
                        oVerticalVelocity = VerticalVelocity;
                        pitch = double.Parse(STData[8]);
                        bank = double.Parse(STData[9]);
                        yaw = double.Parse(STData[10]);
                        lRPM = double.Parse(STData[11]);
                        rRPM = double.Parse(STData[12]);
                        bvx = (vvx * Math.Cos(yaw) + vvz * Math.Sin(yaw)) * 0.5 + obvx * 0.5;
                        bvy = (-vvx * Math.Sin(yaw) + vvz * Math.Cos(yaw)) * 0.5 + obvy * 0.5;
                        obvx = bvx;
                        obvy = bvy;
                        SBP = Math.Sin(Math.Atan2(obvy, obvx));
                        dis_yaw = Convert.ToString((int)(MAGyaw / Math.PI * 180));
                        gradations_yaw = -(int)(((MAGyaw / Math.PI * 90) % 10) * 10);
                    }
                }





                Application.DoEvents();



            }




        }
        //private void recive_loop()
        //{
        //    while (true)
        //    {


        //        //接受
        //        byte[] buffer = new byte[10240];
        //        int length = server_r.ReceiveFrom(buffer, ref point2);//接收数据报  
        //        string message = Encoding.UTF8.GetString(buffer, 0, length);
        //        if (message == "quit") { break; }
        //        if (flag == 0) { break; }
        //        string head = message.Substring(0, 2);
        //        string body = message.Substring(2);



        //        if (head == "tm")
        //        {
        //            currentSimTime = float.Parse(body);

        //        }
        //        else if (head == "va")
        //        {
        //            TAS = float.Parse(body);
        //            TAS = TAS * (float)3.6;


        //        }
        //        else
        //        {

        //            if (head == "ms")
        //            {



        //                string[] rawData = body.Split(';');
        //                string[] STData = rawData[0].Split(',');

        //                simtime = double.Parse(STData[0]);
        //                vvx = double.Parse(STData[1]);
        //                vvy = double.Parse(STData[2]);
        //                vvz = double.Parse(STData[3]);
        //                MAGyaw = double.Parse(STData[4]);
        //                altBar = double.Parse(STData[5]);
        //                altRad = double.Parse(STData[6]);
        //                VerticalVelocity = double.Parse(STData[7]);
        //                dVerticalVelocity = VerticalVelocity - oVerticalVelocity;
        //                oVerticalVelocity = VerticalVelocity;
        //                pitch = double.Parse(STData[8]);
        //                bank = double.Parse(STData[9]);
        //                yaw = double.Parse(STData[10]);
        //                lRPM = double.Parse(STData[11]);
        //                rRPM = double.Parse(STData[12]);
        //                bvx = (vvx * Math.Cos(yaw) + vvz * Math.Sin(yaw)) * 0.5 + obvx * 0.5;
        //                bvy = (-vvx * Math.Sin(yaw) + vvz * Math.Cos(yaw)) * 0.5 + obvy * 0.5;
        //                obvx = bvx;
        //                obvy = bvy;
        //                SBP = Math.Sin(Math.Atan2(obvy, obvx));
        //                dis_yaw = Convert.ToString((int)(MAGyaw / Math.PI * 180));
        //                gradations_yaw = -(int)(((MAGyaw / Math.PI * 90) % 10) * 10);
        //            }
        //        }





        //        Application.DoEvents();



        //    }
        //}
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.H) //按下空格键
            {
                if (flag == 0)
                {
                    flag = 1;
                    startup();
                    //recive_loop();
                    timer1.Start();
                }
                else
                {
                    flag = 0;
                    timer1.Stop();
                    g.Clear(Color.White);
                    button1.BackColor = Color.LimeGreen;
                }
            }
            if (e.KeyCode == Keys.Q)
            {
                Application.ExitThread();
            }
                
        }

        private void button1_KeyDown(object sender, KeyEventArgs e)
        {
           
        }

        static string dis_yaw;
        static int  gradations_yaw;
        static Bitmap bmp = new Bitmap(500, 500);
        //static Bitmap bmp1 = new Bitmap(300, 300);
        
        Graphics g = Graphics.FromImage(bmp);
       // Graphics g1 = Graphics.FromImage(bmp1);
        
        Pen p = new Pen(Color.GreenYellow);
        Pen p2 = new Pen(Color.GreenYellow);

        // p.Width = 3;


        private void timer1_Tick(object sender, EventArgs e)
        {
            p.Width = 2;
            g.Clear(Color.White);
            //g1.Clear(Color.White);
            ////////////方向刻度/////////////////////////////
            if (gradations_yaw >= -50) {
                g.DrawLine(p, new Point(150 + gradations_yaw, 10), new Point(150 + gradations_yaw, 30));
            }
            if (gradations_yaw >= -75)
            {
                g.DrawLine(p, new Point(175 + gradations_yaw, 10), new Point(175 + gradations_yaw, 20));
            }
            if (gradations_yaw >= -25)
            {
                g.DrawLine(p, new Point(125 + gradations_yaw, 10), new Point(125 + gradations_yaw, 20));
            }

            if (gradations_yaw >= -25)
            {
                g.DrawLine(p, new Point(100 + gradations_yaw, 10), new Point(100 + gradations_yaw, 20));
            }

            if (gradations_yaw <= -25)
            {
                g.DrawLine(p, new Point(425 + gradations_yaw, 10), new Point(425 + gradations_yaw, 20));
            }
            if (gradations_yaw <= -50)
            {
                g.DrawLine(p, new Point(450 + gradations_yaw, 10), new Point(450 + gradations_yaw, 30));
            }

            if (gradations_yaw <= -75)
            {
                g.DrawLine(p, new Point(475 + gradations_yaw, 10), new Point(475 + gradations_yaw, 20));
            }

            if (gradations_yaw <= -75)
            {
                g.DrawLine(p, new Point(450 + gradations_yaw, 10), new Point(450 + gradations_yaw, 20));
            }

            g.DrawLine(p, new Point(200+ gradations_yaw, 10), new Point(200 + gradations_yaw , 20));
            g.DrawLine(p, new Point(225 + gradations_yaw, 10), new Point(225 + gradations_yaw, 20));
            g.DrawLine(p, new Point(250 + gradations_yaw, 10), new Point(250 + gradations_yaw, 30));
            g.DrawLine(p, new Point(275 + gradations_yaw, 10), new Point(275 + gradations_yaw, 20));
            g.DrawLine(p, new Point(300 + gradations_yaw, 10), new Point(300 + gradations_yaw , 20));
            g.DrawLine(p, new Point(325 + gradations_yaw, 10), new Point(325 + gradations_yaw, 20));
            g.DrawLine(p, new Point(350 + gradations_yaw, 10), new Point(350 + gradations_yaw, 30));
            g.DrawLine(p, new Point(375 + gradations_yaw, 10), new Point(375 + gradations_yaw, 20));
            g.DrawLine(p, new Point(400 + gradations_yaw, 10), new Point(400 + gradations_yaw , 20));

            if (MAGyaw <= Math.PI / 6)
            {
                g.DrawString("N", new Font("Arial", 10), Brushes.GreenYellow, new PointF(245 - (int)(MAGyaw / Math.PI * 900), 0));
            }
            if (MAGyaw >= Math.PI / 6 * 11)
            {
                g.DrawString("N", new Font("Arial", 10), Brushes.GreenYellow, new PointF(2045 - (int)(MAGyaw / Math.PI * 900), 0));
            }

            if (MAGyaw >= Math.PI / 3 && MAGyaw <= Math.PI / 6 * 4)
            {
                g.DrawString("E", new Font("Arial", 10), Brushes.GreenYellow, new PointF(695 - (int)(MAGyaw / Math.PI * 900), 0));
            }

            if (MAGyaw >= Math.PI / 6 * 5 && MAGyaw <= Math.PI / 6 * 7)
            {
                g.DrawString("S", new Font("Arial", 10), Brushes.GreenYellow, new PointF(1145 - (int)(MAGyaw / Math.PI * 900), 0));
            }

            if (MAGyaw >= Math.PI / 6 * 8 && MAGyaw <= Math.PI / 6 * 10)
            {
                g.DrawString("W", new Font("Arial", 10), Brushes.GreenYellow, new PointF(1595 - (int)(MAGyaw / Math.PI * 900), 0));
            }

            g.DrawString(dis_yaw, new Font("Arial", 15), Brushes.GreenYellow, new PointF(235, 30));

            g.DrawRectangle(p, 247, 10, 6,20);

            ////////////////////////////////////


            /////////////////速度矢量/////////////////////////
            ///
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

            if (TAS <= 50) {
                g.DrawLine(p, new Point(250, 250), new Point(250 + pix_bvy, 250 - pix_bvx));
                // g.DrawEllipse(p, 247, 247, 6, 6);
                g.DrawEllipse(p, 247 + pix_bvy, 247 - pix_bvx, 6, 6);
                }
            ////////////////////////////////////////////////////

            /////////////速度///////////////////////
            g.DrawString(Convert.ToString((int)TAS), new Font("Arial", 14), Brushes.GreenYellow, new PointF(66, 78));
            if (TAS <= 50)
            {
                g.DrawRectangle(p, 70, 100, 10, 300);
                p2.Width = 10;
                g.DrawLine(p2, new Point(75, 400), new Point(75, 400 - (int)(TAS * 6)));
                g.DrawString("50", new Font("Arial", 10), Brushes.GreenYellow, new PointF(50, 90));

            }
            else
            {
                g.DrawRectangle(p, 70, 100, 10, 300);
                p2.Width = 10;
                g.DrawLine(p2, new Point(75, 400), new Point(75, 400 - (int)(TAS)));
                g.DrawString("300", new Font("Arial", 10), Brushes.GreenYellow, new PointF(40, 90));
            }
            //////////////////////////////////////////
            ////////////////////////RPM/////////////
            g.DrawString(Convert.ToString((int)(lRPM)), new Font("Arial", 10), Brushes.GreenYellow, new PointF(60, 410));
            g.DrawString(Convert.ToString((int)(rRPM)), new Font("Arial", 10), Brushes.GreenYellow, new PointF(80, 410));
            g.DrawLine(p, new Point(69, 400), new Point(69, 400 - (int)((rRPM-70) * 10)));
            g.DrawLine(p, new Point(67, 400), new Point(67, 400 - (int)((lRPM - 70) * 10)));
            ///////////////////////////////////////////////////////////
            /////////////高度///////////////////////////////////
            g.DrawString(Convert.ToString((int)altBar)+"B", new Font("Arial", 14), Brushes.GreenYellow, new PointF(412, 58));
            g.DrawString(Convert.ToString((int)altRad)+"R", new Font("Arial", 14), Brushes.GreenYellow, new PointF(412, 78)); 
            

            g.DrawLine(p, new Point(400, 100), new Point(430, 100));
            g.DrawLine(p, new Point(400, 150), new Point(430, 150));
            g.DrawLine(p, new Point(410, 170), new Point(420, 170));
            g.DrawLine(p, new Point(410, 190), new Point(420, 190));
            g.DrawLine(p, new Point(410, 210), new Point(420, 210));
            g.DrawLine(p, new Point(410, 230), new Point(420, 230));
            g.DrawLine(p, new Point(400, 250), new Point(420, 250));
            g.DrawLine(p, new Point(400, 250), new Point(430, 250));
            g.DrawLine(p, new Point(410, 270), new Point(420, 270));
            g.DrawLine(p, new Point(410, 290), new Point(420, 290));
            g.DrawLine(p, new Point(410, 310), new Point(420, 310));
            g.DrawLine(p, new Point(410, 330), new Point(420, 330));
            g.DrawLine(p, new Point(400, 350), new Point(430, 350));
            g.DrawLine(p, new Point(400, 400), new Point(430, 400));
            g.DrawLine(p, new Point(398, 244 - (int)(VerticalVelocity*20)), new Point(398, 256 - (int)(VerticalVelocity * 20)));
            g.DrawLine(p, new Point(398, 244 - (int)(VerticalVelocity * 20)), new Point(410, 250 - (int)(VerticalVelocity * 20)));
            g.DrawLine(p, new Point(398, 256 - (int)(VerticalVelocity * 20)), new Point(410, 250 - (int)(VerticalVelocity * 20)));
            g.DrawLine(p2, new Point(435, 250), new Point(435, 250 - (int)(dVerticalVelocity * 4000)));
            if (altRad <= 50)
            {
                g.DrawRectangle(p, 420, 100, 10, 300);
                p2.Width = 10;
                g.DrawLine(p2, new Point(425, 400), new Point(425, 400 - (int)(altRad * 6)));
                g.DrawString("50", new Font("Arial", 10), Brushes.GreenYellow, new PointF(440, 90));
            }
            else
            {
                g.DrawRectangle(p, 420, 100, 10, 300);
                p2.Width = 10;
                g.DrawLine(p2, new Point(425, 400), new Point(425, 400 - (int)(altRad*6/20)));
                g.DrawString("1000", new Font("Arial", 10), Brushes.GreenYellow, new PointF(440, 90));
            }

            if (TAS <= 50 && VerticalVelocity <= -4)
            {
                g.DrawString("VRS", new Font("Arial", 30), Brushes.GreenYellow, new PointF(200, 150));
                g.DrawRectangle(p, 200, 154, 100, 35);
            }
            //////////////////////////////////////////
            ///

            ///////////////////SBP////////////////////////
            
            g.DrawLine(p, new Point(150, 420), new Point(350, 420));
            g.DrawRectangle(p, 235, 400, 20, 20);
            g.DrawEllipse(p, 235+ (int)(SBP * 100), 400, 20, 20);
            ////////////////////////////////////////
            ///
            ////////////////ADI/////////////////////////
            g.DrawLine(p, new Point(245, 250), new Point(255, 250));
            g.DrawLine(p, new Point(250, 245), new Point(250, 255));

            g.DrawLine(p, new Point(180, 250), new Point(230, 250));
            g.DrawLine(p, new Point(230, 250), new Point(230, 260));

            g.DrawLine(p, new Point(270, 250), new Point(320, 250));
            g.DrawLine(p, new Point(270, 250), new Point(270, 260));

            CB = Math.Cos(bank);
            SB = Math.Sin(bank);
            CTB = Math.Cos(Math.PI / 2 + bank);
            STB = Math.Sin(Math.PI / 2 + bank);
            //CTB = 0;
            //STB = 0;
            if (TAS >= 25 && TAS <= 50)
            {
                g.DrawLine(p, new Point(250 - ((int)(100 * CB)), 250 + ((int)(100 * SB)) + ((int)(pitch / Math.PI * 900))), new Point(250 + ((int)(100 * CB)), 250 - ((int)(100 * SB)) + ((int)(pitch / Math.PI * 900))));

            }
            if (TAS > 50)
            {
                g.DrawLine(p, new Point(250 - ((int)(100 * CB)), 250 + ((int)(100 * SB)) + ((int)(pitch / Math.PI * 900))), new Point(250 + ((int)(100 * CB)), 250 - ((int)(100 * SB)) + ((int)(pitch / Math.PI * 900))));
                g.DrawLine(p, new Point(250 - ((int)(50 * CB)) + ((int)(50 * CTB)), 250 - ((int)(50 * STB)) + ((int)(50 * SB)) + ((int)(pitch / Math.PI * 900))), new Point(250 + ((int)(50 * CB)) + ((int)(50 * CTB)), 250 - ((int)(50 * STB)) - ((int)(50 * SB)) + ((int)(pitch / Math.PI * 900))));
                g.DrawLine(p, new Point(250 - ((int)(50 * CB)) - ((int)(50 * CTB)), 250 + ((int)(50 * STB)) + ((int)(50 * SB)) + ((int)(pitch / Math.PI * 900))), new Point(250 + ((int)(50 * CB)) - ((int)(50 * CTB)), 250 + ((int)(50 * STB)) - ((int)(50 * SB)) + ((int)(pitch / Math.PI * 900))));
            }
            ////////////////////////////////////
            ///
            this.CreateGraphics().DrawImage(bmp, 0, 0);
            //this.CreateGraphics().DrawImage(bmp1, 100, 100);
            

        }

        int pix_bvx, pix_bvy;


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this .BackColor =Color.White;

            this .TransparencyKey = Color.White;
            
          
        }
       


        private void From1_paint(object sender, PaintEventArgs e)
        {
            
        }
        
        private void b1click(object sender, EventArgs e)
        {
          
            //接受网络设置
            server_r = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            server_r.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345));//绑定端口号和IP  
            server_r.ReceiveTimeout = 1000000;
            server_r.SendTimeout = 1000000;
            EndPoint point2 = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345);
            timer1.Start();
            button1.BackColor = Color.White;
            button1.Enabled = false;




            while (true)
            {
                

                //接受
                byte[] buffer = new byte[10240];
                int length = server_r.ReceiveFrom(buffer, ref point2);//接收数据报  
                string message = Encoding.UTF8.GetString(buffer, 0, length);
                if (message == "quit") { break; }
                
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
                        dVerticalVelocity = VerticalVelocity - oVerticalVelocity;
                        oVerticalVelocity = VerticalVelocity;
                        pitch = double.Parse(STData[8]);
                        bank = double.Parse(STData[9]);
                        yaw = double.Parse(STData[10]);
                        lRPM= double.Parse(STData[11]);
                        rRPM = double.Parse(STData[12]);
                        bvx = (vvx * Math.Cos(yaw) + vvz * Math.Sin(yaw)) * 0.5 + obvx * 0.5;
                        bvy = (-vvx * Math.Sin(yaw) + vvz * Math.Cos(yaw)) * 0.5 + obvy * 0.5;
                        obvx = bvx;
                        obvy = bvy;
                        SBP = Math.Sin(Math.Atan2(obvy, obvx));
                        dis_yaw = Convert.ToString((int)(MAGyaw/Math.PI*180));
                        gradations_yaw = -(int)(((MAGyaw / Math.PI * 90) % 10) * 10);
                    }
                }
                

                

                
                Application.DoEvents();
               


            }
        }
        

       

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }
    }
}
