#nullable enable

using System;

namespace Lengthy
{
    internal readonly struct Unit : IEquatable<Unit>
    {
        public static readonly Unit Default = new Unit();

        public override int GetHashCode()
        {
            return 0;
        }

        public bool Equals(Unit other)
        {
            return true;
        }

        public override string ToString()
        {
            return "()";
        }
    }
}