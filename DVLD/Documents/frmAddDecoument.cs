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


        private string _Translate(string arabic, string english)
        {
            bool isArabic =
                System.Globalization.CultureInfo.CurrentUICulture
                .TwoLetterISOLanguageName == "ar";

            return isArabic ? arabic : english;
        }

        private void btnBrowser_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {

                openFileDialog.Title = _Translate(
                    "اختيار مستند PDF",
                    "Select PDF Document");

                openFileDialog.Filter = _Translate(
                    "ملفات PDF (*.pdf)|*.pdf",
                    "PDF Files (*.pdf)|*.pdf");

                openFileDialog.Multiselect = false;

                if (openFileDialog.ShowDialog() != DialogResult.OK)
                    return;

                FileInfo fileInfo = new FileInfo(openFileDialog.FileName);

                if (!fileInfo.Exists)
                {

                    MessageBox.Show(
                        _Translate(
                            "الملف المحدد غير موجود.",
                            "The selected file does not exist."),
                        _Translate("خطأ", "Error"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);


                    return;
                }

                if (fileInfo.Extension.ToLower() != ".pdf")
                {

                    MessageBox.Show(
                        _Translate(
                            "يرجى اختيار ملف بصيغة PDF.",
                            "Please select a PDF file."),
                        _Translate("ملف غير صالح", "Invalid File"),
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


                txtFileType.Text = _Translate(
                    "مستند PDF",
                    "PDF Document");


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
                    _Translate(
                        "يرجى اختيار ملف PDF قبل بدء الرفع.",
                        "Please select a PDF file first."),
                    _Translate("لم يتم اختيار ملف", "No File Selected"),
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
                                    _Translate(
                                        "فشل رفع المستند إلى الخادم.\n\nالتفاصيل:\n",
                                        "Upload failed.\n\nDetails:\n")
                                        + responseText,
                                    _Translate("فشل رفع المستند", "Upload Failed"),
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);


                                return;
                            }


                            progressBar1.Value = 100;
                            lblProgress.Text = "100%";



                            MessageBox.Show(
                                _Translate(
                                    "تم رفع المستند بنجاح.\n\nستستمر معالجة المستند في الخلفية.",
                                    "Document uploaded successfully.\n\nProcessing will continue in the background."),
                                _Translate("اكتمل رفع المستند", "Upload Complete"),
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
                    _Translate(
                        "تعذّر رفع المستند.\n\nالتفاصيل:\n",
                        "Could not upload the document.\n\nDetails:\n")
                        + ex.Message,
                    _Translate("خطأ أثناء الرفع", "Upload Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

            }
            finally
            {
                btnBrowser.Enabled = true;
            }

        }

        private void lblTitle_Click(object sender, EventArgs e)
        {

        }
    }
}