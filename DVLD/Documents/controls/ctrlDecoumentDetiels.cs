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
                    ? "Yes"
                    : "No";


            lblProcessingStatus.Text =
                _Document.ProcessingStatus;


            lblTotalPages.Text =
                _Document.TotalPages.HasValue
                    ? _Document.TotalPages.Value.ToString()
                    : "N/A";


            lblChunkCount.Text =
                _Document.ChunkCount.ToString();


            lblProcessedAt.Text =
                _Document.ProcessedAt.HasValue
                    ? _Document.ProcessedAt.Value
                        .ToString("dd/MM/yyyy HH:mm")
                    : "N/A";


            lblErrorMessage.Text =
                string.IsNullOrWhiteSpace(
                    _Document.ErrorMessage)
                    ? "No processing errors."
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
                    "No document with DocumentID = " +
                    DocumentID.ToString(),
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }


            _FillDocumentInfo();
        }
    }
}