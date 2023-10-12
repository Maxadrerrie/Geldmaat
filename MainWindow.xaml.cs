using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Geldmaat;
using MySql.Data.MySqlClient;

namespace GeldAutomaatApp
{
    public partial class LoginWindow : Window
    {
        private readonly Window1 w1 = new Window1();
        private string rekeningnummer = null;

        public LoginWindow()
        {
            InitializeComponent();
        }

        private bool FindAccountNumber(string accountNumber)
        {
            string connectionString = "Server=localhost;Uid=root;Pwd=;Database=mydb";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string sqlQuery = "SELECT * FROM accounts WHERE blocked != 1 AND account_number = @accountNumber";
                MySqlCommand cmd = new MySqlCommand(sqlQuery, connection);
                cmd.Parameters.AddWithValue("@accountNumber", accountNumber);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    return reader.HasRows;
                }
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (FindAccountNumber(AccountNumberTextBox.Text))
            {
                rekeningnummer = AccountNumberTextBox.Text;
                rekeningnummerpanel.Visibility = Visibility.Hidden;
                pin.Visibility = Visibility.Visible;
                PinTextBox.Text = null;
            }
        }

        private void PincodeCheck(object sender, RoutedEventArgs e)
        {
            string enteredPin = PinTextBox.Text;
            string expectedPin = GetPinCode(rekeningnummer);

            if (Tools.VerifySHA256Hash(enteredPin, expectedPin))
            {
                loginn.Visibility = Visibility.Hidden;
                numpad.Visibility = Visibility.Hidden;
                home.Visibility = Visibility.Visible;
                saldo.Text = GetBalance(rekeningnummer);
            }
        }

        private string GetPinCode(string accountNumberString)
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
                    return reader.Read() ? reader.GetString(0) : null;
                }
            }
        }

        private string GetBalance(string accountNumberString)
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
                    return reader.Read() ? reader.GetString(0) : null;
                }
            }
        }

        // Other methods...

        private void NumberButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string number = button.Tag.ToString();

            AccountNumberTextBox.Text += number;
            PinTextBox.Text += number;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (AccountNumberTextBox.Text.Length > 0)
            {
                AccountNumberTextBox.Text = AccountNumberTextBox.Text.Substring(0, AccountNumberTextBox.Text.Length - 1);
                PinTextBox.Text = PinTextBox.Text.Substring(0, PinTextBox.Text.Length - 1);
            }
        }

        private void EnterButton_Click(object sender, RoutedEventArgs e)
        {
            LoginButton_Click(sender, e);
            PincodeCheck(sender, e);
        }
    }
}
