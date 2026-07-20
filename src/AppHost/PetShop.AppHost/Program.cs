var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.PetShop_Api>("petshop-api");

builder.Build().Run();
