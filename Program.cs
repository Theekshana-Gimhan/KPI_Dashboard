using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using KPI_Dashboard.Data;
using KPI_Dashboard.Models;
using KPI_Dashboard.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure the database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Identity
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddDefaultUI()
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// Configure logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

// Configure authentication and authorization
builder.Services.AddAuthorization();
builder.Services.AddAuthentication()
    .AddCookie(options =>
    {
        options.AccessDeniedPath = "/Home/AccessDenied";
        options.LoginPath = "/Identity/Account/Login";
    });
builder.Services.AddTransient<IEmailService, EmailService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// Seed roles and users
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        // Seed roles
        string[] roleNames = { "Admin", "Admission", "Visa" };
        foreach (var roleName in roleNames)
        {
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

        // Seed a test Visa user
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
        logger.LogError(ex, "Error during seeding: {Message}", ex.Message);
        throw;
    }
}

app.Run();