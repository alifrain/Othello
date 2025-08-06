using System;
using System.Windows;

namespace OthelloWPF
{
    public partial class PlayerNameDialog : Window
    {
        public string Player1Name { get; private set; } = string.Empty;
        public string Player2Name { get; private set; } = string.Empty;

        public PlayerNameDialog()
        {
            InitializeComponent();

            Player1TextBox.Focus();
            Player1TextBox.SelectAll();

            Player1TextBox.KeyDown += (s, e) =>
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                {
                    Player2TextBox.Focus();
                    Player2TextBox.SelectAll();
                }
            };

            Player2TextBox.KeyDown += (s, e) =>
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                {
                    StartGameButton_Click(this, new RoutedEventArgs());
                }
            };
        }

        private void StartGameButton_Click(object sender, RoutedEventArgs e)
        {
            string player1 = Player1TextBox.Text?.Trim() ?? string.Empty;
            string player2 = Player2TextBox.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(player1))
            {
                MessageBox.Show("Please enter a name for Player 1!", "Invalid Input",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                Player1TextBox.Focus();
                return;
            }

            if (string.IsNullOrEmpty(player2))
            {
                MessageBox.Show("Please enter a name for Player 2!", "Invalid Input",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                Player2TextBox.Focus();
                return;
            }

            if (player1.Equals(player2, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Player names must be different!", "Invalid Input",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                Player2TextBox.Focus();
                Player2TextBox.SelectAll();
                return;
            }

            Player1Name = player1;
            Player2Name = player2;

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}