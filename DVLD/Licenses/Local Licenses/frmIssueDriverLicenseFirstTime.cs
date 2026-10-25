
using DVLD.Classes;
using DVLD_Buisness;
using System;
using System.Windows.Forms;

namespace DVLD.DriverLicense
{
    public partial class frmIssueDriverLicenseFirstTime : Form
    {
        private int _LocalDrivingLicenseApplicationID;

        private clsLocalDrivingLicenseApplication
            _LocalDrivingLicenseApplication;

        public frmIssueDriverLicenseFirstTime(
            int LocalDrivingLicenseApplicationID)
        {
            InitializeComponent();

            _LocalDrivingLicenseApplicationID =
                LocalDrivingLicenseApplicationID;
        }

        private string _Translate(string arabic, string english)
        {
            bool isArabic =
                System.Globalization.CultureInfo.CurrentUICulture
                .TwoLetterISOLanguageName == "ar";

            return isArabic ? arabic : english;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void frmIssueDriverLicenseFirstTime_Load(
            object sender, EventArgs e)
        {
            txtNotes.Focus();

            _LocalDrivingLicenseApplication =
                clsLocalDrivingLicenseApplication
                .FindByLocalDrivingAppLicenseID(
                    _LocalDrivingLicenseApplicationID);

            if (_LocalDrivingLicenseApplication == null)
            {
                MessageBox.Show(
                    _Translate(
                        "لم يتم العثور على طلب رخصة القيادة رقم: ",
                        "No application was found with ID = ")
                        + _LocalDrivingLicenseApplicationID,
                    _Translate("العملية غير مسموحة", "Not Allowed"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                this.Close();
                return;
            }

            if (!_LocalDrivingLicenseApplication.PassedAllTests())
            {
                MessageBox.Show(
                    _Translate(
                        "يجب على المتقدم اجتياز جميع الاختبارات أولًا قبل إصدار الرخصة.",
                        "The applicant must pass all tests before a license can be issued."),
                    _Translate("العملية غير مسموحة", "Not Allowed"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                this.Close();
                return;
            }

            int LicenseID =
                _LocalDrivingLicenseApplication.GetActiveLicenseID();

            if (LicenseID != -1)
            {
                MessageBox.Show(
                    _Translate(
                        "يمتلك هذا الشخص رخصة قيادة فعالة بالفعل، رقم الرخصة: ",
                        "The applicant already has an active license with ID = ")
                        + LicenseID,
                    _Translate("العملية غير مسموحة", "Not Allowed"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                this.Close();
                return;
            }

            ctrlDrivingLicenseApplicationInfo1
                .LoadApplicationInfoByLocalDrivingAppID(
                    _LocalDrivingLicenseApplicationID);
        }

        private void btnIssueLicense_Click(
            object sender, EventArgs e)
        {
            if (_LocalDrivingLicenseApplication == null)
                return;

            btnIssueLicense.Enabled = false;

            try
            {
                int LicenseID =
                    _LocalDrivingLicenseApplication
                    .IssueLicenseForTheFirtTime(
                        txtNotes.Text.Trim(),
                        clsGlobal.CurrentUser.UserID);

                if (LicenseID != -1)
                {
                    MessageBox.Show(
                        _Translate(
                            "تم إصدار رخصة القيادة بنجاح.\n\nرقم الرخصة: ",
                            "Driving license issued successfully.\n\nLicense ID: ")
                            + LicenseID,
                        _Translate("تم إصدار الرخصة", "Succeeded"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    this.Close();
                }
                else
                {
                    MessageBox.Show(
                        _Translate(
                            "تعذّر إصدار رخصة القيادة. يرجى المحاولة مرة أخرى.",
                            "The driving license could not be issued."),
                        _Translate("فشل إصدار الرخصة", "Failed"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            finally
            {
                if (!this.IsDisposed)
                    btnIssueLicense.Enabled = true;
            }
        }
    }
}
