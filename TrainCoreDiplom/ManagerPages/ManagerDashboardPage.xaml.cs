using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TrainCoreDiplom.DBConnection;

namespace TrainCoreDiplom.ManagerPages
{
    public partial class ManagerDashboardPage : Page
    {
        public ManagerDashboardPage()
        {
            InitializeComponent();
            Loaded += ManagerDashboardPage_Loaded;

            ManagerNameText.Text = App.CurrentUser?.Login ?? "Manager";
            WelcomeText.Text = $"Добро пожаловать, {App.CurrentUser?.Login ?? "Manager"}!";
        }

        private void ManagerDashboardPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadStatistics();
            LoadTodaySchedule();
            LoadRecentSales();
        }

        private void LoadStatistics()
        {
            try
            {
                using (var db = new TrainCoreDiplomEntities1())
                {
                    var today = DateTime.Today;

                    // Активные поезда сегодня
                    ActiveTrainsText.Text = db.Schedule
                        .Count(s => s.Date_Start == today).ToString();

                    // Продажи сегодня
                    TodaySalesText.Text = db.Tickets
                        .Count(t => t.Date_buy.HasValue && t.Date_buy.Value.Date == today).ToString();

                    // Выручка сегодня
                    var revenue = db.Tickets
                        .Where(t => t.Date_buy.HasValue && t.Date_buy.Value.Date == today)
                        .Sum(t => (decimal?)t.Stoimost) ?? 0;
                    TodayRevenueText.Text = $"{revenue:N0} ₽";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка статистики: {ex.Message}");
            }
        }

        private void LoadTodaySchedule()
        {
            try
            {
                using (var db = new TrainCoreDiplomEntities1())
                {
                    var today = DateTime.Today;
                    var schedule = db.Schedule
                        .Where(s => s.Date_Start == today)
                        .Take(10)
                        .ToList()
                        .Select(s => new
                        {
                            Time = s.Time_start.ToString(@"hh\:mm"),
                            Train = $"{s.Trains?.Number_train} {s.Trains?.Name_train}",
                            Route = $"{s.Marshrut?.Stations?.Name_Station} → {s.Marshrut?.Stations1?.Name_Station}",
                            FreeSeats = GetFreeSeatsCount(db, s.ID_Train)
                        })
                        .ToList();

                    TodayScheduleGrid.ItemsSource = schedule;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки расписания: {ex.Message}");
            }
        }

        private int GetFreeSeatsCount(TrainCoreDiplomEntities1 db, int trainId)
        {
            return db.Seats.Count(s => s.Wagons.ID_Train == trainId && s.IsAvailable == true);
        }

        private void LoadRecentSales()
        {
            try
            {
                using (var db = new TrainCoreDiplomEntities1())
                {
                    var sales = db.Tickets
                        .Where(t => t.Date_buy.HasValue)
                        .OrderByDescending(t => t.Date_buy)
                        .Take(20)
                        .ToList()
                        .Select(t => new
                        {
                            Time = t.Date_buy?.ToString("HH:mm") ?? "",
                            Train = $"{t.Schedule?.Trains?.Number_train} {t.Schedule?.Trains?.Name_train}",
                            Route = $"{t.Schedule?.Marshrut?.Stations?.Name_Station} → {t.Schedule?.Marshrut?.Stations1?.Name_Station}",
                            Amount = t.Stoimost.ToString("N0") + " ₽"
                        })
                        .ToList();

                    RecentSalesGrid.ItemsSource = sales;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки продаж: {ex.Message}");
            }
        }

        // Навигация
        private void Dashboard_Click(object sender, RoutedEventArgs e) { }

        private void Trains_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ManagerTrainsPage());
        }

        private void Schedule_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ManagerSchedulePage());
        }

        private void Reports_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ManagerReportsPage());
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ManagerProfilePage());
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentUser = null;
            NavigationService.Navigate(new Pages.LoginPage());
        }
    }
}