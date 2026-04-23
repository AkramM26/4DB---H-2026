using System.Windows;

namespace HugoLand.WPF
{
    public partial class NewGameDialog : Window
    {
        public string GameName { get; private set; } = string.Empty;
        public string GameDescription { get; private set; } = string.Empty;

        public NewGameDialog()
        {
            InitializeComponent();
            txtName.Text = $"Game-{System.DateTime.Now:yyyyMMdd-HHmmss}";
            txtName.Focus();
            txtName.SelectAll();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Game name cannot be empty.", "Invalid input",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            GameName = txtName.Text.Trim();
            GameDescription = txtDescription.Text?.Trim() ?? string.Empty;
            DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
