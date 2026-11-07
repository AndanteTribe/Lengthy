#nullable enable

using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEditor;

namespace Lengthy.Editor
{
    internal static class StringArrayClipboard
    {
        public static string Serialize(IReadOnlyList<string> values)
        {
            var builder = new StringBuilder("GenericPropertyJSON:{\"name\":\"Array\",\"type\":");
            builder.Append((int)SerializedPropertyType.Generic);
            builder.Append(",\"arraySize\":").Append(values.Count);
            builder.Append(",\"arrayType\":\"string\",\"children\":[{\"name\":\"Array\",\"type\":");
            builder.Append((int)SerializedPropertyType.Generic);
            builder.Append(",\"arraySize\":").Append(values.Count);
            builder.Append(",\"arrayType\":\"string\"");
            builder.Append(",\"children\":[{\"name\":\"size\",\"type\":");
            builder.Append((int)SerializedPropertyType.ArraySize);
            builder.Append(",\"val\":").Append(values.Count).Append('}');
            for (var i = 0; i < values.Count; i++)
            {
                builder.Append(",{\"name\":\"data\",\"type\":").Append((int)SerializedPropertyType.String);
                builder.Append(",\"val\":");
                AppendValue(builder, values[i]);
                builder.Append('}');
            }

            return builder.Append("]}]}").ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void AppendValue(StringBuilder builder, string value)
        {
            builder.Append('"');
            foreach (var character in value)
            {
                switch (character)
                {
                    case '"': builder.Append("\\\""); break;
                    case '\\': builder.Append("\\\\"); break;
                    case '\n': builder.Append("\\n"); break;
                    case '\r': builder.Append("\\r"); break;
                    case '\t': builder.Append("\\t"); break;
                    default:
                        if (char.IsControl(character))
                        {
                            builder.Append("\\u").Append(((int)character).ToString("x4", CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            builder.Append(character);
                        }

                        break;
                }
            }

            builder.Append('"');
        }
    }
}