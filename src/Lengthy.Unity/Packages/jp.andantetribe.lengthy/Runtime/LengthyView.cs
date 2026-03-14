#nullable enable

using System;
using UnityEngine.UIElements;

namespace Lengthy
{
    /// <summary>
    /// Displays long-form content as a header and paragraph list using UI Toolkit.
    /// </summary>
    public class LengthyView : ILengthyView
    {
        private const string EmptyParagraphPlaceholder = "\u200B";

        private readonly VisualElement _container;
        private readonly VisualElement _topBar;
        private readonly Label _headerLabel;
        private readonly Button _closeButton;
        private readonly ListView _listView;
        private Func<int, string>? _itemProvider;

        /// <summary>
        /// Occurs when the close button is clicked.
        /// </summary>
        public event Action? OnCloseClicked;

        /// <summary>
        /// Builds the view layout under the specified root element.
        /// </summary>
        /// <param name="root">The root element to which the view is added.</param>
        public LengthyView(VisualElement root)
        {
            _container = new VisualElement();
            _container.AddToClassList(LengthyStyleClassNames.Root);
            root.Add(_container);
            (_topBar, _headerLabel, _closeButton, _listView) = SetupLayout(_container);
        }

        /// <inheritdoc />
        public void Initialize(string title, int itemCount, Func<int, string> itemProvider)
        {
            _itemProvider = itemProvider;
            ApplyContent(title, itemCount);
            _listView.RefreshItems();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _itemProvider = null;
            _listView.bindItem = null;
            _listView.itemsSource = null;
            _container.RemoveFromHierarchy();
        }

        /// <summary>
        /// Retrieves text from the current paragraph provider and applies it to a list item <see cref="Label"/>.
        /// Empty strings are replaced with a placeholder to preserve rendering.
        /// </summary>
        /// <param name="element">The list item element to update.</param>
        /// <param name="index">The index of the paragraph to display.</param>
        private void BindItem(VisualElement element, int index)
        {
            if (_itemProvider == null)
            {
                ((Label)element).text = EmptyParagraphPlaceholder;
                return;
            }

            var paragraph = _itemProvider(index);
            ((Label)element).text = paragraph.Length == 0 ? EmptyParagraphPlaceholder : paragraph;
        }

        /// <summary>
        /// Applies the title and list data source to the view state.
        /// </summary>
        /// <param name="title">The title displayed in the header.</param>
        /// <param name="itemCount">The number of items to display.</param>
        private void ApplyContent(string title, int itemCount)
        {
            _headerLabel.text = title;
            var hasTitle = !string.IsNullOrEmpty(title);
            _headerLabel.EnableInClassList(LengthyStyleClassNames.HeaderHidden, !hasTitle);
            _topBar.EnableInClassList(LengthyStyleClassNames.TopBarWithTitle, hasTitle);
            _closeButton.EnableInClassList(LengthyStyleClassNames.CloseButtonInline, hasTitle);

            _listView.itemsSource = new int[itemCount];
            _listView.bindItem = BindItem;
        }

        /// <summary>
        /// Initializes the layout, including the header, close button, and paragraph list.
        /// </summary>
        /// <param name="root">The root element to which the layout is added.</param>
        /// <returns>The created header label and paragraph list.</returns>
        private (VisualElement TopBar, Label HeaderLabel, Button CloseButton, ListView ListView) SetupLayout(VisualElement root)
        {
            var safeAreaContainer = new SafeAreaContainer();
            root.Add(safeAreaContainer);
            var safeRoot = new VisualElement();
            safeRoot.AddToClassList(LengthyStyleClassNames.SafeRoot);
            safeAreaContainer.Add(safeRoot);

            var topBar = new VisualElement();
            topBar.AddToClassList(LengthyStyleClassNames.TopBar);
            safeRoot.Add(topBar);

            var headerLabel = new Label();
            headerLabel.AddToClassList(LengthyStyleClassNames.Header);
            headerLabel.AddToClassList(LengthyStyleClassNames.HeaderHidden);
            topBar.Add(headerLabel);

            var closeButton = new Button(() => OnCloseClicked?.Invoke())
            {
                text = "X"
            };
            closeButton.AddToClassList(LengthyStyleClassNames.CloseButton);
            topBar.Add(closeButton);

            var listView = new ListView
            {
                focusable = true,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                reorderable = false,
                selectionType = SelectionType.None,
                makeItem = CreateItem
            };
            listView.AddToClassList(LengthyStyleClassNames.List);

            safeRoot.Add(listView);
            return (topBar, headerLabel, closeButton, listView);
        }

        private static VisualElement CreateItem()
        {
            var label = new Label
            {
                enableRichText = true,
                focusable = false,
                pickingMode = PickingMode.Ignore,
            };
            label.AddToClassList(LengthyStyleClassNames.Paragraph);
            return label;
        }
    }
}
