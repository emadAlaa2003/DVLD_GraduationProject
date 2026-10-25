using DVLD.Classes;
using DVLD.Properties;
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
using System.IO;

namespace DVLD.Licenses.International_Licenses.Controls
{
    public partial class ctrlDriverInternationalLicenseInfo : UserControl
    {
        private int _InternationalLicenseID;
        private clsInternationalLicense _InternationalLicense;
        public ctrlDriverInternationalLicenseInfo()
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

        public int InternationalLicenseID
        {
            get { return _InternationalLicenseID; }
        }


        private void _LoadPersonImage()
        {
            // Clear the previous image path.
            pbPersonImage.ImageLocation = null;

            if (_InternationalLicense.DriverInfo.PersonInfo.Gendor == 0)
                pbPersonImage.Image = Resources.Male_512;
            else
                pbPersonImage.Image = Resources.Female_512;

            string imagePath =
                _InternationalLicense.DriverInfo.PersonInfo.ImagePath;

            if (string.IsNullOrWhiteSpace(imagePath))
                return;

            if (File.Exists(imagePath))
            {
                pbPersonImage.ImageLocation = imagePath;
            }
            else
            {
                MessageBox.Show(
                    _Translate(
                        "لم يتم العثور على صورة السائق في المسار:\n",
                        "Could not find this image:\n")
                        + imagePath,
                    _Translate("خطأ", "Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


        public void LoadInfo(int InternationalLicenseID)
        {
            _InternationalLicenseID = InternationalLicenseID;
            _InternationalLicense = clsInternationalLicense.Find(_InternationalLicenseID);
            if (_InternationalLicense == null)
            {

                MessageBox.Show(
                    _Translate(
                        "لم يتم العثور على رخصة قيادة دولية برقم: ",
                        "Could not find International License ID = ")
                        + _InternationalLicenseID,
                    _Translate("خطأ", "Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                _InternationalLicenseID = -1;
                return;
            }

            lblInternationalLicenseID.Text = _InternationalLicense.InternationalLicenseID.ToString();
            lblApplicationID.Text = _InternationalLicense.ApplicationID.ToString();

            lblIsActive.Text =
                _InternationalLicense.IsActive
                    ? _Translate("نعم", "Yes")
                    : _Translate("لا", "No");

            lblLocalLicenseID.Text = _InternationalLicense.IssuedUsingLocalLicenseID.ToString();
            lblFullName.Text = _InternationalLicense.DriverInfo.PersonInfo.FullName;
            lblNationalNo.Text = _InternationalLicense.DriverInfo.PersonInfo.NationalNo;

            lblGendor.Text =
                _InternationalLicense.DriverInfo.PersonInfo.Gendor == 0
                    ? _Translate("ذكر", "Male")
                    : _Translate("أنثى", "Female");
            lblDateOfBirth.Text = clsFormat.DateToShort(_InternationalLicense.DriverInfo.PersonInfo.DateOfBirth);

            lblDriverID.Text = _InternationalLicense.DriverID.ToString();
            lblIssueDate.Text = clsFormat.DateToShort(_InternationalLicense.IssueDate);
            lblExpirationDate.Text = clsFormat.DateToShort(_InternationalLicense.ExpirationDate);
           
            _LoadPersonImage();



        }

     
    }
}
