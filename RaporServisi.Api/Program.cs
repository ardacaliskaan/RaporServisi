using Microsoft.EntityFrameworkCore;
using RaporServisi.Infrastructure.Persistence;
using RaporServisi.Application.Contracts;
using RaporServisi.Infrastructure.External;
using RaporServisi.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// EF Core - SQL Server
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<SgkViziteOptions>(builder.Configuration.GetSection("SgkVizite"));

// MVC Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient<ISgkViziteClient, SgkViziteClient>();
builder.Services.AddHostedService<ReportSyncService>();

builder.Services.AddHttpClient<SgkViziteDirectClient>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
