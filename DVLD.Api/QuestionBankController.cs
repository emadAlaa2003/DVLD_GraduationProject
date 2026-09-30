using DVLD.AI;
using DVLD_Buisness;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DVLD.Api.Controllers
{
    [ApiController]
    [Route("api/question-bank")]
    public class QuestionBankController : ControllerBase
    {
        // نحافظ على الجودة بدون جعل المستخدم ينتظر عشرات
        // استدعاءات Ollama. لكل سؤال: مصدر أساسي + مصدر بديل واحد.
        private const int MaxSourceAttemptsPerQuestion = 3;


        // =====================================================
        // توليد سؤال واحد من نص محدد
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


            if (!IsValidQuestionType(request.QuestionType))
            {
                return BadRequest(new
                {
                    message =
                        "QuestionType must be MultipleChoice or TrueFalse."
                });
            }


            EvidenceBackedGeneratedQuestion validatedQuestion;

            try
            {
                validatedQuestion =
                    await GenerateEvidenceBackedQuestion(
                        request.SourceText,
                        request.QuestionType);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }


            int questionID =
                SaveQuestion(
                    validatedQuestion.Question,
                    request.SourceDocumentID,
                    request.SourcePageNumber,
                    sourceChunkIndex: null);


            return Ok(new
            {
                QuestionID =
                    questionID,

                QuestionText =
                    validatedQuestion.Question.QuestionText,

                QuestionType =
                    validatedQuestion.Question.QuestionType,

                OptionA =
                    validatedQuestion.Question.OptionA,

                OptionB =
                    validatedQuestion.Question.OptionB,

                OptionC =
                    validatedQuestion.Question.OptionC,

                OptionD =
                    validatedQuestion.Question.OptionD,

                CorrectOption =
                    validatedQuestion.Question.CorrectOption,

                Explanation =
                    validatedQuestion.Question.Explanation,

                SourceEvidence =
                    validatedQuestion.Question.SourceEvidence,

                EvidenceValidated =
                    validatedQuestion.EvidenceValidation.IsValid,

                ReviewStatus =
                    "Draft",

                SourceDocumentID =
                    request.SourceDocumentID,

                SourcePageNumber =
                    request.SourcePageNumber
            });
        }


        // =====================================================
        // مراجعة السؤال: Approved / Rejected
        // =====================================================

        [HttpPatch("{questionID:int}/review-status")]
        public IActionResult UpdateReviewStatus(
            int questionID,
            [FromBody] UpdateQuestionReviewStatusRequest request)
        {
            if (questionID <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid question ID."
                });
            }


            if (request == null)
            {
                return BadRequest(new
                {
                    message = "Request is required."
                });
            }


            if (request.ReviewStatus != "Approved" &&
                request.ReviewStatus != "Rejected")
            {
                return BadRequest(new
                {
                    message =
                        "ReviewStatus must be Approved or Rejected."
                });
            }


            bool updated =
                clsQuestionBank.UpdateReviewStatus(
                    questionID,
                    request.ReviewStatus);


            if (!updated)
            {
                return NotFound(new
                {
                    message =
                        "Question was not found."
                });
            }


            return Ok(new
            {
                QuestionID =
                    questionID,

                ReviewStatus =
                    request.ReviewStatus,

                message =
                    "Question review status updated successfully."
            });
        }


        // =====================================================
        // توليد مجموعة أسئلة من كتاب كامل
        // =====================================================

        [HttpPost("generate-from-document")]
        public async Task<IActionResult> GenerateFromDocument(
            [FromBody] GenerateQuestionsFromDocumentRequest request)
        {
            Stopwatch totalStopwatch =
                Stopwatch.StartNew();

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


            if (totalQuestions > 100)
            {
                return BadRequest(new
                {
                    message =
                        "Maximum question count is 100 per request."
                });
            }


            string filePath =
                clsKnowledgeDocument
                    .GetFilePathByDocumentID(
                        request.DocumentID);


            if (string.IsNullOrWhiteSpace(filePath))
            {
                return NotFound(new
                {
                    message =
                        "Document was not found."
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


            Stopwatch qdrantStopwatch =
                Stopwatch.StartNew();


            List<TextChunk> allChunks =
                await QdrantKnowledgeStore
                    .GetDocumentChunksAsync(
                        request.DocumentID);


            qdrantStopwatch.Stop();


            int totalPages =
                allChunks.Count > 0
                    ? allChunks.Max(chunk =>
                        chunk.PageNumber)
                    : 0;


            if (allChunks.Count == 0)
            {
                return BadRequest(new
                {
                    message =
                        "No text chunks were found in the document."
                });
            }


            Stopwatch sourceSelectionStopwatch =
                Stopwatch.StartNew();


            List<QuestionSourceCandidate> selectedSources;

            try
            {
                selectedSources =
                    QuestionSourceSelector
                        .SelectBestSources(
                            allChunks,
                            totalPages,
                            totalQuestions);
            }
            catch (InvalidOperationException ex)
            {
                sourceSelectionStopwatch.Stop();
                totalStopwatch.Stop();

                return BadRequest(new
                {
                    message =
                        ex.Message,

                    performance = new
                    {
                        QdrantLoadMs =
                            qdrantStopwatch.ElapsedMilliseconds,

                        SourceSelectionMs =
                            sourceSelectionStopwatch.ElapsedMilliseconds,

                        TotalMs =
                            totalStopwatch.ElapsedMilliseconds
                    }
                });
            }


            sourceSelectionStopwatch.Stop();


            // Pool احتياطي للمصدر البديل فقط.
            List<QuestionSourceCandidate> fallbackSources =
                allChunks
                    .Select(chunk =>
                        QuestionSourceSelector
                            .EvaluateChunk(chunk))
                    .Where(candidate =>
                        candidate.IsSuitable)
                    .OrderByDescending(candidate =>
                        candidate.ExamValueScore)
                    .ThenByDescending(candidate =>
                        candidate.QualityScore)
                    .ToList();


            List<string> questionTypes =
                BuildQuestionTypes(
                    request.MultipleChoiceCount,
                    request.TrueFalseCount);


            List<PendingGeneratedQuestion> pendingQuestions =
                new List<PendingGeneratedQuestion>();


            HashSet<int> usedChunkIndexes =
                new HashSet<int>();


            int rejectedSourceAttempts =
                0;


            List<object> generationAttemptTimings =
                new List<object>();


            List<object> failedQuestions =
                new List<object>();


            for (int i = 0;
                 i < totalQuestions;
                 i++)
            {
                string questionType =
                    questionTypes[i];


                QuestionSourceCandidate primarySource =
                    selectedSources[i];


                List<QuestionSourceCandidate> sourcesForQuestion =
                    BuildSourceAttempts(
                        primarySource,
                        fallbackSources,
                        usedChunkIndexes,
                        MaxSourceAttemptsPerQuestion);


                PendingGeneratedQuestion acceptedQuestion =
                    null;


                Exception lastException =
                    null;


                foreach (QuestionSourceCandidate sourceCandidate
                         in sourcesForQuestion)
                {
                    TextChunk sourceChunk =
                        sourceCandidate.Chunk;


                    usedChunkIndexes.Add(
                        sourceChunk.ChunkIndex);


                    Stopwatch generationAttemptStopwatch =
                        Stopwatch.StartNew();


                    try
                    {
                        EvidenceBackedGeneratedQuestion validatedQuestion =
                            await GenerateEvidenceBackedQuestion(
                                sourceChunk.Text,
                                questionType);


                        bool isDuplicate =
                            clsQuestionBank.IsDuplicateQuestion(
                                validatedQuestion.Question.QuestionText,
                                request.DocumentID,
                                validatedQuestion.Question.SourceEvidence);


                        if (isDuplicate)
                        {
                            throw new InvalidOperationException(
                                "Generated question duplicates an existing active question from the same document.");
                        }


                        generationAttemptStopwatch.Stop();


                        generationAttemptTimings.Add(
                            new
                            {
                                QuestionNumber =
                                    i + 1,

                                QuestionType =
                                    questionType,

                                SourceChunkIndex =
                                    sourceChunk.ChunkIndex,

                                SourcePageNumber =
                                    sourceChunk.PageNumber,

                                Success =
                                    true,

                                ElapsedMs =
                                    generationAttemptStopwatch.ElapsedMilliseconds,

                                Error =
                                    (string)null
                            });


                        acceptedQuestion =
                            new PendingGeneratedQuestion
                            {
                                Question =
                                    validatedQuestion.Question,

                                EvidenceValidation =
                                    validatedQuestion.EvidenceValidation,

                                SourcePageNumber =
                                    sourceChunk.PageNumber,

                                SourceChunkIndex =
                                    sourceChunk.ChunkIndex,

                                SourceQualityScore =
                                    sourceCandidate.QualityScore
                            };


                        break;
                    }
                    catch (Exception ex)
                    {
                        generationAttemptStopwatch.Stop();


                        generationAttemptTimings.Add(
                            new
                            {
                                QuestionNumber =
                                    i + 1,

                                QuestionType =
                                    questionType,

                                SourceChunkIndex =
                                    sourceChunk.ChunkIndex,

                                SourcePageNumber =
                                    sourceChunk.PageNumber,

                                Success =
                                    false,

                                ElapsedMs =
                                    generationAttemptStopwatch.ElapsedMilliseconds,

                                Error =
                                    ex.Message
                            });


                        lastException =
                            ex;

                        rejectedSourceAttempts++;
                    }
                }


                if (acceptedQuestion == null)
                {
                    failedQuestions.Add(new
                    {
                        QuestionNumber =
                            i + 1,

                        QuestionType =
                            questionType,

                        Attempts =
                            sourcesForQuestion.Count,

                        LastError =
                            lastException?.Message
                    });


                    // فشل سؤال واحد لا يوقف بقية الدفعة.
                    // نكمل للأسئلة التالية، ونرجع ملخصاً كاملاً في النهاية.
                    continue;
                }


                pendingQuestions.Add(
                    acceptedQuestion);
            }


            if (pendingQuestions.Count == 0)
            {
                totalStopwatch.Stop();

                return BadRequest(new
                {
                    message =
                        "No questions could be generated from the requested batch.",

                    RequestedQuestions =
                        totalQuestions,

                    GeneratedQuestions =
                        0,

                    FailedQuestions =
                        failedQuestions.Count,

                    RejectedSourceAttempts =
                        rejectedSourceAttempts,

                    QuestionFailures =
                        failedQuestions,

                    Performance = new
                    {
                        QdrantLoadMs =
                            qdrantStopwatch.ElapsedMilliseconds,

                        SourceSelectionMs =
                            sourceSelectionStopwatch.ElapsedMilliseconds,

                        GenerationAttempts =
                            generationAttemptTimings,

                        TotalMs =
                            totalStopwatch.ElapsedMilliseconds
                    }
                });
            }


            // نحفظ كل سؤال نجح، حتى لو فشلت بعض أسئلة الدفعة.
            List<object> generatedQuestions =
                new List<object>();


            Stopwatch saveStopwatch =
                Stopwatch.StartNew();


            foreach (PendingGeneratedQuestion pending
                     in pendingQuestions)
            {
                int questionID =
                    SaveQuestion(
                        pending.Question,
                        request.DocumentID,
                        pending.SourcePageNumber,
                        pending.SourceChunkIndex);


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

                    SourceEvidence =
                        pending.Question.SourceEvidence,

                    EvidenceValidated =
                        pending.EvidenceValidation.IsValid,

                    ReviewStatus =
                        "Draft",

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


            saveStopwatch.Stop();
            totalStopwatch.Stop();


            return Ok(new
            {
                DocumentID =
                    request.DocumentID,

                TotalPages =
                    totalPages,

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

                FailedQuestions =
                    failedQuestions.Count,

                IsComplete =
                    generatedQuestions.Count ==
                    totalQuestions,

                RejectedSourceAttempts =
                    rejectedSourceAttempts,

                QuestionFailures =
                    failedQuestions,

                Performance = new
                {
                    QdrantLoadMs =
                        qdrantStopwatch.ElapsedMilliseconds,

                    SourceSelectionMs =
                        sourceSelectionStopwatch.ElapsedMilliseconds,

                    GenerationAttempts =
                        generationAttemptTimings,

                    SqlSaveMs =
                        saveStopwatch.ElapsedMilliseconds,

                    TotalMs =
                        totalStopwatch.ElapsedMilliseconds
                },

                Questions =
                    generatedQuestions
            });
        }


        // =====================================================
        // التوليد + التحقق البرمجي من الدليل
        // =====================================================

        private static async Task<EvidenceBackedGeneratedQuestion>
            GenerateEvidenceBackedQuestion(
                string sourceText,
                string questionType)
        {
            GeneratedQuestion question =
                await OllamaQuestionGeneratorService
                    .GenerateQuestionAsync(
                        sourceText,
                        questionType);


            PrepareAndValidateQuestion(
                question,
                questionType);


            QuestionEvidenceValidationResult evidenceValidation =
                QuestionEvidenceValidator.Validate(
                    sourceText,
                    question.SourceEvidence);


            if (!evidenceValidation.IsValid)
            {
                throw new InvalidOperationException(
                    "Generated question failed deterministic " +
                    "source-evidence validation. " +
                    evidenceValidation.FailureReason +
                    $" SourceEvidence=[{question.SourceEvidence}]");
            }


            return new EvidenceBackedGeneratedQuestion
            {
                Question =
                    question,

                EvidenceValidation =
                    evidenceValidation
            };
        }


        // =====================================================
        // Structural validation
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


        // =====================================================
        // مصدر أساسي + مصدر بديل واحد
        // =====================================================

        private static List<QuestionSourceCandidate>
            BuildSourceAttempts(
                QuestionSourceCandidate primarySource,
                List<QuestionSourceCandidate> fallbackSources,
                HashSet<int> usedChunkIndexes,
                int maxSources)
        {
            List<QuestionSourceCandidate> result =
                new List<QuestionSourceCandidate>();


            if (primarySource != null &&
                primarySource.Chunk != null &&
                !usedChunkIndexes.Contains(
                    primarySource.Chunk.ChunkIndex))
            {
                result.Add(
                    primarySource);
            }


            int referencePage =
                primarySource?.Chunk?.PageNumber ??
                1;


            IEnumerable<QuestionSourceCandidate> orderedFallbacks =
                fallbackSources
                    .Where(candidate =>
                        candidate?.Chunk != null &&

                        !usedChunkIndexes.Contains(
                            candidate.Chunk.ChunkIndex) &&

                        (primarySource == null ||
                         candidate.Chunk.ChunkIndex !=
                         primarySource.Chunk.ChunkIndex))
                    .OrderBy(candidate =>
                        candidate.Chunk.PageNumber ==
                            referencePage
                                ? 1
                                : 0)
                    .ThenByDescending(candidate =>
                        candidate.ExamValueScore)
                    .ThenByDescending(candidate =>
                        candidate.QualityScore);


            foreach (QuestionSourceCandidate fallback
                     in orderedFallbacks)
            {
                if (result.Count >=
                    maxSources)
                {
                    break;
                }


                if (result.Any(existing =>
                        existing.Chunk.ChunkIndex ==
                        fallback.Chunk.ChunkIndex))
                {
                    continue;
                }


                result.Add(
                    fallback);
            }


            return result;
        }


        private static int SaveQuestion(
            GeneratedQuestion question,
            int? sourceDocumentID,
            int? sourcePageNumber,
            int? sourceChunkIndex)
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
                    sourcePageNumber,
                    question.SourceEvidence,
                    sourceChunkIndex,
                    reviewStatus: "Draft");


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


            int trueFalseAdded =
                0;


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
                }
            }


            return types;
        }
    }


    internal class EvidenceBackedGeneratedQuestion
    {
        public GeneratedQuestion Question { get; set; }

        public QuestionEvidenceValidationResult
            EvidenceValidation
        { get; set; }
    }


    internal class PendingGeneratedQuestion
    {
        public GeneratedQuestion Question { get; set; }

        public QuestionEvidenceValidationResult
            EvidenceValidation
        { get; set; }

        public int SourcePageNumber { get; set; }

        public int SourceChunkIndex { get; set; }

        public double SourceQualityScore { get; set; }
    }


    public class GenerateQuestionRequest
    {
        public string SourceText { get; set; } =
            string.Empty;

        public string QuestionType { get; set; } =
            "MultipleChoice";

        public int? SourceDocumentID { get; set; }

        public int? SourcePageNumber { get; set; }
    }


    public class UpdateQuestionReviewStatusRequest
    {
        public string ReviewStatus { get; set; } =
            string.Empty;
    }


    public class GenerateQuestionsFromDocumentRequest
    {
        public int DocumentID { get; set; }

        public int MultipleChoiceCount { get; set; }

        public int TrueFalseCount { get; set; }
    }
}
