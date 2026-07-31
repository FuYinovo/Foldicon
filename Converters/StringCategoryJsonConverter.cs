using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Foldicon.Struct;

namespace Foldicon.Converters;

/// <summary>
///     Json {"分类名": ["图标1", "图标2"], ...}
///     => ObservableCollection = [ Category{"分类名", ["图标1","图标2"]}, ... ]
/// </summary>
public class StringCategoryJsonConverter : JsonConverter<ObservableCollection<Category<string>>>
{
    public override ObservableCollection<Category<string>>? Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Categories 应为 JSON 对象 {\"分类名\": [...]}");

        var result = new ObservableCollection<Category<string>>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            // 属性名 = 分类名
            var name = reader.GetString()!;

            // 属性值 = 图标文件名数组，逐 token 读取
            reader.Read(); // -> StartArray
            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException($"分类 '{name}' 的值应为字符串数组");

            var items = new List<string>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    break;
                items.Add(reader.GetString()!);
            }

            result.Add(new Category<string> { Name = name, Items = items });
        }

        return result;
    }

    public override void Write(Utf8JsonWriter writer, ObservableCollection<Category<string>>? value,
        JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();
        foreach (var category in value)
        {
            writer.WritePropertyName(category.Name);
            JsonSerializer.Serialize(writer, category.Items, options);
        }

        writer.WriteEndObject();
    }
}