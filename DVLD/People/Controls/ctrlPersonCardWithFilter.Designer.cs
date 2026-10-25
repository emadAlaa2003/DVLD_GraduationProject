namespace DVLD.Controls
{
    partial class ctrlPersonCardWithFilter
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ctrlPersonCardWithFilter));
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.gbFilters = new System.Windows.Forms.GroupBox();
            this.cbFilterBy = new System.Windows.Forms.ComboBox();
            this.txtFilterValue = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ctrlPersonCard1 = new DVLD.Controls.ctrlPersonCard();
            this.btnAddNewPerson = new System.Windows.Forms.Button();
            this.btnFind = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.gbFilters.SuspendLayout();
            this.SuspendLayout();
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            resources.ApplyResources(this.errorProvider1, "errorProvider1");
            // 
            // gbFilters
            // 
            resources.ApplyResources(this.gbFilters, "gbFilters");
            this.gbFilters.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(248)))), ((int)(((byte)(253)))));
            this.gbFilters.Controls.Add(this.btnAddNewPerson);
            this.gbFilters.Controls.Add(this.btnFind);
            this.gbFilters.Controls.Add(this.cbFilterBy);
            this.gbFilters.Controls.Add(this.txtFilterValue);
            this.gbFilters.Controls.Add(this.label1);
            this.errorProvider1.SetError(this.gbFilters, resources.GetString("gbFilters.Error"));
            this.errorProvider1.SetIconAlignment(this.gbFilters, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("gbFilters.IconAlignment"))));
            this.errorProvider1.SetIconPadding(this.gbFilters, ((int)(resources.GetObject("gbFilters.IconPadding"))));
            this.gbFilters.Name = "gbFilters";
            this.gbFilters.TabStop = false;
            // 
            // cbFilterBy
            // 
            resources.ApplyResources(this.cbFilterBy, "cbFilterBy");
            this.cbFilterBy.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.errorProvider1.SetError(this.cbFilterBy, resources.GetString("cbFilterBy.Error"));
            this.cbFilterBy.FormattingEnabled = true;
            this.errorProvider1.SetIconAlignment(this.cbFilterBy, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("cbFilterBy.IconAlignment"))));
            this.errorProvider1.SetIconPadding(this.cbFilterBy, ((int)(resources.GetObject("cbFilterBy.IconPadding"))));
            this.cbFilterBy.Items.AddRange(new object[] {
            resources.GetString("cbFilterBy.Items"),
            resources.GetString("cbFilterBy.Items1")});
            this.cbFilterBy.Name = "cbFilterBy";
            // 
            // txtFilterValue
            // 
            resources.ApplyResources(this.txtFilterValue, "txtFilterValue");
            this.txtFilterValue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.errorProvider1.SetError(this.txtFilterValue, resources.GetString("txtFilterValue.Error"));
            this.errorProvider1.SetIconAlignment(this.txtFilterValue, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("txtFilterValue.IconAlignment"))));
            this.errorProvider1.SetIconPadding(this.txtFilterValue, ((int)(resources.GetObject("txtFilterValue.IconPadding"))));
            this.txtFilterValue.Name = "txtFilterValue";
            this.txtFilterValue.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtFilterValue_KeyPress);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.errorProvider1.SetError(this.label1, resources.GetString("label1.Error"));
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(61)))), ((int)(((byte)(125)))));
            this.errorProvider1.SetIconAlignment(this.label1, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("label1.IconAlignment"))));
            this.errorProvider1.SetIconPadding(this.label1, ((int)(resources.GetObject("label1.IconPadding"))));
            this.label1.Name = "label1";
            // 
            // ctrlPersonCard1
            // 
            resources.ApplyResources(this.ctrlPersonCard1, "ctrlPersonCard1");
            this.ctrlPersonCard1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(248)))), ((int)(((byte)(253)))));
            this.errorProvider1.SetError(this.ctrlPersonCard1, resources.GetString("ctrlPersonCard1.Error"));
            this.errorProvider1.SetIconAlignment(this.ctrlPersonCard1, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("ctrlPersonCard1.IconAlignment"))));
            this.errorProvider1.SetIconPadding(this.ctrlPersonCard1, ((int)(resources.GetObject("ctrlPersonCard1.IconPadding"))));
            this.ctrlPersonCard1.Name = "ctrlPersonCard1";
            // 
            // btnAddNewPerson
            // 
            resources.ApplyResources(this.btnAddNewPerson, "btnAddNewPerson");
            this.errorProvider1.SetError(this.btnAddNewPerson, resources.GetString("btnAddNewPerson.Error"));
            this.errorProvider1.SetIconAlignment(this.btnAddNewPerson, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("btnAddNewPerson.IconAlignment"))));
            this.errorProvider1.SetIconPadding(this.btnAddNewPerson, ((int)(resources.GetObject("btnAddNewPerson.IconPadding"))));
            this.btnAddNewPerson.Image = global::DVLD.Properties.Resources.AddPerson_32;
            this.btnAddNewPerson.Name = "btnAddNewPerson";
            this.btnAddNewPerson.UseVisualStyleBackColor = true;
            this.btnAddNewPerson.Click += new System.EventHandler(this.btnAddNewPerson_Click);
            // 
            // btnFind
            // 
            resources.ApplyResources(this.btnFind, "btnFind");
            this.errorProvider1.SetError(this.btnFind, resources.GetString("btnFind.Error"));
            this.errorProvider1.SetIconAlignment(this.btnFind, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("btnFind.IconAlignment"))));
            this.errorProvider1.SetIconPadding(this.btnFind, ((int)(resources.GetObject("btnFind.IconPadding"))));
            this.btnFind.Image = global::DVLD.Properties.Resources.SearchPerson;
            this.btnFind.Name = "btnFind";
            this.btnFind.UseVisualStyleBackColor = true;
            this.btnFind.Click += new System.EventHandler(this.btnFind_Click);
            // 
            // ctrlPersonCardWithFilter
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(248)))), ((int)(((byte)(253)))));
            this.Controls.Add(this.gbFilters);
            this.Controls.Add(this.ctrlPersonCard1);
            this.Name = "ctrlPersonCardWithFilter";
            this.Load += new System.EventHandler(this.ctrlPersonCardWithFilter_Load);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.gbFilters.ResumeLayout(false);
            this.gbFilters.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private ctrlPersonCard ctrlPersonCard1;
        private System.Windows.Forms.ErrorProvider errorProvider1;
        private System.Windows.Forms.GroupBox gbFilters;
        private System.Windows.Forms.Button btnAddNewPerson;
        private System.Windows.Forms.Button btnFind;
        private System.Windows.Forms.ComboBox cbFilterBy;
        private System.Windows.Forms.TextBox txtFilterValue;
        private System.Windows.Forms.Label label1;
    }
}
