using Microsoft.AspNetCore.Mvc;
using TestWithADO.Services;
using TestWithADO.Models;
using System.Text.Json;
using System.Collections.Generic;

namespace TestWithADO.Controllers
{
    public class AdminController : Controller
    {
        private readonly CommandeService _commandeService;
        private const string UserSessionKey = "_LoggedInUser";

        public AdminController(CommandeService commandeService)
        {
            _commandeService = commandeService;
        }

        private Utilisateur GetCurrentUserFromSession()
        {
            var userJson = HttpContext.Session.GetString(UserSessionKey);
            return userJson == null ? null : JsonSerializer.Deserialize<Utilisateur>(userJson);
        }

        private bool IsAdmin()
        {
            var user = GetCurrentUserFromSession();
            return user?.Role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) ?? false;
        }

        public IActionResult Dashboard()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }
            ViewData["Title"] = "Admin Dashboard";
            return View();
        }

        public IActionResult ManageProducts()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }
            return RedirectToAction("Index", "Produit");
        }

        public IActionResult ManageCategories()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }
            return RedirectToAction("Index", "Categorie");
        }

        public IActionResult OrdersList()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["Title"] = "Manage All Customer Orders";

            List<Commande> allOrders = _commandeService.GetAllOrders();

            return View("Orders/OrdersList", allOrders);
        }

        public IActionResult OrderDetail(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            var (order, customer) = _commandeService.GetFullOrderDetailsForAdmin(id);

            if (order == null)
            {
                TempData["Error"] = $"Order #{id} not found.";
                return RedirectToAction("OrdersList");
            }

            ViewData["Title"] = $"Order Details #{id}";

            return View("Orders/OrderDetail", (order, customer));
        }

        [HttpPost]
        public IActionResult ConfirmDelivery(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            const string deliveredStatus = "Delivered";

            if (_commandeService.UpdateOrderStatus(id, deliveredStatus))
            {
                TempData["Success"] = $"Order #{id} status successfully updated to '{deliveredStatus}'.";
            }
            else
            {
                TempData["Error"] = $"Error updating order #{id}. The order may have already been processed.";
            }

            return RedirectToAction("OrdersList");
        }
    }
}