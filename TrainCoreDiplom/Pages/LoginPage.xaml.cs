using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TrainCoreDiplom.DBConnection;
using TrainCoreDiplom.Helpers;

namespace TrainCoreDiplom.Pages
{
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new TrainCoreDiplomEntities1())
                {
                    // Ищем пользователя по логину
                    var user = db.Users.FirstOrDefault(u => u.Login == login);

                    if (user == null)
                    {
                        MessageBox.Show($"Пользователь '{login}' не найден", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (user.IsActive != true)
                    {
                        MessageBox.Show($"Пользователь '{login}' заблокирован", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    string inputHash = PasswordHelper.HashPassword(password);

                    if (user.PasswordHash == inputHash)
                    {
                        App.CurrentUser = user;
                        user.LastLogin = DateTime.Now;
                        db.SaveChanges();

                        string roleName = "Пользователь";
                        if (user.Role == 1) roleName = "Администратор";
                        else if (user.Role == 2) roleName = "Менеджер";

                        MessageBox.Show($"Добро пожаловать, {login}!\nВаша роль: {roleName}",
                            "Успешный вход", MessageBoxButton.OK, MessageBoxImage.Information);

                        if (user.Role == 1 || user.Role == 2)
                        {
                            NavigationService.Navigate(new AdminPages.AdminDashboardPage());
                        }
                        else
                        {
                            NavigationService.Navigate(new TrainSearchPage());
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Неверный пароль для пользователя '{login}'",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения к БД: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ForgotPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ForgotPasswordPage());
        }
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new RegisterPage());
        }
    }
}