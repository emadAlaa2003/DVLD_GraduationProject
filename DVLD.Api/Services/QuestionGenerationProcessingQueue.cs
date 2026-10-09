using System.Threading.Channels;

namespace DVLD.Api.Services
{
    public class QuestionGenerationProcessingJob
    {
        public int QuestionGenerationJobID { get; set; }

        public int DocumentID { get; set; }

        public int MultipleChoiceCount { get; set; }

        public int TrueFalseCount { get; set; }


        public QuestionGenerationProcessingJob(
            int questionGenerationJobID,
            int documentID,
            int multipleChoiceCount,
            int trueFalseCount)
        {
            QuestionGenerationJobID =
                questionGenerationJobID;

            DocumentID =
                documentID;

            MultipleChoiceCount =
                multipleChoiceCount;

            TrueFalseCount =
                trueFalseCount;
        }
    }


    public interface IQuestionGenerationProcessingQueue
    {
        ValueTask QueueAsync(
            QuestionGenerationProcessingJob job,
            CancellationToken cancellationToken = default);

        ValueTask<QuestionGenerationProcessingJob> DequeueAsync(
            CancellationToken cancellationToken);
    }


    public class QuestionGenerationProcessingQueue
        : IQuestionGenerationProcessingQueue
    {
        private readonly Channel<QuestionGenerationProcessingJob>
            _queue;


        public QuestionGenerationProcessingQueue()
        {
            BoundedChannelOptions options =
                new BoundedChannelOptions(100)
                {
                    FullMode = BoundedChannelFullMode.Wait,
                    SingleReader = true,
                    SingleWriter = false
                };


            _queue =
                Channel.CreateBounded<QuestionGenerationProcessingJob>(
                    options);
        }


        public async ValueTask QueueAsync(
            QuestionGenerationProcessingJob job,
            CancellationToken cancellationToken = default)
        {
            if (job == null)
                throw new ArgumentNullException(nameof(job));


            await _queue.Writer.WriteAsync(
                job,
                cancellationToken);
        }


        public async ValueTask<QuestionGenerationProcessingJob>
            DequeueAsync(
                CancellationToken cancellationToken)
        {
            return await _queue.Reader.ReadAsync(
                cancellationToken);
        }
    }
}