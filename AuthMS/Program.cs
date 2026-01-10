using Application.Interfaces.ICommand;
using Application.Interfaces.IQuery;
using Application.Interfaces.IServices.IUserServices;
using Application.UseCase.UserServices;
using Application.Validators;
using FluentValidation.AspNetCore;
using FluentValidation;
using Infrastructure.Command;
using Infrastructure.Persistence;
using Infrastructure.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Application.Interfaces.IServices.ICryptographyService;
using Application.UseCase.CryptographyService;
using Application.Interfaces.IServices.IAuthServices;
using Application.UseCase.AuthServices;
using Infrastructure.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Application.Interfaces.IServices;
using Application.Interfaces.IRepositories;
using Application.UseCase.NotificationServices;
using Infrastructure.Repositories;
using Infrastructure.Service.NotificationFormatter;
using Application.UseCase;
using Domain.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

#if DEBUG
builder.Configuration.AddUserSecrets<Program>();
#endif

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "AuthMS", Version = "1.0" });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});


// Custom            
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseSqlServer(connectionString)
        .ConfigureWarnings(warnings => 
            warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
);

// Services
builder.Services.AddScoped<IUserPostServices, UserPostServices>();
builder.Services.AddScoped<IUserPutServices, UserPutServices>();
builder.Services.AddScoped<IUserGetServices, UserGetServices>();
builder.Services.AddScoped<IUserPatchServices, UserPatchServices>();
builder.Services.AddScoped<ICryptographyService, CryptographyService>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();
builder.Services.AddScoped<IAuthTokenService, JwtService>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<ILogoutService, LogoutService>();
builder.Services.AddSingleton<ITimeProvider, ArgentinaTimeProvider>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddSingleton<IResetCodeGenerator, ResetCodeGenerator>();
builder.Services.AddScoped<IEmailVerificationService, EmailVerificationService>();
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddHostedService<NotificationDispatcher>();
// Formatters para CuidarMed+ (Telemedicina)
//builder.Services.AddSingleton<INotificationFormatter, AppointmentCreatedFormatter>();





//CQRS
builder.Services.AddScoped<IUserCommand, UserCommand>();
builder.Services.AddScoped<IUserQuery, UserQuery>();
builder.Services.AddScoped<IRefreshTokenCommand, RefreshTokenCommand>();
builder.Services.AddScoped<IRefreshTokenQuery, RefreshTokenQuery>();
builder.Services.AddScoped<IPasswordResetCommand, PasswordResetCommand>();
builder.Services.AddScoped<IPasswordResetQuery, PasswordResetQuery>();
builder.Services.AddScoped<IEmailVerificationCommand, EmailVerificationCommand>();
builder.Services.AddScoped<IEmailVerificationQuery, EmailVerificationQuery>();



//validators
builder.Services.AddValidatorsFromAssembly(typeof(UserRequestValidator).Assembly);
builder.Services.AddFluentValidationAutoValidation(config =>
{
    // Configurar para que FluentValidation reemplace la validación automática de ASP.NET Core
    config.DisableDataAnnotationsValidation = true;
});

//TokenConfiguration
var jwtKey = builder.Configuration["JwtSettings:key"];

if (string.IsNullOrEmpty(jwtKey))
{
    throw new Exception("No se encontro 'JwtSettings:key'. Configuralo en User Secrets o Variables de Entorno.");
}

builder.Services.AddAuthentication(config =>
{
    config.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    config.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(config =>
{
    config.RequireHttpsMetadata = false;
    config.SaveToken = true;
    config.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// Configurar políticas de autorización basadas en claims
builder.Services.AddAuthorization(options =>
{
    // Política para editar perfil propio
    options.AddPolicy("CanEditOwnProfile", policy =>
        policy.RequireClaim(CustomClaims.CanEditOwnProfile, "true"));

    // Política para ver información de Fumigadores
    options.AddPolicy("CanViewFumigatorInfo", policy =>
        policy.RequireClaim(CustomClaims.CanViewFumigatorInfo, "true", "limited"));

    // Política para ver información de clientes
    options.AddPolicy("CanViewClientInfo", policy =>
        policy.RequireClaim(CustomClaims.CanViewClientInfo, "true"));

    // Política para gestionar citas
    options.AddPolicy("CanManageAppointments", policy =>
        policy.RequireClaim(CustomClaims.CanManageAppointments, "true"));

    // Política para gestionar agenda
    options.AddPolicy("CanManageSchedule", policy =>
        policy.RequireClaim(CustomClaims.CanManageSchedule, "true"));

    // Política para ver citas propias
    options.AddPolicy("CanViewOwnAppointments", policy =>
        policy.RequireClaim(CustomClaims.CanViewOwnAppointments, "true"));

    // Política para fumigadores únicamente
    options.AddPolicy("FumigatorOnly", policy =>
        policy.RequireRole(UserRoles.Fumigator));

    // Política para clientes únicamente
    options.AddPolicy("ClientOnly", policy =>
        policy.RequireRole(UserRoles.Client));

    // Política para usuarios con email verificado
    options.AddPolicy("EmailVerified", policy =>
        policy.RequireClaim(CustomClaims.IsEmailVerified, "true"));

    // Política para usuarios activos
    options.AddPolicy("ActiveUser", policy =>
        policy.RequireClaim(CustomClaims.AccountStatus, "Active"));
});

//Obtener informacion del claim dentro del service

builder.Services.AddHttpContextAccessor();




//CORS

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        logger.LogInformation("Applying database migrations...");
        logger.LogInformation("EF DataSource: {Ds}", dbContext.Database.GetDbConnection().DataSource);
        logger.LogInformation("EF Database: {Db}", dbContext.Database.GetDbConnection().Database);
        logger.LogInformation("EF ConnectionString: {Cs}", dbContext.Database.GetDbConnection().ConnectionString);

        dbContext.Database.Migrate();
        logger.LogInformation("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error applying database migrations. The application will continue but the database may not be up to date.");
        // No lanzar la excepción para que la aplicación pueda iniciar
        // La migración se puede aplicar manualmente después
    }
}

app.Use(async (context, next) =>
{
    // Contin�a con la solicitud
    await next();

    // Si el estado de la respuesta es 401 (No autorizado), a�ade los encabezados CORS
    if (context.Response.StatusCode == 401)
    {
        context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
        context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE");
        context.Response.Headers.Add("Access-Control-Allow-Headers", "Authorization, Content-Type");

    }
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.UseCors("AllowAll");

app.MapControllers();

app.Run();
