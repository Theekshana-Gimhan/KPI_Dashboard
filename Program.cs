using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using KPI_Dashboard.Data;
using KPI_Dashboard.Models;
using KPI_Dashboard.Services;

var builder = WebApplication.CreateBuilder(args);

// Add MVC controllers and views support to the service container
builder.Services.AddControllersWithViews();

// Configure the application's database context to use SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure ASP.NET Core Identity for authentication and user management
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    // Set password and sign-in requirements
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddDefaultUI() // Adds default UI for Identity
.AddRoles<IdentityRole>() // Enables role management
.AddEntityFrameworkStores<ApplicationDbContext>(); // Stores identity data in the database

// Configure logging for the application
builder.Services.AddLogging(logging =>
{
    logging.AddConsole(); // Log to the console
    logging.SetMinimumLevel(LogLevel.Information); // Set minimum log level
    logging.AddFilter("Microsoft", LogLevel.Warning); // Only log warnings or higher for Microsoft logs
});

// Configure cookie policy for security
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Strict;
    options.Secure = CookieSecurePolicy.Always;
});

// Configure anti-forgery protection for forms
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

// Add authentication and authorization services
builder.Services.AddAuthorization();
builder.Services.AddAuthentication()
    .AddCookie(options =>
    {
        options.AccessDeniedPath = "/Home/AccessDenied"; // Redirect if access denied
        options.LoginPath = "/Identity/Account/Login";   // Redirect to login page if not authenticated
    });

// Register the email service for dependency injection
builder.Services.AddTransient<IEmailService, EmailService>();

var app = builder.Build();

// Configure the HTTP request pipeline (middleware)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); // Use custom error page in production
    app.UseHsts(); // Enforce HTTP Strict Transport Security
}

app.UseHttpsRedirection(); // Redirect HTTP requests to HTTPS
app.UseStaticFiles();      // Serve static files (css, js, images)

app.UseRouting();          // Enable endpoint routing
app.UseCookiePolicy();     // Apply cookie policy
app.UseAuthentication();   // Enable authentication
app.UseAuthorization();    // Enable authorization

// Set up default route for MVC controllers
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Enable Razor Pages routing (for Identity UI and any Razor Pages)
app.MapRazorPages();

// Seed roles and test users at application startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        // Define the roles to be created
        string[] roleNames = { "Admin", "Admission", "Visa" };
        foreach (var roleName in roleNames)
        {
            // Create the role if it does not exist
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                logger.LogInformation("Creating role: {RoleName}", roleName);
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // Seed a test admin user
        var adminUser = await userManager.FindByEmailAsync("admin@example.com");
        if (adminUser == null)
        {
            adminUser = new ApplicationUser { UserName = "admin@example.com", Email = "admin@example.com", Name = "Admin User" };
            logger.LogInformation("Creating admin user: {Email}", adminUser.Email);
            var result = await userManager.CreateAsync(adminUser, "Admin@123");
            if (result.Succeeded)
            {
                logger.LogInformation("Admin user created successfully");
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
            else
            {
                logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        // Seed a test admission user
        var admissionUser = await userManager.FindByEmailAsync("admission@example.com");
        if (admissionUser == null)
        {
            admissionUser = new ApplicationUser { UserName = "admission@example.com", Email = "admission@example.com", Name = "Admission User" };
            logger.LogInformation("Creating admission user: {Email}", admissionUser.Email);
            var result = await userManager.CreateAsync(admissionUser, "Admission@123");
            if (result.Succeeded)
            {
                logger.LogInformation("Admission user created successfully");
                await userManager.AddToRoleAsync(admissionUser, "Admission");
            }
            else
            {
                logger.LogError("Failed to create admission user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        // Seed a test visa user
        var visaUser = await userManager.FindByEmailAsync("visa@example.com");
        if (visaUser == null)
        {
            visaUser = new ApplicationUser { UserName = "visa@example.com", Email = "visa@example.com", Name = "Visa User" };
            logger.LogInformation("Creating visa user: {Email}", visaUser.Email);
            var result = await userManager.CreateAsync(visaUser, "Visa@123");
            if (result.Succeeded)
            {
                logger.LogInformation("Visa user created successfully");
                await userManager.AddToRoleAsync(visaUser, "Visa");
            }
            else
            {
                logger.LogError("Failed to create visa user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
    catch (Exception ex)
    {
        // Log any errors that occur during seeding
        logger.LogError(ex, "Error during seeding: {Message}", ex.Message);
        throw;
    }
}

app.Run(); // Start the application
