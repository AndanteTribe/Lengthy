#nullable enable

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks.Sources;

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

        public short Version { get; private set; }

        private LengthyValueTaskSource()
        {
        }

        public static LengthyValueTaskSource Create(CancellationToken cancellationToken)
        {
            LengthyValueTaskSource instance;
            if (s_head is null)
            {
                instance = new LengthyValueTaskSource();
                instance.Version = instance._core.Version;
                instance.RegisterCancellation(cancellationToken);
                return instance;
            }

            instance = s_head;
            s_head = instance._next;
            instance._next = null;
            instance.Version = instance._core.Version;
            instance.RegisterCancellation(cancellationToken);
            return instance;
        }

        private void RegisterCancellation(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                SetCancel(cancellationToken);
                return;
            }

            _registration = cancellationToken.UnsafeRegister(static state =>
            {
                var self = (LengthyValueTaskSource)state!;
                self.SetCancel(self._registration.Token);
            }, this);
        }

        public void SetResult()
        {
            if (_core.Version != Version)
            {
                return;
            }
            _core.SetResult(Unit.Default);
        }

        private void SetCancel(CancellationToken cancellationToken)
        {
            if (_core.Version != Version)
            {
                return;
            }
            _core.SetException(new OperationCanceledException(cancellationToken));
        }

        [DebuggerNonUserCode]
        void IValueTaskSource.GetResult(short token)
        {
            try
            {
                _core.GetResult(token);
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
            _core.Reset();
            _next = s_head;
            s_head = this;
        }
    }
}