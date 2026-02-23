using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TrainCoreDiplom.DBConnection;

namespace TrainCoreDiplom.AdminWindows
{
    public partial class UserHistoryPage : Page
    {
        private int _userId;

        public class TicketHistoryItem
        {
            public string TicketId { get; set; }
            public string TrainName { get; set; }
            public string Route { get; set; }
            public string Date { get; set; }
            public string Seat { get; set; }
            public string Price { get; set; }
            public string Status { get; set; }
        }

        public UserHistoryPage(int userId)
        {
            InitializeComponent();
            _userId = userId;
            LoadUserInfo();
            LoadUserTickets();
        }

        private void LoadUserInfo()
        {
            try
            {
                using (var db = new TrainCoreDiplomEntities1())
                {
                    var user = db.Users.Find(_userId);
                    if (user != null)
                    {
                        UserNameText.Text = $"История пользователя: {user.Login} ({user.Email})";
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        private void LoadUserTickets()
        {
            try
            {
                using (var db = new TrainCoreDiplomEntities1())
                {
                    var user = db.Users.Find(_userId);
                    if (user == null) return;

                    var passenger = db.Passangers.FirstOrDefault(p => p.Email == user.Email);
                    if (passenger == null) return;

                    var tickets = db.Tickets
                        .Where(t => t.ID_Passanger == passenger.ID_Passanger)
                        .OrderByDescending(t => t.Date_buy)
                        .ToList();

                    var history = new List<TicketHistoryItem>();

                    foreach (var t in tickets)
                    {
                        string trainName = "";
                        string route = "";

                        if (t.Schedule != null)
                        {
                            if (t.Schedule.Trains != null)
                            {
                                trainName = $"{t.Schedule.Trains.Number_train} {t.Schedule.Trains.Name_train}";
                            }
                            if (t.Schedule.Marshrut != null)
                            {
                                string from = t.Schedule.Marshrut.Stations?.Name_Station ?? "";
                                string to = t.Schedule.Marshrut.Stations1?.Name_Station ?? "";
                                route = $"{from} → {to}";
                            }
                        }

                        history.Add(new TicketHistoryItem
                        {
                            TicketId = t.ID_Ticket.ToString("000000"),
                            TrainName = trainName,
                            Route = route,
                            Date = t.Schedule?.Date_Start.ToString("dd.MM.yyyy") ?? "",
                            Seat = t.Seats != null ? $"Вагон {t.Seats.Wagons?.Number_wagon} / {t.Seats.Number_seats}" : "",
                            Price = t.Stoimost.ToString("N0") + " ₽",
                            Status = t.Status ?? "Оплачен"
                        });
                    }

                    TicketsGrid.ItemsSource = history;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки истории: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}