using System.Globalization;
using System.Text.Json;
using Amazon.DynamoDBv2.Model;

namespace bard.Utility;

internal static class DictionaryExtensions
{
    public static IList<IDictionary<string, object?>> ToJsonItems(this IEnumerable<IDictionary<string, AttributeValue>> items)
    {
        return items.Select(ToStringObjectDictionary).ToList();
    }

    public static IList<IDictionary<string, AttributeValue?>> ToAwsItems(this IEnumerable<IDictionary<string, object>> items)
    {
        return items.Select(ToStringAttributeValueDictionary).ToList();
    }

    #region Utility Methods

    private static IDictionary<string, AttributeValue?> ToStringAttributeValueDictionary(IDictionary<string, object> item)
    {
        return item.ToDictionary(pair => pair.Key, pair => ToAwsValue(pair.Value));
    }

    private static IDictionary<string, object?> ToStringObjectDictionary(IDictionary<string, AttributeValue> item)
    {
        return item.ToDictionary(pair => pair.Key, pair => ToJsonValue(pair.Value));
    }

    private static object? ToJsonValue(AttributeValue value)
    {
        if (value.NULL)
        {
            return null;
        }

        if (value.IsBOOLSet)
        {
            return value.BOOL;
        }

        if (value.IsMSet)
        {
            return ToStringObjectDictionary(value.M);
        }

        if (value.IsLSet)
        {
            return value.L?.Select(ToJsonValue);
        }

        if (value.S is not null)
        {
            return value.S;
        }

        if (value.N is not null)
        {
            return GetNumber(value.N);
        }

        if (value.B is not null)
        {
            return value.B.ToArray();
        }

        if (value.SS is not null && value.SS.Count > 0)
        {
            return value.SS;
        }

        if (value.BS is not null && value.BS.Count > 0)
        {
            return value.BS;
        }

        if (value.NS is not null && value.NS.Count > 0)
        {
            return value.NS.Select(GetNumber);
        }

        return new List<string>();
    }

    private static object GetNumber(string value)
    {
        return double.Parse(value);
    }

    private static AttributeValue? ToAwsValue(object? value)
    {
        if (value is null)
        {
            return new AttributeValue
            {
                NULL = true
            };
        }

        var jsonElement = (JsonElement)value;
        return jsonElement.ValueKind switch
        {
            JsonValueKind.Null => null,
            JsonValueKind.String => new AttributeValue { S = jsonElement.GetString() },
            JsonValueKind.False => new AttributeValue { BOOL = false, IsBOOLSet = true },
            JsonValueKind.True => new AttributeValue { BOOL = true, IsBOOLSet = true },
            JsonValueKind.Number => new AttributeValue { N = jsonElement.GetDouble().ToString(CultureInfo.InvariantCulture) },
            JsonValueKind.Array => new AttributeValue { L = jsonElement.EnumerateArray().ToArray().Select(e => ToAwsValue(e)).ToList(), IsLSet = true },
            JsonValueKind.Object => new AttributeValue { M = jsonElement.Deserialize< Dictionary<string, object>>()!.ToDictionary(pair => pair.Key, pair => ToAwsValue(pair.Value)), IsMSet = true },
            JsonValueKind.Undefined => new AttributeValue { M = jsonElement.Deserialize<Dictionary<string, object>>()!.ToDictionary(pair => pair.Key, pair => ToAwsValue(pair.Value)), IsMSet = true },
            _ => new AttributeValue { M = jsonElement.Deserialize<Dictionary<string, object>>()!.ToDictionary(pair => pair.Key, pair => ToAwsValue(pair.Value)), IsMSet = true }
        };
    }

    #endregion
}