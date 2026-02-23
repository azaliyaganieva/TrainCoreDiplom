using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TrainCoreDiplom.DBConnection;
using TrainCoreDiplom.Helpers;

namespace TrainCoreDiplom.Pages
{
    public partial class ForgotPasswordPage : Page
    {
        public ForgotPasswordPage()
        {
            InitializeComponent();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text.Trim();
            string passport = PassportTextBox.Text.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(passport))
            {
                MessageBox.Show("❌ Заполните все поля", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new TrainCoreDiplomEntities1())
                {
                    // ШАГ 1: Проверяем все email в таблице Users
                    var allUsersEmails = db.Users.Select(u => u.Email).ToList();
                    string usersEmailList = string.Join("\n", allUsersEmails);

                    // ШАГ 2: Проверяем все email в таблице Passangers
                    var allPassengersEmails = db.Passangers.Select(p => p.Email).ToList();
                    string passengersEmailList = string.Join("\n", allPassengersEmails);

                    // ШАГ 3: Проверяем все паспорта в таблице Passangers
                    var allPassports = db.Passangers.Select(p => p.Number_passport).ToList();
                    string passportsList = string.Join("\n", allPassports);

                    // Формируем отладочное сообщение
                    string debugInfo = $"🔍 ОТЛАДОЧНАЯ ИНФОРМАЦИЯ:\n\n" +
                                      $"Введенный email: {email}\n" +
                                      $"Введенный паспорт: {passport}\n\n" +
                                      $"=== EMAIL В ТАБЛИЦЕ USERS ===\n{usersEmailList}\n\n" +
                                      $"=== EMAIL В ТАБЛИЦЕ PASSANGERS ===\n{passengersEmailList}\n\n" +
                                      $"=== ПАСПОРТА В ТАБЛИЦЕ PASSANGERS ===\n{passportsList}";

                    // Показываем отладочную информацию
                    MessageBox.Show(debugInfo, "Отладка", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Теперь проверяем конкретного пользователя
                    var user = db.Users.FirstOrDefault(u => u.Email == email);

                    if (user == null)
                    {
                        MessageBox.Show($"❌ Пользователь с email '{email}' не найден в таблице Users",
                                      "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Проверяем пассажира
                    var passenger = db.Passangers.FirstOrDefault(p => p.Email == email);

                    if (passenger == null)
                    {
                        MessageBox.Show($"❌ Пассажир с email '{email}' не найден в таблице Passangers",
                                      "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Проверяем паспорт (учитываем пробелы)
                    string passportFromDb = passenger.Number_passport?.Trim() ?? "";
                    string passportInput = passport.Trim();

                    if (passportFromDb != passportInput)
                    {
                        MessageBox.Show($"❌ Неверный номер паспорта\n\n" +
                                      $"Из БД: '{passportFromDb}'\n" +
                                      $"Введено: '{passportInput}'",
                                      "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Всё правильно - переходим к подтверждению по коду
                    NavigationService.Navigate(new ConfirmCodePage(email, user.Login));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Ошибка: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GenerateRandomPassword()
        {
            const string chars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new LoginPage());
        }
    }
}