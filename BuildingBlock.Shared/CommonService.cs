using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlock.Shared
{
    public static class CommonService
    {
        public static async Task<string> GetJwtTokenClaim(string token, string key)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            var claims = jwtToken.Claims.ToList();
            string? value = claims.Where(x => 
            x.Type == key).Select(x => x.Value).FirstOrDefault();

            return value;
        }

        public async static Task<string> SendMail(string toEmail, string subject, string body)
        {
            var smtpServer = "smtp.gmail.com";
            var port = 587; // Use 465 for SSL
            var fromEmail = "testsmartdata06@gmail.com";
            var appPassword = "wplx toho cbvj rpjl";
      
            try
            {
                // Create the MailMessage
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, "SmartData Test"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true // Set to true if your email body is HTML
                };
                mailMessage.To.Add(toEmail);
                mailMessage.To.Add("kamalsinghgaira@smartdatainc.net");

                // Configure the SMTP client
                var smtpClient = new SmtpClient(smtpServer, port)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(fromEmail, appPassword)
                };

                // Send the email
                smtpClient.Send(mailMessage);
                return "Email sent successfully!";
            }
            catch (Exception ex)
            {
                return $"Failed to send email: {ex.Message}";
            }
        }
    }
}
