using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using DeployDevOps.Models;
using Microsoft.AspNetCore.Mvc;
namespace DeployDevOps.Controllers;

using System.Configuration;
using System.IO;
using System.Text;
using Neo4j.Driver;

[ApiController]
[Route("[controller]")]
public class DevOpsController : ControllerBase
{

    //
    // The Client ID is used by the application to uniquely identify itself to Azure AD.
    // The Tenant is the name or Id of the Azure AD tenant in which this application is registered.
    // The AAD Instance is the instance of Azure, for example public Azure or Azure China.


    private readonly IConfiguration _config;

    private readonly IDriver _driver;
    private readonly QueryConfig _queryConfig;

    private readonly ILogger<DevOpsController> _logger;

    public DevOpsController(ILogger<DevOpsController> logger, IConfiguration configuration, IDriver driver)
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
        _logger = logger;
        _config = configuration;
        _driver = driver;
    }

    [HttpGet(Name = "GetBacklog")]
    public async Task<bool> GetAsync()
    {

        string aadInstance = _config["AADInstance"];
        string tenant = _config["Tenant"];
        string clientId = _config["ClientId"];
        string projectId = _config["ProjectId"];
        string azureDevOpsOrganizationUrl = _config["OrganizationUrl"];
        try
        {
            using (var client = new HttpClient())
            {
                string personalAccessToken = _config["AzureDevOps:PatKey"];
                string credentials = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", personalAccessToken)));

                client.BaseAddress = new Uri(azureDevOpsOrganizationUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("User-Agent", "msal-neo4j");
                client.DefaultRequestHeaders.Add("X-TFS-FedAuthRedirect", "Suppress");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

                var requestBody = new
                {
                    query = @"Select 
                            [System.Id]
                        From WorkItems 
                        Where [System.TeamProject] = 'Neo4jDemo' AND [State] <> 'Removed' 
                        order by [Microsoft.VSTS.Common.Priority] asc, [System.CreatedDate] desc"
                };

                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                HttpResponseMessage response = client.PostAsync($"{projectId}/_apis/wit/wiql?api-version=7.2-preview.2", content).Result;

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation(response.Content.ReadAsStringAsync().Result);
                    Console.WriteLine("\tSuccesful REST call");

                    var responseJson = JsonSerializer.Deserialize<JsonResultModel.Root>(response.Content.ReadAsStringAsync().Result);

                    var ids = string.Join(",", responseJson.WorkItems.Select(wi => wi.Id).ToArray());

                    HttpResponseMessage responseDetails = client.GetAsync($"{projectId}/_apis/wit/workitems?ids={ids}&api-version=7.2-preview.2").Result;
                    if (responseDetails.IsSuccessStatusCode)
                    {
                        _logger.LogInformation(responseDetails.Content.ReadAsStringAsync().Result);
                        var responseJsonDetails = JsonSerializer.Deserialize<JsonResulDetailsModel.Root>(responseDetails.Content.ReadAsStringAsync().Result);

                        foreach (var value in responseJsonDetails.value)
                        {

                            var queryBoardColumn = "";

                            if (!string.IsNullOrEmpty(value.fields.SystemBoardColumn))
                            {
                                queryBoardColumn = @"MERGE (bc:BoardColumn {key:$fields.SystemBoardColumn})
                                                     MERGE (wi)-[:WORKITEM_LOCATED_IN_BOARD_COLUMN]->(bc)";
                            }
                            var (queryResults, _) = await _driver
                                        .ExecutableQuery(@$"
                                            
                                            MERGE (wi:WorkItem {{key: $id}})
                                            SET wi.workItemTitle = $fields.SystemTitle,
                                                wi.workItemStoryPoints = $fields.MicrosoftVSTSSchedulingStoryPoints

                                            MERGE (user:User {{key:$fields.SystemCreatedBy}})
                                            MERGE (wi)-[:CREATE_BY]->(user) 

                                            WITH wi
                                            CALL apoc.create.addLabels(wi,[$fields.SystemWorkItemType]) YIELD node

                                            WITH wi
                                            {queryBoardColumn}

                                            MERGE (va:ValueArea {{key: $fields.MicrosoftVSTSCommonValueArea}})
                                            MERGE (wi)-[:BELONGS_TO]->(va) 

                                            RETURN true as retorno")
                                        .WithParameters(new { value.id, value.fields })
                                        .WithConfig(_queryConfig)
                                        .ExecuteAsync();
                            Console.WriteLine(queryResults.Select(record => record["retorno"].As<bool>()).FirstOrDefault());
                        }
                        HttpResponseMessage responseLinks = client.GetAsync($"{projectId}/_apis/wit/reporting/workitemlinks?api-version=7.1-preview.3").Result;
                        if (responseLinks.IsSuccessStatusCode)
                        {
                            _logger.LogInformation(responseLinks.Content.ReadAsStringAsync().Result);
                            //System.IO.File.WriteAllText(Environment.CurrentDirectory + "/resultsLinks.json", responseLinks.Content.ReadAsStringAsync().Result);

                            var responseJsonLinks = JsonSerializer.Deserialize<JsonResulLinksModel.Root>(responseLinks.Content.ReadAsStringAsync().Result);

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
                                Console.WriteLine(queryResults.Select(record => record["retorno"].As<bool>()).FirstOrDefault());


                            }
                            //var responseLinks = JsonSerializer.Deserialize<RootLinks>(responseDetails.Content.ReadAsStringAsync().Result);
                        }
                    }

                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException();
                }
                else
                {
                    Console.WriteLine("{0}:{1}", response.StatusCode, response.ReasonPhrase);
                }
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Something went wrong.");
            Console.WriteLine("Message: " + ex.Message + "\n");
        }

        return true;
    }

}
