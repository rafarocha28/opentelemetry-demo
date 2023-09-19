using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OTM.Options;
using System.Reflection;

var configPath = (Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                  ?? Path.GetDirectoryName(Environment.ProcessPath))
                 ?? Environment.CurrentDirectory;

IConfiguration config = new ConfigurationBuilder()
    .SetBasePath(configPath)
    .AddJsonFile("appsettings.json", optional: false)
    .AddUserSecrets(Assembly.GetExecutingAssembly())
    .Build();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.Configure<TcwOptions>(config.GetSection(TcwOptions.Tcw));

var serviceName = "TCW Service";

builder.Services.AddOpenTelemetryTracing(b => {
    b.AddConsoleExporter()
    .AddSource(serviceName)
    .SetResourceBuilder(
        ResourceBuilder.CreateDefault().AddService(serviceName, serviceVersion: "1.0.0")
    )
    .AddAspNetCoreInstrumentation()    
    .AddHttpClientInstrumentation();    
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
