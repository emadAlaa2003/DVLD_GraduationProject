using DVLD.Classes;
using DVLD_Buisness;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DVLD.Tests
{
    public partial class frmTakeTest: Form
    {
        private int _AppointmentID;
        private clsTestType.enTestType _TestType;

        private int _TestID = -1;
        private clsTest _Test;


        public frmTakeTest(int AppointmentID,clsTestType.enTestType TestType )
        {
            InitializeComponent();
            _AppointmentID= AppointmentID;
            _TestType = TestType;

        }


        private string _Translate(string arabic, string english)
        {
            bool isArabic =
                System.Globalization.CultureInfo.CurrentUICulture
                .TwoLetterISOLanguageName == "ar";

            return isArabic ? arabic : english;
        }


        private void frmTakeTest_Load(object sender, EventArgs e)
        {
            ctrlSecheduledTest1.TestTypeID = _TestType;

            ctrlSecheduledTest1.LoadInfo(_AppointmentID);
            
            if (ctrlSecheduledTest1.TestAppointmentID==-1)
                btnSave.Enabled = false;
            else
                btnSave.Enabled = true;


            int _TestID = ctrlSecheduledTest1.TestID;
            if (_TestID != -1)
            {
                _Test = clsTest.Find(_TestID);

                if (_Test.TestResult)
                    rbPass.Checked = true;
                else
                    rbFail.Checked = true;
                   txtNotes.Text = _Test.Notes;


                lblUserMessage.Text = _Translate(
                    "تم تسجيل نتيجة هذا الاختبار مسبقًا، ولا يمكن تغييرها.",
                    "This test result has already been recorded and cannot be changed.");

                lblUserMessage.Visible = true;
                rbFail.Enabled = false;
                rbPass.Enabled = false;

            }

            else
                _Test = new clsTest();

        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {

            if (MessageBox.Show(
                    _Translate(
                        "هل أنت متأكد من حفظ نتيجة الاختبار؟ بعد الحفظ لن تتمكن من تغيير نتيجة النجاح أو الرسوب.",
                        "Are you sure you want to save? You cannot change the Pass/Fail result after saving."),
                    _Translate("تأكيد الحفظ", "Confirm"),
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning) == DialogResult.No)
            {
                return;
            }


            _Test.TestAppointmentID = _AppointmentID;
            _Test.TestResult = rbPass.Checked;
            _Test.Notes = txtNotes.Text.Trim();
            _Test.CreatedByUserID=clsGlobal.CurrentUser.UserID;

            if (_Test.Save())
            {

                MessageBox.Show(
                    _Translate(
                        "تم حفظ نتيجة الاختبار بنجاح.",
                        "Data Saved Successfully."),
                    _Translate("تم الحفظ", "Saved"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                btnSave.Enabled = false;

            }
            else

                MessageBox.Show(
                    _Translate(
                        "حدث خطأ، لم يتم حفظ نتيجة الاختبار.",
                        "Error: Data was not saved successfully."),
                    _Translate("خطأ", "Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

        }
    }
}
