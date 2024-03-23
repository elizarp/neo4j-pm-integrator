using System.Text.Json.Serialization;

namespace DeployDevOps.Models;

public class JsonResulDetailsModel
{
    public class Fields
    {
        [JsonPropertyName("System.AreaPath")]
        public string SystemAreaPath { get; set; }

        [JsonPropertyName("System.TeamProject")]
        public string SystemTeamProject { get; set; }

        [JsonPropertyName("System.IterationPath")]
        public string SystemIterationPath { get; set; }

        [JsonPropertyName("System.WorkItemType")]
        public string SystemWorkItemType { get; set; }

        [JsonPropertyName("System.State")]
        public string SystemState { get; set; }

        [JsonPropertyName("System.Reason")]
        public string SystemReason { get; set; }

        [JsonPropertyName("System.CreatedDate")]
        public DateTime SystemCreatedDate { get; set; }

        [JsonPropertyName("System.CreatedBy")]
        public string SystemCreatedBy { get; set; }

        [JsonPropertyName("System.ChangedDate")]
        public DateTime SystemChangedDate { get; set; }

        [JsonPropertyName("System.ChangedBy")]
        public string SystemChangedBy { get; set; }

        [JsonPropertyName("System.CommentCount")]
        public int SystemCommentCount { get; set; }

        [JsonPropertyName("System.Title")]
        public string SystemTitle { get; set; }

        [JsonPropertyName("System.BoardColumn")]
        public string SystemBoardColumn { get; set; }

        [JsonPropertyName("System.BoardColumnDone")]
        public bool SystemBoardColumnDone { get; set; }

        [JsonPropertyName("Microsoft.VSTS.Scheduling.RemainingWork")]
        public double MicrosoftVSTSSchedulingRemainingWork { get; set; }

        [JsonPropertyName("Microsoft.VSTS.Common.StateChangeDate")]
        public DateTime MicrosoftVSTSCommonStateChangeDate { get; set; }

        [JsonPropertyName("Microsoft.VSTS.Common.Priority")]
        public int MicrosoftVSTSCommonPriority { get; set; }

        [JsonPropertyName("Microsoft.VSTS.Common.ValueArea")]
        public string MicrosoftVSTSCommonValueArea { get; set; }

        [JsonPropertyName("Microsoft.VSTS.Scheduling.Effort")]
        public double MicrosoftVSTSSchedulingEffort { get; set; }

        [JsonPropertyName("Microsoft.VSTS.Scheduling.StoryPoints")]
        public double MicrosoftVSTSSchedulingStoryPoints { get; set; }



        [JsonPropertyName("WEF_EC5F9E46397F450FBE5891ABDE7E1656_Kanban.Column")]
        public string WEF_EC5F9E46397F450FBE5891ABDE7E1656_KanbanColumn { get; set; }

        [JsonPropertyName("WEF_EC5F9E46397F450FBE5891ABDE7E1656_Kanban.Column.Done")]
        public bool WEF_EC5F9E46397F450FBE5891ABDE7E1656_KanbanColumnDone { get; set; }

        [JsonPropertyName("System.Description")]
        public string SystemDescription { get; set; }

        [JsonPropertyName("System.AssignedTo")]
        public string SystemAssignedTo { get; set; }

        [JsonPropertyName("Microsoft.VSTS.Common.ActivatedBy")]
        public string MicrosoftVSTSCommonActivatedBy { get; set; }

        [JsonPropertyName("Microsoft.VSTS.Common.ActivatedDate")]
        public DateTime? MicrosoftVSTSCommonActivatedDate { get; set; }

        [JsonPropertyName("Microsoft.VSTS.Common.ResolvedBy")]
        public string MicrosoftVSTSCommonResolvedBy { get; set; }

        [JsonPropertyName("Microsoft.VSTS.Common.ResolvedDate")]
        public DateTime? MicrosoftVSTSCommonResolvedDate { get; set; }

        [JsonPropertyName("Microsoft.VSTS.Common.StackRank")]
        public double? MicrosoftVSTSCommonStackRank { get; set; }
    }

    public class Root
    {
        public int count { get; set; }
        public List<Value> value { get; set; }
    }

    public class Value
    {
        public int id { get; set; }
        public int rev { get; set; }
        public Fields fields { get; set; }
        public string url { get; set; }
    }

}