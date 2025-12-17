using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TestWithADO.Models;
using TestWithADO.Services;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;

namespace TestWithADO.Controllers
{
    public class AccountController : Controller
    {
        private readonly UtilisateurService _utilisateurService;
        private readonly CommandeService _commandeService;
        private const string UserSessionKey = "_LoggedInUser";

        public AccountController(UtilisateurService utilisateurService, CommandeService commandeService)
        {
            _utilisateurService = utilisateurService;
            _commandeService = commandeService;
        }

        private Utilisateur GetCurrentUserFromSession()
        {
            var userJson = HttpContext.Session.GetString(UserSessionKey);
            return userJson == null ? null : JsonSerializer.Deserialize<Utilisateur>(userJson);
        }

        public IActionResult Login()
        {
            var currentUser = GetCurrentUserFromSession();
            if (currentUser != null)
            {
                if (currentUser.Role == "ADMIN")
                {
                    return RedirectToAction("Dashboard", "Admin");
                }
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = _utilisateurService.ValidateUser(email, password);

            if (user != null)
            {
                HttpContext.Session.SetString(UserSessionKey, JsonSerializer.Serialize(user));

                if (user.Role == "ADMIN")
                {
                    return RedirectToAction("Dashboard", "Admin");
                }

                return RedirectToAction("Index", "Home");
            }

            ViewData["Error"] = "Invalid email or password.";
            return View();
        }

        public IActionResult Register()
        {
            ViewData["Title"] = "User Registration";
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var newUser = new Utilisateur
                {
                    Nom = model.Nom,
                    Prenom = model.Prenom,
                    Email = model.Email,
                    MotDePasse = model.Password,
                    Role = "CLIENT"
                };

                if (_utilisateurService.AddUtilisateur(newUser))
                {
                    TempData["Success"] = "Registration successful! Please log in.";
                    return RedirectToAction("Login");
                }
                ModelState.AddModelError("Email", "This email address is already registered.");
            }
            ViewData["Title"] = "User Registration";
            return View(model);
        }

        public IActionResult Profile()
        {
            var currentUser = GetCurrentUserFromSession();

            if (currentUser == null)
            {
                return RedirectToAction("Login");
            }

            var userOrders = _commandeService.GetOrdersByUserId(currentUser.UtilisateurID);

            var viewModel = new ProfileViewModel
            {
                User = currentUser,
                Orders = userOrders
            };

            ViewData["Title"] = $"{currentUser.Nom} {currentUser.Prenom}'s Profile";
            return View(viewModel);
        }

        public IActionResult EditProfile()
        {
            var currentUser = GetCurrentUserFromSession();
            if (currentUser == null)
            {
                return RedirectToAction("Login");
            }

            var viewModel = new ProfileEditViewModel
            {
                UtilisateurID = currentUser.UtilisateurID,
                Nom = currentUser.Nom,
                Prenom = currentUser.Prenom,
                Email = currentUser.Email
            };

            ModelState.Clear();

            ViewData["Title"] = "Edit Profile";
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditProfile(ProfileEditViewModel model)
        {
            ModelState.Remove("NewPassword");
            ModelState.Remove("ConfirmNewPassword");

            if (ModelState.IsValid)
            {
                try
                {
                    var currentUser = GetCurrentUserFromSession();
                    if (currentUser == null)
                    {
                        return RedirectToAction("Login");
                    }

                    if (currentUser.UtilisateurID != model.UtilisateurID)
                    {
                        TempData["Error"] = "Unauthorized access.";
                        return RedirectToAction("Profile");
                    }

                    var updatedUser = new Utilisateur
                    {
                        UtilisateurID = model.UtilisateurID,
                        Nom = model.Nom,
                        Prenom = model.Prenom,
                        Email = model.Email,
                        Role = currentUser.Role 
                    };

                    if (!string.IsNullOrEmpty(model.NewPassword))
                    {
                        if (model.NewPassword.Length < 6)
                        {
                            ModelState.AddModelError("NewPassword", "Le mot de passe doit contenir au moins 6 caractères.");
                            ViewData["Title"] = "Edit Profile";
                            return View(model);
                        }

                        if (model.NewPassword != model.ConfirmNewPassword)
                        {
                            ModelState.AddModelError("ConfirmNewPassword", "Le nouveau mot de passe et sa confirmation ne correspondent pas.");
                            ViewData["Title"] = "Edit Profile";
                            return View(model);
                        }

                        updatedUser.MotDePasse = model.NewPassword;
                    }
                    else
                    {
                        updatedUser.MotDePasse = null; 
                    }

                    bool updateSuccess = _utilisateurService.UpdateUtilisateur(updatedUser);

                    if (!updateSuccess)
                    {
                        ModelState.AddModelError("", "Failed to update profile. Please try again.");
                        ViewData["Title"] = "Edit Profile";
                        return View(model);
                    }

                    var freshUser = _utilisateurService.GetUtilisateurById(model.UtilisateurID);
                    if (freshUser != null)
                    {
                        HttpContext.Session.SetString(UserSessionKey, JsonSerializer.Serialize(freshUser));
                    }

                    TempData["Success"] = "Your profile has been updated successfully!";
                    return RedirectToAction("Profile");
                }
                catch (SqlException ex) when (ex.Number == 2627) 
                {
                    ModelState.AddModelError("Email", "This email is already taken by another account.");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An unexpected error occurred: " + ex.Message);
                }
            }

            ViewData["Title"] = "Edit Profile";
            return View(model);
        }

        public IActionResult OrderHistory()
        {
            var currentUser = GetCurrentUserFromSession();
            if (currentUser == null)
            {
                return RedirectToAction("Login");
            }

            var orders = _commandeService.GetOrdersByUserId(currentUser.UtilisateurID);
            ViewData["Title"] = "My Order History";
            return View(orders);
        }

        public IActionResult OrderDetails(int id)
        {
            var currentUser = GetCurrentUserFromSession();
            if (currentUser == null)
            {
                return RedirectToAction("Login");
            }

            var order = _commandeService.GetOrderById(id);

            if (order == null || order.UtilisateurID != currentUser.UtilisateurID)
            {
                TempData["Error"] = "Order not found or access denied.";
                return RedirectToAction("OrderHistory");
            }

            ViewData["Title"] = $"Order #{order.CommandeID} Details";
            return View(order);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove(UserSessionKey);
            return RedirectToAction("Index", "Home");
        }
    }
}