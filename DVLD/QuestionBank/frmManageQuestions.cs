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

namespace DVLD.QuestionBank
{
    public partial class frmManageQuestions : Form
    {
        public frmManageQuestions()
        {
            InitializeComponent();
        }
        private DataTable _dtAllQuestions = new DataTable();
        private DataTable _dtQuestions = new DataTable();

        private void _RefreshQuestionsList()
        {
            try
            {
                _dtAllQuestions = clsQuestionBank.GetAllQuestions();

                _dtQuestions = _dtAllQuestions.DefaultView.ToTable(
                    false,
                    "QuestionID",
                    "QuestionText",
                    "QuestionType",
                    "OptionA",
                    "OptionB",
                    "OptionC",
                    "OptionD",
                    "CorrectOption",
                    "Explanation",
                    "ReviewStatus",
                    "IsActive");

                dgvQuestions.DataSource = _dtQuestions;
                lblRecordsCount.Text = dgvQuestions.Rows.Count.ToString();
            }
            catch (Exception ex)
            {
                _dtAllQuestions = new DataTable();
                _dtQuestions = new DataTable();

                dgvQuestions.DataSource = null;
                lblRecordsCount.Text = "0";

                MessageBox.Show(
                    "Could not load qustion data.\n\n" + ex.Message,
                    "Database Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void frmManageQuestions_Load(object sender, EventArgs e)
        {
            _RefreshQuestionsList();
            if (dgvQuestions.Rows.Count > 0)
            {

                dgvQuestions.Columns[0].HeaderText = "Question ID";
                dgvQuestions.Columns[0].Width = 110;

                dgvQuestions.Columns[1].HeaderText = "Question Text.";
                dgvQuestions.Columns[1].Width = 120;


                dgvQuestions.Columns[2].HeaderText = "Question Type";
                dgvQuestions.Columns[2].Width = 120;

                dgvQuestions.Columns[3].HeaderText = "OptionA";
                dgvQuestions.Columns[3].Width = 140;


                dgvQuestions.Columns[4].HeaderText = "OptionB";
                dgvQuestions.Columns[4].Width = 120;

                dgvQuestions.Columns[5].HeaderText = "OptionC";
                dgvQuestions.Columns[5].Width = 120;

                dgvQuestions.Columns[6].HeaderText = "OptionD";
                dgvQuestions.Columns[6].Width = 120;

                dgvQuestions.Columns[7].HeaderText = " Correct Option";
                dgvQuestions.Columns[7].Width = 140;

                dgvQuestions.Columns[8].HeaderText = "Explanation";
                dgvQuestions.Columns[8].Width = 120;


                dgvQuestions.Columns[9].HeaderText = "Review Status";
                dgvQuestions.Columns[9].Width = 120;


                dgvQuestions.Columns[10].HeaderText = "IsActive";
                dgvQuestions.Columns[10].Width = 170;
            }

        }

        private void cbFilterBy_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbFilterBy.Text == "Is Active")
            {
                txtFilterValue.Visible = false;
                cbReviewStatus.Visible = false;
                cbIsActive.Visible = true;
                cbIsActive.Focus();
                cbIsActive.SelectedIndex = 0;
            }
            else if(cbFilterBy.Text == "Review Status")
            {
                txtFilterValue.Visible = false;
                cbIsActive.Visible = false;
                cbReviewStatus.Visible = true;
                cbReviewStatus.Focus();
                cbReviewStatus.SelectedIndex = 0;
            }
            else

            {

                txtFilterValue.Visible = (cbFilterBy.Text != "None");
                cbIsActive.Visible = false;
                cbReviewStatus.Visible = false;
                if (cbFilterBy.Text == "None")
                {
                    txtFilterValue.Enabled = false;
                }
                else
                    txtFilterValue.Enabled = true;

                txtFilterValue.Text = "";
                txtFilterValue.Focus();
            }
        }

        private void cbIsActive_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_dtQuestions == null || _dtQuestions.Columns.Count == 0)
                return;

            switch (cbIsActive.Text)
            {
                case "Yes":
                    _dtQuestions.DefaultView.RowFilter = "[IsActive] = True";
                    break;

                case "No":
                    _dtQuestions.DefaultView.RowFilter = "[IsActive] = False";
                    break;

                default:
                    _dtQuestions.DefaultView.RowFilter = "";
                    break;
            }

            lblRecordsCount.Text =
                _dtQuestions.DefaultView.Count.ToString();
        }

        private void cbReviewStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_dtQuestions == null || _dtQuestions.Columns.Count == 0)
                return;

            string filterValue = cbReviewStatus.Text;

            if (filterValue == "All" || string.IsNullOrWhiteSpace(filterValue))
            {
                _dtQuestions.DefaultView.RowFilter = "";
            }
            else
            {
                _dtQuestions.DefaultView.RowFilter =
                    $"[ReviewStatus] = '{filterValue}'";
            }

            lblRecordsCount.Text =
                _dtQuestions.DefaultView.Count.ToString();
        }

        private void txtFilterValue_TextChanged(object sender, EventArgs e)
        {

            if (_dtQuestions == null || _dtQuestions.Columns.Count == 0)
                return;

            string search = txtFilterValue.Text.Trim();
            string filterBy = cbFilterBy.Text;

            if (search == "" || filterBy == "None")
            {
                _dtQuestions.DefaultView.RowFilter = "";
            }
            else if (filterBy == "Question ID")
            {
                int questionID;

                if (int.TryParse(search, out questionID))
                {
                    _dtQuestions.DefaultView.RowFilter =
                        "[QuestionID] = " + questionID;
                }
                else
                {
                    _dtQuestions.DefaultView.RowFilter = "1 = 0";
                }
            }
            else if (filterBy == "Question Text" ||
                     filterBy == "Question Type")
            {
                // حماية البحث من علامات الاقتباس والرموز الخاصة.
                string safeSearch =
                    System.Text.RegularExpressions.Regex.Replace(
                        search.Replace("'", "''"),
                        @"[\[\]%*]",
                        m => "[" + m.Value + "]");

                if (filterBy == "Question Text")
                {
                    _dtQuestions.DefaultView.RowFilter =
                        "[QuestionText] LIKE '%" + safeSearch + "%'";
                }
                else
                {
                    _dtQuestions.DefaultView.RowFilter =
                        "[QuestionType] LIKE '" + safeSearch + "%'";
                }
            }

            lblRecordsCount.Text =
                _dtQuestions.DefaultView.Count.ToString();
        
    }

        private void btnAddPerson_Click(object sender, EventArgs e)
        {
            frmAddUpdateQuestion frm=new frmAddUpdateQuestion();
            frm.ShowDialog();
            _RefreshQuestionsList();
        }

        private void editQuestionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmAddUpdateQuestion frm = new frmAddUpdateQuestion((int)dgvQuestions.CurrentRow.Cells[0].Value);
            frm.ShowDialog();
            _RefreshQuestionsList();
        }

        private void addNewQuestionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmAddUpdateQuestion frm = new frmAddUpdateQuestion();
            frm.ShowDialog();
            _RefreshQuestionsList();
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _RefreshQuestionsList();
        }

        private void showDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmShowQuestionInfo frm=new frmShowQuestionInfo((int)dgvQuestions.CurrentRow.Cells[0].Value);
            frm.ShowDialog();
            _RefreshQuestionsList();
        }

        private void approveQuestionToolStripMenuItem_Click(object sender, EventArgs e)
        {



            if (dgvQuestions.CurrentRow == null)
                return;

            int questionID = Convert.ToInt32(
                dgvQuestions.CurrentRow.Cells["QuestionID"].Value);

            try
            {
                clsQuestionBank question =
                    clsQuestionBank.Find(questionID);

                if (question == null)
                {
                    MessageBox.Show(
                        "Question not found.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                if (!question.IsActive)
                {
                    MessageBox.Show(
                        "Cannot approve an inactive question.",
                        "Inactive Question",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                if (question.ReviewStatus ==
                    clsQuestionBank.enReviewStatus.Approved)
                {
                    MessageBox.Show(
                        "This question is already approved.",
                        "Information",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                if (MessageBox.Show(
                    "Are you sure you want to approve question " +
                    questionID + "?",
                    "Confirm Approval",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }

                if (question.Approve())
                {
                    MessageBox.Show(
                        "Question approved successfully.",
                        "Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    _RefreshQuestionsList();
                }
                else
                {
                    MessageBox.Show(
                        "Failed to approve the question.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        

        private void rejectQuestionToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (dgvQuestions.CurrentRow == null)
                return;

            int questionID = Convert.ToInt32(
                dgvQuestions.CurrentRow.Cells["QuestionID"].Value);

            try
            {
                clsQuestionBank question =
                    clsQuestionBank.Find(questionID);

                if (question == null)
                {
                    MessageBox.Show(
                        "Question not found.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                if (!question.IsActive)
                {
                    MessageBox.Show(
                        "Cannot reject an inactive question.",
                        "Inactive Question",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                if (question.ReviewStatus ==
                    clsQuestionBank.enReviewStatus.Rejected)
                {
                    MessageBox.Show(
                        "This question is already rejected.",
                        "Information",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                if (MessageBox.Show(
                    "Are you sure you want to reject question "
                    + questionID + "?",
                    "Confirm Rejection",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }

                if (question.Reject())
                {
                    MessageBox.Show(
                        "Question rejected successfully.",
                        "Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    _RefreshQuestionsList();
                }
                else
                {
                    MessageBox.Show(
                        "Failed to reject the question.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        

        private void activateQuestionToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (dgvQuestions.CurrentRow == null)
                return;

            int questionID = Convert.ToInt32(
                dgvQuestions.CurrentRow.Cells["QuestionID"].Value);

            try
            {
                clsQuestionBank question =
                    clsQuestionBank.Find(questionID);

                if (question == null)
                {
                    MessageBox.Show(
                        "Question not found.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                if (question.IsActive)
                {
                    MessageBox.Show(
                        "This question is already active.",
                        "Information",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                if (MessageBox.Show(
                    "Are you sure you want to activate question "
                    + questionID + "?",
                    "Confirm Activation",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }

                if (question.Activate())
                {
                    MessageBox.Show(
                        "Question activated successfully.",
                        "Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    _RefreshQuestionsList();
                }
                else
                {
                    MessageBox.Show(
                        "Failed to activate the question.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        

        private void deactivateQuestionToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (dgvQuestions.CurrentRow == null)
                return;

            int questionID = Convert.ToInt32(
                dgvQuestions.CurrentRow.Cells["QuestionID"].Value);

            try
            {
                clsQuestionBank question =
                    clsQuestionBank.Find(questionID);

                if (question == null)
                {
                    MessageBox.Show(
                        "Question not found.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                if (!question.IsActive)
                {
                    MessageBox.Show(
                        "This question is already inactive.",
                        "Information",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                if (MessageBox.Show(
                    "Are you sure you want to deactivate question "
                    + questionID + "?",
                    "Confirm Deactivation",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }

                if (question.Deactivate())
                {
                    MessageBox.Show(
                        "Question deactivated successfully.",
                        "Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    _RefreshQuestionsList();
                }
                else
                {
                    MessageBox.Show(
                        "Failed to deactivate the question.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        

        private void deleteQuestionToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (dgvQuestions.CurrentRow == null)
                return;

            int questionID = Convert.ToInt32(
                dgvQuestions.CurrentRow.Cells["QuestionID"].Value);

            if (MessageBox.Show(
                "Are you sure you want to permanently delete Question "
                + questionID + "?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2)
                != DialogResult.Yes)
            {
                return;
            }

            try
            {
                clsQuestionBank question =
                    clsQuestionBank.Find(questionID);

                if (question == null)
                {
                    MessageBox.Show(
                        "Question not found.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    _RefreshQuestionsList();
                    return;
                }

                if (question.Delete())
                {
                    MessageBox.Show(
                        "Question deleted successfully.",
                        "Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    _RefreshQuestionsList();
                }
                else
                {
                    MessageBox.Show(
                        "The question could not be deleted. " +
                        "It may be used in an official exam. " +
                        "You can deactivate it instead.",
                        "Delete Failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not delete the question.\n\n" + ex.Message,
                    "Delete Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        

        private void dgvQuestions_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.RowIndex >= 0)
            {
                dgvQuestions.ClearSelection();
                dgvQuestions.Rows[e.RowIndex].Selected = true;

                dgvQuestions.CurrentCell =
                    dgvQuestions.Rows[e.RowIndex].Cells[0];
            }
        }
    }
}
