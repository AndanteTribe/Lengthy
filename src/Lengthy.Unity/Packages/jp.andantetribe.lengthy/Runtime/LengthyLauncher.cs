#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Lengthy
{
    public static class LengthyLauncher
    {
        /// <summary>
        /// ビューの作成
        /// </summary>
        /// <param name="root"></param>
        /// <param name="lengthyUss"></param>
        /// <param name="textAsset"></param>
        /// <param name="title"></param>
        public static async Task ShowAsync(TextAsset textAsset, string title = "")
        {
            var styleSheet = ExternalResources.LoadStyleSheet();
            var uiDocument = Object.FindFirstObjectByType<UIDocument>();
            var root = uiDocument.rootVisualElement;

            // 表示するよ
            root.styleSheets.Add(styleSheet);
            var lengthyView = new LengthyView(textAsset, title: title);
            root.Add(lengthyView);

            using var cts = new CancellationTokenSource();
            lengthyView.CloseButtonClicked += () => cts.Cancel();

            try
            {
                await Task.Delay(Timeout.Infinite, cts.Token);
            }
            catch (OperationCanceledException)
            {
                // 非表示にするよ
            }
            finally
            {
                root.Remove(lengthyView);
                root.styleSheets.Remove(styleSheet);
            }
        }
    }
}