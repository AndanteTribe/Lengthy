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

        public CancellationTokenRegistration Registration { get; set; }

        public short Version => _core.Version;

        private LengthyValueTaskSource()
        {
        }

        public static LengthyValueTaskSource Create()
        {
            if (s_head is null)
            {
                return new LengthyValueTaskSource();
            }

            var instance = s_head;
            s_head = instance._next;
            instance._next = null;
            return instance;
        }

        public void SetResult() => _core.SetResult(Unit.Default);

        public void SetCancel() => _core.SetException(new OperationCanceledException(Registration.Token));

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
            Registration.Dispose();
            Registration = default;
            _core.Reset();
            _next = s_head;
            s_head = this;
        }
    }
}