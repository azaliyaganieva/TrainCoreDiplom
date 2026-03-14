using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TrainCoreDiplom.DBConnection;

namespace TrainCoreDiplom.AdminPages
{
    public class WagonTypeReport
    {
        public string Тип_вагона { get; set; }
        public int Продажи { get; set; }
        public string Выручка { get; set; }
        public string Доля { get; set; }
    }

    public class OccupancyReport
    {
        public string Поезд { get; set; }
        public string Маршрут { get; set; }
        public string Дата { get; set; }
        public int Всего_мест { get; set; }
        public int Продано { get; set; }
        public string Заполняемость { get; set; }
    }

    public partial class ReportsPage : Page
    {
        public ReportsPage()
        {
            InitializeComponent();
            StartDatePicker.SelectedDate = DateTime.Today.AddDays(-30);
            EndDatePicker.SelectedDate = DateTime.Today;
        }

        private void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!StartDatePicker.SelectedDate.HasValue || !EndDatePicker.SelectedDate.HasValue)
                {
                    MessageBox.Show("Выберите период для отчета", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var startDate = StartDatePicker.SelectedDate.Value;
                var endDate = EndDatePicker.SelectedDate.Value;

                if (startDate > endDate)
                {
                    MessageBox.Show("Дата начала не может быть позже даты окончания", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                switch (ReportTypeComboBox.SelectedIndex)
                {
                    case 0: LoadSalesByDayReport(startDate, endDate); break;
                    case 1: LoadSalesByRouteReport(startDate, endDate); break;
                    case 2: LoadSalesByWagonTypeReport(startDate, endDate); break;
                    case 3: LoadOccupancyReport(startDate, endDate); break;
                    case 4: LoadRevenueByDirectionReport(startDate, endDate); break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка формирования отчета: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                ReportDataGrid.ItemsSource = null;
            }
        }

        private void LoadSalesByDayReport(DateTime startDate, DateTime endDate)
        {
            try
            {
                using (var db = new TrainCoreDiplomEntities1())
                {
                    var data = db.Tickets
                        .Where(t => t.Date_buy >= startDate && t.Date_buy <= endDate)
                        .ToList()
                        .GroupBy(t => t.Date_buy?.Date ?? DateTime.MinValue)
                        .Select(g => new
                        {
                            Дата = g.Key.ToString("dd.MM.yyyy"),
                            Продажи = g.Count(),
                            Выручка = g.Sum(t => t.Stoimost)
                        })
                        .OrderBy(x => x.Дата)
                        .ToList();

                    var report = data.Select(x => new
                    {
                        x.Дата,
                        x.Продажи,
                        Выручка = x.Выручка.ToString("N0") + " ₽"
                    }).ToList();

                    ReportDataGrid.ItemsSource = report;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки отчета: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadSalesByRouteReport(DateTime startDate, DateTime endDate)
        {
            try
            {
                using (var db = new TrainCoreDiplomEntities1())
                {
                    var tickets = db.Tickets
                        .Where(t => t.Date_buy >= startDate && t.Date_buy <= endDate)
                        .ToList();

                    var data = tickets
                        .Where(t => t.Schedule?.Marshrut?.Stations != null && t.Schedule?.Marshrut?.Stations1 != null)
                        .GroupBy(t => new {
                            From = t.Schedule.Marshrut.Stations.Name_Station,
                            To = t.Schedule.Marshrut.Stations1.Name_Station
                        })
                        .Select(g => new
                        {
                            Маршрут = $"{g.Key.From} → {g.Key.To}",
                            Продажи = g.Count(),
                            Выручка = g.Sum(t => t.Stoimost)
                        })
                        .OrderByDescending(x => x.Продажи)
                        .ToList();

                    var report = data.Select(x => new
                    {
                        x.Маршрут,
                        x.Продажи,
                        Выручка = x.Выручка.ToString("N0") + " ₽"
                    }).ToList();

                    ReportDataGrid.ItemsSource = report;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки отчета: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadSalesByWagonTypeReport(DateTime startDate, DateTime endDate)
        {
            try
            {
                using (var db = new TrainCoreDiplomEntities1())
                {
                    var tickets = db.Tickets
                        .Where(t => t.Date_buy >= startDate && t.Date_buy <= endDate)
                        .ToList();

                    var data = tickets
                        .Where(t => t.Seats?.Wagons?.Type_Wagons != null)
                        .GroupBy(t => t.Seats.Wagons.Type_Wagons.Name_type_wagon)
                        .Select(g => new
                        {
                            Тип = g.Key,
                            Продажи = g.Count(),
                            Выручка = g.Sum(t => t.Stoimost)
                        })
                        .ToList();

                    decimal total = data.Sum(x => x.Выручка);

                    var report = new List<WagonTypeReport>();
                    foreach (var item in data)
                    {
                        report.Add(new WagonTypeReport
                        {
                            Тип_вагона = item.Тип,
                            Продажи = item.Продажи,
                            Выручка = item.Выручка.ToString("N0") + " ₽",
                            Доля = total > 0 ? $"{(item.Выручка / total * 100):F1}%" : "0%"
                        });
                    }

                    ReportDataGrid.ItemsSource = report;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки отчета: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                ReportDataGrid.ItemsSource = new List<WagonTypeReport>();
            }
        }

        private void LoadOccupancyReport(DateTime startDate, DateTime endDate)
        {
            try
            {
                using (var db = new TrainCoreDiplomEntities1())
                {
                    var schedules = db.Schedule
                        .Where(s => s.Date_Start >= startDate && s.Date_Start <= endDate)
                        .ToList();

                    var report = new List<OccupancyReport>();

                    foreach (var s in schedules)
                    {
                        var train = db.Trains.Find(s.ID_Train);
                        var route = db.Marshrut.Find(s.ID_Route);

                        string from = "", to = "";
                        if (route != null)
                        {
                            var fromStation = db.Stations.Find(route.ID_start_station);
                            var toStation = db.Stations.Find(route.ID_finish_station);
                            from = fromStation?.Name_Station ?? "";
                            to = toStation?.Name_Station ?? "";
                        }

                        int totalSeats = db.Seats.Count(seat => seat.Wagons.ID_Train == s.ID_Train);
                        int soldSeats = db.Tickets.Count(t => t.ID_Schedule == s.ID_Schedule);

                        report.Add(new OccupancyReport
                        {
                            Поезд = train != null ? $"{train.Number_train} {train.Name_train}" : "Неизвестно",
                            Маршрут = $"{from} → {to}",
                            Дата = s.Date_Start.ToString("dd.MM.yyyy"),
                            Всего_мест = totalSeats,
                            Продано = soldSeats,
                            Заполняемость = totalSeats > 0 ? $"{(double)soldSeats / totalSeats * 100:F1}%" : "0%"
                        });
                    }

                    ReportDataGrid.ItemsSource = report;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки отчета: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadRevenueByDirectionReport(DateTime startDate, DateTime endDate)
        {
            try
            {
                using (var db = new TrainCoreDiplomEntities1())
                {
                    var tickets = db.Tickets
                        .Where(t => t.Date_buy >= startDate && t.Date_buy <= endDate)
                        .ToList();

                    var data = tickets
                        .Where(t => t.Schedule?.Marshrut?.Stations != null)
                        .GroupBy(t => t.Schedule.Marshrut.Stations.Name_Station)
                        .Select(g => new
                        {
                            Направление = g.Key,
                            Продажи = g.Count(),
                            Выручка = g.Sum(t => t.Stoimost)
                        })
                        .OrderByDescending(x => x.Выручка)
                        .ToList();

                    var report = data.Select(x => new
                    {
                        x.Направление,
                        x.Продажи,
                        Выручка = x.Выручка.ToString("N0") + " ₽"
                    }).ToList();

                    ReportDataGrid.ItemsSource = report;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки отчета: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null && NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
            else
            {
                NavigationService.Navigate(new AdminDashboardPage());
            }
        }
    }
}