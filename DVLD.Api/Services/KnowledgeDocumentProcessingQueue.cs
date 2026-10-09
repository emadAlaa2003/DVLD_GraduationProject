using System.Threading.Channels;

namespace DVLD.Api.Services
{
    public class KnowledgeDocumentProcessingJob
    {
        public int DocumentID { get; set; }

        public string FilePath { get; set; }

        public KnowledgeDocumentProcessingJob(
            int documentID,
            string filePath)
        {
            DocumentID = documentID;
            FilePath = filePath;
        }
    }


    public interface IKnowledgeDocumentProcessingQueue
    {
        ValueTask QueueAsync(
            KnowledgeDocumentProcessingJob job,
            CancellationToken cancellationToken = default);

        ValueTask<KnowledgeDocumentProcessingJob> DequeueAsync(
            CancellationToken cancellationToken);
    }


    public class KnowledgeDocumentProcessingQueue
        : IKnowledgeDocumentProcessingQueue
    {
        private readonly Channel<KnowledgeDocumentProcessingJob>
            _queue;

        public KnowledgeDocumentProcessingQueue()
        {
            BoundedChannelOptions options =
                new BoundedChannelOptions(100)
                {
                    FullMode = BoundedChannelFullMode.Wait,
                    SingleReader = true,
                    SingleWriter = false
                };

            _queue =
                Channel.CreateBounded<KnowledgeDocumentProcessingJob>(
                    options);
        }


        public async ValueTask QueueAsync(
            KnowledgeDocumentProcessingJob job,
            CancellationToken cancellationToken = default)
        {
            if (job == null)
                throw new ArgumentNullException(nameof(job));

            await _queue.Writer.WriteAsync(
                job,
                cancellationToken);
        }


        public async ValueTask<KnowledgeDocumentProcessingJob>
            DequeueAsync(
                CancellationToken cancellationToken)
        {
            return await _queue.Reader.ReadAsync(
                cancellationToken);
        }
    }
}