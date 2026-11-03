using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DVLD.Classes;
using System.Globalization;
using DVLD_Buisness;
using System.Data;
using DVLD.People;
using DVLD.Applications;
using System.Globalization;
namespace DVLD
{
    public partial class ctrlDashboard : UserControl
    {
        public ctrlDashboard()
        {
            InitializeComponent();
            this.Load += ctrlDashboard_Load;
            dgvTodayAppointments.CellFormatting +=
    dgvTodayAppointments_CellFormatting;
        }


        private void dgvTodayAppointments_CellFormatting(
            object sender,
            DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 ||
                e.Value == null || e.Value == DBNull.Value)
                return;

            bool isArabic =
                CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ar";

            if (!isArabic)
                return;

            string columnName =
                dgvTodayAppointments.Columns[e.ColumnIndex].DataPropertyName;

            string value = Convert.ToString(e.Value);

            if (columnName == "TestType")
            {
                switch (value)
                {
                    case "Vision Test":
                        e.Value = "فحص النظر";
                        break;

                    case "Written Test":
                        e.Value = "الاختبار النظري";
                        break;

                    case "Street Test":
                        e.Value = "اختبار القيادة العملي";
                        break;
                }

                e.FormattingApplied = true;
            }
            else if (columnName == "Status")
            {
                switch (value)
                {
                    case "Scheduled":
                        e.Value = "مجدول";
                        break;

                    case "Completed":
                        e.Value = "مكتمل";
                        break;
                }

                e.FormattingApplied = true;
            }
            else if (columnName == "AppointmentTime")
            {
                if (e.Value is DateTime appointmentTime)
                {
                    string time =
                        appointmentTime.ToString("hh:mm");

                    string period =
                        appointmentTime.Hour < 12
                            ? " صباحًا"
                            : " مساءً";

                    e.Value = time + period;
                    e.FormattingApplied = true;
                }
            }
        }


        private string _Translate(string arabic, string english)
        {
            bool isArabic =
                CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ar";

            return isArabic ? arabic : english;
        }



        private void ctrlDashboard_Load(object sender, EventArgs e)
        {
            bool isArabic =
                CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ar";

            string username =
                clsGlobal.CurrentUser?.UserName ??
                _Translate("مستخدم", "User");

            lblWelcome.Text =
                _Translate("مرحبًا، ", "Welcome, ") + username;

            CultureInfo dateCulture =
                CultureInfo.GetCultureInfo(
                    isArabic ? "ar-PS" : "en-US");

            lblDate.Text = DateTime.Now.ToString(
                isArabic ? "dddd، d MMMM yyyy"
                         : "dddd, MMMM d, yyyy",
                dateCulture);

            RefreshDashboardData();
        }


        private void LoadActiveLicensesCount()
        {
            DataTable licenses = clsLicense.GetAllLicenses();

            int activeCount = 0;

            foreach (DataRow row in licenses.Rows)
            {
                bool isActive = Convert.ToBoolean(row["IsActive"]);

                DateTime expirationDate =
                    Convert.ToDateTime(row["ExpirationDate"]);

                if (isActive && expirationDate.Date >= DateTime.Today)
                {
                    activeCount++;
                }
            }

            lblActiveLicensesCount.Text = activeCount.ToString();
        }


        private void LoadDetainedLicensesCount()
        {
            DataTable detainedLicenses =
                clsDetainedLicense.GetAllDetainedLicenses();

            int detainedCount = 0;

            foreach (DataRow row in detainedLicenses.Rows)
            {
                bool isReleased = Convert.ToBoolean(row["IsReleased"]);

                if (!isReleased)
                {
                    detainedCount++;
                }
            }

            lblDetainedCount.Text = detainedCount.ToString();
        }


        private void LoadTodayTestsCount()
        {
            DataTable appointments =
                clsTestAppointment.GetAllTestAppointments();

            int todayTestsCount = 0;

            foreach (DataRow row in appointments.Rows)
            {
                if (row["AppointmentDate"] == DBNull.Value)
                    continue;

                DateTime appointmentDate =
                    Convert.ToDateTime(row["AppointmentDate"]);

                if (appointmentDate.Date == DateTime.Today)
                {
                    todayTestsCount++;
                }
            }

            lblTodayTestsCount.Text = todayTestsCount.ToString();
        }



        private void LoadPendingApplicationsCount()
        {
            int pendingCount =
                clsApplication.GetPendingApplicationsCount();

            lblPendingCount.Text = pendingCount.ToString();
        }


        private void LoadTodayAppointments()
        {
            DataTable appointments =
                clsTestAppointment.GetTodayDashboardAppointments(
                    DateTime.Today);

            dgvTodayAppointments.AutoGenerateColumns = true;
            dgvTodayAppointments.DataSource = appointments;

            if (dgvTodayAppointments.Columns.Contains("TestAppointmentID"))
            {
                dgvTodayAppointments.Columns["TestAppointmentID"].Visible = false;
            }

            if (dgvTodayAppointments.Columns.Contains("Candidate"))
            {
                dgvTodayAppointments.Columns["Candidate"].HeaderText =
     _Translate("اسم المتقدم", "Candidate");
            }

            if (dgvTodayAppointments.Columns.Contains("TestType"))
            {
                dgvTodayAppointments.Columns["TestType"].HeaderText =
     _Translate("نوع الاختبار", "Test Type");
            }

            if (dgvTodayAppointments.Columns.Contains("AppointmentTime"))
            {
                dgvTodayAppointments.Columns["AppointmentTime"].HeaderText =
    _Translate("الوقت", "Time");

                dgvTodayAppointments.Columns["AppointmentTime"]
                    .DefaultCellStyle.Format = "hh:mm tt";
            }

            if (dgvTodayAppointments.Columns.Contains("Status"))
            {
                dgvTodayAppointments.Columns["Status"].HeaderText =
    _Translate("الحالة", "Status");
            }

            bool isArabic =
    CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ar";

            dgvTodayAppointments.RightToLeft =
                isArabic ? RightToLeft.Yes : RightToLeft.No;
            dgvTodayAppointments.ClearSelection();
        }

        private void btnAddPerson_Click(object sender, EventArgs e)
        {
            using (frmAddUpdatePerson frm = new frmAddUpdatePerson())
            {
                frm.ShowDialog();
            }

            RefreshDashboardData();
        }

        private void btnSearchLicense_Click(object sender, EventArgs e)
        {
            using (frmSearchLicense frm = new frmSearchLicense())
            {
                frm.ShowDialog(this);
            }

            
        }

        private void btnNewLicenseApplication_Click(object sender, EventArgs e)
        {
            using (frmAddUpdateLocalDrivingLicesnseApplication frm =
     new frmAddUpdateLocalDrivingLicesnseApplication())
            {
                frm.ShowDialog();
            }
            RefreshDashboardData();
        }


        public void RefreshDashboardData()
        {
            LoadPendingApplicationsCount();
            LoadActiveLicensesCount();
            LoadTodayTestsCount();
            LoadDetainedLicensesCount();

            LoadTodayAppointments();
        }

        private void lblPendingCount_Click(object sender, EventArgs e)
        {

        }

        private void lblTodayTestsTitle_Click(object sender, EventArgs e)
        {

        }
    }
}
