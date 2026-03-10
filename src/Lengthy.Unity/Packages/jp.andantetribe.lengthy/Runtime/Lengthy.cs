using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class Lengthy : MonoBehaviour
{
    private UIDocument _uiDocument;
    private ILengthyDocument _document;
    private LengthyView _view;

    private void Awake()
    {
        _uiDocument = GetComponent<UIDocument>();
    }

    public Lengthy Show(string fileName, string title = "")
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            Debug.LogWarning("Lengthy.Show(fileName): fileName が空です。", this);
            return null;
        }

        string filePath = Path.Combine(Application.streamingAssetsPath, fileName);
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"Lengthy.Show(fileName): ファイルが見つかりません: {filePath}", this);
            return null;
        }

        string text = File.ReadAllText(filePath);
        return Show(new TextAsset(text), title);
    }

    public Lengthy Show(TextAsset textAsset, string title = "")
    {
        if (textAsset == null)
        {
            Debug.LogWarning("Lengthy.Show(textAsset): TextAsset が未設定です。", this);
            return null;
        }

        if (!EnsureReady())
        {
            return null;
        }

        TearDownCurrent();

        _document = new LengthyNativeAssetDocument(textAsset);
        _view = new LengthyView(_uiDocument.rootVisualElement);
        _view.OnCloseClicked += Close;
        _view.Initialize(title, _document.ParagraphCount, _document.ReadParagraph);

        return this;
    }

    private bool EnsureReady()
    {
        if (_uiDocument == null)
        {
            _uiDocument = GetComponent<UIDocument>();
        }

        if (_uiDocument == null)
        {
            Debug.LogError("Lengthy: UIDocument が見つかりません。", this);
            return false;
        }

        if (_uiDocument.panelSettings != null)
        {
            return true;
        }

        var panelSettings = ResolvePanelSettings();
        if (panelSettings == null)
        {
            Debug.LogError("Lengthy を表示するための PanelSettings が見つかりません。UIDocument に PanelSettings を割り当ててください。", this);
            return false;
        }

        _uiDocument.panelSettings = panelSettings;
        return true;
    }

    private PanelSettings ResolvePanelSettings()
    {
        var anyDocument = FindAnyObjectByType<UIDocument>();
        if (anyDocument != null && anyDocument.panelSettings != null)
        {
            return anyDocument.panelSettings;
        }

        var loadedPanelSettings = Resources.FindObjectsOfTypeAll<PanelSettings>();
        return loadedPanelSettings.Length > 0 ? loadedPanelSettings[0] : null;
    }

    public void Hide()
    {
        TearDownCurrent();
    }

    private void Close()
    {
        TearDownCurrent();
    }

    private void TearDownCurrent()
    {
        if (_view != null)
        {
            _view.OnCloseClicked -= Close;
            _view = null;
        }

        _uiDocument?.rootVisualElement.Clear();

        _document?.Dispose();
        _document = null;
    }

    private void OnDestroy()
    {
        TearDownCurrent();
    }
}