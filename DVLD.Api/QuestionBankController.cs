using DVLD.AI;
using DVLD_Buisness;
using Microsoft.AspNetCore.Mvc;

namespace DVLD.Api.Controllers
{
    [ApiController]
    [Route("api/question-bank")]
    public class QuestionBankController : ControllerBase
    {
        // =====================================================
        // توليد سؤال واحد من نص محدد
        // نحتفظ به للاختبار والاستخدام الداخلي
        // =====================================================

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateQuestion(
            [FromBody] GenerateQuestionRequest request)
        {
            if (request == null)
            {
                return BadRequest(new
                {
                    message = "Request is required."
                });
            }

            if (string.IsNullOrWhiteSpace(request.SourceText))
            {
                return BadRequest(new
                {
                    message = "Source text is required."
                });
            }

            if (!IsValidQuestionType(
                    request.QuestionType))
            {
                return BadRequest(new
                {
                    message =
                        "QuestionType must be MultipleChoice or TrueFalse."
                });
            }


            GeneratedQuestion generatedQuestion =
                await OllamaQuestionGeneratorService
                    .GenerateQuestionAsync(
                        request.SourceText,
                        request.QuestionType);


            PrepareAndValidateQuestion(
                generatedQuestion,
                request.QuestionType);


            int questionID =
                SaveQuestion(
                    generatedQuestion,
                    request.SourceDocumentID,
                    request.SourcePageNumber);


            return Ok(new
            {
                QuestionID =
                    questionID,

                QuestionText =
                    generatedQuestion.QuestionText,

                QuestionType =
                    generatedQuestion.QuestionType,

                OptionA =
                    generatedQuestion.OptionA,

                OptionB =
                    generatedQuestion.OptionB,

                OptionC =
                    generatedQuestion.OptionC,

                OptionD =
                    generatedQuestion.OptionD,

                CorrectOption =
                    generatedQuestion.CorrectOption,

                Explanation =
                    generatedQuestion.Explanation,

                SourceDocumentID =
                    request.SourceDocumentID,

                SourcePageNumber =
                    request.SourcePageNumber
            });
        }


        // =====================================================
        // توليد مجموعة أسئلة موزعة على كامل الكتاب
        // =====================================================

        [HttpPost("generate-from-document")]
        public async Task<IActionResult> GenerateFromDocument(
            [FromBody] GenerateQuestionsFromDocumentRequest request)
        {
            if (request == null)
            {
                return BadRequest(new
                {
                    message = "Request is required."
                });
            }


            if (request.DocumentID <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid document ID."
                });
            }


            if (request.MultipleChoiceCount < 0 ||
                request.TrueFalseCount < 0)
            {
                return BadRequest(new
                {
                    message =
                        "Question counts cannot be negative."
                });
            }


            int totalQuestions =
                request.MultipleChoiceCount +
                request.TrueFalseCount;


            if (totalQuestions <= 0)
            {
                return BadRequest(new
                {
                    message =
                        "At least one question must be requested."
                });
            }


            // حماية مؤقتة حتى لا يطلب المستخدم
            // عدداً ضخماً بالخطأ.
            if (totalQuestions > 100)
            {
                return BadRequest(new
                {
                    message =
                        "Maximum question count is 100 per request."
                });
            }


            // نجيب ملف الـ PDF المرتبط بالوثيقة
            string filePath =
                clsKnowledgeDocument
                    .GetFilePathByDocumentID(
                        request.DocumentID);


            if (string.IsNullOrWhiteSpace(filePath))
            {
                return NotFound(new
                {
                    message = "Document was not found."
                });
            }


            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(new
                {
                    message =
                        "The PDF file does not exist on disk."
                });
            }


            // استخراج الكتاب وتقسيمه بنفس
            // TextChunker المستخدم في الـ RAG.
            PdfExtractionResult extractionResult =
                PdfTextExtractor.Extract(
                    filePath);


            List<TextChunk> allChunks =
                TextChunker.CreateChunks(
                    extractionResult)
                    .Where(chunk =>
                        !string.IsNullOrWhiteSpace(
                            chunk.Text))
                    .ToList();


            if (allChunks.Count == 0)
            {
                return BadRequest(new
                {
                    message =
                        "No text chunks were found in the document."
                });
            }


            // اختيار مصادر الأسئلة أصبح مسؤولية
            // QuestionSourceSelector وليس الـ Controller.
            List<QuestionSourceCandidate> selectedSources;

            try
            {
                selectedSources =
                    QuestionSourceSelector
                        .SelectBestSources(
                            allChunks,
                            extractionResult.TotalPages,
                            totalQuestions);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message =
                        ex.Message
                });
            }


            // نوزع أنواع الأسئلة على العدد المطلوب.
            List<string> questionTypes =
                BuildQuestionTypes(
                    request.MultipleChoiceCount,
                    request.TrueFalseCount);


            List<PendingGeneratedQuestion> pendingQuestions =
                new List<PendingGeneratedQuestion>();


            for (int i = 0;
                 i < selectedSources.Count;
                 i++)
            {
                QuestionSourceCandidate sourceCandidate =
                    selectedSources[i];

                TextChunk sourceChunk =
                    sourceCandidate.Chunk;

                string questionType =
                    questionTypes[i];


                GeneratedQuestion generatedQuestion =
                    await GenerateValidQuestionWithRetry(
                        sourceChunk.Text,
                        questionType,
                        maxAttempts: 3);


                pendingQuestions.Add(
                    new PendingGeneratedQuestion
                    {
                        Question =
                            generatedQuestion,

                        SourcePageNumber =
                            sourceChunk.PageNumber,

                        SourceChunkIndex =
                            sourceChunk.ChunkIndex,

                        SourceQualityScore =
                            sourceCandidate.QualityScore
                    });
            }


            // لا نحفظ أي سؤال إلا بعد نجاح توليد
            // والتحقق من جميع الأسئلة المطلوبة.
            List<object> generatedQuestions =
                new List<object>();


            foreach (PendingGeneratedQuestion pending
                     in pendingQuestions)
            {
                int questionID =
                    SaveQuestion(
                        pending.Question,
                        request.DocumentID,
                        pending.SourcePageNumber);


                generatedQuestions.Add(new
                {
                    QuestionID =
                        questionID,

                    QuestionText =
                        pending.Question.QuestionText,

                    QuestionType =
                        pending.Question.QuestionType,

                    OptionA =
                        pending.Question.OptionA,

                    OptionB =
                        pending.Question.OptionB,

                    OptionC =
                        pending.Question.OptionC,

                    OptionD =
                        pending.Question.OptionD,

                    CorrectOption =
                        pending.Question.CorrectOption,

                    Explanation =
                        pending.Question.Explanation,

                    SourceDocumentID =
                        request.DocumentID,

                    SourcePageNumber =
                        pending.SourcePageNumber,

                    SourceChunkIndex =
                        pending.SourceChunkIndex,

                    SourceQualityScore =
                        pending.SourceQualityScore
                });
            }


            return Ok(new
            {
                DocumentID =
                    request.DocumentID,

                TotalPages =
                    extractionResult.TotalPages,

                TotalChunks =
                    allChunks.Count,

                SelectedSources =
                    selectedSources.Count,

                RequestedQuestions =
                    totalQuestions,

                MultipleChoiceCount =
                    request.MultipleChoiceCount,

                TrueFalseCount =
                    request.TrueFalseCount,

                GeneratedQuestions =
                    generatedQuestions.Count,

                Questions =
                    generatedQuestions
            });
        }


        // =====================================================
        // Helpers
        // =====================================================

        private static bool IsValidQuestionType(
            string questionType)
        {
            return
                questionType == "MultipleChoice" ||
                questionType == "TrueFalse";
        }


        private static void PrepareAndValidateQuestion(
            GeneratedQuestion question,
            string questionType)
        {
            if (question == null)
            {
                throw new InvalidOperationException(
                    "AI returned an empty question.");
            }


            if (string.IsNullOrWhiteSpace(
                    question.QuestionText))
            {
                throw new InvalidOperationException(
                    "AI returned an empty question text.");
            }


            // نرفض أي مخرجات أعاد فيها الـ AI تعليمات الـ Prompt
            // بدل إنشاء سؤال فعلي.
            if (question.QuestionText.Contains(
                    "نوع السؤال المطلوب",
                    StringComparison.OrdinalIgnoreCase) ||
                question.QuestionText.Contains(
                    "TrueFalse",
                    StringComparison.OrdinalIgnoreCase) ||
                question.QuestionText.Contains(
                    "MultipleChoice",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "AI returned instructions instead of a valid question.");
            }


            question.QuestionType =
                questionType;


            if (questionType == "TrueFalse")
            {
                question.OptionA = "صح";
                question.OptionB = "خطأ";
                question.OptionC = string.Empty;
                question.OptionD = string.Empty;


                if (question.CorrectOption != "A" &&
                    question.CorrectOption != "B")
                {
                    throw new InvalidOperationException(
                        "AI returned an invalid TrueFalse answer.");
                }

                return;
            }


            if (string.IsNullOrWhiteSpace(
                    question.OptionA) ||

                string.IsNullOrWhiteSpace(
                    question.OptionB) ||

                string.IsNullOrWhiteSpace(
                    question.OptionC) ||

                string.IsNullOrWhiteSpace(
                    question.OptionD))
            {
                throw new InvalidOperationException(
                    "AI returned incomplete answer choices.");
            }


            if (question.CorrectOption != "A" &&
                question.CorrectOption != "B" &&
                question.CorrectOption != "C" &&
                question.CorrectOption != "D")
            {
                throw new InvalidOperationException(
                    "AI returned an invalid correct option.");
            }
        }


        private static async Task<GeneratedQuestion>
            GenerateValidQuestionWithRetry(
                string sourceText,
                string questionType,
                int maxAttempts)
        {
            Exception lastException = null;


            for (int attempt = 1;
                 attempt <= maxAttempts;
                 attempt++)
            {
                try
                {
                    GeneratedQuestion question =
                        await OllamaQuestionGeneratorService
                            .GenerateQuestionAsync(
                                sourceText,
                                questionType);


                    PrepareAndValidateQuestion(
                        question,
                        questionType);


                    return question;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                }
            }


            throw new InvalidOperationException(
                $"AI could not generate a valid {questionType} " +
                $"question after {maxAttempts} attempts.",
                lastException);
        }


        private static int SaveQuestion(
            GeneratedQuestion question,
            int? sourceDocumentID,
            int? sourcePageNumber)
        {
            int questionID =
                clsQuestionBank.AddNewQuestion(
                    question.QuestionText,
                    question.QuestionType,
                    question.OptionA,
                    question.OptionB,
                    question.OptionC,
                    question.OptionD,
                    question.CorrectOption,
                    question.Explanation,
                    sourceDocumentID,
                    sourcePageNumber);


            if (questionID <= 0)
            {
                throw new InvalidOperationException(
                    "Question could not be saved.");
            }


            return questionID;
        }


        private static List<string>
            BuildQuestionTypes(
                int multipleChoiceCount,
                int trueFalseCount)
        {
            int total =
                multipleChoiceCount +
                trueFalseCount;


            List<string> types =
                new List<string>();


            int trueFalseAdded = 0;
            int multipleChoiceAdded = 0;


            for (int i = 0;
                 i < total;
                 i++)
            {
                int expectedTrueFalse =
                    ((i + 1) *
                     trueFalseCount) /
                    total;


                if (expectedTrueFalse >
                    trueFalseAdded)
                {
                    types.Add(
                        "TrueFalse");

                    trueFalseAdded++;
                }
                else
                {
                    types.Add(
                        "MultipleChoice");

                    multipleChoiceAdded++;
                }
            }


            return types;
        }
    }


    internal class PendingGeneratedQuestion
    {
        public GeneratedQuestion Question { get; set; }

        public int SourcePageNumber { get; set; }

        public int SourceChunkIndex { get; set; }

        public double SourceQualityScore { get; set; }
    }


    // =========================================================
    // Request لتوليد سؤال واحد من نص
    // =========================================================

    public class GenerateQuestionRequest
    {
        public string SourceText { get; set; } =
            string.Empty;

        public string QuestionType { get; set; } =
            "MultipleChoice";

        public int? SourceDocumentID { get; set; }

        public int? SourcePageNumber { get; set; }
    }


    // =========================================================
    // Request لتوليد مجموعة أسئلة من كامل الوثيقة
    // =========================================================

    public class GenerateQuestionsFromDocumentRequest
    {
        public int DocumentID { get; set; }

        public int MultipleChoiceCount { get; set; }

        public int TrueFalseCount { get; set; }
    }
}