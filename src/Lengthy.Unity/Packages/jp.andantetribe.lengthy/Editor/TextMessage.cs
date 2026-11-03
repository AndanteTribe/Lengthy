#nullable enable

using System;
using UnityEngine;

namespace Lengthy.Editor
{
    internal enum TextMessage : byte
    {
        Invalid = 0,
        WindowTitle,
        GenerateArray,
        GenerateString,
        LicensePlaceholder,
        NoPackages
    }

    internal static class TextMessageExtensions
    {
        public static string Localize(this TextMessage message) => (message, Application.systemLanguage) switch
        {
            (TextMessage.WindowTitle, SystemLanguage.Japanese) => "権利表記作成",
            (TextMessage.WindowTitle, _) => "Rights Notation Creator",
            (TextMessage.GenerateArray, SystemLanguage.Japanese) => "string[] で生成してコピー",
            (TextMessage.GenerateArray, _) => "Generate and copy as string[]",
            (TextMessage.GenerateString, SystemLanguage.Japanese) => "string で生成してコピー",
            (TextMessage.GenerateString, _) => "Generate and copy as string",
            (TextMessage.LicensePlaceholder, SystemLanguage.Japanese) => "ライセンス文などを入力",
            (TextMessage.LicensePlaceholder, _) => "Enter license text or other notices",
            (TextMessage.NoPackages, SystemLanguage.Japanese) => "パッケージがありません。",
            (TextMessage.NoPackages, _) => "No packages found.",
            _ => throw new ArgumentOutOfRangeException(nameof(message), message, null)
        };
    }
}
