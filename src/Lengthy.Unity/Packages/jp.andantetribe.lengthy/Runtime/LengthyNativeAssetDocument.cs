using System;
using System.Text;
using UnityEngine;

public class LengthyNativeAssetDocument : ILengthyDocument
{
    private struct ParagraphInfo
    {
        public int Offset;
        public int Length;
    }

    private byte[] _data;
    private ParagraphInfo[] _paragraphs = Array.Empty<ParagraphInfo>();
    private int _paragraphCount;

    public int ParagraphCount => _paragraphCount;

    public LengthyNativeAssetDocument(TextAsset textAsset)
    {
        _data = textAsset.bytes;
        AnalyzeData();
    }

    private void AnalyzeData()
    {
        int paragraphStart = 0;
        ReadOnlySpan<byte> span = _data.AsSpan();

        for (int i = 0; i < span.Length; i++)
        {
            if (span[i] == (byte)'\n')
            {
                int pLen = i - paragraphStart;
                if (pLen > 0)
                {
                    AddParagraph(paragraphStart, pLen);
                }
                paragraphStart = i + 1;
            }
        }

        if (span.Length > paragraphStart)
        {
            AddParagraph(paragraphStart, span.Length - paragraphStart);
        }
    }

    private void AddParagraph(int offset, int length)
    {
        if (_paragraphCount == _paragraphs.Length)
        {
            int nextCapacity = _paragraphs.Length == 0 ? 256 : _paragraphs.Length * 2;
            Array.Resize(ref _paragraphs, nextCapacity);
        }

        _paragraphs[_paragraphCount++] = new ParagraphInfo { Offset = offset, Length = length };
    }

    public string ReadParagraph(int index)
    {
        var info = _paragraphs[index];
        ReadOnlySpan<byte> span = _data.AsSpan(info.Offset, info.Length);
        return Encoding.UTF8.GetString(span).TrimEnd('\r');
    }

    public void Dispose()
    {
        _paragraphs = Array.Empty<ParagraphInfo>();
        _data = Array.Empty<byte>();
        _paragraphCount = 0;
    }
}