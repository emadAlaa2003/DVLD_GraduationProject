using DVLD.Controls.ApplicationControls;
using DVLD.Properties;
using DVLD_Buisness;
using System;
using System.Data;
using System.Threading;
using System.Windows.Forms;

namespace DVLD.Tests
{
    public partial class frmListTestAppointments : Form
    {
        private DataTable _dtLicenseTestAppointments;
        private int _LocalDrivingLicenseApplicationID;
        private clsTestType.enTestType _TestType = clsTestType.enTestType.VisionTest;

        public frmListTestAppointments(int LocalDrivingLicenseApplicationID, clsTestType.enTestType TestType)
        {
            InitializeComponent();
            _LocalDrivingLicenseApplicationID = LocalDrivingLicenseApplicationID;
            _TestType = TestType;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private bool _IsArabic()
        {
            return Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName == "ar";
        }

        private string _Translate(string arabic, string english)
        {
            return _IsArabic() ? arabic : english;
        }

        private void _LoadTestTypeImageAndTitle()
        {
            bool isArabic = _IsArabic();

            switch (_TestType)
            {
                case clsTestType.enTestType.VisionTest:
                    lblTitle.Text = isArabic ? "مواعيد اختبار النظر" : "Vision Test Appointments";
                    this.Text = lblTitle.Text;
                    pbTestTypeImage.Image = Resources.Vision_512;
                    break;

                case clsTestType.enTestType.WrittenTest:
                    lblTitle.Text = isArabic ? "مواعيد الاختبار النظري" : "Written Test Appointments";
                    this.Text = lblTitle.Text;
                    pbTestTypeImage.Image = Resources.Written_Test_512;
                    break;

                case clsTestType.enTestType.StreetTest:
                    lblTitle.Text = isArabic ? "مواعيد اختبار القيادة العملي" : "Street Test Appointments";
                    this.Text = lblTitle.Text;
                    pbTestTypeImage.Image = Resources.driving_test_512;
                    break;
            }
        }

        private void frmListTestAppointments_Load(object sender, EventArgs e)
        {
            _LoadTestTypeImageAndTitle();

            ctrlDrivingLicenseApplicationInfo1.LoadApplicationInfoByLocalDrivingAppID(_LocalDrivingLicenseApplicationID);

            _dtLicenseTestAppointments =
                clsTestAppointment.GetApplicationTestAppointmentsPerTestType(
                    _LocalDrivingLicenseApplicationID, _TestType);

            dgvLicenseTestAppointments.DataSource = _dtLicenseTestAppointments;
            lblRecordsCount.Text = dgvLicenseTestAppointments.Rows.Count.ToString();

            if (dgvLicenseTestAppointments.Columns.Count >= 4)
            {
                bool isArabic = _IsArabic();

                dgvLicenseTestAppointments.Columns[0].HeaderText = isArabic ? "رقم الموعد" : "Appointment ID";
                dgvLicenseTestAppointments.Columns[0].Width = 150;

                dgvLicenseTestAppointments.Columns[1].HeaderText = isArabic ? "تاريخ الموعد" : "Appointment Date";
                dgvLicenseTestAppointments.Columns[1].Width = 200;

                dgvLicenseTestAppointments.Columns[2].HeaderText = isArabic ? "الرسوم المدفوعة" : "Paid Fees";
                dgvLicenseTestAppointments.Columns[2].Width = 150;

                dgvLicenseTestAppointments.Columns[3].HeaderText = isArabic ? "مقفل" : "Is Locked";
                dgvLicenseTestAppointments.Columns[3].Width = 100;
            }
        }

        private void btnAddNewAppointment_Click(object sender, EventArgs e)
        {
            clsLocalDrivingLicenseApplication localDrivingLicenseApplication =
                clsLocalDrivingLicenseApplication.FindByLocalDrivingAppLicenseID(
                    _LocalDrivingLicenseApplicationID);

            if (localDrivingLicenseApplication.IsThereAnActiveScheduledTest(_TestType))
            {
                MessageBox.Show(
                    _Translate(
                        "يوجد بالفعل موعد نشط لهذا الاختبار، ولا يمكنك إضافة موعد جديد.",
                        "Person already has an active appointment for this test. You cannot add a new appointment."),
                    _Translate("غير مسموح", "Not Allowed"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            clsTest LastTest = localDrivingLicenseApplication.GetLastTestPerTestType(_TestType);

            if (LastTest == null)
            {
                frmScheduleTest frm1 = new frmScheduleTest(_LocalDrivingLicenseApplicationID, _TestType);
                frm1.ShowDialog();
                frmListTestAppointments_Load(null, null);
                return;
            }

            if (LastTest.TestResult == true)
            {
                MessageBox.Show(
                    _Translate(
                        "لقد اجتاز هذا الشخص هذا الاختبار مسبقًا، ويمكن فقط إعادة الاختبار في حالة الرسوب.",
                        "This person already passed this test before. Only failed tests can be retaken."),
                    _Translate("غير مسموح", "Not Allowed"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            frmScheduleTest frm2 =
                new frmScheduleTest(LastTest.TestAppointmentInfo.LocalDrivingLicenseApplicationID, _TestType);
            frm2.ShowDialog();
            frmListTestAppointments_Load(null, null);
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int TestAppointmentID = (int)dgvLicenseTestAppointments.CurrentRow.Cells[0].Value;

            frmScheduleTest frm = new frmScheduleTest(_LocalDrivingLicenseApplicationID, _TestType, TestAppointmentID);
            frm.ShowDialog();
            frmListTestAppointments_Load(null, null);
        }

        private void takeTestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int TestAppointmentID = (int)dgvLicenseTestAppointments.CurrentRow.Cells[0].Value;

            frmTakeTest frm = new frmTakeTest(TestAppointmentID, _TestType);
            frm.ShowDialog();
            frmListTestAppointments_Load(null, null);
        }
    }
}