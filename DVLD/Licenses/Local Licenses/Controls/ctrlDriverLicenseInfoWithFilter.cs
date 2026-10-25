using DVLD.Controls;
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

namespace DVLD.Licenses.Controls
{
    public partial class ctrlDriverLicenseInfoWithFilter : UserControl
    {

        // Define a custom event handler delegate with parameters
        public event Action<int> OnLicenseSelected;
        // Create a protected method to raise the event with a parameter
        protected virtual void PersonSelected(int LicenseID)
        {
            Action<int> handler = OnLicenseSelected;
            if (handler != null)
            {
                handler(LicenseID); // Raise the event with the parameter
            }
        }

        public ctrlDriverLicenseInfoWithFilter()
        {
            InitializeComponent();
        }

        private bool _FilterEnabled = true;
      
        public bool FilterEnabled
        {
            get
            {
                return _FilterEnabled;
            }
            set
            {
                _FilterEnabled = value;
                gbFilters.Enabled = _FilterEnabled;
            }
        }

        private int _LicenseID = -1;

        public int LicenseID
        {
            get { return ctrlDriverLicenseInfo1.LicenseID; }
        }

        public clsLicense SelectedLicenseInfo
        { get { return ctrlDriverLicenseInfo1.SelectedLicenseInfo; } }



        private string _Translate(string arabic, string english)
        {
            bool isArabic =
                System.Globalization.CultureInfo.CurrentUICulture
                .TwoLetterISOLanguageName == "ar";

            return isArabic ? arabic : english;
        }


        public void LoadLicenseInfo(int LicenseID)
        {


            txtLicenseID.Text = LicenseID.ToString();
            ctrlDriverLicenseInfo1.LoadInfo(LicenseID);
            _LicenseID= ctrlDriverLicenseInfo1.LicenseID;
            if (OnLicenseSelected != null && FilterEnabled)
                // Raise the event with a parameter
                OnLicenseSelected(_LicenseID);


        }

        private void txtLicenseID_KeyPress(object sender, KeyPressEventArgs e)
        {
         
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);

          
            // Check if the pressed key is Enter (character code 13)
            if (e.KeyChar == (char)13)
            {
              
                btnFind.PerformClick();
            }

        }

        private void btnFind_Click(object sender, EventArgs e)
        {
            if (!this.ValidateChildren())
            {
                //Here we dont continue becuase the form is not valid

                MessageBox.Show(
                    _Translate(
                        "بعض البيانات المدخلة غير صحيحة. مرّر مؤشر الفأرة فوق علامة الخطأ الحمراء لمعرفة السبب.",
                        "Some fields are invalid. Hover over the red error icons to see the details."),
                    _Translate("خطأ في التحقق من البيانات", "Validation Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                txtLicenseID.Focus();
                return;

            }
            _LicenseID= int.Parse(txtLicenseID.Text);
            LoadLicenseInfo(_LicenseID);
        }

        public void txtLicenseIDFocus()
        {
            txtLicenseID.Focus();
        }

        private void txtLicenseID_Validating(object sender, CancelEventArgs e)
        {
            if (string.IsNullOrEmpty(txtLicenseID.Text.Trim()))
            {
                e.Cancel = true;

                errorProvider1.SetError(
                    txtLicenseID,
                    _Translate(
                        "رقم الرخصة مطلوب.",
                        "This field is required!"));
            }
            else
            {
                //e.Cancel = false;
                errorProvider1.SetError(txtLicenseID, null);
            }
        }

     
    }
}
