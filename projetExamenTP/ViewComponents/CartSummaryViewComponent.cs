using Microsoft.AspNetCore.Mvc;
using TestWithADO.Services;
using System.Linq;

namespace TestWithADO.ViewComponents
{
    public class CartSummaryViewComponent : ViewComponent
    {
        private readonly CartService _cartService;

        public CartSummaryViewComponent(CartService cartService)
        {
            _cartService = cartService;
        }

        public IViewComponentResult Invoke()
        {
            try
            {
                var cart = _cartService.GetCart();
                var totalItems = cart?.Sum(item => item.Quantite) ?? 0;
                return View(totalItems);
            }
            catch
            {
               
                return View(0);
            }
        }
    }
}