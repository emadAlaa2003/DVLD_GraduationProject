 using DVLD.Applications;
using DVLD.Applications.Detain_License;
using DVLD.Applications.International_License;
using DVLD.Applications.ReplaceLostOrDamagedLicense;
using DVLD.Applications.Rlease_Detained_License;
using DVLD.Classes;
using DVLD.Documents;
using DVLD.Drivers;
using DVLD.Licenses;
using DVLD.Licenses.International_License;
using DVLD.Login;
using DVLD.People;
using DVLD.QuestionBank;
using DVLD.Tests;
using DVLD.User;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Net.Http;
using System.Text.Json;

namespace DVLD
{

    public partial class frmMain : Form
    {
        frmLogin _frmLogin;
        private int _QuestionGenerationJobID = -1;
        private Timer _QuestionGenerationTimer;
        private int _CompletedQuestionGenerationDocumentID = -1;
        private int _CompletedGeneratedQuestionsCount = 0;
        private class QuestionGenerationJobResponse
        {
            public int QuestionGenerationJobID { get; set; }

            public int DocumentID { get; set; }

            public int MultipleChoiceCount { get; set; }

            public int TrueFalseCount { get; set; }

            public int TotalQuestions { get; set; }

            public string Status { get; set; }

            public int? GeneratedQuestionsCount { get; set; }

            public string ErrorMessage { get; set; }
        }
        public void TrackQuestionGenerationJob(
           int questionGenerationJobID)
        {
            _QuestionGenerationJobID =
                questionGenerationJobID;

            _QuestionGenerationTimer.Start();
        }
        private async void _QuestionGenerationTimer_Tick(
            object sender,
            EventArgs e)
        {
            if (_QuestionGenerationJobID <= 0)
                return;

            _QuestionGenerationTimer.Stop();

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout =
                        TimeSpan.FromSeconds(30);

                    HttpResponseMessage response =
                        await client.GetAsync(
                            "https://localhost:7077/api/question-bank/generation-jobs/" +
                            _QuestionGenerationJobID);

                    if (!response.IsSuccessStatusCode)
                    {
                        _QuestionGenerationTimer.Start();
                        return;
                    }

                    string responseText =
                        await response.Content.ReadAsStringAsync();

                    JsonSerializerOptions options =
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };

                    QuestionGenerationJobResponse job =
                        JsonSerializer.Deserialize<QuestionGenerationJobResponse>(
                            responseText,
                            options);

                    if (job == null)
                    {
                        _QuestionGenerationTimer.Start();
                        return;
                    }

                    if (job.Status == "Pending" ||
                        job.Status == "Processing")
                    {
                        _QuestionGenerationTimer.Start();
                        return;
                    }
                    _CompletedQuestionGenerationDocumentID =
    job.DocumentID;
                    _CompletedGeneratedQuestionsCount =
    job.GeneratedQuestionsCount ?? 0;

                    if (job.Status == "Completed")
                    {
                        MessageBox.Show(
                            "Questions generated successfully.\n\n" +
                            "Generated Questions: " +
                            (job.GeneratedQuestionsCount ?? 0) +
                            "\n\nClick OK to view the generated questions.",
                            "Question Generation Completed",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

                        frmManageQuestions frm =
                            new frmManageQuestions(
                                this,
                                _CompletedQuestionGenerationDocumentID,
                                _CompletedGeneratedQuestionsCount);

                        frm.ShowDialog();
                    }
                    _QuestionGenerationJobID = -1;
                }
            }
            catch
            {
                _QuestionGenerationTimer.Start();
            }
        }
        public frmMain(frmLogin frm)
        {
            InitializeComponent();

            _frmLogin = frm;

            _QuestionGenerationTimer = new Timer();
            _QuestionGenerationTimer.Interval = 5000;
            _QuestionGenerationTimer.Tick += _QuestionGenerationTimer_Tick;
        }

        private void localLicenseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmAddUpdateLocalDrivingLicesnseApplication frm = new frmAddUpdateLocalDrivingLicesnseApplication();
            frm.ShowDialog();
        }

        private void peopleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form frm = new frmListPeople();
            frm.ShowDialog();
        }

        private void employeesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form frm = new frmListUsers();
            frm.ShowDialog();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            this.BackColor = Color.White;
            lblLoggedInUser.Text = "LoggedIn User: " + clsGlobal.CurrentUser.UserName;
            this.Refresh();

        }

        private void currentUserInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmUserInfo frm = new frmUserInfo(clsGlobal.CurrentUser.UserID);
            frm.ShowDialog();

        }

        private void signOutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            clsGlobal.CurrentUser = null;
            _frmLogin.Show();
            this.Close();
        }

        private void changePasswordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmChangePassword frm = new frmChangePassword(clsGlobal.CurrentUser.UserID);
            frm.ShowDialog();

        }

        private void manageApplicationTypesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmManageApplicationTypes frm = new frmManageApplicationTypes();
            frm.ShowDialog();
        }

        private void manageTestTypesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmListTestTypes frm = new frmListTestTypes();
            frm.ShowDialog();
        }

        private void internationalLicenseToolStripMenuItem_Click(object sender, EventArgs e)
        {

            frmNewInternationalLicenseApplication frm = new frmNewInternationalLicenseApplication();
            frm.ShowDialog();

        }

        private void renewDrivingLicenseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmRenewLocalDrivingLicenseApplication frm = new frmRenewLocalDrivingLicenseApplication();
            frm.ShowDialog();

        }

       

        private void releaseDetainedDrivingLicenseToolStripMenuItem_Click(object sender, EventArgs e)
        {

            frmReleaseDetainedLicenseApplication frm = new frmReleaseDetainedLicenseApplication();
            frm.ShowDialog();
        }

        private void retakeTestToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            frmListLocalDrivingLicesnseApplications frm = new frmListLocalDrivingLicesnseApplications();
            frm.ShowDialog();
        }

      
        private void vehiclesLicensesServicesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("This Feature Is Not Implemented Yet!", "Not Ready", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void manageLocalDrivingLicenseApplicationsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmListLocalDrivingLicesnseApplications frm = new frmListLocalDrivingLicesnseApplications();
            frm.ShowDialog();

        }

        private void driversToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmListDrivers frm = new frmListDrivers();
            frm.ShowDialog();

        }

      

        private void ManageInternationaDrivingLicenseToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            frmListInternationalLicesnseApplications frm = new frmListInternationalLicesnseApplications();
            frm.ShowDialog();

        }

        private void ReplacementLostOrDamagedDrivingLicenseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmReplaceLostOrDamagedLicenseApplication frm = new frmReplaceLostOrDamagedLicenseApplication();
            frm.ShowDialog();

        }

        private void ManageDetainedLicensestoolStripMenuItem1_Click(object sender, EventArgs e)
        {
            frmListDetainedLicenses frm = new frmListDetainedLicenses();
            frm.ShowDialog();

        }

        private void detainLicenseToolStripMenuItem_Click(object sender, EventArgs e)
        {
           frmDetainLicenseApplication frm = new frmDetainLicenseApplication();
            frm.ShowDialog();

        }

        private void releaseDetainedLicenseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmReleaseDetainedLicenseApplication frm= new frmReleaseDetainedLicenseApplication();   
            frm.ShowDialog();

        }

        private void manageQuestionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmManageQuestions frm = new frmManageQuestions(this);
            frm.ShowDialog();
        }

        private void mangeDecumentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmManageDocuments frm=new frmManageDocuments();
            frm.ShowDialog();
        }
    }
}
