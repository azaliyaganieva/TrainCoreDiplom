using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TrainCoreDiplom.DBConnection;

namespace TrainCoreDiplom.Pages
{
    public partial class TrainSearchPage : Page
    {
        private List<Stations> _stations;

        public TrainSearchPage()
        {
            InitializeComponent();
            LoadStations();

            if (App.CurrentUser != null)
            {
                UserNameText.Text = App.CurrentUser.Login;
            }
        }

        private void LoadStations()
        {
            try
            {
                using (var db = new TrainCoreDiplomEntities1())
                {
                    _stations = db.Stations.OrderBy(s => s.Name_Station).ToList();
                    FromStationComboBox.ItemsSource = _stations;
                    ToStationComboBox.ItemsSource = _stations;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки станций: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверка выбора станций
            if (FromStationComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите станцию отправления", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ToStationComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите станцию назначения", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (FromStationComboBox.SelectedItem == ToStationComboBox.SelectedItem)
            {
                MessageBox.Show("Станции отправления и назначения должны отличаться", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка даты
            if (DepartureDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату отправления", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime selectedDate = DepartureDatePicker.SelectedDate.Value;

            // Если дата в прошлом - предупреждение
            if (selectedDate.Date < DateTime.Today)
            {
                MessageBox.Show("Нельзя выбрать прошедшую дату. Дата будет изменена на сегодня.",
                              "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                DepartureDatePicker.SelectedDate = DateTime.Today;
                selectedDate = DateTime.Today;
            }

            var fromStation = (Stations)FromStationComboBox.SelectedItem;
            var toStation = (Stations)ToStationComboBox.SelectedItem;

            SearchTrains(fromStation.ID_Station, toStation.ID_Station, selectedDate);
        }

        private void SearchTrains(int fromStationId, int toStationId, DateTime date)
        {
            try
            {
                using (var db = new TrainCoreDiplomEntities1())
                {
                    var schedules = db.Schedule
                        .Include("Trains")
                        .Include("Marshrut")
                        .Include("Marshrut.Stations")
                        .Include("Marshrut.Stations1")
                        .Where(s => s.Marshrut.ID_start_station == fromStationId
                                 && s.Marshrut.ID_finish_station == toStationId
                                 && s.Date_Start == date.Date)
                        .OrderBy(s => s.Time_start)
                        .ToList();

                    var results = new List<object>();

                    foreach (var s in schedules)
                    {
                        // ПРОВЕРКА на null
                        if (s.Trains == null || s.Marshrut == null ||
                            s.Marshrut.Stations == null || s.Marshrut.Stations1 == null)
                        {
                            continue; // пропускаем если данные неполные
                        }

                        int freeSeats = GetFreeSeatsCount(db, s.ID_Train);
                        decimal minPrice = GetMinPrice(db, s.ID_Train);

                        results.Add(new
                        {
                            ScheduleId = s.ID_Schedule,
                            TrainName = $"{s.Trains.Number_train} {s.Trains.Name_train}".Trim(),
                            DepartureStation = s.Marshrut.Stations.Name_Station ?? "",
                            DepartureTime = s.Time_start.ToString(@"hh\:mm"),
                            ArrivalStation = s.Marshrut.Stations1.Name_Station ?? "",
                            ArrivalTime = s.Time_finish.ToString(@"hh\:mm"),
                            Duration = CalculateDuration(s.Time_start, s.Time_finish, s.Date_Start, s.Date_finish),
                            FreeSeats = freeSeats.ToString() + " мест",
                            Price = minPrice.ToString("N0") + " ₽"
                        });
                    }

                    TrainsItemsControl.ItemsSource = results;

                    ResultsTitle.Visibility = results.Count > 0
                        ? Visibility.Visible
                        : Visibility.Collapsed;

                    if (results.Count == 0)
                    {
                        MessageBox.Show("На указанную дату поездов не найдено", "Информация",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int GetFreeSeatsCount(TrainCoreDiplomEntities1 db, int trainId)
        {
            var seats = db.Seats.Where(s => s.Wagons.ID_Train == trainId && s.IsAvailable == true);
            return seats.Count();
        }

        private decimal GetMinPrice(TrainCoreDiplomEntities1 db, int trainId)
        {
            var seats = db.Seats.Where(s => s.Wagons.ID_Train == trainId && s.IsAvailable == true);
            if (seats.Any())
            {
                return seats.Min(s => s.Price);
            }
            return 0;
        }

        private string CalculateDuration(TimeSpan startTime, TimeSpan endTime, DateTime startDate, DateTime endDate)
        {
            DateTime start = startDate.Add(startTime);
            DateTime end = endDate.Add(endTime);
            TimeSpan duration = end - start;

            if (duration.TotalHours < 24)
            {
                return $"{(int)duration.TotalHours} ч {duration.Minutes} мин";
            }
            else
            {
                return $"{(int)duration.TotalDays} д {duration.Hours} ч {duration.Minutes} мин";
            }
        }

        private void SwapStations_Click(object sender, RoutedEventArgs e)
        {
            object temp = FromStationComboBox.SelectedItem;
            FromStationComboBox.SelectedItem = ToStationComboBox.SelectedItem;
            ToStationComboBox.SelectedItem = temp;
        }
        private void SelectTrain_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button button = sender as Button;
                if (button == null)
                {
                    MessageBox.Show("Ошибка кнопки", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (button.Tag == null)
                {
                    MessageBox.Show("Не найден ID поезда", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                int scheduleId = Convert.ToInt32(button.Tag);

                using (var db = new TrainCoreDiplomEntities1())
                {
                    var schedule = db.Schedule
                        .Include("Trains")
                        .Include("Marshrut")
                        .Include("Marshrut.Stations")
                        .Include("Marshrut.Stations1")
                        .FirstOrDefault(s => s.ID_Schedule == scheduleId);

                    if (schedule == null)
                    {
                        MessageBox.Show("Расписание не найдено", "Ошибка",
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    DateTime selectedDate = DepartureDatePicker.SelectedDate ?? DateTime.Today;

                    // ✅ СОХРАНЯЕМ ВЫБРАННЫЕ СТАНЦИИ
                    var fromStation = (Stations)FromStationComboBox.SelectedItem;
                    var toStation = (Stations)ToStationComboBox.SelectedItem;

                    // Передаем расписание И выбранные станции
                    NavigationService.Navigate(new SeatSelectionPage(schedule, selectedDate, fromStation, toStation));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}\n\n{ex.StackTrace}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TodayButton_Click(object sender, RoutedEventArgs e)
        {
            DepartureDatePicker.SelectedDate = DateTime.Today;
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentUser = null;
            NavigationService.Navigate(new LoginPage());
        }
    }
}