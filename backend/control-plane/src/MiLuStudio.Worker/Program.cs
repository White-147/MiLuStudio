using MiLuStudio.Worker;
using MiLuStudio.Application.Production;
using MiLuStudio.Application.Projects;
using MiLuStudio.Infrastructure;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<ProductionJobService>();
builder.Services.AddScoped<ProductionSkillExecutionService>();
builder.Services.AddScoped<SkillEnvelopePersistenceService>();
builder.Services.AddScoped<TaskQueueService>();
builder.Services.AddMiLuStudioInfrastructure(builder.Configuration);
builder.Services.AddHostedService<ProductionWorker>();

var host = builder.Build();
host.Run();
