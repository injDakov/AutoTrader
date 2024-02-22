using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Threading.Tasks;
using AT.Business.Interfaces;
using AT.Business.Models.AppSettings;
using AT.Business.Models.Dto;
using AT.Data;
using AT.Domain;
using AT.Domain.Enums;
using AutoMapper;
using HandlebarsDotNet;
using Microsoft.Extensions.Configuration;

namespace AT.Business.Services
{
    public class LoggerService : ILoggerService
    {
        private readonly SqlContext _context;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        private readonly AppSettings _appSettings;

        private readonly string _assemblyVersion = Assembly.GetEntryAssembly().GetName().Version.ToString();

        public LoggerService(SqlContext context, IConfiguration configuration, IMapper mapper)
        {
            _context = context;
            _configuration = configuration;
            _mapper = mapper;

            _appSettings = _configuration.GetSection("AppSettings").Get<AppSettings>();
        }

        public async Task CreateLogAsync(LogDto log)
        {
            var logEntity = _mapper.Map<Log>(log);

            logEntity.Message = $"AT v{_assemblyVersion}, {logEntity.Message}";

            try
            {
#if DEBUG
                log.Event = $"[D] {log.Event}";
                logEntity.Action = log.Action = $"[D] {log.Action}";
#endif

                await CreateDbLogAsync(logEntity);

                WriteLogInConsole(logEntity);

                if (_appSettings.SmtpServer.IsActive)
                {
                    await SendEmailAsync(log);
                }
            }
            catch (Exception ex)
            {
                WriteLogInSerilog("CreateLogAsync", logEntity, ex);
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
                WriteLogInSerilog("CreateDbLogAsync", log, ex);
            }
        }

        private void WriteLogInConsole(Log log)
        {
            try
            {
                string msg = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} UTC Time {log.Type} : {log.Action} - {log.Message}";

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
            catch { }
        }

        private async Task SendEmailAsync(LogDto log)
        {
            try
            {
                var body =
                    $"<strong>{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC Time</strong><br />" +
                    $"<br />";

                var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                Handlebars.RegisterHelper("formatDate", (writer, context, arguments) =>
                {
                    if (arguments.Length != 2)
                    {
                        throw new HandlebarsException("{{formatDate}} helper requires two arguments: date and format.");
                    }

                    if (arguments[0] is DateTime date && arguments[1] is string format)
                    {
                        var formattedDate = date.ToString(format);
                        writer.WriteSafeString(formattedDate);
                    }
                });

                switch (log.SourceType)
                {
                    case Enums.LogSourceType.PlaceOrder:
                        string placeOrderTemplate = File.ReadAllText(@$"{binPath}\Templates\PlaceOrderNotification.html");
                        var placeOrderCompiledTemplate = Handlebars.Compile(placeOrderTemplate);

                        body += placeOrderCompiledTemplate(log.DetailedMessage.NewOrder);

                        break;

                    case Enums.LogSourceType.Default:
                    default:
                        body +=
                            $" {log.DetailedMessage.Text.Replace("   ", "<br />")}<br />";

                        break;
                }

                body +=
                    $"<br />" +
                    $"<strong>AT v{_assemblyVersion}</strong>";

                SmtpClient smtp = new SmtpClient
                {
                    Host = _appSettings.SmtpServer.Host,
                    Port = _appSettings.SmtpServer.Port,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_appSettings.SmtpServer.Username, _appSettings.SmtpServer.Password),
                    EnableSsl = true,

                    DeliveryMethod = SmtpDeliveryMethod.Network,
                };

                var subjectPrefix = log.Type != LogType.Info ? $" {log.Type}" : string.Empty;
                var subject = string.IsNullOrWhiteSpace(log.Event) ? log.Action : log.Event;

                MailMessage message = new MailMessage
                {
                    From = new MailAddress(_appSettings.SmtpServer.Username, "Auto Trader"),
                    IsBodyHtml = true,

                    Subject = $"{subject}{subjectPrefix}",

                    Body = body,
                };
                message.To.Add(_appSettings.SmtpServer.Recipients);

                smtp.Send(message);
            }
            catch (Exception ex)
            {
                var exLog = new Log { Type = LogType.Error, Action = $"SendEmail", Message = ex.Message };
                await CreateDbLogAsync(exLog);
            }
        }

        private void WriteLogInSerilog(string methodName, Log log, Exception ex)
        {
            string msg = $"{methodName}(msg), {log.Type} : {log.Action} - {log.Message}";
            Serilog.Log.Information(msg);

            string exMsg = $"{methodName}(ex), {ex}";
            Serilog.Log.Information(exMsg);
        }
    }
}