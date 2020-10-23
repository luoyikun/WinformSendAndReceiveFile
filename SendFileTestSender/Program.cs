using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
namespace SendFileTestSender
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.ThreadException += HandleUnknownError;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SenderForm());
        }
        static void HandleUnknownError(object sender, ThreadExceptionEventArgs e)
        {
            MessageBox.Show("出现意外的错误:\n" + e.Exception.Message, "程序即将关闭", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }
    }
}
