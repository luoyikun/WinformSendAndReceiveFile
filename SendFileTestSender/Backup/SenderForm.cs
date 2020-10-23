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
namespace SendFileTestSender
{
    public partial class SenderForm : Form
    {
        string FullFileName;
        FileSender task;
        Socket listensocket;
        Thread ListenThread;
        public SenderForm()
        {
            InitializeComponent();
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.toolStripStatusLabel2.Alignment = ToolStripItemAlignment.Right;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (button2.Text == "启动")
            {
                if (string.IsNullOrEmpty(textBox1.Text))
                    return;
                FullFileName = openFileDialog1.FileName;
                ListenThread = new Thread(BeginListen);
                ListenThread.IsBackground = true;
                ListenThread.Start();
                textBox1.Enabled = label1.Enabled = button1.Enabled = false;
                button2.Text = "停止";
            }
            else
            {
                ListenThread.Abort();
                listensocket.Close();
                textBox1.Enabled = label1.Enabled = button1.Enabled = true;
                button2.Text = "启动";
            }
        }

        private void BeginListen()
        {
            try
            {
                listensocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint ep = new IPEndPoint(IPAddress.Parse("192.168.1.2"), 3477);
                listensocket.Bind(ep);
                listensocket.Listen(20);
                while (true)
                {
                    Socket newsocket = listensocket.Accept();
                    //task = new FileTransmission(FileTransmission.TransmissionMode.Send);
                    task = new FileSender();
                    task.FullFileName = FullFileName;
                    task.Socket = newsocket;
                    task.EnabledIOBuffer = true;
                    task.BlockFinished += new BlockFinishedEventHandler(task_BlockFinished);
                    task.CommandReceived += new CommandReceivedEventHandler(task_CommandReceived);
                    task.ConnectLost += new EventHandler(task_ConnectLost);
                    task.ErrorOccurred += new FileTransmissionErrorOccurEventHandler(task_ErrorOccurred);
                    task.Start();
                }
            }
            catch (ThreadAbortException)
            {
                if (task != null && task.IsAlive)
                    task.Stop(true);
            }
        }

        void task_ErrorOccurred(object sender, FileTransmissionErrorOccurEventArgs e)
        {
            if (InvokeRequired)
                this.Invoke(new Delegate_String(s => this.listBox1.Items.Add(s)), e.InnerException.ToString());
            else
                this.listBox1.Items.Add(e.InnerException.ToString());
        }

        void task_BlockFinished(object sender, BlockFinishedEventArgs e)
        {
            FileTransmission task = (FileTransmission)sender;
            if (InvokeRequired)
                this.Invoke(new Delegate_Progress(SetProgress), task);
            else
                SetProgress(task);
        }
        delegate void Delegate_Progress(FileTransmission task);
        void SetProgress(FileTransmission task)
        {
            this.toolStripStatusLabel1.Text = string.Format("已发送:{0:N2}%", task.Progress);
            this.toolStripStatusLabel2.Text = string.Format("{0:N2}KB/s", task.KByteAverSpeed);
        }
        void task_ConnectLost(object sender, EventArgs e)
        {
            if (InvokeRequired)
                this.Invoke(new Delegate_String(s => this.listBox1.Items.Add(s)), "ConnectLost");
            else
                listBox1.Items.Add("ConnectLost");
        }

        public delegate void Delegate_String(string s);
        void task_CommandReceived(object sender, CommandReceivedEventArgs e)
        {
            if (InvokeRequired)
                this.Invoke(new Delegate_String(s => this.listBox1.Items.Add(s)), e.CommandLine);
            else
                listBox1.Items.Add(e.CommandLine);
        }
    }
}
