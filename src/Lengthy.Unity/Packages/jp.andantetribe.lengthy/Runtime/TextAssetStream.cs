#nullable enable

using System;
using System.IO;
using Unity.Collections;
using UnityEngine;

namespace Lengthy
{
    internal class TextAssetStream : Stream
    {
        private NativeArray<byte> _data;
        private long _position;

        /// <inheritdoc />
        public override bool CanRead => true;

        /// <inheritdoc />
        public override bool CanSeek => true;

        /// <inheritdoc />
        public override bool CanWrite => false;

        /// <inheritdoc />
        public override long Length => _data.Length;

        /// <inheritdoc />
        public override long Position
        {
            get => _position;
            set => throw new NotSupportedException("TextAssetStream is read-only.");
        }

        /// <inheritdoc />
        public TextAssetStream(TextAsset textAsset) => _data = textAsset.GetData<byte>();

        /// <inheritdoc />
        public override void Flush()
        {
            // Flush is typically implemented as an empty method to ensure full compatibility with other Stream types.        }
        }

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count) => Read(new Span<byte>(buffer, offset, count));

        /// <inheritdoc />
        public override int Read(Span<byte> buffer)
        {
            var remaining = _data.Length - (int)_position;
            if (remaining <= 0)
            {
                return 0;
            }

            var count = Math.Min(buffer.Length, remaining);
            _data.AsSpan()
                .Slice((int)_position, count)
                .CopyTo(buffer);

            _position += count;
            return count;
        }

        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin)
        {
            var newPos = origin switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => _position + offset,
                SeekOrigin.End => Length + offset,
                _ => throw new ArgumentException()
            };

            if (newPos < 0 || newPos > Length)
            {
                throw new IOException("Invalid seek");
            }

            _position = newPos;
            return _position;
        }

        /// <inheritdoc />
        public override void SetLength(long value) => throw new NotSupportedException("TextAssetStream is read-only.");

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException("TextAssetStream is read-only.");

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            _data.Dispose();
            base.Dispose(disposing);
        }
    }
}