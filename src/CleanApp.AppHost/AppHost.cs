var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.CleanApp>("cleanapp");

builder.Build().Run();
