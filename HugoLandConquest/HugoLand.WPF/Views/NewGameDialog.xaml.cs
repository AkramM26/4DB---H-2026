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
            //Stop musique Menu
            AudioManager.MenuSound.Stop();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            //Button sound 
            AudioManager.MenuSound.Play();

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
            //Button sound 
            AudioManager.MenuSound.Play();
            DialogResult = false;
        }
    }
}
