using DeployDevOps.Settings;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using Neo4j.Driver;
using System.Net.Http.Headers;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddOptions<Neo4jConfigs>()
    .BindConfiguration("Neo4j")
    .ValidateOnStart()
    .Validate(config =>
    {
        if (config is null 
        || config.Uri is null 
        || config.User is null
        || config.Password is null)
        {
            throw new Exception("Neo4j is not configured");
        }
        return true;
    });

builder.Services
    .AddOptions<AzureDevOpsConfigs>()
    .BindConfiguration("AzureDevOps")
    .ValidateOnStart()
    .Validate(config =>
    {
        if (config is null
        || config.BaseURL is null
        || config.OrganizationName is null
        || config.ProjectName is null
        || config.ProjectId is null
        || config.PAT is null)
        {
            throw new Exception("Azure DevOps is not configured");
        }
        return true;
    });

builder.Services.AddSingleton(GraphDatabase.Driver(
    builder.Configuration.GetValue<string>("Neo4j:Uri"),
    AuthTokens.Basic(
        builder.Configuration.GetValue<string>("Neo4j:User"),
        builder.Configuration.GetValue<string>("Neo4j:Password")
        )
    ));

builder.Services.AddHttpClient("azureDevOps", client =>
{
    client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("AzureDevOps:BaseURL"));
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
               Convert.ToBase64String(Encoding.ASCII.GetBytes($":{builder.Configuration.GetValue<string>("AzureDevOps:PAT")}")));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
