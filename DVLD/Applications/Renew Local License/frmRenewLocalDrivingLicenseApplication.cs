using DVLD.Classes;
using DVLD.DriverLicense;
using DVLD.Licenses.International_License;
using DVLD.Licenses.International_Licenses;
using DVLD_Buisness;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DVLD.Licenses
{
    public partial class frmRenewLocalDrivingLicenseApplication: Form
    {
        private int _NewLicenseID = -1;
        public frmRenewLocalDrivingLicenseApplication()
        {
            InitializeComponent();
        }


        private string _Translate(string arabic, string english)
        {
            bool isArabic =
                System.Globalization.CultureInfo.CurrentUICulture
                .TwoLetterISOLanguageName == "ar";

            return isArabic ? arabic : english;
        }


        private void frmRenewLocalDrivingLicenseApplication_Load(object sender, EventArgs e)
        {
            ctrlDriverLicenseInfoWithFilter1.txtLicenseIDFocus();


            lblApplicationDate.Text = clsFormat.DateToShort(DateTime.Now);
            lblIssueDate.Text = lblApplicationDate.Text;

            lblExpirationDate.Text = "???";
            lblApplicationFees.Text = clsApplicationType.Find((int)clsApplication.enApplicationType.RenewDrivingLicense).Fees.ToString();
            lblCreatedByUser.Text = clsGlobal.CurrentUser.UserName;

        }

        private void ctrlDriverLicenseInfoWithFilter1_OnLicenseSelected(int obj)
        {
            int SelectedLicenseID = obj;

            lblOldLicenseID.Text = SelectedLicenseID.ToString();

            llShowLicenseHistory.Enabled = (SelectedLicenseID != -1);

            if (SelectedLicenseID == -1)

            {
                return;
            }

            int DefaultValidityLength = ctrlDriverLicenseInfoWithFilter1.SelectedLicenseInfo.LicenseClassIfo.DefaultValidityLength;
            lblExpirationDate.Text = clsFormat.DateToShort(DateTime.Now.AddYears(DefaultValidityLength));
            lblLicenseFees.Text = ctrlDriverLicenseInfoWithFilter1.SelectedLicenseInfo.LicenseClassIfo.ClassFees.ToString();
            lblTotalFees.Text = (Convert.ToSingle(lblApplicationFees.Text) + Convert.ToSingle(lblLicenseFees.Text)).ToString();
            txtNotes.Text = ctrlDriverLicenseInfoWithFilter1.SelectedLicenseInfo.Notes;


            //check the license is not Expired.
            if (!ctrlDriverLicenseInfoWithFilter1.SelectedLicenseInfo.IsLicenseExpired())
            {

                MessageBox.Show(
                    _Translate(
                        "لا يمكن تجديد الرخصة لأنها لم تنتهِ بعد. تاريخ انتهاء الرخصة: ",
                        "The selected license has not expired yet. Expiration date: ")
                    + clsFormat.DateToShort(
                        ctrlDriverLicenseInfoWithFilter1.SelectedLicenseInfo.ExpirationDate),
                    _Translate("غير مسموح", "Not allowed"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                btnRenewLicense.Enabled = false;
                return;
            }

            //check the license is not Expired.
            if (!ctrlDriverLicenseInfoWithFilter1.SelectedLicenseInfo.IsActive)
            {
                MessageBox.Show(
                    _Translate(
                        "لا يمكن تجديد رخصة غير نشطة. يرجى اختيار رخصة نشطة.",
                        "The selected license is not active. Please choose an active license."),
                    _Translate("غير مسموح", "Not allowed"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                btnRenewLicense.Enabled = false;
                return;
            }


           
            btnRenewLicense.Enabled = true;

        }


        private void frmRenewLocalDrivingLicenseApplication_Activated(object sender, EventArgs e)
        {
            ctrlDriverLicenseInfoWithFilter1.txtLicenseIDFocus();

        }

        private void llShowLicenseHistory_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            frmShowPersonLicenseHistory frm =
               new frmShowPersonLicenseHistory(ctrlDriverLicenseInfoWithFilter1.SelectedLicenseInfo.DriverInfo.PersonID);
            frm.ShowDialog();
        }

        private void llShowLicenseInfo_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            frmShowLicenseInfo frm =
           new frmShowLicenseInfo(_NewLicenseID);
            frm.ShowDialog();
        }

        private void btnRenewLicense_Click_1(object sender, EventArgs e)
        {

            if (MessageBox.Show(
                    _Translate(
                        "هل أنت متأكد من رغبتك في تجديد رخصة القيادة؟",
                        "Are you sure you want to renew the license?"),
                    _Translate("تأكيد تجديد الرخصة", "Confirm"),
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }



            clsLicense NewLicense =
                ctrlDriverLicenseInfoWithFilter1.SelectedLicenseInfo.RenewLicense(txtNotes.Text.Trim(),
                clsGlobal.CurrentUser.UserID);

            if (NewLicense == null)
            {

                MessageBox.Show(
                    _Translate(
                        "فشل تجديد رخصة القيادة.",
                        "Failed to renew the license."),
                    _Translate("خطأ", "Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            lblApplicationID.Text = NewLicense.ApplicationID.ToString();
            _NewLicenseID = NewLicense.LicenseID;
            lblRenewedLicenseID.Text = _NewLicenseID.ToString();

            MessageBox.Show(
                _Translate(
                    "تم تجديد رخصة القيادة بنجاح. رقم الرخصة الجديدة: ",
                    "License renewed successfully with ID = ")
                + _NewLicenseID,
                _Translate("تم تجديد الرخصة", "License Renewed"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            btnRenewLicense.Enabled = false;
            ctrlDriverLicenseInfoWithFilter1.FilterEnabled = false;
            llShowLicenseInfo.Enabled = true;

        }

        private void btnClose_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
