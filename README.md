<h1 align="center"> 👩‍💻  XTREMEPC E-commerce Platform  👩‍💻 </h1>
This project is a modern e-commerce application built on ASP.NET Core (.NET 9.0) utilizing ADO.NET for direct database access and featuring integration with Web 3.0 concepts for secure order and payment simulation.

## 🛠️ Project Setup

### 1. Prerequisites
Ensure you have the following installed:

- Visual Studio 2022 (with ASP.NET and web development workload)
- .NET 9.0 SDK
- Visual Studio
- SQL Server Management Studio (SSMS)
- Truffle (for the Blockchain )

### 2. Database Setup (SQL Server)
The application uses raw ADO.NET, so the database schema must be initialized manually.

#### A. Create the Database
1. Open SSMS and create a new database (e.g., `projetExamenTP` or the name used in your connection string).  
2. Execute the following DDL script to create all necessary tables:

```sql
-- 1. Create Categorie Table
CREATE TABLE Categorie (
    CategorieID INT PRIMARY KEY IDENTITY(1,1),
    Nom NVARCHAR(100) NOT NULL UNIQUE
);

-- 2. Create Utilisateur Table
CREATE TABLE Utilisateur (
    UtilisateurID INT PRIMARY KEY IDENTITY(1,1),
    Nom NVARCHAR(50) NOT NULL,
    Prenom NVARCHAR(50) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    MotDePasse NVARCHAR(255) NOT NULL,
    Role NVARCHAR(20) NOT NULL -- 'Admin', 'Client'
);

-- 3. Create Produit Table (Products/Inventory)
CREATE TABLE Produit (
    ProduitID INT PRIMARY KEY IDENTITY(1,1),
    Titre NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX),
    Prix DECIMAL(10, 3) NOT NULL,
    Capacite INT NOT NULL, -- Stock quantity
    ImagePath NVARCHAR(255),
    CategorieID INT NOT NULL,
    FOREIGN KEY (CategorieID) REFERENCES Categorie(CategorieID)
);

-- 4. Create Commande (Order) Table
CREATE TABLE Commande (
    CommandeID INT PRIMARY KEY IDENTITY(1,1),
    UtilisateurID INT NOT NULL,
    DateCommande DATETIME NOT NULL DEFAULT GETDATE(),
    Total DECIMAL(10, 3) NOT NULL,
    Etat VARCHAR(50) NOT NULL DEFAULT 'Processing',
    AdresseExpedition NVARCHAR(255) NOT NULL,
    Ville NVARCHAR(100) NOT NULL,
    CodePostal NVARCHAR(20) NOT NULL,
    Telephone NVARCHAR(20),
    MethodeLivraison NVARCHAR(50),
    FOREIGN KEY (UtilisateurID) REFERENCES Utilisateur(UtilisateurID)
);

-- 5. Create LigneCommande (Order Line/Item) Table
CREATE TABLE LigneCommande (
    LigneCommandeID INT PRIMARY KEY IDENTITY(1,1),
    CommandeID INT NOT NULL,
    ProduitID INT NOT NULL,
    Quantite INT NOT NULL,
    PrixUnitaire DECIMAL(10, 3) NOT NULL, -- Price at time of order
    FOREIGN KEY (CommandeID) REFERENCES Commande(CommandeID) ON DELETE CASCADE,
    FOREIGN KEY (ProduitID) REFERENCES Produit(ProduitID)
);


```


### B. Configure Connection String

Update the DefaultConnection string in your appsettings.json file to match your local SQL Server instance:
```
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER_NAME\\SQLEXPRESS;Database=examen_ecomerce;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True" 
}

```












# 🚀 Key Features

The application provides two main user roles: **Client** and **Administrator**.

---

## 🧑‍💻 Client (Standard User)

- **Product Catalog**: Browse products with filtering by Category, Price Range, and Pagination (12 items per page)
- **Shopping Cart**: Add, remove, and update product quantities in a session-based cart
- **Checkout**: Enter shipping details and complete the purchase
- **Web 3.0 Integration**: Orders communicate with the local Blockchain service for secure confirmation and ledger entry
- **User Profile**: View account details and detailed Order History (items, shipping, status)

---

## 👑 Administrator (ADMIN)

- **Dashboard**: Centralized access for all management features
- **Product Management**: CRUD operations on products, with pagination and modal confirmation for deletions
- **Category Management**: CRUD operations on categories, with modal confirmation
- **Order Management**: View Orders List with status (Processing, Delivered), detailed order views, and ability to confirm delivery





## 😄 Demo Pictures



|<img width="1902" height="931" alt="Screenshot 2025-12-12 222140" src="https://github.com/user-attachments/assets/b45b7a32-41be-4951-b23e-29fac642b47f" />|<img width="1894" height="927" alt="Screenshot 2025-12-12 222151" src="https://github.com/user-attachments/assets/252b5345-e962-4428-9c52-5690526f907f" />|<img width="1899" height="937" alt="Screenshot 2025-12-12 222201" src="https://github.com/user-attachments/assets/547c3f2a-c2b8-4f90-a727-ca1a30fb7bcf" />|
|---------|---------|---------|





|<img width="1898" height="942" alt="Screenshot 2025-12-12 222216" src="https://github.com/user-attachments/assets/554c95c3-c052-4750-934e-decd62f30d26" />|<img width="1894" height="949" alt="Screenshot 2025-12-12 222229" src="https://github.com/user-attachments/assets/ab5a946a-21cc-413d-bccc-07d10746d31c" />|<img width="1905" height="942" alt="Screenshot 2025-12-12 222358" src="https://github.com/user-attachments/assets/13db5a6b-2eed-4466-867b-dd662e49251d" />|
|---------|---------|---------|

|<img width="1906" height="942" alt="Screenshot 2025-12-12 222451" src="https://github.com/user-attachments/assets/f1049288-6b1e-4ac4-bbb8-a84d16b6a7ac" />|<img width="1900" height="942" alt="Screenshot 2025-12-12 222622" src="https://github.com/user-attachments/assets/a5e69a25-e82e-4949-850c-a029444a4658" />|<img width="1908" height="942" alt="Screenshot 2025-12-12 222637" src="https://github.com/user-attachments/assets/7b7b3615-9fa6-4532-b116-46f80545a0ee" />|
|---------|---------|---------|


|<img width="1897" height="950" alt="Screenshot 2025-12-12 222723" src="https://github.com/user-attachments/assets/6612449d-59e8-4b5b-b2ac-73f9dbad865e" />|<img width="1900" height="946" alt="Screenshot 2025-12-12 222740" src="https://github.com/user-attachments/assets/18c47c4b-9bbc-4f44-b40f-a4400be02110" />|<img width="1898" height="936" alt="Screenshot 2025-12-12 222749" src="https://github.com/user-attachments/assets/d4494023-3618-40c0-bd0e-a1482e575cec" />|
|---------|---------|---------|






|<img width="1902" height="942" alt="Screenshot 2025-12-12 222811" src="https://github.com/user-attachments/assets/255c1d94-6f19-4da6-9ef2-8085ca11709b" />|<img width="1897" height="942" alt="Screenshot 2025-12-12 222822" src="https://github.com/user-attachments/assets/cf26dfa3-7206-479e-b309-126777b195d9" />|<img width="1898" height="948" alt="Screenshot 2025-12-12 222834" src="https://github.com/user-attachments/assets/d9e875ca-877c-41cf-a381-7d1d83453dbb" />|
|---------|---------|---------|




## 😄 Demo Video



https://github.com/user-attachments/assets/44f4ae95-5bff-4eb6-a949-bd6d3c56bc57













































## 🚀 Acknowledgements


**- ADO .NET**  

**- Solidity** 

**- Web3.js** 







 












⚡️ [![MIT License](https://img.shields.io/badge/License-MIT-green.svg)](https://choosealicense.com/licenses/mit/)
⚡️ [![GPLv3 License](https://img.shields.io/badge/License-GPL%20v3-yellow.svg)](https://opensource.org/licenses/)
⚡️ [![AGPL License](https://img.shields.io/badge/license-AGPL-blue.svg)](http://www.gnu.org/licenses/agpl-3.0)









## 📩 Contact

If you have any questions or the project source code or any need additional assistance, please feel free to contact me.

https://t.me/MoezKr/
