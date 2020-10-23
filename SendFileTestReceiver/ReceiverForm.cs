using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Takamachi660.FileTransmissionSolution;
namespace SendFileTestReceiver
{
    public partial class ReceiverForm : Form
    {
        Socket listensocket;
     
        public ReceiverForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //启动服务器
            try
            {
                listensocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint ep = new IPEndPoint(IPAddress.Parse("192.168.8.163"), 3477);
                listensocket.Bind(ep);
                listensocket.Listen(20);

                listensocket.BeginAccept(AcceptCB, null); //开始监听客户端的连接
            }
            catch (ThreadAbortException)
            {
                
            }

            //IPEndPoint ep = new IPEndPoint(IPAddress.Parse("192.168.8.163"), 3477);
            //Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //socket.Connect(ep);
            //FileReceiver task = new FileReceiver();
            //task.Socket = socket;
            //task.EnabledIOBuffer = true;
            //task.BlockFinished += new BlockFinishedEventHandler(task_BlockFinished);
            //task.ConnectLost += new EventHandler(task_ConnectLost);
            //task.AllFinished += new EventHandler(task_AllFinished);
            //task.BlockHashed += new BlockFinishedEventHandler(task_BlockHashed);
            //task.ErrorOccurred += new FileTransmissionErrorOccurEventHandler(task_ErrorOccurred);
            ////task.FilePath = @"A:";
            //task.Start();
        }

        private void AcceptCB(IAsyncResult ar)
        {
            
            
                Socket socket = listensocket.EndAccept(ar);
            FileReceiver task = new FileReceiver();
            task.Socket = socket;
            task.EnabledIOBuffer = true;
            task.BlockFinished += new BlockFinishedEventHandler(task_BlockFinished);
            task.ConnectLost += new EventHandler(task_ConnectLost);
            task.AllFinished += new EventHandler(task_AllFinished);
            task.BlockHashed += new BlockFinishedEventHandler(task_BlockHashed);
            task.ErrorOccurred += new FileTransmissionErrorOccurEventHandler(task_ErrorOccurred);
            //task.FilePath = @"A:";
            task.Start();
            listensocket.BeginAccept(AcceptCB, null);
        }


        void task_ErrorOccurred(object sender, FileTransmissionErrorOccurEventArgs e)
        {
            if (e.InnerException is IOException)
            {
                if (MessageBox.Show(e.InnerException.Message, "IO异常", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == DialogResult.Cancel)
                {
                    e.Continue = false;
                    Application.Exit();
                }
                else
                    e.Continue = true;
            }
            else
                MessageBox.Show(e.InnerException.ToString(), "异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        void task_AllFinished(object sender, EventArgs e)
        {
            MessageBox.Show("完成", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        void task_ConnectLost(object sender, EventArgs e)
        {
            MessageBox.Show("连接中断", "失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            this.Close();
        }

        void task_BlockFinished(object sender, BlockFinishedEventArgs e)
        {
            FileTransmission task = (FileTransmission)sender;
            if (InvokeRequired)
                this.Invoke(new Delegate_Progress(SetProgress), task);
            else
                SetProgress(task);
        }
        void task_BlockHashed(object sender, BlockFinishedEventArgs e)
        {
            FileTransmission task = (FileTransmission)sender;
            if (InvokeRequired)
            {
                this.Invoke(new Delegate_Void_Bool(b => this.Text = "接收端 校验中"), false);
                this.Invoke(new Delegate_Progress(SetProgressBar), task);
            }
            else
            {
                this.Text = "接收端 校验中";
                SetProgressBar(task);
            }
        }
        delegate void Delegate_Progress(FileTransmission task);
        void SetProgressBar(FileTransmission task)
        {
            this.progressBar1.Maximum = task.Blocks.Count;
            this.progressBar1.Value = task.Blocks.CountValid;
        }
        void SetProgress(FileTransmission task)
        {
            this.Text = "接收端 下载中";
            SetProgressBar(task);
            this.label3.Text = string.Format("进度:{0:N2}%   总长度:{1}   已完成:{2}", task.Progress, task.TotalSize, task.FinishedSize);
            this.label1.Text = string.Format("平均速度:{0:N2}KB/s   已用时:{1}   估计剩余时间:{2}", task.KByteAverSpeed, task.TimePast, task.TimeRemaining);
            this.label2.Text = string.Format("瞬时速度:{0:N2}KB/s   丢弃的区块:{1}", task.KByteSpeed, task.Blocks.Cast.Count);
        }
    }
}
