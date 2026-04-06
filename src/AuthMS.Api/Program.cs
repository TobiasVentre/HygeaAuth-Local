using ApiDependencyInjection = AuthMS.Api.DependencyInjection;
using Application;
using AuthMS.Api;
using AuthMS.Api.Middleware;
using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

static bool IsEfDesignTime()
{
    return AppDomain.CurrentDomain.GetAssemblies()
        .Any(assembly => string.Equals(
            assembly.GetName().Name,
            "Microsoft.EntityFrameworkCore.Design",
            StringComparison.OrdinalIgnoreCase));
}

var builder = WebApplication.CreateBuilder(args);

#if DEBUG
builder.Configuration.AddUserSecrets<Program>();
#endif

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddApi(builder.Configuration);

var app = builder.Build();

if (!IsEfDesignTime())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Applying database migrations...");
        logger.LogInformation("EF DataSource: {Ds}", dbContext.Database.GetDbConnection().DataSource);
        logger.LogInformation("EF Database: {Db}", dbContext.Database.GetDbConnection().Database);

        dbContext.Database.Migrate();
        logger.LogInformation("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error applying database migrations. The application will continue but the database may not be up to date.");
    }
}

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Configuration.GetValue("Swagger:Enabled", true))
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "AuthMS API v1");
        options.RoutePrefix = "swagger";
    });

    app.MapGet("/", () => Results.Redirect("/swagger"));
}

app.UseCors(ApiDependencyInjection.AllowAllCorsPolicy);

if (app.Configuration.GetValue("Http:UseHttpsRedirection", false))
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "authms" }));
app.MapControllers();
app.Run();
