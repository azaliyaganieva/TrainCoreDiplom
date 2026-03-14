using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TrainCoreDiplom.DBConnection;

namespace TrainCoreDiplom.AdminPages
{
    public partial class AdminDashboardPage : Page
    {
        public class MonthlyStat
        {
            public string Month { get; set; }
            public int TicketsCount { get; set; }
            public string Revenue { get; set; }
        }

        public class PopularRoute
        {
            public string RouteName { get; set; }
            public int Count { get; set; }
            public string Revenue { get; set; }
        }

        public class OperationItem
        {
            public string Date { get; set; }
            public string Time { get; set; }
            public string User { get; set; }
            public string Action { get; set; }
            public string Amount { get; set; }
        }

        public AdminDashboardPage()
        {
            InitializeComponent();
            Loaded += AdminDashboardPage_Loaded;

            AdminNameText.Text = App.CurrentUser?.Login ?? "Admin";
            WelcomeText.Text = $"Добро пожаловать, {App.CurrentUser?.Login ?? "Admin"}!";
        }

        private void AdminDashboardPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadStatistics();
            LoadMonthlyStats();
            LoadPopularRoutes();
            LoadRecentOperations();
        }

        private void LoadStatistics()
        {
            try
            {
                using (var db = new TrainCoreDiplomEntities1())
                {
                    // Всего продаж
                    TotalSalesText.Text = db.Tickets.Count().ToString();

                    // Общая выручка
                    var totalRevenue = db.Tickets.Sum(t => (decimal?)t.Stoimost) ?? 0;
                    TotalRevenueText.Text = $"{totalRevenue:N0} ₽";

                    // Всего поездов
                    TotalTrainsText.Text = db.Trains.Count().ToString();

                    // Всего пользователей
                    TotalUsersText.Text = db.Users.Count().ToString();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка статистики: {ex.Message}");
            }
        }

        private void LoadMonthlyStats()
        {
            try
            {
                using (var db = new TrainCoreDiplomEntities1())
                {
                    var stats = db.Tickets
                        .Where(t => t.Date_buy.HasValue)
                        .ToList()
                        .GroupBy(t => new { t.Date_buy.Value.Year, t.Date_buy.Value.Month })
                        .Select(g => new MonthlyStat
                        {
                            Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM yyyy"),
                            TicketsCount = g.Count(),
                            Revenue = g.Sum(t => t.Stoimost).ToString("N0") + " ₽"
                        })
                        .OrderByDescending(x => x.Month)
                        .Take(6)
                        .ToList();

                    MonthlyStatsGrid.ItemsSource = stats;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки статистики по месяцам: {ex.Message}");
            }
        }

        private void LoadPopularRoutes()
        {
            try
            {
                using (var db = new TrainCoreDiplomEntities1())
                {
                    var routes = db.Tickets
                        .Where(t => t.Schedule != null && t.Schedule.Marshrut != null)
                        .ToList()
                        .GroupBy(t => new
                        {
                            From = t.Schedule.Marshrut.Stations?.Name_Station ?? "Неизвестно",
                            To = t.Schedule.Marshrut.Stations1?.Name_Station ?? "Неизвестно"
                        })
                        .Select(g => new PopularRoute
                        {
                            RouteName = $"{g.Key.From} → {g.Key.To}",
                            Count = g.Count(),
                            Revenue = g.Sum(t => t.Stoimost).ToString("N0") + " ₽"
                        })
                        .OrderByDescending(x => x.Count)
                        .Take(5)
                        .ToList();

                    PopularRoutesGrid.ItemsSource = routes;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки популярных маршрутов: {ex.Message}");
            }
        }

        private void LoadRecentOperations()
        {
            try
            {
                using (var db = new TrainCoreDiplomEntities1())
                {
                    var operations = db.Tickets
                        .Where(t => t.Date_buy.HasValue)
                        .OrderByDescending(t => t.Date_buy)
                        .Take(20)
                        .ToList()
                        .Select(t => new OperationItem
                        {
                            Date = t.Date_buy?.ToString("dd.MM.yyyy") ?? "",
                            Time = t.Date_buy?.ToString("HH:mm") ?? "",
                            User = t.Passangers?.Fam_Pas ?? "",
                            Action = $"Билет {t.ID_Ticket} - {t.Schedule?.Trains?.Number_train}",
                            Amount = t.Stoimost.ToString("N0") + " ₽"
                        })
                        .ToList();

                    RecentOperationsGrid.ItemsSource = operations;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки операций: {ex.Message}");
            }
        }

        // Методы навигации
        private void Dashboard_Click(object sender, RoutedEventArgs e) { }
        private void Trains_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new TrainsManagementPage());
        private void Schedule_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new ScheduleManagementPage());
        private void Prices_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new PriceManagementPage());
        private void Users_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new UsersManagementPage());
        private void Reports_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new ReportsPage());
        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentUser = null;
            NavigationService.Navigate(new Pages.LoginPage());
        }
    }
}