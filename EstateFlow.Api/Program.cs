using System.Text;
using EstateFlow.Api;
using EstateFlow.Api.Services;
using EstateFlow.Api.Services.Auth;
using EstateFlow.Api.Services.Letters;
using EstateFlow.Api.Services.Notifications;
using EstateFlow.Api.Services.Storage;
using EstateFlow.Core.Interfaces;
using EstateFlow.Core.Models;
using EstateFlow.Data.Context;
using EstateFlow.Data.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ======================================================
// DATABASE
// ======================================================

builder.Services.AddDbContext<EstateFlowDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString(
            "EstateFlowDb")));

// ======================================================
// IDENTITY
// ======================================================

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<EstateFlowDbContext>()
    .AddDefaultTokenProviders();

// ======================================================
// JWT AUTHENTICATION
// ======================================================

var jwtKey =
    builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "JWT key is missing from configuration.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme =
        JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters =
        new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer =
                builder.Configuration["Jwt:Issuer"],

            ValidAudience =
                builder.Configuration["Jwt:Audience"],

            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtKey))
        };
});

// ======================================================
// AUTHORIZATION
// ======================================================

builder.Services.AddAuthorization();

// ======================================================
// REPOSITORIES
// ======================================================

builder.Services.AddScoped<
    IEstateRepository,
    EstateRepository>();

// ======================================================
// ESTATE SERVICES
// ======================================================

builder.Services.AddScoped<
    IEstateService,
    EstateService>();

builder.Services.AddScoped<TokenService>();

builder.Services.AddScoped<EstateFlow.Api.Services.Forms.FormService>();

// ======================================================
// AZURE BLOB STORAGE
// ======================================================

builder.Services.AddScoped<
    IBlobStorageService,
    AzureBlobStorageService>();

// ======================================================
// DESIGN PATTERN SERVICES
// ======================================================

builder.Services.AddSingleton<
    LetterGeneratorFactory>();

builder.Services.AddHttpClient<
    INotificationService,
    SendGridNotificationService>();

// ======================================================
// CONTROLLERS
// ======================================================

builder.Services.AddControllers(options =>
{
    options.Filters.Add<EstateFlow.Api.Filters.EstateOwnershipFilter>();
});

// ======================================================
// SWAGGER
// ======================================================

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "EstateFlow API",
            Version = "v1",
            Description =
                "EstateFlow Estate Administration API"
        });

    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description =
                "Enter your JWT bearer token."
        });

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference =
                        new OpenApiReference
                        {
                            Type =
                                ReferenceType.SecurityScheme,

                            Id = "Bearer"
                        }
                },

                Array.Empty<string>()
            }
        });
});

// ======================================================
// BUILD
// ======================================================

var app = builder.Build();

// ======================================================
// GLOBAL API ERROR HANDLING
// Must be early in the pipeline.
// ======================================================



// ======================================================
// SEED IDENTITY ROLES
// ======================================================

using (var scope = app.Services.CreateScope())
{
    await RoleSeeder.SeedRolesAsync(
        scope.ServiceProvider);
}

// ======================================================
// SWAGGER
// ======================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "EstateFlow API v1");
    });
}

// ======================================================
// HTTP / HTTPS
// ======================================================


app.UseHttpsRedirection();

// ======================================================
// AUTHENTICATION
// ======================================================

app.UseAuthentication();

app.UseAuthorization();

// ======================================================
// AUDIT TRAIL – tell the DbContext who is making this request
// ======================================================

app.Use(async (context, next) =>
{
    AuditContext.CurrentUser =
        context.User.Identity?.Name
        ?? context.User.FindFirst("email")?.Value;

    await next();
});

// ======================================================
// CONTROLLERS
// ======================================================

app.MapControllers();

// ======================================================
// RUN
// ======================================================

app.Run();