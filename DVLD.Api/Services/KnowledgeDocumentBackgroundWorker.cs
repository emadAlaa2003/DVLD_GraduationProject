namespace DVLD.Api.Services
{
    public class KnowledgeDocumentBackgroundWorker
        : BackgroundService
    {
        private readonly IKnowledgeDocumentProcessingQueue
            _queue;

        private readonly ILogger<KnowledgeDocumentBackgroundWorker>
            _logger;


        public KnowledgeDocumentBackgroundWorker(
            IKnowledgeDocumentProcessingQueue queue,
            ILogger<KnowledgeDocumentBackgroundWorker> logger)
        {
            _queue = queue;
            _logger = logger;
        }


        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                KnowledgeDocumentProcessingJob job;

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
                    _logger.LogInformation(
                        "Processing document {DocumentID} started.",
                        job.DocumentID);

                    await KnowledgeDocumentProcessingService
                        .ProcessAsync(
                            job.DocumentID,
                            job.FilePath);

                    _logger.LogInformation(
                        "Processing document {DocumentID} completed.",
                        job.DocumentID);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Processing document {DocumentID} failed.",
                        job.DocumentID);
                }
            }
        }
    }
}