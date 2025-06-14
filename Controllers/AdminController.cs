﻿using KPI_Dashboard.Models;
using KPI_Dashboard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace KPI_Dashboard.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailService _emailService;

        public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IEmailService emailService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _emailService = emailService;
        }

        // List all users
        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id ?? string.Empty,
                    Name = user.Name ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    Role = roles.FirstOrDefault() ?? string.Empty
                });
            }

            return View(userViewModels);
        }

        // GET: Add User
        [HttpGet]
        public IActionResult AddUser()
        {
            var model = new AddUserViewModel
            {
                Name = string.Empty,
                Email = string.Empty,
                Password = string.Empty,
                AvailableRoles = _roleManager.Roles.Select(r => r.Name ?? string.Empty).ToList(),
                SelectedRoles = new List<string>()
            };
            return View(model);
        }

        // POST: Add User
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUser(AddUserViewModel model)
        {
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<AdminController>>();
            logger.LogInformation("Attempting to add user with email: {Email}", model.Email);

            if (!ModelState.IsValid)
            {
                logger.LogWarning("ModelState is invalid. Errors: {Errors}", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                model.AvailableRoles = _roleManager.Roles.Select(r => r.Name ?? string.Empty).ToList();
                model.SelectedRoles ??= new List<string>();
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                Name = model.Name
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                if (model.SelectedRoles != null)
                {
                    await _userManager.AddToRolesAsync(user, model.SelectedRoles);
                }

                logger.LogInformation("User created successfully: {Email}", model.Email);
                TempData["SuccessMessage"] = "User created successfully!";
                return RedirectToAction("Index");
            }

            logger.LogError("Failed to create user. Errors: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            model.AvailableRoles = _roleManager.Roles.Select(r => r.Name ?? string.Empty).ToList();
            model.SelectedRoles ??= new List<string>();
            return View(model);
        }

        // GET: Edit User
        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var model = new EditUserViewModel
            {
                Id = user.Id ?? string.Empty,
                Name = user.Name ?? string.Empty,
                Email = user.Email ?? string.Empty,
                SelectedRoles = roles.ToList(),
                AvailableRoles = _roleManager.Roles.Select(r => r.Name ?? string.Empty).ToList()
            };

            return View(model);
        }

        // POST: Edit User
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<AdminController>>();
            logger.LogInformation("Attempting to edit user with ID: {Id}, Email: {Email}", model.Id, model.Email);

            if (!ModelState.IsValid)
            {
                logger.LogWarning("ModelState is invalid. Errors: {Errors}", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                model.AvailableRoles = _roleManager.Roles.Select(r => r.Name ?? string.Empty).ToList();
                model.SelectedRoles ??= new List<string>();
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                logger.LogError("User not found with ID: {Id}", model.Id);
                return NotFound();
            }

            user.Name = model.Name;
            user.Email = model.Email;
            user.UserName = model.Email;

            var updateResult = await _userManager.UpdateAsync(user);
            if (updateResult.Succeeded)
            {
                logger.LogInformation("User updated successfully: {Email}", model.Email);

                // Update roles
                var currentRoles = await _userManager.GetRolesAsync(user);
                var result = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!result.Succeeded)
                {
                    logger.LogWarning("Failed to remove existing roles: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                }
                if (model.SelectedRoles != null)
                {
                    result = await _userManager.AddToRolesAsync(user, model.SelectedRoles);
                    if (!result.Succeeded)
                    {
                        logger.LogWarning("Failed to add new roles: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }

                TempData["SuccessMessage"] = "User updated successfully!";
                return RedirectToAction("Index");
            }

            logger.LogError("Failed to update user. Errors: {Errors}", string.Join(", ", updateResult.Errors.Select(e => e.Description)));
            foreach (var error in updateResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            model.AvailableRoles = _roleManager.Roles.Select(r => r.Name ?? string.Empty).ToList();
            model.SelectedRoles ??= new List<string>();
            return View(model);
        }

        // GET: Delete User
        [HttpGet]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var model = new UserViewModel
            {
                Id = user.Id ?? string.Empty,
                Name = user.Name ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? string.Empty
            };

            return View(model);
        }

        // POST: Delete User
        [HttpPost, ActionName("DeleteUser")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "User deleted successfully!";
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(new UserViewModel { Id = user.Id ?? string.Empty, Name = user.Name ?? string.Empty, Email = user.Email ?? string.Empty, Role = string.Empty });
        }

        // GET: Change Password
        [HttpGet]
        public async Task<IActionResult> ChangePassword(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var model = new ChangePasswordViewModel
            {
                Id = user.Id ?? string.Empty,
                Email = user.Email ?? string.Empty
            };

            return View(model);
        }

        // POST: Change Password
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return NotFound();
            }

            // Remove old password and set new one
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
            if (result.Succeeded)
            {
                // Temporarily disable email sending
                // await _emailService.SendEmailAsync(user.Email, "Password Changed", $"Your password has been changed to: {model.NewPassword}");

                TempData["SuccessMessage"] = "Password changed successfully!";
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }
    }

    public class UserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public class AddUserViewModel
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name must not exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string Password { get; set; } = string.Empty;

        public List<string> AvailableRoles { get; set; } = new List<string>();
        public List<string> SelectedRoles { get; set; } = new List<string>();
    }

    public class EditUserViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name must not exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        public List<string> AvailableRoles { get; set; } = new List<string>();
        public List<string> SelectedRoles { get; set; } = new List<string>();
    }

    public class ChangePasswordViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "New Password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "New Password must be at least 6 characters long.")]
        public string NewPassword { get; set; } = string.Empty;

        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
