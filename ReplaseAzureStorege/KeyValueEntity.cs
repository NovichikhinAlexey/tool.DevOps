using Microsoft.WindowsAzure.Storage.Table;

namespace ReplaseAzureStorege
{
    public class KeyValueEntity : TableEntity
    {
        public static string GeneratePartitionKey() => "K";
        public static string GenerateRowKey(string key) => key;

        public string Value { get; set; }
        public OverrideValue[] Override { get; set; }
        public string[] Types { get; set; }
        public bool? IsDuplicated { get; set; }
        public bool? UseNotTaggedValue { get; set; }
        public string[] RepositoryNames { get; set; }
        public bool? HasFullAccess { get; set; }

        public string RepositoryId { get; set; }

        public string Tag { get; set; }

        public string EmptyValueType { get; set; }
    }

    public class OverrideValue
    {
        public string NetworkId { get; set; }
        public string Value { get; set; }
    }
}