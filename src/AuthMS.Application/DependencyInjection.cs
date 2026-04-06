using Application.Interfaces.IServices;
using Application.Interfaces.IServices.IAuthServices;
using Application.Interfaces.IServices.ICryptographyService;
using Application.Interfaces.IServices.IUserServices;
using Application.UseCase.AuthServices;
using Application.UseCase.NotificationServices;
using Application.UseCase.UserServices;
using Microsoft.Extensions.DependencyInjection;

namespace Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IUserPostServices, UserPostServices>();
            services.AddScoped<IUserPutServices, UserPutServices>();
            services.AddScoped<IUserGetServices, UserGetServices>();
            services.AddScoped<IUserPatchServices, UserPatchServices>();
            services.AddScoped<IPasswordResetService, PasswordResetService>();
            services.AddScoped<ILoginService, LoginService>();
            services.AddScoped<ILogoutService, LogoutService>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            services.AddScoped<IEmailVerificationService, EmailVerificationService>();
            services.AddScoped<INotificationService, NotificationService>();

            return services;
        }
    }
}
