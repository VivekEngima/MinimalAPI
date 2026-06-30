using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using MinimalAPI.Data;
using MinimalAPI.Endpoints;
using MinimalAPI.Endpoints.v1;
using MinimalAPI.Endpoints.v2;
using MinimalAPI.Models;
using MinimalAPI.Models.DTOs;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDBContext>().AddDefaultTokenProviders();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin();
        policy.AllowAnyMethod();
        policy.AllowAnyHeader();
    });


    options.AddPolicy("RestrictedPolicy", policy =>
    {
        policy.WithOrigins(
                "https://myfrontend.com",
                "https://admin.myfrontend.com"
            )
            .WithMethods("GET", "POST")
            .WithHeaders("Content-Type", "Authorization");
    });
});
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
    options.ReportApiVersions = true;
}).AddApiExplorer(option =>
{
    option.GroupNameFormat = "'v'VVV";
    option.SubstituteApiVersionInUrl = true;
});

var apiVersions = new[] { new ApiVersion(1, 0), new ApiVersion(2, 0) };

foreach (var apiVersion in apiVersions)
{
    var versionName = $"v{apiVersion.MajorVersion}";
    var displayName = $"Sample API {apiVersion.ToString()}";
    builder.Services.AddOpenApi(versionName, options =>
    {
        options.AddDocumentTransformer((document, context, cancellationToken) =>
        {
            document.Info.Title = displayName;
            document.Info.Version = versionName;
            document.Info.Description = "Demo Minimal API showing CRUD operations with JWT Authentication";
            document.Components ??= new();
            document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
            {
                ["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Enter JWT Bearer token **only** (without 'Bearer' prefix)"
                }
            };

            // Apply security globally to all endpoints
            document.Security =
            [
                new OpenApiSecurityRequirement
            {
                { new OpenApiSecuritySchemeReference("Bearer"), new List<string>() }
            }
            ];
            return Task.CompletedTask;
        });
    });
}

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services.AddDbContext<ApplicationDBContext>(option =>
option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddValidation();
builder.Services.AddAutoMapper(cfg =>
{
    //category mappings
    cfg.CreateMap<Category, CategoryDTO>().ReverseMap();

    cfg.CreateMap<CategoryCreateDTO, Category>()
        .ForMember(dest => dest.Id, opt => opt.Ignore())
        .ForMember(dest => dest.AddedDate, opt => opt.Ignore());
    cfg.CreateMap<CategoryUpdateDTO, Category>()
        .ForMember(dest => dest.Id, opt => opt.Ignore())
        .ForMember(dest => dest.AddedDate, opt => opt.Ignore());

    // MenuItem mappings
    cfg.CreateMap<MenuItem, MenuItemDTO>()
        .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name));
    cfg.CreateMap<MenuItemCreateDTO, MenuItem>()
        .ForMember(dest => dest.Id, opt => opt.Ignore())
        .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
        .ForMember(dest => dest.Image, opt => opt.Ignore())
        .ForMember(dest => dest.Category, opt => opt.Ignore());
    cfg.CreateMap<MenuItemUpdateDTO, MenuItem>()
        .ForMember(dest => dest.Id, opt => opt.Ignore())
        .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
        .ForMember(dest => dest.Image, opt => opt.Ignore()) // Image updated only via upload endpoint
        .ForMember(dest => dest.Category, opt => opt.Ignore());
});
builder.Services.AddProblemDetails();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["ApiSettings:Secret"]!)),
            ValidateAudience = false,
            ValidateIssuer = false
        };
    });
builder.Services.AddAuthorization();


var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapOpenApi("/openapi/{documentName}.json");
app.MapScalarApiReference(option =>
{
    option.Title = "Sample Minimal API with Versioning";
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    var sortedVersion = provider.ApiVersionDescriptions.OrderBy(v => v.ApiVersion).ToList();

    foreach (var description in sortedVersion)
    {
        var versionName = description.GroupName;
        var versionNumber = description.ApiVersion.ToString();
        var displayName = $"Sample API {versionNumber}";

        option.AddDocument(versionName, displayName, $"/openapi/{versionName}.json");
    }
});
app.UseCors("AllowAll");
app.MapGet("/", () => Results.Redirect("/scalar")).ExcludeFromDescription();

app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();

var versionSet = app.NewApiVersionSet().HasApiVersion(new ApiVersion(1, 0)).HasApiVersion(new ApiVersion(2, 0)).ReportApiVersions().Build();

app.MapAuthEndpoints();
app.MapCategoryEndpoints().WithApiVersionSet(versionSet);
app.MapMenuItemEndpoints().WithApiVersionSet(versionSet);
app.MapMenuItemEndpointsV2().WithApiVersionSet(versionSet);
app.MapCategoryEndpointsV2(versionSet);

app.UseHttpsRedirection();

app.Run();
