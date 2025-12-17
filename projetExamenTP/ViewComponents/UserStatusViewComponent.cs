using Microsoft.AspNetCore.Mvc;
using TestWithADO.Models;
using System.Text.Json;

namespace TestWithADO.ViewComponents
{
    public class UserStatusViewComponent : ViewComponent
    {
        private const string UserSessionKey = "_LoggedInUser";

        public IViewComponentResult Invoke()
        {
            try
            {
                if (HttpContext?.Session == null)
                {
                    return View((Utilisateur)null);
                }

                var userJson = HttpContext.Session.GetString(UserSessionKey);
                if (string.IsNullOrEmpty(userJson))
                {
                    return View((Utilisateur)null);
                }

                var user = JsonSerializer.Deserialize<Utilisateur>(userJson);
                return View(user);
            }
            catch
            {
                return View((Utilisateur)null);
            }
        }
    }
}