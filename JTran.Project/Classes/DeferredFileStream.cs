using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace JTran.Project
{
    /****************************************************************************/
    /****************************************************************************/
    internal class DeferredFileStream : Stream
    {
        private Stream? _stream;

        internal DeferredFileStream(string initialFileName)
        {
            this.FileName = initialFileName;
        }

        internal string FileName { get; set; }

        private Stream Stream => _stream == null ? _stream = File.Create(FileName) : _stream;

        #region Stream

        public override bool CanRead  => this.Stream.CanRead;
        public override bool CanSeek  => this.Stream.CanSeek;
        public override bool CanWrite => this.Stream.CanWrite;
        public override long Length   => this.Stream.Length;

        public override long Position { get => this.Stream.Position; set => this.Stream.Position = value; }

        public override void Flush()
        {
            this.Stream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return this.Stream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return this.Stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            this.Stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.Stream.Write(buffer, offset, count);
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            _stream?.Dispose();
        }

        public override async ValueTask DisposeAsync()
        {
            if(_stream != null)
                await _stream!.DisposeAsync();
        }
    }
}
