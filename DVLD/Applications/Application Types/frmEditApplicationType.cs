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

namespace DVLD.Applications
{
    public partial class frmEditApplicationType : Form
    {

        private int _ApplicationTypeID=-1;
        private clsApplicationType _ApplicationType;

        public frmEditApplicationType(int ApplicationTypeID )
        {
            InitializeComponent();

            _ApplicationTypeID = ApplicationTypeID; 
        }


        private string _Translate(string arabic, string english)
        {
            bool isArabic =
                System.Globalization.CultureInfo.CurrentUICulture
                .TwoLetterISOLanguageName == "ar";

            return isArabic ? arabic : english;
        }


        private void frmEditApplicationType_Load(object sender, EventArgs e)
        {
            lblApplicationTypeID.Text=_ApplicationTypeID.ToString();

            _ApplicationType = clsApplicationType.Find(_ApplicationTypeID);

            if (_ApplicationType!=null)
            {
                txtTitle.Text = _ApplicationType.Title;
                txtFees.Text = _ApplicationType.Fees.ToString();


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

            _ApplicationType.Title= txtTitle.Text.Trim();
            _ApplicationType.Fees = Convert.ToSingle( txtFees.Text.Trim());


            if (_ApplicationType.Save())
            {

                MessageBox.Show(
                    _Translate(
                        "تم حفظ تعديلات نوع الطلب بنجاح.",
                        "Data saved successfully."),
                    _Translate("تم الحفظ", "Saved"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else

                MessageBox.Show(
                    _Translate(
                        "حدث خطأ، لم يتم حفظ تعديلات نوع الطلب.",
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
                        "يجب إدخال اسم نوع الطلب.",
                        "Title cannot be empty!"));
            }
            else
            {
                errorProvider1.SetError(txtTitle, null);
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
