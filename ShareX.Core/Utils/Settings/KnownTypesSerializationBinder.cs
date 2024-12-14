#region License Information (GPL v3)

/*
    ShareX - A program that allows you to take screenshots and share any file type
    Copyright (c) 2007-2024 ShareX Team

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion License Information (GPL v3)


using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShareX.Core.Utils.Settings;

public class KnownTypesJsonConverter : JsonConverter<object>
{
    public IEnumerable<Type> KnownTypes { get; set; }

    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var typeName = reader.GetString();

        // Find the type that matches the name in KnownTypes
        var matchedType = KnownTypes.SingleOrDefault(t => t.Name == typeName);

        if (matchedType == null)
        {
            throw new JsonException($"Unknown type: {typeName}");
        }

        // Deserialize to the matched type
        return JsonSerializer.Deserialize(ref reader, matchedType, options);
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        // Write just the type name (no assembly info needed)
        var typeName = value.GetType().Name;
        writer.WriteStringValue(typeName);
    }
}

