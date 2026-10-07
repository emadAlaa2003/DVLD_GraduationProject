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
                        "Person was not found.",
                        "Error",
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
                    "Could not load mobile account information.\n\n" +
                    ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void _RefreshAccountState()
        {
            // No mobile account exists yet.
            if (_MobileUser == null)
            {
                lblUsernameValue.Text = "Not Created";
                lblStatusValue.Text = "No Account";

                lblTemporaryPasswordValue.Text =
                    "Will appear after account creation";

                btnCreateAccount.Enabled = true;
                btnResetPassword.Enabled = false;
                btnToggleActive.Enabled = false;

                btnToggleActive.Text = "Deactivate";

                pbMobileAccount.Image =
                    Properties.Resources.MobileAccount_NoAccount;

                return;
            }

            lblUsernameValue.Text =
                _MobileUser.Username;

            if (string.IsNullOrEmpty(_TemporaryPassword))
            {
                lblTemporaryPasswordValue.Text =
                    "Shown only after create / reset";
            }
            else
            {
                lblTemporaryPasswordValue.Text =
                    _TemporaryPassword;
            }

            btnCreateAccount.Enabled = false;
            btnResetPassword.Enabled = true;
            btnToggleActive.Enabled = true;

            if (_MobileUser.IsActive)
            {
                lblStatusValue.Text = "Active";

                btnToggleActive.Text =
                    "Deactivate";

                pbMobileAccount.Image =
                    Properties.Resources.MobileAccount_Active;
            }
            else
            {
                lblStatusValue.Text = "Inactive";

                btnToggleActive.Text =
                    "Activate";

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
                        "This person already has a mobile account.",
                        "Mobile Account",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    return;
                }

                if (_Person == null)
                {
                    MessageBox.Show(
                        "Person information is not available.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }

                if (string.IsNullOrWhiteSpace(_Person.NationalNo))
                {
                    MessageBox.Show(
                        "This person does not have a valid National Number.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }

                if (MessageBox.Show(
                    "Create a mobile account for this person?\n\n" +
                    "The National Number will be used as the username.",
                    "Confirm",
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
                        "Could not create the mobile account.\n\n" +
                        "The National Number may already be used by another mobile account.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }

                // Plain password exists only while this form is open
                _TemporaryPassword = Password;

                _RefreshAccountState();

                MessageBox.Show(
                    "Mobile account created successfully.\n\n" +
                    "Username: " + _MobileUser.Username +
                    "\nTemporary Password: " + Password +
                    "\n\nGive these credentials to the citizen.",
                    "Account Created",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not create the mobile account.\n\n" +
                    ex.Message,
                    "Error",
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
                    "Generate a new temporary password?",
                    "Reset Password",
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
                        "Password could not be reset.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }

                _TemporaryPassword = Password;

                _RefreshAccountState();

                MessageBox.Show(
                    "Password reset successfully.\n\n" +
                    "Username: " + _MobileUser.Username +
                    "\nNew Temporary Password: " + Password +
                    "\n\nGive the new password to the citizen.",
                    "Password Reset",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not reset the password.\n\n" +
                    ex.Message,
                    "Error",
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

                bool NewStatus =
                    !_MobileUser.IsActive;

                string ActionText =
                    NewStatus ? "activate" : "deactivate";

                if (MessageBox.Show(
                    "Are you sure you want to " +
                    ActionText +
                    " this mobile account?",
                    "Confirm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }

                if (!_MobileUser.SetIsActive(NewStatus))
                {
                    MessageBox.Show(
                        "Account status could not be updated.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }

                _RefreshAccountState();

                MessageBox.Show(
                    NewStatus
                        ? "Mobile account activated successfully."
                        : "Mobile account deactivated successfully.",
                    "Mobile Account",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not update account status.\n\n" +
                    ex.Message,
                    "Error",
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
