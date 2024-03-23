# Neo4j Project Management Integration

![banner live canal DEPLOY](./docs/Neo4j.png)

[Link da Live no youtube](https://www.youtube.com/watch?v=YYRO2OhRMs0&ab_channel=D.E.P.L.O.Y)

:bowtie:

This project is a .NET Core 8 API designed to integrate Project Management (Azure DevOps) with a Neo4j database, facilitating the synchronization and manipulation of data between Azure DevOps projects and a Neo4j graph database.

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes.

### Prerequisites

- .NET Core 8 SDK
- Neo4j Database
- Azure DevOps account

### Installing

1. Clone the repository to your local machine.
2. Navigate to the project directory.
3. Restore the .NET project dependencies by running:

```
dotnet restore
```

4. Configure the application settings as described in the [Configuration](#configuration) section.

### Configuration

Before running the application, update the appsettings.json or your environment variables with the following configurations to match your Azure DevOps and Neo4j setup:

```json
{
  "Neo4j": {
    "Uri": "",
    "User": "",
    "Password": ""
  },
  "AzureDevOps": {
    "BaseURL": "https://dev.azure.com",
    "OrganizationName": "",
    "ProjectName": "",
    "ProjectId": "",
    "PAT": ""
  }
}
```

Make sure to replace your_neo4j_host, your_neo4j_user, your_neo4j_password, your_tenant_id, your_client_id, your_project_id, and your_organization with your actual Neo4j and Azure DevOps information.

### Running the Application

To start the application, run:

```
dotnet run --launch-profile https
```

This will start the API on a local server (usually http://localhost:5176 and https://localhost:7168).

## Authors

Eliézer Zarpelão

## Refactory by

Felipe Augusto
