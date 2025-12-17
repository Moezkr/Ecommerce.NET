using Microsoft.AspNetCore.Mvc;
using TestWithADO.Models;
using TestWithADO.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace TestWithADO.Controllers
{
    public class ProduitController : Controller
    {
        private readonly ProduitService _produitService;
        private readonly CategorieService _categorieService;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly CartService _cartService;

        public ProduitController(ProduitService produitService,
                                 CategorieService categorieService,
                                 IWebHostEnvironment hostEnvironment,
                                 CartService cartService)
        {
            _produitService = produitService;
            _categorieService = categorieService;
            _hostEnvironment = hostEnvironment;
            _cartService = cartService;
        }

        private void PopulateCategories(object selectedCategory = null)
        {
            var categories = _categorieService.GetAllCategories();
            ViewBag.Categories = new SelectList(categories, "CategorieID", "Nom", selectedCategory);
        }

        public IActionResult Index()
        {
            var produits = _produitService.GetAllProduits();
            ViewData["Title"] = "Manage Products";
            return View("~/Views/Admin/Produit/Index.cshtml", produits);
        }

        public IActionResult Create()
        {
            PopulateCategories();
            ViewData["Title"] = "Create New Product";
            return View("~/Views/Admin/Produit/Create.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Produit produit)
        {
            ModelState.Remove("ImagePath");
            ModelState.Remove("CategorieNom");
            ModelState.Remove("IsAvailable");

            if (produit.ImageFile != null && produit.ImageFile.Length > 0)
            {
                string wwwRootPath = _hostEnvironment.WebRootPath;
                string uploadPath = Path.Combine(wwwRootPath, "images", "products");

                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(produit.ImageFile.FileName);
                string filePath = Path.Combine(uploadPath, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await produit.ImageFile.CopyToAsync(fileStream);
                }

                produit.ImagePath = "/images/products/" + fileName;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _produitService.AddProduit(produit);
                    TempData["Success"] = "Product created successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error: " + ex.Message);
                }
            }

            PopulateCategories(produit.CategorieID);
            ViewData["Title"] = "Create New Product";
            return View("~/Views/Admin/Produit/Create.cshtml", produit);
        }

        public IActionResult Edit(int id)
        {
            var produit = _produitService.GetProduitById(id);
            if (produit == null)
            {
                return NotFound();
            }

            PopulateCategories(produit.CategorieID);
            ViewData["Title"] = $"Edit Product: {produit.Titre}";
            return View("~/Views/Admin/Produit/Edit.cshtml", produit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Produit produit)
        {
            ModelState.Remove("ImageFile");
            ModelState.Remove("ImagePath");
            ModelState.Remove("CategorieNom");
            ModelState.Remove("IsAvailable");

            var existingProduit = _produitService.GetProduitById(produit.ProduitID);
            if (existingProduit == null)
            {
                TempData["Error"] = "Product not found.";
                return RedirectToAction("Index");
            }

            string oldImagePath = existingProduit.ImagePath;

            if (produit.ImageFile != null && produit.ImageFile.Length > 0)
            {
                if (!string.IsNullOrEmpty(oldImagePath))
                {
                    string oldFilePath = Path.Combine(_hostEnvironment.WebRootPath, oldImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                string wwwRootPath = _hostEnvironment.WebRootPath;
                string uploadPath = Path.Combine(wwwRootPath, "images", "products");
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(produit.ImageFile.FileName);
                string filePath = Path.Combine(uploadPath, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await produit.ImageFile.CopyToAsync(fileStream);
                }

                produit.ImagePath = "/images/products/" + fileName;
            }
            else
            {
                produit.ImagePath = oldImagePath;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _produitService.UpdateProduit(produit);
                    TempData["Success"] = "Product updated successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error updating product: " + ex.Message);
                }
            }

            PopulateCategories(produit.CategorieID);
            ViewData["Title"] = "Edit Product";
            return View("~/Views/Admin/Produit/Edit.cshtml", produit);
        }

        public IActionResult Details(int id)
        {
            var produit = _produitService.GetProduitById(id);
            if (produit == null)
            {
                return NotFound();
            }

            var cartItem = _cartService.GetCart().FirstOrDefault(i => i.ProduitID == id);
            int quantityInCart = cartItem?.Quantite ?? 0;
            int maxAddable = produit.Capacite - quantityInCart;

            ViewData["MaxAddable"] = maxAddable;
            ViewData["Title"] = produit.Titre;
            return View("~/Views/Produit/Details.cshtml", produit);
        }

        public IActionResult Delete(int id)
        {
            try
            {
                var produit = _produitService.GetProduitById(id);

                _produitService.DeleteProduit(id);
                TempData["Success"] = "Product deleted successfully.";

                if (produit != null && !string.IsNullOrEmpty(produit.ImagePath))
                {
                    string filePath = Path.Combine(_hostEnvironment.WebRootPath, produit.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                TempData["Error"] = "Cannot delete this product because it is included in past orders.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
    }
}