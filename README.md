# Lengthy
[![Releases](https://img.shields.io/github/release/AndanteTribe/Lengthy.svg)](https://github.com/AndanteTribe/Lengthy/releases)
[![GitHub license](https://img.shields.io/github/license/AndanteTribe/Lengthy.svg)](./LICENSE)
[![openupm](https://img.shields.io/npm/v/jp.andantetribe.lengthy?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/jp.andantetribe.lengthy/)
[![Unity](https://img.shields.io/badge/Unity-6000.0%2B-black?logo=unity)](https://unity.com/releases/unity-6)

English | [日本語](README_JA.md)

## Overview
**Lengthy** is a UI Toolkit library for displaying long-form text in Unity.

<img width="1600" height="900" alt="lengthy-license" src="https://github.com/user-attachments/assets/3383467a-7567-40cf-95f8-9504fb0f44be" />

Display a `TextAsset`, a `string`, or an array of paragraphs, and await completion when the window closes. You can also add a `LengthyView` directly to an existing visual tree.

- Scrollable text with dynamic-height `ListView` virtualization.
- Optional title and a close button.
- Built-in USS styling, with support for custom stylesheets.
- Asynchronous completion using `ValueTask`.

## Requirements
- Unity 6.0 (6000.0) or later

## Installation
Open `Window > Package Manager`, select `[+] > Add package from git URL`, and enter:

```text
https://github.com/AndanteTribe/Lengthy.git?path=src/Lengthy.Unity/Packages/jp.andantetribe.lengthy
```

## Quick Start

### Display a TextAsset

Attach this component to a GameObject and assign a text asset in the Inspector. No scene `UIDocument`, UXML, or PanelSettings asset is required for `ShowAsync`.

```csharp
using System;
using Lengthy;
using UnityEngine;

public class LengthySample : MonoBehaviour
{
    [SerializeField]
    private TextAsset _textAsset;

    private async void Start()
    {
        var cancellationToken = destroyCancellationToken;
        try
        {
            await LengthyView.ShowAsync(_textAsset, "Document", cancellationToken);
            Debug.Log("Document closed.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // The component was destroyed while the document was open.
        }
    }
}
```

### Display strings and paragraphs

```csharp
await LengthyView.ShowAsync("A short message.");

await LengthyView.ShowAsync(
    new[] { "First paragraph.", "Second paragraph.", "Third paragraph." },
    title: "Document",
    cancellationToken: destroyCancellationToken);
```

Each `string[]` entry becomes a separate list item. For long documents, split the text into paragraphs to take advantage of virtualization. `TextAsset` and `string` inputs are displayed as a single item; line breaks are not automatically split into items.

### Add to an existing UIDocument

With an existing, configured `UIDocument`:

```csharp
var view = new LengthyView(
    new[] { "First paragraph.", "Second paragraph." },
    title: "Document",
    alwaysIncludeDefaultStyleSheet: true);

document.rootVisualElement.Add(view);

// To remove it programmatically:
// view.RemoveFromHierarchy();
```

The close button removes the view from its parent. The existing `UIDocument` and PanelSettings remain owned by your application.

## API

### `LengthyView.ShowAsync`

All overloads return `ValueTask`.

| Method | Description |
|--------|-------------|
| `ShowAsync(TextAsset textAsset, string title = "", CancellationToken cancellationToken = default)` | Displays the text asset as one item. |
| `ShowAsync(string value, string title = "", CancellationToken cancellationToken = default)` | Displays a string as one item. |
| `ShowAsync(string[] values, string title = "", CancellationToken cancellationToken = default)` | Displays each array entry as a separate paragraph. |

- Closing the window completes the operation normally. Cancellation closes it and throws `OperationCanceledException`.
- The generated GameObject, PanelSettings, and ThemeStyleSheet are destroyed on completion or cancellation.
- The generated GameObject persists across scene changes. Pass a lifetime token such as `destroyCancellationToken` to close it with its caller.
- Call the API and cancel its token on the Unity main thread.
- Await each returned `ValueTask` only once.

### `LengthyView` constructors

The constructors accept `TextAsset`, `string`, or `string[]`, followed by `string title = ""` and `bool alwaysIncludeDefaultStyleSheet = false`.

An empty title hides the title label; the close button remains available. Directly constructed views do not create a `UIDocument` or rendering settings.

## Styling

The bundled `DefaultLengthyUss.uss` imports Unity's default runtime theme. `ShowAsync` uses it with runtime-created PanelSettings and an empty ThemeStyleSheet.

On the view's first panel attachment, the default stylesheet is added to its parent if the parent has no directly attached stylesheets. Set `alwaysIncludeDefaultStyleSheet: true` to insert it before any existing stylesheets and layer your own styles on top.

Useful USS classes:

| Class | Element |
|-------|---------|
| `.lengthy-root` | Root view |
| `.lengthy-window` | Window container |
| `.lengthy-title` | Title label |
| `.lengthy-close-button` | Close button |
| `.lengthy-list` | Paragraph list |
| `.lengthy-paragraph` | Paragraph label |

## License
This library is released under the [MIT license](LICENSE).
