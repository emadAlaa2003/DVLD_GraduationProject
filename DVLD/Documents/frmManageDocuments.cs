using DVLD_Buisness;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static DVLD_Buisness.clsQuestionBank;

namespace DVLD.Documents
{
    public partial class frmManageDocuments : Form
    {
        public frmManageDocuments()
        {
            InitializeComponent();
        }
        private DataTable _dtAllDocuments = new DataTable();
        private DataTable _dtDocuments = new DataTable();


        private void _RefreshDocumentsList()
        {
            try
            {
                _dtAllDocuments =
                    clsKnowledgeDocument.GetAllDocuments();

                _dtDocuments = _dtAllDocuments.DefaultView.ToTable(
                    false,
                    "DocumentID",
                    "OriginalFileName",
                    "FileSizeBytes",
                    "ProcessingStatus",
                    "UploadedAt",
                    "IsActive"
                );

                dgvDecuments.DataSource = _dtDocuments;

                lblRecordsCount.Text =
                    _dtDocuments.DefaultView.Count.ToString();
            }
            catch (Exception ex)
            {
                _dtAllDocuments = new DataTable();
                _dtDocuments = new DataTable();

                dgvDecuments.DataSource = null;
                lblRecordsCount.Text = "0";

                MessageBox.Show(
                    "Could not load document data.\n\n" + ex.Message,
                    "Database Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void frmManageDocuments_Load(object sender, EventArgs e)
        {
            _RefreshDocumentsList();
            if (dgvDecuments.Columns.Count == 0)
                return;

            dgvDecuments.Columns["DocumentID"].HeaderText = "Document ID";
            dgvDecuments.Columns["DocumentID"].Width = 110;

            dgvDecuments.Columns["OriginalFileName"].HeaderText = "File Name";
            dgvDecuments.Columns["OriginalFileName"].Width = 260;

            dgvDecuments.Columns["FileSizeBytes"].HeaderText = "File Size (Bytes)";
            dgvDecuments.Columns["FileSizeBytes"].Width = 140;

            dgvDecuments.Columns["ProcessingStatus"].HeaderText = "Processing Status";
            dgvDecuments.Columns["ProcessingStatus"].Width = 150;


            dgvDecuments.Columns["UploadedAt"].HeaderText = "Uploaded At";
            dgvDecuments.Columns["UploadedAt"].Width = 160;



            dgvDecuments.Columns["IsActive"].HeaderText = "Is Active";
            dgvDecuments.Columns["IsActive"].Width = 100;

            dgvDecuments.Columns["UploadedAt"].DefaultCellStyle.Format =
                "dd/MM/yyyy HH:mm";


        }

        private void cbFilterBy_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_dtDocuments.Columns.Count > 0)
            {
                _dtDocuments.DefaultView.RowFilter = "";
                lblRecordsCount.Text =
                    _dtDocuments.DefaultView.Count.ToString();
            }
            if (cbFilterBy.Text == "Is Active")
            {
                txtFilterValue.Visible = false;
                cbProcessingStatus.Visible = false;
                cbIsActive.Visible = true;
                cbIsActive.Focus();
                cbIsActive.SelectedIndex = 0;
            }
            else if (cbFilterBy.Text == "Processing Status")
            {
                txtFilterValue.Visible = false;
                cbIsActive.Visible = false;
                cbProcessingStatus.Visible = true;
                cbProcessingStatus.Focus();
                cbProcessingStatus.SelectedIndex = 0;
            }
            else

            {

                txtFilterValue.Visible = (cbFilterBy.Text != "None");
                cbIsActive.Visible = false;
                cbProcessingStatus.Visible = false;
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
            if (_dtDocuments == null || _dtDocuments.Columns.Count == 0)
                return;

            switch (cbIsActive.Text)
            {
                case "Yes":
                    _dtDocuments.DefaultView.RowFilter = "[IsActive] = True";
                    break;

                case "No":
                    _dtDocuments.DefaultView.RowFilter = "[IsActive] = False";
                    break;

                default:
                    _dtDocuments.DefaultView.RowFilter = "";
                    break;
            }

            lblRecordsCount.Text =
                _dtDocuments.DefaultView.Count.ToString();
        }

        private void cbProcessingStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_dtDocuments == null || _dtDocuments.Columns.Count == 0)
                return;

            string filterValue = cbProcessingStatus.Text;

            if (filterValue == "All" || string.IsNullOrWhiteSpace(filterValue))
            {
                _dtDocuments.DefaultView.RowFilter = "";
            }
            else
            {
                _dtDocuments.DefaultView.RowFilter =
     $"[ProcessingStatus] = '{filterValue}'";
            }

            lblRecordsCount.Text =
                _dtDocuments.DefaultView.Count.ToString();
        }

        private void txtFilterValue_TextChanged(object sender, EventArgs e)
        {


            if (_dtDocuments == null ||
                _dtDocuments.Columns.Count == 0)
                return;

            string search = txtFilterValue.Text.Trim();
            string filterBy = cbFilterBy.Text;

            if (search == "" || filterBy == "None")
            {
                _dtDocuments.DefaultView.RowFilter = "";
            }
            else if (filterBy == "Document ID")
            {
                int documentID;

                if (int.TryParse(search, out documentID))
                {
                    _dtDocuments.DefaultView.RowFilter =
                        "[DocumentID] = " + documentID;
                }
                else
                {
                    _dtDocuments.DefaultView.RowFilter = "1 = 0";
                }
            }
            else if (filterBy == "File Name")
            {
                // حماية البحث من الرموز الخاصة في RowFilter
                string safeSearch =
                    System.Text.RegularExpressions.Regex.Replace(
                        search.Replace("'", "''"),
                        @"[\[\]%*]",
                        m => m.Value == "[" ? "[[]" :
                             m.Value == "]" ? "[]]" :
                             "[" + m.Value + "]");

                _dtDocuments.DefaultView.RowFilter =
                    "[OriginalFileName] LIKE '%" + safeSearch + "%'";
            }

            lblRecordsCount.Text =
                _dtDocuments.DefaultView.Count.ToString();


        }

        private void btnAddPerson_Click(object sender, EventArgs e)
        {
            frmAddDecoument frm = new frmAddDecoument();
            frm.ShowDialog();
            _RefreshDocumentsList();
        }


        private void showDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dgvDecuments.CurrentRow == null)
                return;

            int documentID =
                Convert.ToInt32(
                    dgvDecuments.CurrentRow.Cells["DocumentID"].Value);

            using (frmShowDecoumentInfo frm =
                   new frmShowDecoumentInfo(documentID))
            {
                frm.ShowDialog();
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (dgvDecuments.CurrentRow == null)
                return;

            int documentID =
                Convert.ToInt32(
                    dgvDecuments.CurrentRow.Cells["DocumentID"].Value);

            clsKnowledgeDocument document =
                clsKnowledgeDocument.Find(documentID);

            if (document == null)
            {
                MessageBox.Show(
                    "Document was not found.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            if (document.Activate())
            {
                MessageBox.Show(
                    "Document activated successfully.",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                _RefreshDocumentsList();
            }
            else
            {
                MessageBox.Show(
                    "Could not activate the document.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void deactivateDocumentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dgvDecuments.CurrentRow == null)
                return;

            int documentID =
                Convert.ToInt32(
                    dgvDecuments.CurrentRow.Cells["DocumentID"].Value);

            clsKnowledgeDocument document =
                clsKnowledgeDocument.Find(documentID);

            if (document == null)
            {
                MessageBox.Show(
                    "Document was not found.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            if (document.Deactivate())
            {
                MessageBox.Show(
                    "Document deactivated successfully.",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                _RefreshDocumentsList();
            }
            else
            {
                MessageBox.Show(
                    "Could not deactivate the document.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private async void reprocessDocumentToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (dgvDecuments.CurrentRow == null)
                return;

            int documentID =
                Convert.ToInt32(
                    dgvDecuments.CurrentRow
                        .Cells["DocumentID"].Value);

            bool isActive =
                Convert.ToBoolean(
                    dgvDecuments.CurrentRow
                        .Cells["IsActive"].Value);

            if (!isActive)
            {
                MessageBox.Show(
                    "The document must be active before reprocessing.",
                    "Reprocess Document",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            DialogResult result =
                MessageBox.Show(
                    "Are you sure you want to reprocess this document?",
                    "Confirm Reprocess",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            try
            {
                Cursor = Cursors.WaitCursor;

                using (HttpClient client = new HttpClient())
                {
                    client.Timeout =
                        TimeSpan.FromMinutes(30);

                    HttpResponseMessage response =
                        await client.PostAsync(
                            $"https://localhost:7077/api/knowledge-documents/{documentID}/reprocess",
                            null);

                    string responseText =
                        await response.Content
                            .ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        MessageBox.Show(
                            "Reprocess failed.\n\n" +
                            responseText,
                            "Reprocess Failed",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);

                        return;
                    }

                    MessageBox.Show(
                        "Document reprocessed successfully.",
                        "Reprocess Complete",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }

                _RefreshDocumentsList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not reprocess the document.\n\n" +
                    ex.Message,
                    "Reprocess Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }
    }

}
