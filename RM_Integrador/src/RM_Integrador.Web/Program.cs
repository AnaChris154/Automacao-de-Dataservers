using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RM_Integrador.Web.Data;
using RM_Integrador.Web.Models;
using RM_Integrador.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ConsumoDS.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuração simples - deixar IIS gerenciar URLs quando em produção
Console.WriteLine($"🚀 Iniciando aplicação...");
Console.WriteLine($"📍 Environment: {builder.Environment.EnvironmentName}");

// Add CORS para sistema híbrido
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
    
    options.AddPolicy("LocalRM", builder =>
    {
        builder
            .WithOrigins("http://localhost:8080", "http://127.0.0.1:8080")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
    
    options.AddPolicy("Hybrid", builder =>
    {
        builder
            .SetIsOriginAllowed(origin =>
            {
                return origin.Contains("localhost") || 
                       origin.Contains("127.0.0.1") ||
                       origin.Contains("192.168.") ||
                       origin.Contains("10.0.") ||
                       origin.Contains("172.16.") ||
                       origin.Contains("5095");
            })
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Add services to the container
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Configure database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        }));

// Add authentication services
builder.Services.AddDefaultIdentity<ApplicationUser>(options => 
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 1;
    options.Password.RequiredUniqueChars = 0;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllersWithViews();

// Configure RMSettings
builder.Services.Configure<RMSettings>(builder.Configuration.GetSection("RMSettings"));

// Register HttpClient and HttpClientFactory
builder.Services.AddHttpClient();

// Register DataServerService
builder.Services.AddScoped<IDataServerService, RM_Integrador.Web.Services.DataServerService>();

// Register additional services
builder.Services.AddScoped<IRMConnectionService, RMConnectionService>();
builder.Services.AddScoped<IDataServerSearchService, DataServerSearchService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Use CORS
app.UseCors("Hybrid");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.MapRazorPages();

// Database initialization
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureCreated();
        Console.WriteLine("✅ Conexão com o banco de dados estabelecida com sucesso!");
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocorreu um erro ao inicializar o banco de dados.");
        throw;
    }
}

// Show startup information
Console.WriteLine($"🚀 Aplicação iniciada - Environment: {app.Environment.EnvironmentName}");
Console.WriteLine("🌐 Servidor web iniciando...");

app.Run();
