namespace TestWithADO.Models
{
    public class CartItem
    {
        public int ProduitID { get; set; }
        public string Titre { get; set; }
        public decimal Prix { get; set; }
        public int Quantite { get; set; }
        public decimal SubTotal => Quantite * Prix;
        public int MaxStock { get; set; }
    }
}