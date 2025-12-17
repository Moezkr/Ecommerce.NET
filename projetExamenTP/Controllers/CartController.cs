using Microsoft.AspNetCore.Mvc;
using TestWithADO.Services;
using System.Linq;
using System.Text.Json;
using TestWithADO.Models;
using System.Collections.Generic;

namespace TestWithADO.Controllers
{
    public class CartController : Controller
    {
        private readonly CartService _cartService;
        private readonly CommandeService _commandeService;
        private readonly ProduitService _produitService;
        private const string UserSessionKey = "_LoggedInUser";

        public CartController(CartService cartService, CommandeService commandeService, ProduitService produitService)
        {
            _cartService = cartService;
            _commandeService = commandeService;
            _produitService = produitService;
        }

        private Utilisateur GetCurrentUserFromSession()
        {
            var userJson = HttpContext.Session.GetString(UserSessionKey);
            return userJson == null ? null : JsonSerializer.Deserialize<Utilisateur>(userJson);
        }

        private int GetUserIdFromSession()
        {
            var user = GetCurrentUserFromSession();
            return user?.UtilisateurID ?? 0;
        }

        public IActionResult Index()
        {
            var cartItems = _cartService.GetCart();
            ViewData["Title"] = "Shopping Cart";
            return View(cartItems);
        }

        [HttpPost]
        public IActionResult AddToCart(int produitId, string titre, decimal prix, int quantity = 1)
        {
            _cartService.AddToCart(produitId, titre, prix, quantity);
            return RedirectToAction("Index", "Cart");
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int produitId, int quantity)
        {
            if (quantity <= 0)
            {
                _cartService.RemoveItem(produitId);
            }
            else
            {
                _cartService.UpdateQuantity(produitId, quantity);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult RemoveItem(int produitId)
        {
            _cartService.RemoveItem(produitId);
            return RedirectToAction("Index");
        }

        public IActionResult Checkout()
        {
            if (GetUserIdFromSession() == 0)
            {
                TempData["Info"] = "Please log in to proceed with checkout.";
                return RedirectToAction("Login", "Account");
            }

            var cart = _cartService.GetCart();
            if (!cart.Any())
            {
                TempData["Error"] = "Your cart is empty. Please add products before checking out.";
                return RedirectToAction("Index");
            }

            decimal cartTotalDinar = _cartService.GetCartTotal();
            const decimal EthPriceInDinar = 8896.41M;

            decimal ethNeeded = 0;
            if (cartTotalDinar > 0 && EthPriceInDinar > 0)
            {
                ethNeeded = cartTotalDinar / EthPriceInDinar;
            }

            ViewData["CartTotal"] = cartTotalDinar;
            ViewData["Title"] = "Checkout";
            ViewData["EthPriceInDinar"] = EthPriceInDinar;
            ViewData["EthNeeded"] = ethNeeded;

            return View(new CheckoutViewModel());
        }

        [HttpPost]
        public IActionResult PlaceOrder(CheckoutViewModel model, [FromForm] string IsBlockchainPaid = "false")
        {
            int userId = GetUserIdFromSession();
            if (userId == 0)
            {
                TempData["Error"] = "Please log in first.";
                return RedirectToAction("Login", "Account");
            }

            var cartItems = _cartService.GetCart();

            if (cartItems == null || !cartItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            bool isBlockchainPaid = IsBlockchainPaid?.ToLower() == "true";

            if (!isBlockchainPaid)
            {
                TempData["Error"] = "Blockchain payment was not completed.";
                return RedirectToAction("Checkout");
            }

            if (!ModelState.IsValid)
            {
                decimal cartTotalDinar = _cartService.GetCartTotal();
                ViewData["CartTotal"] = cartTotalDinar;
                ViewData["Title"] = "Checkout";
                ViewData["EthPriceInDinar"] = 8896.41M;
                ViewData["EthNeeded"] = cartTotalDinar / 8896.41M;

                TempData["Error"] = "Please fill all required fields correctly.";
                return View("Checkout", model);
            }

            var stockErrors = new List<string>();
            foreach (var item in cartItems)
            {
                var product = _produitService.GetProduitById(item.ProduitID);

                if (product == null)
                {
                    stockErrors.Add($"Error: Product '{item.Titre}' no longer exists.");
                    continue;
                }

                if (item.Quantite > product.Capacite)
                {
                    stockErrors.Add($"Product '{item.Titre}' has only {product.Capacite} item(s) left in stock. Requested: {item.Quantite}.");
                }
            }

            if (stockErrors.Any())
            {
                foreach (var error in stockErrors)
                {
                    ModelState.AddModelError("", error);
                }

                decimal cartTotalDinar = _cartService.GetCartTotal();
                ViewData["CartTotal"] = cartTotalDinar;
                ViewData["Title"] = "Checkout";
                ViewData["EthPriceInDinar"] = 8896.41M;
                ViewData["EthNeeded"] = cartTotalDinar / 8896.41M;

                return View("Checkout", model);
            }

            decimal total = _cartService.GetCartTotal();
            bool orderSaved = _commandeService.SaveOrder(userId, total, model, cartItems);

            if (orderSaved)
            {
                _cartService.SaveCart(new List<CartItem>());
                string successMsg = $"Your order was placed successfully! Ethereum payment confirmed. Total: {total:N3} DT.";
                TempData["Success"] = successMsg;

                return RedirectToAction("OrderConfirmation");
            }
            else
            {
                TempData["Error"] = "An error occurred while placing your order. Please try again.";

                decimal cartTotalDinar = _cartService.GetCartTotal();
                ViewData["CartTotal"] = cartTotalDinar;
                ViewData["Title"] = "Checkout";
                ViewData["EthPriceInDinar"] = 8896.41M;
                ViewData["EthNeeded"] = cartTotalDinar / 8896.41M;

                return View("Checkout", model);
            }
        }

        public IActionResult OrderConfirmation()
        {
            if (TempData["Success"] == null)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewData["Title"] = "Order Confirmed";
            return View();
        }
    }
}