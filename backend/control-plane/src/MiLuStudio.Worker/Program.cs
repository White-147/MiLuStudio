using MiLuStudio.Worker;
using MiLuStudio.Application.Abstractions;
using MiLuStudio.Application.Production;
using MiLuStudio.Application.Projects;
using MiLuStudio.Infrastructure.Persistence.InMemory;
using MiLuStudio.Infrastructure.Time;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<ProductionJobService>();
builder.Services.AddScoped<TaskQueueService>();
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddSingleton<InMemoryControlPlaneStore>();
builder.Services.AddSingleton<IProjectRepository>(provider => provider.GetRequiredService<InMemoryControlPlaneStore>());
builder.Services.AddSingleton<IProductionJobRepository>(provider => provider.GetRequiredService<InMemoryControlPlaneStore>());
builder.Services.AddHostedService<ProductionWorker>();

var host = builder.Build();
host.Run();
