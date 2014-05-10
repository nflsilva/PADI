namespace MasterServer
{
    partial class MasterUI
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
            this.mainPanel = new System.Windows.Forms.TextBox();
            this.StartButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.portBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.maxPadIntBox = new System.Windows.Forms.TextBox();
            this.minPadIntBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // mainPanel
            // 
            this.mainPanel.Location = new System.Drawing.Point(16, 62);
            this.mainPanel.Multiline = true;
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.ReadOnly = true;
            this.mainPanel.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.mainPanel.Size = new System.Drawing.Size(450, 199);
            this.mainPanel.TabIndex = 0;
            // 
            // StartButton
            // 
            this.StartButton.Location = new System.Drawing.Point(391, 30);
            this.StartButton.Name = "StartButton";
            this.StartButton.Size = new System.Drawing.Size(75, 23);
            this.StartButton.TabIndex = 1;
            this.StartButton.Text = "Start";
            this.StartButton.UseVisualStyleBackColor = true;
            this.StartButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Master Server UI";
            // 
            // portBox
            // 
            this.portBox.Location = new System.Drawing.Point(271, 30);
            this.portBox.Name = "portBox";
            this.portBox.Size = new System.Drawing.Size(114, 20);
            this.portBox.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(199, 35);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(66, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Run on port:";
            // 
            // maxPadIntBox
            // 
            this.maxPadIntBox.Enabled = false;
            this.maxPadIntBox.Location = new System.Drawing.Point(156, 32);
            this.maxPadIntBox.Name = "maxPadIntBox";
            this.maxPadIntBox.Size = new System.Drawing.Size(37, 20);
            this.maxPadIntBox.TabIndex = 20;
            this.maxPadIntBox.TextChanged += new System.EventHandler(this.maxPadIntBox_TextChanged);
            // 
            // minPadIntBox
            // 
            this.minPadIntBox.Enabled = false;
            this.minPadIntBox.Location = new System.Drawing.Point(95, 32);
            this.minPadIntBox.Name = "minPadIntBox";
            this.minPadIntBox.Size = new System.Drawing.Size(37, 20);
            this.minPadIntBox.TabIndex = 19;
            this.minPadIntBox.TextChanged += new System.EventHandler(this.minPadIntBox_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(138, 36);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(10, 13);
            this.label5.TabIndex = 18;
            this.label5.Text = "-";
            this.label5.Click += new System.EventHandler(this.label5_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(26, 36);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(71, 13);
            this.label4.TabIndex = 17;
            this.label4.Text = "PadInt range:";
            this.label4.Click += new System.EventHandler(this.label4_Click);
            // 
            // MasterUI
            // 
            this.ClientSize = new System.Drawing.Size(478, 273);
            this.Controls.Add(this.maxPadIntBox);
            this.Controls.Add(this.minPadIntBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.portBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.StartButton);
            this.Controls.Add(this.mainPanel);
            this.Name = "MasterUI";
            this.Text = "Master Interface";
            this.Load += new System.EventHandler(this.MasterUI_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox mainPanel;
        private System.Windows.Forms.Button StartButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox portBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox maxPadIntBox;
        private System.Windows.Forms.TextBox minPadIntBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;

    }
}

