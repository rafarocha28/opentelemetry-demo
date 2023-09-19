using OpenTelemetry.Exporter.InfluxDB;
using OpenTelemetry.Metrics;
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

builder.Services.AddOpenTelemetry()
    .WithTracing(b => 
    {
        b.AddConsoleExporter()
        .AddSource(serviceName)
        .SetResourceBuilder(
            ResourceBuilder.CreateDefault().AddService(serviceName, serviceVersion: "1.0.0")
        )        
        .AddAspNetCoreInstrumentation()    
        .AddHttpClientInstrumentation();    
    })
    .WithMetrics(b =>
    {
        var isInfluxActive = config.GetSection("InfluxDB").GetValue<bool>("active");
        if (isInfluxActive)
            b.AddInfluxDBMetricsExporter(options =>
            {
                options.Org = "-";
                options.Bucket = config.GetSection("InfluxDB:bucket").Value;
                options.Token = config.GetSection("InfluxDB:username").Value + ":" + config.GetSection("InfluxDB:password").Value;
                options.Endpoint = new Uri(config.GetSection("InfluxDB:url").Value);
                options.MetricsSchema = MetricsSchema.TelegrafPrometheusV2;
            });
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
