using TicketApp.Repository;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
// Add services to the container.


builder.Services.AddScoped<IUserRepository>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("Conexion");
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new Exception("Database connection string 'Conexion' is not configured.");
    }
    return new UserRepository(connectionString);
});

builder.Services.AddScoped<IEmailRepository>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("Conexion");

    if (string.IsNullOrEmpty(connectionString))
    {
        throw new Exception("Database connection string 'Conexion' is not configured.");
    }

    return new EmailRepository(connectionString, configuration);
});

builder.Services.AddScoped<ITaskRepository>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("Conexion");

    if (string.IsNullOrEmpty(connectionString))
    {
        throw new Exception("Database connection string 'Conexion' is not configured.");
    }

    return new TaskRepository(connectionString, configuration);
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // User session timeout
    options.Cookie.HttpOnly = true; // Prevent JavaScript access
    options.Cookie.IsEssential = true; // Required for session persistence
});
var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
// Ensure default admin user is created on first run



// Enable session for keep the state of the login across the app
builder.Services.AddDistributedMemoryCache();

app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=User}/{action=Login}/{id?}");

app.Run();
