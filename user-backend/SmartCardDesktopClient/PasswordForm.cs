using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmartCardDesktopClient
{
    public partial class PasswordForm : Form
    {
        private readonly Logger _log;
        private readonly X509Certificate2 _xcer;
        private readonly SmartCardHandler _smartCardHandler;
        Task _task = null;
        CancellationTokenSource taskController = new CancellationTokenSource();

        public PasswordForm(Logger log, X509Certificate2 xcer, SmartCardHandler smartCardHandler)
        {
            _log = log;
            _xcer = xcer;
            _smartCardHandler = smartCardHandler;
            InitializeComponent();
        }
        ~PasswordForm()
        {
            if(_task != null)
            {
                taskController.Cancel();
                _task.Dispose();
                
            }
        }

        private void submitBtn_Click(object sender, EventArgs e)
        {
            if(_xcer == null)
            {
                Close();
                backBtn_Click(sender, e);               
                return;
            }

            string pinCode = textBox1.Text;
            _smartCardHandler.userPinCode = pinCode;
            Close();
            _smartCardHandler.SigningHubProcess();
        }

        private void backBtn_Click(object sender, EventArgs e)
        {
            Close();
            _smartCardHandler.PopupBallonTip("Choose Your Certificate");
            Task.Run(() =>
            {
                _smartCardHandler.ShowCertificateSelctionUI();
            });
        }

        private void OneClickShowPassword(object sender, MouseEventArgs e)
        {
            if (textBox1.PasswordChar == '*')
            {
                textBox1.PasswordChar = default;
            }
            else
            {
                textBox1.PasswordChar = '*';
            }
        }

        private void Form1_FormClosed(Object sender, FormClosedEventArgs e)
        {
            _smartCardHandler.timer.Enabled = true;
            _smartCardHandler.timer.Start();
        }

    }
}
