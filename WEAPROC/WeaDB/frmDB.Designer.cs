namespace WeaSDB
{
    partial class frmDB
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
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtWDM = new System.Windows.Forms.TextBox();
            this.txtSDB = new System.Windows.Forms.TextBox();
            this.btnWDM = new System.Windows.Forms.Button();
            this.btnSDB = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 13);
            this.label1.TabIndex = 13;
            this.label1.Text = "WDM Database:";
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 52);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(60, 13);
            this.label3.TabIndex = 14;
            this.label3.Text = "SQLite DB:";
            // 
            // txtWDM
            // 
            this.txtWDM.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtWDM.Location = new System.Drawing.Point(105, 20);
            this.txtWDM.Name = "txtWDM";
            this.txtWDM.Size = new System.Drawing.Size(420, 20);
            this.txtWDM.TabIndex = 15;
            // 
            // txtSDB
            // 
            this.txtSDB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSDB.Location = new System.Drawing.Point(105, 49);
            this.txtSDB.Name = "txtSDB";
            this.txtSDB.Size = new System.Drawing.Size(420, 20);
            this.txtSDB.TabIndex = 16;
            // 
            // btnWDM
            // 
            this.btnWDM.Image = global::WeaSDB.Properties.Resources.openfolderhs;
            this.btnWDM.Location = new System.Drawing.Point(531, 20);
            this.btnWDM.Name = "btnWDM";
            this.btnWDM.Size = new System.Drawing.Size(31, 23);
            this.btnWDM.TabIndex = 19;
            this.btnWDM.UseVisualStyleBackColor = true;
            this.btnWDM.Click += new System.EventHandler(this.btnWDM_Click);
            // 
            // btnSDB
            // 
            this.btnSDB.Image = global::WeaSDB.Properties.Resources.openfolderhs;
            this.btnSDB.Location = new System.Drawing.Point(532, 46);
            this.btnSDB.Name = "btnSDB";
            this.btnSDB.Size = new System.Drawing.Size(31, 23);
            this.btnSDB.TabIndex = 20;
            this.btnSDB.UseVisualStyleBackColor = true;
            this.btnSDB.Click += new System.EventHandler(this.btnSDB_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(406, 87);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 21;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(487, 87);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 22;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // frmDB
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(567, 119);
            this.ControlBox = false;
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSDB);
            this.Controls.Add(this.btnWDM);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtWDM);
            this.Controls.Add(this.txtSDB);
            this.Name = "frmDB";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Select Databases ...";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtWDM;
        private System.Windows.Forms.TextBox txtSDB;
        private System.Windows.Forms.Button btnWDM;
        private System.Windows.Forms.Button btnSDB;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
    }
}