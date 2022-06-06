using System;
using System.IO;

namespace ProxyTest
{
    /// <summary>
    ///  Buffering information into MemoryStream in Read operation.
    /// </summary>
    public class BufferedReadStream : Stream
    {
        private readonly Stream _stream;

        private readonly MemoryStream _buffer;

        public BufferedReadStream(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            _stream = stream;
            _buffer = new MemoryStream();
        }

        public byte[] Content => _buffer.GetBuffer();
        public override void Flush()
        {
            _stream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _stream.SetLength(value);
        }

        /// <summary>
        /// When overridden in a derived class, reads a sequence of bytes from the current stream and advances
        /// the position within the stream by the number of bytes read,
        /// Buffering all read bytes into MemoryStream.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead = _stream.Read(buffer, offset, count);

            _buffer.Write(buffer, offset, bytesRead);

            return bytesRead;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _stream.Write(buffer, offset, count);
        }

        public override bool CanRead => _stream.CanRead;
        public override bool CanSeek => _stream.CanSeek;
        public override bool CanWrite => _stream.CanWrite;
        public override long Length => _stream.Length;

        public override long Position
        {
            get { return _stream.Position; }
            set { _stream.Position = value; }
        }
    }
}