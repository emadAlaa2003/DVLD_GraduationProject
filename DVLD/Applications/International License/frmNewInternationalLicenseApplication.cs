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
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static DVLD_Buisness.clsApplication;

namespace DVLD.Applications.International_License
{
    public partial class frmNewInternationalLicenseApplication : Form
    {
        
        private int _InternationalLicenseID = -1;
      
        public frmNewInternationalLicenseApplication()
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


        private void ctrlDriverLicenseInfoWithFilter1_OnLicenseSelected(int obj)
        {
            int SelectedLicenseID = obj;

            lblLocalLicenseID.Text = SelectedLicenseID.ToString();

            llShowLicenseHistory.Enabled = (SelectedLicenseID != -1);

            if (SelectedLicenseID == -1) 
            
            {
                return;
            }

            

          
            //check the license class, person could not issue international license without having
            //normal license of class 3.

            if (ctrlDriverLicenseInfoWithFilter1.SelectedLicenseInfo.LicenseClass != 3)
            {

                MessageBox.Show(
                    _Translate(
                        "يجب أن تكون الرخصة المختارة من الفئة الثالثة. يرجى اختيار رخصة أخرى.",
                        "Selected License should be Class 3, select another one."),
                    _Translate("غير مسموح", "Not allowed"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            //check if person already have an active international license
            int ActiveInternaionalLicenseID = clsInternationalLicense.GetActiveInternationalLicenseIDByDriverID(ctrlDriverLicenseInfoWithFilter1.SelectedLicenseInfo.DriverID);

            if (ActiveInternaionalLicenseID != -1)
            {

                MessageBox.Show(
                    _Translate(
                        "يمتلك هذا الشخص رخصة قيادة دولية سارية بالفعل، رقم الرخصة: ",
                        "Person already has an active international license with ID = ")
                        + ActiveInternaionalLicenseID,
                    _Translate("غير مسموح", "Not allowed"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                llShowLicenseInfo.Enabled = true;
                _InternationalLicenseID = ActiveInternaionalLicenseID;
                btnIssueLicense.Enabled = false;
                return;
            }

            btnIssueLicense.Enabled = true;

        }

        private void frmNewInternationalLicenseApplication_Load(object sender, EventArgs e)
        {
           

            lblApplicationDate.Text = clsFormat.DateToShort(DateTime.Now);
            lblIssueDate.Text = lblApplicationDate.Text;
            lblExpirationDate.Text = clsFormat.DateToShort(DateTime.Now.AddYears(1));//add one year.
            lblFees.Text = clsApplicationType.Find((int)clsApplication.enApplicationType.NewInternationalLicense).Fees.ToString();
            lblCreatedByUser.Text = clsGlobal.CurrentUser.UserName;


        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnIssueLicense_Click(object sender, EventArgs e)
        {


            if (MessageBox.Show(
                    _Translate(
                        "هل أنت متأكد من إصدار رخصة القيادة الدولية؟",
                        "Are you sure you want to issue the license?"),
                    _Translate("تأكيد الإصدار", "Confirm"),
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }






            clsInternationalLicense InternationalLicense = new clsInternationalLicense();
            //those are the information for the base application, because it inhirts from application, they are part of the sub class.

            InternationalLicense.ApplicantPersonID = ctrlDriverLicenseInfoWithFilter1.SelectedLicenseInfo.DriverInfo.PersonID;
            InternationalLicense.ApplicationDate = DateTime.Now;
            InternationalLicense.ApplicationStatus = clsApplication.enApplicationStatus.Completed;
            InternationalLicense.LastStatusDate = DateTime.Now;
            InternationalLicense.PaidFees = clsApplicationType.Find((int)clsApplication.enApplicationType.NewInternationalLicense).Fees;
            InternationalLicense.CreatedByUserID = clsGlobal.CurrentUser.UserID;

           
            InternationalLicense.DriverID= ctrlDriverLicenseInfoWithFilter1.SelectedLicenseInfo.DriverID;
            InternationalLicense.IssuedUsingLocalLicenseID = ctrlDriverLicenseInfoWithFilter1.SelectedLicenseInfo.LicenseID;
            InternationalLicense.IssueDate= DateTime.Now;
            InternationalLicense.ExpirationDate= DateTime.Now.AddYears(1);
           
            InternationalLicense.CreatedByUserID = clsGlobal.CurrentUser.UserID;

            if (!InternationalLicense.Save())
            {

                MessageBox.Show(
                    _Translate(
                        "فشل إصدار رخصة القيادة الدولية.",
                        "Failed to Issue International License."),
                    _Translate("خطأ", "Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            lblApplicationID.Text = InternationalLicense.ApplicationID.ToString();
            _InternationalLicenseID = InternationalLicense.InternationalLicenseID;
            lblInternationalLicenseID.Text = InternationalLicense.InternationalLicenseID.ToString();

            MessageBox.Show(
                _Translate(
                    "تم إصدار رخصة القيادة الدولية بنجاح، رقم الرخصة: ",
                    "International License Issued Successfully with ID = ")
                    + InternationalLicense.InternationalLicenseID,
                _Translate("تم إصدار الرخصة", "License Issued"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            btnIssueLicense.Enabled = false;
            ctrlDriverLicenseInfoWithFilter1.FilterEnabled = false;
            llShowLicenseInfo.Enabled = true;


        }

        private void llShowDriverLicenseHistory_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
           

            frmShowPersonLicenseHistory  frm = 
                new frmShowPersonLicenseHistory(ctrlDriverLicenseInfoWithFilter1.SelectedLicenseInfo.DriverInfo.PersonID);
            frm.ShowDialog();

        }

        private void llShowLicenseInfo_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            frmShowInternationalLicenseInfo frm =
              new frmShowInternationalLicenseInfo(_InternationalLicenseID);
            frm.ShowDialog();
        }

        private void frmNewInternationalLicenseApplication_Activated(object sender, EventArgs e)
        {
            ctrlDriverLicenseInfoWithFilter1.txtLicenseIDFocus();
        }
    }
}
