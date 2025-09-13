using RaporServisi.Application.Services;
using RaporServisi.Infrastructure.Services;
using RaporServisi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SgkVizite;
using System.ServiceModel;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// SOAP Client Configuration
builder.Services.AddScoped<ViziteGonderClient>(provider =>
{
    var binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport)
    {
        MaxReceivedMessageSize = 10 * 1024 * 1024,
        OpenTimeout = TimeSpan.FromSeconds(60),
        SendTimeout = TimeSpan.FromSeconds(60),
        ReceiveTimeout = TimeSpan.FromSeconds(60)
    };

    var endpoint = new EndpointAddress("https://uyg.sgk.gov.tr/Ws_Vizite/services/ViziteGonder");
    return new ViziteGonderClient(binding, endpoint);
});

// Application Services
builder.Services.AddScoped<IReportService, ReportService>();

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