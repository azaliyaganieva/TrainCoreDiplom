using System;
using System.Linq;
using System.Windows;
using TrainCoreDiplom.DBConnection;

namespace TrainCoreDiplom.AdminWindows
{
    public partial class ScheduleEditWindow : Window
    {
        private Schedule _schedule;
        private bool _isEdit;

        public ScheduleEditWindow(Schedule schedule = null)
        {
            InitializeComponent();

            LoadComboBoxes();

            if (schedule != null)
            {
                _schedule = schedule;
                _isEdit = true;
                LoadScheduleData();
                Title = "Редактирование рейса";
            }
            else
            {
                _isEdit = false;
                Title = "Добавление рейса";
                StartDatePicker.SelectedDate = DateTime.Today;
                EndDatePicker.SelectedDate = DateTime.Today;
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

        private void LoadScheduleData()
        {
            try
            {
                TrainComboBox.SelectedValue = _schedule.ID_Train;
                RouteComboBox.SelectedValue = _schedule.ID_Route;
                StartDatePicker.SelectedDate = _schedule.Date_Start;
                StartTimeTextBox.Text = _schedule.Time_start.ToString(@"hh\:mm");
                EndDatePicker.SelectedDate = _schedule.Date_finish;
                EndTimeTextBox.Text = _schedule.Time_finish.ToString(@"hh\:mm");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных рейса: {ex.Message}", "Ошибка",
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

                if (!StartDatePicker.SelectedDate.HasValue || !EndDatePicker.SelectedDate.HasValue)
                {
                    MessageBox.Show("Выберите даты", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!TimeSpan.TryParse(StartTimeTextBox.Text, out TimeSpan startTime))
                {
                    MessageBox.Show("Некорректное время отправления", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!TimeSpan.TryParse(EndTimeTextBox.Text, out TimeSpan endTime))
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
                            schedule.Date_Start = StartDatePicker.SelectedDate.Value;
                            schedule.Date_finish = EndDatePicker.SelectedDate.Value;
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
                            Date_Start = StartDatePicker.SelectedDate.Value,
                            Date_finish = EndDatePicker.SelectedDate.Value,
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