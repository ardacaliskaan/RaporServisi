using Microsoft.EntityFrameworkCore;
using RaporServisi.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// --- Services ---
// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext (Connection string appsettings.json’dan okunur)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

var app = builder.Build();

// --- Middleware ---
// Swagger sadece Dev’de deðil her ortamda açýk kalsýn istersen aþaðýdaki if’i kaldýrabilirsin
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HTTPS redirect (zorunlu deðil ama güvenlik için iyi)
app.UseHttpsRedirection();

// Authorization (eklersen burada aktif olur)
app.UseAuthorization();

// Controller route’larý
app.MapControllers();

app.Run();
