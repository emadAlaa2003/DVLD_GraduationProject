using DVLD.Classes;
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

namespace DVLD.Tests
{


    public partial class frmEditTestType: Form
    {

        private clsTestType.enTestType _TestTypeID = clsTestType.enTestType.VisionTest;
        private clsTestType _TestType;

        public frmEditTestType(clsTestType.enTestType TestTypeID)
        {
            InitializeComponent();
            _TestTypeID = TestTypeID;

        }


        private string _Translate(string arabic, string english)
        {
            bool isArabic =
                System.Globalization.CultureInfo.CurrentUICulture
                .TwoLetterISOLanguageName == "ar";

            return isArabic ? arabic : english;
        }

        private void frmEditTestType_Load(object sender, EventArgs e)
        {
          
            _TestType = clsTestType.Find( _TestTypeID);

            if (_TestType != null)
            {
              

                lblTestTypeID.Text = ((int) _TestTypeID).ToString();
                txtTitle.Text = _TestType.Title;
                txtDescription.Text = _TestType.Description;
                txtFees.Text = _TestType.Fees.ToString();
            }
            
            else

            {

                MessageBox.Show(
                    _Translate(
                        "لم يتم العثور على نوع الاختبار، المعرف: ",
                        "Could not find Test Type with ID = ")
                        + _TestTypeID.ToString(),
                    _Translate("خطأ", "Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                this.Close();

            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
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

            _TestType.Title = txtTitle.Text.Trim();
            _TestType.Description = txtDescription.Text.Trim();
            _TestType.Fees = Convert.ToSingle(txtFees.Text.Trim());


            if (_TestType.Save())
            {

                MessageBox.Show(
                    _Translate(
                        "تم حفظ تعديلات نوع الاختبار بنجاح.",
                        "Data saved successfully."),
                    _Translate("تم الحفظ", "Saved"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else

                MessageBox.Show(
                    _Translate(
                        "حدث خطأ، لم يتم حفظ تعديلات نوع الاختبار.",
                        "Error: Data was not saved successfully."),
                    _Translate("خطأ", "Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

        }

        private void txtTitle_Validating(object sender, CancelEventArgs e)
        {
            if (string.IsNullOrEmpty(txtTitle.Text.Trim()))
            {
                e.Cancel = true;

                errorProvider1.SetError(
                    txtTitle,
                    _Translate(
                        "يجب إدخال اسم نوع الاختبار.",
                        "Title cannot be empty!"));
            }
            else
            {
                errorProvider1.SetError(txtTitle, null);
            };
        }

        private void txtDescription_Validating(object sender, CancelEventArgs e)
        {
            if (string.IsNullOrEmpty(txtDescription.Text.Trim()))
            {
                e.Cancel = true;

                errorProvider1.SetError(
                    txtDescription,
                    _Translate(
                        "يجب إدخال وصف الاختبار.",
                        "Description cannot be empty!"));
            }
            else
            {
                errorProvider1.SetError(txtDescription, null);
            };
        }

        private void txtFees_Validating(object sender, CancelEventArgs e)
        {
            if (string.IsNullOrEmpty(txtFees.Text.Trim()))
            {
                e.Cancel = true;

                errorProvider1.SetError(
                    txtFees,
                    _Translate(
                        "يجب إدخال قيمة الرسوم.",
                        "Fees cannot be empty!"));
                return;
            }
            else
            {
                errorProvider1.SetError(txtFees, null);

            };

           
            if (!clsValidatoin.IsNumber(txtFees.Text))
            {
                e.Cancel = true;

                errorProvider1.SetError(
                    txtFees,
                    _Translate(
                        "يرجى إدخال قيمة رقمية صحيحة للرسوم.",
                        "Invalid Number."));
            }
            else
            {
                errorProvider1.SetError(txtFees, null);
            };
        }
    }
}
