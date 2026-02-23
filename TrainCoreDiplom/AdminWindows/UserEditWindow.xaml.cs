using System;
using System.Windows;
using TrainCoreDiplom.DBConnection;
using TrainCoreDiplom.Helpers;

namespace TrainCoreDiplom.AdminWindows
{
    public partial class UserEditWindow : Window
    {
        private Users _user;
        private bool _isEdit;

        public UserEditWindow(Users user = null)
        {
            InitializeComponent();

            if (user != null)
            {
                _user = user;
                _isEdit = true;
                LoginTextBox.Text = user.Login;
                EmailTextBox.Text = user.Email;
                IsActiveCheckBox.IsChecked = user.IsActive;
                PasswordBox.IsEnabled = false;
                ConfirmPasswordBox.IsEnabled = false;
                Title = "Редактирование пользователя";
            }
            else
            {
                _isEdit = false;
                Title = "Добавление пользователя";
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LoginTextBox.Text))
            {
                MessageBox.Show("Введите логин", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                MessageBox.Show("Введите email", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!_isEdit)
            {
                if (string.IsNullOrWhiteSpace(PasswordBox.Password))
                {
                    MessageBox.Show("Введите пароль", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (PasswordBox.Password != ConfirmPasswordBox.Password)
                {
                    MessageBox.Show("Пароли не совпадают", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            try
            {
                using (var db = new TrainCoreDiplomEntities1())
                {
                    if (_isEdit)
                    {
                        var user = db.Users.Find(_user.ID_User);
                        if (user != null)
                        {
                            user.Login = LoginTextBox.Text.Trim();
                            user.Email = EmailTextBox.Text.Trim();
                            user.IsActive = IsActiveCheckBox.IsChecked;
                        }
                    }
                    else
                    {
                        var newUser = new Users
                        {
                            Login = LoginTextBox.Text.Trim(),
                            Email = EmailTextBox.Text.Trim(),
                            PasswordHash = PasswordHelper.HashPassword(PasswordBox.Password),
                            Role = 3,
                            IsActive = IsActiveCheckBox.IsChecked,
                            CreatedAt = DateTime.Now
                        };
                        db.Users.Add(newUser);
                    }
                    db.SaveChanges();
                }

                MessageBox.Show("Пользователь сохранен", "Успех",
                              MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}