#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Lengthy.Editor
{
    public sealed class RightsNotationCreator : EditorWindow
    {
        [Serializable]
        private struct Entry : IComparable<Entry>
        {
            public PackageManagerKind Manager;
            public string ID;
            public string Author;
            public string Title;
            public bool Excluded;
            public string Text;

            public Entry(PackageManagerKind manager, string id, string? author, string packageTitle)
            {
                Manager = manager;
                ID = id;
                Author = string.IsNullOrWhiteSpace(author) ? "Other" : author!;
                Title = packageTitle;
                Excluded = false;
                Text = "";
            }

            public readonly int CompareTo(Entry other)
            {
                var managerComparison = ((byte)Manager).CompareTo((byte)other.Manager);
                return managerComparison != 0
                    ? managerComparison
                    : string.Compare(ID, other.ID, StringComparison.Ordinal);
            }
        }

        [SerializeField]
        private List<Entry> _entries = new();

        [MenuItem("Tools/Lengthy/Rights Notation Creator")]
        public static void Open()
        {
            var window = GetWindow<RightsNotationCreator>();
            window.titleContent = new GUIContent(TextMessage.WindowTitle.Localize());
            window.minSize = new Vector2(420, 300);
        }

        public void CreateGUI()
        {
            rootVisualElement.Clear();
            var root = new VisualElement { style = { flexGrow = 1 } };
            rootVisualElement.Add(root);
            var content = new ScrollView
            {
                name = "packageList",
                style = { flexGrow = 1 }
            };
            root.Add(content);
            var footer = new VisualElement()
            {
                style =
                {
                    paddingTop = 8,
                    paddingBottom = 8,
                    paddingLeft = 12,
                    paddingRight = 12,
                    flexShrink = 0
                }
            };
            var arrayButton = new Button
            {
                name = "generateArrayButton",
                text = TextMessage.GenerateArray.Localize(),
                style = { height = 28 }
            };
            arrayButton.RegisterCallback<ClickEvent, RightsNotationCreator>(
                static (_, window) => window.CopyToClipboard(true), this);
            footer.Add(arrayButton);
            var stringButton = new Button
            {
                name = "generateStringButton",
                text = TextMessage.GenerateString.Localize(),
                style = { height = 28 }
            };
            stringButton.RegisterCallback<ClickEvent, RightsNotationCreator>(
                static (_, window) => window.CopyToClipboard(false), this);
            footer.Add(stringButton);
            root.Add(footer);
            RefreshEntries();
            AddSection(content, PackageManagerKind.Upm);
            AddSection(content, PackageManagerKind.NuGet);
        }

        private void CopyToClipboard(bool asArray)
        {
            var values = new List<string>(_entries.Count * 2);
            var entries = _entries.Where(static e => !e.Excluded)
                .OrderBy(static e => e.Author == "Other")
                .ThenBy(static e => e.Author, StringComparer.OrdinalIgnoreCase)
                .ThenBy(static e => e.Title, StringComparer.OrdinalIgnoreCase);
            foreach (var entry in entries)
            {
                values.Add(entry.Title);
                values.Add(entry.Text);
            }

            EditorGUIUtility.systemCopyBuffer = asArray
                ? StringArrayClipboard.Serialize(values)
                : string.Join(Environment.NewLine + Environment.NewLine, values);
        }

        private void RefreshEntries()
        {
            var next = new List<Entry>();
            foreach (var package in PackageInfo.GetAllRegisteredPackages())
            {
                if (package.name.StartsWith("com.unity.", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(package.author?.name, "Unity", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(package.author?.name, "Unity Technologies", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var displayName = string.IsNullOrWhiteSpace(package.displayName) ? package.name : package.displayName;
                next.Add(new Entry(PackageManagerKind.Upm, package.name, package.author?.name, displayName));
            }

#if ENABLE_NUGETFORUNITY
            foreach (var package in NugetForUnity.InstalledPackagesManager.InstalledPackages)
            {
                var author = string.Join(", ", package.Authors.Where(static a => !string.IsNullOrWhiteSpace(a)));
                next.Add(new Entry(PackageManagerKind.NuGet, package.Id, author, package.Id));
            }
#endif

            next.Sort();
            var previousIndex = 0;
            for (var nextIndex = 0; nextIndex < next.Count; nextIndex++)
            {
                var entry = next[nextIndex];
                while (previousIndex < _entries.Count && _entries[previousIndex].CompareTo(entry) < 0)
                {
                    previousIndex++;
                }

                if (previousIndex < _entries.Count && _entries[previousIndex].CompareTo(entry) == 0)
                {
                    var previousEntry = _entries[previousIndex++];
                    entry.Excluded = previousEntry.Excluded;
                    entry.Text = previousEntry.Text;
                    next[nextIndex] = entry;
                }
            }

            _entries = next;

        }

        private void AddSection(VisualElement parent, PackageManagerKind manager)
        {
            var entries = _entries.Select(static (entry, index) => (Entry: entry, Index: index));
            var displayName = "";
            (displayName, entries) = manager switch
            {
                PackageManagerKind.Upm => ("UPM", entries.Where(static e => e.Entry.Manager == PackageManagerKind.Upm)),
                PackageManagerKind.NuGet => (nameof(PackageManagerKind.NuGet),
                    entries.Where(static e => e.Entry.Manager == PackageManagerKind.NuGet)),
                _ => throw new NotSupportedException("Unsupported package manager: " + manager)
            };

            var section = new VisualElement()
            {
                style =
                {
                    marginTop = 12,
                    marginBottom = 12,
                    marginLeft = 12,
                    marginRight = 12
                }
            };
            var heading = new Label(displayName)
            {
                style =
                {
                    fontSize = 20,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginBottom = 8
                }
            };
            section.Add(heading);

            var groupedEntries = entries
                .GroupBy(static e => e.Entry.Author)
                .OrderBy(static g => g.Key == "Other")
                .ThenBy(static g => g.Key, StringComparer.OrdinalIgnoreCase);
            foreach (var group in groupedEntries)
            {
                var author = new Label(group.Key)
                {
                    style =
                    {
                        fontSize = 15,
                        unityFontStyleAndWeight = FontStyle.Bold,
                        marginTop = 12,
                        marginBottom = 6
                    }
                };
                section.Add(author);

                foreach (var (entry, entryIndex) in group.OrderBy(static e => e.Entry.Title,
                             StringComparer.OrdinalIgnoreCase))
                {
                    var row = new VisualElement
                    {
                        style =
                        {
                            marginLeft = 12,
                            marginBottom = 12
                        }
                    };
                    var header = new VisualElement
                    {
                        style =
                        {
                            flexDirection = FlexDirection.Row,
                            alignItems = Align.Center,
                            minHeight = 24
                        }
                    };
                    var titleLabel = new Label(entry.Title)
                    {
                        style =
                        {
                            marginTop = 0,
                            marginBottom = 0,
                            flexGrow = 1,
                            flexShrink = 1,
                            whiteSpace = WhiteSpace.Normal,
                            unityTextAlign = TextAnchor.MiddleLeft,
                            alignSelf = Align.Stretch
                        }
                    };
                    var toggle = new Toggle
                    {
                        value = !entry.Excluded, tooltip = entry.ID,
                        style =
                        {
                            marginTop = 0,
                            marginBottom = 0,
                            flexShrink = 0
                        }
                    };
                    header.Add(toggle);
                    header.Add(titleLabel);
                    row.Add(header);
                    var text = new TextField
                    {
                        multiline = true,
                        value = entry.Text,
                        style =
                        {
                            minHeight = 84,
                            marginTop = 4,
                            display = entry.Excluded ? DisplayStyle.None : DisplayStyle.Flex
                        },
                        textEdition = { placeholder = TextMessage.LicensePlaceholder.Localize() }
                    };
                    toggle.RegisterCallback<ChangeEvent<bool>, (List<Entry> Entries, int Index, TextField Input)>(
                        static (evt, args) =>
                        {
                            var (entries, entryIndex, inputField) = args;
                            var updatedEntry = entries[entryIndex];
                            updatedEntry.Excluded = !evt.newValue;
                            entries[entryIndex] = updatedEntry;
                            inputField.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
                        }, (_entries, entryIndex, text));
                    var input = text.Q(className: "unity-text-field__input");
                    if (input != null)
                    {
                        input.style.whiteSpace = WhiteSpace.Normal;
                        input.style.unityTextAlign = TextAnchor.UpperLeft;
                    }

                    text.RegisterCallback<ChangeEvent<string>, (List<Entry> Entries, int Index)>(static (evt, args) =>
                    {
                        var (entries, entryIndex) = args;
                        var updatedEntry = entries[entryIndex];
                        updatedEntry.Text = evt.newValue;
                        entries[entryIndex] = updatedEntry;
                    }, (_entries, entryIndex));
                    row.Add(text);
                    section.Add(row);
                }
            }

            if (section.childCount == 1)
            {
                section.Add(new Label(TextMessage.NoPackages.Localize()));
            }

            parent.Add(section);
        }
    }
}