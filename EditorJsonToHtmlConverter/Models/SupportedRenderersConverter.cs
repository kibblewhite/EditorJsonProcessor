namespace EditorJsonToHtmlConverter.Models;

public class SupportedRenderersConverter : JsonConverter<SupportedRenderers>
{
    public override SupportedRenderers Read(ref Utf8JsonReader reader, Type type_to_convert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            string? enum_string = reader.GetString();
            if (Enum.TryParse(enum_string, true, out SupportedRenderers result))
            {
                return result;
            }
        }

        return default; // or throw an exception if necessary
    }

    public override void Write(Utf8JsonWriter writer, SupportedRenderers value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString().ToLowerInvariant());
}