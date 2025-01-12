var builder = DistributedApplication.CreateBuilder(args);

const string testKey = "unit-tests";
bool isUnitTest = args.Contains(testKey);

/*Postgres configuration*/
var username = builder.AddParameter("username", "postgres", secret: true);
var password = builder.AddParameter("password", "P@ssw0rd", secret: true);

var postgres = builder.AddPostgres("postgres", username, password, port: 6379);

/*Discard databsae after tests*/
if (!isUnitTest) postgres.WithVolume("VolumeMount.DesafioBackend-postgres", "/var/lib/postgresql/data");

var postgresDb = postgres.AddDatabase("desafio-backend");

/*Migration configuration*/
var migrationService = builder.AddProject<Projects.DesafioBackend_MigrationService>("migrationService")
    .WithReference(postgresDb)
    .WaitFor(postgresDb);

/*Api configuration*/
var apiService = builder.AddProject<Projects.DesafioBackend_ApiService>("apiservice")
    .WaitFor(postgresDb)
    .WithReference(postgresDb);

/* For Testing */
// Comente a linha abaixo para o aspire subir o https na porta 5001
//if (!isUnitTest) apiService.WithHttpsEndpoint(port: 5001);

builder.Build().Run();