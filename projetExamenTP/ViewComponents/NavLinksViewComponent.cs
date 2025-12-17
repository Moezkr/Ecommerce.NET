using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TestWithADO.Models; 

namespace TestWithADO.ViewComponents
{
    public class NavLinksViewComponent : ViewComponent
    {
        private const string UserSessionKey = "_LoggedInUser";

        private Utilisateur GetCurrentUserFromSession()
        {
            var userJson = HttpContext.Session.GetString(UserSessionKey);
            return userJson == null ? null : JsonSerializer.Deserialize<Utilisateur>(userJson);
        }

        public IViewComponentResult Invoke()
        {
            var user = GetCurrentUserFromSession();
            
            bool isAdmin = user?.Role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) ?? false;
            
            ViewData["ShouldShowCustomerLinks"] = !isAdmin;
            
            return View();
        }
    }
}