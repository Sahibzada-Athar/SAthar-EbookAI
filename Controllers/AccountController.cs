﻿﻿using EBookDashboard.Interfaces;
using EBookDashboard.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;


namespace EBookDashboard.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            ApplicationDbContext context,
            IEmailService emailService,
            ILogger<AccountController> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }
        //--------------------------------------------------------------------------
        //-----------------  User Login -------------------------------------
        //--------------------------------------------------------------------------           
         // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        
        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(string UserEmail, string Password, bool RememberMe)
        {
            int? userId = 0;
            // check against Users table in MySQL
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserEmail == UserEmail && u.Password == Password);

            if (user != null)
            {
                HttpContext.Session.SetInt32("UserId", user.UserId);
                userId = user.UserId;
               // ✅ Login success → redirect to main layout (Dashboard, Home, etc.)

                // ✅ fetch role name from Roles table
               var role = await _context.Roles
                    .Where(r => r.RoleId == user.RoleId)
                    .Select(r => r.RoleName)
                    .FirstOrDefaultAsync();

                // ✅ redirect based on role
                // ✅ build claims
                var claims = new List<Claim>
            {
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.UserEmail),
            new Claim(ClaimTypes.Role, role ?? "Reader") // default role if null
            };

                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = RememberMe,   // remember me checkbox
                    ExpiresUtc = DateTime.UtcNow.AddMinutes(30) // session timeout
                };

                // ✅ Sign in the user with cookie
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

				// ✅ redirect to AI Writer after successful login
				return RedirectToAction("AIGenerateBook", "Books");
            }
            else
            {
                // ❌ Login failed → show error message
                ViewBag.Error = "Invalid username or password. Please try again.";
                ViewBag.UserId = userId; // ✅ send to Razor view
                return View();
            }
        }
        //--------------------------------------------------------------------------
        //-----------------  User Registration -------------------------------------
        //--------------------------------------------------------------------------
        // GET: Registration
        [HttpGet]
        public IActionResult Registration()
        {
            return View();
        }

        // POST: Registration
        [HttpPost]
        public async Task<IActionResult> Register(Users model, string ConfirmPassword)
        {
            
            // Check ConfirmPassword
            if (model.Password != ConfirmPassword)
            {
                ViewBag.Error = "Passwords do not match!";
                return View("Registration", model);
            }
            // Check if email already exists
            var existingUser = _context.Users.FirstOrDefault(u => u.UserEmail == model.UserEmail);
            if (existingUser != null)
            {
                ViewBag.Error = "Email already registered!";
                return View("Registration", model);
            }
            // OK Save the Data

            // Set default values
            model.CreatedAt = DateTime.UtcNow;
            model.LastLoginAt = DateTime.UtcNow;
            model.Status = "Active";
            model.RoleId = 3; // Default Reader Role

            // Save to DB
            _context.Users.Add(model);
            await _context.SaveChangesAsync();

            ViewBag.Success = "Account created successfully! Please login.";
            return RedirectToAction("Login", "Account");
        }
        //--------------------------------------------------------------------------
        //-----------------  Forgot Password   --------------------------------------
        //--------------------------------------------------------------------------
        // GET: ForgotPassword Action --> It will return Views\Account\ForgotPassword.cshtml page
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
       {
            return View();
        }
        // POST: ForgotPassword Action --> It will generate email and return Views\Account\ForgotPassword.cshtml page
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPassword model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserEmail == model.UserEmail);

                if (user != null)
                {
                    // Generate a random reset token (you can store it in DB with expiry)
                    var token = Guid.NewGuid().ToString();

                    // Save token in DB (create a PasswordResetTokens table or add field in Users)
                    var resetLink = Url.Action(
                        "ResetPassword",
                        "Account",
                        new { email = model.UserEmail, token = token },
                        Request.Scheme
                    );

                        var emailBody = $@"
                        <h2>Password Reset</h2>
                        <p>Click the link below to reset your password:</p>
                        <p><a href='{resetLink}'>Reset Password</a></p>
                    ";

                    await _emailService.SendEmailAsync(model.UserEmail, "Password Reset Request", emailBody);
                    _logger.LogWarning(resetLink);
                    return RedirectToAction("ForgotPasswordConfirmation", "Account");
                }

                // Don’t reveal if email doesn’t exist
                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            return View(model);
        }
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }
        //--------------------------------------------------------------------------
        //-----------------  Reset Password   --------------------------------------
        //--------------------------------------------------------------------------
        // POST: ForgotPassword Action --> It will generate email and return Views\Account\ForgotPassword.cshtml page
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPassword model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserEmail == model.UserEmail);
            // if user did not provide valid email address this part will not execut
            if (user == null)
            {
                // don’t reveal user existence
                return View("ResetPasswordConfirmation");
            }

            // ✅ TODO: validate token manually (since no Identity is used)

            // Update password (hash it in production!)
            user.Password = model.Password;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Password reset for {Email}", model.UserEmail);

            return View("ResetPasswordConfirmation");
        }

        // GET: Reset Password Action --> It will Check toke and email and return Message page
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string token, string email)
        {
            if (token == null || email == null)
            {
                ModelState.AddModelError("", "Invalid Password reset token");
                
            }
            return View();
        }
        // Logout
        public async Task<IActionResult> Logout()
        {
            // Clear session
            HttpContext.Session.Clear();
            
            // Sign out from cookie authentication
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            
            return RedirectToAction("Login");
        }

        //public IActionResult Login(string? returnUrl = null, bool sessionExpired = false)
        //{
        //    if (User.Identity?.IsAuthenticated == true)
        //    {
        //        return RedirectToAction("Index", "Dashboard");
        //    }

        //    ViewBag.ReturnUrl = returnUrl;

        //    if (sessionExpired)
        //    {
        //        ViewBag.Message = "Your session has expired. Please log in again.";
        //    }

        //    return View();
        //}

        //// API endpoint for checking authentication status
        //[HttpGet]
        //public IActionResult CheckAuth()
        //{
        //    if (User.Identity?.IsAuthenticated == true)
        //    {
        //        return Ok(new { isAuthenticated = true, userName = User.Identity.Name });
        //    }
        //    return Unauthorized();
        //}

        // API endpoint for checking session status
        //[HttpGet]
        //[Authorize]
        //public IActionResult CheckSession()
        //{
        //    var lastActivity = HttpContext.Session.GetString("LastActivity");
        //    if (lastActivity != null)
        //    {
        //        var lastActivityTime = DateTime.Parse(lastActivity);
        //        var timeRemaining = TimeSpan.FromMinutes(30) - (DateTime.UtcNow - lastActivityTime);

        //        return Ok(new { 
        //            isActive = true, 
        //            timeRemaining = timeRemaining.TotalMinutes,
        //            lastActivity = lastActivityTime
        //        });
        //    }
        //    return Unauthorized();
        //}

        //[HttpPost]
        //public async Task<IActionResult> Login(LoginViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        var errors = ModelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage).ToList();
        //        var errorMessage = string.Join(", ", errors);
        //        if (Request.Headers["Content-Type"].ToString().Contains("application/json"))
        //            return Json(new { success = false, message = errorMessage });
        //        ViewBag.Error = errorMessage;
        //        return View();
        //    }

        //    // Simple authentication - in production, use proper password hashing and database
        //    var users = HttpContext.Session.GetString("RegisteredUsers");
        //    if (!string.IsNullOrEmpty(users))
        //    {
        //        var userList = System.Text.Json.JsonSerializer.Deserialize<List<UserRegistrationModel>>(users);
        //        var user = userList?.FirstOrDefault(u => u.Email == model.Email && u.Password == model.Password);

        //        if (user != null)
        //        {
        //            var claims = new List<Claim>
        //            {
        //                new Claim(ClaimTypes.Name, user.FullName),
        //                new Claim(ClaimTypes.Email, user.Email),
        //                new Claim("AuthorName", user.AuthorName ?? ""),
        //                new Claim("Genre", user.Genre ?? "")
        //            };

        //            var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
        //            var authProperties = new AuthenticationProperties
        //            {
        //                IsPersistent = model.RememberMe,
        //                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(model.RememberMe ? 30 : 1)
        //            };

        //            await HttpContext.SignInAsync("Cookies", new ClaimsPrincipal(claimsIdentity), authProperties);

        //            if (Request.Headers["Content-Type"].ToString().Contains("application/json"))
        //                return Json(new { success = true, message = "Login successful!", redirectUrl = "/Dashboard" });
        //            return RedirectToAction("Index", "Dashboard");
        //        }
        //    }

        //    var loginErrorMessage = "Invalid email or password.";
        //    if (Request.Headers["Content-Type"].ToString().Contains("application/json"))
        //        return Json(new { success = false, message = loginErrorMessage });
        //    ViewBag.Error = loginErrorMessage;
        //    return View();
        //}

        //[HttpPost]  
        //public async Task<IActionResult> Register(UserRegistrationModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        var errors = ModelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage).ToList();
        //        var errorMessage = string.Join(", ", errors);
        //        if (Request.Headers["Content-Type"].ToString().Contains("application/json"))
        //            return Json(new { success = false, message = errorMessage });
        //        ViewBag.Error = errorMessage;
        //        return View("Login");
        //    }

        //    if (model.Password != model.ConfirmPassword)
        //    {
        //        var passwordMismatchError = "Passwords do not match!";
        //        if (Request.Headers["Content-Type"].ToString().Contains("application/json"))
        //            return Json(new { success = false, message = passwordMismatchError });
        //        ViewBag.Error = passwordMismatchError;
        //        return View("Login");
        //    }

        //    // Get existing users from session
        //    var existingUsers = new List<UserRegistrationModel>();
        //    var usersJson = HttpContext.Session.GetString("RegisteredUsers");
        //    if (!string.IsNullOrEmpty(usersJson))
        //    {
        //        existingUsers = System.Text.Json.JsonSerializer.Deserialize<List<UserRegistrationModel>>(usersJson) ?? new List<UserRegistrationModel>();
        //    }

        //    // Check if user already exists
        //    if (existingUsers.Any(u => u.Email == model.Email))
        //    {
        //        var duplicateEmailError = "An account with this email already exists.";
        //        if (Request.Headers["Content-Type"].ToString().Contains("application/json"))
        //            return Json(new { success = false, message = duplicateEmailError });
        //        ViewBag.Error = duplicateEmailError;
        //        return View("Login");
        //    }

        //    // Add new user
        //    existingUsers.Add(model);

        //    // Save back to session
        //    var updatedUsersJson = System.Text.Json.JsonSerializer.Serialize(existingUsers);
        //    HttpContext.Session.SetString("RegisteredUsers", updatedUsersJson);

        //    if (Request.Headers["Content-Type"].ToString().Contains("application/json"))
        //        return Json(new { success = true, message = "Account created successfully!" });
        //    ViewBag.Success = "Account created successfully! Please login.";
        //    return View("Login");
        //}

        //public async Task<IActionResult> Logout()
        //{
        //    await HttpContext.SignOutAsync("Cookies");
        //    return RedirectToAction("Login");
        //}

        //public IActionResult AccessDenied()
        //{
        //    return View();
        //}
    }
}