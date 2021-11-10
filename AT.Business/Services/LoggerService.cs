using System;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Threading.Tasks;
using AT.Business.Interfaces;
using AT.Business.Models.AppSettings;
using AT.Data;
using AT.Domain;
using AT.Domain.Enums;
using Microsoft.Extensions.Configuration;

namespace AT.Business.Services
{
    /// <summary>LoggerService class.</summary>
    public class LoggerService : ILoggerService
    {
        private readonly string _assemblyVersion = Assembly.GetEntryAssembly().GetName().Version.ToString();

        private readonly SqlContext _context;
        private readonly IConfiguration _configuration;

        private readonly AppSettings _appSettings;

        /// <summary>Initializes a new instance of the <see cref="LoggerService" /> class.</summary>
        /// <param name="context">The context.</param>
        /// <param name="configuration">The configuration.</param>
        public LoggerService(SqlContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;

            _appSettings = _configuration.GetSection("AppSettings").Get<AppSettings>();
        }

        /// <summary>Creates the log.</summary>
        /// <param name="log">The log.</param>
        /// <returns>The task.</returns>
        public async Task CreateLogAsync(Log log)
        {
            try
            {
#if DEBUG
                log.Action = $"[D] {log.Action}";
#endif
                await CreateDbLogAsync(log);

                WriteLogInConsole(log);

                if (_appSettings.SmtpServer.IsActive)
                {
                    await SendEmailAsync(log);
                }
            }
            catch (Exception ex)
            {
                string msg = $"AutoTrader v{_assemblyVersion}, CreateLog(msg), {log.Type} : {log.Action} - {log.Message}";
                Serilog.Log.Information(msg);

                string exMsg = $"AutoTrader v{_assemblyVersion}, CreateLog(ex), {ex}";
                Serilog.Log.Information(exMsg);
            }
        }

        private async Task CreateDbLogAsync(Log log)
        {
            try
            {
                await _context.Logs.AddAsync(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                string msg = $"AutoTrader v{_assemblyVersion}, CreateDbLog(msg), {log.Type} : {log.Action} - {log.Message}";
                Serilog.Log.Information(msg);

                string exMsg = $"AutoTrader v{_assemblyVersion}, CreateDbLog(ex), {ex}";
                Serilog.Log.Information(exMsg);
            }
        }

        private void WriteLogInConsole(Log log)
        {
            try
            {
                string msg = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {log.Type} : {log.Action} - {log.Message}";

                switch (log.Type)
                {
                    case LogType.Info:
                        Console.ForegroundColor = ConsoleColor.DarkGreen;

                        break;

                    case LogType.Warning:
                        Console.BackgroundColor = ConsoleColor.DarkYellow;
                        Console.ForegroundColor = ConsoleColor.White;

                        break;

                    case LogType.Error:
                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.ForegroundColor = ConsoleColor.White;

                        break;

                    default:
                        break;
                }

                Console.WriteLine(msg);

                Console.ResetColor();
            }
            catch (Exception ex)
            {
            }
        }

        private async Task SendEmailAsync(Log log)
        {
            try
            {
                SmtpClient smtp = new SmtpClient
                {
                    Host = _appSettings.SmtpServer.Host,
                    Port = _appSettings.SmtpServer.Port,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_appSettings.SmtpServer.Username, _appSettings.SmtpServer.Password),
                    EnableSsl = true,

                    DeliveryMethod = SmtpDeliveryMethod.Network,
                };

                MailMessage message = new MailMessage
                {
                    From = new MailAddress(_appSettings.SmtpServer.Username, "Auto Trader"),
                    IsBodyHtml = true,

                    Subject = $"{log.Type} - {log.Action} - {log.Message}",
                    Body = log.Message + Environment.NewLine + log.DetailedMessage,
                };
                message.To.Add(_appSettings.SmtpServer.Recipients);

                smtp.Send(message);
            }
            catch (Exception ex)
            {
                var exLog = new Log(LogType.Error, $"AutoTrader v{_assemblyVersion}, SendEmail", ex.Message);
                await CreateDbLogAsync(exLog);
            }
        }
    }
}