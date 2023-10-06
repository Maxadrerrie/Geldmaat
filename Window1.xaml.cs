using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Geldmaat;
using MySql.Data.MySqlClient;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Security.Cryptography;



namespace Geldmaat
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        Tools tls = new Tools();

        public Window1()
        {
            InitializeComponent();
        }
        public string rekeningnummer = null;
        public object Messagebox { get; private set; }

        private void NumberButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string number = button.Tag.ToString();

            // Voeg het gekozen cijfer toe aan de TextBox
            AccountNumberTextBox.Text += number;
            PinTextBox.Text += number;
        }

        public bool findAccountNumber(string accountNumber)
        {
            string connectionString = "Server=localhost;Uid=root;Pwd=;Database=mydb";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                // Connection is established

                // Example: Execute a SQL query
                string sqlQuery = "SELECT * FROM accounts WHERE account_number = '" + accountNumber + "'";
                MySqlCommand cmd = new MySqlCommand(sqlQuery, connection);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("Foute gebruikersnaam");
                        return false;
                    }
                }
            }
        }
        public bool findEmployee(string accountNumber)
        {
            string connectionString = "Server=localhost;Uid=root;Pwd=;Database=mydb";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                // Connection is established

                // Example: Execute a SQL query
                string sqlQuery = "SELECT * FROM employees WHERE name = '" + accountNumber + "'";
                MySqlCommand cmd = new MySqlCommand(sqlQuery, connection);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("Foute gebruikersnaam");
                        return false;
                    }
                }
            }
        }

        public string getPinCode(string accountNumberString)
        {
            string connectionString = "Server=localhost;Uid=root;Pwd=;Database=mydb";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string sqlQuery = "SELECT password FROM employees WHERE name = @accountNumber";
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

        private void checkAccountPincodeButtonClick(object sender, RoutedEventArgs e)
        {
            string enteredpin = PinTextBox.Text;
            string expectedpin = getPinCode(rekeningnummer);
            MessageBox.Show(expectedpin);
            if (Tools.VerifySHA256Hash(enteredpin, expectedpin))
            {
                pin.Visibility = Visibility.Hidden;
                home.Visibility = Visibility.Visible;
                MessageBox.Show("Goed");

            }
            else
            {
                MessageBox.Show("Fout");

            }
        }


        private void checkAccountNameButtonClick(object sender, RoutedEventArgs e)
        {
            if (findEmployee(AccountNumberTextBox.Text))
            {
                rekeningnummer = AccountNumberTextBox.Text;
                rekeningnummerpanel.Visibility = Visibility.Hidden;
                pin.Visibility = Visibility.Visible;
                PinTextBox.Text = null;

            }
        }


        public bool insertAccoount(string accountNumberString, string pincode)
        {
            string connectionString = "Server=localhost;Uid=root;Pwd=;Database=mydb";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string sqlQuery = "INSERT INTO accounts (account_number,pincode         ) VALUES ('" + accountNumberString + "','" + pincode + "')";

                MySqlCommand cmd = new MySqlCommand(sqlQuery, connection);

                int result = cmd.ExecuteNonQuery();

                if (result > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private void Rekening_toevoegen_knop(object sender, RoutedEventArgs e)
        {
            rekeningtoevoegen.Visibility = Visibility.Visible;
            home.Visibility = Visibility.Hidden;
        }

        private void accounttoevoegen_Click(object sender, RoutedEventArgs e)
        {
            string hash = tls.CreateSHA256Hash(pincodeBox.Text);

            if (insertAccoount(accountNumberBox.Text, hash))
            {
                MessageBox.Show("Gelukt");

                accountNumberBox.Text = null;
                pincodeBox.Text = null;

                home.Visibility = Visibility.Visible;
                rekeningtoevoegen.Visibility = Visibility.Hidden;
            }
        }

        public bool ChangePinCode(string accountNumber, string newPin)
        {
            string connectionString = "Server=localhost;Uid=root;Pwd=;Database=mydb";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                // Check if the account number exists
                if (!findAccountNumber(accountNumber))
                {
                    MessageBox.Show("Account not found");
                    return false;
                }

                // Update the PIN code
                string hashedPin = tls.CreateSHA256Hash(newPin);
                string sqlQuery = "UPDATE accounts SET pincode = @newPin WHERE account_number = @accountNumber";

                MySqlCommand cmd = new MySqlCommand(sqlQuery, connection);
                cmd.Parameters.AddWithValue("@newPin", hashedPin);
                cmd.Parameters.AddWithValue("@accountNumber", accountNumber);

                int result = cmd.ExecuteNonQuery();

                if (result > 0)
                {
                    MessageBox.Show("Pincode successfully changed");
                    return true;
                }
                else
                {
                    MessageBox.Show("Failed to change pincode");
                    return false;
                }
            }
        }

        private void pincode_button_click(object sender, RoutedEventArgs e)
        {

            home.Visibility = Visibility.Hidden;
            pinwijzigen.Visibility = Visibility.Visible;
        }

        private void pincode_wijzig_Click(object sender, RoutedEventArgs e)
        {
            ChangePinCode(rekeningnummer_pincode_wijzigen.Text, pincode_wijzigen.Text);
            pinwijzigen.Visibility = Visibility.Hidden;
            home.Visibility = Visibility.Visible;
        }

        private void Blokkeren_Click(object sender, RoutedEventArgs e)
        {
            string accountNumberToBlock = rekeningnummerBlokkeren.Text;

            if (BlockAccount(accountNumberToBlock))
            {
                MessageBox.Show("Rekening succesvol geblokkeerd");
                Blokkeren.Visibility = Visibility.Hidden;
                home.Visibility = Visibility.Visible;

            }
            else
            {
                MessageBox.Show("Fout bij het blokkeren van de rekening");
            }
        }

        public bool BlockAccount(string accountNumber)
        {
            string connectionString = "Server=localhost;Uid=root;Pwd=;Database=mydb";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                // Check if the account number exists
                if (!findAccountNumber(accountNumber))
                {
                    MessageBox.Show("Rekening niet gevonden");
                    return false;
                }

                // Update the 'blocked' status to indicate the account is blocked
                string sqlQuery = "UPDATE accounts SET blocked = '1' WHERE account_number = @accountNumber";

                MySqlCommand cmd = new MySqlCommand(sqlQuery, connection);
                cmd.Parameters.AddWithValue("@accountNumber", accountNumber);

                int result = cmd.ExecuteNonQuery();

                if (result > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private void blokkerengrid_Click(object sender, RoutedEventArgs e)
        {
            home.Visibility = Visibility.Hidden;
            Blokkeren.Visibility = Visibility.Visible;
        }

        private void stortenButton_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(amountTextBox.Text, out double amount))
            {
                rekeningnummerTextBox.Text = rekeningnummer;
                UpdateBalance(amount);
                MessageBox.Show($"Geld gestort: {amount:C}");
            }
            else
            {
                MessageBox.Show("Ongeldig bedrag. Voer een geldig bedrag in.");
            }
        }

        private void afhalenButton_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(amountTextBox.Text, out double amount))
            {
                rekeningnummer  = rekeningnummerTextBox.Text;
                UpdateBalance(-amount);
                MessageBox.Show($"Geld afgehaald: {amount:C}");
            }
            else
            {
                MessageBox.Show("Ongeldig bedrag. Voer een geldig bedrag in.");
            }
        }

        public void UpdateBalance(double amount)
        {
            string connectionString = "Server=localhost;Uid=root;Pwd=;Database=mydb";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string sqlQuery = "UPDATE accounts SET balance = balance + @amount WHERE account_number = @accountNumber";

                MySqlCommand cmd = new MySqlCommand(sqlQuery, connection);
                cmd.Parameters.AddWithValue("@amount", amount);
                cmd.Parameters.AddWithValue("@accountNumber", rekeningnummer);

                int result = cmd.ExecuteNonQuery();

                if (result > 0)
                {
                    // Update het weergegeven saldo
                    currentBalance.Text = $"Huidig saldo: {(GetBalance(rekeningnummer)):C}";
                }
                else
                {
                    MessageBox.Show("Saldo aanpassen mislukt");
                }
            }
        }

        public double GetBalance(string accountNumber)
        {
            string connectionString = "Server=localhost;Uid=root;Pwd=;Database=mydb";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string sqlQuery = "SELECT balance FROM accounts WHERE account_number = @accountNumber";
                MySqlCommand cmd = new MySqlCommand(sqlQuery, connection);
                cmd.Parameters.AddWithValue("@accountNumber", accountNumber);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return reader.GetDouble(0);
                    }
                }
            }

            return 0;
        }

        private void stortenknop_Click(object sender, RoutedEventArgs e)
        {
            home.Visibility = Visibility.Collapsed;
            saldoaanpassen.Visibility = Visibility.Visible;
        }

        private void TerugStorten_Click(object sender, RoutedEventArgs e)
        {
            saldoaanpassen.Visibility = Visibility.Collapsed;
            home.Visibility = Visibility.Visible;
        }

        private void BlokkerenTerug_Click(object sender, RoutedEventArgs e)
        {
            Blokkeren.Visibility = Visibility.Collapsed;
            home.Visibility = Visibility.Visible;
        }

        private void ToevoegenTerug_Click(object sender, RoutedEventArgs e)
        {
            rekeningtoevoegen.Visibility = Visibility.Collapsed;
            home.Visibility = Visibility.Visible;
        }

        private void pincodeterug_Click(object sender, RoutedEventArgs e)
        {
            pinwijzigen.Visibility = Visibility.Collapsed;
            home.Visibility = Visibility.Visible;
        }
    }
}