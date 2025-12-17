using TestWithADO.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace TestWithADO.Services
{
    public class CategorieService
    {
        private readonly string _connectionString;

        public CategorieService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public List<Categorie> GetAllCategories()
        {
            var categories = new List<Categorie>();
            string query = "SELECT CategorieID, Nom FROM Categorie ORDER BY Nom";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        categories.Add(new Categorie
                        {
                            CategorieID = reader.GetInt32(0),
                            Nom = reader.GetString(1)
                        });
                    }
                }
            }
            return categories;
        }

        public Categorie GetCategorieById(int id)
        {
            Categorie categorie = null;
            string query = "SELECT CategorieID, Nom FROM Categorie WHERE CategorieID = @CategorieID";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@CategorieID", id);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        categorie = new Categorie
                        {
                            CategorieID = reader.GetInt32(0),
                            Nom = reader.GetString(1)
                        };
                    }
                }
            }
            return categorie;
        }

        public void AddCategorie(Categorie categorie)
        {
            string query = "INSERT INTO Categorie (Nom) VALUES (@Nom)";
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Nom", categorie.Nom);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void UpdateCategorie(Categorie categorie)
        {
            string query = "UPDATE Categorie SET Nom = @Nom WHERE CategorieID = @CategorieID";
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Nom", categorie.Nom);
                command.Parameters.AddWithValue("@CategorieID", categorie.CategorieID);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void DeleteCategorie(int id)
        {
            string query = "DELETE FROM Categorie WHERE CategorieID = @CategorieID";
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@CategorieID", id);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}