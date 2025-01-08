using System;
using System.Net;
using System.Net.Mail;

public class EmailService
{
    public void SendEmail(string toEmail, string subject, string body)
    {
        try
        {
            var fromEmail = "admin@yourcompany.com"; // Email gửi đi
            var fromPassword = "YourEmailPassword"; // Mật khẩu email

            var smtp = new SmtpClient
            {
                Host = "smtp.yourmailserver.com", // Thay bằng máy chủ SMTP của bạn
                Port = 587, // Cổng SMTP, thường là 587 hoặc 465
                EnableSsl = true, // Bật SSL
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail, fromPassword)
            };

            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true // Cho phép HTML trong email
            })
            {
                smtp.Send(message);
            }

            Console.WriteLine("Email gửi thành công!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi khi gửi email: {ex.Message}");
        }
    }
}
