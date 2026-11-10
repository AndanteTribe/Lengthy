#nullable enable

using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEditor;

namespace Lengthy.Editor
{
    internal static class StringArrayClipboard
    {
        public static string Serialize(IReadOnlyList<string> values)
        {
            var builder = new StringBuilder("GenericPropertyJSON:{\"name\":\"Array\",\"type\":");
            _ = builder.Append((int)SerializedPropertyType.Generic);
            _ = builder.Append(",\"arraySize\":").Append(values.Count);
            _ = builder.Append(",\"arrayType\":\"string\",\"children\":[{\"name\":\"Array\",\"type\":");
            _ = builder.Append((int)SerializedPropertyType.Generic);
            _ = builder.Append(",\"arraySize\":").Append(values.Count);
            _ = builder.Append(",\"arrayType\":\"string\"");
            _ = builder.Append(",\"children\":[{\"name\":\"size\",\"type\":");
            _ = builder.Append((int)SerializedPropertyType.ArraySize);
            _ = builder.Append(",\"val\":").Append(values.Count).Append('}');
            for (var i = 0; i < values.Count; i++)
            {
                _ = builder.Append(",{\"name\":\"data\",\"type\":").Append((int)SerializedPropertyType.String);
                _ = builder.Append(",\"val\":");
                AppendValue(builder, values[i]);
                _ = builder.Append('}');
            }

            return builder.Append("]}]}").ToString();
        }

        private static void AppendValue(StringBuilder builder, string value)
        {
            _ = builder.Append('"');
            foreach (var character in value)
            {
                _ = character switch
                {
                    '"' => builder.Append("\\\""),
                    '\\' => builder.Append("\\\\"),
                    '\n' => builder.Append("\\n"),
                    '\r' => builder.Append("\\r"),
                    '\t' => builder.Append("\\t"),
                    _ => char.IsControl(character)
                                                ? builder.Append("\\u").Append(((int)character).ToString("x4", CultureInfo.InvariantCulture))
                                                : builder.Append(character),
                };
            }

            _ = builder.Append('"');
        }
    }
}