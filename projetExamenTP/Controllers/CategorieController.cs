using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using TestWithADO.Models;
using TestWithADO.Services;

namespace TestWithADO.Controllers
{
    public class CategorieController : Controller
    {
        private readonly CategorieService _categorieService;

        public CategorieController(CategorieService categorieService)
        {
            _categorieService = categorieService;
        }

        public IActionResult Index()
        {
            var categories = _categorieService.GetAllCategories();
            ViewData["Title"] = "Manage Categories";
            return View("~/Views/Admin/Categorie/Index.cshtml", categories);
        }

        public IActionResult Create()
        {
            ViewData["Title"] = "Create New Category";
            return View("~/Views/Admin/Categorie/Create.cshtml");
        }

        [HttpPost]
        public IActionResult Create(Categorie categorie)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _categorieService.AddCategorie(categorie);
                    TempData["Success"] = $"Category '{categorie.Nom}' was successfully created.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"An error occurred while creating category '{categorie.Nom}': {ex.Message}";
                }
            }

            ViewData["Title"] = "Create New Category";
            return View("~/Views/Admin/Categorie/Create.cshtml", categorie);
        }

        public IActionResult Edit(int id)
        {
            var categorie = _categorieService.GetCategorieById(id);
            if (categorie == null)
            {
                return NotFound();
            }
            ViewData["Title"] = "Edit Category";
            return View("~/Views/Admin/Categorie/Edit.cshtml", categorie);
        }

        [HttpPost]
        public IActionResult Edit(Categorie categorie)
        {
            if (ModelState.IsValid)
            {
                if (categorie.CategorieID == 0)
                {
                    TempData["Error"] = "Error: Category ID is missing for the update operation.";
                    return RedirectToAction("Index");
                }

                try
                {
                    _categorieService.UpdateCategorie(categorie);
                    TempData["Success"] = $"Category '{categorie.Nom}' was successfully updated.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"An error occurred while updating category '{categorie.Nom}': {ex.Message}";
                }
            }

            ViewData["Title"] = "Edit Category";
            return View("~/Views/Admin/Categorie/Edit.cshtml", categorie);
        }

        public IActionResult Delete(int id, string categoryName)
        {
            try
            {
                _categorieService.DeleteCategorie(id);
                TempData["Success"] = $"Category '{categoryName}' was successfully deleted.";
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                TempData["Error"] = $"Cannot delete category '{categoryName}' because products are still linked to it.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
    }
}