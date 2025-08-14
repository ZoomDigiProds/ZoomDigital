using InternalProj.Data;
using InternalProj.Filters;
using InternalProj.Models;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace InternalProj.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailSettings _emailSettings;
        private static readonly HttpClient _httpClient = new HttpClient();

        public AccountController(ApplicationDbContext context, IOptions<EmailSettings> emailSettings)
        {
            _context = context;
            _emailSettings = emailSettings.Value;
        }
        //for invalid user who tries to accesspage without authorization

        [HttpGet]
        public IActionResult LoginLogList() {
            var logs= _context.LoginLogs.ToList();
            return View(logs);
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            var sessionLoginId = HttpContext.Session.GetInt32("LoginHistoryId");
            var cookieLoginId = Request.Cookies["LoginHistoryId"];

            if (sessionLoginId == null && !string.IsNullOrEmpty(cookieLoginId))
            {
                if (int.TryParse(cookieLoginId, out int loginId))
                {
                    var loginRecord = _context.LoginLogs.FirstOrDefault(l => l.Id == loginId);
                    if (loginRecord != null && loginRecord.LogoutTime == null)
                    {
                        loginRecord.LogoutTime = DateTime.UtcNow;
                        loginRecord.Reason = "Session expired";
                        _context.SaveChanges();
                    }

                    Response.Cookies.Delete("LoginHistoryId");
                }
            }

            if (HttpContext.Session.GetString("UserName") != null)
            {
                return RedirectToAction("Index", "StaffReg");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = _context.StaffCredentials.FirstOrDefault(u => u.UserName == username && u.Active == "Y");
            if (user != null)
            {
                var hasher = new PasswordHasher<StaffCredentials>();
                var result = hasher.VerifyHashedPassword(user, user.Password, password);
                if (result == PasswordVerificationResult.Success)
                {
                    string localIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

                    string publicIp = await GetPublicIpAddressAsync();
                    string location = await GetLocationFromIpAsync(publicIp);

                    var loginHistory = new LoginLogs
                    {
                        StaffName = user.UserName,
                        LoginTime = DateTime.UtcNow,
                        Local_IPAddres = localIp,
                        Public_IPAddress = publicIp,
                        Location = location,
                        LogoutTime = null
                    };
                    _context.LoginLogs.Add(loginHistory);
                    _context.SaveChanges();

                    HttpContext.Session.SetInt32("LoginHistoryId", loginHistory.Id);
                    Response.Cookies.Append("LoginHistoryId", loginHistory.Id.ToString(), new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddMinutes(30),
                        HttpOnly = true
                    });


                    HttpContext.Session.SetString("UserName", user.UserName);
                    HttpContext.Session.SetString("StaffId", user.StaffId.ToString());

                    //gettting current staff related departments
                    var userDepartments = _context.StaffDepartments
                        .Where(sd => sd.StaffId == user.StaffId)
                        .Select(sd => sd.Department.Name)
                        .ToList();
                    HttpContext.Session.SetString("UserDepartments", string.Join(",", userDepartments));

                    //gettting current staff related branch
                    var userBranch = _context.StaffRegs
                        .Where(sd => sd.StaffId == user.StaffId)
                        .Select(sd => sd.BranchId.ToString())
                        .FirstOrDefault();
                    HttpContext.Session.SetString("BranchId", userBranch.ToString());
                    
                    
                    TempData["SuccessMessage"] = "Login successful!";

                    if (user.IsFirstLogin == true)
                    {
                        HttpContext.Session.SetString("IsFirstLogin", "true");
                        return RedirectToAction("ResetPassword", "Account", new { staffId = user.StaffId });
                    }
                    else
                    {
                        HttpContext.Session.SetString("IsFirstLogin", "false");
                        return RedirectToAction("Index", "Dashboard");
                    }
                }
            }
            ViewBag.Error = "Invalid username or password.";
            return View();
        }

        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPassword(string usernameOrEmail)
        {
            var staffUser = _context.StaffCredentials.FirstOrDefault(u => u.UserName == usernameOrEmail && u.Active == "Y");

            if (staffUser == null)
            {
                var contact = _context.StaffContacts.FirstOrDefault(c => c.Email == usernameOrEmail);
                if (contact != null)
                {
                    staffUser = _context.StaffCredentials.FirstOrDefault(u => u.StaffId == contact.StaffId && u.Active == "Y");
                }
            }

            if (staffUser == null)
            {
                ViewBag.Error = "No matching user found.";
                return View();
            }

            string? recipientEmail = usernameOrEmail.Contains("@")
                ? usernameOrEmail
                : _context.StaffContacts.FirstOrDefault(c => c.StaffId == staffUser.StaffId)?.Email;

            if (string.IsNullOrEmpty(recipientEmail) || !recipientEmail.Contains("@"))
            {
                ViewBag.Error = "A valid email address was not found for this user.";
                return View();
            }

            string token = Guid.NewGuid().ToString();
            var resetToken = new PasswordResetToken
            {
                StaffId = staffUser.StaffId,
                Token = token,
                Expiration = DateTime.UtcNow.AddHours(1)
            };
            _context.PasswordResetTokens.Add(resetToken);
            _context.SaveChanges();

            string? resetLink = Url.Action("ResetPassword", "Account", new { token = token }, Request.Scheme);

            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_emailSettings.GmailUser));
            email.To.Add(MailboxAddress.Parse(recipientEmail));
            email.Subject = "Password Reset - ZoomColorLab";
            email.Body = new TextPart(TextFormat.Plain)
            {
                Text = $"Hello,\n\nClick the link to reset your password:\n{resetLink}\n\nThis link expires in 1 hour."
            };

            try
            {
                using var smtp = new SmtpClient();
                smtp.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                smtp.Authenticate(_emailSettings.GmailUser, _emailSettings.GmailAppPassword);
                smtp.Send(email);
                smtp.Disconnect(true);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Failed to send email: {ex.Message}";
                return View();
            }

            TempData["SuccessMessage"] = "Password reset instructions sent to your email.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ResetPassword(string? token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                var tokenEntry = _context.PasswordResetTokens
                    .FirstOrDefault(t => t.Token == token && t.Expiration > DateTime.UtcNow);

                if (tokenEntry == null)
                    return NotFound("Invalid or expired token.");

                ViewBag.Token = token;
                return View();
            }
            else
            {
                var currentStaffId = HttpContext.Session.GetString("StaffId");
                if (string.IsNullOrEmpty(currentStaffId))
                    return Unauthorized();

                var staffUser = _context.StaffCredentials
                    .FirstOrDefault(u => u.StaffId.ToString() == currentStaffId && u.Active == "Y");

                if (staffUser == null)
                    return NotFound("User not found.");

                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(string? token, string newPassword)
        {
            StaffCredentials? staffUser = null;

            if (!string.IsNullOrEmpty(token))
            {
                var tokenEntry = _context.PasswordResetTokens
                    .FirstOrDefault(t => t.Token == token && t.Expiration > DateTime.UtcNow);

                if (tokenEntry == null)
                    return NotFound("Invalid or expired token.");

                staffUser = _context.StaffCredentials.FirstOrDefault(u => u.StaffId == tokenEntry.StaffId);
                if (staffUser == null)
                    return NotFound("User not found.");

                _context.PasswordResetTokens.Remove(tokenEntry);
            }
            else
            {
                var currentStaffId = HttpContext.Session.GetString("StaffId");
                if (string.IsNullOrEmpty(currentStaffId))
                    return Unauthorized();

                staffUser = _context.StaffCredentials
                    .FirstOrDefault(u => u.StaffId.ToString() == currentStaffId && u.Active == "Y");

                if (staffUser == null)
                    return NotFound("User not found.");

                staffUser.IsFirstLogin = false;
                HttpContext.Session.SetString("IsFirstLogin", "false");
            }

            var passwordHasher = new PasswordHasher<StaffCredentials>();

            var lastPasswords = _context.StaffPasswordHistories
                .Where(p => p.StaffId == staffUser.StaffId)
                .OrderByDescending(p => p.ChangedOn)
                .Take(3)
                .Select(p => p.HashedPassword)
                .ToList();

            lastPasswords.Insert(0, staffUser.Password);

            foreach (var oldHashed in lastPasswords)
            {
                var check = passwordHasher.VerifyHashedPassword(staffUser, oldHashed, newPassword);
                if (check == PasswordVerificationResult.Success)
                {
                    ViewBag.Error = "You cannot reuse your current or any of your last 3 passwords.";
                    ViewBag.Token = token;
                    return View();
                }
            }

            _context.StaffPasswordHistories.Add(new StaffPasswordHistory
            {
                StaffId = staffUser.StaffId,
                HashedPassword = staffUser.Password,
                ChangedOn = DateTime.UtcNow
            });

            staffUser.Password = passwordHasher.HashPassword(staffUser, newPassword);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Password reset successful!";
            return RedirectToAction("Index", "Dashboard");
        }

        public IActionResult Logout(string? reason = "Manually Logout")
        {
            var loginHistoryId = HttpContext.Session.GetInt32("LoginHistoryId");
            if (loginHistoryId != null)
            {
                var loginRecord = _context.LoginLogs.FirstOrDefault(l => l.Id == loginHistoryId);
                if (loginRecord != null && loginRecord.LogoutTime == null)
                {
                    loginRecord.LogoutTime = DateTime.UtcNow;
                    loginRecord.Reason = reason;
                    _context.SaveChanges();
                }
            }

            TempData["SuccessMessage"] = "You have been logged out successfully.";
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        private async Task<string> GetPublicIpAddressAsync()
        {
            try
            {
                string publicIp = await _httpClient.GetStringAsync("https://api.ipify.org");
                return publicIp.Trim();
            }
            catch
            {
                return "Unknown";
            }
        }

        private async Task<string> GetLocationFromIpAsync(string ip)
        {
            try
            {
                var response = await _httpClient.GetStringAsync($"http://ip-api.com/json/{ip}");
                dynamic locationData = JsonConvert.DeserializeObject(response);

                if (locationData?.status == "success")
                {
                    string city = locationData.city ?? "";
                    string region = locationData.regionName ?? "";
                    string country = locationData.country ?? "";
                    return $"{city}, {region}, {country}".Trim(' ', ',');
                }

                return "Unknown Location";
            }
            catch
            {
                return "Unknown Location";
            }
        }
    }
}