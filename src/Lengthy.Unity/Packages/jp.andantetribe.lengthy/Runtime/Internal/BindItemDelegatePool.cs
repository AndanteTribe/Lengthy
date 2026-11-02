#nullable enable

using System;
using UnityEngine.UIElements;

namespace Lengthy.Internal
{
    internal static class BindItemDelegatePool
    {
        private static Entry? s_head;

        public static Entry Rent(string[] values)
        {
            var entry = s_head;
            if (entry == null)
            {
                entry = new Entry();
            }
            else
            {
                s_head = entry.Next;
                entry.Next = null;
            }

            entry.Values = values;
            return entry;
        }

        public static void Return(Entry entry)
        {
            entry.Values = null;
            entry.Next = s_head;
            s_head = entry;
        }

        internal sealed class Entry
        {
            public Entry? Next;
            public string[]? Values;
            public readonly Action<VisualElement, int> Callback;

            public Entry() => Callback = Invoke;

            private void Invoke(VisualElement element, int index) => ((Label)element).text = Values![index];
        }
    }
}