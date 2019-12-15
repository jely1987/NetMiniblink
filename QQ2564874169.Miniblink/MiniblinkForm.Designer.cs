using System.ComponentModel;

namespace QQ2564874169.Miniblink
{
	partial class MiniblinkForm
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
            this._browser = new QQ2564874169.Miniblink.MiniblinkBrowser();
            this.SuspendLayout();
            // 
            // _browser
            // 
            this._browser.BackColor = System.Drawing.Color.White;
            this._browser.CspCheckEnable = false;
            this._browser.Dock = System.Windows.Forms.DockStyle.Fill;
            this._browser.FireDropFile = false;
            this._browser.HeadlessEnabled = true;
            this._browser.Location = new System.Drawing.Point(0, 0);
            this._browser.MemoryCacheEnable = true;
            this._browser.MouseEnabled = true;
            this._browser.Name = "_browser";
            this._browser.NpapiPluginsEnable = true;
            this._browser.ResourceCache = null;
            this._browser.Size = new System.Drawing.Size(429, 335);
            this._browser.TabIndex = 0;
            this._browser.TouchEnabled = false;
            // 
            // MiniblinkForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(429, 335);
            this.Controls.Add(this._browser);
            this.KeyPreview = true;
            this.Name = "MiniblinkForm";
            this.Text = "MiniblinkForm";
            this.Load += new System.EventHandler(this.MiniblinkForm_Load);
            this.ResumeLayout(false);

		}

		#endregion

		private MiniblinkBrowser _browser;
	}
}