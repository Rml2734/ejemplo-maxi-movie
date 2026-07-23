using maxi_movie_mvc.Data;
using maxi_movie_mvc.Models;
using maxi_movie_mvc.Service;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Google.GenAI;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// --- Configuración Adaptativa de Base de Datos (Local + Railway) ---
var connectionString = builder.Configuration.GetConnectionString("MovieDbContext");

// Si estamos en Railway, detecta la variable de entorno DATABASE_URL de PostgreSQL
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(databaseUrl))
{
    // Transforma la URL tipo 'postgres://user:pass@host:port/db' al formato de Npgsql
    var databaseUri = new Uri(databaseUrl);
    var userInfo = databaseUri.UserInfo.Split(':');
    connectionString = $"Host={databaseUri.Host};Port={databaseUri.Port};Database={databaseUri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true;";
}

builder.Services.AddDbContext<MovieDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add Identity
builder.Services.AddIdentityCore<Usuario>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 3;
    options.Password.RequireUppercase = false;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<MovieDbContext>()
    .AddSignInManager();

// Manejo de la cookie
builder.Services.AddAuthentication(opt =>
{
    opt.DefaultScheme = IdentityConstants.ApplicationScheme;
})
.AddIdentityCookies();

builder.Services.ConfigureApplicationCookie(o =>
{
    o.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    o.SlidingExpiration = true;
    o.LoginPath = "/Usuario/Login";
    o.AccessDeniedPath = "/Usuario/AccessDenied";
});

// Servicios de archivos
builder.Services.AddScoped<ImagenStorage>();
builder.Services.Configure<FormOptions>(o => { o.MultipartBodyLengthLimit = 2 * 1024 * 1024; });

// Servicios de email
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddScoped<IEmailService, SmtpEmailService>();

// Servicio LLM (Gemini)
string geminiApiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? "";

builder.Services.AddChatClient(new Google.GenAI.Client(apiKey: geminiApiKey)
    .AsIChatClient("gemini-2.5-flash"));

builder.Services.AddScoped<LlmService>();

var app = builder.Build();

// Invocar la ejecución de migraciones automáticas y DbSeeder
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<MovieDbContext>();

        // 1. Aplica migraciones pendientes en PostgreSQL (crea las tablas si no existen)
        await context.Database.MigrateAsync();

        // 2. Ejecuta el sembrado de datos (roles, usuario admin, géneros, etc.)
        var userManager = services.GetRequiredService<UserManager<Usuario>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        await DbSeeder.Seed(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred migration/seeding the DB.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
