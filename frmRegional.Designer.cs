namespace FREQANAL
{
    partial class frmRegional
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
            this.layoutRegional = new System.Windows.Forms.TableLayoutPanel();
            this.pnlRegional = new System.Windows.Forms.Panel();
            this.layoutRegional.SuspendLayout();
            this.SuspendLayout();
            // 
            // layoutRegional
            // 
            this.layoutRegional.ColumnCount = 2;
            this.layoutRegional.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 77.83688F));
            this.layoutRegional.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 22.16312F));
            this.layoutRegional.Controls.Add(this.pnlRegional, 0, 0);
            this.layoutRegional.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutRegional.Location = new System.Drawing.Point(0, 0);
            this.layoutRegional.Name = "layoutRegional";
            this.layoutRegional.RowCount = 2;
            this.layoutRegional.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 91.38381F));
            this.layoutRegional.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 8.616188F));
            this.layoutRegional.Size = new System.Drawing.Size(704, 435);
            this.layoutRegional.TabIndex = 0;
            // 
            // pnlRegional
            // 
            this.pnlRegional.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRegional.Location = new System.Drawing.Point(3, 3);
            this.pnlRegional.Name = "pnlRegional";
            this.pnlRegional.Size = new System.Drawing.Size(541, 391);
            this.pnlRegional.TabIndex = 0;
            // 
            // frmRegional
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(704, 435);
            this.Controls.Add(this.layoutRegional);
            this.Name = "frmRegional";
            this.Text = "Regional Analysis";
            this.layoutRegional.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel layoutRegional;
        private System.Windows.Forms.Panel pnlRegional;
    }
}