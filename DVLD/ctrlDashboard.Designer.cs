namespace DVLD
{
    partial class ctrlDashboard
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ctrlDashboard));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.tlpDashboard = new System.Windows.Forms.TableLayoutPanel();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblDate = new System.Windows.Forms.Label();
            this.lblWelcome = new System.Windows.Forms.Label();
            this.lblDashboardTitle = new System.Windows.Forms.Label();
            this.tlpStatistics = new System.Windows.Forms.TableLayoutPanel();
            this.pnlDetainedLicenses = new System.Windows.Forms.Panel();
            this.pictureBox4 = new System.Windows.Forms.PictureBox();
            this.lblDetainedCount = new System.Windows.Forms.Label();
            this.lblDetainedTitle = new System.Windows.Forms.Label();
            this.pnlTodayTests = new System.Windows.Forms.Panel();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.lblTodayTestsCount = new System.Windows.Forms.Label();
            this.lblTodayTestsTitle = new System.Windows.Forms.Label();
            this.pnlActiveLicenses = new System.Windows.Forms.Panel();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.lblActiveLicensesCount = new System.Windows.Forms.Label();
            this.lblActiveLicensesTitle = new System.Windows.Forms.Label();
            this.pnlPendingApplications = new System.Windows.Forms.Panel();
            this.lblPendingCount = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lblPendingTitle = new System.Windows.Forms.Label();
            this.tlpBottomSection = new System.Windows.Forms.TableLayoutPanel();
            this.pnlTodayAppointments = new System.Windows.Forms.Panel();
            this.pictureBox5 = new System.Windows.Forms.PictureBox();
            this.dgvTodayAppointments = new System.Windows.Forms.DataGridView();
            this.lblAppointmentsTitle = new System.Windows.Forms.Label();
            this.pnlQuickActions = new System.Windows.Forms.Panel();
            this.pictureBox6 = new System.Windows.Forms.PictureBox();
            this.btnSearchLicense = new System.Windows.Forms.Button();
            this.btnNewLicenseApplication = new System.Windows.Forms.Button();
            this.btnAddPerson = new System.Windows.Forms.Button();
            this.lblQuickActionsTitle = new System.Windows.Forms.Label();
            this.tlpDashboard.SuspendLayout();
            this.pnlHeader.SuspendLayout();
            this.tlpStatistics.SuspendLayout();
            this.pnlDetainedLicenses.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).BeginInit();
            this.pnlTodayTests.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            this.pnlActiveLicenses.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.pnlPendingApplications.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.tlpBottomSection.SuspendLayout();
            this.pnlTodayAppointments.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTodayAppointments)).BeginInit();
            this.pnlQuickActions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox6)).BeginInit();
            this.SuspendLayout();
            // 
            // tlpDashboard
            // 
            resources.ApplyResources(this.tlpDashboard, "tlpDashboard");
            this.tlpDashboard.Controls.Add(this.pnlHeader, 0, 0);
            this.tlpDashboard.Controls.Add(this.tlpStatistics, 0, 1);
            this.tlpDashboard.Controls.Add(this.tlpBottomSection, 0, 2);
            this.tlpDashboard.Name = "tlpDashboard";
            // 
            // pnlHeader
            // 
            this.pnlHeader.Controls.Add(this.lblDate);
            this.pnlHeader.Controls.Add(this.lblWelcome);
            this.pnlHeader.Controls.Add(this.lblDashboardTitle);
            resources.ApplyResources(this.pnlHeader, "pnlHeader");
            this.pnlHeader.Name = "pnlHeader";
            // 
            // lblDate
            // 
            resources.ApplyResources(this.lblDate, "lblDate");
            this.lblDate.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(61)))), ((int)(((byte)(125)))));
            this.lblDate.Name = "lblDate";
            // 
            // lblWelcome
            // 
            resources.ApplyResources(this.lblWelcome, "lblWelcome");
            this.lblWelcome.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(61)))), ((int)(((byte)(125)))));
            this.lblWelcome.Name = "lblWelcome";
            // 
            // lblDashboardTitle
            // 
            resources.ApplyResources(this.lblDashboardTitle, "lblDashboardTitle");
            this.lblDashboardTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(61)))), ((int)(((byte)(125)))));
            this.lblDashboardTitle.Name = "lblDashboardTitle";
            // 
            // tlpStatistics
            // 
            resources.ApplyResources(this.tlpStatistics, "tlpStatistics");
            this.tlpStatistics.Controls.Add(this.pnlDetainedLicenses, 3, 0);
            this.tlpStatistics.Controls.Add(this.pnlTodayTests, 2, 0);
            this.tlpStatistics.Controls.Add(this.pnlActiveLicenses, 1, 0);
            this.tlpStatistics.Controls.Add(this.pnlPendingApplications, 0, 0);
            this.tlpStatistics.Name = "tlpStatistics";
            // 
            // pnlDetainedLicenses
            // 
            this.pnlDetainedLicenses.BackColor = System.Drawing.Color.White;
            this.pnlDetainedLicenses.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlDetainedLicenses.Controls.Add(this.pictureBox4);
            this.pnlDetainedLicenses.Controls.Add(this.lblDetainedCount);
            this.pnlDetainedLicenses.Controls.Add(this.lblDetainedTitle);
            resources.ApplyResources(this.pnlDetainedLicenses, "pnlDetainedLicenses");
            this.pnlDetainedLicenses.Name = "pnlDetainedLicenses";
            // 
            // pictureBox4
            // 
            resources.ApplyResources(this.pictureBox4, "pictureBox4");
            this.pictureBox4.Name = "pictureBox4";
            this.pictureBox4.TabStop = false;
            // 
            // lblDetainedCount
            // 
            resources.ApplyResources(this.lblDetainedCount, "lblDetainedCount");
            this.lblDetainedCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(61)))), ((int)(((byte)(125)))));
            this.lblDetainedCount.Name = "lblDetainedCount";
            // 
            // lblDetainedTitle
            // 
            resources.ApplyResources(this.lblDetainedTitle, "lblDetainedTitle");
            this.lblDetainedTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(61)))), ((int)(((byte)(125)))));
            this.lblDetainedTitle.Name = "lblDetainedTitle";
            // 
            // pnlTodayTests
            // 
            this.pnlTodayTests.BackColor = System.Drawing.Color.White;
            this.pnlTodayTests.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlTodayTests.Controls.Add(this.pictureBox3);
            this.pnlTodayTests.Controls.Add(this.lblTodayTestsCount);
            this.pnlTodayTests.Controls.Add(this.lblTodayTestsTitle);
            resources.ApplyResources(this.pnlTodayTests, "pnlTodayTests");
            this.pnlTodayTests.Name = "pnlTodayTests";
            // 
            // pictureBox3
            // 
            resources.ApplyResources(this.pictureBox3, "pictureBox3");
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.TabStop = false;
            // 
            // lblTodayTestsCount
            // 
            resources.ApplyResources(this.lblTodayTestsCount, "lblTodayTestsCount");
            this.lblTodayTestsCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(61)))), ((int)(((byte)(125)))));
            this.lblTodayTestsCount.Name = "lblTodayTestsCount";
            // 
            // lblTodayTestsTitle
            // 
            resources.ApplyResources(this.lblTodayTestsTitle, "lblTodayTestsTitle");
            this.lblTodayTestsTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(61)))), ((int)(((byte)(125)))));
            this.lblTodayTestsTitle.Name = "lblTodayTestsTitle";
            this.lblTodayTestsTitle.Click += new System.EventHandler(this.lblTodayTestsTitle_Click);
            // 
            // pnlActiveLicenses
            // 
            this.pnlActiveLicenses.BackColor = System.Drawing.Color.White;
            this.pnlActiveLicenses.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlActiveLicenses.Controls.Add(this.pictureBox2);
            this.pnlActiveLicenses.Controls.Add(this.lblActiveLicensesCount);
            this.pnlActiveLicenses.Controls.Add(this.lblActiveLicensesTitle);
            resources.ApplyResources(this.pnlActiveLicenses, "pnlActiveLicenses");
            this.pnlActiveLicenses.Name = "pnlActiveLicenses";
            // 
            // pictureBox2
            // 
            resources.ApplyResources(this.pictureBox2, "pictureBox2");
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.TabStop = false;
            // 
            // lblActiveLicensesCount
            // 
            resources.ApplyResources(this.lblActiveLicensesCount, "lblActiveLicensesCount");
            this.lblActiveLicensesCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(61)))), ((int)(((byte)(125)))));
            this.lblActiveLicensesCount.Name = "lblActiveLicensesCount";
            // 
            // lblActiveLicensesTitle
            // 
            resources.ApplyResources(this.lblActiveLicensesTitle, "lblActiveLicensesTitle");
            this.lblActiveLicensesTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(61)))), ((int)(((byte)(125)))));
            this.lblActiveLicensesTitle.Name = "lblActiveLicensesTitle";
            // 
            // pnlPendingApplications
            // 
            this.pnlPendingApplications.BackColor = System.Drawing.Color.White;
            this.pnlPendingApplications.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlPendingApplications.Controls.Add(this.lblPendingCount);
            this.pnlPendingApplications.Controls.Add(this.pictureBox1);
            this.pnlPendingApplications.Controls.Add(this.lblPendingTitle);
            resources.ApplyResources(this.pnlPendingApplications, "pnlPendingApplications");
            this.pnlPendingApplications.Name = "pnlPendingApplications";
            // 
            // lblPendingCount
            // 
            resources.ApplyResources(this.lblPendingCount, "lblPendingCount");
            this.lblPendingCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(61)))), ((int)(((byte)(125)))));
            this.lblPendingCount.Name = "lblPendingCount";
            this.lblPendingCount.Click += new System.EventHandler(this.lblPendingCount_Click);
            // 
            // pictureBox1
            // 
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            // 
            // lblPendingTitle
            // 
            resources.ApplyResources(this.lblPendingTitle, "lblPendingTitle");
            this.lblPendingTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(61)))), ((int)(((byte)(125)))));
            this.lblPendingTitle.Name = "lblPendingTitle";
            // 
            // tlpBottomSection
            // 
            resources.ApplyResources(this.tlpBottomSection, "tlpBottomSection");
            this.tlpBottomSection.Controls.Add(this.pnlTodayAppointments, 0, 0);
            this.tlpBottomSection.Controls.Add(this.pnlQuickActions, 1, 0);
            this.tlpBottomSection.Name = "tlpBottomSection";
            // 
            // pnlTodayAppointments
            // 
            this.pnlTodayAppointments.BackColor = System.Drawing.Color.White;
            this.pnlTodayAppointments.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlTodayAppointments.Controls.Add(this.pictureBox5);
            this.pnlTodayAppointments.Controls.Add(this.dgvTodayAppointments);
            this.pnlTodayAppointments.Controls.Add(this.lblAppointmentsTitle);
            resources.ApplyResources(this.pnlTodayAppointments, "pnlTodayAppointments");
            this.pnlTodayAppointments.Name = "pnlTodayAppointments";
            // 
            // pictureBox5
            // 
            resources.ApplyResources(this.pictureBox5, "pictureBox5");
            this.pictureBox5.Name = "pictureBox5";
            this.pictureBox5.TabStop = false;
            // 
            // dgvTodayAppointments
            // 
            this.dgvTodayAppointments.AllowUserToAddRows = false;
            this.dgvTodayAppointments.AllowUserToDeleteRows = false;
            this.dgvTodayAppointments.AllowUserToResizeRows = false;
            resources.ApplyResources(this.dgvTodayAppointments, "dgvTodayAppointments");
            this.dgvTodayAppointments.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvTodayAppointments.BackgroundColor = System.Drawing.Color.White;
            this.dgvTodayAppointments.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(61)))), ((int)(((byte)(125)))));
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvTodayAppointments.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.dgvTodayAppointments.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvTodayAppointments.EnableHeadersVisualStyles = false;
            this.dgvTodayAppointments.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(232)))), ((int)(((byte)(240)))));
            this.dgvTodayAppointments.MultiSelect = false;
            this.dgvTodayAppointments.Name = "dgvTodayAppointments";
            this.dgvTodayAppointments.ReadOnly = true;
            this.dgvTodayAppointments.RowHeadersVisible = false;
            this.dgvTodayAppointments.RowTemplate.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.dgvTodayAppointments.RowTemplate.DefaultCellStyle.BackColor = System.Drawing.Color.White;
            this.dgvTodayAppointments.RowTemplate.DefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(61)))), ((int)(((byte)(125)))));
            this.dgvTodayAppointments.RowTemplate.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(235)))), ((int)(((byte)(252)))));
            this.dgvTodayAppointments.RowTemplate.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(61)))), ((int)(((byte)(125)))));
            this.dgvTodayAppointments.RowTemplate.Height = 36;
            this.dgvTodayAppointments.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            // 
            // lblAppointmentsTitle
            // 
            resources.ApplyResources(this.lblAppointmentsTitle, "lblAppointmentsTitle");
            this.lblAppointmentsTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(61)))), ((int)(((byte)(125)))));
            this.lblAppointmentsTitle.Name = "lblAppointmentsTitle";
            // 
            // pnlQuickActions
            // 
            this.pnlQuickActions.BackColor = System.Drawing.Color.White;
            this.pnlQuickActions.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlQuickActions.Controls.Add(this.pictureBox6);
            this.pnlQuickActions.Controls.Add(this.btnSearchLicense);
            this.pnlQuickActions.Controls.Add(this.btnNewLicenseApplication);
            this.pnlQuickActions.Controls.Add(this.btnAddPerson);
            this.pnlQuickActions.Controls.Add(this.lblQuickActionsTitle);
            resources.ApplyResources(this.pnlQuickActions, "pnlQuickActions");
            this.pnlQuickActions.Name = "pnlQuickActions";
            // 
            // pictureBox6
            // 
            resources.ApplyResources(this.pictureBox6, "pictureBox6");
            this.pictureBox6.Name = "pictureBox6";
            this.pictureBox6.TabStop = false;
            // 
            // btnSearchLicense
            // 
            resources.ApplyResources(this.btnSearchLicense, "btnSearchLicense");
            this.btnSearchLicense.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(240)))), ((int)(((byte)(252)))));
            this.btnSearchLicense.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(61)))), ((int)(((byte)(125)))));
            this.btnSearchLicense.Name = "btnSearchLicense";
            this.btnSearchLicense.UseVisualStyleBackColor = false;
            this.btnSearchLicense.Click += new System.EventHandler(this.btnSearchLicense_Click);
            // 
            // btnNewLicenseApplication
            // 
            resources.ApplyResources(this.btnNewLicenseApplication, "btnNewLicenseApplication");
            this.btnNewLicenseApplication.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(240)))), ((int)(((byte)(252)))));
            this.btnNewLicenseApplication.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(61)))), ((int)(((byte)(125)))));
            this.btnNewLicenseApplication.Name = "btnNewLicenseApplication";
            this.btnNewLicenseApplication.UseVisualStyleBackColor = false;
            this.btnNewLicenseApplication.Click += new System.EventHandler(this.btnNewLicenseApplication_Click);
            // 
            // btnAddPerson
            // 
            resources.ApplyResources(this.btnAddPerson, "btnAddPerson");
            this.btnAddPerson.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(240)))), ((int)(((byte)(252)))));
            this.btnAddPerson.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(61)))), ((int)(((byte)(125)))));
            this.btnAddPerson.Name = "btnAddPerson";
            this.btnAddPerson.UseVisualStyleBackColor = false;
            this.btnAddPerson.Click += new System.EventHandler(this.btnAddPerson_Click);
            // 
            // lblQuickActionsTitle
            // 
            resources.ApplyResources(this.lblQuickActionsTitle, "lblQuickActionsTitle");
            this.lblQuickActionsTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(61)))), ((int)(((byte)(125)))));
            this.lblQuickActionsTitle.Name = "lblQuickActionsTitle";
            // 
            // ctrlDashboard
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(245)))), ((int)(((byte)(251)))));
            this.Controls.Add(this.tlpDashboard);
            this.Name = "ctrlDashboard";
            this.tlpDashboard.ResumeLayout(false);
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.tlpStatistics.ResumeLayout(false);
            this.pnlDetainedLicenses.ResumeLayout(false);
            this.pnlDetainedLicenses.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).EndInit();
            this.pnlTodayTests.ResumeLayout(false);
            this.pnlTodayTests.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            this.pnlActiveLicenses.ResumeLayout(false);
            this.pnlActiveLicenses.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.pnlPendingApplications.ResumeLayout(false);
            this.pnlPendingApplications.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.tlpBottomSection.ResumeLayout(false);
            this.pnlTodayAppointments.ResumeLayout(false);
            this.pnlTodayAppointments.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTodayAppointments)).EndInit();
            this.pnlQuickActions.ResumeLayout(false);
            this.pnlQuickActions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox6)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpDashboard;
        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblDashboardTitle;
        private System.Windows.Forms.Label lblDate;
        private System.Windows.Forms.Label lblWelcome;
        private System.Windows.Forms.TableLayoutPanel tlpStatistics;
        private System.Windows.Forms.Panel pnlPendingApplications;
        private System.Windows.Forms.Label lblPendingCount;
        private System.Windows.Forms.Label lblPendingTitle;
        private System.Windows.Forms.Panel pnlTodayTests;
        private System.Windows.Forms.Label lblTodayTestsCount;
        private System.Windows.Forms.Label lblTodayTestsTitle;
        private System.Windows.Forms.Panel pnlActiveLicenses;
        private System.Windows.Forms.Label lblActiveLicensesCount;
        private System.Windows.Forms.Label lblActiveLicensesTitle;
        private System.Windows.Forms.Panel pnlDetainedLicenses;
        private System.Windows.Forms.Label lblDetainedCount;
        private System.Windows.Forms.Label lblDetainedTitle;
        private System.Windows.Forms.TableLayoutPanel tlpBottomSection;
        private System.Windows.Forms.Panel pnlTodayAppointments;
        private System.Windows.Forms.Label lblAppointmentsTitle;
        private System.Windows.Forms.DataGridView dgvTodayAppointments;
        private System.Windows.Forms.Panel pnlQuickActions;
        private System.Windows.Forms.Label lblQuickActionsTitle;
        private System.Windows.Forms.Button btnSearchLicense;
        private System.Windows.Forms.Button btnNewLicenseApplication;
        private System.Windows.Forms.Button btnAddPerson;
        private System.Windows.Forms.PictureBox pictureBox4;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox5;
        private System.Windows.Forms.PictureBox pictureBox6;
    }
}
