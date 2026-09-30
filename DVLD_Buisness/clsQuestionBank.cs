using System;
using System.Data;
using DVLD_DataAccess;

namespace DVLD_Buisness
{
    // Business Object لشاشة Add / Edit / Review.
    // الدوال static القديمة محفوظة بنفس تواقيعها حتى لا يتأثر الـ API والـ AI.
    public class clsQuestionBank
    {
        public enum enMode { AddNew = 0, Update = 1 }
        public enum enQuestionType { MultipleChoice = 1, TrueFalse = 2 }
        public enum enReviewStatus { Draft = 0, Approved = 1, Rejected = 2 }

        public enMode Mode { get; private set; }
        public int QuestionID { get; private set; }

        public string QuestionText { get; set; }
        public enQuestionType QuestionType { get; set; }
        public string OptionA { get; set; }
        public string OptionB { get; set; }
        public string OptionC { get; set; }
        public string OptionD { get; set; }
        public string CorrectOption { get; set; }
        public string Explanation { get; set; }

        // Metadata المصادر الأصلية لا تُغيّر عند تحرير محتوى السؤال.
        public int? SourceDocumentID { get; private set; }
        public int? SourcePageNumber { get; private set; }
        public string SourceEvidence { get; private set; }
        public int? SourceChunkIndex { get; private set; }

        public enReviewStatus ReviewStatus { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public bool IsActive { get; private set; }

        // إضافة سؤال يدوي (بدون مصدر PDF).
        public clsQuestionBank()
        {
            QuestionID = -1;
            QuestionText = string.Empty;
            QuestionType = enQuestionType.MultipleChoice;
            OptionA = string.Empty;
            OptionB = string.Empty;
            OptionC = string.Empty;
            OptionD = string.Empty;
            CorrectOption = string.Empty;
            Explanation = string.Empty;
            SourceDocumentID = null;
            SourcePageNumber = null;
            SourceEvidence = string.Empty;
            SourceChunkIndex = null;
            ReviewStatus = enReviewStatus.Draft;
            CreatedAt = DateTime.MinValue;
            IsActive = true;
            Mode = enMode.AddNew;
        }

        // يتم استدعاؤه فقط من Find للسؤال المحفوظ في قاعدة البيانات.
        private clsQuestionBank(
            int questionID,
            string questionText,
            enQuestionType questionType,
            string optionA,
            string optionB,
            string optionC,
            string optionD,
            string correctOption,
            string explanation,
            int? sourceDocumentID,
            int? sourcePageNumber,
            string sourceEvidence,
            int? sourceChunkIndex,
            enReviewStatus reviewStatus,
            DateTime createdAt,
            bool isActive)
        {
            QuestionID = questionID;
            QuestionText = questionText;
            QuestionType = questionType;
            OptionA = optionA;
            OptionB = optionB;
            OptionC = optionC;
            OptionD = optionD;
            CorrectOption = correctOption;
            Explanation = explanation;
            SourceDocumentID = sourceDocumentID;
            SourcePageNumber = sourcePageNumber;
            SourceEvidence = sourceEvidence;
            SourceChunkIndex = sourceChunkIndex;
            ReviewStatus = reviewStatus;
            CreatedAt = createdAt;
            IsActive = isActive;
            Mode = enMode.Update;
        }

        public static clsQuestionBank Find(int questionID)
        {
            string questionText = string.Empty;
            string questionType = string.Empty;
            string optionA = string.Empty;
            string optionB = string.Empty;
            string optionC = string.Empty;
            string optionD = string.Empty;
            string correctOption = string.Empty;
            string explanation = string.Empty;
            int? sourceDocumentID = null;
            int? sourcePageNumber = null;
            string sourceEvidence = string.Empty;
            int? sourceChunkIndex = null;
            string reviewStatus = string.Empty;
            DateTime createdAt = DateTime.MinValue;
            bool isActive = true;

            bool found = clsQuestionBankData.GetQuestionInfoByID(
                questionID,
                ref questionText,
                ref questionType,
                ref optionA,
                ref optionB,
                ref optionC,
                ref optionD,
                ref correctOption,
                ref explanation,
                ref sourceDocumentID,
                ref sourcePageNumber,
                ref sourceEvidence,
                ref sourceChunkIndex,
                ref reviewStatus,
                ref createdAt,
                ref isActive);

            if (!found)
                return null;

            enQuestionType parsedType;
            if (!Enum.TryParse(questionType, false, out parsedType) ||
                !Enum.IsDefined(typeof(enQuestionType), parsedType))
            {
                throw new InvalidOperationException(
                    "Unsupported QuestionType in database: " + questionType);
            }

            enReviewStatus parsedStatus;
            if (!Enum.TryParse(reviewStatus, false, out parsedStatus) ||
                !Enum.IsDefined(typeof(enReviewStatus), parsedStatus))
            {
                throw new InvalidOperationException(
                    "Unsupported ReviewStatus in database: " + reviewStatus);
            }

            return new clsQuestionBank(
                questionID,
                questionText,
                parsedType,
                optionA,
                optionB,
                optionC,
                optionD,
                correctOption,
                explanation,
                sourceDocumentID,
                sourcePageNumber,
                sourceEvidence,
                sourceChunkIndex,
                parsedStatus,
                createdAt,
                isActive);
        }

        private void ValidateAndNormalize()
        {
            QuestionText = (QuestionText ?? string.Empty).Trim();
            OptionA = (OptionA ?? string.Empty).Trim();
            OptionB = (OptionB ?? string.Empty).Trim();
            OptionC = (OptionC ?? string.Empty).Trim();
            OptionD = (OptionD ?? string.Empty).Trim();
            CorrectOption = (CorrectOption ?? string.Empty).Trim().ToUpperInvariant();
            Explanation = (Explanation ?? string.Empty).Trim();

            if (QuestionText.Length == 0 || QuestionText.Length > 1000)
                throw new InvalidOperationException("QuestionText must contain 1-1000 characters.");

            if (QuestionType != enQuestionType.MultipleChoice &&
                QuestionType != enQuestionType.TrueFalse)
                throw new InvalidOperationException("Unsupported QuestionType.");

            if (QuestionType == enQuestionType.TrueFalse)
            {
                // خيارات الصح والخطأ ثابتة، ولا نقبل إجابة من C/D.
                OptionA = "صح";
                OptionB = "خطأ";
                OptionC = string.Empty;
                OptionD = string.Empty;

                if (CorrectOption != "A" && CorrectOption != "B")
                    throw new InvalidOperationException("TrueFalse answer must be A or B.");
            }
            else
            {
                if (OptionA.Length == 0 || OptionB.Length == 0 ||
                    OptionC.Length == 0 || OptionD.Length == 0)
                    throw new InvalidOperationException("All four MultipleChoice options are required.");

                if (CorrectOption != "A" && CorrectOption != "B" &&
                    CorrectOption != "C" && CorrectOption != "D")
                    throw new InvalidOperationException("MultipleChoice answer must be A, B, C or D.");
            }

            if (OptionA.Length > 500 || OptionB.Length > 500 ||
                OptionC.Length > 500 || OptionD.Length > 500)
                throw new InvalidOperationException("Each option must be at most 500 characters.");

            if (Explanation.Length > 2000)
                throw new InvalidOperationException("Explanation must be at most 2000 characters.");
        }

        private bool _AddNewQuestion()
        {
            // الأسئلة اليدوية تبدأ Draft وبدون دليل من PDF.
            int newID = clsQuestionBankData.AddNewQuestion(
                QuestionText,
                QuestionType.ToString(),
                OptionA,
                OptionB,
                OptionC,
                OptionD,
                CorrectOption,
                Explanation,
                sourceDocumentID: null,
                sourcePageNumber: null,
                sourceEvidence: null,
                sourceChunkIndex: null,
                reviewStatus: "Draft");

            if (newID <= 0)
                return false;

            QuestionID = newID;
            Mode = enMode.Update;
            ReviewStatus = enReviewStatus.Draft;

            // خذ تاريخ الإنشاء وحالة التفعيل الفعليين من قاعدة البيانات.
            clsQuestionBank saved = Find(QuestionID);
            if (saved != null)
            {
                CreatedAt = saved.CreatedAt;
                IsActive = saved.IsActive;
            }

            return true;
        }

        private bool _UpdateQuestion()
        {
            if (QuestionID <= 0)
                return false;

            bool updated = clsQuestionBankData.UpdateQuestion(
                QuestionID,
                QuestionText,
                QuestionType.ToString(),
                OptionA,
                OptionB,
                OptionC,
                OptionD,
                CorrectOption,
                Explanation);

            // لا يجوز بقاء السؤال Approved بعد تغيير نصه أو إجابته.
            if (updated)
                ReviewStatus = enReviewStatus.Draft;

            return updated;
        }

        public bool Save()
        {
            ValidateAndNormalize();

            switch (Mode)
            {
                case enMode.AddNew:
                    return _AddNewQuestion();
                case enMode.Update:
                    return _UpdateQuestion();
                default:
                    return false;
            }
        }

        public bool Approve()
        {
            return ChangeReviewStatus(enReviewStatus.Approved);
        }

        public bool Reject()
        {
            return ChangeReviewStatus(enReviewStatus.Rejected);
        }

        public bool Activate()
        {
            return _SetActive(true);
        }

        public bool Deactivate()
        {
            return _SetActive(false);
        }

        private bool _SetActive(bool isActive)
        {
            // لازم يكون السؤال موجودًا في قاعدة البيانات
            if (Mode != enMode.Update || QuestionID <= 0)
                return false;

            // إذا كانت الحالة المطلوبة موجودة أصلًا
            if (IsActive == isActive)
                return true;

            bool updated = clsQuestionBankData.SetQuestionActive(
                QuestionID, isActive);

            if (updated)
                IsActive = isActive;

            return updated;
        }

        private bool ChangeReviewStatus(enReviewStatus newStatus)
        {
            if (Mode != enMode.Update || QuestionID <= 0 || !IsActive)
                return false;

            bool changed = clsQuestionBankData.UpdateReviewStatus(
                QuestionID, newStatus.ToString());

            if (changed)
                ReviewStatus = newStatus;

            return changed;
        }

        // =====================================================
        // واجهة التوافق القديمة: يُستخدم توقيعها حاليًا من الـ API.
        // إبقاء هذه الدوال static مهم لعدم التأثير في AI generation.
        // =====================================================

        public static int AddNewQuestion(
            string questionText,
            string questionType,
            string optionA,
            string optionB,
            string optionC,
            string optionD,
            string correctOption,
            string explanation,
            int? sourceDocumentID,
            int? sourcePageNumber,
            string sourceEvidence = null,
            int? sourceChunkIndex = null,
            string reviewStatus = "Draft")
        {
            return clsQuestionBankData.AddNewQuestion(
                questionText,
                questionType,
                optionA,
                optionB,
                optionC,
                optionD,
                correctOption,
                explanation,
                sourceDocumentID,
                sourcePageNumber,
                sourceEvidence,
                sourceChunkIndex,
                reviewStatus);
        }

        public static bool IsDuplicateQuestion(
            string questionText,
            int? sourceDocumentID,
            string sourceEvidence)
        {
            return clsQuestionBankData.IsDuplicateQuestion(
                questionText,
                sourceDocumentID,
                sourceEvidence);
        }

        public static bool UpdateReviewStatus(
            int questionID,
            string reviewStatus)
        {
            return clsQuestionBankData.UpdateReviewStatus(
                questionID,
                reviewStatus);
        }

        public static DataTable GetAllQuestions()
        {
            return clsQuestionBankData.GetAllQuestions();
        }
        public bool Delete()
        {
            // لا يمكن حذف سؤال غير محفوظ.
            if (Mode != enMode.Update || QuestionID <= 0)
                return false;

            bool deleted = clsQuestionBankData.DeleteQuestion(QuestionID);

            if (deleted)
            {
                QuestionID = -1;
                Mode = enMode.AddNew;
            }

            return deleted;
        } 
    }
}
