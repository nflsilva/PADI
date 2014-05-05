namespace SlaveServer
{
    partial class SlaveUI
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
            this.startButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.slavePortLabel = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.slavePortBox = new System.Windows.Forms.TextBox();
            this.masterPortBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.serverIDBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // mainPanel
            // 
            this.mainPanel.Location = new System.Drawing.Point(12, 60);
            this.mainPanel.Multiline = true;
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.ReadOnly = true;
            this.mainPanel.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.mainPanel.Size = new System.Drawing.Size(441, 201);
            this.mainPanel.TabIndex = 0;
            // 
            // startButton
            // 
            this.startButton.Location = new System.Drawing.Point(378, 31);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(75, 23);
            this.startButton.TabIndex = 1;
            this.startButton.Text = "Start";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Slave Server UI";
            // 
            // slavePortLabel
            // 
            this.slavePortLabel.AutoSize = true;
            this.slavePortLabel.Location = new System.Drawing.Point(29, 36);
            this.slavePortLabel.Name = "slavePortLabel";
            this.slavePortLabel.Size = new System.Drawing.Size(66, 13);
            this.slavePortLabel.TabIndex = 3;
            this.slavePortLabel.Text = "Run on port:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(205, 36);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Master port:";
            // 
            // slavePortBox
            // 
            this.slavePortBox.Location = new System.Drawing.Point(101, 31);
            this.slavePortBox.Name = "slavePortBox";
            this.slavePortBox.Size = new System.Drawing.Size(98, 20);
            this.slavePortBox.TabIndex = 5;
            // 
            // masterPortBox
            // 
            this.masterPortBox.Location = new System.Drawing.Point(274, 33);
            this.masterPortBox.Name = "masterPortBox";
            this.masterPortBox.Size = new System.Drawing.Size(98, 20);
            this.masterPortBox.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(100, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(55, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Server ID:";
            // 
            // serverIDBox
            // 
            this.serverIDBox.Enabled = false;
            this.serverIDBox.Location = new System.Drawing.Point(161, 5);
            this.serverIDBox.Name = "serverIDBox";
            this.serverIDBox.Size = new System.Drawing.Size(38, 20);
            this.serverIDBox.TabIndex = 7;
            // 
            // SlaveUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(465, 273);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.serverIDBox);
            this.Controls.Add(this.masterPortBox);
            this.Controls.Add(this.slavePortBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.slavePortLabel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.startButton);
            this.Controls.Add(this.mainPanel);
            this.Name = "SlaveUI";
            this.Text = "Slave Interface";
            this.Load += new System.EventHandler(this.SlaveUI_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox mainPanel;
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label slavePortLabel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox slavePortBox;
        private System.Windows.Forms.TextBox masterPortBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox serverIDBox;
    }
}