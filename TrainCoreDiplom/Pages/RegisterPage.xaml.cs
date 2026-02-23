using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using TrainCoreDiplom.DBConnection;
using TrainCoreDiplom.Helpers;

namespace TrainCoreDiplom.Pages
{
    public partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                using (var db = new TrainCoreDiplomEntities1())
                {
                    if (db.Users.Any(u => u.Login == LoginTextBox.Text.Trim()))
                    {
                        MessageBox.Show("Пользователь с таким логином уже существует",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (db.Users.Any(u => u.Email == EmailTextBox.Text.Trim()))
                    {
                        MessageBox.Show("❌ Пользователь с таким email уже существует",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var user = new Users
                    {
                        Login = LoginTextBox.Text.Trim(),
                        PasswordHash = PasswordHelper.HashPassword(PasswordBox.Password),
                        Email = EmailTextBox.Text.Trim(),
                        Role = 3,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    };

                    db.Users.Add(user);
                    db.SaveChanges();

                    if (!string.IsNullOrWhiteSpace(PassportTextBox.Text))
                    {
                        var passenger = new Passangers
                        {
                            Name_Pas = FirstNameTextBox.Text.Trim(),
                            Fam_Pas = LastNameTextBox.Text.Trim(),
                            Email = EmailTextBox.Text.Trim(),
                            Phone = PhoneTextBox.Text.Trim(),
                            Number_passport = PassportTextBox.Text.Trim()
                        };
                        db.Passangers.Add(passenger);
                        db.SaveChanges();
                    }

                    MessageBox.Show("Регистрация прошла успешно! Теперь вы можете войти в систему.",
                                  "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    NavigationService.Navigate(new LoginPage());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при регистрации: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(FirstNameTextBox.Text))
            {
                MessageBox.Show("Введите имя", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                FirstNameTextBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(LastNameTextBox.Text))
            {
                MessageBox.Show("Введите фамилию", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                LastNameTextBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                MessageBox.Show("Введите email", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                EmailTextBox.Focus();
                return false;
            }

            string email = EmailTextBox.Text.Trim();
            if (!System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Введите корректный email адрес", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                EmailTextBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(LoginTextBox.Text))
            {
                MessageBox.Show("Введите логин", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                LoginTextBox.Focus();
                return false;
            }

            if (LoginTextBox.Text.Length < 3)
            {
                MessageBox.Show("Логин должен содержать минимум 3 символа", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                LoginTextBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                MessageBox.Show("Введите пароль", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                PasswordBox.Focus();
                return false;
            }

            if (PasswordBox.Password.Length < 6)
            {
                MessageBox.Show("Пароль должен содержать минимум 6 символов", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                PasswordBox.Focus();
                return false;
            }

            if (PasswordBox.Password != ConfirmPasswordBox.Password)
            {
                MessageBox.Show("Пароли не совпадают", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                ConfirmPasswordBox.Focus();
                return false;
            }

            if (AgreeCheckBox.IsChecked != true)
            {
                MessageBox.Show("Необходимо согласиться с условиями использования",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        
        private void BackToLoginButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new LoginPage());
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}