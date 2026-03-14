using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TrainCoreDiplom.DBConnection;

namespace TrainCoreDiplom.ManagerPages
{
    public partial class ManagerReportsPage : Page
    {
        public ManagerReportsPage()
        {
            InitializeComponent();
            StartDatePicker.SelectedDate = DateTime.Today.AddDays(-30);
            EndDatePicker.SelectedDate = DateTime.Today;
        }

        private void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            if (!StartDatePicker.SelectedDate.HasValue || !EndDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Выберите период", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime start = StartDatePicker.SelectedDate.Value;
            DateTime end = EndDatePicker.SelectedDate.Value;

            if (start > end)
            {
                MessageBox.Show("Дата начала не может быть позже даты окончания", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ReportTypeComboBox.SelectedIndex == 0)
            {
                LoadSalesByDay(start, end);
            }
            else if (ReportTypeComboBox.SelectedIndex == 1)
            {
                LoadSalesByRoute(start, end);
            }
            else if (ReportTypeComboBox.SelectedIndex == 2)
            {
                LoadSalesByWagonType(start, end);
            }
        }

        private void LoadSalesByDay(DateTime start, DateTime end)
        {
            using (var db = new TrainCoreDiplomEntities1())
            {
                var tickets = db.Tickets
                    .Where(t => t.Date_buy >= start && t.Date_buy <= end)
                    .ToList();

                var grouped = tickets
                    .GroupBy(t => t.Date_buy.Value.Date)
                    .Select(g => new
                    {
                        Дата = g.Key.ToString("dd.MM.yyyy"),
                        Продажи = g.Count(),
                        Выручка = g.Sum(t => t.Stoimost).ToString("N0") + " ₽"
                    })
                    .OrderBy(x => x.Дата)
                    .ToList();

                ReportDataGrid.ItemsSource = grouped;
            }
        }

        private void LoadSalesByRoute(DateTime start, DateTime end)
        {
            using (var db = new TrainCoreDiplomEntities1())
            {
                var tickets = db.Tickets
                    .Where(t => t.Date_buy >= start && t.Date_buy <= end)
                    .ToList();

                var validTickets = new System.Collections.Generic.List<Tickets>();
                foreach (var t in tickets)
                {
                    if (t.Schedule != null && t.Schedule.Marshrut != null &&
                        t.Schedule.Marshrut.Stations != null && t.Schedule.Marshrut.Stations1 != null)
                    {
                        validTickets.Add(t);
                    }
                }

                var grouped = validTickets
                    .GroupBy(t => new
                    {
                        From = t.Schedule.Marshrut.Stations.Name_Station,
                        To = t.Schedule.Marshrut.Stations1.Name_Station
                    })
                    .Select(g => new
                    {
                        Маршрут = g.Key.From + " → " + g.Key.To,
                        Продажи = g.Count(),
                        Выручка = g.Sum(t => t.Stoimost).ToString("N0") + " ₽"
                    })
                    .OrderByDescending(x => x.Продажи)
                    .ToList();

                ReportDataGrid.ItemsSource = grouped;
            }
        }

        private void LoadSalesByWagonType(DateTime start, DateTime end)
        {
            using (var db = new TrainCoreDiplomEntities1())
            {
                var tickets = db.Tickets
                    .Where(t => t.Date_buy >= start && t.Date_buy <= end)
                    .ToList();

                var validTickets = new System.Collections.Generic.List<Tickets>();
                foreach (var t in tickets)
                {
                    if (t.Seats != null && t.Seats.Wagons != null &&
                        t.Seats.Wagons.Type_Wagons != null)
                    {
                        validTickets.Add(t);
                    }
                }

                var grouped = validTickets
                    .GroupBy(t => t.Seats.Wagons.Type_Wagons.Name_type_wagon)
                    .Select(g => new
                    {
                        Тип_вагона = g.Key,
                        Продажи = g.Count(),
                        Выручка = g.Sum(t => t.Stoimost).ToString("N0") + " ₽"
                    })
                    .OrderByDescending(x => x.Продажи)
                    .ToList();

                ReportDataGrid.ItemsSource = grouped;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}