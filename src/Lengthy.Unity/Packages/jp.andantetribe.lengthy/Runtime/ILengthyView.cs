#nullable enable

using System;

namespace Lengthy
{
    /// <summary>
    /// Represents a view that can notify when the user requests to close it.
    /// </summary>
    public interface ILengthyView : IDisposable
    {
        /// <summary>
        /// Occurs when the user requests to close the current view.
        /// </summary>
        event Action? OnCloseClicked;

        /// <summary>
        /// Initializes the view content.
        /// </summary>
        /// <param name="title">Title text displayed in the header.</param>
        /// <param name="itemCount">Number of paragraphs to render.</param>
        /// <param name="itemProvider">Returns paragraph text by index.</param>
        void Initialize(string title, int itemCount, Func<int, string> itemProvider);

    }
}
