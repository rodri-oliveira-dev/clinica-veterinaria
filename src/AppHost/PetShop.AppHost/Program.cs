var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume();

var petshopDatabase = postgres.AddDatabase("petshop");

var keycloak = builder.AddKeycloak("keycloak", 8080)
    .WithDataVolume();

builder.AddProject<Projects.PetShop_Api>("petshop-api")
    .WithReference(petshopDatabase)
    .WithReference(keycloak)
    .WaitFor(petshopDatabase)
    .WaitFor(keycloak);

builder.Build().Run();
