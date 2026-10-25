using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DVLD.OfficialExam
{
    public partial class frmExamStart : Form
    {

        public frmExamStart()
        {
            InitializeComponent();

            LoadExamInstructions();
        }



        private void LoadExamInstructions()
        {
            lblInstructions.Text =
                "١. يتكوّن الامتحان من ٥ أسئلة، والمدة المحددة ١٥ دقيقة." +
                Environment.NewLine +
                "٢. يبدأ احتساب الوقت بمجرد بدء الامتحان." +
                Environment.NewLine +
                "٣. يمكنك التنقل بين الأسئلة وتعديل إجاباتك قبل انتهاء الوقت." +
                Environment.NewLine +
                "٤. تُحفظ كل إجابة تلقائيًا عند اختيارها. تأكد من نجاح الحفظ." +
                Environment.NewLine +
                "٥. يتفعّل زر تسليم الامتحان بعد حفظ إجابات جميع الأسئلة." +
                Environment.NewLine +
                "٦. عند انتهاء الوقت، يُسلَّم الامتحان تلقائيًا، وتظهر النتيجة مباشرة.";
        }


        private async void btnStartExam_Click(object sender, EventArgs e)
        {
            string nationalNo = txtNationalNo.Text.Trim();

            if (string.IsNullOrWhiteSpace(nationalNo))
            {
                MessageBox.Show(
                    "الرجاء إدخال الرقم الوطني.",
                    "تنبيه",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtNationalNo.Focus();
                return;
            }

            btnStartExam.Enabled = false;

            try
            {
                OfficialExamApiClient api =
                    new OfficialExamApiClient();

                // إرسال الرقم الوطني وطلب بدء الامتحان من السيرفر.
                StartExamResponse exam =
                    await api.StartExamAsync(nationalNo);

                // فتح شاشة الأسئلة باستخدام البيانات الراجعة من السيرفر.
                using (frmExamQuestions examForm =
                    new frmExamQuestions(exam))
                {
                    this.Hide();

                    examForm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "تعذر بدء الامتحان:\n" + ex.Message,
                    "خطأ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                // إظهار شاشة البداية بعد إغلاق شاشة الأسئلة.
                if (!this.IsDisposed)
                {
                    this.Show();
                    btnStartExam.Enabled = true;
                }
            }
        }
    }
}
