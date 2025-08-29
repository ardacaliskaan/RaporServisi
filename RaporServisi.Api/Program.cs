using Microsoft.EntityFrameworkCore;
using RaporServisi.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// --- Services ---
// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext (Connection string appsettings.json�dan okunur)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

var app = builder.Build();

// --- Middleware ---
// Swagger sadece Dev�de de�il her ortamda a��k kals�n istersen a�a��daki if�i kald�rabilirsin
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HTTPS redirect (zorunlu de�il ama g�venlik i�in iyi)
app.UseHttpsRedirection();

// Authorization (eklersen burada aktif olur)
app.UseAuthorization();

// Controller route�lar�
app.MapControllers();

app.Run();
