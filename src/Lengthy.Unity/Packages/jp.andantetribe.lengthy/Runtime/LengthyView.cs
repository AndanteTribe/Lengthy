using System;
using UnityEngine;
using UnityEngine.UIElements;

public class LengthyView
{
    private readonly VisualElement _root;
    private Label _headerLabel;
    private ListView _listView;

    public event Action OnCloseClicked;

    public LengthyView(VisualElement root)
    {
        _root = root;
        SetupLayout();
    }

    public void Initialize(string title, int itemCount, Func<int, string> itemProvider)
    {
        if (!string.IsNullOrEmpty(title))
        {
            _headerLabel.text = title;
            _headerLabel.style.display = DisplayStyle.Flex;
        }

        _listView.itemsSource = new int[itemCount];

        _listView.bindItem = (element, i) =>
        {
            ((Label)element).text = itemProvider(i);
        };
    }

    private void SetupLayout()
    {
        _root.style.position = Position.Absolute;
        _root.style.width = Length.Percent(100);
        _root.style.height = Length.Percent(100);
        _root.style.backgroundColor = new Color(0.05f, 0.05f, 0.05f, 1f);
        _root.style.paddingLeft = 50;
        _root.style.paddingRight = 50;
        _root.style.paddingTop = 50;
        _root.style.paddingBottom = 50;

        _headerLabel = new Label();
        _headerLabel.style.color = Color.white;
        _headerLabel.style.fontSize = 36;
        _headerLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        _headerLabel.style.marginBottom = 30;
        _headerLabel.style.display = DisplayStyle.None;
        _root.Add(_headerLabel);

        _listView = new ListView();
        _listView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
        _listView.reorderable = false;
        _listView.style.flexGrow = 1;
        _listView.selectionType = SelectionType.None;

        _listView.makeItem = () =>
        {
            var label = new Label();
            label.style.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            label.style.fontSize = 20;
            label.style.whiteSpace = WhiteSpace.Normal;
            label.enableRichText = true;
            label.style.marginBottom = 15;
            return label;
        };

        _root.Add(_listView);

        var closeButton = new Button(() => OnCloseClicked?.Invoke());
        closeButton.text = "CLOSE";
        closeButton.style.marginTop = 30;
        closeButton.style.height = 50;
        closeButton.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 1f);
        closeButton.style.color = Color.white;
        closeButton.style.borderBottomWidth = 0;
        closeButton.style.borderTopWidth = 0;
        closeButton.style.borderRightWidth = 0;
        closeButton.style.borderLeftWidth = 0;
        _root.Add(closeButton);
    }
}