using DVLD_Buisness;

namespace DVLD.Api.Services
{
    public class QuestionGenerationBackgroundWorker
        : BackgroundService
    {
        private readonly IQuestionGenerationProcessingQueue
            _queue;

        private readonly ILogger<QuestionGenerationBackgroundWorker>
            _logger;


        public QuestionGenerationBackgroundWorker(
            IQuestionGenerationProcessingQueue queue,
            ILogger<QuestionGenerationBackgroundWorker> logger)
        {
            _queue = queue;
            _logger = logger;
        }


        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                QuestionGenerationProcessingJob job;

                try
                {
                    job = await _queue.DequeueAsync(
                        stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }


                try
                {
                    bool processingMarked =
                        clsQuestionGenerationJob
                            .MarkProcessing(
                                job.QuestionGenerationJobID);

                    if (!processingMarked)
                    {
                        throw new InvalidOperationException(
                            "Question generation job could not be marked as Processing.");
                    }


                    _logger.LogInformation(
                        "Question generation job {QuestionGenerationJobID} " +
                        "for document {DocumentID} started.",
                        job.QuestionGenerationJobID,
                        job.DocumentID);


                    int generatedQuestionsCount =
                        await QuestionGenerationProcessingService
                            .ProcessAsync(
                                job.DocumentID,
                                job.MultipleChoiceCount,
                                job.TrueFalseCount);


                    bool completedMarked =
                        clsQuestionGenerationJob
                            .MarkCompleted(
                                job.QuestionGenerationJobID,
                                generatedQuestionsCount);

                    if (!completedMarked)
                    {
                        throw new InvalidOperationException(
                            "Question generation job could not be marked as Completed.");
                    }


                    _logger.LogInformation(
                        "Question generation job {QuestionGenerationJobID} " +
                        "completed with {GeneratedQuestionsCount} questions.",
                        job.QuestionGenerationJobID,
                        generatedQuestionsCount);
                }
                catch (Exception ex)
                {
                    try
                    {
                        clsQuestionGenerationJob
                            .MarkFailed(
                                job.QuestionGenerationJobID,
                                ex.Message);
                    }
                    catch (Exception statusException)
                    {
                        _logger.LogError(
                            statusException,
                            "Failed to mark question generation job " +
                            "{QuestionGenerationJobID} as Failed.",
                            job.QuestionGenerationJobID);
                    }


                    _logger.LogError(
                        ex,
                        "Question generation job {QuestionGenerationJobID} " +
                        "for document {DocumentID} failed.",
                        job.QuestionGenerationJobID,
                        job.DocumentID);
                }
            }
        }
    }
}