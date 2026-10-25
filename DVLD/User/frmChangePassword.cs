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

namespace DVLD.User
{
    public partial class frmChangePassword : Form
    {
        private int _UserID;
        private clsUser _User;

        public frmChangePassword(int UserID )
        {
            InitializeComponent();

            _UserID=UserID;
        }


        private string _Translate(string arabic, string english)
        {
            bool isArabic =
                System.Globalization.CultureInfo.CurrentUICulture
                .TwoLetterISOLanguageName == "ar";

            return isArabic ? arabic : english;
        }

        private void _ResetDefualtValues()
        {
            txtCurrentPassword.Text = "";
            txtNewPassword.Text = "";
            txtConfirmPassword.Text = "";
            txtCurrentPassword.Focus(); 
        }

        private void frmChangePassword_Load(object sender, EventArgs e)
        {
             _ResetDefualtValues();

              _User = clsUser.FindByUserID(_UserID);

            if (_User == null)
            {
                //Here we dont continue becuase the form is not valid

                MessageBox.Show(
                    _Translate(
                        "لم يتم العثور على المستخدم رقم: ",
                        "Could not find User with ID = ") + _UserID,
                    _Translate("خطأ", "Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                this.Close();

                return;

            }
            ctrlUserCard1.LoadUserInfo(_UserID);

        }

        private void txtCurrentPassword_Validating(object sender, CancelEventArgs e)
        {

            if (string.IsNullOrEmpty(txtCurrentPassword.Text.Trim()))
            {
                e.Cancel = true;

                errorProvider1.SetError(
                    txtCurrentPassword,
                    _Translate(
                        "يجب إدخال كلمة المرور الحالية.",
                        "Current password cannot be blank."));

                return;
            }
            else
            {
                errorProvider1.SetError(txtCurrentPassword, null);
            };

            if (_User.Password != txtCurrentPassword.Text.Trim())
            {
                e.Cancel = true;

                errorProvider1.SetError(
                    txtCurrentPassword,
                    _Translate(
                        "كلمة المرور الحالية غير صحيحة.",
                        "Current password is incorrect!"));

                return;
            }
            else
            {
                errorProvider1.SetError(txtCurrentPassword, null);
            };
        }

        private void txtNewPassword_Validating(object sender, CancelEventArgs e)
        {
            if (string.IsNullOrEmpty(txtNewPassword.Text.Trim()))
            {
                e.Cancel = true;

                errorProvider1.SetError(
                    txtNewPassword,
                    _Translate(
                        "يجب إدخال كلمة المرور الجديدة.",
                        "New password cannot be blank."));

            }
            else
            {
                errorProvider1.SetError(txtNewPassword, null);
            };
        }

        private void txtConfirmPassword_Validating(object sender, CancelEventArgs e)
        {
            if (txtConfirmPassword.Text.Trim() != txtNewPassword.Text.Trim())
            {
                e.Cancel = true;

                errorProvider1.SetError(
                    txtConfirmPassword,
                    _Translate(
                        "تأكيد كلمة المرور لا يطابق كلمة المرور الجديدة.",
                        "Password confirmation does not match the new password!"));
            }
            else
            {
                errorProvider1.SetError(txtConfirmPassword, null);
            };
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

            _User.Password = txtNewPassword.Text;

            if (_User.Save())
            {

                MessageBox.Show(
                    _Translate(
                        "تم تغيير كلمة المرور بنجاح.",
                        "Password changed successfully."),
                    _Translate("تم الحفظ", "Saved"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                _ResetDefualtValues();
            }
            else
            {

                MessageBox.Show(
                    _Translate(
                        "حدث خطأ أثناء تغيير كلمة المرور. لم يتم حفظ التغيير.",
                        "An error occurred. Password was not changed."),
                    _Translate("خطأ", "Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();

        }
    }
}
