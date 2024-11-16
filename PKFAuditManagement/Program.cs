using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PKFAuditManagement;
using PKFAuditManagement.Data;
using PKFAuditManagement.Models;
using PKFAuditManagement.Interface;
using Microsoft.Extensions.Options;
using PKFAuditManagement.Services;
using Amazon.S3;
using Microsoft.Extensions.Caching.Memory;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

// Load the .env file
Env.Load();

// Enable legacy timestamp behavior for Npgsql
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Register HttpClient for dependency injection
builder.Services.AddHttpClient();

// Register IMemoryCache
builder.Services.AddMemoryCache();

// Change options.SignIn.RequireConfirmedAccount = true if you want confirmed account email address
builder.Services.AddDefaultIdentity<CustomUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();  // Ensure Razor Pages are mapped

builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/Error/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;
});

// Load environment variables
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
//var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
var connectionString = builder.Configuration["DefaultConnection"];
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//        options.UseNpgsql(connectionString, npgsqlOptions =>
//            npgsqlOptions.CommandTimeout(300) 
//        ));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();


// Configure SMTP options from environment variables
builder.Services.Configure<SmtpOptions>(options =>
{
    options.Host = builder.Configuration["SMTP_HOST"];
    options.Port = int.Parse(builder.Configuration["SMTP_PORT"] ?? "587");
    options.Username = builder.Configuration["SMTP_USERNAME"];
    options.Password = builder.Configuration["SMTP_PASSWORD"];
    options.EnableSsl = bool.Parse(builder.Configuration["SMTP_ENABLESSL"] ?? "true");
    options.From = builder.Configuration["SMTP_FROM"];
});

// Service registrations
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IS3Service, S3Service>();

// Register OpenAIService with the OpenAI API key
builder.Services.AddScoped<IOpenAIService>(provider =>
{
    var apiKey = builder.Configuration["OPENAI_API_KEY"];
    var memoryCache = provider.GetRequiredService<IMemoryCache>();
    return new OpenAIService(apiKey, memoryCache);
});

// Register EmbeddingService with the OpenAI API key
builder.Services.AddScoped<IEmbeddingService>(provider =>
{
    var apiKey = builder.Configuration["OPENAI_API_KEY"];
    return new EmbeddingService(apiKey);
});

// Register the MongoDB service with both the connection string and the IEmbeddingService
builder.Services.AddScoped<IMongoDBService>(sp =>
{
    var embeddingService = sp.GetRequiredService<IEmbeddingService>(); // Resolve the IEmbeddingService
    var logger = sp.GetRequiredService<ILogger<MongoDBService>>(); // Resolve the ILogger<MongoDBService>
    return new MongoDBService(builder.Configuration["MONGODB_CONNECTION_STRING"], embeddingService, logger); // Pass both parameters to the constructor
});


builder.Services.AddTransient<IEmailSender, EmailSender>(sp =>
{
    var smtpOptions = sp.GetRequiredService<IOptions<SmtpOptions>>().Value;
    return new EmailSender(smtpOptions); 
});

// Add SignInManager service
builder.Services.AddScoped<SignInManager<CustomUser>>();

//AWS S3 Configurations
var awsOptions = builder.Configuration.GetAWSOptions("AWS");
builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonS3>();
builder.Services.AddLogging();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<RoleRedirectMiddleware>();

app.MapGet("/", async context =>
{
    context.Response.Redirect("/Identity/Account/Login");
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

//// Run pending migrations
//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;

//    var context = services.GetRequiredService<ApplicationDbContext>();
//    if (context.Database.GetPendingMigrations().Any())
//    {
//        context.Database.Migrate();
//    }
//}

// Data Seeder for User Roles
using (var scope = app.Services.CreateScope())
{
    // Initialise an instance of the roleManager
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    var roles = new[] { "Admin", "User", "Non-Auditor", "Reviewer" };

    // Iterate through the roles and add them to database if they have not been created
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }
}

// Data Seeder for mapping user to roles
using (var scope = app.Services.CreateScope())
{
    // Initialise an instance of the userManager and roleManager
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<CustomUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    // Admin user details from configuration
    var email = builder.Configuration["ADMIN_ACCOUNT_EMAIL"];
    var password = builder.Configuration["ADMIN_ACCOUNT_PASSWORD"];

    // Validate that email and password are not null or empty
    if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
    {
        throw new ArgumentNullException("Admin email or password is not configured properly.");
    }

    // Check if the Admin role exists, and create it if it doesn't
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        var roleResult = await roleManager.CreateAsync(new IdentityRole("Admin"));
        if (!roleResult.Succeeded)
        {
            throw new Exception($"Failed to create Admin role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
        }
    }

    // Check if the admin user has already been created
    var existingUser = await userManager.FindByEmailAsync(email);
    if (existingUser == null)
    {
        // Set details of the admin user
        var user = new CustomUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true  // You can change this if you require email confirmation
        };

        // Create a new admin account asynchronously
        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            // Assign the Admin role to the new user
            var roleAssignResult = await userManager.AddToRoleAsync(user, "Admin");
            if (!roleAssignResult.Succeeded)
            {
                throw new Exception($"Failed to assign Admin role: {string.Join(", ", roleAssignResult.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            throw new Exception($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }
    else
    {
        Console.WriteLine("Admin user already exists.");
    }
}

app.Run();
