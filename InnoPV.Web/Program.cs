using InnoPV.Domain.Identity;
using InnoPV.Infrastructure.Data;
using InnoPV.Infrastructure.Seed;
using InnoPV.Web.Services.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using InnoPV.Web.Services.Email;
using InnoPV.Web.Settings;
using InnoPV.Web.Services.SlaAlerts;
using InnoPV.Web.Services.CaseClosure;
using InnoPV.Web.Services.DuplicateCheck;
using InnoPV.Web.Middleware;
using QuestPDF.Infrastructure;
using InnoPV.Web.Services.CaseCompleteReport;
using InnoPV.Web.Services.CaseIntake;
using InnoPV.Web.Services.Workflow;
using InnoPV.Web.Services.SubmissionValidation;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DefaultConnection not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
        sqlOptions.EnableRetryOnFailure());
});

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;

        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;

        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedEmail = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";

    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;

    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole(AppRoles.Admin));

    options.AddPolicy("CanCreateCase", policy =>
        policy.RequireRole(AppRoles.Admin, AppRoles.PvAssociate));

    options.AddPolicy("CanReviewCase", policy =>
        policy.RequireRole(AppRoles.Admin, AppRoles.PvManager));

    options.AddPolicy("CanMedicalReview", policy =>
        policy.RequireRole(AppRoles.Admin, AppRoles.MedicalReviewer));

    options.AddPolicy("CanViewAuditTrail", policy =>
        policy.RequireRole(AppRoles.Admin, AppRoles.PvManager));
});

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<ICaseClosureValidationService, CaseClosureValidationService>();

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddScoped<IAppEmailSender, AppEmailSender>();

builder.Services.Configure<SlaAlertSettings>(
    builder.Configuration.GetSection("SlaAlertSettings"));

builder.Services.AddScoped<ISlaAlertService, SlaAlertService>();

builder.Services.AddHostedService<SlaAlertHostedService>();
builder.Services.AddScoped<IRolePermissionMatrixService, RolePermissionMatrixService>();
builder.Services.AddScoped<ICaseSecurityService, CaseSecurityService>();

builder.Services.AddScoped<IDuplicateCheckService, DuplicateCheckService>();
builder.Services.AddScoped<ICaseIntakeValidationService, CaseIntakeValidationService>();
builder.Services.AddScoped<ICaseWorkflowTransitionService, CaseWorkflowTransitionService>();
builder.Services.AddScoped<ISubmissionReadinessValidationService, SubmissionReadinessValidationService>();

builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromHours(2);
});

builder.Services.AddScoped<ICaseCompleteReportService, CaseCompleteReportService>();

builder.Services.AddRazorPages();

var app = builder.Build();

await DbInitializer.SeedAsync(app.Services);

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

app.UseMiddleware<ForcePasswordChangeMiddleware>();

app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
