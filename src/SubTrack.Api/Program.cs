using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SubTrack.Domain.Interfaces;
using SubTrack.Infrastructure.Persistence;
using SubTrack.Api.Services;
using Microsoft.OpenApi;
using Amazon.SimpleEmailV2;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste in: Bearer {your token}"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<SubscriptionService>();
builder.Services.AddScoped<ReminderService>();
builder.Services.AddScoped<IClock, SystemClock>();
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<INotificationSender, ConsoleNotificationSender>();
}
else
{
    builder.Services.AddSingleton<IAmazonSimpleEmailServiceV2>(_ =>
        new AmazonSimpleEmailServiceV2Client(Amazon.RegionEndpoint.USEast2));
    builder.Services.AddScoped<INotificationSender, SesNotificationSender>();
}
// Tells ASP.NET Core how to validate an incoming JWT: which signature to
// check it against, and which issuer/audience/expiry claims to enforce.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Keeps claim names exactly as they were set in GenerateToken ("sub", "email")
        // instead of ASP.NET Core silently remapping "sub" to a long legacy URI.
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),

            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],

            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],

            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () => Results.Ok("Healthy"));

app.Run();
