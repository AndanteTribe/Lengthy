#nullable enable

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks.Sources;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lengthy.Internal
{
    internal sealed class LengthyValueTaskSource : IValueTaskSource
    {
        private static LengthyValueTaskSource? s_head;
        private LengthyValueTaskSource? _next;

        private ManualResetValueTaskSourceCore<Unit> _core = new()
        {
            RunContinuationsAsynchronously = false
        };

        private CancellationTokenRegistration _registration;
        private CancellationToken _cancellationToken;
        private GameObject? _gameObject;
        private PanelSettings? _panelSettings;
        private ThemeStyleSheet? _themeStyleSheet;
        private bool _completed;

        public short Version { get; private set; }

        private LengthyValueTaskSource()
        {
        }

        public static LengthyValueTaskSource Create(string title, string[] values, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LengthyValueTaskSource instance;

            if (s_head is null)
            {
                instance = new LengthyValueTaskSource();
            }
            else
            {
                instance = s_head;
                s_head = instance._next;
                instance._next = null;
            }

            instance.Version = instance._core.Version;
            instance._cancellationToken = cancellationToken;
            try
            {
                instance._themeStyleSheet = ScriptableObject.CreateInstance<ThemeStyleSheet>();
                instance._panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
                instance._panelSettings.themeStyleSheet = instance._themeStyleSheet;
                instance._gameObject = new GameObject(nameof(LengthyView));
                UnityEngine.Object.DontDestroyOnLoad(instance._gameObject);
                var document = instance._gameObject.AddComponent<UIDocument>();
                document.panelSettings = instance._panelSettings;
                document.rootVisualElement.Add(new LengthyView(values, title, false, instance));

                instance._registration = cancellationToken.UnsafeRegister(static state =>
                {
                    var self = (LengthyValueTaskSource)state!;
                    self.SetCancel(self._cancellationToken);
                }, instance);
            }
            catch
            {
                instance.Reset();
                throw;
            }
            return instance;
        }

        public void SetResult()
        {
            if (_completed || _core.Version != Version)
            {
                return;
            }
            _completed = true;
            Cleanup();
            _core.SetResult(Unit.Default);
        }

        private void SetCancel(CancellationToken cancellationToken)
        {
            if (_completed || _core.Version != Version)
            {
                return;
            }
            _completed = true;
            Cleanup();
            _core.SetException(new OperationCanceledException(cancellationToken));
        }

        [DebuggerNonUserCode]
        void IValueTaskSource.GetResult(short token)
        {
            try
            {
                _ = _core.GetResult(token);
            }
            finally
            {
                Reset();
            }
        }

        ValueTaskSourceStatus IValueTaskSource.GetStatus(short token) => _core.GetStatus(token);

        void IValueTaskSource.OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
            => _core.OnCompleted(continuation, state, token, flags);

        private void Reset()
        {
            _registration.Dispose();
            _registration = default;
            _cancellationToken = default;
            Cleanup();
            _completed = false;
            _core.Reset();
            _next = s_head;
            s_head = this;
        }

        private void Cleanup()
        {
            if (_gameObject != null)
            {
                _gameObject.SetActive(false);
                UnityEngine.Object.Destroy(_gameObject);
            }
            if (_panelSettings != null)
            {
                UnityEngine.Object.Destroy(_panelSettings);
            }
            if (_themeStyleSheet != null)
            {
                UnityEngine.Object.Destroy(_themeStyleSheet);
            }
            _gameObject = null;
            _panelSettings = null;
            _themeStyleSheet = null;
        }
    }
}