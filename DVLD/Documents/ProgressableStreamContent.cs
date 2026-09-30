using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace DVLD.Documents
{
    public class ProgressableStreamContent : HttpContent
    {
        private readonly Stream _stream;
        private readonly Action<long, long> _progress;
        private readonly int _bufferSize;


        public ProgressableStreamContent(
            Stream stream,
            Action<long, long> progress,
            int bufferSize = 81920)
        {
            _stream = stream;
            _progress = progress;
            _bufferSize = bufferSize;
        }


        protected override async Task SerializeToStreamAsync(
            Stream targetStream,
            TransportContext context)
        {
            byte[] buffer =
                new byte[_bufferSize];

            long totalLength =
                _stream.Length;

            long uploaded =
                0;

            int read;


            while ((read =
                await _stream.ReadAsync(
                    buffer,
                    0,
                    buffer.Length)) > 0)
            {
                await targetStream.WriteAsync(
                    buffer,
                    0,
                    read);

                uploaded += read;

                _progress?.Invoke(
                    uploaded,
                    totalLength);
            }
        }


        protected override bool TryComputeLength(
            out long length)
        {
            length = _stream.Length;

            return true;
        }


        protected override void Dispose(
            bool disposing)
        {
            if (disposing)
            {
                _stream.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}