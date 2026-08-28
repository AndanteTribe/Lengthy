#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lengthy
{
    public class LengthyView : VisualElement
    {
        private readonly StreamReader _reader;
        private readonly string[] _values = new string[1];

        /// <summary>
        /// 閉じるボタンが押された時に発火する
        /// </summary>
        public event Action? CloseButtonClicked;

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
                closeButton.RegisterCallbackOnce<ClickEvent>(evt =>
                {
                    CloseButtonClicked?.Invoke();
                });
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

        private const string DefaultStyleSheetResourcePath = "DefaultLengthyUss";
        private static StyleSheet? s_defaultStyleSheet;

        /// <summary>
        /// StyleSheetを当てた上で、指定した要素の子として表示し、閉じられるまで待機する
        /// </summary>
        /// <param name="root">表示先の親要素</param>
        /// <param name="styleSheet">適用するStyleSheet。省略した場合はパッケージ同梱のデフォルトを使用する</param>
        /// <param name="additionalStyleSheets">styleSheetの上から重ねる追加のStyleSheet。不要ならnullでよい</param>
        /// <param name="token">キャンセルすると非表示になる。閉じるボタン押下時にもこのTaskは完了する</param>
        public async Task ShowAsync(VisualElement root, StyleSheet? styleSheet = null, IReadOnlyList<StyleSheet>? additionalStyleSheets = null, CancellationToken token = default)
        {
            styleSheet ??= s_defaultStyleSheet ??= Resources.Load<StyleSheet>(DefaultStyleSheetResourcePath)
                ?? throw new InvalidOperationException($"Default StyleSheet not found at Resources/{DefaultStyleSheetResourcePath}.");
            root.styleSheets.Add(styleSheet);
            if (additionalStyleSheets != null)
            {
                foreach (var additional in additionalStyleSheets)
                {
                    root.styleSheets.Add(additional);
                }
            }
            root.Add(this);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            var tcs = new TaskCompletionSource<bool>();
            using var registration = cts.Token.Register(static state => ((TaskCompletionSource<bool>)state!).TrySetResult(true), tcs);
            CloseButtonClicked += OnCloseButtonClicked;

            try
            {
                // 閉じるボタン、または外部からのキャンセルを待つ
                await tcs.Task;
            }
            finally
            {
                // 非表示にするよ
                CloseButtonClicked -= OnCloseButtonClicked;
                root.Remove(this);
                root.styleSheets.Remove(styleSheet);
                if (additionalStyleSheets != null)
                {
                    foreach (var additional in additionalStyleSheets)
                    {
                        root.styleSheets.Remove(additional);
                    }
                }
            }

            void OnCloseButtonClicked() => cts.Cancel();
        }
    }
}
