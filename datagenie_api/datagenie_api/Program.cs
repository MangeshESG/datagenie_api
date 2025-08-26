using datagenie_api.Data;
using datagenie_api.Middleware;
using datagenie_api.Utility;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ? Register DapperContext
builder.Services.AddSingleton<DapperContext>();

// ? Register repositories/services
builder.Services.AddDatagenieServices();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseMiddleware<ExceptionLoggingMiddleware>(); // ? Token Blacklist Check

app.UseAuthorization();

app.MapControllers();

app.Run();
