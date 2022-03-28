namespace VSIXProjectTemplate
{
    partial class SetupForm
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
            this.Swagger = new System.Windows.Forms.CheckBox();
            this.Serilog = new System.Windows.Forms.CheckBox();
            this.EntityFramework = new System.Windows.Forms.CheckBox();
            this.HealthCheck = new System.Windows.Forms.CheckBox();
            this.button1 = new System.Windows.Forms.Button();
            this.RestSharp = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // Swagger
            // 
            this.Swagger.AutoSize = true;
            this.Swagger.Checked = true;
            this.Swagger.CheckState = System.Windows.Forms.CheckState.Checked;
            this.Swagger.Location = new System.Drawing.Point(112, 57);
            this.Swagger.Name = "Swagger";
            this.Swagger.Size = new System.Drawing.Size(90, 17);
            this.Swagger.TabIndex = 0;
            this.Swagger.Text = "Use Swagger";
            this.Swagger.UseVisualStyleBackColor = true;
            // 
            // Serilog
            // 
            this.Serilog.AutoSize = true;
            this.Serilog.Checked = true;
            this.Serilog.CheckState = System.Windows.Forms.CheckState.Checked;
            this.Serilog.Location = new System.Drawing.Point(112, 80);
            this.Serilog.Name = "Serilog";
            this.Serilog.Size = new System.Drawing.Size(80, 17);
            this.Serilog.TabIndex = 1;
            this.Serilog.Text = "Use Serilog";
            this.Serilog.UseVisualStyleBackColor = true;
            // 
            // EntityFramework
            // 
            this.EntityFramework.AutoSize = true;
            this.EntityFramework.Checked = true;
            this.EntityFramework.CheckState = System.Windows.Forms.CheckState.Checked;
            this.EntityFramework.Location = new System.Drawing.Point(112, 103);
            this.EntityFramework.Name = "EntityFramework";
            this.EntityFramework.Size = new System.Drawing.Size(129, 17);
            this.EntityFramework.TabIndex = 2;
            this.EntityFramework.Text = "Use Entity Framework";
            this.EntityFramework.UseVisualStyleBackColor = true;
            // 
            // HealthCheck
            // 
            this.HealthCheck.AutoSize = true;
            this.HealthCheck.Checked = true;
            this.HealthCheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this.HealthCheck.Location = new System.Drawing.Point(112, 126);
            this.HealthCheck.Name = "HealthCheck";
            this.HealthCheck.Size = new System.Drawing.Size(110, 17);
            this.HealthCheck.TabIndex = 3;
            this.HealthCheck.Text = "Use HealthCheck";
            this.HealthCheck.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(112, 176);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(177, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "Confirm";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // RestSharp
            // 
            this.RestSharp.AutoSize = true;
            this.RestSharp.Checked = true;
            this.RestSharp.CheckState = System.Windows.Forms.CheckState.Checked;
            this.RestSharp.Location = new System.Drawing.Point(112, 150);
            this.RestSharp.Name = "RestSharp";
            this.RestSharp.Size = new System.Drawing.Size(98, 17);
            this.RestSharp.TabIndex = 5;
            this.RestSharp.Text = "Use RestSharp";
            this.RestSharp.UseVisualStyleBackColor = true;
            // 
            // SetupForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.RestSharp);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.HealthCheck);
            this.Controls.Add(this.EntityFramework);
            this.Controls.Add(this.Serilog);
            this.Controls.Add(this.Swagger);
            this.Name = "SetupForm";
            this.Text = "SetupForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox Swagger;
        private System.Windows.Forms.CheckBox Serilog;
        private System.Windows.Forms.CheckBox EntityFramework;
        private System.Windows.Forms.CheckBox HealthCheck;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.CheckBox RestSharp;
    }
}