using DVLD.AI;
using DVLD_Buisness;

namespace DVLD.Api.Services
{
    public static class QuestionGenerationProcessingService
    {
        private const int MaxSourceAttemptsPerQuestion = 3;


        public static async Task<int> ProcessAsync(
            int documentID,
            int multipleChoiceCount,
            int trueFalseCount)
        {
            int totalQuestions =
                multipleChoiceCount +
                trueFalseCount;

            if (totalQuestions <= 0)
            {
                throw new InvalidOperationException(
                    "At least one question must be requested.");
            }

            if (totalQuestions > 100)
            {
                throw new InvalidOperationException(
                    "Maximum question count is 100 per request.");
            }


            string filePath =
                clsKnowledgeDocument
                    .GetFilePathByDocumentID(
                        documentID);

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new InvalidOperationException(
                    "Document was not found.");
            }

            if (!File.Exists(filePath))
            {
                throw new InvalidOperationException(
                    "The PDF file does not exist on disk.");
            }


            List<TextChunk> allChunks =
                await QdrantKnowledgeStore
                    .GetDocumentChunksAsync(
                        documentID);

            if (allChunks.Count == 0)
            {
                throw new InvalidOperationException(
                    "No text chunks were found in the document.");
            }


            int totalPages =
                allChunks.Max(
                    chunk => chunk.PageNumber);


            List<QuestionSourceCandidate> selectedSources =
                QuestionSourceSelector
                    .SelectBestSources(
                        allChunks,
                        totalPages,
                        totalQuestions);


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
                    multipleChoiceCount,
                    trueFalseCount);


            HashSet<int> usedChunkIndexes =
                new HashSet<int>();

            List<PendingGeneratedQuestion> pendingQuestions =
                new List<PendingGeneratedQuestion>();


            string lastGenerationError =
                null;


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


                foreach (QuestionSourceCandidate sourceCandidate
                         in sourcesForQuestion)
                {
                    TextChunk sourceChunk =
                        sourceCandidate.Chunk;

                    usedChunkIndexes.Add(
                        sourceChunk.ChunkIndex);


                    try
                    {
                        GeneratedQuestion generatedQuestion =
                            await GenerateEvidenceBackedQuestion(
                                sourceChunk.Text,
                                questionType);


                        bool isDuplicate =
                            clsQuestionBank
                                .IsDuplicateQuestion(
                                    generatedQuestion.QuestionText,
                                    documentID,
                                    generatedQuestion.SourceEvidence);


                        if (isDuplicate)
                        {
                            throw new InvalidOperationException(
                                "Generated question duplicates an existing active question from the same document.");
                        }


                        acceptedQuestion =
                            new PendingGeneratedQuestion
                            {
                                Question =
                                    generatedQuestion,

                                SourcePageNumber =
                                    sourceChunk.PageNumber,

                                SourceChunkIndex =
                                    sourceChunk.ChunkIndex
                            };


                        break;
                    }
                    catch (Exception ex)
                    {
                        lastGenerationError =
                            ex.Message;

                        // فشل هذا المصدر لا يوقف السؤال.
                        // نجرب المصدر البديل.
                    }
                }


                if (acceptedQuestion != null)
                {
                    pendingQuestions.Add(
                        acceptedQuestion);
                }
            }


            if (pendingQuestions.Count == 0)
            {
                throw new InvalidOperationException(
                    "No questions could be generated from the requested batch. " +
                    "Last error: " +
                    (lastGenerationError ??
                     "Unknown error."));
            }


            foreach (PendingGeneratedQuestion pending
                     in pendingQuestions)
            {
                SaveQuestion(
                    pending.Question,
                    documentID,
                    pending.SourcePageNumber,
                    pending.SourceChunkIndex);
            }


            return pendingQuestions.Count;
        }


        private static async Task<GeneratedQuestion>
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
                QuestionEvidenceValidator
                    .Validate(
                        sourceText,
                        question.SourceEvidence);


            if (!evidenceValidation.IsValid)
            {
                throw new InvalidOperationException(
                    "Generated question failed deterministic " +
                    "source-evidence validation. " +
                    evidenceValidation.FailureReason);
            }


            return question;
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
                primarySource?.Chunk?.PageNumber ?? 1;


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
                if (result.Count >= maxSources)
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


        private static void SaveQuestion(
            GeneratedQuestion question,
            int sourceDocumentID,
            int sourcePageNumber,
            int sourceChunkIndex)
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


        private class PendingGeneratedQuestion
        {
            public GeneratedQuestion Question { get; set; }

            public int SourcePageNumber { get; set; }

            public int SourceChunkIndex { get; set; }
        }
    }
}