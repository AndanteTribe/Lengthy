#nullable enable

using System;
using System.Text;
using Unity.Collections;
using UnityEngine;

namespace Lengthy
{
    /// <summary>
    /// Provides paragraph-based access to a <see cref="TextAsset"/> without materializing the entire document as a managed string.
    /// </summary>
    /// <remarks>
    /// The document is indexed once up front by recording the byte offset and byte length of each paragraph.
    /// Paragraph text is decoded on demand, which keeps managed allocations proportional to the visible content.
    /// Empty lines are preserved as empty paragraphs so the original visual spacing can be reproduced by the view layer.
    /// </remarks>
    public class LengthyTextAssetDocument : ILengthyDocument
    {
        private struct ParagraphInfo
        {
            public int Offset;
            public int Length;
        }

        // このTextAssetは使われませんが、NativeArray<byte>の寿命を管理するために保持しています。
        private TextAsset _textAsset;
        private NativeArray<byte> _data;
        private ParagraphInfo[] _paragraphs = Array.Empty<ParagraphInfo>();

        /// <summary>
        /// Gets the number of indexed paragraphs, including empty paragraphs created by blank lines.
        /// </summary>
        public int ParagraphCount => _paragraphs.Length;

        /// <summary>
        /// Creates a document view over the UTF-8 byte data stored in the specified <see cref="TextAsset"/>.
        /// </summary>
        /// <param name="textAsset">The source text asset to index.</param>
        public LengthyTextAssetDocument(TextAsset textAsset)
        {
            _textAsset = textAsset;
            _data = textAsset.GetData<byte>();
            AnalyzeData();
        }

        /// <summary>
        /// Scans the source bytes once and builds a compact paragraph index.
        /// </summary>
        /// <remarks>
        /// Paragraphs are split on line-feed characters. A trailing carriage return in CRLF input is excluded from the stored length.
        /// Blank lines are retained as zero-length paragraphs. An empty file produces zero paragraphs.
        /// </remarks>
        private void AnalyzeData()
        {
            ReadOnlySpan<byte> span = _data.AsSpan();
            if (span.Length == 0)
            {
                _paragraphs = Array.Empty<ParagraphInfo>();
                return;
            }

            // まず、段落の開始オフセットと長さを計算するために行フィードをスキャンして段落数を数えます。
            var count = 0;
            var paragraphStart = 0;
            for (var i = 0; i < span.Length; i++)
            {
                if (span[i] != (byte)'\n')
                {
                    continue;
                }

                if (TryGetParagraphLength(span, paragraphStart, i, out _))
                {
                    count++;
                }

                paragraphStart = i + 1;
            }

            if (TryGetParagraphLength(span, paragraphStart, span.Length, out _))
            {
                count++;
            }

            _paragraphs = new ParagraphInfo[count];

            // 次に、段落の開始オフセットと長さを実際に記録します。
            var paragraphIndex = 0;
            paragraphStart = 0;
            for (var i = 0; i < span.Length; i++)
            {
                if (span[i] != (byte)'\n')
                {
                    continue;
                }

                if (TryGetParagraphLength(span, paragraphStart, i, out var length))
                {
                    _paragraphs[paragraphIndex++] = new ParagraphInfo
                    {
                        Offset = paragraphStart,
                        Length = length
                    };
                }

                paragraphStart = i + 1;
            }

            if (TryGetParagraphLength(span, paragraphStart, span.Length, out var tailLength))
            {
                _paragraphs[paragraphIndex] = new ParagraphInfo
                {
                    Offset = paragraphStart,
                    Length = tailLength
                };
            }
        }

        /// <summary>
        /// Computes the byte length of a paragraph between two offsets.
        /// </summary>
        /// <param name="span">The full source byte span.</param>
        /// <param name="start">The inclusive start offset of the paragraph.</param>
        /// <param name="endExclusive">The exclusive end offset of the paragraph.</param>
        /// <param name="length">When this method returns <see langword="true"/>, contains the normalized paragraph byte length.</param>
        /// <returns>
        /// <see langword="true"/> when the range represents a valid paragraph, including blank lines;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        private static bool TryGetParagraphLength(ReadOnlySpan<byte> span, int start, int endExclusive, out int length)
        {
            length = endExclusive - start;
            if (length < 0)
            {
                return false;
            }

            if (length > 0 && span[endExclusive - 1] == (byte)'\r')
            {
                length--;
            }

            return length >= 0;
        }

        /// <summary>
        /// Decodes and returns the paragraph at the specified index.
        /// </summary>
        /// <param name="index">The zero-based paragraph index.</param>
        /// <returns>The decoded paragraph text. Blank lines are returned as <see cref="string.Empty"/>.</returns>
        public string ReadParagraph(int index)
        {
            var info = _paragraphs[index];
            ReadOnlySpan<byte> span = _data.AsSpan().Slice(info.Offset, info.Length);
            return Encoding.UTF8.GetString(span);
        }

        /// <summary>
        /// Releases references held by this document so the indexed data can be collected when no longer used.
        /// </summary>
        public void Dispose()
        {
            _paragraphs = Array.Empty<ParagraphInfo>();
            _data = default;
            _textAsset = null!;
        }
    }
}
