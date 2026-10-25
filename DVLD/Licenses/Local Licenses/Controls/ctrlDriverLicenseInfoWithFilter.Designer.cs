namespace DVLD.Licenses.Controls
{
    partial class ctrlDriverLicenseInfoWithFilter
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ctrlDriverLicenseInfoWithFilter));
            this.gbFilters = new System.Windows.Forms.GroupBox();
            this.txtLicenseID = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.ctrlDriverLicenseInfo1 = new DVLD.DriverLicense.ctrlDriverLicenseInfo();
            this.btnFind = new System.Windows.Forms.Button();
            this.gbFilters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.SuspendLayout();
            // 
            // gbFilters
            // 
            resources.ApplyResources(this.gbFilters, "gbFilters");
            this.gbFilters.Controls.Add(this.btnFind);
            this.gbFilters.Controls.Add(this.txtLicenseID);
            this.gbFilters.Controls.Add(this.label1);
            this.gbFilters.Name = "gbFilters";
            this.gbFilters.TabStop = false;
            // 
            // txtLicenseID
            // 
            resources.ApplyResources(this.txtLicenseID, "txtLicenseID");
            this.txtLicenseID.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtLicenseID.Name = "txtLicenseID";
            this.txtLicenseID.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtLicenseID_KeyPress);
            this.txtLicenseID.Validating += new System.ComponentModel.CancelEventHandler(this.txtLicenseID_Validating);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(61)))), ((int)(((byte)(125)))));
            this.label1.Name = "label1";
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            resources.ApplyResources(this.errorProvider1, "errorProvider1");
            // 
            // ctrlDriverLicenseInfo1
            // 
            resources.ApplyResources(this.ctrlDriverLicenseInfo1, "ctrlDriverLicenseInfo1");
            this.ctrlDriverLicenseInfo1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(248)))), ((int)(((byte)(253)))));
            this.errorProvider1.SetError(this.ctrlDriverLicenseInfo1, resources.GetString("ctrlDriverLicenseInfo1.Error"));
            this.errorProvider1.SetIconAlignment(this.ctrlDriverLicenseInfo1, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("ctrlDriverLicenseInfo1.IconAlignment"))));
            this.errorProvider1.SetIconPadding(this.ctrlDriverLicenseInfo1, ((int)(resources.GetObject("ctrlDriverLicenseInfo1.IconPadding"))));
            this.ctrlDriverLicenseInfo1.Name = "ctrlDriverLicenseInfo1";
            // 
            // btnFind
            // 
            resources.ApplyResources(this.btnFind, "btnFind");
            this.errorProvider1.SetError(this.btnFind, resources.GetString("btnFind.Error"));
            this.errorProvider1.SetIconAlignment(this.btnFind, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("btnFind.IconAlignment"))));
            this.errorProvider1.SetIconPadding(this.btnFind, ((int)(resources.GetObject("btnFind.IconPadding"))));
            this.btnFind.Image = global::DVLD.Properties.Resources.License_View_32;
            this.btnFind.Name = "btnFind";
            this.btnFind.UseVisualStyleBackColor = true;
            this.btnFind.Click += new System.EventHandler(this.btnFind_Click);
            // 
            // ctrlDriverLicenseInfoWithFilter
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.gbFilters);
            this.Controls.Add(this.ctrlDriverLicenseInfo1);
            this.Name = "ctrlDriverLicenseInfoWithFilter";
            this.gbFilters.ResumeLayout(false);
            this.gbFilters.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DriverLicense.ctrlDriverLicenseInfo ctrlDriverLicenseInfo1;
        private System.Windows.Forms.GroupBox gbFilters;
        private System.Windows.Forms.Button btnFind;
        private System.Windows.Forms.TextBox txtLicenseID;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ErrorProvider errorProvider1;
    }
}
