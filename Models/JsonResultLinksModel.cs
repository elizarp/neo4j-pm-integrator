using System.Text.Json.Serialization;

namespace DeployDevOps.Models;

public class JsonResulLinksModel
{
    public class Attributes
    {
        public string linkType { get; set; }
        public int sourceId { get; set; }
        public int targetId { get; set; }
        public bool isActive { get; set; }
        public DateTime changedDate { get; set; }
        public ChangedBy changedBy { get; set; }
        public string comment { get; set; }
        public string changedOperation { get; set; }
        public string sourceProjectId { get; set; }
        public string targetProjectId { get; set; }
    }

    public class ChangedBy
    {
        public string id { get; set; }
        public string displayName { get; set; }
        public string uniqueName { get; set; }
        public string descriptor { get; set; }
    }

    public class Root
    {
        public List<Value> values { get; set; }
        public string nextLink { get; set; }
        public string continuationToken { get; set; }
        public bool isLastBatch { get; set; }
    }

    public class Value
    {
        public string rel { get; set; }
        public Attributes attributes { get; set; }
    }

}