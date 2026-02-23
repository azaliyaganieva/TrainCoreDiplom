using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TrainCoreDiplom.DBConnection;

namespace TrainCoreDiplom.Pages
{
    public partial class TicketHistoryPage : Page
    {
        public class HistoryTicket
        {
            public string BuyDate { get; set; }
            public string TicketNumber { get; set; }
            public string TrainName { get; set; }
            public string Route { get; set; }
            public string DepartureDate { get; set; }
            public string Seat { get; set; }
            public string Price { get; set; }
            public string Status { get; set; }
        }

        public TicketHistoryPage()
        {
            InitializeComponent();

            // Устанавливаем период по умолчанию (последний месяц)
            StartDatePicker.SelectedDate = DateTime.Today.AddMonths(-1);
            EndDatePicker.SelectedDate = DateTime.Today;

            LoadTicketHistory();
        }

        private void LoadTicketHistory()
        {
            try
            {
                using (var db = new TrainCoreDiplomEntities1())
                {
                    DateTime startDate = StartDatePicker.SelectedDate ?? DateTime.Today.AddMonths(-1);
                    DateTime endDate = EndDatePicker.SelectedDate ?? DateTime.Today;

                    // Получаем все билеты за период
                    List<Tickets> tickets = new List<Tickets>();

                    foreach (Tickets t in db.Tickets)
                    {
                        if (t.Date_buy.HasValue &&
                            t.Date_buy.Value.Date >= startDate.Date &&
                            t.Date_buy.Value.Date <= endDate.Date)
                        {
                            tickets.Add(t);
                        }
                    }

                    List<HistoryTicket> history = new List<HistoryTicket>();

                    foreach (Tickets t in tickets)
                    {
                        string trainNumber = t.Schedule?.Trains?.Number_train ?? "";
                        string trainName = t.Schedule?.Trains?.Name_train ?? "";
                        string fromStation = t.Schedule?.Marshrut?.Stations?.Name_Station ?? "";
                        string toStation = t.Schedule?.Marshrut?.Stations1?.Name_Station ?? "";
                        string departureDate = t.Schedule != null
                            ? t.Schedule.Date_Start.ToString("dd.MM.yyyy")
                            : "";

                        history.Add(new HistoryTicket
                        {
                            BuyDate = t.Date_buy.HasValue ? t.Date_buy.Value.ToString("dd.MM.yyyy") : "",
                            TicketNumber = t.ID_Ticket.ToString("000000"),
                            TrainName = $"{trainNumber} {trainName}".Trim(),
                            Route = $"{fromStation} → {toStation}",
                            DepartureDate = departureDate,
                            Seat = $"Вагон {t.Seats?.Wagons?.Number_wagon} / {t.Seats?.Number_seats}",
                            Price = t.Stoimost.ToString("N0") + " ₽",
                            Status = t.Status ?? "Оплачен"
                        });
                    }

                    // Сортируем по дате покупки (сначала новые)
                    history.Sort((a, b) => string.Compare(b.BuyDate, a.BuyDate));

                    TicketsHistoryGrid.ItemsSource = history;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки истории: {ex.Message}");
                MessageBox.Show($"Ошибка загрузки истории: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            if (!StartDatePicker.SelectedDate.HasValue || !EndDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Выберите период", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (StartDatePicker.SelectedDate > EndDatePicker.SelectedDate)
            {
                MessageBox.Show("Дата начала не может быть позже даты окончания", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            LoadTicketHistory();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}