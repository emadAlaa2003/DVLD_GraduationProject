using DVLD.Classes;
using DVLD.People;
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
using static System.Net.Mime.MediaTypeNames;

namespace DVLD.Applications
{


    public partial class frmAddUpdateLocalDrivingLicesnseApplication: Form
    {

        public enum enMode { AddNew = 0, Update = 1 };

        private enMode _Mode;
        private int _LocalDrivingLicenseApplicationID = -1;
        private int _SelectedPersonID = -1;
        clsLocalDrivingLicenseApplication _LocalDrivingLicenseApplication;

        public frmAddUpdateLocalDrivingLicesnseApplication()
        {
            InitializeComponent();
            _Mode = enMode.AddNew;
        }

        public frmAddUpdateLocalDrivingLicesnseApplication(int LocalDrivingLicenseApplicationID)
        {
            InitializeComponent();

            _Mode = enMode.Update;
            _LocalDrivingLicenseApplicationID = LocalDrivingLicenseApplicationID;

        }

        private string _Translate(string arabic, string english)
        {
            bool isArabic =
                System.Globalization.CultureInfo.CurrentUICulture
                .TwoLetterISOLanguageName == "ar";

            return isArabic ? arabic : english;
        }


        private void UpdateTitleLanguage()
        {
            bool isArabic =
                System.Globalization.CultureInfo.CurrentUICulture
                .TwoLetterISOLanguageName == "ar";

            string title;

            if (_Mode == enMode.AddNew)
            {
                title = isArabic
                    ? "إضافة طلب رخصة قيادة محلية جديد"
                    : "New Local Driving License Application";
            }
            else
            {
                title = isArabic
                    ? "تعديل طلب رخصة قيادة محلية"
                    : "Update Local Driving License Application";
            }

            lblTitle.Text = title;
            this.Text = title;
        }


        private void _FillLicenseClassesInComoboBox()
        {
            DataTable dtLicenseClasses = clsLicenseClass.GetAllLicenseClasses();

            foreach (DataRow row in dtLicenseClasses.Rows)
            {
                cbLicenseClass.Items.Add(row["ClassName"]);
            }
        }

        private void _ResetDefualtValues()
        {
            //this will initialize the reset the defaule values
            _FillLicenseClassesInComoboBox();


            if (_Mode == enMode.AddNew)
            {
                
                
                _LocalDrivingLicenseApplication = new clsLocalDrivingLicenseApplication();
                ctrlPersonCardWithFilter1.FilterFocus();
                tpApplicationInfo.Enabled = false;
              
                cbLicenseClass.SelectedIndex = 2;
                lblFees.Text = clsApplicationType.Find((int)clsApplication.enApplicationType.NewDrivingLicense).Fees.ToString();  
                lblApplicationDate.Text= DateTime.Now.ToShortDateString();
                lblCreatedByUser.Text = clsGlobal.CurrentUser.UserName;
            }
            else
            {
                
           
                tpApplicationInfo.Enabled = true;
                btnSave.Enabled = true;
             

            }
            UpdateTitleLanguage();
        }

        private void _LoadData()
        {

            ctrlPersonCardWithFilter1.FilterEnabled = false;
            _LocalDrivingLicenseApplication = clsLocalDrivingLicenseApplication.FindByLocalDrivingAppLicenseID(_LocalDrivingLicenseApplicationID);

            if (_LocalDrivingLicenseApplication == null)
            {

                MessageBox.Show(
                    _Translate(
                        "لم يتم العثور على طلب رخصة القيادة رقم: ",
                        "No application found with ID = ")
                        + _LocalDrivingLicenseApplicationID,
                    _Translate("الطلب غير موجود", "Application Not Found"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                this.Close();

                return;
            }

            ctrlPersonCardWithFilter1.LoadPersonInfo(_LocalDrivingLicenseApplication.ApplicantPersonID);
            lblLocalDrivingLicebseApplicationID.Text = _LocalDrivingLicenseApplication.LocalDrivingLicenseApplicationID.ToString();
            lblApplicationDate.Text = clsFormat.DateToShort( _LocalDrivingLicenseApplication.ApplicationDate);
            cbLicenseClass.SelectedIndex = cbLicenseClass.FindString(clsLicenseClass.Find(_LocalDrivingLicenseApplication.LicenseClassID).ClassName);
            lblFees.Text= _LocalDrivingLicenseApplication.PaidFees.ToString();
            lblCreatedByUser.Text = clsUser.FindByUserID(_LocalDrivingLicenseApplication.CreatedByUserID).UserName;

        }

        private void DataBackEvent(object sender, int PersonID)
        {
            // Handle the data received
            _SelectedPersonID=PersonID;
            ctrlPersonCardWithFilter1.LoadPersonInfo(PersonID);
           

        }

        private void frmAddUpdateLocalDrivingLicesnseApplication_Load(object sender, EventArgs e)
        {
            _ResetDefualtValues();

            if (_Mode==enMode.Update)
            {
                _LoadData();
            }
           
        }

        private void btnApplicationInfoNext_Click(object sender, EventArgs e)
        {

          

            if (_Mode == enMode.Update)
            {
                btnSave.Enabled = true;
                tpApplicationInfo.Enabled = true;
                tcApplicationInfo.SelectedTab = tcApplicationInfo.TabPages["tpApplicationInfo"];
                return;
            }


            //incase of add new mode.
            if (ctrlPersonCardWithFilter1.PersonID != -1)
            {
        
                    btnSave.Enabled = true;
                    tpApplicationInfo.Enabled = true;
                    tcApplicationInfo.SelectedTab = tcApplicationInfo.TabPages["tpApplicationInfo"];
   
            }

            else

            {

                MessageBox.Show(
                    _Translate(
                        "يرجى اختيار شخص أولًا قبل المتابعة.",
                        "Please select a person before continuing."),
                    _Translate("اختيار شخص", "Select a Person"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                ctrlPersonCardWithFilter1.FilterFocus();
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {


            if (!this.ValidateChildren())
            {
                //Here we dont continue becuase the form is not valid

                MessageBox.Show(
                    _Translate(
                        "بعض البيانات المدخلة غير صحيحة. مرّر مؤشر الفأرة فوق علامات الخطأ الحمراء لمعرفة السبب.",
                        "Some fields are invalid. Hover over the red error icons to see the details."),
                    _Translate("خطأ في التحقق من البيانات", "Validation Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;

            }

            int LicenseClassID = clsLicenseClass.Find(cbLicenseClass.Text).LicenseClassID;


            int ActiveApplicationID = clsApplication.GetActiveApplicationIDForLicenseClass(_SelectedPersonID, clsApplication.enApplicationType.NewDrivingLicense, LicenseClassID);

            if (ActiveApplicationID != -1)
            {

                MessageBox.Show(
                    _Translate(
                        "هذا الشخص لديه طلب رخصة قيادة فعال لنفس الفئة.\n\nرقم الطلب: ",
                        "This person already has an active application for the selected license class.\n\nApplication ID: ")
                        + ActiveApplicationID,
                    _Translate("طلب موجود مسبقًا", "Existing Application"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                cbLicenseClass.Focus();
                return;
            }


            //check if user already have issued license of the same driving  class.
           if( clsLicense.IsLicenseExistByPersonID(ctrlPersonCardWithFilter1.PersonID, LicenseClassID))
            {


                MessageBox.Show(
                    _Translate(
                        "هذا الشخص يمتلك بالفعل رخصة قيادة من الفئة المختارة. يرجى اختيار فئة أخرى.",
                        "This person already has a driving license for the selected class. Please choose another class."),
                    _Translate("غير مسموح", "Not Allowed"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            _LocalDrivingLicenseApplication.ApplicantPersonID = ctrlPersonCardWithFilter1.PersonID; ;
            _LocalDrivingLicenseApplication.ApplicationDate = DateTime.Now;
            _LocalDrivingLicenseApplication.ApplicationTypeID = 1;
            _LocalDrivingLicenseApplication.ApplicationStatus = clsApplication.enApplicationStatus.New;
            _LocalDrivingLicenseApplication.LastStatusDate = DateTime.Now;
            _LocalDrivingLicenseApplication.PaidFees = Convert.ToSingle(lblFees.Text);
            _LocalDrivingLicenseApplication.CreatedByUserID = clsGlobal.CurrentUser.UserID;
            _LocalDrivingLicenseApplication.LicenseClassID= LicenseClassID;


            if (_LocalDrivingLicenseApplication.Save())
            {
                lblLocalDrivingLicebseApplicationID.Text = _LocalDrivingLicenseApplication.LocalDrivingLicenseApplicationID.ToString();
                //change form mode to update.
                _Mode = enMode.Update;
                UpdateTitleLanguage();


                MessageBox.Show(
                    _Translate(
                        "تم حفظ طلب رخصة القيادة بنجاح.",
                        "Driving license application saved successfully."),
                    _Translate("تم الحفظ", "Saved"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

            }
            else

                MessageBox.Show(
                    _Translate(
                        "تعذّر حفظ طلب رخصة القيادة.",
                        "Could not save the driving license application."),
                    _Translate("خطأ", "Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);



        }

        private void ctrlPersonCardWithFilter1_OnPersonSelected(int obj)
        {
            _SelectedPersonID = obj;

        }

        private void frmAddUpdateLocalDrivingLicesnseApplication_Activated(object sender, EventArgs e)
        {
            ctrlPersonCardWithFilter1.FilterFocus();
        }
    }
}
