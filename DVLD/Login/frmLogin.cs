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

namespace DVLD.Login
{
    public partial class frmLogin : Form
    {
        public frmLogin()
        {
            InitializeComponent();
            InitializeLanguageSelector();
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


        private void btnLogin_Click(object sender, EventArgs e)
        {
            clsUser user = clsUser.FindByUsernameAndPassword(
                txtUserName.Text.Trim(),
                txtPassword.Text.Trim());

            if (user == null)
            {
                txtUserName.Focus();

                MessageBox.Show(
                    _Translate(
                        "اسم المستخدم أو كلمة المرور غير صحيحة.",
                        "Invalid username or password."),
                    _Translate(
                        "بيانات دخول غير صحيحة",
                        "Invalid Credentials"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            if (!user.IsActive)
            {
                txtUserName.Focus();

                MessageBox.Show(
                    _Translate(
                        "هذا الحساب غير نشط. يرجى التواصل مع مسؤول النظام.",
                        "This account is inactive. Please contact the administrator."),
                    _Translate(
                        "حساب غير نشط",
                        "Inactive Account"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            // Remember credentials only after successful validation.
            if (chkRememberMe.Checked)
            {
                clsGlobal.RememberUsernameAndPassword(
                    txtUserName.Text.Trim(),
                    txtPassword.Text.Trim());
            }
            else
            {
                clsGlobal.RememberUsernameAndPassword("", "");
            }

            clsGlobal.CurrentUser = user;

            this.Hide();

            using (frmMain frm = new frmMain(this))
            {
                frm.ShowDialog();
            }

            // Close the login form if the main form closes
            // and the login form remains hidden.
            if (!this.IsDisposed && !this.Visible)
            {
                this.Close();
            }
        }



        private void frmLogin_Load(object sender, EventArgs e)
        {
            string UserName = "";
            string Password = "";

            if (clsGlobal.GetStoredCredential(ref UserName, ref Password))
            {
                txtUserName.Text = UserName;
                txtPassword.Text = Password;
                chkRememberMe.Checked = true;
            }
            else
            {
                chkRememberMe.Checked = false;
            }
        }


        private void InitializeLanguageSelector()
        {
            cmbLanguage.Items.Clear();

            cmbLanguage.Items.Add("English");
            cmbLanguage.Items.Add("العربية (فلسطين)");

            string language =
                Properties.Settings.Default.UiLanguage;

            // Select the saved language before subscribing to the event.
            cmbLanguage.SelectedIndex =
                language == "ar-PS" ? 1 : 0;

            cmbLanguage.SelectedIndexChanged +=
                cmbLanguage_SelectedIndexChanged;
        }

        private void cmbLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbLanguage.SelectedIndex < 0)
                return;

            string selectedLanguage =
                cmbLanguage.SelectedIndex == 1 ? "ar-PS" : "en";

            if (selectedLanguage ==
                Properties.Settings.Default.UiLanguage)
                return;

            Properties.Settings.Default.UiLanguage =
                selectedLanguage;

            Properties.Settings.Default.Save();

            MessageBox.Show(
                "The application will restart to apply the language.\n" +
                "سيُعاد تشغيل البرنامج لتطبيق اللغة.",
                "Language / اللغة",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );

            Application.Restart();
        }
    }
}
