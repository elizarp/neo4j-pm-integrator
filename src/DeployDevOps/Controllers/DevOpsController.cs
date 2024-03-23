using DeployDevOps.Models;
using DeployDevOps.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Neo4j.Driver;
using System.Text;
using System.Text.Json;

namespace DeployDevOps.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevOpsController : ControllerBase
{
    private readonly IDriver _driver;
    private readonly QueryConfig _queryConfig;
    private readonly AzureDevOpsConfigs _azureDevOpsConfigs;
    private readonly ILogger<DevOpsController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public DevOpsController(
        ILogger<DevOpsController> logger,
        IOptions<AzureDevOpsConfigs> options,
        IHttpClientFactory httpClientFactory,
        IDriver driver)
    {
        var versionStr = Environment.GetEnvironmentVariable("NEO4J_VERSION") ?? "";
        if (double.TryParse(versionStr, out var version) && version >= 4.0)
        {
            _queryConfig = new QueryConfig(database: Environment.GetEnvironmentVariable("NEO4J_DATABASE") ?? "neo4j");
        }
        else
        {
            _queryConfig = new QueryConfig();
        }

        _azureDevOpsConfigs = options.Value;

        _logger = logger;
        _driver = driver;
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet()]
    public async Task<IActionResult> GetBacklogAsync()
    {
        try
        {
            using (var client = _httpClientFactory.CreateClient("azureDevOps"))
            {
                var requestBody = new
                {
                    query = @$"SELECT 
                                [System.Id]
                            FROM
                                WorkItems 
                            WHERE
                                [System.TeamProject] = '{_azureDevOpsConfigs.ProjectName}'
                                    AND [State] <> 'Removed' 
                            ORDER BY
                                [Microsoft.VSTS.Common.Priority] ASC,
                                [System.CreatedDate] DESC"
                };

                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync($"/{_azureDevOpsConfigs.OrganizationName}/{_azureDevOpsConfigs.ProjectId}/_apis/wit/wiql?api-version=7.2-preview.2", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation(await response.Content.ReadAsStringAsync());

                    var responseJson = JsonSerializer.Deserialize<JsonResultModel.Root>(await response.Content.ReadAsStringAsync());

                    var ids = string.Join(",", responseJson.WorkItems.Select(wi => wi.Id).ToArray());

                    HttpResponseMessage responseDetails = await client.GetAsync($"/{_azureDevOpsConfigs.OrganizationName}/{_azureDevOpsConfigs.ProjectId}/_apis/wit/workitems?ids={ids}&api-version=7.2-preview.2");

                    if (responseDetails.IsSuccessStatusCode)
                    {
                        _logger.LogInformation(await responseDetails.Content.ReadAsStringAsync());
                        var responseJsonDetails = JsonSerializer.Deserialize<JsonResulDetailsModel.Root>(await responseDetails.Content.ReadAsStringAsync());

                        foreach (var value in responseJsonDetails.value)
                        {
                            var queryBoardColumn = "";
                            var queryAssignedTo = "";
                            var queryValueArea = "";

                            if (!string.IsNullOrEmpty(value.fields.SystemBoardColumn))
                            {
                                queryBoardColumn = @"MERGE (bc:BoardColumn {key:$fields.SystemBoardColumn})
                                                     MERGE (wi)-[:WORKITEM_LOCATED_IN_BOARD_COLUMN]->(bc)";
                            }

                            if (!string.IsNullOrEmpty(value.fields.SystemAssignedTo))
                            {
                                queryAssignedTo = @"MERGE (userAssigned:User {key:$fields.SystemAssignedTo})
                                                     MERGE (wi)-[:ASSIGNED_TO]->(userAssigned)";
                            }

                            if (!string.IsNullOrEmpty(value.fields.SystemAssignedTo))
                            {
                                queryValueArea = @"MERGE (va:ValueArea {key: $fields.SystemAssignedTo})
                                                    MERGE (wi)-[:BELONGS_TO]->(va) ";
                            }

                            var (queryResults, _) = await _driver
                                        .ExecutableQuery(@$"
                                            
                                            MERGE (wi:WorkItem {{key: $id}})
                                            SET wi.workItemTitle = $fields.SystemTitle,
                                                wi.workItemStoryPoints = $fields.MicrosoftVSTSSchedulingStoryPoints

                                            MERGE (userCreator:User {{key:$fields.SystemCreatedBy}})
                                            MERGE (wi)-[:CREATE_BY]->(userCreator)

                                            WITH wi
                                            CALL apoc.create.addLabels(wi,[$fields.SystemWorkItemType]) YIELD node

                                            WITH wi                                            

                                            {queryBoardColumn}
                                            
                                            {queryAssignedTo}

                                            {queryValueArea}

                                            RETURN true as retorno")
                                        .WithParameters(new { value.id, value.fields })
                                        .WithConfig(_queryConfig)
                                        .ExecuteAsync();

                            _logger.LogInformation(queryResults.Select(record => record["retorno"].As<bool>()).FirstOrDefault().ToString());
                        }

                        HttpResponseMessage responseLinks = await client.GetAsync($"/{_azureDevOpsConfigs.OrganizationName}/{_azureDevOpsConfigs.ProjectId}/_apis/wit/reporting/workitemlinks?api-version=7.1-preview.3");

                        if (responseLinks.IsSuccessStatusCode)
                        {
                            _logger.LogInformation(await responseLinks.Content.ReadAsStringAsync());

                            var responseJsonLinks = JsonSerializer.Deserialize<JsonResulLinksModel.Root>(await responseLinks.Content.ReadAsStringAsync());

                            foreach (var link in responseJsonLinks.values)
                            {
                                var (queryResults, _) = await _driver
                                        .ExecutableQuery(@$"
                                            MATCH (wiSource:WorkItem {{key: $sourceId}})
                                            MATCH (wiTarget:WorkItem {{key: $targetId}})
                                            MERGE (wiSource)<-[:PART_OF]-(wiTarget)

                                            RETURN true as retorno")
                                        .WithParameters(new { link.attributes.sourceId, link.attributes.targetId })
                                        .WithConfig(_queryConfig)
                                        .ExecuteAsync();

                                _logger.LogInformation(queryResults.Select(record => record["retorno"].As<bool>()).FirstOrDefault().ToString());
                            }
                        }
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return Unauthorized();
                }
                else
                {
                    _logger.LogInformation("{0}:{1}", response.StatusCode, response.ReasonPhrase);
                    return BadRequest();
                }

            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return BadRequest();
        }

        return Ok();
    }
}
