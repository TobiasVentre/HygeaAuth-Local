using Application.Dtos.Response;
using Application.Interfaces.IServices;
using FluentValidation;
using AuthMS.Api.Security;
using Domain.Entities;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace AuthMS.Api
{
    public static class DependencyInjection
    {
        public const string AllowAllCorsPolicy = "AllowAll";

        public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "AuthMS", Version = "1.0" });
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }
            });

            services.AddFluentValidationAutoValidation(config =>
            {
                config.DisableDataAnnotationsValidation = true;
            });
            services.AddValidatorsFromAssembly(typeof(Application.DependencyInjection).Assembly);

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(entry => entry.Value?.Errors.Count > 0)
                        .SelectMany(entry => entry.Value!.Errors)
                        .Select(error => error.ErrorMessage)
                        .ToArray();

                    return new BadRequestObjectResult(new ApiError
                    {
                        Message = string.Join("; ", errors)
                    });
                };
            });

            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserContext, HttpCurrentUserContext>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                var rawKey = configuration["JwtSettings:key"];
                if (string.IsNullOrWhiteSpace(rawKey))
                {
                    throw new InvalidOperationException("No se encontró 'JwtSettings:key'. Configúralo en User Secrets o Variables de Entorno.");
                }

                var keyBytes = Encoding.UTF8.GetBytes(rawKey);
                if (keyBytes.Length < 32)
                {
                    keyBytes = SHA256.HashData(keyBytes);
                }

                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
                };
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("CanEditOwnProfile", policy => policy.RequireClaim(CustomClaims.CanEditOwnProfile, "true"));
                options.AddPolicy("CanViewTechnicianInfo", policy => policy.RequireClaim(CustomClaims.CanViewTechnicianInfo, "true", "limited"));
                options.AddPolicy("CanViewClientInfo", policy => policy.RequireClaim(CustomClaims.CanViewClientInfo, "true"));
                options.AddPolicy("CanManageAppointments", policy => policy.RequireClaim(CustomClaims.CanManageAppointments, "true"));
                options.AddPolicy("CanManageSchedule", policy => policy.RequireClaim(CustomClaims.CanManageSchedule, "true"));
                options.AddPolicy("CanViewOwnAppointments", policy => policy.RequireClaim(CustomClaims.CanViewOwnAppointments, "true"));
                options.AddPolicy("TechnicianOnly", policy => policy.RequireRole(UserRoles.Technician));
                options.AddPolicy("ClientOnly", policy => policy.RequireRole(UserRoles.Client));
                options.AddPolicy("EmailVerified", policy => policy.RequireClaim(CustomClaims.IsEmailVerified, "true"));
                options.AddPolicy("ActiveUser", policy => policy.RequireClaim(CustomClaims.AccountStatus, "Active"));
            });

            services.AddCors(options =>
            {
                options.AddPolicy(AllowAllCorsPolicy, policy =>
                {
                    policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });

            return services;
        }
    }
}
