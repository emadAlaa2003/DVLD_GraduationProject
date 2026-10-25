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
using System.Xml.Linq;
using System.IO;
using DVLD.People;

namespace DVLD.Controls
{
    public partial class ctrlPersonCard : UserControl
    {
        private clsPerson _Person;

        private int  _PersonID=-1;

        public int PersonID   
        {
            get { return _PersonID; }   
        }

        public clsPerson SelectedPersonInfo
        {
            get { return _Person; }
        }

        public ctrlPersonCard()
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

        public void LoadPersonInfo(int PersonID)
        {
            _Person=clsPerson.Find(PersonID);
            if (_Person == null)
            {
                ResetPersonInfo();

                MessageBox.Show(
                    _Translate(
                        "لم يتم العثور على الشخص رقم: ",
                        "No Person with PersonID = ") + PersonID,
                    _Translate("خطأ", "Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
           
                _FillPersonInfo();
        }

        public void LoadPersonInfo(string NationalNo)
        {
            _Person = clsPerson.Find(NationalNo);
            if (_Person == null)
            {
                ResetPersonInfo();

                MessageBox.Show(
                    _Translate(
                        "لم يتم العثور على شخص بالرقم الوطني: ",
                        "No Person with National No. = ") + NationalNo,
                    _Translate("خطأ", "Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
            
             _FillPersonInfo();
        }

        private void _LoadPersonImage()
        {
            // Clear the previous image path.
            pbPersonImage.ImageLocation = null;

            if (_Person.Gendor == 0)
                pbPersonImage.Image = Resources.Male_512;
            else
                pbPersonImage.Image = Resources.Female_512;

            string imagePath = _Person.ImagePath;

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
                        "لم يتم العثور على صورة الشخص في المسار:\n",
                        "Could not find this image:\n") + imagePath,
                    _Translate("خطأ", "Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void _FillPersonInfo()
        {
            llEditPersonInfo.Enabled = true;
            _PersonID = _Person.PersonID;
            lblPersonID.Text=_Person.PersonID.ToString();
            lblNationalNo.Text = _Person.NationalNo;
            lblFullName.Text = _Person.FullName;

            lblGendor.Text =
                _Person.Gendor == 0
                    ? _Translate("ذكر", "Male")
                    : _Translate("أنثى", "Female");

            lblEmail.Text = _Person.Email;
            lblPhone.Text = _Person.Phone;
            lblDateOfBirth.Text = _Person.DateOfBirth.ToShortDateString();
            lblCountry.Text= clsCountry.Find( _Person.NationalityCountryID).CountryName ;
            lblAddress.Text= _Person.Address;
            _LoadPersonImage();

           


        }

        public void ResetPersonInfo()
        {
            _PersonID = -1;
            lblPersonID.Text = "[????]";
            lblNationalNo.Text = "[????]";
            lblFullName.Text = "[????]";
            pbGendor.Image = Resources.Man_32;
            lblGendor.Text = "[????]";
            lblEmail.Text = "[????]";
            lblPhone.Text = "[????]";
            lblDateOfBirth.Text = "[????]";
            lblCountry.Text = "[????]";
            lblAddress.Text = "[????]";
            pbPersonImage.Image = Resources.Male_512;
        
        }

        private void llEditPersonInfo_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            frmAddUpdatePerson frm = new frmAddUpdatePerson( _PersonID );
            frm.ShowDialog();

            //refresh
            LoadPersonInfo(_PersonID);
        }

        private void label22_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
