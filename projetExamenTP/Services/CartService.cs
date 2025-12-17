using TestWithADO.Models;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Linq;
using System.Collections.Generic;
using TestWithADO.Services;

namespace TestWithADO.Services
{
    public class CartService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ProduitService _produitService;
        private const string CartSessionKey = "ShoppingCart";

        public CartService(IHttpContextAccessor httpContextAccessor, ProduitService produitService)
        {
            _httpContextAccessor = httpContextAccessor;
            _produitService = produitService;
        }

        private ISession Session => _httpContextAccessor.HttpContext.Session;

        public List<CartItem> GetCart()
        {
            var json = Session.GetString(CartSessionKey);
            return json == null ? new List<CartItem>() : JsonSerializer.Deserialize<List<CartItem>>(json) ?? new List<CartItem>();
        }

        public void SaveCart(List<CartItem> cart)
        {
            var json = JsonSerializer.Serialize(cart);
            Session.SetString(CartSessionKey, json);
        }

        public void AddToCart(int produitId, string titre, decimal prix, int quantity = 1)
        {
            var cart = GetCart();

            var product = _produitService.GetProduitById(produitId);
            if (product == null) return;

            var item = cart.FirstOrDefault(i => i.ProduitID == produitId);

            if (item == null)
            {
                if (quantity <= product.Capacite)
                {
                    cart.Add(new CartItem { ProduitID = produitId, Titre = titre, Prix = prix, Quantite = quantity, MaxStock = product.Capacite });
                }
            }
            else
            {
                int newQuantity = item.Quantite + quantity;
                if (newQuantity <= product.Capacite)
                {
                    item.Quantite = newQuantity;
                }
                else
                {
                    item.Quantite = product.Capacite;
                }
                item.MaxStock = product.Capacite;
            }

            SaveCart(cart);
        }

        public void UpdateQuantity(int produitId, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(i => i.ProduitID == produitId);

            if (item != null)
            {
                var product = _produitService.GetProduitById(produitId);

                if (product == null)
                {
                    cart.Remove(item);
                }
                else if (quantity <= product.Capacite)
                {
                    item.Quantite = quantity;
                    item.MaxStock = product.Capacite;
                }
                else
                {
                    item.Quantite = product.Capacite;
                    item.MaxStock = product.Capacite;
                }
                SaveCart(cart);
            }
        }

        public void RemoveItem(int produitId)
        {
            var cart = GetCart();
            cart.RemoveAll(i => i.ProduitID == produitId);
            SaveCart(cart);
        }

        public decimal GetCartTotal()
        {
            return GetCart().Sum(item => item.SubTotal);
        }
    }
}