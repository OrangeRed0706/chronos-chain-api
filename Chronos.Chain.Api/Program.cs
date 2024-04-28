using Chronos.Chain.Api.DbContext;
using Chronos.Chain.Api.Hub;
using Chronos.Chain.Api.Worker;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddHttpClient();
builder.Services.AddCors(options =>
    options.AddPolicy("default", builder =>
    {
        builder
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowAnyOrigin();
    }));

builder.Services.AddHostedService<TaskHandler>();
builder.Services.AddDbContext<ChronosDbContext>(optionsBuilder =>
{
    var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "chronos.db");
    optionsBuilder.UseSqlite($"Data Source={dbPath}");
});

var app = builder.Build();
app.UseCors("default");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapHub<ChatHub>("/hub");
app.MapControllers();

app.Run();
