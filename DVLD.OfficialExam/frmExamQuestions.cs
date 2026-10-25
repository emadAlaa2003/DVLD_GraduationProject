
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Threading.Tasks;
using System.Globalization;
namespace DVLD.OfficialExam
{
    public partial class frmExamQuestions : Form
    {
        // Exam data received from the API.
        private StartExamResponse _exam;

        // Index of the currently displayed question.
        private int _currentQuestionIndex = 0;

        // Store selected answers locally by question ID.
        private readonly Dictionary<long, string> _selectedAnswers =
            new Dictionary<long, string>();

        private readonly HashSet<long> _questionsSelectedByUser =
    new HashSet<long>();
        private readonly Timer _examTimer = new Timer();

        private DateTimeOffset _deadlineUtc;

        private bool _isSubmitting = false;
        private bool _examSubmitted = false;

        private bool _isDisplayingQuestion = false;

        private bool _isSavingAnswer = false;

        private int _pendingSaveRequests = 0;

        // الأسئلة التي أكد السيرفر حفظ إجاباتها بنجاح.
        private readonly HashSet<long> _confirmedSavedQuestionIDs =
            new HashSet<long>();
        // Default constructor required by the Designer.


        public frmExamQuestions()
        {
            InitializeComponent();
            this.ActiveControl = btnNext;
            // حفظ الإجابة فقط عند ضغط المستخدم بالماوس.
            rbOptionA.MouseClick +=
                (s, e) => AnswerOption_CheckedChanged(s, e);

            rbOptionB.MouseClick +=
                (s, e) => AnswerOption_CheckedChanged(s, e);

            rbOptionC.MouseClick +=
                (s, e) => AnswerOption_CheckedChanged(s, e);

            rbOptionD.MouseClick +=
                (s, e) => AnswerOption_CheckedChanged(s, e);
        }



        // Constructor used when an exam starts.

        internal frmExamQuestions(StartExamResponse exam) : this()
        {
            // التحقق من بيانات الامتحان القادمة من الـAPI.
            if (exam == null ||
                exam.questions == null ||
                exam.questions.Count == 0)
            {
                throw new ArgumentException(
                    "بيانات الامتحان غير مكتملة.");
            }

            _exam = exam;

            // قراءة وقت انتهاء الامتحان من السيرفر.
            DateTimeOffset parsedDeadline;

            if (!DateTimeOffset.TryParse(
                    exam.deadlineAtUtc,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal,
                    out parsedDeadline))
            {
                throw new ArgumentException(
                    "وقت انتهاء الامتحان غير صالح.");
            }

            _deadlineUtc = parsedDeadline.ToUniversalTime();

            // تشغيل مؤقت الامتحان.
            _examTimer.Interval = 1000;
            _examTimer.Tick += ExamTimer_Tick;
            _examTimer.Start();

            UpdateRemainingTime();

            // عرض السؤال الأول.
            ShowCurrentQuestion();

            // تنفيذ هذا الكود بعد ظهور الشاشة لأول مرة.
            this.Shown += (sender, e) =>
            {
                if (_examSubmitted ||
                    DateTimeOffset.UtcNow >= _deadlineUtc)
                {
                    return;
                }

                // نقل التركيز إلى زر بدل خيارات الإجابة.
                if (btnNext.Enabled)
                {
                    btnNext.Select();
                }
                else if (btnSubmitExam.Enabled)
                {
                    btnSubmitExam.Select();
                }

                // منع ظهور إجابة محددة تلقائيًا عند فتح الامتحان.
                _isDisplayingQuestion = true;

                try
                {
                    rbOptionA.Checked = false;
                    rbOptionB.Checked = false;
                    rbOptionC.Checked = false;
                    rbOptionD.Checked = false;
                }
                finally
                {
                    _isDisplayingQuestion = false;
                }
            };
        }


        // Display the current question and its options.


        private void ShowCurrentQuestion()
        {
            if (_exam == null)
                return;

            ExamQuestion question =
                _exam.questions[_currentQuestionIndex];

            // منع الحفظ الفوري أثناء تغيير السؤال برمجيًا.
            _isDisplayingQuestion = true;

            try
            {
                lblQuestionProgress.Text =
                    "السؤال " + (_currentQuestionIndex + 1) +
                    " من " + _exam.questions.Count;

                lblQuestionText.Text = question.questionText;

                // إلغاء تحديد إجابة السؤال السابق.
                rbOptionA.Checked = false;
                rbOptionB.Checked = false;
                rbOptionC.Checked = false;
                rbOptionD.Checked = false;

                // عرض خيارات السؤال الحالي.
                rbOptionA.Text = question.optionA;
                rbOptionB.Text = question.optionB;
                rbOptionC.Text = question.optionC;
                rbOptionD.Text = question.optionD;

                // إخفاء الخيارات غير الموجودة.
                rbOptionC.Visible =
                    !string.IsNullOrWhiteSpace(question.optionC);

                rbOptionD.Visible =
                    !string.IsNullOrWhiteSpace(question.optionD);

                // استرجاع الإجابة المختارة سابقًا، إن وُجدت.
                string savedOption;

                if (_selectedAnswers.TryGetValue(
                    question.examQuestionID, out savedOption))
                {
                    switch (savedOption)
                    {
                        case "A":
                            rbOptionA.Checked = true;
                            break;

                        case "B":
                            rbOptionB.Checked = true;
                            break;

                        case "C":
                            rbOptionC.Checked = true;
                            break;

                        case "D":
                            rbOptionD.Checked = true;
                            break;
                    }
                }
            }
            finally
            {
                _isDisplayingQuestion = false;
            }

            // السماح بالتنقل فقط إذا كان الامتحان ما زال مفتوحًا.
            bool canInteract =
                DateTimeOffset.UtcNow < _deadlineUtc &&
                !_isSubmitting &&
                !_examSubmitted &&
                !_isSavingAnswer &&
                _pendingSaveRequests == 0;

            btnPrevious.Enabled =
                canInteract && _currentQuestionIndex > 0;

            btnNext.Enabled =
                canInteract &&
                _currentQuestionIndex < _exam.questions.Count - 1;

            UpdateSubmitButtonState();
        }

        private void ExamTimer_Tick(object sender, EventArgs e)
        {
            UpdateRemainingTime();
        }

        private void UpdateRemainingTime()
        {
            TimeSpan remaining =
                _deadlineUtc - DateTimeOffset.UtcNow;



            if (remaining <= TimeSpan.Zero)
            {
                lblRemainingTime.Text = "الوقت المتبقي: 00:00";
                lblRemainingTime.ForeColor = Color.Red;

                // Prevent further changes after the deadline.
                grpAnswers.Enabled = false;
                btnNext.Enabled = false;
                btnPrevious.Enabled = false;
                btnSubmitExam.Enabled = false;

                // The exam has already been submitted.
                if (_examSubmitted)
                {
                    _examTimer.Stop();
                    return;
                }

                // Wait for any pending answer saves to finish.
                // Keep the timer running so it checks again.
                if (_pendingSaveRequests > 0)
                {
                    lblSaveStatus.Text =
                        "انتهى الوقت، بانتظار انتهاء حفظ الإجابة...";

                    lblSaveStatus.ForeColor = Color.DarkOrange;

                    return;
                }

                // A submission is already in progress.
                if (_isSubmitting)
                {
                    return;
                }

                // No pending saves: submit the expired exam.
                _examTimer.Stop();

                if (_exam != null)
                {
                    btnSubmitExam_Click(
                        btnSubmitExam,
                        EventArgs.Empty);
                }

                return;
            }

            int totalMinutes =
                (int)remaining.TotalMinutes;

            lblRemainingTime.Text =
                "الوقت المتبقي: " +
                totalMinutes.ToString("00") +
                ":" +
                remaining.Seconds.ToString("00");

            if (remaining.TotalMinutes <= 2)
            {
                lblRemainingTime.ForeColor = Color.Red;
            }
            else
            {
                lblRemainingTime.ForeColor = Color.DarkRed;
            }
        }

        // Save the currently selected answer locally.
        private void SaveCurrentAnswerLocally()
        {
            if (_exam == null)
                return;

            long questionID =
                _exam.questions[_currentQuestionIndex].examQuestionID;

            string selectedOption = null;


            // Identify the selected option.
            if (rbOptionA.Checked)
            {
                selectedOption = "A";
            }
            else if (rbOptionB.Checked)
            {
                selectedOption = "B";
            }
            else if (rbOptionC.Checked && rbOptionC.Visible)
            {
                selectedOption = "C";
            }
            else if (rbOptionD.Checked && rbOptionD.Visible)
            {
                selectedOption = "D";
            }


            // Save or clear the local answer.
            if (selectedOption == null)
            {
                _selectedAnswers.Remove(questionID);
            }
            else
            {
                _selectedAnswers[questionID] = selectedOption;
            }
        }



        private async Task<bool> SaveCurrentAnswerToApiAsync()
        {
            if (_exam == null)
                return false;


            long currentQuestionID =
                _exam.questions[_currentQuestionIndex].examQuestionID;

            // إذا المتقدّم ما اختار إجابة بنفسه، لا نحفظ أي خيار.
            if (!_questionsSelectedByUser.Contains(currentQuestionID))
            {
                _selectedAnswers.Remove(currentQuestionID);

                lblSaveStatus.Text = "لم يتم اختيار إجابة لهذا السؤال.";
                lblSaveStatus.ForeColor = Color.DarkSlateGray;

                // السماح بالتنقل حتى لو السؤال بدون إجابة.
                return true;
            }

            SaveCurrentAnswerLocally();

            long questionID =
                _exam.questions[_currentQuestionIndex].examQuestionID;

            string selectedOption;

            _selectedAnswers.TryGetValue(
                questionID, out selectedOption);


            if (string.IsNullOrWhiteSpace(selectedOption))
            {
                // لا نعتبر السؤال محفوظًا إذا لم تكن له إجابة محددة.
                _confirmedSavedQuestionIDs.Remove(questionID);

                lblSaveStatus.Text =
                    "لم يتم تحديد إجابة. اختر إجابة لحفظها.";

                lblSaveStatus.ForeColor = Color.DarkOrange;

                // لا ترسل طلب حفظ فارغًا إلى السيرفر.
                return false;
            }



            // إلغاء التأكيد السابق لحين نجاح حفظ الإجابة الحالية.
            _confirmedSavedQuestionIDs.Remove(questionID);

            // Track the pending save request.
            _pendingSaveRequests++;

            lblSaveStatus.Text = "جاري حفظ الإجابة...";
            lblSaveStatus.ForeColor = Color.DarkOrange;

            try
            {
                OfficialExamApiClient api =
                    new OfficialExamApiClient();

                await api.SaveAnswerAsync(
                    _exam.examAttemptID,
                    questionID,
                    selectedOption);

                // السيرفر أكد حفظ الإجابة بنجاح.
                _confirmedSavedQuestionIDs.Add(questionID);

                lblSaveStatus.Text = "تم حفظ الإجابة بنجاح";
                lblSaveStatus.ForeColor = Color.Green;

                return true;
            }

            catch (Exception ex)
            {
                // Keep technical details in Visual Studio's Output window.
                System.Diagnostics.Debug.WriteLine(ex);

                bool expired =
                    DateTimeOffset.UtcNow >= _deadlineUtc;

                if (expired)
                {
                    lblSaveStatus.Text =
                        "انتهى الوقت، لم يتم حفظ الإجابة الأخيرة.";
                }
                else
                {
                    lblSaveStatus.Text =
                        "فشل حفظ الإجابة. يرجى إعادة المحاولة.";
                }

                lblSaveStatus.ForeColor = Color.Red;

                return false;
            }
            finally
            {
                // The request has finished, successfully or not.
                _pendingSaveRequests--;
            }
        }


        private void UpdateSubmitButtonState()
        {
            // منع التسليم أثناء الحفظ أو أثناء تسليم سابق.
            if (_exam == null ||
                _examSubmitted ||
                _isSubmitting ||
                _isSavingAnswer ||
                _pendingSaveRequests > 0)
            {
                btnSubmitExam.Enabled = false;
                return;
            }

            // بعد انتهاء الوقت، نسمح بإعادة محاولة التسليم
            // إذا فشل التسليم التلقائي.
            if (DateTimeOffset.UtcNow >= _deadlineUtc)
            {
                btnSubmitExam.Enabled = true;
                return;
            }

            // التأكد أن جميع الأسئلة مجاوبة ومحفوظة في السيرفر.
            foreach (ExamQuestion question in _exam.questions)
            {
                string selectedOption;

                bool hasAnswer =
                    _selectedAnswers.TryGetValue(
                        question.examQuestionID,
                        out selectedOption) &&
                    !string.IsNullOrEmpty(selectedOption);

                bool savedSuccessfully =
                    _confirmedSavedQuestionIDs.Contains(
                        question.examQuestionID);

                if (!hasAnswer || !savedSuccessfully)
                {
                    btnSubmitExam.Enabled = false;
                    return;
                }
            }

            // جميع الأسئلة مجاوبة ومحفوظة بنجاح.
            btnSubmitExam.Enabled = true;
        }



        private async void AnswerOption_CheckedChanged(
            object sender, EventArgs e)
        {
            if (_exam == null ||
                _isDisplayingQuestion ||
                _isSavingAnswer ||
                _isSubmitting ||
                _examSubmitted)
            {
                return;
            }

            RadioButton selectedRadio = sender as RadioButton;

            // Ignore deselection events.
            if (selectedRadio == null || !selectedRadio.Checked)
                return;

            // Do not accept answers after the deadline.
            if (DateTimeOffset.UtcNow >= _deadlineUtc)
                return;

            // تسجيل أن المتقدّم اختار إجابة هذا السؤال بنفسه.
            long questionID =
                _exam.questions[_currentQuestionIndex].examQuestionID;

            _questionsSelectedByUser.Add(questionID);

            _isSavingAnswer = true;

            // Prevent changes while the server saves the answer.
            grpAnswers.Enabled = false;
            btnNext.Enabled = false;
            btnPrevious.Enabled = false;
            btnSubmitExam.Enabled = false;

            try
            {
                // Save the selected answer through the API.
                await SaveCurrentAnswerToApiAsync();
            }

            finally
            {
                _isSavingAnswer = false;

                bool expired =
                    DateTimeOffset.UtcNow >= _deadlineUtc;

                bool canInteract =
                    !expired &&
                    !_isSubmitting &&
                    !_examSubmitted &&
                    _pendingSaveRequests == 0;

                // Restore controls only while the exam is open.
                grpAnswers.Enabled = canInteract;

                btnPrevious.Enabled =
                    canInteract && _currentQuestionIndex > 0;

                btnNext.Enabled =
                    canInteract &&
                    _currentQuestionIndex < _exam.questions.Count - 1;

                UpdateSubmitButtonState();

                // If time expired during saving, trigger the
                // expiration handler after the save request finishes.
                if (expired &&
                    !_isSubmitting &&
                    !_examSubmitted &&
                    _pendingSaveRequests == 0)
                {
                    UpdateRemainingTime();
                }
            }
        }

        // Navigate to the next question.


        private async void btnNext_Click(object sender, EventArgs e)
        {
            if (_exam == null ||
                _isSubmitting ||
                _examSubmitted ||
                DateTimeOffset.UtcNow >= _deadlineUtc)
            {
                return;
            }

            if (_currentQuestionIndex >=
                _exam.questions.Count - 1)
            {
                return;
            }

            btnNext.Enabled = false;
            btnPrevious.Enabled = false;
            btnSubmitExam.Enabled = false;
            grpAnswers.Enabled = false;

            try
            {

                long questionID =
                    _exam.questions[_currentQuestionIndex].examQuestionID;

                // احفظ فقط إذا اختار المتقدّم إجابة
                // ولم يؤكد السيرفر حفظها بعد.
                bool needsSave =
                    _questionsSelectedByUser.Contains(questionID) &&
                    !_confirmedSavedQuestionIDs.Contains(questionID);

                if (needsSave)
                {
                    bool saved = await SaveCurrentAnswerToApiAsync();

                    if (!saved)
                        return;
                }


                // Check again after awaiting the API.
                if (DateTimeOffset.UtcNow >= _deadlineUtc ||
                    _isSubmitting ||
                    _examSubmitted)
                {
                    return;
                }

                _currentQuestionIndex++;

                ShowCurrentQuestion();

                lblSaveStatus.Text = "اختر إجابتك للسؤال الحالي";
                lblSaveStatus.ForeColor = Color.DarkSlateGray;
            }
            finally
            {
                bool canInteract =
                    DateTimeOffset.UtcNow < _deadlineUtc &&
                    !_isSubmitting &&
                    !_examSubmitted &&
                    _pendingSaveRequests == 0;

                grpAnswers.Enabled = canInteract;

                btnPrevious.Enabled =
                    canInteract && _currentQuestionIndex > 0;

                btnNext.Enabled =
                    canInteract &&
                    _currentQuestionIndex < _exam.questions.Count - 1;

                UpdateSubmitButtonState();
            }
        }




        private async void btnPrevious_Click(object sender, EventArgs e)
        {
            // Prevent navigation if the exam is unavailable,
            // expired, being submitted or saving an answer.
            if (_exam == null ||
                _isSubmitting ||
                _examSubmitted ||
                _isSavingAnswer ||
                _pendingSaveRequests > 0 ||
                DateTimeOffset.UtcNow >= _deadlineUtc)
            {
                return;
            }

            // Prevent moving before the first question.
            if (_currentQuestionIndex <= 0)
                return;

            // Disable controls while saving.
            btnNext.Enabled = false;
            btnPrevious.Enabled = false;
            btnSubmitExam.Enabled = false;
            grpAnswers.Enabled = false;

            try
            {

                long questionID =
                    _exam.questions[_currentQuestionIndex].examQuestionID;

                // هل اختار المتقدّم إجابة ولم يؤكد السيرفر حفظها؟
                bool needsSave =
                    _questionsSelectedByUser.Contains(questionID) &&
                    !_confirmedSavedQuestionIDs.Contains(questionID);

                if (needsSave)
                {
                    bool saved = await SaveCurrentAnswerToApiAsync();

                    // إذا فشل الحفظ، ابقَ على السؤال الحالي.
                    if (!saved)
                        return;
                }


                // Recheck the exam state after awaiting the API.
                if (DateTimeOffset.UtcNow >= _deadlineUtc ||
                    _isSubmitting ||
                    _examSubmitted)
                {
                    return;
                }

                // Move to the previous question.
                _currentQuestionIndex--;

                // Display the question and restore its answer.
                ShowCurrentQuestion();

                lblSaveStatus.Text =
                    "اختر إجابتك للسؤال الحالي";

                lblSaveStatus.ForeColor =
                    Color.DarkSlateGray;
            }
            finally
            {
                // Restore controls only if the exam is still open.
                bool canInteract =
                    DateTimeOffset.UtcNow < _deadlineUtc &&
                    !_isSubmitting &&
                    !_examSubmitted &&
                    !_isSavingAnswer &&
                    _pendingSaveRequests == 0;

                grpAnswers.Enabled = canInteract;

                btnPrevious.Enabled =
                    canInteract && _currentQuestionIndex > 0;

                btnNext.Enabled =
                    canInteract &&
                    _currentQuestionIndex < _exam.questions.Count - 1;

                UpdateSubmitButtonState();
            }
        }


        private async void btnSubmitExam_Click(object sender, EventArgs e)
        {

            if (_exam == null ||
                _isSubmitting ||
                _examSubmitted ||
                _isSavingAnswer ||
                _pendingSaveRequests > 0)
            {
                return;
            }


            // لا نسمح بالتسليم اليدوي قبل حفظ جميع الإجابات.
            // بعد انتهاء الوقت، نتجاوز هذا الشرط للسماح بالتسليم التلقائي.
            if (DateTimeOffset.UtcNow < _deadlineUtc)
            {
                foreach (ExamQuestion question in _exam.questions)
                {
                    string selectedOption;

                    bool hasAnswer =
                        _selectedAnswers.TryGetValue(
                            question.examQuestionID,
                            out selectedOption) &&
                        !string.IsNullOrWhiteSpace(selectedOption);

                    bool savedSuccessfully =
                        _confirmedSavedQuestionIDs.Contains(
                            question.examQuestionID);

                    if (!hasAnswer || !savedSuccessfully)
                    {
                        UpdateSubmitButtonState();

                        MessageBox.Show(
                            "لا يمكن تسليم الامتحان قبل الإجابة " +
                            "على جميع الأسئلة وحفظها بنجاح.",
                            "الامتحان غير مكتمل",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);

                        return;
                    }
                }
            }




            // Ask for confirmation while the exam is still open.
            if (DateTimeOffset.UtcNow < _deadlineUtc)
            {
                DialogResult confirmation = MessageBox.Show(
                    "هل أنت متأكد من تسليم الامتحان؟\n" +
                    "لن تتمكن من تعديل إجاباتك بعد التسليم.",
                    "تأكيد تسليم الامتحان",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);

                if (confirmation != DialogResult.Yes)
                    return;
            }


            // Recheck the exam state after the confirmation dialog.
            if (_isSubmitting ||
                _examSubmitted ||
                _isSavingAnswer ||
                _pendingSaveRequests > 0)
            {
                return;
            }

            _isSubmitting = true;

            btnSubmitExam.Enabled = false;
            btnNext.Enabled = false;
            btnPrevious.Enabled = false;
            grpAnswers.Enabled = false;

            try
            {
             
                OfficialExamApiClient api = new OfficialExamApiClient();

                SubmitExamResponse result =
                    await api.SubmitExamAsync(_exam.examAttemptID);

                _examSubmitted = true;
                _examTimer.Stop();

                string resultText = result.passed
                    ? "ناجح"
                    : "راسب";

                string statusText = result.status == "TimedOut"
                    ? "\nانتهى وقت الامتحان."
                    : "";

                MessageBox.Show(
                    "تم تسليم الامتحان.\n\n" +
                    "الإجابات الصحيحة: " + result.correctAnswers +
                    " من " + result.questionCount + "\n" +
                    "الحد الأدنى للنجاح: " +
                    result.requiredCorrectAnswers + "\n" +
                    "النتيجة: " + resultText +
                    statusText,
                    "نتيجة الامتحان",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }

            catch (Exception ex)
            {
                // تسجيل تفاصيل الخطأ في Visual Studio.
                System.Diagnostics.Debug.WriteLine(ex);

                bool expired =
                    DateTimeOffset.UtcNow >= _deadlineUtc;

                if (expired)
                {
                    // إيقاف المحاولات التلقائية المتكررة.
                    _examTimer.Stop();

                    // إبقاء الشاشة مفتوحة لإعادة المحاولة يدويًا.
                    lblSaveStatus.Text =
                        "تعذر تسليم الامتحان. تحقق من الاتصال، " +
                        "ثم اضغط زر تسليم الامتحان لإعادة المحاولة.";

                    lblSaveStatus.ForeColor = Color.Red;
                }
                else
                {
                    lblSaveStatus.Text =
                        "فشل تسليم الامتحان. حاول مرة أخرى.";

                    lblSaveStatus.ForeColor = Color.Red;

                    MessageBox.Show(
                        "تعذر تسليم الامتحان:\n" + ex.Message,
                        "خطأ",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            finally
            {
                _isSubmitting = false;

                // Re-enable available controls if submission failed.
                if (!_examSubmitted)
                {
                    bool expired =
                        DateTimeOffset.UtcNow >= _deadlineUtc;

                    grpAnswers.Enabled = !expired;

                    btnPrevious.Enabled =
                        !expired && _currentQuestionIndex > 0;

                    btnNext.Enabled =
                        !expired &&
                        _currentQuestionIndex < _exam.questions.Count - 1;

                    // Allow retrying submission if an error occurred.
                    UpdateSubmitButtonState();
                }
            }
        }
    }
}