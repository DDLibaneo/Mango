using Mango.Services.EmailAPI.Data;
using Mango.Services.EmailAPI.Models;
using Mango.Services.EmailAPI.Models.Dto;
using Mango.Services.EmailAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Mango.Services.EmailAPI.Services
{
    public class EmailService : IEmailService
    {
        private DbContextOptions<AppDbContext> _dbOptions;

        public EmailService(DbContextOptions<AppDbContext> dbOptions)
        {
            _dbOptions = dbOptions;
        }

        public async Task EmailCartAndLog(CartDto cartDto)
        {
            var message = new StringBuilder();

            message.AppendLine("</br>Cart email requested ");
            message.AppendLine("</br>Total " + cartDto.CartHeader.CartTotal);
            message.AppendLine("</br>");
            message.AppendLine("<ul>");

            foreach (var item in cartDto.CartDetails)
            {
                message.Append("<li>");
                message.Append(item.Product.Name + " x " + item.Count);
                message.Append("</li>");
            }

            message.Append("</ul>");

            await LogAndEmail(message.ToString(), cartDto.CartHeader.Email);
        }

        public async Task RegisterUserEmailAndLog(string email)
        {
            var message = "User registration successful. <br/> Email:  " + email;

            await LogAndEmail(message, "admin@gmail.com");
        }

        private async Task<bool> LogAndEmail(string message, string email)
        {
            try
            {
                var emailLogger = new EmailLogger
                {
                    Email = email,
                    EmailSent = DateTime.Now,
                    Message = message,
                };

                await using var _db = new AppDbContext(_dbOptions);

                await _db.EmailLoggers.AddAsync(emailLogger);

                await _db.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
