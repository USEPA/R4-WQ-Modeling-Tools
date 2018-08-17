namespace FREQANAL
{
    partial class frmWeb
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
            this.webUSGS = new System.Windows.Forms.WebBrowser();
            this.SuspendLayout();
            // 
            // webUSGS
            // 
            this.webUSGS.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webUSGS.Location = new System.Drawing.Point(0, 0);
            this.webUSGS.MinimumSize = new System.Drawing.Size(20, 20);
            this.webUSGS.Name = "webUSGS";
            this.webUSGS.Size = new System.Drawing.Size(587, 362);
            this.webUSGS.TabIndex = 0;
            // 
            // frmWeb
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(587, 362);
            this.Controls.Add(this.webUSGS);
            this.Name = "frmWeb";
            this.Text = "USGS NWIS";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmWeb_FormClosed);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser webUSGS;
    }
}