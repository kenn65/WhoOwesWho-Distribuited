using System.Net;
using System.Net.Mail;
using WhoOwesWho.MessagingService.Models;
using WhoOwesWho.MessagingService.Services.Base;
using WhoOwesWho.MessagingService.Services.Gateways;
using WhoOwesWho.Shared.Models;

namespace WhoOwesWho.MessagingService.Services
{
    public enum EmailType
    {
        SignUp,
        ResetPassword,
        Authentication
    }

    public interface IEmailMessagingService
    {
        Task<bool> SendEmailAsync(MessagingRequestModel request);
    }
    public class EmailMessagingService(
        IConfiguration configuration, 
        IEncryptionGatewayService encryptionGatewayService
        ) : ServiceBase(configuration), IEmailMessagingService
    {
        public async Task<bool> SendEmailAsync(MessagingRequestModel request)
        {
            var pathAndSubject = await GetTemplateAsync(request.Type!);
            var body = await CreateBodyAsync(pathAndSubject.Path, request.User!, request.Host!, request.Code, request.ForgotPasswordToken);
            try
            {
                using (var smtpClient = new SmtpClient(AppSettings.SmtpServer, AppSettings.SmtpPort))
                {
                    smtpClient.Credentials = new NetworkCredential(AppSettings.SmtpUserName, AppSettings.SmtpPassword);
                    smtpClient.EnableSsl = true;

                    var message = new EmailMessageModel
                    {
                        From = new MailAddress("kennskjellerup@gmail.com", "WhoOwesWho"),
                        Subject = pathAndSubject.Subject,
                        Body = body,
                        IsBodyHtml = true,
                    };
                    message.To.Add(request.User!.EmailAddress!);
                    smtpClient.Send(message);
                }
                return await Task.FromResult(true);
            }
            catch (Exception e)
            {
                throw new Exception($"An error occurred while sending the {pathAndSubject.Subject} e-mail!", e);
            }
        }

        private async Task<(string Path, string Subject)> GetTemplateAsync(string type)
        {
            if (string.Equals(EmailType.SignUp.ToString(), type, StringComparison.InvariantCultureIgnoreCase))
            {
                var result = (AppSettings.SignUpTemplatePath, AppSettings.SignUpTemplateSubject);
                return (await Task.FromResult(result))!;

            }
            else if (string.Equals(EmailType.ResetPassword.ToString(), type, StringComparison.InvariantCultureIgnoreCase))
            {
                var result = (AppSettings.ResetPasswordTemplatePath, AppSettings.ResetPasswordTemplateSubject);
                return (await Task.FromResult(result))!;
            }
            else if (string.Equals(EmailType.Authentication.ToString(), type, StringComparison.InvariantCultureIgnoreCase))
            {
                var result = (AppSettings.AuthenticationTemplatePath, AppSettings.AuthenticationTemplateSubject);
                return (await Task.FromResult(result))!;
            }
            else
            {
                throw new Exception($"The type '{type}' is not supported!");
            }
        }

        private async Task<string> CreateBodyAsync(string path, UserMessageRequestModel entity, string host, string? code = null, string? forgotPasswordToken = null)
        {
            var protectedEmailAddress = string.Empty;
            if (!string.Equals(path, AppSettings.AuthenticationTemplatePath, StringComparison.InvariantCultureIgnoreCase))
            {
                protectedEmailAddress = await encryptionGatewayService.ProtectAsync(entity.EmailAddress!);
            }
            var body = await File.ReadAllTextAsync(path);
            body = body.Replace("#fullname#", entity.FullName);
            body = body.Replace("#encryptedemail#", protectedEmailAddress);
            body = body.Replace("#forgotpasswordtoken#", forgotPasswordToken ?? string.Empty);
            body = body.Replace("#host#", $"{host}");
            body = body.Replace("#code#", code ?? string.Empty);
            return body;
        }
    }
}
