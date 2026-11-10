#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using Lengthy.Internal;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lengthy
{
    /// <summary>
    /// A scrollable UI Toolkit view for displaying long-form text.
    /// </summary>
    public class LengthyView : VisualElement
    {
        private const string DefaultStyleSheetResourcePath = "DefaultLengthyUss";

        /// <summary>
        /// Creates a view that displays the contents of a text asset as a single item.
        /// </summary>
        /// <param name="textAsset">The text asset to display.</param>
        /// <param name="title">The window title. An empty string hides the title label.</param>
        /// <param name="alwaysIncludeDefaultStyleSheet">Whether to insert the default stylesheet into the parent even when it already has stylesheets.</param>
        public LengthyView(TextAsset textAsset, string title = "", bool alwaysIncludeDefaultStyleSheet = false)
            : this(new[] { textAsset.text }, title, alwaysIncludeDefaultStyleSheet, null)
        {
        }

        /// <summary>
        /// Creates a view that displays a string as a single item.
        /// </summary>
        /// <param name="value">The text to display.</param>
        /// <param name="title">The window title. An empty string hides the title label.</param>
        /// <param name="alwaysIncludeDefaultStyleSheet">Whether to insert the default stylesheet into the parent even when it already has stylesheets.</param>
        public LengthyView(string value, string title = "", bool alwaysIncludeDefaultStyleSheet = false)
            : this(new[] { value }, title, alwaysIncludeDefaultStyleSheet, null)
        {
        }

        /// <summary>
        /// Creates a view that displays each array element as a separate paragraph.
        /// </summary>
        /// <param name="values">The paragraphs to display.</param>
        /// <param name="title">The window title. An empty string hides the title label.</param>
        /// <param name="alwaysIncludeDefaultStyleSheet">Whether to insert the default stylesheet into the parent even when it already has stylesheets.</param>
        public LengthyView(string[] values, string title = "", bool alwaysIncludeDefaultStyleSheet = false)
            : this(values, title, alwaysIncludeDefaultStyleSheet, null)
        {
        }

        internal LengthyView(string[] values, string title, bool alwaysIncludeDefaultStyleSheet, LengthyValueTaskSource? valueTaskSource)
        {
            RegisterCallbackOnce<AttachToPanelEvent, bool>(static (evt, alwaysIncludeDefaultStyleSheet) =>
            {
                var parent = (evt.currentTarget as VisualElement)?.parent;
                if (parent != null && (alwaysIncludeDefaultStyleSheet || parent.styleSheets.count == 0))
                {
                    parent.styleSheets.Insert(0, Resources.Load<StyleSheet>(DefaultStyleSheetResourcePath));
                }
            }, alwaysIncludeDefaultStyleSheet);

            AddToClassList("lengthy-root");

            var window = new VisualElement();
            window.AddToClassList("lengthy-window");
            Add(window);

            var hasTitle = !string.IsNullOrEmpty(title);
            var topBar = new VisualElement();
            topBar.AddToClassList(hasTitle ? "lengthy-top-bar--with-title" : "lengthy-top-bar");
            window.Add(topBar);

            if (hasTitle)
            {
                var titleLabel = new Label(title);
                titleLabel.AddToClassList("lengthy-title");
                topBar.Add(titleLabel);
            }

            var closeButton = new Button { text = "X" };
            closeButton.AddToClassList("lengthy-close-button");
            if (valueTaskSource != null)
            {
                closeButton.RegisterCallbackOnce<ClickEvent, LengthyValueTaskSource>(static (_, vts) => vts.SetResult(), valueTaskSource);
            }
            else
            {
                closeButton.RegisterCallback<ClickEvent, LengthyView>(static (_, view) => view.RemoveFromHierarchy(), this);
            }
            topBar.Add(closeButton);

            var listView = new ListView
            {
                makeItem = static () =>
                {
                    var label = new Label();
                    label.AddToClassList("lengthy-paragraph");
                    return label;
                },
                itemsSource = values,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                selectionType = SelectionType.None,
            };
            listView.AddToClassList("lengthy-list");
            window.Add(listView);
            BindItems(listView);

            static void BindItems(ListView listView)
            {
                var entry = BindItemDelegatePool.Rent((string[])listView.itemsSource);
                listView.bindItem = entry.Callback;
                listView.RegisterCallbackOnce<DetachFromPanelEvent, (ListView List, BindItemDelegatePool.Entry Entry)>(static (_, state) =>
                {
                    var (listView, entry) = state;
                    listView.bindItem = null;
                    BindItemDelegatePool.Return(entry);
                    listView.RegisterCallbackOnce<AttachToPanelEvent, ListView>(static (_, list) => BindItems(list), listView);
                }, (listView, entry));
            }
        }

        /// <summary>
        /// Displays a text asset and waits until the view is closed or canceled.
        /// </summary>
        /// <param name="textAsset">The text asset to display as a single item.</param>
        /// <param name="title">The window title. An empty string hides the title label.</param>
        /// <param name="cancellationToken">A token that closes the view when canceled. Cancellation must occur on the Unity main thread.</param>
        /// <returns>An operation that completes when the view is closed.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="textAsset"/> is null.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static ValueTask ShowAsync(TextAsset textAsset, string title = "", CancellationToken cancellationToken = default)
        {
            if (textAsset == null)
            {
                throw new ArgumentNullException(nameof(textAsset));
            }
            return ShowAsync(textAsset.text, title, cancellationToken);
        }

        /// <summary>
        /// Displays a string and waits until the view is closed or canceled.
        /// </summary>
        /// <param name="value">The text to display as a single item.</param>
        /// <param name="title">The window title. An empty string hides the title label.</param>
        /// <param name="cancellationToken">A token that closes the view when canceled. Cancellation must occur on the Unity main thread.</param>
        /// <returns>An operation that completes when the view is closed.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static ValueTask ShowAsync(string value, string title = "", CancellationToken cancellationToken = default)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            return ShowAsync(new[] { value }, title, cancellationToken);
        }

        /// <summary>
        /// Displays each array element as a separate paragraph and waits until the view is closed or canceled.
        /// </summary>
        /// <param name="values">The paragraphs to display.</param>
        /// <param name="title">The window title. An empty string hides the title label.</param>
        /// <param name="cancellationToken">A token that closes the view when canceled. Cancellation must occur on the Unity main thread.</param>
        /// <returns>An operation that completes when the view is closed.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="values"/> is null.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static ValueTask ShowAsync(string[] values, string title = "", CancellationToken cancellationToken = default)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }
            var source = LengthyValueTaskSource.Create(title, values, cancellationToken);
            return new ValueTask(source, source.Version);
        }
    }
}