namespace DVLD
{
    partial class frmMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.msMainMenue = new System.Windows.Forms.MenuStrip();
            this.servicesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.drivingLicensesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.oNewDrivingLicenseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.localLicenseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.internationalLicenseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renewDrivingLicenseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.ReplacementLostOrDamagedDrivingLicenseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.releaseDetainedDrivingLicenseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.retakeTestToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.tsMManageApplications = new System.Windows.Forms.ToolStripMenuItem();
            this.manageLocalDrivingLicenseApplicationsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ManageInternationaDrivingLicenseToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.DetainLicensesToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.ManageDetainedLicensestoolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.detainLicenseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.releaseDetainedLicenseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.manageApplicationTypesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.manageTestTypesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.peopleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.driversToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.employeesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.manageQuestionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mangeDecumentsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.currentUserInfoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changePasswordToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.signOutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lblLoggedInUser = new System.Windows.Forms.Label();
            this.statusStripMain = new System.Windows.Forms.StatusStrip();
            this.lblStatusSystem = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblStatusUser = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblStatusReady = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblWelcomeTitle = new System.Windows.Forms.Label();
            this.lblWelcomeSubtitle = new System.Windows.Forms.Label();
            this.lblWelcomeMessage = new System.Windows.Forms.Label();
            this.pnlWelcome = new System.Windows.Forms.Panel();
            this.picWelcomeIcon = new System.Windows.Forms.PictureBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem7 = new System.Windows.Forms.ToolStripMenuItem();
            this.msMainMenue.SuspendLayout();
            this.statusStripMain.SuspendLayout();
            this.pnlWelcome.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picWelcomeIcon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // msMainMenue
            // 
            resources.ApplyResources(this.msMainMenue, "msMainMenue");
            this.msMainMenue.BackColor = System.Drawing.Color.White;
            this.msMainMenue.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.msMainMenue.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.servicesToolStripMenuItem,
            this.peopleToolStripMenuItem,
            this.driversToolStripMenuItem,
            this.employeesToolStripMenuItem,
            this.manageQuestionsToolStripMenuItem,
            this.mangeDecumentsToolStripMenuItem,
            this.closeToolStripMenuItem});
            this.msMainMenue.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.msMainMenue.Name = "msMainMenue";
            // 
            // servicesToolStripMenuItem
            // 
            resources.ApplyResources(this.servicesToolStripMenuItem, "servicesToolStripMenuItem");
            this.servicesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.drivingLicensesToolStripMenuItem,
            this.toolStripSeparator6,
            this.tsMManageApplications,
            this.toolStripSeparator5,
            this.DetainLicensesToolStripMenuItem1,
            this.manageApplicationTypesToolStripMenuItem,
            this.manageTestTypesToolStripMenuItem});
            this.servicesToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(64)))), ((int)(((byte)(120)))));
            this.servicesToolStripMenuItem.Image = global::DVLD.Properties.Resources.Applications_64;
            this.servicesToolStripMenuItem.Name = "servicesToolStripMenuItem";
            // 
            // drivingLicensesToolStripMenuItem
            // 
            resources.ApplyResources(this.drivingLicensesToolStripMenuItem, "drivingLicensesToolStripMenuItem");
            this.drivingLicensesToolStripMenuItem.BackColor = System.Drawing.SystemColors.Control;
            this.drivingLicensesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.oNewDrivingLicenseToolStripMenuItem,
            this.renewDrivingLicenseToolStripMenuItem,
            this.toolStripSeparator1,
            this.ReplacementLostOrDamagedDrivingLicenseToolStripMenuItem,
            this.toolStripSeparator2,
            this.releaseDetainedDrivingLicenseToolStripMenuItem,
            this.retakeTestToolStripMenuItem1});
            this.drivingLicensesToolStripMenuItem.Image = global::DVLD.Properties.Resources.Driver_License_32;
            this.drivingLicensesToolStripMenuItem.Name = "drivingLicensesToolStripMenuItem";
            // 
            // oNewDrivingLicenseToolStripMenuItem
            // 
            resources.ApplyResources(this.oNewDrivingLicenseToolStripMenuItem, "oNewDrivingLicenseToolStripMenuItem");
            this.oNewDrivingLicenseToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.localLicenseToolStripMenuItem,
            this.internationalLicenseToolStripMenuItem});
            this.oNewDrivingLicenseToolStripMenuItem.Image = global::DVLD.Properties.Resources.New_Driving_License_32;
            this.oNewDrivingLicenseToolStripMenuItem.Name = "oNewDrivingLicenseToolStripMenuItem";
            // 
            // localLicenseToolStripMenuItem
            // 
            resources.ApplyResources(this.localLicenseToolStripMenuItem, "localLicenseToolStripMenuItem");
            this.localLicenseToolStripMenuItem.Image = global::DVLD.Properties.Resources.Local_32;
            this.localLicenseToolStripMenuItem.Name = "localLicenseToolStripMenuItem";
            this.localLicenseToolStripMenuItem.Click += new System.EventHandler(this.localLicenseToolStripMenuItem_Click);
            // 
            // internationalLicenseToolStripMenuItem
            // 
            resources.ApplyResources(this.internationalLicenseToolStripMenuItem, "internationalLicenseToolStripMenuItem");
            this.internationalLicenseToolStripMenuItem.BackColor = System.Drawing.Color.White;
            this.internationalLicenseToolStripMenuItem.Image = global::DVLD.Properties.Resources.International_32;
            this.internationalLicenseToolStripMenuItem.Name = "internationalLicenseToolStripMenuItem";
            this.internationalLicenseToolStripMenuItem.Click += new System.EventHandler(this.internationalLicenseToolStripMenuItem_Click);
            // 
            // renewDrivingLicenseToolStripMenuItem
            // 
            resources.ApplyResources(this.renewDrivingLicenseToolStripMenuItem, "renewDrivingLicenseToolStripMenuItem");
            this.renewDrivingLicenseToolStripMenuItem.Image = global::DVLD.Properties.Resources.Renew_Driving_License_32;
            this.renewDrivingLicenseToolStripMenuItem.Name = "renewDrivingLicenseToolStripMenuItem";
            this.renewDrivingLicenseToolStripMenuItem.Click += new System.EventHandler(this.renewDrivingLicenseToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            // 
            // ReplacementLostOrDamagedDrivingLicenseToolStripMenuItem
            // 
            resources.ApplyResources(this.ReplacementLostOrDamagedDrivingLicenseToolStripMenuItem, "ReplacementLostOrDamagedDrivingLicenseToolStripMenuItem");
            this.ReplacementLostOrDamagedDrivingLicenseToolStripMenuItem.Image = global::DVLD.Properties.Resources.Damaged_Driving_License_32;
            this.ReplacementLostOrDamagedDrivingLicenseToolStripMenuItem.Name = "ReplacementLostOrDamagedDrivingLicenseToolStripMenuItem";
            this.ReplacementLostOrDamagedDrivingLicenseToolStripMenuItem.Click += new System.EventHandler(this.ReplacementLostOrDamagedDrivingLicenseToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            // 
            // releaseDetainedDrivingLicenseToolStripMenuItem
            // 
            resources.ApplyResources(this.releaseDetainedDrivingLicenseToolStripMenuItem, "releaseDetainedDrivingLicenseToolStripMenuItem");
            this.releaseDetainedDrivingLicenseToolStripMenuItem.Image = global::DVLD.Properties.Resources.Detained_Driving_License_32;
            this.releaseDetainedDrivingLicenseToolStripMenuItem.Name = "releaseDetainedDrivingLicenseToolStripMenuItem";
            this.releaseDetainedDrivingLicenseToolStripMenuItem.Click += new System.EventHandler(this.releaseDetainedDrivingLicenseToolStripMenuItem_Click);
            // 
            // retakeTestToolStripMenuItem1
            // 
            resources.ApplyResources(this.retakeTestToolStripMenuItem1, "retakeTestToolStripMenuItem1");
            this.retakeTestToolStripMenuItem1.Image = global::DVLD.Properties.Resources.Retake_Test_32;
            this.retakeTestToolStripMenuItem1.Name = "retakeTestToolStripMenuItem1";
            this.retakeTestToolStripMenuItem1.Click += new System.EventHandler(this.retakeTestToolStripMenuItem1_Click);
            // 
            // toolStripSeparator6
            // 
            resources.ApplyResources(this.toolStripSeparator6, "toolStripSeparator6");
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            // 
            // tsMManageApplications
            // 
            resources.ApplyResources(this.tsMManageApplications, "tsMManageApplications");
            this.tsMManageApplications.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.manageLocalDrivingLicenseApplicationsToolStripMenuItem,
            this.ManageInternationaDrivingLicenseToolStripMenuItem1});
            this.tsMManageApplications.Image = global::DVLD.Properties.Resources.Manage_Applications_64;
            this.tsMManageApplications.Name = "tsMManageApplications";
            // 
            // manageLocalDrivingLicenseApplicationsToolStripMenuItem
            // 
            resources.ApplyResources(this.manageLocalDrivingLicenseApplicationsToolStripMenuItem, "manageLocalDrivingLicenseApplicationsToolStripMenuItem");
            this.manageLocalDrivingLicenseApplicationsToolStripMenuItem.Image = global::DVLD.Properties.Resources.LocalDriving_License;
            this.manageLocalDrivingLicenseApplicationsToolStripMenuItem.Name = "manageLocalDrivingLicenseApplicationsToolStripMenuItem";
            this.manageLocalDrivingLicenseApplicationsToolStripMenuItem.Click += new System.EventHandler(this.manageLocalDrivingLicenseApplicationsToolStripMenuItem_Click);
            // 
            // ManageInternationaDrivingLicenseToolStripMenuItem1
            // 
            resources.ApplyResources(this.ManageInternationaDrivingLicenseToolStripMenuItem1, "ManageInternationaDrivingLicenseToolStripMenuItem1");
            this.ManageInternationaDrivingLicenseToolStripMenuItem1.Image = global::DVLD.Properties.Resources.International_32;
            this.ManageInternationaDrivingLicenseToolStripMenuItem1.Name = "ManageInternationaDrivingLicenseToolStripMenuItem1";
            this.ManageInternationaDrivingLicenseToolStripMenuItem1.Click += new System.EventHandler(this.ManageInternationaDrivingLicenseToolStripMenuItem1_Click);
            // 
            // toolStripSeparator5
            // 
            resources.ApplyResources(this.toolStripSeparator5, "toolStripSeparator5");
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            // 
            // DetainLicensesToolStripMenuItem1
            // 
            resources.ApplyResources(this.DetainLicensesToolStripMenuItem1, "DetainLicensesToolStripMenuItem1");
            this.DetainLicensesToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ManageDetainedLicensestoolStripMenuItem1,
            this.detainLicenseToolStripMenuItem,
            this.releaseDetainedLicenseToolStripMenuItem});
            this.DetainLicensesToolStripMenuItem1.Image = global::DVLD.Properties.Resources.Detain_64;
            this.DetainLicensesToolStripMenuItem1.Name = "DetainLicensesToolStripMenuItem1";
            // 
            // ManageDetainedLicensestoolStripMenuItem1
            // 
            resources.ApplyResources(this.ManageDetainedLicensestoolStripMenuItem1, "ManageDetainedLicensestoolStripMenuItem1");
            this.ManageDetainedLicensestoolStripMenuItem1.Image = global::DVLD.Properties.Resources.Detain_32;
            this.ManageDetainedLicensestoolStripMenuItem1.Name = "ManageDetainedLicensestoolStripMenuItem1";
            this.ManageDetainedLicensestoolStripMenuItem1.Click += new System.EventHandler(this.ManageDetainedLicensestoolStripMenuItem1_Click);
            // 
            // detainLicenseToolStripMenuItem
            // 
            resources.ApplyResources(this.detainLicenseToolStripMenuItem, "detainLicenseToolStripMenuItem");
            this.detainLicenseToolStripMenuItem.Image = global::DVLD.Properties.Resources.Detain_32;
            this.detainLicenseToolStripMenuItem.Name = "detainLicenseToolStripMenuItem";
            this.detainLicenseToolStripMenuItem.Click += new System.EventHandler(this.detainLicenseToolStripMenuItem_Click);
            // 
            // releaseDetainedLicenseToolStripMenuItem
            // 
            resources.ApplyResources(this.releaseDetainedLicenseToolStripMenuItem, "releaseDetainedLicenseToolStripMenuItem");
            this.releaseDetainedLicenseToolStripMenuItem.Image = global::DVLD.Properties.Resources.Release_Detained_License_32;
            this.releaseDetainedLicenseToolStripMenuItem.Name = "releaseDetainedLicenseToolStripMenuItem";
            this.releaseDetainedLicenseToolStripMenuItem.Click += new System.EventHandler(this.releaseDetainedLicenseToolStripMenuItem_Click);
            // 
            // manageApplicationTypesToolStripMenuItem
            // 
            resources.ApplyResources(this.manageApplicationTypesToolStripMenuItem, "manageApplicationTypesToolStripMenuItem");
            this.manageApplicationTypesToolStripMenuItem.Image = global::DVLD.Properties.Resources.Application_Types_64;
            this.manageApplicationTypesToolStripMenuItem.Name = "manageApplicationTypesToolStripMenuItem";
            this.manageApplicationTypesToolStripMenuItem.Click += new System.EventHandler(this.manageApplicationTypesToolStripMenuItem_Click);
            // 
            // manageTestTypesToolStripMenuItem
            // 
            resources.ApplyResources(this.manageTestTypesToolStripMenuItem, "manageTestTypesToolStripMenuItem");
            this.manageTestTypesToolStripMenuItem.Image = global::DVLD.Properties.Resources.Test_Type_64;
            this.manageTestTypesToolStripMenuItem.Name = "manageTestTypesToolStripMenuItem";
            this.manageTestTypesToolStripMenuItem.Click += new System.EventHandler(this.manageTestTypesToolStripMenuItem_Click);
            // 
            // peopleToolStripMenuItem
            // 
            resources.ApplyResources(this.peopleToolStripMenuItem, "peopleToolStripMenuItem");
            this.peopleToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(64)))), ((int)(((byte)(120)))));
            this.peopleToolStripMenuItem.Name = "peopleToolStripMenuItem";
            this.peopleToolStripMenuItem.Click += new System.EventHandler(this.peopleToolStripMenuItem_Click);
            // 
            // driversToolStripMenuItem
            // 
            resources.ApplyResources(this.driversToolStripMenuItem, "driversToolStripMenuItem");
            this.driversToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(64)))), ((int)(((byte)(120)))));
            this.driversToolStripMenuItem.Image = global::DVLD.Properties.Resources.Drivers_64;
            this.driversToolStripMenuItem.Name = "driversToolStripMenuItem";
            this.driversToolStripMenuItem.Click += new System.EventHandler(this.driversToolStripMenuItem_Click);
            // 
            // employeesToolStripMenuItem
            // 
            resources.ApplyResources(this.employeesToolStripMenuItem, "employeesToolStripMenuItem");
            this.employeesToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(64)))), ((int)(((byte)(120)))));
            this.employeesToolStripMenuItem.Image = global::DVLD.Properties.Resources.Users_2_64;
            this.employeesToolStripMenuItem.Name = "employeesToolStripMenuItem";
            this.employeesToolStripMenuItem.Click += new System.EventHandler(this.employeesToolStripMenuItem_Click);
            // 
            // manageQuestionsToolStripMenuItem
            // 
            resources.ApplyResources(this.manageQuestionsToolStripMenuItem, "manageQuestionsToolStripMenuItem");
            this.manageQuestionsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(64)))), ((int)(((byte)(120)))));
            this.manageQuestionsToolStripMenuItem.Image = global::DVLD.Properties.Resources.List_32;
            this.manageQuestionsToolStripMenuItem.Name = "manageQuestionsToolStripMenuItem";
            this.manageQuestionsToolStripMenuItem.Click += new System.EventHandler(this.manageQuestionsToolStripMenuItem_Click);
            // 
            // mangeDecumentsToolStripMenuItem
            // 
            resources.ApplyResources(this.mangeDecumentsToolStripMenuItem, "mangeDecumentsToolStripMenuItem");
            this.mangeDecumentsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(64)))), ((int)(((byte)(120)))));
            this.mangeDecumentsToolStripMenuItem.Image = global::DVLD.Properties.Resources.explintion2;
            this.mangeDecumentsToolStripMenuItem.Name = "mangeDecumentsToolStripMenuItem";
            this.mangeDecumentsToolStripMenuItem.Click += new System.EventHandler(this.mangeDecumentsToolStripMenuItem_Click);
            // 
            // closeToolStripMenuItem
            // 
            resources.ApplyResources(this.closeToolStripMenuItem, "closeToolStripMenuItem");
            this.closeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.currentUserInfoToolStripMenuItem,
            this.changePasswordToolStripMenuItem,
            this.toolStripSeparator4,
            this.signOutToolStripMenuItem});
            this.closeToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(64)))), ((int)(((byte)(120)))));
            this.closeToolStripMenuItem.Image = global::DVLD.Properties.Resources.account_settings_64;
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            // 
            // currentUserInfoToolStripMenuItem
            // 
            resources.ApplyResources(this.currentUserInfoToolStripMenuItem, "currentUserInfoToolStripMenuItem");
            this.currentUserInfoToolStripMenuItem.Image = global::DVLD.Properties.Resources.PersonDetails_32;
            this.currentUserInfoToolStripMenuItem.Name = "currentUserInfoToolStripMenuItem";
            this.currentUserInfoToolStripMenuItem.Click += new System.EventHandler(this.currentUserInfoToolStripMenuItem_Click);
            // 
            // changePasswordToolStripMenuItem
            // 
            resources.ApplyResources(this.changePasswordToolStripMenuItem, "changePasswordToolStripMenuItem");
            this.changePasswordToolStripMenuItem.Image = global::DVLD.Properties.Resources.Password_32;
            this.changePasswordToolStripMenuItem.Name = "changePasswordToolStripMenuItem";
            this.changePasswordToolStripMenuItem.Click += new System.EventHandler(this.changePasswordToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            resources.ApplyResources(this.toolStripSeparator4, "toolStripSeparator4");
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            // 
            // signOutToolStripMenuItem
            // 
            resources.ApplyResources(this.signOutToolStripMenuItem, "signOutToolStripMenuItem");
            this.signOutToolStripMenuItem.Image = global::DVLD.Properties.Resources.sign_out_32__2;
            this.signOutToolStripMenuItem.Name = "signOutToolStripMenuItem";
            this.signOutToolStripMenuItem.Click += new System.EventHandler(this.signOutToolStripMenuItem_Click);
            // 
            // lblLoggedInUser
            // 
            resources.ApplyResources(this.lblLoggedInUser, "lblLoggedInUser");
            this.lblLoggedInUser.BackColor = System.Drawing.SystemColors.Control;
            this.lblLoggedInUser.Name = "lblLoggedInUser";
            // 
            // statusStripMain
            // 
            resources.ApplyResources(this.statusStripMain, "statusStripMain");
            this.statusStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatusSystem,
            this.lblStatusUser,
            this.lblStatusReady});
            this.statusStripMain.Name = "statusStripMain";
            this.statusStripMain.SizingGrip = false;
            // 
            // lblStatusSystem
            // 
            resources.ApplyResources(this.lblStatusSystem, "lblStatusSystem");
            this.lblStatusSystem.ForeColor = System.Drawing.Color.MidnightBlue;
            this.lblStatusSystem.Name = "lblStatusSystem";
            this.lblStatusSystem.Spring = true;
            // 
            // lblStatusUser
            // 
            resources.ApplyResources(this.lblStatusUser, "lblStatusUser");
            this.lblStatusUser.Name = "lblStatusUser";
            // 
            // lblStatusReady
            // 
            resources.ApplyResources(this.lblStatusReady, "lblStatusReady");
            this.lblStatusReady.Name = "lblStatusReady";
            // 
            // lblWelcomeTitle
            // 
            resources.ApplyResources(this.lblWelcomeTitle, "lblWelcomeTitle");
            this.lblWelcomeTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(65)))), ((int)(((byte)(125)))));
            this.lblWelcomeTitle.Name = "lblWelcomeTitle";
            // 
            // lblWelcomeSubtitle
            // 
            resources.ApplyResources(this.lblWelcomeSubtitle, "lblWelcomeSubtitle");
            this.lblWelcomeSubtitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblWelcomeSubtitle.Name = "lblWelcomeSubtitle";
            // 
            // lblWelcomeMessage
            // 
            resources.ApplyResources(this.lblWelcomeMessage, "lblWelcomeMessage");
            this.lblWelcomeMessage.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblWelcomeMessage.Name = "lblWelcomeMessage";
            // 
            // pnlWelcome
            // 
            resources.ApplyResources(this.pnlWelcome, "pnlWelcome");
            this.pnlWelcome.Controls.Add(this.lblWelcomeMessage);
            this.pnlWelcome.Controls.Add(this.picWelcomeIcon);
            this.pnlWelcome.Controls.Add(this.lblWelcomeSubtitle);
            this.pnlWelcome.Controls.Add(this.lblWelcomeTitle);
            this.pnlWelcome.Name = "pnlWelcome";
            this.pnlWelcome.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlWelcome_Paint);
            // 
            // picWelcomeIcon
            // 
            resources.ApplyResources(this.picWelcomeIcon, "picWelcomeIcon");
            this.picWelcomeIcon.Name = "picWelcomeIcon";
            this.picWelcomeIcon.TabStop = false;
            // 
            // pictureBox1
            // 
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(248)))), ((int)(((byte)(253)))));
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            // 
            // toolStripMenuItem1
            // 
            resources.ApplyResources(this.toolStripMenuItem1, "toolStripMenuItem1");
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem2,
            this.toolStripMenuItem3});
            this.toolStripMenuItem1.Image = global::DVLD.Properties.Resources.Manage_Applications_64;
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            // 
            // toolStripMenuItem2
            // 
            resources.ApplyResources(this.toolStripMenuItem2, "toolStripMenuItem2");
            this.toolStripMenuItem2.Image = global::DVLD.Properties.Resources.LocalDriving_License;
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.manageLocalDrivingLicenseApplicationsToolStripMenuItem_Click);
            // 
            // toolStripMenuItem3
            // 
            resources.ApplyResources(this.toolStripMenuItem3, "toolStripMenuItem3");
            this.toolStripMenuItem3.Image = global::DVLD.Properties.Resources.International_32;
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Click += new System.EventHandler(this.ManageInternationaDrivingLicenseToolStripMenuItem1_Click);
            // 
            // toolStripMenuItem4
            // 
            resources.ApplyResources(this.toolStripMenuItem4, "toolStripMenuItem4");
            this.toolStripMenuItem4.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem5,
            this.toolStripMenuItem6,
            this.toolStripMenuItem7});
            this.toolStripMenuItem4.Image = global::DVLD.Properties.Resources.Detain_64;
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            // 
            // toolStripMenuItem5
            // 
            resources.ApplyResources(this.toolStripMenuItem5, "toolStripMenuItem5");
            this.toolStripMenuItem5.Image = global::DVLD.Properties.Resources.Detain_32;
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Click += new System.EventHandler(this.ManageDetainedLicensestoolStripMenuItem1_Click);
            // 
            // toolStripMenuItem6
            // 
            resources.ApplyResources(this.toolStripMenuItem6, "toolStripMenuItem6");
            this.toolStripMenuItem6.Image = global::DVLD.Properties.Resources.Detain_32;
            this.toolStripMenuItem6.Name = "toolStripMenuItem6";
            this.toolStripMenuItem6.Click += new System.EventHandler(this.detainLicenseToolStripMenuItem_Click);
            // 
            // toolStripMenuItem7
            // 
            resources.ApplyResources(this.toolStripMenuItem7, "toolStripMenuItem7");
            this.toolStripMenuItem7.Image = global::DVLD.Properties.Resources.Release_Detained_License_32;
            this.toolStripMenuItem7.Name = "toolStripMenuItem7";
            this.toolStripMenuItem7.Click += new System.EventHandler(this.releaseDetainedLicenseToolStripMenuItem_Click);
            // 
            // frmMain
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.statusStripMain);
            this.Controls.Add(this.pnlWelcome);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.lblLoggedInUser);
            this.Controls.Add(this.msMainMenue);
            this.DoubleBuffered = true;
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.msMainMenue;
            this.Name = "frmMain";
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.msMainMenue.ResumeLayout(false);
            this.msMainMenue.PerformLayout();
            this.statusStripMain.ResumeLayout(false);
            this.statusStripMain.PerformLayout();
            this.pnlWelcome.ResumeLayout(false);
            this.pnlWelcome.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picWelcomeIcon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip msMainMenue;
        private System.Windows.Forms.ToolStripMenuItem servicesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem drivingLicensesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem oNewDrivingLicenseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem renewDrivingLicenseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ReplacementLostOrDamagedDrivingLicenseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem peopleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem driversToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem employeesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem localLicenseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem internationalLicenseToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem releaseDetainedDrivingLicenseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem retakeTestToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.Label lblLoggedInUser;
        private System.Windows.Forms.ToolStripMenuItem currentUserInfoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem signOutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem changePasswordToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem manageApplicationTypesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem manageTestTypesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tsMManageApplications;
        private System.Windows.Forms.ToolStripMenuItem manageLocalDrivingLicenseApplicationsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ToolStripMenuItem ManageInternationaDrivingLicenseToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem DetainLicensesToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem detainLicenseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem releaseDetainedLicenseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ManageDetainedLicensestoolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem manageQuestionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mangeDecumentsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem5;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem6;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem7;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
        private System.Windows.Forms.StatusStrip statusStripMain;
        private System.Windows.Forms.ToolStripStatusLabel lblStatusSystem;
        private System.Windows.Forms.ToolStripStatusLabel lblStatusUser;
        private System.Windows.Forms.ToolStripStatusLabel lblStatusReady;
        private System.Windows.Forms.Label lblWelcomeTitle;
        private System.Windows.Forms.Label lblWelcomeSubtitle;
        private System.Windows.Forms.PictureBox picWelcomeIcon;
        private System.Windows.Forms.Label lblWelcomeMessage;
        private System.Windows.Forms.Panel pnlWelcome;
    }
}

