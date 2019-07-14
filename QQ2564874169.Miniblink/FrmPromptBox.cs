using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace QQ2564874169.Miniblink
{
    public partial class FrmPromptBox : Form
    {
        private string _value;
        private bool _confirm;

        public FrmPromptBox(string title, string msg, string value)
        {
            InitializeComponent();
            btnYes.TabIndex = 0;
            Text = "来自 " + title;
            lbl.Text = msg ?? "";
            txt.Text = value ?? "";
        }

        private void btnYes_Click(object sender, EventArgs e)
        {
            _confirm = true;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _value = null;
            Close();
        }

        public string GetValue()
        {
            if (_confirm == false)
            {
                return null;
            }
            if (_value != null && _value.Trim().Length > 0)
                return _value;
            return null;
        }

        private void txt_TextChanged(object sender, EventArgs e)
        {
            _value = txt.Text;
        }
    }
}
