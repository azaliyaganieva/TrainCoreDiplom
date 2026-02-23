using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TrainCoreDiplom.DBConnection;

namespace TrainCoreDiplom.Pages
{
    public partial class ReturnTicketPage : Page
    {
        private Tickets _foundTicket;
        private decimal _refundAmount;

        public class TicketInfo
        {
            public int Id { get; set; }
            public string Number { get; set; }
            public string Train { get; set; }
            public string Route { get; set; }
            public string Date { get; set; }
            public decimal Price { get; set; }
            public DateTime DepartureTime { get; set; }
        }

        public ReturnTicketPage()
        {
            InitializeComponent();
        }

        private void FindTicket_Click(object sender, RoutedEventArgs e)
        {
            string ticketNumber = TicketNumberTextBox.Text.Trim();

            if (string.IsNullOrEmpty(ticketNumber))
            {
                MessageBox.Show("Введите номер билета", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Парсим номер билета (может быть "Билет № 123456" или просто "123456")
            int ticketId = 0;
            string numberOnly = new string(ticketNumber.Where(char.IsDigit).ToArray());

            if (!int.TryParse(numberOnly, out ticketId) || ticketId == 0)
            {
                MessageBox.Show("Некорректный номер билета", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new TrainCoreDiplomEntities1())
                {
                    // Ищем билет
                    _foundTicket = null;
                    foreach (Tickets t in db.Tickets)
                    {
                        if (t.ID_Ticket == ticketId)
                        {
                            _foundTicket = t;
                            break;
                        }
                    }

                    if (_foundTicket == null)
                    {
                        MessageBox.Show("Билет не найден", "Информация",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                        TicketInfoBorder.Visibility = Visibility.Collapsed;
                        return;
                    }

                    // Проверяем, можно ли вернуть
                    if (_foundTicket.Status != "Оплачен")
                    {
                        MessageBox.Show("Этот билет уже был возвращен или использован", "Ошибка",
                                      MessageBoxButton.OK, MessageBoxImage.Warning);
                        TicketInfoBorder.Visibility = Visibility.Collapsed;
                        return;
                    }

                    // Загружаем связанные данные
                    System.Data.Entity.Infrastructure.DbEntityEntry<Tickets> entry = db.Entry(_foundTicket);
                    entry.Reference(t => t.Schedule).Load();
                    if (_foundTicket.Schedule != null)
                    {
                        entry.Reference(t => t.Schedule.Marshrut).Load();
                        entry.Reference(t => t.Schedule.Trains).Load();
                    }

                    DisplayTicketInfo();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска билета: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DisplayTicketInfo()
        {
            try
            {
                // Отображаем информацию
                TicketNumberInfoText.Text = $"Билет № {_foundTicket.ID_Ticket:000000}";

                string trainNumber = _foundTicket.Schedule?.Trains?.Number_train ?? "";
                string trainName = _foundTicket.Schedule?.Trains?.Name_train ?? "";
                TrainInfoText.Text = $"{trainNumber} {trainName}".Trim();

                string fromStation = _foundTicket.Schedule?.Marshrut?.Stations?.Name_Station ?? "";
                string toStation = _foundTicket.Schedule?.Marshrut?.Stations1?.Name_Station ?? "";
                RouteInfoText.Text = $"{fromStation} → {toStation}";

                if (_foundTicket.Schedule != null)
                {
                    DateTime departureDate = _foundTicket.Schedule.Date_Start;
                    TimeSpan departureTime = _foundTicket.Schedule.Time_start;
                    DateTime departureDateTime = departureDate.Add(departureTime);
                    DateInfoText.Text = departureDateTime.ToString("dd.MM.yyyy HH:mm");
                }

                PriceInfoText.Text = _foundTicket.Stoimost.ToString("N0") + " ₽";

                // Рассчитываем сумму возврата
                CalculateRefundAmount();

                TicketInfoBorder.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка отображения: {ex.Message}");
            }
        }

        private void CalculateRefundAmount()
        {
            decimal originalPrice = _foundTicket.Stoimost;
            decimal refundAmount = 0;
            DateTime now = DateTime.Now;

            if (_foundTicket.Schedule != null)
            {
                DateTime departureDate = _foundTicket.Schedule.Date_Start;
                TimeSpan departureTime = _foundTicket.Schedule.Time_start;
                DateTime departureDateTime = departureDate.Add(departureTime);

                TimeSpan timeUntilDeparture = departureDateTime - now;
                double hoursUntilDeparture = timeUntilDeparture.TotalHours;

                if (hoursUntilDeparture > 24)
                {
                    // 100% - сбор 500 руб
                    refundAmount = originalPrice - 500;
                }
                else if (hoursUntilDeparture > 8)
                {
                    // 50% - сбор 500 руб
                    refundAmount = (originalPrice / 2) - 500;
                }
                else
                {
                    // Возврат невозможен
                    refundAmount = 0;
                }

                if (refundAmount < 0) refundAmount = 0;
            }

            _refundAmount = refundAmount;
            RefundAmountText.Text = _refundAmount.ToString("N0") + " ₽";
        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            if (_foundTicket == null) return;

            if (_refundAmount <= 0)
            {
                MessageBox.Show("Возврат билета невозможен (менее 8 часов до отправления)",
                              "Возврат невозможен",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                $"Вы уверены, что хотите вернуть билет?\n\nСумма к возврату: {_refundAmount:N0} ₽\n\nБилет будет аннулирован.",
                "Подтверждение возврата",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new TrainCoreDiplomEntities1())
                    {
                        // Находим билет
                        Tickets ticket = db.Tickets.Find(_foundTicket.ID_Ticket);
                        if (ticket != null)
                        {
                            // Меняем статус
                            ticket.Status = "Возврат";

                            // Освобождаем место
                            Seats seat = db.Seats.Find(ticket.ID_Seat);
                            if (seat != null)
                            {
                                seat.IsAvailable = true;
                            }

                            db.SaveChanges();

                            MessageBox.Show($"Билет успешно возвращен\nСумма к возврату: {_refundAmount:N0} ₽\nДеньги будут переведены на карту в течение 3-5 рабочих дней",
                                          "Возврат оформлен",
                                          MessageBoxButton.OK,
                                          MessageBoxImage.Information);

                            // Скрываем информацию о билете
                            TicketInfoBorder.Visibility = Visibility.Collapsed;
                            TicketNumberTextBox.Text = "";
                            _foundTicket = null;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при возврате билета: {ex.Message}", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}