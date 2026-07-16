#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lengthy
{
    public static class LengthyViewLauncher
    {
        /// <summary>
        /// ビューの作成
        /// </summary>
        /// <param name="root"></param>
        /// <param name="lengthyUss"></param>
        /// <param name="textAsset"></param>
        /// <param name="title"></param>
        public static async Task ShowLengthyViewAsync(VisualElement root, StyleSheet lengthyUss, TextAsset textAsset, CancellationToken token, string title = "")
        {
            try
            {
                // 表示するよ
                root.styleSheets.Add(lengthyUss);
                var lengthyView = new LengthyView(textAsset, title: title);
                root.Add(lengthyView);

                await Task.Delay(Timeout.Infinite, token);
            }
            catch (OperationCanceledException)
            {
                // 非表示にするよ
            }
        }
    }
}