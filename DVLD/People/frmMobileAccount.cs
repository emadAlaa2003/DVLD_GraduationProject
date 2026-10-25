using System;
using System.Windows.Forms;
using DVLD_Buisness;

namespace DVLD.People
{
    public partial class frmMobileAccount : Form
    {
        private int _PersonID = -1;

        private clsPerson _Person = null;
        private clsMobileUser _MobileUser = null;

        // We keep the plain temporary password only while
        // this form is open. It is never saved in the database.
        private string _TemporaryPassword = "";

        public frmMobileAccount()
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


        public frmMobileAccount(int PersonID)
            : this()
        {
            _PersonID = PersonID;
        }

        private void frmMobileAccount_Load(object sender, EventArgs e)
        {
             _LoadData();
        }

        private void _LoadData()
        {
            try
            {
                _Person = clsPerson.Find(_PersonID);

                if (_Person == null)
                {

                    MessageBox.Show(
                        _Translate(
                            "لم يتم العثور على الشخص.",
                            "Person was not found."),
                        _Translate("خطأ", "Error"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);


                    this.Close();
                    return;
                }

                lblPersonIDValue.Text =
                    _Person.PersonID.ToString();

                lblPersonNameValue.Text =
                    _Person.FullName;

                _MobileUser =
                    clsMobileUser.FindByPersonID(_PersonID);

                _RefreshAccountState();
            }
            catch (Exception ex)
            {

                MessageBox.Show(
                    _Translate(
                        "تعذّر تحميل معلومات حساب المواطن.\n\n",
                        "Could not load mobile account information.\n\n")
                        + ex.Message,
                    _Translate("خطأ", "Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

            }
        }


        private void _RefreshAccountState()
        {
            if (_MobileUser == null)
            {
                lblUsernameValue.Text =
                    _Translate("لم يتم إنشاء حساب", "Not Created");

                lblStatusValue.Text =
                    _Translate("لا يوجد حساب", "No Account");

                lblTemporaryPasswordValue.Text =
                    _Translate(
                        "ستظهر بعد إنشاء الحساب",
                        "Will appear after account creation");

                btnCreateAccount.Enabled = true;
                btnResetPassword.Enabled = false;
                btnToggleActive.Enabled = false;

                btnToggleActive.Text =
                    _Translate("تعطيل الحساب", "Deactivate");

                pbMobileAccount.Image =
                    Properties.Resources.MobileAccount_NoAccount;

                return;
            }

            lblUsernameValue.Text = _MobileUser.Username;

            lblTemporaryPasswordValue.Text =
                string.IsNullOrEmpty(_TemporaryPassword)
                    ? _Translate(
                        "تظهر فقط بعد إنشاء الحساب أو إعادة تعيين كلمة المرور",
                        "Shown only after create / reset")
                    : _TemporaryPassword;

            btnCreateAccount.Enabled = false;
            btnResetPassword.Enabled = true;
            btnToggleActive.Enabled = true;

            if (_MobileUser.IsActive)
            {
                lblStatusValue.Text =
                    _Translate("نشط", "Active");

                btnToggleActive.Text =
                    _Translate("تعطيل الحساب", "Deactivate");

                pbMobileAccount.Image =
                    Properties.Resources.MobileAccount_Active;
            }
            else
            {
                lblStatusValue.Text =
                    _Translate("غير نشط", "Inactive");

                btnToggleActive.Text =
                    _Translate("تفعيل الحساب", "Activate");

                pbMobileAccount.Image =
                    Properties.Resources.MobileAccount_Inactive;
            }
        }


        private void btnCreateAccount_Click(
            object sender,
            EventArgs e)
        {
            try
            {

                if (_MobileUser != null)
                {
                    MessageBox.Show(
                        _Translate(
                            "هذا الشخص لديه حساب موبايل بالفعل.",
                            "This person already has a mobile account."),
                        _Translate("حساب المواطن", "Mobile Account"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    return;
                }

                if (_Person == null)
                {
                    MessageBox.Show(
                        _Translate(
                            "معلومات الشخص غير متوفرة.",
                            "Person information is not available."),
                        _Translate("خطأ", "Error"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }

                if (string.IsNullOrWhiteSpace(_Person.NationalNo))
                {
                    MessageBox.Show(
                        _Translate(
                            "لا يوجد رقم وطني صالح لهذا الشخص.",
                            "This person does not have a valid National Number."),
                        _Translate("خطأ", "Error"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }



                if (MessageBox.Show(
                    _Translate(
                        "هل أنت متأكد من إنشاء حساب موبايل لهذا الشخص؟\n\n" +
                        "سيتم استخدام الرقم الوطني كاسم مستخدم.",
                        "Create a mobile account for this person?\n\n" +
                        "The National Number will be used as the username."),
                    _Translate("تأكيد إنشاء الحساب", "Confirm"),
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }


                // Username = National Number
                string Username =
                    _Person.NationalNo.Trim();

                // Generate temporary password
                string Password =
      _Person.FirstName.Trim() +
      _Person.NationalNo.Trim() +
      "!";

                // Only the hash will be stored in the database
                string PasswordHash =
                    clsMobileUser.HashPassword(Password);

                _MobileUser =
                    clsMobileUser.Create(
                        _PersonID,
                        Username,
                        PasswordHash,
                        true);

                if (_MobileUser == null)
                {

                    MessageBox.Show(
                        _Translate(
                            "تعذّر إنشاء حساب المواطن.\n\n" +
                            "قد يكون الرقم الوطني مستخدمًا في حساب آخر.",
                            "Could not create the mobile account.\n\n" +
                            "The National Number may already be used by another mobile account."),
                        _Translate("خطأ", "Error"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);


                    return;
                }

                // Plain password exists only while this form is open
                _TemporaryPassword = Password;

                _RefreshAccountState();


                MessageBox.Show(
                    _Translate(
                        "تم إنشاء حساب المواطن بنجاح.\n\nاسم المستخدم: ",
                        "Mobile account created successfully.\n\nUsername: ")
                    + _MobileUser.Username
                    + _Translate(
                        "\nكلمة المرور المؤقتة: ",
                        "\nTemporary Password: ")
                    + Password
                    + _Translate(
                        "\n\nيرجى تسليم بيانات الدخول للمواطن بطريقة آمنة.",
                        "\n\nGive these credentials to the citizen securely."),
                    _Translate("تم إنشاء الحساب", "Account Created"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

            }
            catch (Exception ex)
            {

                MessageBox.Show(
                    _Translate(
                        "تعذّر إنشاء حساب المواطن.\n\n",
                        "Could not create the mobile account.\n\n")
                        + ex.Message,
                    _Translate("خطأ", "Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

            }
        }
        private void btnResetPassword_Click(
            object sender,
            EventArgs e)
        {
            try
            {
                if (_MobileUser == null)
                    return;


                if (MessageBox.Show(
                    _Translate(
                        "هل تريد إنشاء كلمة مرور مؤقتة جديدة؟",
                        "Generate a new temporary password?"),
                    _Translate("إعادة تعيين كلمة المرور", "Reset Password"),
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }

           

                string Password =
    _Person.FirstName.Trim() +
    _Person.NationalNo.Trim() +
    "!";

                string PasswordHash =
                    clsMobileUser.HashPassword(Password);

                if (!_MobileUser.ResetPasswordHash(
                    PasswordHash))
                {

                    MessageBox.Show(
                        _Translate(
                            "تعذّرت إعادة تعيين كلمة المرور.",
                            "Password could not be reset."),
                        _Translate("خطأ", "Error"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);


                    return;
                }

                _TemporaryPassword = Password;

                _RefreshAccountState();

                MessageBox.Show(
                    _Translate(
                        "تمت إعادة تعيين كلمة المرور بنجاح.\n\nاسم المستخدم: ",
                        "Password reset successfully.\n\nUsername: ")
                    + _MobileUser.Username
                    + _Translate(
                        "\nكلمة المرور المؤقتة الجديدة: ",
                        "\nNew Temporary Password: ")
                    + Password
                    + _Translate(
                        "\n\nيرجى تسليم كلمة المرور الجديدة للمواطن بطريقة آمنة.",
                        "\n\nGive the new password to the citizen securely."),
                    _Translate("تم تغيير كلمة المرور", "Password Reset"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

            }
            catch (Exception ex)
            {

                MessageBox.Show(
                    _Translate(
                        "تعذّرت إعادة تعيين كلمة المرور.\n\n",
                        "Could not reset the password.\n\n")
                        + ex.Message,
                    _Translate("خطأ", "Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

            }
        }



        private void btnToggleActive_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (_MobileUser == null)
                    return;


                bool NewStatus = !_MobileUser.IsActive;

                string confirmationMessage = NewStatus
                    ? _Translate(
                        "هل أنت متأكد من تفعيل حساب المواطن؟",
                        "Are you sure you want to activate this mobile account?")
                    : _Translate(
                        "هل أنت متأكد من تعطيل حساب المواطن؟",
                        "Are you sure you want to deactivate this mobile account?");

                if (MessageBox.Show(
                    confirmationMessage,
                    _Translate("تأكيد العملية", "Confirm"),
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }


                if (!_MobileUser.SetIsActive(NewStatus))
                {

                    MessageBox.Show(
                        _Translate(
                            "تعذّر تحديث حالة حساب المواطن.",
                            "Account status could not be updated."),
                        _Translate("خطأ", "Error"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);


                    return;
                }

                _RefreshAccountState();


                MessageBox.Show(
                    NewStatus
                        ? _Translate(
                            "تم تفعيل حساب المواطن بنجاح.",
                            "Mobile account activated successfully.")
                        : _Translate(
                            "تم تعطيل حساب المواطن بنجاح.",
                            "Mobile account deactivated successfully."),
                    _Translate("حساب المواطن", "Mobile Account"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

            }
            catch (Exception ex)
            {

                MessageBox.Show(
                    _Translate(
                        "تعذّر تحديث حالة حساب المواطن.\n\n",
                        "Could not update account status.\n\n")
                        + ex.Message,
                    _Translate("خطأ", "Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

            }
        }

        private void btnClose_Click_1(object sender, EventArgs e)
        {
            this.Close();

        }
    }
}
