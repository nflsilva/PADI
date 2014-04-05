namespace SampleClientApp
{
    partial class AppUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AppUI));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.clientPortBox = new System.Windows.Forms.TextBox();
            this.masterPortBox = new System.Windows.Forms.TextBox();
            this.connectButton = new System.Windows.Forms.Button();
            this.createButton = new System.Windows.Forms.Button();
            this.dialogTextBox = new System.Windows.Forms.TextBox();
            this.valuesTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.accessIDBox = new System.Windows.Forms.TextBox();
            this.createIDBox = new System.Windows.Forms.TextBox();
            this.accessButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // clientPortBox
            // 
            resources.ApplyResources(this.clientPortBox, "clientPortBox");
            this.clientPortBox.Name = "clientPortBox";
            // 
            // masterPortBox
            // 
            resources.ApplyResources(this.masterPortBox, "masterPortBox");
            this.masterPortBox.Name = "masterPortBox";
            // 
            // connectButton
            // 
            resources.ApplyResources(this.connectButton, "connectButton");
            this.connectButton.Name = "connectButton";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // createButton
            // 
            resources.ApplyResources(this.createButton, "createButton");
            this.createButton.Name = "createButton";
            this.createButton.UseVisualStyleBackColor = true;
            this.createButton.Click += new System.EventHandler(this.createButton_Click);
            // 
            // dialogTextBox
            // 
            resources.ApplyResources(this.dialogTextBox, "dialogTextBox");
            this.dialogTextBox.Name = "dialogTextBox";
            this.dialogTextBox.ReadOnly = true;
            // 
            // valuesTextBox
            // 
            resources.ApplyResources(this.valuesTextBox, "valuesTextBox");
            this.valuesTextBox.Name = "valuesTextBox";
            this.valuesTextBox.ReadOnly = true;
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // accessIDBox
            // 
            resources.ApplyResources(this.accessIDBox, "accessIDBox");
            this.accessIDBox.Name = "accessIDBox";
            // 
            // createIDBox
            // 
            resources.ApplyResources(this.createIDBox, "createIDBox");
            this.createIDBox.Name = "createIDBox";
            // 
            // accessButton
            // 
            resources.ApplyResources(this.accessButton, "accessButton");
            this.accessButton.Name = "accessButton";
            this.accessButton.UseVisualStyleBackColor = true;
            // 
            // AppUI
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.accessButton);
            this.Controls.Add(this.createIDBox);
            this.Controls.Add(this.accessIDBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.valuesTextBox);
            this.Controls.Add(this.dialogTextBox);
            this.Controls.Add(this.createButton);
            this.Controls.Add(this.connectButton);
            this.Controls.Add(this.masterPortBox);
            this.Controls.Add(this.clientPortBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "AppUI";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox clientPortBox;
        private System.Windows.Forms.TextBox masterPortBox;
        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.Button createButton;
        private System.Windows.Forms.TextBox dialogTextBox;
        private System.Windows.Forms.TextBox valuesTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox accessIDBox;
        private System.Windows.Forms.TextBox createIDBox;
        private System.Windows.Forms.Button accessButton;
    }
}

