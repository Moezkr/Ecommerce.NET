using TestWithADO.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System;
using System.Data;
using System.Linq;

namespace TestWithADO.Services
{
    public class ProduitService
    {
        private readonly string _connectionString;

        public ProduitService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public Produit GetProduitById(int id)
        {
            Produit produit = null;
            string query = @"
                SELECT p.ProduitID, p.Titre, p.Description, p.Prix, p.Capacite, p.ImagePath, p.CategorieID, c.Nom AS CategorieNom
                FROM Produit p
                JOIN Categorie c ON p.CategorieID = c.CategorieID
                WHERE p.ProduitID = @ProduitID";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ProduitID", id);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        produit = new Produit
                        {
                            ProduitID = reader.GetInt32(0),
                            Titre = reader.GetString(1),
                            Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                            Prix = reader.GetDecimal(3),
                            Capacite = reader.GetInt32(4),
                            ImagePath = reader.IsDBNull(5) ? null : reader.GetString(5),
                            CategorieID = reader.GetInt32(6),
                            CategorieNom = reader.GetString(7)
                        };
                    }
                }
            }
            return produit;
        }

        public List<Produit> GetAllProduits()
        {
            var produits = new List<Produit>();
            string query = @"
                SELECT p.ProduitID, p.Titre, p.Description, p.Prix, p.Capacite, p.ImagePath, p.CategorieID, c.Nom AS CategorieNom
                FROM Produit p
                JOIN Categorie c ON p.CategorieID = c.CategorieID
                ORDER BY p.ProduitID DESC";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        produits.Add(new Produit
                        {
                            ProduitID = reader.GetInt32(0),
                            Titre = reader.GetString(1),
                            Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                            Prix = reader.GetDecimal(3),
                            Capacite = reader.GetInt32(4),
                            ImagePath = reader.IsDBNull(5) ? null : reader.GetString(5),
                            CategorieID = reader.GetInt32(6),
                            CategorieNom = reader.GetString(7)
                        };
                    }
                }
            }
            return produits;
        }

        public void AddProduit(Produit produit)
        {
            string query = @"
                INSERT INTO Produit (Titre, Description, Prix, Capacite, ImagePath, CategorieID)
                VALUES (@Titre, @Description, @Prix, @Capacite, @ImagePath, @CategorieID)";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Titre", produit.Titre ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Description",
                    string.IsNullOrEmpty(produit.Description) ? (object)DBNull.Value : produit.Description);
                command.Parameters.AddWithValue("@ImagePath",
                    string.IsNullOrEmpty(produit.ImagePath) ? (object)DBNull.Value : produit.ImagePath);
                command.Parameters.AddWithValue("@CategorieID", produit.CategorieID);

                command.Parameters.Add(new SqlParameter("@Prix", SqlDbType.Decimal)
                {
                    Precision = 10,
                    Scale = 3,
                    Value = produit.Prix
                });

                command.Parameters.Add(new SqlParameter("@Capacite", SqlDbType.Int)
                {
                    Value = produit.Capacite
                });

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void UpdateProduit(Produit produit)
        {
            string query = @"
                UPDATE Produit SET 
                    Titre = @Titre, 
                    Description = @Description, 
                    Prix = @Prix, 
                    Capacite = @Capacite, 
                    ImagePath = @ImagePath, 
                    CategorieID = @CategorieID 
                WHERE ProduitID = @ProduitID";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Titre", produit.Titre);
                command.Parameters.AddWithValue("@Description", produit.Description ?? (object)DBNull.Value);
                command.Parameters.Add(new SqlParameter("@Prix", SqlDbType.Decimal)
                {
                    Precision = 10,
                    Scale = 3,
                    Value = produit.Prix
                });
                command.Parameters.AddWithValue("@Capacite", produit.Capacite);
                command.Parameters.AddWithValue("@ImagePath", produit.ImagePath ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@CategorieID", produit.CategorieID);
                command.Parameters.AddWithValue("@ProduitID", produit.ProduitID);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void UpdateProductCapacity(int produitId, int quantityChange, SqlConnection connection, SqlTransaction transaction)
        {
            string query = @"
                UPDATE Produit SET Capacite = Capacite + @QuantityChange 
                WHERE ProduitID = @ProduitID";

            using (var command = new SqlCommand(query, connection, transaction))
            {
                command.Parameters.AddWithValue("@QuantityChange", quantityChange);
                command.Parameters.AddWithValue("@ProduitID", produitId);
                command.ExecuteNonQuery();
            }
        }

        public void DeleteProduit(int id)
        {
            string query = "DELETE FROM Produit WHERE ProduitID = @ProduitID";
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ProduitID", id);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public List<Produit> GetFilteredProduits(List<int> categoryIds, decimal? minPrice, decimal? maxPrice)
        {
            var produits = new List<Produit>();
            var parameters = new List<SqlParameter>();

            string query = @"
                SELECT p.ProduitID, p.Titre, p.Description, p.Prix, p.Capacite, p.ImagePath, p.CategorieID, c.Nom AS CategorieNom
                FROM Produit p
                JOIN Categorie c ON p.CategorieID = c.CategorieID
                WHERE 1 = 1 ";

            if (categoryIds != null && categoryIds.Any())
            {
                var idList = string.Join(",", categoryIds);
                query += $" AND p.CategorieID IN ({idList})";
            }

            if (minPrice.HasValue)
            {
                query += " AND p.Prix >= @MinPrice";
                parameters.Add(new SqlParameter("@MinPrice", SqlDbType.Decimal) { Precision = 10, Scale = 3, Value = minPrice.Value });
            }

            if (maxPrice.HasValue)
            {
                query += " AND p.Prix <= @MaxPrice";
                parameters.Add(new SqlParameter("@MaxPrice", SqlDbType.Decimal) { Precision = 10, Scale = 3, Value = maxPrice.Value });
            }

            query += " ORDER BY p.ProduitID DESC";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddRange(parameters.ToArray());
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        produits.Add(new Produit
                        {
                            ProduitID = reader.GetInt32(0),
                            Titre = reader.GetString(1),
                            Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                            Prix = reader.GetDecimal(3),
                            Capacite = reader.GetInt32(4),
                            ImagePath = reader.IsDBNull(5) ? null : reader.GetString(5),
                            CategorieID = reader.GetInt32(6),
                            CategorieNom = reader.GetString(7)
                        });
                    }
                }
            }
            return produits;
        }
    }
}