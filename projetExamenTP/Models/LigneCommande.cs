namespace TestWithADO.Models
{
    public class LigneCommande
    {
        public int LigneCommandeID { get; set; }
        public int CommandeID { get; set; }
        public int ProduitID { get; set; }
        public string ProduitTitre { get; set; }
        public int Quantite { get; set; }
        public decimal PrixUnitaire { get; set; }
        public decimal SubTotal => Quantite * PrixUnitaire;
    }
}