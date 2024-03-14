using System.Text.Json.Serialization;

namespace DeployDevOps.Models;

public class JsonResultModel
{
    public class Root
    {
        [JsonPropertyName("workItems")]
        public WorkItem[] WorkItems { get; set; }
    }

    public class WorkItem
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
    }
}