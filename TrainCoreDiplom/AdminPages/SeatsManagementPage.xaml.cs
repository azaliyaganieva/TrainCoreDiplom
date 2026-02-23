using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TrainCoreDiplom.DBConnection;
using TrainCoreDiplom.AdminWindows;

namespace TrainCoreDiplom.AdminPages
{
    public partial class SeatsManagementPage : Page
    {
        private int _wagonId;
        private Wagons _wagon;

        public class SeatDisplay
        {
            public int ID_Seat { get; set; }
            public string Number_seats { get; set; }
            public string Type_seats { get; set; }
            public bool IsAvailable { get; set; }
            public string Price { get; set; }
            public string Features { get; set; }
            public decimal PriceValue { get; set; }
        }

        public SeatsManagementPage(int wagonId)
        {
            InitializeComponent();
            _wagonId = wagonId;
            LoadWagonInfo();
            LoadSeats(); // ← ЭТОТ МЕТОД НУЖНО ИСПРАВИТЬ
        }

        private void LoadWagonInfo()
        {
            try
            {
                using (var db = new TrainCoreDiplomEntities1())
                {
                    _wagon = db.Wagons.Find(_wagonId);
                    if (_wagon != null)
                    {
                        db.Entry(_wagon).Reference(x => x.Type_Wagons).Load();
                        db.Entry(_wagon).Reference(x => x.Trains).Load();

                        TitleText.Text = $"Управление местами";

                        string trainInfo = _wagon.Trains != null
                            ? $"{_wagon.Trains.Number_train} {_wagon.Trains.Name_train}"
                            : "Неизвестный поезд";

                        WagonInfoText.Text = $"Поезд: {trainInfo}";

                        string typeName = _wagon.Type_Wagons?.Name_type_wagon ?? "Неизвестен";
                        WagonDetailsText.Text = $"Вагон №{_wagon.Number_wagon} | Тип: {typeName} | Вместимость: {_wagon.Type_Wagons?.Count_seats ?? 0} мест";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки информации: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ✅ ИСПРАВЛЕННЫЙ МЕТОД LoadSeats()
        private void LoadSeats()
        {
            try
            {
                using (var db = new TrainCoreDiplomEntities1())
                {
                    // Сначала получаем все места из БД
                    var seats = db.Seats
                        .Where(s => s.ID_Wagon == _wagonId)
                        .ToList(); // Выполняем запрос к БД

                    // Сортируем уже в памяти (после получения данных)
                    seats = seats
                        .OrderBy(s => {
                            if (int.TryParse(s.Number_seats, out int num))
                                return num;
                            return 0;
                        })
                        .ToList();

                    var seatList = new List<SeatDisplay>();
                    int availableCount = 0;

                    foreach (var s in seats)
                    {
                        // Определяем характеристики места
                        string features = GetSeatFeatures(s.Number_seats);
                        if (s.IsAvailable == true) availableCount++;

                        seatList.Add(new SeatDisplay
                        {
                            ID_Seat = s.ID_Seat,
                            Number_seats = s.Number_seats,
                            Type_seats = s.Type_seats ?? "Стандартное",
                            IsAvailable = s.IsAvailable ?? true,
                            Price = s.Price.ToString("N0") + " ₽",
                            PriceValue = s.Price,
                            Features = features
                        });
                    }

                    SeatsGrid.ItemsSource = seatList;
                    SeatsStatsText.Text = $"Всего мест: {seats.Count} | Свободно: {availableCount} | Занято: {seats.Count - availableCount}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки мест: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetSeatFeatures(string seatNumber)
        {
            var features = new List<string>();

            if (int.TryParse(seatNumber, out int num))
            {
                if (num % 2 == 1)
                    features.Add("нижнее");
                else
                    features.Add("верхнее");

                if (num % 4 == 1 || num % 4 == 2)
                    features.Add("у окна");
                else
                    features.Add("у прохода");

                if (num % 4 == 1 || num % 4 == 2)
                    features.Add("розетка");
            }

            return string.Join(", ", features);
        }

        private void AddSeat_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SeatEditWindow(_wagonId, null);
            if (dialog.ShowDialog() == true)
            {
                LoadSeats();
            }
        }

        private void EditSeat_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                int seatId = Convert.ToInt32(button.Tag);
                using (var db = new TrainCoreDiplomEntities1())
                {
                    var seat = db.Seats.Find(seatId);
                    if (seat != null)
                    {
                        var dialog = new SeatEditWindow(_wagonId, seat);
                        if (dialog.ShowDialog() == true)
                        {
                            LoadSeats();
                        }
                    }
                }
            }
        }

        private void DeleteSeat_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                int seatId = Convert.ToInt32(button.Tag);

                var result = MessageBox.Show("Удалить это место?", "Подтверждение",
                                           MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var db = new TrainCoreDiplomEntities1())
                        {
                            var seat = db.Seats.Find(seatId);
                            if (seat != null)
                            {
                                db.Seats.Remove(seat);
                                db.SaveChanges();

                                MessageBox.Show("Место удалено", "Успех",
                                              MessageBoxButton.OK, MessageBoxImage.Information);
                                LoadSeats();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}