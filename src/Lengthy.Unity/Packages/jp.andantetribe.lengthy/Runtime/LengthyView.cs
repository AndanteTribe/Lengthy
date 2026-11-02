#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using Lengthy.Internal;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lengthy
{
    public class LengthyView : VisualElement
    {
        private const string DefaultStyleSheetResourcePath = "DefaultLengthyUss";

        public LengthyView(TextAsset textAsset, string title = "", bool alwaysIncludeDefaultStyleSheet = false)
            : this(new []{ textAsset.text }, title, alwaysIncludeDefaultStyleSheet, null)
        {
        }

        public LengthyView(string value, string title = "", bool alwaysIncludeDefaultStyleSheet = false)
            : this(new []{ value }, title, alwaysIncludeDefaultStyleSheet, null)
        {
        }

        public LengthyView(string[] values, string title = "", bool alwaysIncludeDefaultStyleSheet = false)
            :this(values, title, alwaysIncludeDefaultStyleSheet, null)
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
                closeButton.RegisterCallbackOnce<ClickEvent, LengthyValueTaskSource>(static (_, vts) =>
                {
                    vts.SetResult();
                }, valueTaskSource);
            }
            else
            {
                closeButton.RegisterCallback<ClickEvent, LengthyView>(static (_, view) =>
                {
                    view.RemoveFromHierarchy();
                }, this);
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
        /// 渡されたテキストの内容をUIで表示する.
        /// 閉じるかキャンセルされると生成したオブジェクトを破棄する.
        /// </summary>
        /// <param name="textAsset">表示するテキスト.</param>
        /// <param name="title">タイトル.</param>
        /// <param name="cancellationToken">キャンセルすると非表示になる. 閉じるボタン押下時にもこのTaskは完了する. キャンセルはメインスレッドから行うこと.</param>
        public static ValueTask ShowAsync(TextAsset textAsset, string title = "", CancellationToken cancellationToken = default)
        {
            if (textAsset == null)
            {
                throw new ArgumentNullException(nameof(textAsset));
            }
            return ShowAsync(textAsset.text, title, cancellationToken);
        }

        /// <summary>
        /// 渡されたテキストの内容をUIで表示する.
        /// 閉じるかキャンセルされると生成したオブジェクトを破棄する.
        /// </summary>
        /// <param name="value">表示するテキスト.</param>
        /// <param name="title">タイトル.</param>
        /// <param name="cancellationToken">キャンセルすると非表示になる. 閉じるボタン押下時にもこのTaskは完了する. キャンセルはメインスレッドから行うこと.</param>
        public static ValueTask ShowAsync(string value, string title = "", CancellationToken cancellationToken = default)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            return ShowAsync(new[] { value }, title, cancellationToken);
        }

        /// <summary>
        /// 渡されたテキストの内容をUIで表示する.
        /// 閉じるかキャンセルされると生成したオブジェクトを破棄する.
        /// </summary>
        /// <param name="values">段落ごとに分割されたテキスト.</param>
        /// <param name="title">タイトル.</param>
        /// <param name="cancellationToken">キャンセルすると非表示になる. 閉じるボタン押下時にもこのTaskは完了する. キャンセルはメインスレッドから行うこと.</param>
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
