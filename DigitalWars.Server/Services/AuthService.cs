using backend.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace backend.Services
{
    public interface IAuthService
    {
        Task<(User user, List<Claim> claims)> ValidateUserCredentialsAsync(string username, string password);
        Task RegisterUserAsync(string username, string email, string password);
        Task ConfirmUserEmailAsync(string token);
        Task InitiatePasswordResetAsync(string email);
        Task<bool> IsPasswordResetTokenValidAsync(string token);
        Task ResetPasswordAsync(string token, string newPassword);
        Task<User?> GetUserByIdAsync(int userId);
    }

    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IProvisioningService _provisioningService;

        public AuthService(AppDbContext context, IEmailService emailService, IProvisioningService provisioningService)
        {
            _context = context;
            _emailService = emailService;
            _provisioningService = provisioningService;
        }

        public async Task<(User user, List<Claim> claims)> ValidateUserCredentialsAsync(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == username || u.Names == username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
                throw new Exception("Nieprawidłowe dane logowania.");

            if (!user.Email_Confirmed)
            {
                user.Link_Token = Guid.NewGuid().ToString();
                user.Token_Expire_Date = DateTime.UtcNow.AddMinutes(15);
                await _context.SaveChangesAsync();
                await _emailService.SendConfirmationEmailAsync(user.Email, user.Link_Token, user.Token_Expire_Date);
                throw new Exception("E-mail nie został potwierdzony. Wysłano ponownie link aktywacyjny.");
            }

            var claims = JwtService.GenerateToken(user);
            return (user, claims);
        }

        public async Task RegisterUserAsync(string username, string email, string password)
        {
            if (await _context.Users.AnyAsync(u => u.Email == email))
                throw new Exception("Email już istnieje w systemie.");

            var user = new User
            {
                Names = username,
                Email = email,
                Password = BCrypt.Net.BCrypt.HashPassword(password),
                Email_Confirmed = false,
                Link_Token = Guid.NewGuid().ToString(),
                Token_Expire_Date = DateTime.UtcNow.AddMinutes(15)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await _provisioningService.InitializeNewUserAsync(user.Users_Id);
            await _emailService.SendConfirmationEmailAsync(user.Email, user.Link_Token, user.Token_Expire_Date);
        }

        public async Task ConfirmUserEmailAsync(string token)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Link_Token == token);
            if (user == null) throw new Exception("Nieprawidłowy lub wygasły token.");
            if (user.Token_Expire_Date < DateTime.UtcNow) throw new Exception("Link aktywacyjny wygasł.");

            user.Email_Confirmed = true;
            user.Link_Token = null;
            user.Token_Expire_Date = null;
            await _context.SaveChangesAsync();
        }

        public async Task InitiatePasswordResetAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user != null)
            {
                user.Link_Token = Guid.NewGuid().ToString();
                user.Token_Expire_Date = DateTime.UtcNow.AddMinutes(15);
                await _context.SaveChangesAsync();
                await _emailService.SendPasswordResetEmailAsync(user.Email, user.Link_Token, user.Token_Expire_Date);
            }
        }

        public async Task<bool> IsPasswordResetTokenValidAsync(string token)
        {
            if (string.IsNullOrEmpty(token)) return false;
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Link_Token == token);
            return user != null && user.Token_Expire_Date >= DateTime.UtcNow;
        }

        public async Task ResetPasswordAsync(string token, string newPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Link_Token == token);
            if (user == null || user.Token_Expire_Date < DateTime.UtcNow)
                throw new Exception("Nieprawidłowy lub wygasły token.");

            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.Link_Token = null;
            user.Token_Expire_Date = null;
            await _context.SaveChangesAsync();
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Users_Id == userId);
        }
    }
}