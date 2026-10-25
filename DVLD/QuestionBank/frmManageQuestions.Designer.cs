namespace DVLD.QuestionBank
{
    partial class frmManageQuestions
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmManageQuestions));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dgvQuestions = new System.Windows.Forms.DataGridView();
            this.cmsQuestions = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.label2 = new System.Windows.Forms.Label();
            this.lblRecordsCount = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cbFilterBy = new System.Windows.Forms.ComboBox();
            this.txtFilterValue = new System.Windows.Forms.TextBox();
            this.lblTitle = new System.Windows.Forms.Label();
            this.cbIsActive = new System.Windows.Forms.ComboBox();
            this.cbReviewStatus = new System.Windows.Forms.ComboBox();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.addNewQuestionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showDetailsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editQuestionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.approveQuestionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rejectQuestionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.activateQuestionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deactivateQuestionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteQuestionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnAddPerson = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pbPersonImage = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.dgvQuestions)).BeginInit();
            this.cmsQuestions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbPersonImage)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvQuestions
            // 
            resources.ApplyResources(this.dgvQuestions, "dgvQuestions");
            this.dgvQuestions.AllowUserToAddRows = false;
            this.dgvQuestions.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(245)))), ((int)(((byte)(252)))));
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(61)))), ((int)(((byte)(125)))));
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.dgvQuestions.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvQuestions.BackgroundColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(61)))), ((int)(((byte)(125)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(61)))), ((int)(((byte)(125)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvQuestions.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvQuestions.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvQuestions.ContextMenuStrip = this.cmsQuestions;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(61)))), ((int)(((byte)(125)))));
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvQuestions.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvQuestions.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dgvQuestions.EnableHeadersVisualStyles = false;
            this.dgvQuestions.MultiSelect = false;
            this.dgvQuestions.Name = "dgvQuestions";
            this.dgvQuestions.ReadOnly = true;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvQuestions.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.dgvQuestions.RowTemplate.Height = 32;
            this.dgvQuestions.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvQuestions.TabStop = false;
            this.dgvQuestions.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvQuestions_CellMouseDown);
            // 
            // cmsQuestions
            // 
            resources.ApplyResources(this.cmsQuestions, "cmsQuestions");
            this.cmsQuestions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addNewQuestionToolStripMenuItem,
            this.showDetailsToolStripMenuItem,
            this.editQuestionToolStripMenuItem,
            this.toolStripMenuItem1,
            this.approveQuestionToolStripMenuItem,
            this.rejectQuestionToolStripMenuItem,
            this.toolStripMenuItem2,
            this.activateQuestionToolStripMenuItem,
            this.deactivateQuestionToolStripMenuItem,
            this.toolStripMenuItem3,
            this.deleteQuestionToolStripMenuItem,
            this.refreshToolStripMenuItem});
            this.cmsQuestions.Name = "cmsQuestions";
            // 
            // toolStripMenuItem1
            // 
            resources.ApplyResources(this.toolStripMenuItem1, "toolStripMenuItem1");
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            // 
            // toolStripMenuItem2
            // 
            resources.ApplyResources(this.toolStripMenuItem2, "toolStripMenuItem2");
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            // 
            // toolStripMenuItem3
            // 
            resources.ApplyResources(this.toolStripMenuItem3, "toolStripMenuItem3");
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(61)))), ((int)(((byte)(125)))));
            this.label2.Name = "label2";
            // 
            // lblRecordsCount
            // 
            resources.ApplyResources(this.lblRecordsCount, "lblRecordsCount");
            this.lblRecordsCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(61)))), ((int)(((byte)(125)))));
            this.lblRecordsCount.Name = "lblRecordsCount";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(61)))), ((int)(((byte)(125)))));
            this.label1.Name = "label1";
            // 
            // cbFilterBy
            // 
            resources.ApplyResources(this.cbFilterBy, "cbFilterBy");
            this.cbFilterBy.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbFilterBy.FormattingEnabled = true;
            this.cbFilterBy.Items.AddRange(new object[] {
            resources.GetString("cbFilterBy.Items"),
            resources.GetString("cbFilterBy.Items1"),
            resources.GetString("cbFilterBy.Items2"),
            resources.GetString("cbFilterBy.Items3"),
            resources.GetString("cbFilterBy.Items4"),
            resources.GetString("cbFilterBy.Items5")});
            this.cbFilterBy.Name = "cbFilterBy";
            this.cbFilterBy.SelectedIndexChanged += new System.EventHandler(this.cbFilterBy_SelectedIndexChanged);
            // 
            // txtFilterValue
            // 
            resources.ApplyResources(this.txtFilterValue, "txtFilterValue");
            this.txtFilterValue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtFilterValue.Name = "txtFilterValue";
            this.txtFilterValue.TextChanged += new System.EventHandler(this.txtFilterValue_TextChanged);
            // 
            // lblTitle
            // 
            resources.ApplyResources(this.lblTitle, "lblTitle");
            this.lblTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(61)))), ((int)(((byte)(125)))));
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Tag = " ";
            // 
            // cbIsActive
            // 
            resources.ApplyResources(this.cbIsActive, "cbIsActive");
            this.cbIsActive.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbIsActive.FormattingEnabled = true;
            this.cbIsActive.Items.AddRange(new object[] {
            resources.GetString("cbIsActive.Items"),
            resources.GetString("cbIsActive.Items1"),
            resources.GetString("cbIsActive.Items2")});
            this.cbIsActive.Name = "cbIsActive";
            this.cbIsActive.SelectedIndexChanged += new System.EventHandler(this.cbIsActive_SelectedIndexChanged);
            // 
            // cbReviewStatus
            // 
            resources.ApplyResources(this.cbReviewStatus, "cbReviewStatus");
            this.cbReviewStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbReviewStatus.FormattingEnabled = true;
            this.cbReviewStatus.Items.AddRange(new object[] {
            resources.GetString("cbReviewStatus.Items"),
            resources.GetString("cbReviewStatus.Items1"),
            resources.GetString("cbReviewStatus.Items2"),
            resources.GetString("cbReviewStatus.Items3")});
            this.cbReviewStatus.Name = "cbReviewStatus";
            this.cbReviewStatus.SelectedIndexChanged += new System.EventHandler(this.cbReviewStatus_SelectedIndexChanged);
            // 
            // btnClose
            // 
            resources.ApplyResources(this.btnClose, "btnClose");
            this.btnClose.AutoEllipsis = true;
            this.btnClose.BackColor = System.Drawing.Color.White;
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(205)))), ((int)(((byte)(235)))));
            this.btnClose.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(61)))), ((int)(((byte)(125)))));
            this.btnClose.Image = global::DVLD.Properties.Resources.Close_32;
            this.btnClose.Name = "btnClose";
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnGenerate
            // 
            resources.ApplyResources(this.btnGenerate, "btnGenerate");
            this.btnGenerate.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnGenerate.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(205)))), ((int)(((byte)(235)))));
            this.btnGenerate.Image = global::DVLD.Properties.Resources.Magic_Wand802;
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // addNewQuestionToolStripMenuItem
            // 
            resources.ApplyResources(this.addNewQuestionToolStripMenuItem, "addNewQuestionToolStripMenuItem");
            this.addNewQuestionToolStripMenuItem.Image = global::DVLD.Properties.Resources.add_question_32;
            this.addNewQuestionToolStripMenuItem.Name = "addNewQuestionToolStripMenuItem";
            this.addNewQuestionToolStripMenuItem.Click += new System.EventHandler(this.addNewQuestionToolStripMenuItem_Click);
            // 
            // showDetailsToolStripMenuItem
            // 
            resources.ApplyResources(this.showDetailsToolStripMenuItem, "showDetailsToolStripMenuItem");
            this.showDetailsToolStripMenuItem.Image = global::DVLD.Properties.Resources.show_details_32;
            this.showDetailsToolStripMenuItem.Name = "showDetailsToolStripMenuItem";
            this.showDetailsToolStripMenuItem.Click += new System.EventHandler(this.showDetailsToolStripMenuItem_Click);
            // 
            // editQuestionToolStripMenuItem
            // 
            resources.ApplyResources(this.editQuestionToolStripMenuItem, "editQuestionToolStripMenuItem");
            this.editQuestionToolStripMenuItem.Image = global::DVLD.Properties.Resources.edit_question_32;
            this.editQuestionToolStripMenuItem.Name = "editQuestionToolStripMenuItem";
            this.editQuestionToolStripMenuItem.Click += new System.EventHandler(this.editQuestionToolStripMenuItem_Click);
            // 
            // approveQuestionToolStripMenuItem
            // 
            resources.ApplyResources(this.approveQuestionToolStripMenuItem, "approveQuestionToolStripMenuItem");
            this.approveQuestionToolStripMenuItem.Image = global::DVLD.Properties.Resources.approve_question_32;
            this.approveQuestionToolStripMenuItem.Name = "approveQuestionToolStripMenuItem";
            this.approveQuestionToolStripMenuItem.Click += new System.EventHandler(this.approveQuestionToolStripMenuItem_Click);
            // 
            // rejectQuestionToolStripMenuItem
            // 
            resources.ApplyResources(this.rejectQuestionToolStripMenuItem, "rejectQuestionToolStripMenuItem");
            this.rejectQuestionToolStripMenuItem.Image = global::DVLD.Properties.Resources.reject_question_32;
            this.rejectQuestionToolStripMenuItem.Name = "rejectQuestionToolStripMenuItem";
            this.rejectQuestionToolStripMenuItem.Click += new System.EventHandler(this.rejectQuestionToolStripMenuItem_Click);
            // 
            // activateQuestionToolStripMenuItem
            // 
            resources.ApplyResources(this.activateQuestionToolStripMenuItem, "activateQuestionToolStripMenuItem");
            this.activateQuestionToolStripMenuItem.Image = global::DVLD.Properties.Resources.activate_question_32;
            this.activateQuestionToolStripMenuItem.Name = "activateQuestionToolStripMenuItem";
            this.activateQuestionToolStripMenuItem.Click += new System.EventHandler(this.activateQuestionToolStripMenuItem_Click);
            // 
            // deactivateQuestionToolStripMenuItem
            // 
            resources.ApplyResources(this.deactivateQuestionToolStripMenuItem, "deactivateQuestionToolStripMenuItem");
            this.deactivateQuestionToolStripMenuItem.Image = global::DVLD.Properties.Resources.deactivate_question_321;
            this.deactivateQuestionToolStripMenuItem.Name = "deactivateQuestionToolStripMenuItem";
            this.deactivateQuestionToolStripMenuItem.Click += new System.EventHandler(this.deactivateQuestionToolStripMenuItem_Click);
            // 
            // deleteQuestionToolStripMenuItem
            // 
            resources.ApplyResources(this.deleteQuestionToolStripMenuItem, "deleteQuestionToolStripMenuItem");
            this.deleteQuestionToolStripMenuItem.Image = global::DVLD.Properties.Resources.delete_question_32;
            this.deleteQuestionToolStripMenuItem.Name = "deleteQuestionToolStripMenuItem";
            this.deleteQuestionToolStripMenuItem.Click += new System.EventHandler(this.deleteQuestionToolStripMenuItem_Click);
            // 
            // refreshToolStripMenuItem
            // 
            resources.ApplyResources(this.refreshToolStripMenuItem, "refreshToolStripMenuItem");
            this.refreshToolStripMenuItem.Image = global::DVLD.Properties.Resources.refresh_32;
            this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
            this.refreshToolStripMenuItem.Click += new System.EventHandler(this.refreshToolStripMenuItem_Click);
            // 
            // btnAddPerson
            // 
            resources.ApplyResources(this.btnAddPerson, "btnAddPerson");
            this.btnAddPerson.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAddPerson.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(205)))), ((int)(((byte)(235)))));
            this.btnAddPerson.Image = global::DVLD.Properties.Resources.icons8_add_to_clipboard_100;
            this.btnAddPerson.Name = "btnAddPerson";
            this.btnAddPerson.UseVisualStyleBackColor = true;
            this.btnAddPerson.Click += new System.EventHandler(this.btnAddPerson_Click);
            // 
            // pictureBox1
            // 
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.Image = global::DVLD.Properties.Resources.Test_32;
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            // 
            // pbPersonImage
            // 
            resources.ApplyResources(this.pbPersonImage, "pbPersonImage");
            this.pbPersonImage.Image = global::DVLD.Properties.Resources.Applications;
            this.pbPersonImage.Name = "pbPersonImage";
            this.pbPersonImage.TabStop = false;
            // 
            // frmManageQuestions
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(248)))), ((int)(((byte)(253)))));
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnGenerate);
            this.Controls.Add(this.cbReviewStatus);
            this.Controls.Add(this.cbIsActive);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.txtFilterValue);
            this.Controls.Add(this.cbFilterBy);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblRecordsCount);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnAddPerson);
            this.Controls.Add(this.dgvQuestions);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.pbPersonImage);
            this.Name = "frmManageQuestions";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Load += new System.EventHandler(this.frmManageQuestions_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvQuestions)).EndInit();
            this.cmsQuestions.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbPersonImage)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pbPersonImage;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.DataGridView dgvQuestions;
        private System.Windows.Forms.Button btnAddPerson;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblRecordsCount;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbFilterBy;
        private System.Windows.Forms.TextBox txtFilterValue;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.ComboBox cbIsActive;
        private System.Windows.Forms.ComboBox cbReviewStatus;
        private System.Windows.Forms.ContextMenuStrip cmsQuestions;
        private System.Windows.Forms.ToolStripMenuItem addNewQuestionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showDetailsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editQuestionToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem approveQuestionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rejectQuestionToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem activateQuestionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deactivateQuestionToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem deleteQuestionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem refreshToolStripMenuItem;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.Button btnClose;
    }
}