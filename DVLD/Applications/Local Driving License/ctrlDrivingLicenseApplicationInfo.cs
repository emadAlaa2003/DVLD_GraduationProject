using DVLD.DriverLicense;
using DVLD_Buisness;
using System.Windows.Forms;

namespace DVLD.Controls.ApplicationControls
{
    public partial class ctrlDrivingLicenseApplicationInfo : UserControl
    {

        private clsLocalDrivingLicenseApplication _LocalDrivingLicenseApplication;

        private int _LocalDrivingLicenseApplicationID = -1;

        private int _LicenseID;

        public int LocalDrivingLicenseApplicationID
        {
            get { return _LocalDrivingLicenseApplicationID; }
        }

        public ctrlDrivingLicenseApplicationInfo()
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


        public void LoadApplicationInfoByLocalDrivingAppID(
            int LocalDrivingLicenseApplicationID)
        {
            _LocalDrivingLicenseApplication =
                clsLocalDrivingLicenseApplication
                .FindByLocalDrivingAppLicenseID(
                    LocalDrivingLicenseApplicationID);

            if (_LocalDrivingLicenseApplication == null)
            {
                _ResetLocalDrivingLicenseApplicationInfo();

                MessageBox.Show(
                    _Translate(
                        "لم يتم العثور على طلب رخصة القيادة المحلية رقم: ",
                        "No local driving license application found with ID = ")
                        + LocalDrivingLicenseApplicationID,
                    _Translate("خطأ", "Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            _FillLocalDrivingLicenseApplicationInfo();
        }



        public void LoadApplicationInfoByApplicationID(int ApplicationID)
        {
            _LocalDrivingLicenseApplication =
                clsLocalDrivingLicenseApplication
                .FindByApplicationID(ApplicationID);

            if (_LocalDrivingLicenseApplication == null)
            {
                _ResetLocalDrivingLicenseApplicationInfo();

                MessageBox.Show(
                    _Translate(
                        "لم يتم العثور على طلب برقم: ",
                        "No application found with Application ID = ")
                        + ApplicationID,
                    _Translate("خطأ", "Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            _FillLocalDrivingLicenseApplicationInfo();
        }



        private void _FillLocalDrivingLicenseApplicationInfo()
        {
            _LocalDrivingLicenseApplicationID =
                _LocalDrivingLicenseApplication
                .LocalDrivingLicenseApplicationID;

            _LicenseID =
                _LocalDrivingLicenseApplication.GetActiveLicenseID();

            llShowLicenceInfo.Enabled = (_LicenseID != -1);

            lblLocalDrivingLicenseApplicationID.Text =
                _LocalDrivingLicenseApplicationID.ToString();

            clsLicenseClass licenseClass =
                clsLicenseClass.Find(
                    _LocalDrivingLicenseApplication.LicenseClassID);

            lblAppliedFor.Text =
                licenseClass != null
                    ? licenseClass.ClassName
                    : _Translate("غير متوفر", "N/A");

            lblPassedTests.Text =
                _LocalDrivingLicenseApplication
                .GetPassedTestCount().ToString() + "/3";

            ctrlApplicationBasicInfo1.LoadApplicationInfo(
                _LocalDrivingLicenseApplication.ApplicationID);
        }



        private void _ResetLocalDrivingLicenseApplicationInfo()
        {
            _LocalDrivingLicenseApplication = null;
            _LocalDrivingLicenseApplicationID = -1;
            _LicenseID = -1;

            lblLocalDrivingLicenseApplicationID.Text = "[????]";
            lblAppliedFor.Text = "[????]";
            lblPassedTests.Text = "[????]";

            llShowLicenceInfo.Enabled = false;

            ctrlApplicationBasicInfo1.ResetApplicationInfo();
        }


        private void llShowLicenceInfo_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            frmShowLicenseInfo frm = new frmShowLicenseInfo(_LocalDrivingLicenseApplication.GetActiveLicenseID());
            frm.ShowDialog();

        }
    }
}
