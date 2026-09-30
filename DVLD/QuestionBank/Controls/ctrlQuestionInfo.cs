using DVLD.Properties;
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
using static DVLD.People.frmAddUpdatePerson;

namespace DVLD.QuestionBank.Controls
{
    public partial class ctrlQuestionInfo : UserControl
    {
        private clsQuestionBank _Question;

        private int _QuestionID = -1;

        public int QuestionID
        {
            get { return _QuestionID; }
        }

        public clsQuestionBank SelectedQuestionInfo
        {
            get { return _Question; }
        }
        public void ResetnQuestionInfo()
        {
            _QuestionID = -1;
            lblQuestionD.Text = "[????]";
            lblQuestionD.Text = "[????]";
            lblSourceBook.Text = "[????]";
            lblPageNo.Text ="?????";
            lblIsActive.Text =  "[????]";
            lblReviewStatus.Text = "[????]";
            lblTextQuestion .Text = "[????]";
            rdbQuestionA.Text= "[????]";
            rdbQuestionA.Checked = false;
            RdbQuestionB.Text = "[????]";
            RdbQuestionB.Checked = false;
            rdbQuestionC.Text = "[????]";
            rdbQuestionC.Checked = false;
            rdbQuestionD.Text = "[????]";
            rdbQuestionD .Checked = false;
            lblExplanation.Text = "[????]";
          

        }
        public ctrlQuestionInfo()
        {
            InitializeComponent();
            rdbQuestionA.AutoCheck = false;
            RdbQuestionB.AutoCheck = false;
            rdbQuestionC.AutoCheck = false;
            rdbQuestionD.AutoCheck = false;
        }

        private void _FillQuestionInfo()
        {
            if (_Question == null)
                return;

            // Question Information
            _QuestionID = _Question.QuestionID;

            lblQuestionD.Text = _Question.QuestionID.ToString();

            lblIsActive.Text =
                _Question.IsActive ? "Yes" : "No";

            lblReviewStatus.Text =
                _Question.ReviewStatus.ToString();

            // Source Information
            lblSourceBook.Text =
                _Question.SourceDocumentID.HasValue
                    ? "Document ID: " +
                      _Question.SourceDocumentID.Value.ToString()
                    : "Manual Question";

            lblPageNo.Text =
                _Question.SourcePageNumber?.ToString() ?? "N/A";

            // Question Text
            lblTextQuestion.Text =
                _Question.QuestionText ?? "";

            // Answer Options
            rdbQuestionA.Text = _Question.OptionA ?? "";
            RdbQuestionB.Text = _Question.OptionB ?? "";
            rdbQuestionC.Text = _Question.OptionC ?? "";
            rdbQuestionD.Text = _Question.OptionD ?? "";

            // Reset the selected answer
            rdbQuestionA.Checked = false;
            RdbQuestionB.Checked = false;
            rdbQuestionC.Checked = false;
            rdbQuestionD.Checked = false;

            // Highlight the correct answer
            switch (_Question.CorrectOption)
            {
                case "A":
                    rdbQuestionA.Checked = true;
                    break;

                case "B":
                    RdbQuestionB.Checked = true;
                    break;

                case "C":
                    rdbQuestionC.Checked = true;
                    break;

                case "D":
                    rdbQuestionD.Checked = true;
                    break;
            }

            // True / False questions have only two options
            bool isTrueFalse =
                _Question.QuestionType ==
                clsQuestionBank.enQuestionType.TrueFalse;

            rdbQuestionC.Visible = !isTrueFalse;
            rdbQuestionD.Visible = !isTrueFalse;

            // Explanation
            lblExplanation.Text =
                string.IsNullOrWhiteSpace(_Question.Explanation)
                    ? "No explanation available."
                    : _Question.Explanation;
        }

        public void LoadQuestionInfo(int QuestionID)
        {
            _Question = clsQuestionBank.Find(QuestionID);
            if (_Question == null)
            {
                ResetnQuestionInfo();
                MessageBox.Show("No Question with QuestionID = " + QuestionID.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _FillQuestionInfo();
        }

        private void llEditQuestionInfo_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_QuestionID == -1)
                return;

            using (frmAddUpdateQuestion frm =
                new frmAddUpdateQuestion(_QuestionID))
            {
                frm.ShowDialog();
            }

            LoadQuestionInfo(_QuestionID);
        }

        private void ctrlQuestionInfo_Load(object sender, EventArgs e)
        {

        }
    }
}
