using System.Configuration;
using Neo4j.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton(GraphDatabase.Driver(
            builder.Configuration.GetValue<string>("NEO4J_URI") ?? "neo4j+s://demo.neo4jlabs.com",
            AuthTokens.Basic(
                builder.Configuration.GetValue<string>("NEO4J_USER") ?? "movies",
                builder.Configuration.GetValue<string>("NEO4J_PASSWORD") ?? "movies"
            )
        ));
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
