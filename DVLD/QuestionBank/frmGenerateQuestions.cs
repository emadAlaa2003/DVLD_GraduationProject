using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DVLD_Buisness;
using System.Net.Http;
using System.Text;
using System.Text.Json;
namespace DVLD.QuestionBank
{
    public partial class frmGenerateQuestions : Form
    {
        private int _CurrentQuestionGenerationJobID = -1;
        private DVLD.frmMain _frmMain;
        private class GenerateQuestionsResponse
        {
            public int QuestionGenerationJobID { get; set; }

            public int DocumentID { get; set; }

            public int MultipleChoiceCount { get; set; }

            public int TrueFalseCount { get; set; }

            public int TotalQuestions { get; set; }

            public string Status { get; set; }

            public string Message { get; set; }
        }
        public frmGenerateQuestions()
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

        public frmGenerateQuestions(DVLD.frmMain frmMain)
        {
            InitializeComponent();

            _frmMain = frmMain;
        }
        private void _LoadReadyDocuments()
        {
            DataTable dtDocuments = new DataTable();

            dtDocuments.Columns.Add(
                "DocumentID",
                typeof(int));

            dtDocuments.Columns.Add(
                "OriginalFileName",
                typeof(string));


            foreach (int documentID in
                     clsKnowledgeDocument.GetActiveReadyDocumentIDs())
            {
                clsKnowledgeDocument document =
                    clsKnowledgeDocument.Find(documentID);

                if (document == null)
                    continue;


                dtDocuments.Rows.Add(
                    document.DocumentID,
                    document.OriginalFileName);
            }


            cmbDocuments.DataSource =
                dtDocuments;

            cmbDocuments.DisplayMember =
                "OriginalFileName";

            cmbDocuments.ValueMember =
                "DocumentID";


            if (cmbDocuments.Items.Count > 0)
                cmbDocuments.SelectedIndex = 0;
        }
        private void _UpdateTotalQuestions()
        {
            int totalQuestions =
                (int)nudMultipleChoice.Value +
                (int)nudTrueFalse.Value;

            lblTotalQuestions.Text =
                totalQuestions.ToString();

            if (totalQuestions > 100)
            {
                lblTotalQuestions.ForeColor =
                    Color.Red;

                btnGenerate.Enabled =
                    false;
            }
            else
            {
                lblTotalQuestions.ForeColor =
                    Color.Black;

                btnGenerate.Enabled =
                    totalQuestions > 0 &&
                    cmbDocuments.Items.Count > 0;
            }
        }
        private void frmGenerateQuestions_Load(object sender, EventArgs e)
        {
            nudMultipleChoice.Maximum = 100;
            nudTrueFalse.Maximum = 100;

            _LoadReadyDocuments();
            _UpdateTotalQuestions();


        }

        private void nudMultipleChoice_ValueChanged(object sender, EventArgs e)
        {
            _UpdateTotalQuestions();

        }

        private void nudTrueFalse_ValueChanged(object sender, EventArgs e)
        {
            _UpdateTotalQuestions();

        }
        private async void btnGenerate_Click(object sender, EventArgs e)
        {
            {
                if (cmbDocuments.SelectedValue == null)
                {

                    MessageBox.Show(
                        _Translate(
                            "يرجى اختيار مستند قبل البدء بتوليد الأسئلة.",
                            "Please select a document first."),
                        _Translate("لم يتم اختيار مستند", "No Document Selected"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);


                    return;
                }

                int documentID =
                    Convert.ToInt32(cmbDocuments.SelectedValue);

                int multipleChoiceCount =
                    (int)nudMultipleChoice.Value;

                int trueFalseCount =
                    (int)nudTrueFalse.Value;

                int totalQuestions =
                    multipleChoiceCount +
                    trueFalseCount;
                var requestData = new
                {
                    documentID = documentID,
                    multipleChoiceCount = multipleChoiceCount,
                    trueFalseCount = trueFalseCount
                };

                string json =
                    JsonSerializer.Serialize(requestData);
                using (StringContent content =
                       new StringContent(
                           json,
                           Encoding.UTF8,
                           "application/json"))
                {
                    using (HttpClient client =
                           new HttpClient())
                    {
                        client.Timeout =
                            TimeSpan.FromSeconds(30);

                        HttpResponseMessage response =
                            await client.PostAsync(
                                "https://localhost:7077/api/question-bank/generate-from-document",
                                content);

                        string responseText =
                            await response.Content
                                .ReadAsStringAsync();

                        JsonSerializerOptions options =
           new JsonSerializerOptions
           {
               PropertyNameCaseInsensitive = true
           };

                        GenerateQuestionsResponse result =
                            JsonSerializer.Deserialize<GenerateQuestionsResponse>(
                                responseText,
                                options);

                        if (result == null ||
                            result.QuestionGenerationJobID <= 0)
                        {

                            MessageBox.Show(
                                _Translate(
                                    "تعذّر قراءة رقم مهمة توليد الأسئلة من استجابة الخادم.",
                                    "Could not read the question generation job ID."),
                                _Translate("خطأ في توليد الأسئلة", "Generation Error"),
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);


                            return;
                        }

                        _CurrentQuestionGenerationJobID =
                        result.QuestionGenerationJobID;

                        if (_frmMain != null)
                        {
                            _frmMain.TrackQuestionGenerationJob(
                                _CurrentQuestionGenerationJobID);
                        }


                        MessageBox.Show(
                            _Translate(
                                "تم بدء مهمة توليد الأسئلة بنجاح.\n\nرقم المهمة: ",
                                "Question generation started successfully.\n\nJob ID: ")
                            + _CurrentQuestionGenerationJobID
                            + _Translate(
                                "\n\nيمكنك متابعة استخدام البرنامج أثناء معالجة المهمة.",
                                "\n\nYou can continue using the application while the job is being processed."),
                            _Translate("بدأ توليد الأسئلة", "Generation Started"),
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);


                        this.Close();

                    }
                }
            }
        }

    }
}