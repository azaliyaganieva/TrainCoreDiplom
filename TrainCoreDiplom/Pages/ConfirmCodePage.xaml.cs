using System;
using System.Windows;
using System.Windows.Controls;
using TrainCoreDiplom.DBConnection;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;

namespace TrainCoreDiplom.Pages
{
    public partial class ConfirmCodePage : Page
    {
        private string _userEmail;
        private string _userLogin;
        private string _generatedCode;
        private DateTime _codeExpiration;

        public ConfirmCodePage(string email, string login)
        {
            InitializeComponent();
            _userEmail = email;
            _userLogin = login;
            EmailInfoText.Text = $"Код отправлен на {email}";

            // Генерируем и отправляем код
            SendConfirmationCode();
        }

        private void SendConfirmationCode()
        {
            try
            {
                // Генерируем 6-значный код
                Random random = new Random();
                _generatedCode = random.Next(100000, 999999).ToString();
                _codeExpiration = DateTime.Now.AddMinutes(5); // Код действует 5 минут

                // Отправляем код на почту
                Task.Run(() => SendEmailWithCode());

                MessageBox.Show($"Код подтверждения отправлен на {_userEmail}\nКод действителен 5 минут",
                              "Код отправлен", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка отправки кода: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SendEmailWithCode()
        {
            try
            {
                string yourEmail = "zaya271106@mail.ru";
                string yourPassword = "XOFEDN6vkuV811pun4m1";

                using (var client = new SmtpClient("smtp.mail.ru", 587))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(yourEmail, yourPassword);

                    var mail = new MailMessage();
                    mail.From = new MailAddress(yourEmail, "TrainCore");
                    mail.To.Add(_userEmail);
                    mail.Subject = "Код подтверждения для восстановления пароля";
                    mail.Body = $@"
                        <html>
                        <body style='font-family: Arial;'>
                            <div style='border: 2px solid #1E88E5; padding: 20px; border-radius: 10px;'>
                                <h2 style='color: #1E88E5;'>TrainCore - Восстановление пароля</h2>
                                <p>Здравствуйте!</p>
                                <p>Вы запросили восстановление пароля. Ваш код подтверждения:</p>
                                <p style='font-size: 32px; font-weight: bold; color: #1E88E5; letter-spacing: 5px; text-align: center;'>
                                    {_generatedCode}
                                </p>
                                <p>Код действителен в течение 5 минут.</p>
                                <p>Если вы не запрашивали восстановление пароля, проигнорируйте это письмо.</p>
                                <p>С уважением,<br>Команда TrainCore</p>
                            </div>
                        </body>
                        </html>";
                    mail.IsBodyHtml = true;

                    client.Send(mail);
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Ошибка отправки кода: {ex.Message}", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        private void VerifyButton_Click(object sender, RoutedEventArgs e)
        {
            string enteredCode = CodeTextBox.Text.Trim();

            if (string.IsNullOrEmpty(enteredCode))
            {
                MessageBox.Show("❌ Введите код подтверждения", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (DateTime.Now > _codeExpiration)
            {
                MessageBox.Show("❌ Срок действия кода истек. Запросите новый код", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (enteredCode != _generatedCode)
            {
                MessageBox.Show("❌ Неверный код подтверждения", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Код верный - переходим к смене пароля
            NavigationService.Navigate(new ResetPasswordPage(_userEmail, _userLogin));
        }

        private async void ResendButton_Click(object sender, RoutedEventArgs e)
        {
            ResendButton.IsEnabled = false;
            await Task.Delay(30000); // Блокируем кнопку на 30 секунд

            SendConfirmationCode();
            ResendButton.IsEnabled = true;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}