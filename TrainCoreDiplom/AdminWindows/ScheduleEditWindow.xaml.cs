using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using TrainCoreDiplom.DBConnection;

namespace TrainCoreDiplom.AdminWindows
{
    public partial class ScheduleEditWindow : Window, INotifyPropertyChanged
    {
        private Schedule _schedule;
        private bool _isEdit;

        // Свойства для привязки
        private DateTime _startDate;
        private DateTime _endDate;
        private string _startTime;
        private string _endTime;

        public DateTime StartDate
        {
            get => _startDate;
            set { _startDate = value; OnPropertyChanged(); }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set { _endDate = value; OnPropertyChanged(); }
        }

        public string StartTime
        {
            get => _startTime;
            set { _startTime = value; OnPropertyChanged(); }
        }

        public string EndTime
        {
            get => _endTime;
            set { _endTime = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ScheduleEditWindow(Schedule schedule = null)
        {
            InitializeComponent();
            DataContext = this; // Важно! Устанавливаем DataContext

            LoadComboBoxes();

            if (schedule != null)
            {
                _schedule = schedule;
                _isEdit = true;

                // Загружаем данные
                TrainComboBox.SelectedValue = schedule.ID_Train;
                RouteComboBox.SelectedValue = schedule.ID_Route;
                StartDate = schedule.Date_Start;
                EndDate = schedule.Date_finish;
                StartTime = schedule.Time_start.ToString(@"hh\:mm");
                EndTime = schedule.Time_finish.ToString(@"hh\:mm");

                Title = "Редактирование рейса";
            }
            else
            {
                _isEdit = false;
                Title = "Добавление рейса";
                StartDate = DateTime.Today;
                EndDate = DateTime.Today;
                StartTime = "00:00";
                EndTime = "00:00";
            }
        }

        private void LoadComboBoxes()
        {
            try
            {
                using (var db = new TrainCoreDiplomEntities1())
                {
                    // Загрузка поездов
                    var trains = db.Trains.ToList();
                    TrainComboBox.ItemsSource = trains;
                    TrainComboBox.DisplayMemberPath = "Number_train";
                    TrainComboBox.SelectedValuePath = "ID_Train";

                    // Загрузка маршрутов
                    var routes = db.Marshrut.ToList();
                    var routeList = routes.Select(r =>
                    {
                        var fromStation = db.Stations.Find(r.ID_start_station);
                        var toStation = db.Stations.Find(r.ID_finish_station);
                        return new
                        {
                            r.ID_Route,
                            Display = $"{fromStation?.Name_Station} → {toStation?.Name_Station}"
                        };
                    }).ToList();

                    RouteComboBox.ItemsSource = routeList;
                    RouteComboBox.DisplayMemberPath = "Display";
                    RouteComboBox.SelectedValuePath = "ID_Route";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Валидация
                if (TrainComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите поезд", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (RouteComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите маршрут", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Парсим время
                if (!TimeSpan.TryParse(StartTime, out TimeSpan startTime))
                {
                    MessageBox.Show("Некорректное время отправления", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!TimeSpan.TryParse(EndTime, out TimeSpan endTime))
                {
                    MessageBox.Show("Некорректное время прибытия", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Сохранение
                using (var db = new TrainCoreDiplomEntities1())
                {
                    if (_isEdit)
                    {
                        var schedule = db.Schedule.Find(_schedule.ID_Schedule);
                        if (schedule != null)
                        {
                            schedule.ID_Train = (int)TrainComboBox.SelectedValue;
                            schedule.ID_Route = (int)RouteComboBox.SelectedValue;
                            schedule.Date_Start = StartDate;
                            schedule.Date_finish = EndDate;
                            schedule.Time_start = startTime;
                            schedule.Time_finish = endTime;
                        }
                    }
                    else
                    {
                        var newSchedule = new Schedule
                        {
                            ID_Train = (int)TrainComboBox.SelectedValue,
                            ID_Route = (int)RouteComboBox.SelectedValue,
                            Date_Start = StartDate,
                            Date_finish = EndDate,
                            Time_start = startTime,
                            Time_finish = endTime
                        };
                        db.Schedule.Add(newSchedule);
                    }
                    db.SaveChanges();
                }

                MessageBox.Show("Данные сохранены", "Успех",
                              MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}