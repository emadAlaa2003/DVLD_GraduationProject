namespace DVLD.QuestionBank
{
    partial class frmGenerateQuestions
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmGenerateQuestions));
            this.lblTitle = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.cmbDocuments = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblTotalQuestions = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.nudTrueFalse = new System.Windows.Forms.NumericUpDown();
            this.nudMultipleChoice = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pbPersonImage = new System.Windows.Forms.PictureBox();
            this.panel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudTrueFalse)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMultipleChoice)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbPersonImage)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            resources.ApplyResources(this.lblTitle, "lblTitle");
            this.lblTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(61)))), ((int)(((byte)(125)))));
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Tag = " ";
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Controls.Add(this.cmbDocuments);
            this.panel1.Controls.Add(this.label1);
            this.panel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(61)))), ((int)(((byte)(125)))));
            this.panel1.Name = "panel1";
            // 
            // cmbDocuments
            // 
            resources.ApplyResources(this.cmbDocuments, "cmbDocuments");
            this.cmbDocuments.BackColor = System.Drawing.Color.White;
            this.cmbDocuments.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(61)))), ((int)(((byte)(125)))));
            this.cmbDocuments.FormattingEnabled = true;
            this.cmbDocuments.Name = "cmbDocuments";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // groupBox1
            // 
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Controls.Add(this.lblTotalQuestions);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.nudTrueFalse);
            this.groupBox1.Controls.Add(this.nudMultipleChoice);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(61)))), ((int)(((byte)(125)))));
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // lblTotalQuestions
            // 
            resources.ApplyResources(this.lblTotalQuestions, "lblTotalQuestions");
            this.lblTotalQuestions.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.lblTotalQuestions.Name = "lblTotalQuestions";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // nudTrueFalse
            // 
            resources.ApplyResources(this.nudTrueFalse, "nudTrueFalse");
            this.nudTrueFalse.BackColor = System.Drawing.Color.White;
            this.nudTrueFalse.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(61)))), ((int)(((byte)(125)))));
            this.nudTrueFalse.Name = "nudTrueFalse";
            this.nudTrueFalse.ValueChanged += new System.EventHandler(this.nudTrueFalse_ValueChanged);
            // 
            // nudMultipleChoice
            // 
            resources.ApplyResources(this.nudMultipleChoice, "nudMultipleChoice");
            this.nudMultipleChoice.BackColor = System.Drawing.Color.White;
            this.nudMultipleChoice.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(61)))), ((int)(((byte)(125)))));
            this.nudMultipleChoice.Name = "nudMultipleChoice";
            this.nudMultipleChoice.ValueChanged += new System.EventHandler(this.nudMultipleChoice_ValueChanged);
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // btnClose
            // 
            resources.ApplyResources(this.btnClose, "btnClose");
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Image = global::DVLD.Properties.Resources.Close_32;
            this.btnClose.Name = "btnClose";
            this.btnClose.UseVisualStyleBackColor = true;
            // 
            // btnGenerate
            // 
            resources.ApplyResources(this.btnGenerate, "btnGenerate");
            this.btnGenerate.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.btnGenerate.ForeColor = System.Drawing.Color.Snow;
            this.btnGenerate.Image = global::DVLD.Properties.Resources.Magic_Wand40;
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Tag = " ";
            this.btnGenerate.UseVisualStyleBackColor = false;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // pictureBox1
            // 
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.Image = global::DVLD.Properties.Resources.Magic_Wand801;
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            // 
            // pbPersonImage
            // 
            resources.ApplyResources(this.pbPersonImage, "pbPersonImage");
            this.pbPersonImage.Image = global::DVLD.Properties.Resources.TestType_512;
            this.pbPersonImage.Name = "pbPersonImage";
            this.pbPersonImage.TabStop = false;
            this.pbPersonImage.Tag = " ";
            // 
            // frmGenerateQuestions
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(248)))), ((int)(((byte)(253)))));
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnGenerate);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.pbPersonImage);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmGenerateQuestions";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Load += new System.EventHandler(this.frmGenerateQuestions_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudTrueFalse)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMultipleChoice)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbPersonImage)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pbPersonImage;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ComboBox cmbDocuments;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown nudTrueFalse;
        private System.Windows.Forms.NumericUpDown nudMultipleChoice;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblTotalQuestions;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.Button btnClose;
    }
}