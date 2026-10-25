using DVLD.Applications;
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
using DVLD_Buisness;
using DVLD.DriverLicense;
using System.Security.Cryptography;
using DVLD.Drivers;
using DVLD.Licenses.International_License;

namespace DVLD.Tests
{
    public partial class frmListLocalDrivingLicesnseApplications : Form
    {
        private DataTable _dtAllLocalDrivingLicenseApplications;
        public frmListLocalDrivingLicesnseApplications()
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

        private void frmListLocalDrivingLicesnseApplications_Load(object sender, EventArgs e)
        {
            _dtAllLocalDrivingLicenseApplications = clsLocalDrivingLicenseApplication.GetAllLocalDrivingLicenseApplications();
            dgvLocalDrivingLicenseApplications.DataSource = _dtAllLocalDrivingLicenseApplications;
            
            lblRecordsCount.Text = dgvLocalDrivingLicenseApplications.Rows.Count.ToString();
            if (dgvLocalDrivingLicenseApplications.Rows.Count>0)
            {

                dgvLocalDrivingLicenseApplications.Columns[0].HeaderText = "L.D.L.AppID";
                dgvLocalDrivingLicenseApplications.Columns[0].Width = 120;

                dgvLocalDrivingLicenseApplications.Columns[1].HeaderText = "Driving Class";
                dgvLocalDrivingLicenseApplications.Columns[1].Width = 300;

                dgvLocalDrivingLicenseApplications.Columns[2].HeaderText = "National No.";
                dgvLocalDrivingLicenseApplications.Columns[2].Width = 150;

                dgvLocalDrivingLicenseApplications.Columns[3].HeaderText = "Full Name";
                dgvLocalDrivingLicenseApplications.Columns[3].Width = 350;

                dgvLocalDrivingLicenseApplications.Columns[4].HeaderText = "Application Date";
                dgvLocalDrivingLicenseApplications.Columns[4].Width = 170;

                dgvLocalDrivingLicenseApplications.Columns[5].HeaderText = "Passed Tests";
                dgvLocalDrivingLicenseApplications.Columns[5].Width = 150;
            }

            cbFilterBy.SelectedIndex = 0;


        }

        private void showDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmLocalDrivingLicenseApplicationInfo frm =
                        new frmLocalDrivingLicenseApplicationInfo((int)dgvLocalDrivingLicenseApplications.CurrentRow.Cells[0].Value);
            frm.ShowDialog();
            //refresh
            frmListLocalDrivingLicesnseApplications_Load(null, null);

        }

        private void cbFilterBy_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtFilterValue.Visible = (cbFilterBy.Text != "None");

            if (txtFilterValue.Visible)
            {
                txtFilterValue.Text = "";
                txtFilterValue.Focus();
            }

            _dtAllLocalDrivingLicenseApplications.DefaultView.RowFilter = "";
            lblRecordsCount.Text = dgvLocalDrivingLicenseApplications.Rows.Count.ToString();
        }

        private void txtFilterValue_TextChanged(object sender, EventArgs e)
        {

            string FilterColumn = "";
            //Map Selected Filter to real Column name 
            switch (cbFilterBy.Text)
            {

                case "L.D.L.AppID":
                    FilterColumn = "LocalDrivingLicenseApplicationID";
                    break;

                case "National No.":
                    FilterColumn = "NationalNo";
                    break;


                case "Full Name":
                    FilterColumn = "FullName";
                    break;

                case "Status":
                    FilterColumn = "Status";
                    break;


                default:
                    FilterColumn = "None";
                    break;

            }

            //Reset the filters in case nothing selected or filter value conains nothing.
            if (txtFilterValue.Text.Trim() == "" || FilterColumn == "None")
            {
                _dtAllLocalDrivingLicenseApplications.DefaultView.RowFilter = "";
                lblRecordsCount.Text = dgvLocalDrivingLicenseApplications.Rows.Count.ToString();
                return;
            }


            if (FilterColumn == "LocalDrivingLicenseApplicationID")
                //in this case we deal with integer not string.
                _dtAllLocalDrivingLicenseApplications.DefaultView.RowFilter = string.Format("[{0}] = {1}", FilterColumn, txtFilterValue.Text.Trim());
            else
                _dtAllLocalDrivingLicenseApplications.DefaultView.RowFilter = string.Format("[{0}] LIKE '{1}%'", FilterColumn, txtFilterValue.Text.Trim());

            lblRecordsCount.Text = dgvLocalDrivingLicenseApplications.Rows.Count.ToString();
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {

            int LocalDrivingLicenseApplicationID = (int)dgvLocalDrivingLicenseApplications.CurrentRow.Cells[0].Value;

            frmAddUpdateLocalDrivingLicesnseApplication frm =
                         new frmAddUpdateLocalDrivingLicesnseApplication(LocalDrivingLicenseApplicationID);
            frm.ShowDialog();

            frmListLocalDrivingLicesnseApplications_Load(null, null);
        }

        private void txtFilterValue_KeyPress(object sender, KeyPressEventArgs e)
        {
            //we allow number incase L.D.L.AppID id is selected.
            if (cbFilterBy.Text == "L.D.L.AppID")
                e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void vistionTestToolStripMenuItem_Click(object sender, EventArgs e)
        {

            int LocalDrivingLicenseApplicationID = (int)dgvLocalDrivingLicenseApplications.CurrentRow.Cells[0].Value;

            clsTestAppointment Appointment =
                clsTestAppointment.GetLastTestAppointment(LocalDrivingLicenseApplicationID, clsTestType.enTestType.VisionTest);

            if (Appointment == null)
            {

                MessageBox.Show(
                    _Translate(
                        "لم يتم العثور على موعد لاختبار النظر.",
                        "No Vision Test Appointment Found!"),
                    _Translate("حجز موعد اختبار", "Set Appointment"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }


            frmTakeTest frm = new frmTakeTest(Appointment.TestAppointmentID, clsTestType.enTestType.VisionTest);
            frm.ShowDialog();


        }

        private void writtenTestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int LocalDrivingLicenseApplicationID = (int)dgvLocalDrivingLicenseApplications.CurrentRow.Cells[0].Value;


            if (!clsLocalDrivingLicenseApplication.DoesPassTestType(LocalDrivingLicenseApplicationID, clsTestType.enTestType.VisionTest))
            {


                MessageBox.Show(
                    _Translate(
                        "يجب على المتقدم اجتياز اختبار النظر أولًا.",
                        "The applicant must pass the Vision Test first."),
                    _Translate("غير مسموح", "Not Allowed"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            clsTestAppointment Appointment =
               clsTestAppointment.GetLastTestAppointment(LocalDrivingLicenseApplicationID, clsTestType.enTestType.WrittenTest);


            if (Appointment == null)
            {

                MessageBox.Show(
                    _Translate(
                        "لم يتم العثور على موعد للاختبار النظري.",
                        "No Written Test Appointment Found!"),
                    _Translate("حجز موعد اختبار", "Set Appointment"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }


            frmTakeTest frm = new frmTakeTest(Appointment.TestAppointmentID, clsTestType.enTestType.WrittenTest);
            frm.ShowDialog();

        }

        private void streetTestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int LocalDrivingLicenseApplicationID = (int)dgvLocalDrivingLicenseApplications.CurrentRow.Cells[0].Value;

            if (!clsLocalDrivingLicenseApplication.DoesPassTestType(LocalDrivingLicenseApplicationID, clsTestType.enTestType.WrittenTest))
            {

                MessageBox.Show(
                    _Translate(
                        "يجب على المتقدم اجتياز الاختبار النظري أولًا.",
                        "Person should pass the Written Test first!"),
                    _Translate("غير مسموح", "Not Allowed"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            clsTestAppointment Appointment =
             clsTestAppointment.GetLastTestAppointment(LocalDrivingLicenseApplicationID, clsTestType.enTestType.StreetTest);


            if (Appointment == null)
            {

                MessageBox.Show(
                    _Translate(
                        "لم يتم العثور على موعد للاختبار العملي.",
                        "No Street Test Appointment Found!"),
                    _Translate("حجز موعد اختبار", "Set Appointment"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }


            frmTakeTest frm = new frmTakeTest(Appointment.TestAppointmentID, clsTestType.enTestType.StreetTest);
            frm.ShowDialog();


        }

        private void _ScheduleTest(clsTestType.enTestType TestType)
        {

            int LocalDrivingLicenseApplicationID = (int)dgvLocalDrivingLicenseApplications.CurrentRow.Cells[0].Value;
            frmListTestAppointments frm = new frmListTestAppointments(LocalDrivingLicenseApplicationID, TestType);
            frm.ShowDialog();
            //refresh
            frmListLocalDrivingLicesnseApplications_Load(null, null);

        }
      
        private void scheduleVisionTestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _ScheduleTest(clsTestType.enTestType.VisionTest);
        }

        private void scheduleWrittenTestToolStripMenuItem_Click(object sender, EventArgs e)
        {

            _ScheduleTest(clsTestType.enTestType.WrittenTest);
        }

        private void scheduleStreetTestToolStripMenuItem_Click(object sender, EventArgs e)
        {

            _ScheduleTest(clsTestType.enTestType.StreetTest);
        }

        private void btnAddNewApplication_Click(object sender, EventArgs e)
        {
            frmAddUpdateLocalDrivingLicesnseApplication frm = new frmAddUpdateLocalDrivingLicesnseApplication();
            frm.ShowDialog();
            //refresh
            frmListLocalDrivingLicesnseApplications_Load(null, null);
        }

        private void issueDrivingLicenseFirstTimeToolStripMenuItem_Click(object sender, EventArgs e)
        {

            int LocalDrivingLicenseApplicationID = (int)dgvLocalDrivingLicenseApplications.CurrentRow.Cells[0].Value;
            frmIssueDriverLicenseFirstTime frm = new frmIssueDriverLicenseFirstTime(LocalDrivingLicenseApplicationID);
            frm.ShowDialog();
            //refresh
            frmListLocalDrivingLicesnseApplications_Load(null, null);
        }

        private void cmsApplications_Opening(object sender, CancelEventArgs e)
        {
            int LocalDrivingLicenseApplicationID = (int)dgvLocalDrivingLicenseApplications.CurrentRow.Cells[0].Value;
            clsLocalDrivingLicenseApplication LocalDrivingLicenseApplication =
                    clsLocalDrivingLicenseApplication.FindByLocalDrivingAppLicenseID
                                                    (LocalDrivingLicenseApplicationID);

            int TotalPassedTests = (int)dgvLocalDrivingLicenseApplications.CurrentRow.Cells[5].Value;

            bool LicenseExists = LocalDrivingLicenseApplication.IsLicenseIssued();

            //Enabled only if person passed all tests and Does not have license. 
            issueDrivingLicenseFirstTimeToolStripMenuItem.Enabled = (TotalPassedTests == 3) && !LicenseExists;
            
            showLicenseToolStripMenuItem.Enabled = LicenseExists;
            editToolStripMenuItem.Enabled = !LicenseExists && (LocalDrivingLicenseApplication.ApplicationStatus == clsApplication.enApplicationStatus.New);
            ScheduleTestsMenue.Enabled = !LicenseExists;

            //Enable/Disable Cancel Menue Item
            //We only canel the applications with status=new.
            CancelApplicaitonToolStripMenuItem.Enabled = (LocalDrivingLicenseApplication.ApplicationStatus==clsApplication.enApplicationStatus.New);

            //Enable/Disable Delete Menue Item
            //We only allow delete incase the application status is new not complete or Cancelled.
            DeleteApplicationToolStripMenuItem.Enabled = 
                (LocalDrivingLicenseApplication.ApplicationStatus == clsApplication.enApplicationStatus.New );



            //Enable Disable Schedule menue and it's sub menue
            bool PassedVisionTest = LocalDrivingLicenseApplication.DoesPassTestType(clsTestType.enTestType.VisionTest); ;
            bool PassedWrittenTest = LocalDrivingLicenseApplication.DoesPassTestType(clsTestType.enTestType.WrittenTest);
            bool PassedStreetTest = LocalDrivingLicenseApplication.DoesPassTestType(clsTestType.enTestType.StreetTest);

            ScheduleTestsMenue.Enabled = (!PassedVisionTest || !PassedWrittenTest || !PassedStreetTest) && (LocalDrivingLicenseApplication.ApplicationStatus == clsApplication.enApplicationStatus.New);

            if (ScheduleTestsMenue.Enabled)
            {
                //To Allow Schdule vision test, Person must not passed the same test before.
                scheduleVisionTestToolStripMenuItem.Enabled = !PassedVisionTest;

                //To Allow Schdule written test, Person must pass the vision test and must not passed the same test before.
                scheduleWrittenTestToolStripMenuItem.Enabled = PassedVisionTest && !PassedWrittenTest;

                //To Allow Schdule steet test, Person must pass the vision * written tests, and must not passed the same test before.
                scheduleStreetTestToolStripMenuItem.Enabled = PassedVisionTest && PassedWrittenTest && !PassedStreetTest;

            }



        }

        private void showLicenseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int LocalDrivingLicenseApplicationID = (int)dgvLocalDrivingLicenseApplications.CurrentRow.Cells[0].Value;

            int LicenseID = clsLocalDrivingLicenseApplication.FindByLocalDrivingAppLicenseID(
               LocalDrivingLicenseApplicationID).GetActiveLicenseID();

            if (LicenseID != -1)
            {
                frmShowLicenseInfo frm = new frmShowLicenseInfo(LicenseID);
                frm.ShowDialog();

            }
            else
            {

                MessageBox.Show(
                    _Translate(
                        "لم يتم العثور على رخصة قيادة لهذا الطلب.",
                        "No License Found!"),
                    _Translate("لا توجد رخصة", "No License"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }


        }


        private void CancelApplicaitonToolStripMenuItem_Click(
            object sender, EventArgs e)
        {
            if (dgvLocalDrivingLicenseApplications.CurrentRow == null)
                return;

            if (MessageBox.Show(
                _Translate(
                    "هل أنت متأكد من إلغاء طلب رخصة القيادة؟",
                    "Are you sure you want to cancel this application?"),
                _Translate("تأكيد الإلغاء", "Confirm Cancellation"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2) != DialogResult.Yes)
            {
                return;
            }

            int LocalDrivingLicenseApplicationID =
                Convert.ToInt32(
                    dgvLocalDrivingLicenseApplications.CurrentRow.Cells[0].Value);

            clsLocalDrivingLicenseApplication application =
                clsLocalDrivingLicenseApplication
                .FindByLocalDrivingAppLicenseID(
                    LocalDrivingLicenseApplicationID);

            if (application == null)
                return;

            if (application.Cancel())
            {
                MessageBox.Show(
                    _Translate(
                        "تم إلغاء طلب رخصة القيادة بنجاح.",
                        "Application cancelled successfully."),
                    _Translate("تم الإلغاء", "Cancelled"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                frmListLocalDrivingLicesnseApplications_Load(null, null);
            }
            else
            {
                MessageBox.Show(
                    _Translate(
                        "تعذّر إلغاء طلب رخصة القيادة.",
                        "Could not cancel the application."),
                    _Translate("خطأ", "Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }



        private void DeleteApplicationToolStripMenuItem_Click(
            object sender, EventArgs e)
        {
            if (dgvLocalDrivingLicenseApplications.CurrentRow == null)
                return;

            if (MessageBox.Show(
                _Translate(
                    "هل أنت متأكد من حذف طلب رخصة القيادة؟",
                    "Are you sure you want to delete this application?"),
                _Translate("تأكيد الحذف", "Confirm Delete"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2) != DialogResult.Yes)
            {
                return;
            }

            int LocalDrivingLicenseApplicationID =
                Convert.ToInt32(
                    dgvLocalDrivingLicenseApplications.CurrentRow.Cells[0].Value);

            clsLocalDrivingLicenseApplication application =
                clsLocalDrivingLicenseApplication
                .FindByLocalDrivingAppLicenseID(
                    LocalDrivingLicenseApplicationID);

            if (application == null)
                return;

            if (application.Delete())
            {
                MessageBox.Show(
                    _Translate(
                        "تم حذف طلب رخصة القيادة بنجاح.",
                        "Application deleted successfully."),
                    _Translate("تم الحذف", "Deleted"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                frmListLocalDrivingLicesnseApplications_Load(null, null);
            }
            else
            {
                MessageBox.Show(
                    _Translate(
                        "تعذّر حذف الطلب بسبب وجود بيانات أخرى مرتبطة به.",
                        "Could not delete the application because other data depends on it."),
                    _Translate("خطأ", "Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


        private void showPersonLicenseHistoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int LocalDrivingLicenseApplicationID = (int)dgvLocalDrivingLicenseApplications.CurrentRow.Cells[0].Value;
            clsLocalDrivingLicenseApplication localDrivingLicenseApplication = clsLocalDrivingLicenseApplication.FindByLocalDrivingAppLicenseID(LocalDrivingLicenseApplicationID);

            frmShowPersonLicenseHistory frm = new frmShowPersonLicenseHistory(localDrivingLicenseApplication.ApplicantPersonID);
            frm.ShowDialog();
        }
    }
}
