using Microsoft.AspNetCore.Mvc;
using TestWithADO.Models;
using TestWithADO.Services;
using System;
using System.Collections.Generic;

namespace TestWithADO.Controllers
{
    public class HomeController : Controller
    {
        private readonly ProduitService _produitService;
        private readonly CategorieService _categorieService;
        private readonly CartService _cartService;

        public HomeController(ProduitService produitService, CategorieService categorieService, CartService cartService)
        {
            _produitService = produitService;
            _categorieService = categorieService;
            _cartService = cartService;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Welcome to XtremePC";
            return View();
        }

        public IActionResult Products(
            [FromQuery] List<int> categoryIds, 
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice)
        {
            var products = _produitService.GetFilteredProduits(categoryIds, minPrice, maxPrice);

            var categories = _categorieService.GetAllCategories();
            ViewBag.Categories = categories;

            ViewBag.SelectedCategoryIds = categoryIds ?? new List<int>();
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;

            return View(products);
        }

        [HttpPost]
        public IActionResult AddToCart(int produitId, int quantity = 1)
        {
            var produit = _produitService.GetProduitById(produitId);

            if (produit == null || quantity <= 0)
            {
                TempData["Error"] = "Product not found or quantity is invalid.";
                return RedirectToAction("Products");
            }

            _cartService.AddToCart(
                produit.ProduitID,
                produit.Titre,
                produit.Prix,
                quantity
            );

            TempData["Success"] = $"Added {quantity} x {produit.Titre} to cart.";

            return RedirectToAction("Products");
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}