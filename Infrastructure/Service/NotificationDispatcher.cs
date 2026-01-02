using Application.Dtos.Notification;
using Application.Interfaces.IQuery;
using Application.Interfaces.IRepositories;
using Application.Interfaces.IServices;
using Application.Interfaces.IServices.IAuthServices;
using Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Service
{
    public class NotificationDispatcher : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IEnumerable<INotificationFormatter> _formatters;
        private readonly ILogger<NotificationDispatcher> _logger;

        public NotificationDispatcher(
            IServiceScopeFactory scopeFactory,
            IEnumerable<INotificationFormatter> formatters,
            ILogger<NotificationDispatcher> logger)
        {
            _scopeFactory = scopeFactory;
            _formatters = formatters;
            _logger = logger;

            _logger.LogWarning(" NotificationDispatcher CONSTRUCTOR ejecutado.");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogWarning("===== NotificationDispatcher INICIADO =====");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation(" Iniciando ciclo de ejecución del dispatcher...");

                try
                {
                    _logger.LogInformation(" Creando nuevo scope de servicios...");
                    using var scope = _scopeFactory.CreateScope();

                    _logger.LogInformation(" Resolviendo dependencias...");

                    var repo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
                    _logger.LogInformation(" NotificationRepository cargado");

                    var userQuery = scope.ServiceProvider.GetRequiredService<IUserQuery>();
                    _logger.LogInformation(" UserQuery cargado");

                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                    _logger.LogInformation(" EmailService cargado");

                    _logger.LogInformation(" Buscando notificaciones pendientes...");
                    var pendings = await repo.GetPending();

                    _logger.LogInformation(" Se encontraron {Count} notificaciones pendientes.", pendings.Count);

                    foreach (var n in pendings)
                    {
                        _logger.LogInformation(" Procesando notificación {NotificationId} (Type: {Type}, UserId: {UserId})",
                            n.NotificationId, n.Type, n.UserId);

                        try
                        {
                            // 1) Buscar usuario
                            _logger.LogInformation(" Buscando usuario con ID {UserId}", n.UserId);
                            var user = await userQuery.GetUserById(n.UserId);

                            if (user == null)
                            {
                                _logger.LogError(" Usuario {UserId} NO encontrado. Marcando notificación {NotificationId} como Failed",
                                    n.UserId, n.NotificationId);

                                n.Status = NotificationStatus.Failed;
                                await repo.Update(n);
                                continue;
                            }

                            _logger.LogInformation(" Usuario encontrado: {FullName} <{Email}>",
                                $"{user.FirstName} {user.LastName}", user.Email);

                            // 2) Obtener formatter
                            _logger.LogInformation(" Buscando formatter para Type={Type}", n.Type);

                            _logger.LogInformation("DEBUG: Formatters registrados:");
                            foreach (var f in _formatters)
                            {
                                _logger.LogInformation("  - {FormatterName}", f.GetType().Name);
                            }

                            _logger.LogInformation("DEBUG: Buscando formatter para Type={Type}", n.Type);

                            var formatter = _formatters.FirstOrDefault(f =>
                            {
                                bool can = f.CanHandle(n.Type);
                                _logger.LogInformation("   → {Formatter} CanHandle({Type}) = {Can}",
                                    f.GetType().Name, n.Type, can);
                                return can;
                            });

                            if (formatter == null)
                            {
                                _logger.LogError(" ERROR: No existe formatter para el tipo {Type}. Marcando Failed", n.Type);
                                n.Status = NotificationStatus.Failed;
                                await repo.Update(n);
                                continue;
                            }

                            _logger.LogInformation(" Formatter encontrado: {Formatter}", formatter.GetType().Name);

                            // 3) Generar body
                            _logger.LogInformation(" Generando contenido del email...");
                            var body = await formatter.FormatAsync(n, user);

                            _logger.LogInformation(" Body generado (primeros 200 chars): {Body}",
                                body.Length > 200 ? body.Substring(0, 200) : body);

                            // 4) Enviar email
                            _logger.LogInformation(" Enviando email a {Email}...", user.Email);
                            await emailService.SendCustomNotification(user.Email, body);
                            _logger.LogInformation(" Email enviado (o simulado) correctamente.");

                            // 5) Actualizar notificación
                            n.Status = NotificationStatus.Sent;
                            n.SentAt = DateTime.Now;

                            _logger.LogInformation("Guardando cambios del estado SENT...");
                        }
                        catch (Exception exInner)
                        {
                            _logger.LogError(exInner,
                                "Error interno procesando notificación {NotificationId}. Marcando Failed",
                                n.NotificationId);

                            n.Status = NotificationStatus.Failed;
                        }

                        await repo.Update(n);
                        _logger.LogInformation("Notificación {NotificationId} actualizada correctamente.", n.NotificationId);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogWarning("CancellationToken solicitado. Deteniendo dispatcher.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ERROR GENERAL en el ciclo del NotificationDispatcher.");
                }

                try
                {
                    _logger.LogInformation("Esperando 10 segundos antes del próximo ciclo...");
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    _logger.LogWarning("Delay cancelado. Finalizando dispatcher.");
                    break;
                }

                _logger.LogInformation("Ciclo del dispatcher finalizado. Preparando siguiente ciclo...");
            }

            _logger.LogWarning("NotificationDispatcher DETENIDO.");
        }
    }
}
