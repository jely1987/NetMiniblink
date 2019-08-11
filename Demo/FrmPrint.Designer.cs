namespace Demo
{
    partial class FrmPrint
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.printDialog1 = new System.Windows.Forms.PrintDialog();
            this.printDocument1 = new System.Drawing.Printing.PrintDocument();
            this.miniblinkBrowser1 = new QQ2564874169.Miniblink.MiniblinkBrowser();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.button1.Location = new System.Drawing.Point(0, 350);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(368, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "打印";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // printDialog1
            // 
            this.printDialog1.Document = this.printDocument1;
            this.printDialog1.UseEXDialog = true;
            // 
            // printDocument1
            // 
            this.printDocument1.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(this.printDocument1_PrintPage);
            // 
            // miniblinkBrowser1
            // 
            this.miniblinkBrowser1.BackColor = System.Drawing.Color.White;
            this.miniblinkBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.miniblinkBrowser1.Location = new System.Drawing.Point(0, 0);
            this.miniblinkBrowser1.Name = "miniblinkBrowser1";
            this.miniblinkBrowser1.Size = new System.Drawing.Size(368, 350);
            this.miniblinkBrowser1.TabIndex = 1;
            // 
            // FrmPrint
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(368, 373);
            this.Controls.Add(this.miniblinkBrowser1);
            this.Controls.Add(this.button1);
            this.Name = "FrmPrint";
            this.Text = "FrmPrint";
            this.Load += new System.EventHandler(this.FrmPrint_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.PrintDialog printDialog1;
        private System.Drawing.Printing.PrintDocument printDocument1;
        private QQ2564874169.Miniblink.MiniblinkBrowser miniblinkBrowser1;
    }
}