using Application.Interfaces.ICommand;
using Application.Interfaces.IQuery;
using Application.Interfaces.IRepositories;
using Application.Interfaces.IServices;
using Application.Interfaces.IServices.IAuthServices;
using Application.Interfaces.IServices.ICryptographyService;
using Infrastructure.Command;
using Infrastructure.Persistence;
using Infrastructure.Query;
using Infrastructure.Repositories;
using Infrastructure.Service;
using Infrastructure.Service.NotificationFormatter;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        private static readonly MySqlServerVersion MySqlVersion = new(new Version(8, 0, 0));

        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            }

            services.AddDbContext<AppDbContext>(options =>
                options.UseMySql(connectionString, MySqlVersion)
                    .ConfigureWarnings(warnings =>
                        warnings.Ignore(RelationalEventId.PendingModelChangesWarning)));

            services.AddScoped<IUserCommand, UserCommand>();
            services.AddScoped<IUserQuery, UserQuery>();
            services.AddScoped<IRefreshTokenCommand, RefreshTokenCommand>();
            services.AddScoped<IRefreshTokenQuery, RefreshTokenQuery>();
            services.AddScoped<IPasswordResetCommand, PasswordResetCommand>();
            services.AddScoped<IPasswordResetQuery, PasswordResetQuery>();
            services.AddScoped<IEmailVerificationCommand, EmailVerificationCommand>();
            services.AddScoped<IEmailVerificationQuery, EmailVerificationQuery>();
            services.AddScoped<INotificationRepository, NotificationRepository>();

            services.AddScoped<ICryptographyService, CryptographyService>();
            services.AddScoped<IAuthTokenService, JwtService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IAuthorizationService, AuthorizationService>();
            services.AddHttpClient<IDirectoryProfileProvisioningService, DirectoryProfileProvisioningService>((serviceProvider, client) =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var baseUrl = configuration["Integrations:DirectoryMS:BaseUrl"] ?? "http://localhost:5102/api/";

                if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri))
                {
                    throw new InvalidOperationException("Integrations:DirectoryMS:BaseUrl debe ser una URL absoluta válida.");
                }

                client.BaseAddress = uri;
                client.Timeout = TimeSpan.FromSeconds(
                    configuration.GetValue<int?>("Integrations:DirectoryMS:TimeoutSeconds") ?? 10);

                var internalAccessKey = configuration["Integrations:DirectoryMS:InternalAccessKey"];
                if (!string.IsNullOrWhiteSpace(internalAccessKey))
                {
                    client.DefaultRequestHeaders.Remove("X-Internal-Key");
                    client.DefaultRequestHeaders.Add("X-Internal-Key", internalAccessKey.Trim());
                }
            });
            services.AddSingleton<ITimeProvider, UtcTimeProvider>();
            services.AddSingleton<IRefreshTokenHasher, RefreshTokenHasher>();
            services.AddSingleton<IResetCodeGenerator, ResetCodeGenerator>();

            services.AddSingleton<INotificationFormatter, AppointmentCreatedFormatter>();
            services.AddSingleton<INotificationFormatter, AppointmentCreatedDoctorFormatter>();
            services.AddSingleton<INotificationFormatter, AppointmentConfirmedFormatter>();
            services.AddSingleton<INotificationFormatter, AppointmentConfirmedDoctorFormatter>();
            services.AddSingleton<INotificationFormatter, AppointmentCancelledByPatientFormatter>();
            services.AddSingleton<INotificationFormatter, AppointmentCancelledByPatientDoctorFormatter>();
            services.AddSingleton<INotificationFormatter, AppointmentCancelledByDoctorFormatter>();
            services.AddSingleton<INotificationFormatter, AppointmentCancelledByDoctorDoctorFormatter>();
            services.AddSingleton<INotificationFormatter, AppointmentRescheduledFormatter>();
            services.AddSingleton<INotificationFormatter, AppointmentRescheduledDoctorFormatter>();
            services.AddSingleton<INotificationFormatter, AppointmentReminderFormatter>();
            services.AddSingleton<INotificationFormatter, ConsultationStartedFormatter>();
            services.AddSingleton<INotificationFormatter, MedicationReminderFormatter>();
            services.AddSingleton<INotificationFormatter, PrescriptionReadyFormatter>();
            services.AddSingleton<INotificationFormatter, DefaultNotificationFormatter>();

            services.AddHostedService<NotificationDispatcher>();

            return services;
        }
    }
}
