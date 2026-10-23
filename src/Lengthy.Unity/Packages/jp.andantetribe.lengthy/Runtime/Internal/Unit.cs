#nullable enable

using System;

namespace Lengthy.Internal
{
    internal readonly struct Unit : IEquatable<Unit>
    {
        public static readonly Unit Default = new Unit();

        public override int GetHashCode() => 0;

        public bool Equals(Unit other) => true;

        public override string ToString() => "()";
    }
}