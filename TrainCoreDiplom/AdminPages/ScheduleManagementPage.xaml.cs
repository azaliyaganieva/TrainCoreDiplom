using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TrainCoreDiplom.DBConnection;
using TrainCoreDiplom.AdminWindows;

namespace TrainCoreDiplom.AdminPages
{
    public partial class ScheduleManagementPage : Page
    {
        public class ScheduleDisplay
        {
            public int ID_Schedule { get; set; }
            public string TrainNumber { get; set; }
            public string RouteName { get; set; }
            public string DepartureDate { get; set; }
            public string DepartureTime { get; set; }
            public string ArrivalDate { get; set; }
            public string ArrivalTime { get; set; }
            public int FreeSeats { get; set; }
        }

        public ScheduleManagementPage()
        {
            InitializeComponent();
            LoadFilters();
            LoadSchedule();
        }

        private void LoadFilters()
        {
            try
            {
                using (var db = new TrainCoreDiplomEntities1())
                {
                    var trains = db.Trains.ToList();
                    var trainList = new List<dynamic>();
                    trainList.Add(new { ID_Train = 0, Display = "Все поезда" });
                    foreach (var t in trains)
                    {
                        trainList.Add(new { ID_Train = t.ID_Train, Display = $"{t.Number_train} {t.Name_train}" });
                    }
                    TrainFilterComboBox.ItemsSource = trainList;
                    TrainFilterComboBox.SelectedIndex = 0;

                    var routes = db.Marshrut.ToList();
                    var routeList = new List<dynamic>();
                    routeList.Add(new { ID_Route = 0, Display = "Все маршруты" });
                    foreach (var r in routes)
                    {
                        string from = db.Stations.Find(r.ID_start_station)?.Name_Station ?? "";
                        string to = db.Stations.Find(r.ID_finish_station)?.Name_Station ?? "";
                        routeList.Add(new { ID_Route = r.ID_Route, Display = $"{from} → {to}" });
                    }
                    RouteFilterComboBox.ItemsSource = routeList;
                    RouteFilterComboBox.SelectedIndex = 0;

                    DateFilterPicker.SelectedDate = DateTime.Today;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки фильтров: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadSchedule()
        {
            try
            {
                using (var db = new TrainCoreDiplomEntities1())
                {
                    var query = db.Schedule.AsQueryable();

                    if (TrainFilterComboBox.SelectedValue != null && (int)TrainFilterComboBox.SelectedValue > 0)
                    {
                        int trainId = (int)TrainFilterComboBox.SelectedValue;
                        query = query.Where(s => s.ID_Train == trainId);
                    }

                    if (RouteFilterComboBox.SelectedValue != null && (int)RouteFilterComboBox.SelectedValue > 0)
                    {
                        int routeId = (int)RouteFilterComboBox.SelectedValue;
                        query = query.Where(s => s.ID_Route == routeId);
                    }

                    if (DateFilterPicker.SelectedDate.HasValue)
                    {
                        var date = DateFilterPicker.SelectedDate.Value.Date;
                        query = query.Where(s => s.Date_Start == date);
                    }

                    var scheduleList = query.ToList();
                    var displayList = new List<ScheduleDisplay>();

                    foreach (var s in scheduleList)
                    {
                        // Загружаем связанные данные вручную
                        var train = db.Trains.Find(s.ID_Train);
                        var route = db.Marshrut.Find(s.ID_Route);

                        string from = "";
                        string to = "";

                        if (route != null)
                        {
                            var fromStation = db.Stations.Find(route.ID_start_station);
                            var toStation = db.Stations.Find(route.ID_finish_station);
                            from = fromStation?.Name_Station ?? "";
                            to = toStation?.Name_Station ?? "";
                        }

                        int freeSeats = 0;
                        var wagons = db.Wagons.Where(w => w.ID_Train == s.ID_Train).ToList();
                        foreach (var w in wagons)
                        {
                            freeSeats += db.Seats.Count(seat => seat.ID_Wagon == w.ID_Wagon && seat.IsAvailable == true);
                        }

                        displayList.Add(new ScheduleDisplay
                        {
                            ID_Schedule = s.ID_Schedule,
                            TrainNumber = $"{train?.Number_train} {train?.Name_train}",
                            RouteName = $"{from} → {to}",
                            DepartureDate = s.Date_Start.ToString("dd.MM.yyyy"),
                            DepartureTime = s.Time_start.ToString(@"hh\:mm"),
                            ArrivalDate = s.Date_finish.ToString("dd.MM.yyyy"),
                            ArrivalTime = s.Time_finish.ToString(@"hh\:mm"),
                            FreeSeats = freeSeats
                        });
                    }

                    ScheduleGrid.ItemsSource = displayList;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки расписания: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddSchedule_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ScheduleEditWindow(null);
            if (dialog.ShowDialog() == true)
            {
                LoadSchedule();
            }
        }

        private void EditSchedule_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                int scheduleId = Convert.ToInt32(button.Tag);
                using (var db = new TrainCoreDiplomEntities1())
                {
                    var schedule = db.Schedule.Find(scheduleId);
                    if (schedule != null)
                    {
                        var dialog = new ScheduleEditWindow(schedule);
                        if (dialog.ShowDialog() == true)
                        {
                            LoadSchedule();
                        }
                    }
                }
            }
        }

        private void DeleteSchedule_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                int scheduleId = Convert.ToInt32(button.Tag);

                var result = MessageBox.Show("Удалить этот рейс?", "Подтверждение",
                                           MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var db = new TrainCoreDiplomEntities1())
                        {
                            var schedule = db.Schedule.Find(scheduleId);
                            if (schedule != null)
                            {
                                db.Schedule.Remove(schedule);
                                db.SaveChanges();
                                MessageBox.Show("Рейс удален", "Успех",
                                              MessageBoxButton.OK, MessageBoxImage.Information);
                                LoadSchedule();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void FilterChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadSchedule();
        }

        private void DateFilterChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadSchedule();
        }

        private void ResetFilter_Click(object sender, RoutedEventArgs e)
        {
            TrainFilterComboBox.SelectedIndex = 0;
            RouteFilterComboBox.SelectedIndex = 0;
            DateFilterPicker.SelectedDate = DateTime.Today;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}