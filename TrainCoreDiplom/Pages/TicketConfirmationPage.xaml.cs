using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TrainCoreDiplom.DBConnection;
using System.IO;
using ZXing;  // Для штрих-кода
using ZXing.Common;
using System.Drawing;
using System.Drawing.Imaging;

namespace TrainCoreDiplom.Pages
{
    public partial class TicketConfirmationPage : Page
    {
        private Tickets _ticket;
        private Schedule _schedule;
        private Seats _seat;
        private Wagons _wagon;
        private string _passengerName;
        private string _passengerEmail;
        private string _passengerDocument;
        private bool _emailSent = false;
        private decimal _totalPrice;
        private Stations _fromStation;
        private Stations _toStation;

        public TicketConfirmationPage(Schedule schedule, Seats seat, Wagons wagon, DateTime date,
                                      string lastName, string firstName, string documentNumber, string email,
                                      Stations fromStation, Stations toStation)
        {
            InitializeComponent();
            _schedule = schedule;
            _seat = seat;
            _wagon = wagon;
            _passengerName = $"{lastName} {firstName}";
            _passengerDocument = documentNumber;
            _passengerEmail = email;
            _totalPrice = seat.Price;
            _fromStation = fromStation;
            _toStation = toStation;

            SaveTicketToDatabase(date, lastName, firstName, documentNumber, email);
            DisplayTicketInfo();
            GenerateBarcode();  // ← Теперь штрих-код

            StartEmailTimer();
        }

        private void SaveTicketToDatabase(DateTime date, string lastName, string firstName,
                                         string documentNumber, string email)
        {
            try
            {
                using (var db = new TrainCoreDiplomEntities1())
                {
                    var passenger = db.Passangers
                        .FirstOrDefault(p => p.Number_passport == documentNumber);

                    if (passenger == null)
                    {
                        passenger = new Passangers
                        {
                            Fam_Pas = lastName,
                            Name_Pas = firstName,
                            Number_passport = documentNumber,
                            Email = email
                        };
                        db.Passangers.Add(passenger);
                        db.SaveChanges();
                    }

                    _ticket = new Tickets
                    {
                        ID_Passanger = passenger.ID_Passanger,
                        ID_Seat = _seat.ID_Seat,
                        ID_Schedule = _schedule.ID_Schedule,
                        Date_buy = DateTime.Now,
                        Stoimost = _totalPrice,
                        Status = "Оплачен"
                    };
                    db.Tickets.Add(_ticket);
                    db.SaveChanges();

                    var seat = db.Seats.Find(_seat.ID_Seat);
                    if (seat != null)
                    {
                        seat.IsAvailable = false;
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения: {ex.Message}");
            }
        }

        private void DisplayTicketInfo()
        {
            try
            {
                if (_ticket != null)
                {
                    TicketNumberText.Text = $"Билет № {_ticket.ID_Ticket:000000}";
                }

                DepartureStationText.Text = _fromStation?.Name_Station ?? "—";
                ArrivalStationText.Text = _toStation?.Name_Station ?? "—";

                var departureDateTime = _schedule.Date_Start.Add(_schedule.Time_start);
                var arrivalDateTime = _schedule.Date_finish.Add(_schedule.Time_finish);
                var travelTime = arrivalDateTime - departureDateTime;

                DepartureTimeText.Text = departureDateTime.ToString("HH:mm");
                DepartureDateText.Text = departureDateTime.ToString("dd MMMM yyyy");
                ArrivalTimeText.Text = arrivalDateTime.ToString("HH:mm");
                ArrivalDateText.Text = arrivalDateTime.ToString("dd MMMM yyyy");

                if (travelTime.TotalHours < 24)
                    TravelTimeText.Text = $"{(int)travelTime.TotalHours} ч {travelTime.Minutes} мин";
                else
                    TravelTimeText.Text = $"{(int)travelTime.TotalDays} д {travelTime.Hours} ч";

                if (_schedule.Trains != null)
                {
                    TrainNumberText.Text = _schedule.Trains.Number_train ?? "—";
                    TrainNameText.Text = _schedule.Trains.Name_train ?? "";
                }

                WagonNumberText.Text = _wagon.Number_wagon;
                if (_wagon.Type_Wagons != null)
                {
                    WagonTypeText.Text = _wagon.Type_Wagons.Name_type_wagon ?? "Плацкарт";
                }

                SeatNumberText.Text = _seat.Number_seats;
                SeatTypeText.Text = GetSeatType(_seat.Number_seats);

                PassengerNameText.Text = _passengerName;
                PassengerDocumentText.Text = _passengerDocument;

                PriceText.Text = $"{_totalPrice:N0} ₽";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка отображения: {ex.Message}");
            }
        }

        private string GetSeatType(string seatNumber)
        {
            if (int.TryParse(seatNumber, out int num))
                return num % 2 == 1 ? "Нижнее" : "Верхнее";
            return "Стандартное";
        }

        // ==================== ШТРИХ-КОД ====================
        private void GenerateBarcode()
        {
            try
            {
                // Формируем данные для штрих-кода (уникальный номер билета)
                string barcodeData = _ticket?.ID_Ticket.ToString("000000") ?? "000000";

                // Добавляем контрольную цифру
                string barcodeWithCheck = barcodeData + CalculateCheckDigit(barcodeData);

                // Создаем штрих-код формата CODE_128
                var writer = new BarcodeWriter
                {
                    Format = BarcodeFormat.CODE_128,
                    Options = new EncodingOptions
                    {
                        Width = 300,
                        Height = 100,
                        Margin = 10,
                        PureBarcode = true
                    }
                };

                using (var bitmap = writer.Write(barcodeWithCheck))
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        bitmap.Save(memoryStream, ImageFormat.Png);
                        memoryStream.Position = 0;

                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = memoryStream;
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.EndInit();

                        BarcodeImage.Source = bitmapImage;
                    }
                }

                // Показываем номер под штрих-кодом
                BarcodeNumberText.Text = barcodeWithCheck;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка создания штрих-кода: {ex.Message}");
                CreatePlaceholderBarcode();
            }
        }

        private string CalculateCheckDigit(string data)
        {
            int sum = 0;
            for (int i = 0; i < data.Length; i++)
            {
                sum += int.Parse(data[i].ToString()) * (i + 1);
            }
            int check = sum % 10;
            return check.ToString();
        }

        private void CreatePlaceholderBarcode()
        {
            var grid = new Grid();
            grid.Background = new SolidColorBrush(Colors.LightGray);
            grid.Width = 300;
            grid.Height = 100;

            var textBlock = new TextBlock();
            textBlock.Text = "ШТРИХ-КОД";
            textBlock.FontSize = 16;
            textBlock.FontWeight = FontWeights.Bold;
            textBlock.Foreground = new SolidColorBrush(Colors.DarkGray);
            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            textBlock.VerticalAlignment = VerticalAlignment.Center;

            grid.Children.Add(textBlock);

            grid.Measure(new System.Windows.Size(300, 100));
            grid.Arrange(new Rect(0, 0, 300, 100));

            var renderBitmap = new RenderTargetBitmap(300, 100, 96, 96, PixelFormats.Pbgra32);
            renderBitmap.Render(grid);

            BarcodeImage.Source = renderBitmap;
            BarcodeNumberText.Text = "000000";
        }

        private void StartEmailTimer()
        {
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(2);
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                SendEmailToPassenger();
            };
            timer.Start();
        }

        // ==================== ОТПРАВКА НА ПОЧТУ ====================
        private async void SendEmailToPassenger()
        {
            if (string.IsNullOrEmpty(_passengerEmail))
            {
                UpdateEmailStatus("❌", "Email не указан", "#F44336");
                return;
            }

            try
            {
                UpdateEmailStatus("⏳", "Отправка...", "#1E88E5");

                string yourEmail = "zaya271106@mail.ru";
                string yourPassword = "XOFEDN6vkuV811pun4m1";

                using (var client = new SmtpClient("smtp.mail.ru", 587))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(yourEmail, yourPassword);

                    var mail = new MailMessage();
                    mail.From = new MailAddress(yourEmail, "TrainCore");
                    mail.To.Add(_passengerEmail);
                    mail.Subject = $"🎫 Ваш билет №{_ticket?.ID_Ticket:000000}";
                    mail.Body = GetEmailBody();
                    mail.IsBodyHtml = true;

                    await client.SendMailAsync(mail);
                }

                UpdateEmailStatus("✅", "Отправлено!", "#43A047");
                EmailButton.IsEnabled = false;
                _emailSent = true;

                MessageBox.Show("Билет успешно отправлен на почту!", "Успех",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                UpdateEmailStatus("❌", "Ошибка", "#F44336");
                MessageBox.Show($"Ошибка отправки: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetEmailBody()
        {
            return $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; }}
                    .ticket {{ border: 2px solid #1E88E5; padding: 20px; border-radius: 10px; max-width: 600px; }}
                    .header {{ color: #1E88E5; }}
                    .price {{ color: #43A047; font-size: 24px; font-weight: bold; }}
                </style>
            </head>
            <body>
                <div class='ticket'>
                    <h2 class='header'>TrainCore - Ваш билет</h2>
                    <p>Уважаемый(ая) {_passengerName}, спасибо за покупку!</p>
                    
                    <h3>🎫 Билет № {_ticket?.ID_Ticket:000000}</h3>
                    
                    <table style='width: 100%;'>
                        <tr><td><strong>Поезд:</strong></td><td>{_schedule.Trains?.Number_train} {_schedule.Trains?.Name_train}</td></tr>
                        <tr><td><strong>Маршрут:</strong></td><td>{_fromStation?.Name_Station} → {_toStation?.Name_Station}</td></tr>
                        <tr><td><strong>Отправление:</strong></td><td>{_schedule.Date_Start:dd.MM.yyyy} в {_schedule.Time_start}</td></tr>
                        <tr><td><strong>Вагон:</strong></td><td>{_wagon.Number_wagon}</td></tr>
                        <tr><td><strong>Место:</strong></td><td>{_seat.Number_seats}</td></tr>
                        <tr><td><strong>Пассажир:</strong></td><td>{_passengerName}</td></tr>
                        <tr><td><strong>Документ:</strong></td><td>{_passengerDocument}</td></tr>
                    </table>
                    
                    <p class='price'>Стоимость: {_totalPrice:N0} ₽</p>
                    <p>Счастливого пути! 🚆</p>
                </div>
            </body>
            </html>";
        }

        private void UpdateEmailStatus(string icon, string text, string color)
        {
            Dispatcher.Invoke(() =>
            {
                if (EmailStatusIcon != null)
                    EmailStatusIcon.Text = icon;

                if (EmailStatusText != null)
                {
                    EmailStatusText.Text = text;
                    var converter = new BrushConverter();
                    EmailStatusText.Foreground = (System.Windows.Media.Brush)converter.ConvertFromString(color);
                }
            });
        }

        // ==================== СОХРАНЕНИЕ HTML ====================
        private void SaveHtmlTicket()
        {
            try
            {
                Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
                saveFileDialog.Filter = "HTML files (*.html)|*.html";
                saveFileDialog.FileName = $"Ticket_{_ticket?.ID_Ticket:000000}.html";
                saveFileDialog.Title = "Сохранить билет";

                if (saveFileDialog.ShowDialog() == true)
                {
                    string fileName = saveFileDialog.FileName;

                    // Генерируем штрих-код для HTML
                    string barcodeData = _ticket?.ID_Ticket.ToString("000000") ?? "000000";
                    string barcodeWithCheck = barcodeData + CalculateCheckDigit(barcodeData);

                    // Конвертируем штрих-код в Base64 для вставки в HTML
                    string barcodeBase64 = GenerateBarcodeBase64(barcodeWithCheck);

                    string html = $@"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Билет №{_ticket?.ID_Ticket:000000}</title>
    <style>
        body {{
            font-family: 'Segoe UI', Arial, sans-serif;
            background-color: #f5f5f5;
            display: flex;
            justify-content: center;
            align-items: center;
            min-height: 100vh;
            margin: 0;
            padding: 20px;
        }}
        .ticket {{
            width: 600px;
            background: white;
            border-radius: 20px;
            padding: 30px;
            box-shadow: 0 10px 30px rgba(0,0,0,0.2);
            border: 2px solid #1E88E5;
        }}
        .header {{
            text-align: center;
            border-bottom: 2px dashed #1E88E5;
            padding-bottom: 20px;
            margin-bottom: 20px;
        }}
        .header h1 {{
            color: #1E88E5;
            font-size: 36px;
            margin: 0;
        }}
        .header h2 {{
            color: #666;
            font-size: 18px;
            margin: 5px 0 0;
        }}
        .ticket-number {{
            background: #1E88E5;
            color: white;
            padding: 10px;
            border-radius: 10px;
            text-align: center;
            font-size: 24px;
            font-weight: bold;
            margin-bottom: 20px;
        }}
        .info-grid {{
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 15px;
            margin-bottom: 20px;
        }}
        .info-item {{
            border-bottom: 1px solid #eee;
            padding: 10px 0;
        }}
        .info-label {{
            color: #757575;
            font-size: 12px;
            margin-bottom: 5px;
        }}
        .info-value {{
            color: #333;
            font-size: 16px;
            font-weight: bold;
        }}
        .price {{
            background: #E8F5E9;
            padding: 15px;
            border-radius: 10px;
            text-align: right;
            margin: 20px 0;
        }}
        .price .amount {{
            color: #43A047;
            font-size: 32px;
            font-weight: bold;
        }}
        .barcode-container {{
            text-align: center;
            padding: 20px;
            background: #f9f9f9;
            border-radius: 10px;
            margin: 20px 0;
        }}
        .barcode-image {{
            max-width: 100%;
            height: auto;
        }}
        .barcode-number {{
            font-family: 'Courier New', monospace;
            font-size: 18px;
            letter-spacing: 2px;
            margin-top: 10px;
            color: #1E88E5;
            font-weight: bold;
        }}
        .footer {{
            text-align: center;
            color: #757575;
            font-size: 12px;
            margin-top: 20px;
        }}
    </style>
</head>
<body>
    <div class='ticket'>
        <div class='header'>
            <h1>TRAINCORE</h1>
            <h2>Электронный билет</h2>
        </div>

        <div class='ticket-number'>
            БИЛЕТ № {_ticket?.ID_Ticket:000000}
        </div>

        <div class='info-grid'>
            <div class='info-item'>
                <div class='info-label'>Поезд</div>
                <div class='info-value'>{_schedule.Trains?.Number_train} {_schedule.Trains?.Name_train}</div>
            </div>
            <div class='info-item'>
                <div class='info-label'>Маршрут</div>
                <div class='info-value'>{_fromStation?.Name_Station} → {_toStation?.Name_Station}</div>
            </div>
            <div class='info-item'>
                <div class='info-label'>Отправление</div>
                <div class='info-value'>{_schedule.Date_Start:dd.MM.yyyy} в {_schedule.Time_start}</div>
            </div>
            <div class='info-item'>
                <div class='info-label'>Вагон / Место</div>
                <div class='info-value'>Вагон {_wagon.Number_wagon} ({_wagon.Type_Wagons?.Name_type_wagon}) / {_seat.Number_seats}</div>
            </div>
            <div class='info-item'>
                <div class='info-label'>Пассажир</div>
                <div class='info-value'>{_passengerName}</div>
            </div>
            <div class='info-item'>
                <div class='info-label'>Документ</div>
                <div class='info-value'>{_passengerDocument}</div>
            </div>
        </div>

        <div class='price'>
            <span style='color: #666;'>ИТОГО:</span>
            <span class='amount'>{_totalPrice:N0} ₽</span>
        </div>

        <div class='barcode-container'>
            <img class='barcode-image' src='data:image/png;base64,{barcodeBase64}' alt='Штрих-код'/>
            <div class='barcode-number'>{barcodeWithCheck}</div>
            <div style='margin-top: 5px; color: #757575; font-size: 12px;'>Штрих-код для турникета</div>
        </div>

        <div class='footer'>
            Счастливого пути! 🚆
        </div>
    </div>
</body>
</html>";

                    File.WriteAllText(fileName, html, System.Text.Encoding.UTF8);
                    MessageBox.Show($"✅ HTML билет сохранен:\n{fileName}", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                    System.Diagnostics.Process.Start(fileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Ошибка сохранения HTML: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Добавь этот метод для генерации штрих-кода в Base64
        private string GenerateBarcodeBase64(string data)
        {
            try
            {
                var writer = new BarcodeWriter
                {
                    Format = BarcodeFormat.CODE_128,
                    Options = new EncodingOptions
                    {
                        Width = 400,
                        Height = 100,
                        Margin = 10,
                        PureBarcode = true
                    }
                };

                using (var bitmap = writer.Write(data))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        bitmap.Save(ms, ImageFormat.Png);
                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
            catch
            {
                return "";
            }
        }
        private void EmailButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_emailSent)
            {
                SendEmailToPassenger();
            }
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveHtmlTicket();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new TrainSearchPage());
        }
    }
}