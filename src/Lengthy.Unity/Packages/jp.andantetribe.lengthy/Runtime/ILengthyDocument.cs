using System;

public interface ILengthyDocument : IDisposable
{
    int ParagraphCount { get; }
    string ReadParagraph(int index);
}