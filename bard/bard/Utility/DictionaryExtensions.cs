using Amazon.DynamoDBv2.Model;
#pragma warning disable CS8603

namespace bard.Utility
{
    internal static class DictionaryExtensions
    {
        public static IEnumerable<IDictionary<string, object>> ToJsonItems(this IEnumerable<IDictionary<string, AttributeValue>> items)
        {
            return items.Select(ToStringObjectDictionary).ToList();
        }

        #region Utility Methods

        private static IDictionary<string, object> ToStringObjectDictionary(IDictionary<string, AttributeValue> item)
        {
            return item.ToDictionary(pair =>
                pair.Key, pair => GetValue(pair.Value));
        }

        private static object GetValue(AttributeValue value)
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
                return value.L?.Select(GetValue);
            }

            if (value.S is not null)
            {
                return value.S;
            }

            if (value.N is not null)
            {
                return int.Parse(value.N);
            }

            if (value.B is not null)
            {
                return value.B;
            }

            if (value.BS is not null && value.BS.Count == 0)
            {
                return value.BS;
            }

            if (value.SS is not null && value.SS.Count == 0)
            {
                return value.SS;
            }

            if (value.NS is not null && value.NS.Count == 0)
            {
                return value.NS.Select(int.Parse);
            }

            return new List<string>();
        }

        #endregion
    }
}
