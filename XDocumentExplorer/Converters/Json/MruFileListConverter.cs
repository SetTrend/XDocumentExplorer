using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

using XDocumentExplorer.Code;

namespace XDocumentExplorer.Converters.Json
{
	public class MruFileListConverter : JsonConverter<MruFileList>
	{
		/// <inheritdoc/>
		public override MruFileList? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartObject || !reader.Read()) throw new JsonException($"Start of {nameof(MruFileList)} object expected.");

			byte capacity = 2;
			List<string> fileNames = [];

			do
			{
				if (reader.TokenType != JsonTokenType.PropertyName) throw new JsonException($"Property name expected.");

				switch (reader.GetString())
				{
					case nameof(MruFileList.Capacity):
						if (!reader.Read() || reader.TokenType != JsonTokenType.Number) throw new JsonException($"MRU file capacity expected.");

						capacity = reader.GetByte();

						if (!reader.Read()) throw new JsonException("End of MRU file capacity expected.");
						break;

					case nameof(MruFileList.MruFiles):
						if (!reader.Read() || reader.TokenType != JsonTokenType.StartArray) throw new JsonException($"MRU file array expected.");

						while (reader.Read() && reader.TokenType == JsonTokenType.String)
						{
							string? filePath = reader.GetString();

							if (!string.IsNullOrWhiteSpace(filePath)) fileNames.Add(filePath);
						}

						if (reader.TokenType != JsonTokenType.EndArray || !reader.Read()) throw new JsonException($"End of MRU file array expected.");
						break;
				}
			} while (reader.TokenType != JsonTokenType.EndObject);

			return new MruFileList(capacity, fileNames);
		}

		/// <inheritdoc/>
		public override void Write(Utf8JsonWriter writer, MruFileList value, JsonSerializerOptions options)
		{
			writer.WriteStartObject();

			writer.WriteNumber(nameof(MruFileList.Capacity), value.Capacity);

			writer.WriteStartArray(nameof(MruFileList.MruFiles));
			foreach (string filePath in value.MruFiles) writer.WriteStringValue(filePath);
			writer.WriteEndArray();

			writer.WriteEndObject();
		}
	}
}
