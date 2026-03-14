#nullable enable

using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lengthy
{
    /// <summary>
    /// Coordinates document loading and view lifecycle for the Lengthy UI screen.
    /// </summary>
    /// <remarks>
    /// This component expects a <see cref="UIDocument"/> on the same GameObject.
    /// A presenter instance is typically created from a prefab and then configured before calling <see cref="Show"/> or <see cref="TryShow"/>.
    /// </remarks>
    [RequireComponent(typeof(UIDocument))]
    public sealed class LengthyPresenter : MonoBehaviour
    {
        [SerializeField]
        private UIDocument? _uiDocument;

        [Header("Library Defaults")]
        [SerializeField]
        private PanelSettings? _defaultPanelSettings;

        [SerializeField]
        private StyleSheet? _defaultStyleSheet;

        private PanelSettings? _configuredPanelSettings;
        private StyleSheet? _configuredStyleSheet;

        private ILengthyDocument? _document;
        private ILengthyView? _view;

        /// <summary>
        /// Occurs when the presenter closes the current view and is about to destroy its GameObject.
        /// </summary>
        public event Action? Closed;

        private void Start() => _uiDocument = GetComponent<UIDocument>();

        /// <summary>
        /// Configures runtime panel settings for this presenter instance.
        /// </summary>
        /// <param name="panelSettings">The panel settings to use when the attached <see cref="UIDocument"/> has no panel settings assigned.</param>
        public void Configure(PanelSettings panelSettings) => _configuredPanelSettings = panelSettings;

        /// <summary>
        /// Configures a runtime style sheet for this presenter instance.
        /// </summary>
        /// <param name="styleSheet">The style sheet to apply to the root visual element when the view is created.</param>
        public void Configure(StyleSheet styleSheet) => _configuredStyleSheet = styleSheet;

        /// <summary>
        /// Configures runtime panel settings and style sheet for this presenter instance.
        /// </summary>
        /// <param name="panelSettings">The panel settings to use when needed.</param>
        /// <param name="styleSheet">The style sheet to apply when the view is created.</param>
        public void Configure(PanelSettings panelSettings, StyleSheet styleSheet)
        {
            Configure(panelSettings);
            Configure(styleSheet);
        }

        /// <summary>
        /// Attempts to show content from the provided text asset and returns this presenter on success.
        /// </summary>
        /// <param name="textAsset">The source text asset to display.</param>
        /// <param name="title">An optional title shown above the paragraph list.</param>
        /// <returns>This presenter instance when successful; otherwise, <see langword="null"/>.</returns>
        public LengthyPresenter? Show(TextAsset textAsset, string title = "") => TryShow(textAsset, out _, title) ? this : null;

        /// <summary>
        /// Attempts to show content from the provided text asset without throwing on setup failures.
        /// </summary>
        /// <param name="textAsset">The source text asset to display.</param>
        /// <param name="error">When this method returns <see langword="false"/>, contains the reason the operation failed.</param>
        /// <param name="title">An optional title shown above the paragraph list.</param>
        /// <returns><see langword="true"/> when the view is successfully shown; otherwise, <see langword="false"/>.</returns>
        public bool TryShow(TextAsset textAsset, out string error, string title = "")
        {
            error = string.Empty;

            if (textAsset == null)
            {
                error = "TextAsset is null.";
                return false;
            }

            if (!EnsureReady(out error))
            {
                return false;
            }

            TearDownCurrent();

            _document = new LengthyTextAssetDocument(textAsset);
            _view = CreateView(_uiDocument!.rootVisualElement);
            _view.OnCloseClicked += Close;
            _view.Initialize(title, _document.ParagraphCount, _document.ReadParagraph);

            return true;
        }

        /// <summary>
        /// Creates a view instance for the current document root and applies style settings.
        /// </summary>
        /// <param name="root">The root visual element from the attached <see cref="UIDocument"/>.</param>
        /// <returns>A newly created view instance.</returns>
        private ILengthyView CreateView(VisualElement root)
        {
            ApplyStyleSheet(root);
            return new LengthyView(root);
        }

        /// <summary>
        /// Validates required dependencies and resolves panel settings when needed.
        /// </summary>
        /// <param name="error">When this method returns <see langword="false"/>, contains the setup failure reason.</param>
        /// <returns><see langword="true"/> when the presenter is ready to show content; otherwise, <see langword="false"/>.</returns>
        private bool EnsureReady(out string error)
        {
            error = string.Empty;

            if (_uiDocument == null)
            {
                _uiDocument = GetComponent<UIDocument>();
            }

            if (_uiDocument == null)
            {
                error = "UIDocument not found.";
                return false;
            }

            if (_uiDocument.panelSettings != null)
            {
                return true;
            }

            var panelSettings = ResolvePanelSettings();
            if (panelSettings == null)
            {
                error = "PanelSettings not found. Set default PanelSettings or call Configure(panelSettings).";
                return false;
            }

            _uiDocument.panelSettings = panelSettings;
            return true;
        }

        /// <summary>
        /// Resolves panel settings in priority order: configured value, then library default.
        /// </summary>
        /// <returns>The resolved panel settings, or <see langword="null"/> when no candidate is available.</returns>
        private PanelSettings? ResolvePanelSettings()
        {
            if (_configuredPanelSettings != null)
            {
                return _configuredPanelSettings;
            }

            if (_defaultPanelSettings != null)
            {
                return _defaultPanelSettings;
            }

            return null;
        }

        /// <summary>
        /// Closes the currently shown content and destroys this presenter GameObject.
        /// </summary>
        public void Hide() => Close();

        /// <summary>
        /// Performs close processing, raises <see cref="Closed"/>, and schedules GameObject destruction.
        /// </summary>
        private void Close()
        {
            TearDownCurrent();
            Closed?.Invoke();
            Destroy(gameObject);
        }

        /// <summary>
        /// Applies the resolved style sheet to the provided root element if not already applied.
        /// </summary>
        /// <param name="root">The root visual element to style.</param>
        private void ApplyStyleSheet(VisualElement root)
        {
            var styleSheet = ResolveStyleSheet();
            if (styleSheet == null || root.styleSheets.Contains(styleSheet))
            {
                return;
            }

            root.styleSheets.Add(styleSheet);
        }

        /// <summary>
        /// Resolves style sheet settings in priority order: configured value, then library default.
        /// </summary>
        /// <returns>The resolved style sheet, or <see langword="null"/> when no style sheet is configured.</returns>
        private StyleSheet? ResolveStyleSheet()
        {
            if (_configuredStyleSheet != null)
            {
                return _configuredStyleSheet;
            }

            if (_defaultStyleSheet != null)
            {
                return _defaultStyleSheet;
            }

            return null;
        }

        /// <summary>
        /// Releases current view and document resources and clears internal references.
        /// </summary>
        private void TearDownCurrent()
        {
            if (_view != null)
            {
                _view.OnCloseClicked -= Close;
                _view.Dispose();
                _view = null;
            }
            _document?.Dispose();
            _document = null;
        }

        /// <summary>
        /// Ensures resources are released when this component is destroyed externally.
        /// </summary>
        private void OnDestroy() => TearDownCurrent();
    }
}
