using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TrainCoreDiplom.DBConnection;

namespace TrainCoreDiplom.AdminPages
{
    public partial class AdminDashboardPage : Page
    {
        public AdminDashboardPage()
        {
            InitializeComponent();
            LoadStatistics();
        }

        private void LoadStatistics()
        {
            try
            {
                using (var db = new TrainCoreDiplomEntities1())
                {
                    var today = DateTime.Today;

                    // Продажи сегодня
                    var todaySales = db.Tickets
                        .Count(t => t.Date_buy.HasValue &&
                                   t.Date_buy.Value.Date == today);

                    // Ищем TextBlock по имени и устанавливаем значение
                    var todaySalesText = FindName("TodaySalesText") as TextBlock;
                    if (todaySalesText != null)
                        todaySalesText.Text = todaySales.ToString();

                    // Выручка сегодня
                    var todayRevenue = db.Tickets
                        .Where(t => t.Date_buy.HasValue &&
                                   t.Date_buy.Value.Date == today)
                        .Sum(t => (decimal?)t.Stoimost) ?? 0;

                    var todayRevenueText = FindName("TodayRevenueText") as TextBlock;
                    if (todayRevenueText != null)
                        todayRevenueText.Text = $"{todayRevenue:N0} ₽";

                    // Активные поезда
                    var activeTrains = db.Schedule
                        .Count(s => s.Date_Start == today);

                    var activeTrainsText = FindName("ActiveTrainsText") as TextBlock;
                    if (activeTrainsText != null)
                        activeTrainsText.Text = activeTrains.ToString();

                    // Всего пассажиров
                    var totalPassengers = db.Passangers.Count();

                    var totalPassengersText = FindName("TotalPassengersText") as TextBlock;
                    if (totalPassengersText != null)
                        totalPassengersText.Text = totalPassengers.ToString();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        // Навигация
        private void Dashboard_Click(object sender, RoutedEventArgs e) { }

        private void Trains_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new TrainsManagementPage());
        }

        private void Schedule_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ScheduleManagementPage());
        }

        private void Prices_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new PriceManagementPage());
        }

        private void Users_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new UsersManagementPage());
        }

        private void Reports_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ReportsPage());
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentUser = null;
            NavigationService.Navigate(new Pages.LoginPage());
        }
    }
}