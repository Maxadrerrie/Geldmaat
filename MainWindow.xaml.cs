using Geldmaat;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GeldAutomaatApp
{
    public partial class LoginWindow : Window
    {
        public Window1 w1 = new Window1();
        public string rekeningnummer = null;
        public LoginWindow()
        {
            InitializeComponent();
        }

        public bool findAccountNumber(string accountNumber)
        {
            string connectionString = "Server=localhost;Uid=root;Pwd=;Database=mydb";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                // Connection is established ....

                // Example: Execute a SQL query
                string sqlQuery = "SELECT * FROM accounts WHERE blocked != 1 AND account_number = '" + accountNumber + "'";
                MySqlCommand cmd = new MySqlCommand(sqlQuery, connection);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (findAccountNumber(AccountNumberTextBox.Text))
            {
                rekeningnummer = AccountNumberTextBox.Text;
                rekeningnummerpanel.Visibility = Visibility.Hidden;
                pin.Visibility = Visibility.Visible;
                PinTextBox.Text = null;

            }
        }

        private void pincode_check(object sender, RoutedEventArgs e)
        {
            string enteredpin = PinTextBox.Text;
            string expectedpin = getPinCode(rekeningnummer);
            if (Tools.VerifySHA256Hash(enteredpin, expectedpin))
            {
                loginn.Visibility = Visibility.Hidden;
                numpad.Visibility = Visibility.Hidden;
                home.Visibility = Visibility.Visible;
                saldo.Text = getBalance(rekeningnummer);


            }
        }
      

        public string getPinCode(string accountNumberString)
        {
            string connectionString = "Server=localhost;Uid=root;Pwd=;Database=mydb";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string sqlQuery = "SELECT pincode FROM accounts WHERE account_number = @accountNumber";
                MySqlCommand cmd = new MySqlCommand(sqlQuery, connection);
                cmd.Parameters.AddWithValue("@accountNumber", accountNumberString);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read()) // Check if a result was returned
                    {
                        return reader.GetString(0); // Assuming 'pin_code' is in the first column
                    }
                }
            }

            return null; // Return null if no PIN code was found
        }
        public string getBalance(string accountNumberString)
        {
            string connectionString = "Server=localhost;Uid=root;Pwd=;Database=mydb";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string sqlQuery = "SELECT balance FROM accounts WHERE account_number = @accountNumber";
                MySqlCommand cmd = new MySqlCommand(sqlQuery, connection);
                cmd.Parameters.AddWithValue("@accountNumber", accountNumberString);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read()) // Check if a result was returned
                    {
                        return reader.GetString(0); // Assuming 'pin_code' is in the first column
                    }
                }
            }

            return null; // Return null if no PIN code was found
        }

        public bool Storten(string accountNumberString, double bedrag)
        {
            string connectionString = "Server=localhost;Uid=root;Pwd=;Database=mydb";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                // Stap 1: Haal het huidige saldo op
                string selectQuery = "SELECT balance FROM accounts WHERE account_number = @accountNumber";
                MySqlCommand selectCmd = new MySqlCommand(selectQuery, connection);
                selectCmd.Parameters.AddWithValue("@accountNumber", accountNumberString);

                double huidigSaldo = 0; // Default saldo als er geen resultaat is
                using (MySqlDataReader reader = selectCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        huidigSaldo = reader.GetDouble(0); // Haal het huidige saldo op
                    }
                }

                // Stap 2: Werk het saldo bij met het gestorte bedrag
                double nieuwSaldo = huidigSaldo + bedrag;
                string updateQuery = "UPDATE accounts SET balance = @nieuwSaldo WHERE account_number = @accountNumber";
                string sqlQuery = "INSERT INTO transactions(rekening_nummer, amount, type) VALUES ('" + accountNumberString + "','" + bedrag.ToString() + "','STORTEN')";
                MySqlCommand updateCmd = new MySqlCommand(updateQuery, connection);
                MySqlCommand updateCmd1 = new MySqlCommand(sqlQuery, connection);
                updateCmd.Parameters.AddWithValue("@nieuwSaldo", nieuwSaldo);
                updateCmd.Parameters.AddWithValue("@accountNumber", accountNumberString);

                updateCmd1.ExecuteNonQuery();
                int rowsAffected = updateCmd.ExecuteNonQuery();

                return rowsAffected > 0; // Als er rijen zijn bijgewerkt, is de transactie gelukt
            }
        }

        public bool OpnemenBedrag(string accountNumberString, double bedrag)
        {
            string connectionString = "Server=localhost;Uid=root;Pwd=;Database=mydb";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                // Stap 1: Haal het huidige saldo op
                string selectQuery = "SELECT balance FROM accounts WHERE account_number = @accountNumber";
                MySqlCommand selectCmd = new MySqlCommand(selectQuery, connection);
                selectCmd.Parameters.AddWithValue("@accountNumber", accountNumberString);

                double huidigSaldo = 0; // Default saldo als er geen resultaat is
                using (MySqlDataReader reader = selectCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        huidigSaldo = reader.GetDouble(0); // Haal het huidige saldo op
                    }
                }

                // Stap 2: Controleer of er voldoende saldo is om op te nemen
                if (huidigSaldo >= bedrag)
                {
                    // Stap 3: Werk het saldo bij met het opgenomen bedrag
                    double nieuwSaldo = huidigSaldo - bedrag;
                    string updateQuery = "UPDATE accounts SET balance = @nieuwSaldo WHERE account_number = @accountNumber";
                    string sqlQuery = "INSERT INTO transactions(rekening_nummer, amount, type) VALUES ('" + accountNumberString + "','" + bedrag.ToString() + "','OPNEMEN')";
                    MySqlCommand updateCmd = new MySqlCommand(updateQuery, connection);
                    MySqlCommand updateCmd1 = new MySqlCommand(sqlQuery, connection);
                    updateCmd.Parameters.AddWithValue("@nieuwSaldo", nieuwSaldo);
                    updateCmd.Parameters.AddWithValue("@accountNumber", accountNumberString);

                    updateCmd1.ExecuteNonQuery();
                    int rowsAffected = updateCmd.ExecuteNonQuery();

                    return rowsAffected > 0; // Als er rijen zijn bijgewerkt, is de transactie gelukt
                }
                else
                {
                    // Niet genoeg saldo om op te nemen
                    return false;
                }
            }
        }

        private void stortenknop_Click(object sender, RoutedEventArgs e)
        {
            home.Visibility = Visibility.Hidden;
            storten.Visibility = Visibility.Visible;
        }

        private void TerugButton_Click(object sender, RoutedEventArgs e)
        {
            storten.Visibility = Visibility.Hidden;
            home.Visibility = Visibility.Visible;
        }

        private void StortenButton_Click(object sender, RoutedEventArgs e)
        {
            double bedrag;

            if (double.TryParse(StortBedragTextBox.Text, out bedrag))
            {
                if (Storten(rekeningnummer, bedrag))
                {
                    MessageBox.Show($"€{bedrag} is succesvol gestort op jouw rekening.");
                    // Bijwerken van de saldo-informatie
                    saldo.Text = getBalance(rekeningnummer);
                    StortBedragTextBox.Clear(); // Leeg het tekstvak voor het bedrag
                }
                else
                {
                    MessageBox.Show("Er is een fout opgetreden bij het storten van geld.");
                }
            }
            else
            {
                MessageBox.Show("Ongeldig bedrag. Voer een geldig bedrag in.");
            }
        }
        public int GetTransactionsOfToday()
        {
            string connectionString = "Server=localhost;Uid=root;Pwd=;Database=mydb";

            List<string> listOutcome = new List<string>();
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"SELECT * FROM `transactions` WHERE `rekening_nummer` = '{rekeningnummer}' AND DAY(`date`) = {DateTime.Now.Day} AND  `type` = 'OPNEMEN'";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            listOutcome.Add(reader.GetString(0));
                        }
                    }
                }
            }
            return listOutcome.Count;
        }
        private void OpnemenButton_Click(object sender, RoutedEventArgs e)
        {
            double bedrag;
            if (GetTransactionsOfToday() >= 3)
            {
                MessageBox.Show("U heeft al 3 transacties gedaan vandaag.");
                return;
            }
            if (double.TryParse(OpnameBedragTextBox.Text, out bedrag))
            {
                if (bedrag > 500)
                {
                    MessageBox.Show("Bedrag mag niet meer zijn dan €500.");
                    return;
                }
                if (OpnemenBedrag(rekeningnummer, bedrag))
                {
                    MessageBox.Show($"€{bedrag} is succesvol opgenomen van jouw rekening.");
                    // Update the balance information
                    saldo.Text = getBalance(rekeningnummer);
                    OpnameBedragTextBox.Clear(); // Clear the amount input field
                }
                else
                {
                    MessageBox.Show("Er is een fout opgetreden bij het opnemen van geld.");
                }
            }
            else
            {
                MessageBox.Show("Ongeldig bedrag. Voer een geldig bedrag in.");
            }
        }

        private void opnemenknop_Click(object sender, RoutedEventArgs e)
        {
            home.Visibility = Visibility.Hidden;
            opnemen.Visibility = Visibility.Visible;
        }

        private void TerugStorten_Click(object sender, RoutedEventArgs e)
        {
            opnemen.Visibility = Visibility.Hidden;
            home.Visibility = Visibility.Visible;
        }

        private void SaldoTerug_Click(object sender, RoutedEventArgs e)
        {
            saldoBekijken.Visibility = Visibility.Hidden;
            home.Visibility = Visibility.Visible;
        }

        private void Saldozien_Click(object sender, RoutedEventArgs e)
        {
            home.Visibility = Visibility.Hidden;
            saldoBekijken.Visibility = Visibility.Visible;
            saldoWeergave.Text = getBalance(rekeningnummer);
        }

        private void Transactiesknop_Click(object sender, RoutedEventArgs e)
        {
            home.Visibility = Visibility.Hidden;
            transacties.Visibility = Visibility.Visible;

            // Maak een exemplaar van de LoginWindow-klasse

            // Roep de methode aan vanuit het exemplaar
            GetLast3Transactions();
        }

        private void GetLast3Transactions()
        {
            string connectionString1 = "Server=localhost;Uid=root;Pwd=;Database=mydb";

/*            string query = "SELECT * FROM transactions ORDER BY date DESC LIMIT 3";*/


            MySqlConnection connection = new MySqlConnection(connectionString1);
                connection.Open();

            string sql = "SELECT * FROM transactions WHERE transactions.rekening_nummer = @Rekeningen_idRekeningen ORDER BY transactions.date DESC LIMIT 3";
            MySqlCommand command = new MySqlCommand(sql, connection);
            command.Parameters.Add("Rekeningen_idRekeningen", MySqlDbType.Int64).Value = rekeningnummer;
            MySqlDataReader reader = command.ExecuteReader();

            int index = 0;
            while (reader.Read())
            {
                TextBlock Timestamp = new TextBlock { Text = Convert.ToString(reader["date"]), Foreground= Brushes.White };
                Grid.SetColumn(Timestamp, 0);
                Grid.SetRow(Timestamp, index);

                TextBlock amount = new TextBlock { Text = Convert.ToString(reader["amount"]), Foreground = Brushes.White };
                Grid.SetColumn(amount, 1);
                Grid.SetRow(amount, index);

                TextBlock type = new TextBlock { Text = (string)reader["type"], Foreground = Brushes.White };
                Grid.SetColumn(type, 2);
                Grid.SetRow(type, index);

                dataGrid.Children.Add(Timestamp);
                dataGrid.Children.Add(amount);
                dataGrid.Children.Add(type);

                index++;
            }
                reader.Close();
        }


        private void NumberButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string number = button.Tag.ToString();

            // Voeg het gekozen cijfer toe aan de TextBox
            AccountNumberTextBox.Text += number;
            PinTextBox.Text += number;
        }

        private void TransactiesTerug_Click(object sender, RoutedEventArgs e)
        {
            transacties.Visibility = Visibility.Hidden;
            home.Visibility = Visibility.Visible;
        }

        private void adminbutton_Click(object sender, RoutedEventArgs e)
        {
            w1.Show();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (AccountNumberTextBox.Text.Length > 0)
            {
                // Als er tekens zijn in de TextBox, verwijder het laatste teken
                AccountNumberTextBox.Text = AccountNumberTextBox.Text.Substring(0, AccountNumberTextBox.Text.Length - 1);
                PinTextBox.Text = PinTextBox.Text.Substring(0, PinTextBox.Text.Length - 1);
            }
        }

        private void EnterButton_Click(object sender, RoutedEventArgs e)
        {
            LoginButton_Click(sender, e);
            pincode_check(sender, e);
        }
    }
}

