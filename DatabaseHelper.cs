using MySql.Data.MySqlClient;

namespace BillingApp
{
    public static class DatabaseHelper
    {
        // make sure this matches the actual database name on your MySQL server
        private static string connectionString =
            "server=localhost;database=billingappdb;user=root;password=2004;";

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }

        public static void InitializeDatabase()
        {
            using (var con = GetConnection())
            {
                con.Open();

                // Create bills table
                string createBills = @"
                CREATE TABLE IF NOT EXISTS `bills` (
                  `Id` BIGINT NOT NULL AUTO_INCREMENT,
                  `CustomerName` VARCHAR(255) NULL,
                  `Amount` DECIMAL(18,2) NOT NULL DEFAULT 0,
                  `BillDate` DATETIME NULL,
                  PRIMARY KEY (`Id`)
                ) ENGINE=InnoDB;";

                using (var cmd = new MySqlCommand(createBills, con))
                {
                    cmd.ExecuteNonQuery();
                }

                // Create billitems table
                string createItems = @"
                CREATE TABLE IF NOT EXISTS `billitems` (
                  `Id` BIGINT NOT NULL AUTO_INCREMENT,
                  `BillId` BIGINT NOT NULL,
                  `ItemName` VARCHAR(255) NULL,
                  `Price` DECIMAL(18,2) NOT NULL DEFAULT 0,
                  `Qty` INT NOT NULL DEFAULT 1,
                  `Image` VARCHAR(255) NULL,
                  `Cgst` DECIMAL(7,2) NOT NULL DEFAULT 0,
                  `Sgst` DECIMAL(7,2) NOT NULL DEFAULT 0,
                  `Total` DECIMAL(18,2) NOT NULL DEFAULT 0,
                  PRIMARY KEY (`Id`),
                  INDEX (`BillId`),
                  CONSTRAINT `fk_billitems_bills` FOREIGN KEY (`BillId`) REFERENCES `bills`(`Id`) ON DELETE CASCADE
                ) ENGINE=InnoDB;";

                using (var cmd = new MySqlCommand(createItems, con))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void UpdateBillDate(long billId, System.DateTime newDate)
        {
            using (var con = GetConnection())
            {
                con.Open();
                using (var cmd = new MySqlCommand("UPDATE bills SET BillDate = @Date WHERE Id = @Id", con))
                {
                    cmd.Parameters.AddWithValue("@Date", newDate);
                    cmd.Parameters.AddWithValue("@Id", billId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}