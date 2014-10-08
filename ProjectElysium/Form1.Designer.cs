namespace ProjectElysium
{
    partial class Form1
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.txtMain = new System.Windows.Forms.RichTextBox();
            this.txtNum = new System.Windows.Forms.RichTextBox();
            this.txtCMD = new System.Windows.Forms.TextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.timer2 = new System.Windows.Forms.Timer(this.components);
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.SuspendLayout();
            // 
            // txtMain
            // 
            this.txtMain.BackColor = System.Drawing.Color.Black;
            this.txtMain.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtMain.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtMain.ForeColor = System.Drawing.SystemColors.ActiveCaption;
            this.txtMain.Location = new System.Drawing.Point(84, 1);
            this.txtMain.Name = "txtMain";
            this.txtMain.ReadOnly = true;
            this.txtMain.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.txtMain.Size = new System.Drawing.Size(861, 496);
            this.txtMain.TabIndex = 0;
            this.txtMain.Text = "";
            // 
            // txtNum
            // 
            this.txtNum.BackColor = System.Drawing.Color.Black;
            this.txtNum.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtNum.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtNum.ForeColor = System.Drawing.SystemColors.ActiveCaption;
            this.txtNum.Location = new System.Drawing.Point(0, 1);
            this.txtNum.Name = "txtNum";
            this.txtNum.ReadOnly = true;
            this.txtNum.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.txtNum.Size = new System.Drawing.Size(80, 496);
            this.txtNum.TabIndex = 2;
            this.txtNum.Text = "";
            // 
            // txtCMD
            // 
            this.txtCMD.BackColor = System.Drawing.Color.Black;
            this.txtCMD.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtCMD.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCMD.ForeColor = System.Drawing.SystemColors.ActiveCaption;
            this.txtCMD.Location = new System.Drawing.Point(0, 504);
            this.txtCMD.Name = "txtCMD";
            this.txtCMD.Size = new System.Drawing.Size(945, 14);
            this.txtCMD.TabIndex = 1;
            this.txtCMD.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtCMD_KeyDown);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 30000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "Show Elysium";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseClick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(945, 520);
            this.Controls.Add(this.txtCMD);
            this.Controls.Add(this.txtNum);
            this.Controls.Add(this.txtMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.Opacity = 0.92D;
            this.Text = "Elysium";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox txtMain;
        private System.Windows.Forms.RichTextBox txtNum;
        private System.Windows.Forms.TextBox txtCMD;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Timer timer2;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
    }
}

