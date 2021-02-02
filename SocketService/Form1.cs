///佟志强 2019-10-01
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SocketService
{
    public partial class Form1 : Form
    {

        /// <summary>
        /// 以本机作测试,IP
        /// </summary>
        private IPAddress serverIP = IPAddress.Parse("127.0.0.1");


        /// <summary>
        /// 完整终端地址
        /// </summary>
        private IPEndPoint serverFullAddr;
        /// <summary>
        /// 连接套接字
        /// </summary>
        private static Socket socketWatch;
        /// <summary>
        /// 监听线程
        /// </summary>
        Thread myThead = null;
        //定义一个集合，存储客户端信息
        static Dictionary<string, Socket> ClientConnectionItems = new Dictionary<string, Socket> { };
        private static string ip11;
        //
        private static string messSend; 
        public Form1()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 启动服务器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnConn_Click(object sender, EventArgs e)
        {
            try
            {
                //IP
                serverIP = IPAddress.Parse(tbxIP.Text);
                //设置IP，端口
                serverFullAddr = new IPEndPoint(serverIP, int.Parse(tbxPort.Text));
               socketWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                //指定本地主机地址和端口号
                socketWatch.Bind(serverFullAddr);
                socketWatch.Listen(5);//设置监听频率
                myThead = new Thread(BeginListen);
                myThead.IsBackground = true;
                myThead.Start(socketWatch);
                btnStart.Enabled = false;
                btnstop.Enabled = true;
            }
            catch (Exception ex)
            {
                socketWatch.Close();
            }
          

        }
        /// <summary>
        /// 设置监听
        /// </summary>
        private void BeginListen(object o)
        {

            lbxMessage.Invoke(new SetTextCallback(SetText), "启动成功 时间:" + DateTime.Now, 1);
            byte[] message = new byte[1024];
            string mess = "";
            try
            {
                Socket socketWacth1 = o as Socket;
                while (true)
                {
                  Socket  newSocket = socketWacth1.Accept();//阻塞方式等待接收客户端连接
                    lbxMessage.Invoke(new SetTextCallback(SetText), newSocket.RemoteEndPoint.ToString() + ":连接成功！", 1);
                    //获取客户端的IP和端口
                  //  ip11 = newSocket.RemoteEndPoint.ToString();
                  //  ip11 = newSocket.RemoteEndPoint.AddressFamily.ToString();
                    //   mess = "已接收数据： "+ mess +" 来自：" +ip11+ " 当前时间为：" + DateTime.Now; //处理数据
                    newSocket.Send(Encoding.Default.GetBytes("ok"));//向客户端发送数据

                   
                  

                    //开启一个新线程，执行接收消息方法
                    Thread r_thread = new Thread(Received);
                     r_thread.IsBackground = true;
                     r_thread.Start(newSocket);
                }
            }
            catch (SocketException se)
            {
                lbxMessage.Invoke(new SetTextCallback(SetText), "监听异常", 1);

            }
        }

        /// <summary>
        /// 服务器端不停的接收客户端发来的消息
        /// </summary>
        /// <param name="o"></param>
        void Received(object o)
        {
            try
            {
                Socket socketSend = o as Socket;
                while (true)
                {
                    System.Threading.Thread.Sleep(500);
                    //客户端连接服务器成功后，服务器接收客户端发送的消息
                    byte[] buffer = new byte[1024 * 1024];
                    //实际接收到的有效字节数
                    int len = socketSend.Receive(buffer);
                    if (len == 0)
                    {
                        break;
                    }
                   
                    string mess = Encoding.Default.GetString(buffer, 0, len);
                    //传输数据中带有DTUID  获取出来存入列表
                    if (!ClientConnectionItems.Keys.Contains(mess))
                    {
                        //添加客户端信息  
                        ClientConnectionItems.Add(mess, socketSend);
                        //异步添加ip列表
                        dgv_ip.Invoke(new SetDataGridListAddCallback(SetDataGridListAdd), mess);
                    }
                    // bytes = newSocket.Receive(message);//接收数据

                    //  mess = Encoding.Default.GetString(message, 0, bytes);//对接收字节编码（S与C 两端编码格式必须一致不然中文乱码）（当接收的字节大于1024的时候 这应该是循环接收，测试就没有那样写了）
                    //do
                    //{
                    //    bytes = newSocket.Receive(message, message.Length, 0);
                    //    mess = mess + Encoding.ASCII.GetString(message, 0, bytes);
                    //}
                    //while (bytes > 0);

                    lbxMessage.Invoke(new SetTextCallback(SetText), mess, 1);//子线程操作主线程UI控件
                    if(messSend!=null && messSend!="")
                    {
                        socketSend.Send(Encoding.Default.GetBytes(messSend));
                        lbxMessage.Invoke(new SetTextCallback(SetText), "发送客户端信息成功", 1);//子线程操作主线程UI控件
                        lb_send.Text = "";
                        messSend = null;
                    }
                   
                }
            }
            catch { }
        }

        /// <summary>
        /// 服务器向客户端发送消息
        /// </summary>
        /// <param name="str"></param>
        void Send(string str)
        {
               
            try
            {
                    byte[] buffer = Encoding.UTF8.GetBytes(str);
                   ClientConnectionItems[ip11].Send(buffer);
                    lbxMessage.Invoke(new SetTextCallback(SetText), "发送客服端数据成功", 1);//子线程操作主线程UI控件
            }
            catch(Exception ex)
            {
                lbxMessage.Invoke(new SetTextCallback(SetText), "发送客服端数据错误！", 1);//子线程操作主线程UI控件
            }
        }


    #region//声名委托
    delegate void SetTextCallback(string text, int num);

        delegate void SetDataGridListAddCallback(string text);
        private void SetText(string text, int num)
        {
            lbxMessage.Items.Add(text);
        }

        private void SetDataGridListAdd(string text)
        {
            this.dgv_ip.ClearSelection();
            this.dgv_ip.Rows.Add(new object[] { text });
            this.dgv_ip.Update();
        }
        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {
            btnStart.Enabled = true;
            btnstop.Enabled = false;
            
        }
        //停止
        private void btnstop_Click(object sender, EventArgs e)
        {
            try
            {
                socketWatch.Close();
                //中止监听
                myThead.Abort();
                btnStart.Enabled = true;
                btnstop.Enabled = false;
                lbxMessage.Items.Add("停止成功 时间:" + DateTime.Now);
            }
            catch (Exception ee)
            {
                lbxMessage.Text = "停止失败。。" + ee;
            }
        }

        private void Sending(IAsyncResult rec_socket)
        {
            //发送给客户端的消息
            string sendmsg = "";
            Socket socket = (Socket)rec_socket.AsyncState;
            try
            {
                if (socket.Connected)
                {
                    byte[] msgBuff = Encoding.UTF8.GetBytes(sendmsg);
                    socket.Send(msgBuff);
                }
                else
                {
                    Console.WriteLine("Error!", "Error111111111111111111!");
                }
            }
            catch
            {
                Console.WriteLine("Error!", "Error!");
            }
        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void btn_send_Click(object sender, EventArgs e)
        {
            messSend = lb_send.Text.Trim();
            lb_send.Text = "";
        }

        private void dgv_ip_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            string str = dgv_ip.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
            messSend = str;
        }
    }
}
