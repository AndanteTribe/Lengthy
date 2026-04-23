#nullable enable

using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lengthy
{
    public class LengthyView : VisualElement
    {
        private readonly StreamReader _reader;
        private readonly string[] _values = new string[1];

        public LengthyView(TextAsset textAsset, Encoding? encoding = null, string title = "") : this(new TextAssetStream(textAsset), encoding, true, title)
        {
        }

        public LengthyView(Stream stream, Encoding? encoding = null, bool leaveOpen = false, string title = "")
        {
            if (!stream.CanSeek)
            {
                throw new ArgumentException("The provided Stream must be based on a seekable stream.", nameof(stream));
            }
            _reader = new StreamReader(stream, encoding ?? Encoding.UTF8, false, 1024, leaveOpen);

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

                var closeButton = new Button{ text = "X" };
                closeButton.AddToClassList("lengthy-close-button");
                topBar.Add(closeButton);
            }

            var listView = new ListView
            {
                makeItem = static () =>
                {
                    var label = new Label();
                    label.AddToClassList("lengthy-paragraph");
                    return label;
                },
                bindItem = (element, index) =>
                {
                    if (!_reader.EndOfStream)
                    {
                        _values[index] = _reader.ReadToEnd();
                    }
                    ((Label)element).text = _values[index];
                },
                itemsSource = _values,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight
            };
            listView.AddToClassList("lengthy-list");
            window.Add(listView);

            RegisterCallback<DetachFromPanelEvent>(static evt =>
            {
                var self = (LengthyView)evt.target;
                self._reader.Dispose();
            });
        }
    }
}