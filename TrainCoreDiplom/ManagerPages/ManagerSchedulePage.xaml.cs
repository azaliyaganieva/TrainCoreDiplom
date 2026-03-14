using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TrainCoreDiplom.DBConnection;
using TrainCoreDiplom.AdminWindows;

namespace TrainCoreDiplom.ManagerPages
{
    public partial class ManagerSchedulePage : Page
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

        public class TrainFilterItem
        {
            public int Id { get; set; }
            public string Display { get; set; }
        }

        public class RouteFilterItem
        {
            public int Id { get; set; }
            public string Display { get; set; }
        }

        public ManagerSchedulePage()
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
                    // Загрузка поездов
                    var trains = db.Trains.ToList();
                    var trainList = new List<TrainFilterItem>();
                    trainList.Add(new TrainFilterItem { Id = 0, Display = "Все поезда" });

                    foreach (var t in trains)
                    {
                        trainList.Add(new TrainFilterItem
                        {
                            Id = t.ID_Train,
                            Display = t.Number_train + " " + t.Name_train
                        });
                    }

                    TrainFilterComboBox.ItemsSource = trainList;
                    TrainFilterComboBox.DisplayMemberPath = "Display";
                    TrainFilterComboBox.SelectedValuePath = "Id";
                    TrainFilterComboBox.SelectedIndex = 0;

                    // Загрузка маршрутов
                    var routes = db.Marshrut.ToList();
                    var routeList = new List<RouteFilterItem>();
                    routeList.Add(new RouteFilterItem { Id = 0, Display = "Все маршруты" });

                    foreach (var r in routes)
                    {
                        var fromStation = db.Stations.Find(r.ID_start_station);
                        var toStation = db.Stations.Find(r.ID_finish_station);
                        string from = fromStation != null ? fromStation.Name_Station : "";
                        string to = toStation != null ? toStation.Name_Station : "";

                        routeList.Add(new RouteFilterItem
                        {
                            Id = r.ID_Route,
                            Display = from + " → " + to
                        });
                    }

                    RouteFilterComboBox.ItemsSource = routeList;
                    RouteFilterComboBox.DisplayMemberPath = "Display";
                    RouteFilterComboBox.SelectedValuePath = "Id";
                    RouteFilterComboBox.SelectedIndex = 0;

                    DateFilterPicker.SelectedDate = DateTime.Today;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки фильтров: " + ex.Message, "Ошибка",
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

                    if (TrainFilterComboBox.SelectedItem != null)
                    {
                        var selectedTrain = TrainFilterComboBox.SelectedItem as TrainFilterItem;
                        if (selectedTrain != null && selectedTrain.Id > 0)
                        {
                            query = query.Where(s => s.ID_Train == selectedTrain.Id);
                        }
                    }

                    if (RouteFilterComboBox.SelectedItem != null)
                    {
                        var selectedRoute = RouteFilterComboBox.SelectedItem as RouteFilterItem;
                        if (selectedRoute != null && selectedRoute.Id > 0)
                        {
                            query = query.Where(s => s.ID_Route == selectedRoute.Id);
                        }
                    }

                    if (DateFilterPicker.SelectedDate.HasValue)
                    {
                        DateTime date = DateFilterPicker.SelectedDate.Value.Date;
                        query = query.Where(s => s.Date_Start == date);
                    }

                    var scheduleList = query.ToList();
                    var displayList = new List<ScheduleDisplay>();

                    foreach (var s in scheduleList)
                    {
                        var train = db.Trains.Find(s.ID_Train);
                        var route = db.Marshrut.Find(s.ID_Route);

                        string from = "";
                        string to = "";

                        if (route != null)
                        {
                            var fromStation = db.Stations.Find(route.ID_start_station);
                            var toStation = db.Stations.Find(route.ID_finish_station);
                            if (fromStation != null) from = fromStation.Name_Station;
                            if (toStation != null) to = toStation.Name_Station;
                        }

                        int freeSeats = 0;
                        var wagons = db.Wagons.Where(w => w.ID_Train == s.ID_Train).ToList();
                        foreach (var w in wagons)
                        {
                            freeSeats += db.Seats.Count(seat => seat.ID_Wagon == w.ID_Wagon && seat.IsAvailable == true);
                        }

                        var item = new ScheduleDisplay();
                        item.ID_Schedule = s.ID_Schedule;
                        if (train != null)
                            item.TrainNumber = train.Number_train + " " + train.Name_train;
                        else
                            item.TrainNumber = "Неизвестно";
                        item.RouteName = from + " → " + to;
                        item.DepartureDate = s.Date_Start.ToString("dd.MM.yyyy");
                        item.DepartureTime = s.Time_start.ToString(@"hh\:mm");
                        item.ArrivalDate = s.Date_finish.ToString("dd.MM.yyyy");
                        item.ArrivalTime = s.Time_finish.ToString(@"hh\:mm");
                        item.FreeSeats = freeSeats;

                        displayList.Add(item);
                    }

                    ScheduleGrid.ItemsSource = displayList;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки расписания: " + ex.Message, "Ошибка",
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
            if (button != null && button.Tag != null)
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
            if (button != null && button.Tag != null)
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
                        MessageBox.Show("Ошибка: " + ex.Message, "Ошибка",
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void FilterChanged(object sender, SelectionChangedEventArgs e)
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