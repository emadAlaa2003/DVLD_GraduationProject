using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows.Forms;

namespace DVLD.Documents
{
    public partial class frmAddDecoument : Form
    {
        private string _SelectedFilePath = "";
        public frmAddDecoument()
        {
            InitializeComponent();
        }

        private void btnBrowser_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Select PDF Document";
                openFileDialog.Filter = "PDF Files (*.pdf)|*.pdf";
                openFileDialog.Multiselect = false;

                if (openFileDialog.ShowDialog() != DialogResult.OK)
                    return;

                FileInfo fileInfo = new FileInfo(openFileDialog.FileName);

                if (!fileInfo.Exists)
                {
                    MessageBox.Show(
                        "The selected file does not exist.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }

                if (fileInfo.Extension.ToLower() != ".pdf")
                {
                    MessageBox.Show(
                        "Please select a PDF file.",
                        "Invalid File",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                _SelectedFilePath = fileInfo.FullName;

                TxtFilePath.Text = fileInfo.FullName;
                txtFileName.Text = fileInfo.Name;

                txtFileSize.Text =
                    (fileInfo.Length / 1024.0 / 1024.0).ToString("F2")
                    + " MB";

                txtFileType.Text = "PDF Document";

                progressBar1.Value = 0;
                lblProgress.Text = "0%";

                btnUpload.Enabled = true;
            }
        }

        private async void btnUpload_Click(object sender, EventArgs e)
        {

            if (string.IsNullOrWhiteSpace(_SelectedFilePath) ||
                !File.Exists(_SelectedFilePath))
            {
                MessageBox.Show(
                    "Please select a PDF file first.",
                    "No File Selected",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }


            btnUpload.Enabled = false;
            btnBrowser.Enabled = false;

            progressBar1.Value = 0;
            lblProgress.Text = "0%";


            try
            {
                using (FileStream fileStream =
                       new FileStream(
                           _SelectedFilePath,
                           FileMode.Open,
                           FileAccess.Read,
                           FileShare.Read))
                {
                    using (HttpClient client =
                           new HttpClient())
                    {
                        client.Timeout =
                            TimeSpan.FromMinutes(5);


                        using (MultipartFormDataContent form =
                               new MultipartFormDataContent())
                        {
                            ProgressableStreamContent fileContent =
                                new ProgressableStreamContent(
                                    fileStream,
                                    (uploaded, total) =>
                                    {
                                        int percentage =
                                            total > 0
                                                ? (int)(
                                                    uploaded * 100L /
                                                    total)
                                                : 0;


                                        if (percentage > 100)
                                            percentage = 100;


                                        if (!IsDisposed &&
                                            IsHandleCreated)
                                        {
                                            BeginInvoke(
                                                new Action(() =>
                                                {
                                                    progressBar1.Value =
                                                        percentage;

                                                    lblProgress.Text =
                                                        percentage + "%";
                                                }));
                                        }
                                    });


                            fileContent.Headers.ContentType =
                                new MediaTypeHeaderValue(
                                    "application/pdf");


                            form.Add(
                                fileContent,
                                "file",
                                Path.GetFileName(
                                    _SelectedFilePath));


                            HttpResponseMessage response =
                                await client.PostAsync(
                                    "https://localhost:7077/api/knowledge-documents",
                                    form);


                            string responseText =
                                await response.Content
                                    .ReadAsStringAsync();


                            if (!response.IsSuccessStatusCode)
                            {
                                btnUpload.Enabled = true;

                                MessageBox.Show(
                                    "Upload failed.\n\n" +
                                    responseText,
                                    "Upload Failed",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);

                                return;
                            }


                            progressBar1.Value = 100;
                            lblProgress.Text = "100%";


                            MessageBox.Show(
                                "Document uploaded successfully.\n\n" +
                                "Processing will continue in the background.",
                                "Upload Complete",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);


                            btnUpload.Enabled = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                btnUpload.Enabled = true;

                MessageBox.Show(
                    "Could not upload the document.\n\n" +
                    ex.Message,
                    "Upload Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                btnBrowser.Enabled = true;
            }

        }
    }
}