using DVLD_Buisness;
using System;
using System.Windows.Forms;

namespace DVLD.Documents.Controls
{
    public partial class ctrlDocumentInfo : UserControl
    {
        private clsKnowledgeDocument _Document;

        private int _DocumentID = -1;


        public int DocumentID
        {
            get { return _DocumentID; }
        }


        public clsKnowledgeDocument SelectedDocumentInfo
        {
            get { return _Document; }
        }


        public ctrlDocumentInfo()
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


        private string _TranslateProcessingStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return _Translate("غير معروف", "Unknown");

            switch (status.Trim().ToLowerInvariant())
            {
                case "pending":
                    return _Translate("قيد الانتظار", "Pending");

                case "processing":
                    return _Translate("جارٍ المعالجة", "Processing");

                case "ready":
                    return _Translate("جاهز", "Ready");

                case "completed":
                    return _Translate("مكتمل", "Completed");

                case "failed":
                    return _Translate("فشلت المعالجة", "Failed");

                default:
                    return status;
            }
        }

        public void ResetDocumentInfo()
        {
            _DocumentID = -1;
            _Document = null;

            lblDocumentID.Text = "N/A";
            lblDocumentName.Text = "[????]";
            lblFileSize.Text = "[????]";
            lblUploadedAt.Text = "[????]";
            lblIsActive.Text = "[????]";

            lblProcessingStatus.Text = "[????]";
            lblTotalPages.Text = "[????]";
            lblChunkCount.Text = "[????]";
            lblProcessedAt.Text = "[????]";
            lblErrorMessage.Text = "[????]";
        }


        private void _FillDocumentInfo()
        {
            if (_Document == null)
                return;


            _DocumentID = _Document.DocumentID;


            lblDocumentID.Text =
                _Document.DocumentID.ToString();


            lblDocumentName.Text =
                _Document.OriginalFileName;


            lblFileSize.Text =
                (_Document.FileSizeBytes / 1024.0 / 1024.0)
                .ToString("F2") + " MB";


            lblUploadedAt.Text =
                _Document.UploadedAt
                .ToString("dd/MM/yyyy HH:mm");



            lblIsActive.Text =
                _Document.IsActive
                    ? _Translate("نعم", "Yes")
                    : _Translate("لا", "No");



            lblProcessingStatus.Text =
                _TranslateProcessingStatus(
                    _Document.ProcessingStatus);




            lblTotalPages.Text =
                _Document.TotalPages.HasValue
                    ? _Document.TotalPages.Value.ToString()
                    : _Translate("غير متوفر", "N/A");



            lblChunkCount.Text =
                _Document.ChunkCount.ToString();



            lblProcessedAt.Text =
                _Document.ProcessedAt.HasValue
                    ? _Document.ProcessedAt.Value
                        .ToString("dd/MM/yyyy HH:mm")
                    : _Translate("غير متوفر", "N/A");




            lblErrorMessage.Text =
                string.IsNullOrWhiteSpace(_Document.ErrorMessage)
                    ? _Translate(
                        "لا توجد أخطاء في المعالجة.",
                        "No processing errors.")
                    : _Document.ErrorMessage;

        }


        public void LoadDocumentInfo(
            int DocumentID)
        {
            _Document =
                clsKnowledgeDocument.Find(
                    DocumentID);


            if (_Document == null)
            {
                ResetDocumentInfo();


                MessageBox.Show(
                    _Translate(
                        "لم يتم العثور على المستند رقم: ",
                        "No document found with ID = ")
                        + DocumentID,
                    _Translate("خطأ", "Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }


            _FillDocumentInfo();
        }
    }
}