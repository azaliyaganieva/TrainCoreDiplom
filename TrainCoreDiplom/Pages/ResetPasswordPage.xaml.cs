using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TrainCoreDiplom.DBConnection;
using TrainCoreDiplom.Helpers;

namespace TrainCoreDiplom.Pages
{
    public partial class ResetPasswordPage : Page
    {
        private string _userEmail;
        private string _userLogin;

        public ResetPasswordPage(string email, string login)
        {
            InitializeComponent();
            _userEmail = email;
            _userLogin = login;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            string newPassword = NewPasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            if (string.IsNullOrEmpty(newPassword))
            {
                MessageBox.Show("❌ Введите новый пароль", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (newPassword.Length < 6)
            {
                MessageBox.Show("❌ Пароль должен содержать минимум 6 символов", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (newPassword != confirmPassword)
            {
                MessageBox.Show("❌ Пароли не совпадают", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new TrainCoreDiplomEntities1())
                {
                    // Ищем пользователя по email (который передали со страницы подтверждения)
                    var user = db.Users.FirstOrDefault(u => u.Email == _userEmail);

                    if (user == null)
                    {
                        MessageBox.Show("❌ Пользователь не найден", "Ошибка",
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Сохраняем НОВЫЙ пароль (хешируем его)
                    user.PasswordHash = PasswordHelper.HashPassword(newPassword);

                    // Сохраняем изменения в БД
                    db.SaveChanges();

                    MessageBox.Show("✅ Пароль успешно изменен!\nТеперь вы можете войти с новым паролем",
                                  "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    NavigationService.Navigate(new LoginPage());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Ошибка: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}