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
using QQ2564874169.Miniblink;
using QQ2564874169.Miniblink.LoadResourceImpl;

namespace Demo
{
    public partial class FrmTest : MiniblinkForm
    {
        public FrmTest()
        {
            InitializeComponent();
        }

        private void FrmTest_Load(object sender, EventArgs e)
        {
            LoadUri("https://gitee.com");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var cookies = "";
            foreach (var item in Cookies)
            {
                cookies += $"{item.Domain}={item.Name}={item.Value}\r\n\r\n";
            }

            MessageBox.Show(cookies);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Cookies.Add(new Cookie("ckname","ckvalue")
            {
                HttpOnly = true,
                Secure = true,
                Expires = DateTime.Now.AddDays(1)
            });
        }
    }
}
