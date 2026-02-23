using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TrainCoreDiplom.DBConnection;

namespace TrainCoreDiplom.Pages
{
    public partial class UserProfilePage : Page
    {
        private Users _currentUser;

        public class TicketDisplay
        {
            public int TicketId { get; set; }
            public string BuyDate { get; set; }
            public string TrainName { get; set; }
            public string Route { get; set; }
            public string Seat { get; set; }
            public string Status { get; set; }
        }

        public UserProfilePage()
        {
            InitializeComponent();
            _currentUser = App.CurrentUser;

            if (_currentUser != null)
            {
                UserNameText.Text = _currentUser.Login;
                UserEmailText.Text = _currentUser.Email;
                LoadUserTickets();
                LoadUserData();
            }
        }

        private void LoadUserTickets()
        {
            try
            {
                using (var db = new TrainCoreDiplomEntities1())
                {
                    // Находим пассажира по email пользователя
                    Passangers passenger = null;
                    foreach (Passangers p in db.Passangers)
                    {
                        if (p.Email == _currentUser.Email)
                        {
                            passenger = p;
                            break;
                        }
                    }

                    if (passenger != null)
                    {
                        List<Tickets> tickets = new List<Tickets>();
                        foreach (Tickets t in db.Tickets)
                        {
                            if (t.ID_Passanger == passenger.ID_Passanger)
                            {
                                tickets.Add(t);
                            }
                        }

                        List<TicketDisplay> displayList = new List<TicketDisplay>();
                        foreach (Tickets t in tickets)
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

                            displayList.Add(new TicketDisplay
                            {
                                TicketId = t.ID_Ticket,
                                BuyDate = t.Date_buy.HasValue ? t.Date_buy.Value.ToString("dd.MM.yyyy") : "",
                                TrainName = trainName,
                                Route = route,
                                Seat = $"Вагон {t.Seats?.Wagons?.Number_wagon} / {t.Seats?.Number_seats}",
                                Status = t.Status ?? "Оплачен"
                            });
                        }

                        TicketsGrid.ItemsSource = displayList;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки билетов: {ex.Message}");
            }
        }

        private void LoadUserData()
        {
            try
            {
                using (var db = new TrainCoreDiplomEntities1())
                {
                    // Находим пассажира по email
                    Passangers passenger = null;
                    foreach (Passangers p in db.Passangers)
                    {
                        if (p.Email == _currentUser.Email)
                        {
                            passenger = p;
                            break;
                        }
                    }

                    if (passenger != null)
                    {
                        FirstNameTextBox.Text = passenger.Name_Pas;
                        LastNameTextBox.Text = passenger.Fam_Pas;
                        EmailTextBox.Text = passenger.Email;
                        PhoneTextBox.Text = passenger.Phone;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new TrainCoreDiplomEntities1())
                {
                    // Находим пассажира по email
                    Passangers passenger = null;
                    foreach (Passangers p in db.Passangers)
                    {
                        if (p.Email == _currentUser.Email)
                        {
                            passenger = p;
                            break;
                        }
                    }

                    if (passenger != null)
                    {
                        passenger.Name_Pas = FirstNameTextBox.Text;
                        passenger.Fam_Pas = LastNameTextBox.Text;
                        passenger.Email = EmailTextBox.Text;
                        passenger.Phone = PhoneTextBox.Text;

                        db.SaveChanges();

                        MessageBox.Show("Данные сохранены", "Успех",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewTicket_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button?.Tag != null)
            {
                int ticketId = Convert.ToInt32(button.Tag);
                MessageBox.Show($"Просмотр билета №{ticketId}", "Информация",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentUser = null;
            NavigationService.Navigate(new LoginPage());
        }
    }
}