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
using System.Net.Http;
namespace DVLD.QuestionBank
{
    public partial class frmAddUpdateQuestion : Form
    {

        public enum enMode { AddNew = 0, Update = 1 };
        private enMode _Mode;
        private int _QuestionID = -1;
        clsQuestionBank _Question;
        private int _SelectedDocumentID = 7; // مؤقت للتجربة
        public frmAddUpdateQuestion()
        {
            InitializeComponent();
            _Mode = enMode.AddNew;

        }
        public frmAddUpdateQuestion(int QuestionID)
        {
            InitializeComponent();
            _QuestionID = QuestionID;
            _Mode = enMode.Update;
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

            if (_Mode == enMode.AddNew)
            {
                lblTitle.Text = _Translate("إضافة سؤال جديد", "Add New Question");
                this.Text = lblTitle.Text;
                _Question = new clsQuestionBank();
            }
            else
            {
                lblTitle.Text = _Translate("تعديل السؤال", "Update Question");
                this.Text = lblTitle.Text;
            }

            txtQuestionText.Text = "";
            txtExplanation.Text = "";
            txtPageNumber.Enabled = false;
            txtPageNumber.Text = "";
            txtSourceEvidence.Enabled = false;
            txtSourceEvidence.Text = "";
            TxtOptionA.Text = "";
            TxtOptionB.Text = "";
            TxtOptionC.Text = "";
            TxtOptionD.Text = "";
            cmbReviewStatues.Enabled = false;
            chbIsActive.Enabled = false;
            cmbCorrectOption.SelectedIndex = -1;
            cmbQuestionType.SelectedItem = "MultipleChoice";
            cmbReviewStatues.SelectedItem = "Draft";
            chbIsActive.Checked = true;
            linkLabel1.Visible = (_Mode == enMode.AddNew);
        }
        private void _LoadData()
        {
            _Question = clsQuestionBank.Find(_QuestionID);

            if (_Question == null)
            {
                MessageBox.Show(
                    _Translate("لم يتم العثور على السؤال رقم: ", "No Question with ID = ") + _QuestionID,
                    _Translate("السؤال غير موجود", "Question Not Found"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);

                this.Close();
                return;
            }

            // نوع السؤال أولاً
            cmbQuestionType.SelectedItem =
                _Question.QuestionType.ToString();

            // بيانات السؤال
            lblQuestionD.Text = _Question.QuestionID.ToString();
            txtQuestionText.Text = _Question.QuestionText ?? "";
            txtExplanation.Text = _Question.Explanation ?? "";

            TxtOptionA.Text = _Question.OptionA ?? "";
            TxtOptionB.Text = _Question.OptionB ?? "";
            TxtOptionC.Text = _Question.OptionC ?? "";
            TxtOptionD.Text = _Question.OptionD ?? "";

            // الإجابة الصحيحة
            cmbCorrectOption.SelectedItem =
                _Question.CorrectOption;

            // معلومات المصدر
            txtPageNumber.Text =
                _Question.SourcePageNumber?.ToString() ?? "";

            txtSourceEvidence.Text =
                _Question.SourceEvidence ?? "";

            // معلومات المراجعة والتفعيل
            cmbReviewStatues.SelectedItem =
                _Question.ReviewStatus.ToString();

            chbIsActive.Checked = _Question.IsActive;
            linkLabel1.Visible = false;
        }
        private void frmAddUpdateQuestion_Load(object sender, EventArgs e)
        {
            _ResetDefualtValues();
            if (_Mode == enMode.Update)
            {
                _LoadData();
            }
        }

        private void pictureBox11_Click(object sender, EventArgs e)
        {

        }

        private void txtQuestionText_Validating(object sender, CancelEventArgs e)
        {
            string questionText = txtQuestionText.Text.Trim();

            if (string.IsNullOrWhiteSpace(questionText))
            {
                e.Cancel = true;
                errorProvider1.SetError(
                    txtQuestionText,
                    _Translate("يجب إدخال نص السؤال.", "Question text is required."));
            }
            else if (questionText.Length > 1000)
            {
                e.Cancel = true;
                errorProvider1.SetError(
                    txtQuestionText,
                    _Translate("لا يجوز أن يتجاوز نص السؤال 1000 حرف.", "Question text cannot exceed 1000 characters."));
            }
            else
            {
                e.Cancel = false;
                errorProvider1.SetError(txtQuestionText, "");
            }
        }
        private void txtOption_Validating(
    object sender, CancelEventArgs e)
        {
            TextBox txt = sender as TextBox;

            if (txt == null)
                return;

            errorProvider1.SetError(txt, "");

            // خيارات الصح والخطأ بنحددها تلقائيًا لاحقًا.
            if (cmbQuestionType.Text == "TrueFalse")
            {
                e.Cancel = false;
                errorProvider1.SetError(txt, "");
                return;
            }

            if (string.IsNullOrWhiteSpace(txt.Text))
            {
                e.Cancel = true;

                errorProvider1.SetError(
                    txt,
                    _Translate("يجب إدخال هذا الخيار.", "This option is required."));

                return;
            }

            if (txt.Text.Trim().Length > 500)
            {
                e.Cancel = true;

                errorProvider1.SetError(
                    txt,
                    _Translate("لا يجوز أن يتجاوز نص الخيار 500 حرف.", "Option cannot exceed 500 characters."));
            }
        }

        private void TxtOptionA_Validating(object sender, CancelEventArgs e)
        {
            txtOption_Validating(sender, e);
        }

        private void TxtOptionB_Validating(object sender, CancelEventArgs e)
        {
            txtOption_Validating(sender, e);

        }

        private void TxtOptionC_Validating(object sender, CancelEventArgs e)
        {
            txtOption_Validating(sender, e);
        }

        private void TxtOptionD_Validating(object sender, CancelEventArgs e)
        {
            txtOption_Validating(sender, e);
        }

        private void cmbQuestionType_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool isTrueFalse = cmbQuestionType.Text == "TrueFalse";

            if (isTrueFalse)
            {
                TxtOptionA.Text = "صح";
                TxtOptionB.Text = "خطأ";

                TxtOptionC.Clear();
                TxtOptionD.Clear();

                TxtOptionA.ReadOnly = true;
                TxtOptionB.ReadOnly = true;
                TxtOptionC.Enabled = false;
                TxtOptionD.Enabled = false;

                if (cmbCorrectOption.Text == "C" ||
                    cmbCorrectOption.Text == "D")
                {
                    cmbCorrectOption.SelectedIndex = -1;
                }
            }
            else
            {
                TxtOptionA.ReadOnly = false;
                TxtOptionB.ReadOnly = false;
                TxtOptionC.Enabled = true;
                TxtOptionD.Enabled = true;
            }

            errorProvider1.SetError(TxtOptionA, "");
            errorProvider1.SetError(TxtOptionB, "");
            errorProvider1.SetError(TxtOptionC, "");
            errorProvider1.SetError(TxtOptionD, "");

        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cmbCorrectOption_Validating(object sender, CancelEventArgs e)
        {

            string answer = cmbCorrectOption.Text;
            string questionType = cmbQuestionType.Text;

            errorProvider1.SetError(cmbCorrectOption, "");

            if (string.IsNullOrWhiteSpace(answer))
            {
                e.Cancel = true;

                errorProvider1.SetError(
                    cmbCorrectOption,
                    _Translate("يرجى تحديد الإجابة الصحيحة.", "Please select the correct answer."));

                return;
            }

            if (questionType == "TrueFalse" &&
                answer != "A" && answer != "B")
            {
                e.Cancel = true;

                errorProvider1.SetError(
                    cmbCorrectOption,
                    _Translate("أسئلة صح أو خطأ تسمح بالخيار A أو B فقط.", "True/False questions allow only A or B."));

                return;
            }

            if (answer != "A" && answer != "B" &&
                answer != "C" && answer != "D")
            {
                e.Cancel = true;

                errorProvider1.SetError(
                    cmbCorrectOption,
                    _Translate("الإجابة الصحيحة المحددة غير صالحة.", "Invalid correct answer."));
            }
        }

        private void txtExplanation_Validating(object sender, CancelEventArgs e)
        {

            if (txtExplanation.Text.Trim().Length > 2000)
            {
                e.Cancel = true;

                errorProvider1.SetError(
                    txtExplanation,
                    _Translate("لا يجوز أن يتجاوز شرح الإجابة 2000 حرف.", "Explanation cannot exceed 2000 characters."));
            }
            else
            {
                e.Cancel = false;
                errorProvider1.SetError(txtExplanation, "");
            }

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // لا نحفظ إذا في أي حقل غير صحيح
            if (!this.ValidateChildren())
            {
                MessageBox.Show(
                    _Translate("يرجى تصحيح الأخطاء الموضحة قبل الحفظ.", "Please correct the highlighted errors."),
                    _Translate("خطأ في التحقق من البيانات", "Validation Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (cmbQuestionType.SelectedIndex == -1)
            {
                errorProvider1.SetError(
                    cmbQuestionType, _Translate("يرجى تحديد نوع السؤال.", "Please select a question type."));
                return;
            }

            // نقل البيانات من الفورم إلى Business Layer
            _Question.QuestionText = txtQuestionText.Text.Trim();

            _Question.QuestionType =
                (clsQuestionBank.enQuestionType)Enum.Parse(
                    typeof(clsQuestionBank.enQuestionType),
                    cmbQuestionType.Text);

            _Question.OptionA = TxtOptionA.Text.Trim();
            _Question.OptionB = TxtOptionB.Text.Trim();

            if (cmbQuestionType.Text == "TrueFalse")
            {
                _Question.OptionC = "";
                _Question.OptionD = "";
            }
            else
            {
                _Question.OptionC = TxtOptionC.Text.Trim();
                _Question.OptionD = TxtOptionD.Text.Trim();
            }

            _Question.CorrectOption = cmbCorrectOption.Text;
            _Question.Explanation = txtExplanation.Text.Trim();

            try
            {
                if (!_Question.Save())
                {
                    MessageBox.Show(
                        _Translate("تعذر حفظ السؤال.", "Failed to save the question."),
                        _Translate("خطأ في حفظ السؤال", "Save Error"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                // بعد الإضافة، يتحول الفورم إلى وضع التعديل
                _QuestionID = _Question.QuestionID;
                _Mode = enMode.Update;

                lblQuestionD.Text = _QuestionID.ToString();
                lblTitle.Text = _Translate("تعديل السؤال", "Update Question");
                this.Text = lblTitle.Text;

                // التعديل يعيد السؤال إلى Draft
                cmbReviewStatues.SelectedItem =
                    _Question.ReviewStatus.ToString();

                chbIsActive.Checked = _Question.IsActive;
                linkLabel1.Visible = false;

                MessageBox.Show(
                    _Translate("تم حفظ السؤال بنجاح.", "Question saved successfully."),
                    _Translate("تم الحفظ", "Saved"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    _Translate("خطأ في حفظ السؤال", "Save Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private async void linkLabel1_LinkClicked(
           object sender,
           LinkLabelLinkClickedEventArgs e)
        {
            // TODO: Open Generate Questions form later.

        }
    }

}
