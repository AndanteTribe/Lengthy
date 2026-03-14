#nullable enable

using System;

namespace Lengthy
{
    /// 今後他のテキストソースや新たな読み込み方式を追加することを見込んだインターフェース。
    /// <summary>
    /// Represents a paragraph-addressable document that can provide text to the Lengthy view layer on demand.
    /// </summary>
    public interface ILengthyDocument : IDisposable
    {
        /// <summary>
        /// Gets the total number of paragraphs available for presentation.
        /// </summary>
        int ParagraphCount { get; }

        /// <summary>
        /// Reads and returns the paragraph at the specified zero-based index.
        /// </summary>
        /// <param name="index">The zero-based paragraph index.</param>
        /// <returns>The paragraph text for the requested index.</returns>
        string ReadParagraph(int index);
    }
}