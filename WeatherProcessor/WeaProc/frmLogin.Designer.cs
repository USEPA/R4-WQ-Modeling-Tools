namespace NCEIData
{
    partial class frmLogin
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
            lblUsername = new System.Windows.Forms.Label();
            lblPassword = new System.Windows.Forms.Label();
            txtUsername = new System.Windows.Forms.TextBox();
            txtPassword = new System.Windows.Forms.TextBox();
            btnOK = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            lblAuthFileDirectory = new System.Windows.Forms.Label();
            txtAuthFilesDir = new System.Windows.Forms.TextBox();
            btnSelectDir = new System.Windows.Forms.Button();
            btnLoginPage = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // lblUsername
            // 
            lblUsername.AutoSize = true;
            lblUsername.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            lblUsername.Location = new System.Drawing.Point(58, 70);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new System.Drawing.Size(68, 15);
            lblUsername.TabIndex = 0;
            lblUsername.Text = "User Name:";
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            lblPassword.Location = new System.Drawing.Point(58, 103);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new System.Drawing.Size(60, 15);
            lblPassword.TabIndex = 1;
            lblPassword.Text = "Password:";
            // 
            // txtUsername
            // 
            txtUsername.Location = new System.Drawing.Point(132, 67);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new System.Drawing.Size(220, 23);
            txtUsername.TabIndex = 2;
            // 
            // txtPassword
            // 
            txtPassword.Location = new System.Drawing.Point(132, 100);
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new System.Drawing.Size(220, 23);
            txtPassword.TabIndex = 3;            
            // 
            // btnOK
            // 
            btnOK.Location = new System.Drawing.Point(96, 253);
            btnOK.Name = "btnOK";
            btnOK.Size = new System.Drawing.Size(75, 23);
            btnOK.TabIndex = 4;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new System.Drawing.Point(215, 253);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(75, 23);
            btnCancel.TabIndex = 5;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // lblAuthFileDirectory
            // 
            lblAuthFileDirectory.AutoSize = true;
            lblAuthFileDirectory.Location = new System.Drawing.Point(58, 40);
            lblAuthFileDirectory.Name = "lblAuthFileDirectory";
            lblAuthFileDirectory.Size = new System.Drawing.Size(113, 15);
            lblAuthFileDirectory.TabIndex = 6;
            lblAuthFileDirectory.Text = "Auth Files Directory:";
            // 
            // txtAuthFilesDir
            // 
            txtAuthFilesDir.Location = new System.Drawing.Point(177, 37);
            txtAuthFilesDir.Name = "txtAuthFilesDir";
            txtAuthFilesDir.Size = new System.Drawing.Size(175, 23);
            txtAuthFilesDir.TabIndex = 7;            
            // 
            // btnSelectDir
            // 
            btnSelectDir.FlatAppearance.BorderColor = System.Drawing.Color.White;
            btnSelectDir.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btnSelectDir.Location = new System.Drawing.Point(358, 37);
            btnSelectDir.Name = "btnSelectDir";
            btnSelectDir.Size = new System.Drawing.Size(33, 27);
            btnSelectDir.TabIndex = 8;
            btnSelectDir.Text = "...";
            btnSelectDir.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            btnSelectDir.UseVisualStyleBackColor = true;
            btnSelectDir.Click += btnSelectDir_Click;
            // 
            // btnLoginPage
            // 
            btnLoginPage.Location = new System.Drawing.Point(115, 153);
            btnLoginPage.Name = "btnLoginPage";
            btnLoginPage.Size = new System.Drawing.Size(121, 23);
            btnLoginPage.TabIndex = 9;
            btnLoginPage.Text = "Launch Login Page";
            btnLoginPage.UseVisualStyleBackColor = true;
            btnLoginPage.Click += btnLoginPage_Click;
            // 
            // frmLogin
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(436, 320);
            Controls.Add(btnLoginPage);
            Controls.Add(btnSelectDir);
            Controls.Add(txtAuthFilesDir);
            Controls.Add(lblAuthFileDirectory);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(txtPassword);
            Controls.Add(txtUsername);
            Controls.Add(lblPassword);
            Controls.Add(lblUsername);
            Name = "frmLogin";
            Text = "NASA Earthdata Login";
            Load += frmLogin_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblUsername;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblAuthFileDirectory;
        private System.Windows.Forms.TextBox txtAuthFilesDir;
        private System.Windows.Forms.Button btnSelectDir;
        private System.Windows.Forms.Button btnLoginPage;
    }
}