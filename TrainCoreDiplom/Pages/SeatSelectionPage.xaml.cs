using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TrainCoreDiplom.DBConnection;

namespace TrainCoreDiplom.Pages
{
    public partial class SeatSelectionPage : Page
    {
        private Schedule _currentSchedule;
        private DateTime _selectedDate;
        private List<Wagons> _wagons;
        private List<Seats> _currentSeats;
        private List<Seats> _selectedSeats = new List<Seats>();
        private int _passengerCount = 1;
        private Wagons _selectedWagon;
        private Stations _fromStation;
        private Stations _toStation;

        public class WagonDisplay
        {
            public int Id { get; set; }
            public string DisplayName { get; set; }
            public Wagons Wagon { get; set; }
        }

        public class SeatRecommendation
        {
            public int SeatId { get; set; }
            public string SeatNumber { get; set; }
            public string MatchReason { get; set; }
            public string Price { get; set; }
        }

        public class SeatDisplay
        {
            public int ID_Seat { get; set; }
            public string Number_seats { get; set; }
            public bool? IsAvailable { get; set; }
            public decimal Price { get; set; }
            public string SeatColor { get; set; }
        }

        public SeatSelectionPage(Schedule schedule, DateTime date, Stations fromStation, Stations toStation)
        {
            InitializeComponent();
            _currentSchedule = schedule;
            _selectedDate = date;
            _fromStation = fromStation;
            _toStation = toStation;

            if (PassengerCountComboBox != null)
                PassengerCountComboBox.SelectedIndex = 0;

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                using (var db = new TrainCoreDiplomEntities1())
                {
                    var fullSchedule = db.Schedule
                        .FirstOrDefault(s => s.ID_Schedule == _currentSchedule.ID_Schedule);

                    if (fullSchedule != null)
                    {
                        _currentSchedule = fullSchedule;

                        string trainNumber = _currentSchedule.Trains?.Number_train ?? "";
                        string trainName = _currentSchedule.Trains?.Name_train ?? "";

                        if (TrainInfoText != null)
                            TrainInfoText.Text = $"{trainNumber} {trainName}".Trim();

                        if (RouteInfoText != null)
                            RouteInfoText.Text = $"{_fromStation.Name_Station} → {_toStation.Name_Station}";

                        if (DateInfoText != null)
                            DateInfoText.Text = _selectedDate.ToString("dd.MM.yyyy");
                    }

                    _wagons = db.Wagons
                        .Where(w => w.ID_Train == _currentSchedule.ID_Train)
                        .OrderBy(w => w.Number_wagon)
                        .ToList();

                    var wagonItems = new List<WagonDisplay>();
                    foreach (var w in _wagons)
                    {
                        string typeName = w.Type_Wagons?.Name_type_wagon ?? "Вагон";
                        wagonItems.Add(new WagonDisplay
                        {
                            Id = w.ID_Wagon,
                            DisplayName = $"Вагон {w.Number_wagon} ({typeName})",
                            Wagon = w
                        });
                    }

                    if (WagonComboBox != null)
                    {
                        WagonComboBox.ItemsSource = wagonItems;
                        if (wagonItems.Count > 0)
                            WagonComboBox.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadSeats(int wagonId)
        {
            try
            {
                using (var db = new TrainCoreDiplomEntities1())
                {
                    _currentSeats = db.Seats
                        .Where(s => s.ID_Wagon == wagonId)
                        .OrderBy(s => s.Number_seats)
                        .ToList();

                    var displaySeats = new List<SeatDisplay>();

                    foreach (var seat in _currentSeats)
                    {
                        string color = "#4CAF50";

                        if (_selectedSeats.Any(s => s.ID_Seat == seat.ID_Seat))
                            color = "#1E88E5";
                        else if (seat.IsAvailable == false)
                            color = "#F44336";

                        displaySeats.Add(new SeatDisplay
                        {
                            ID_Seat = seat.ID_Seat,
                            Number_seats = seat.Number_seats,
                            IsAvailable = seat.IsAvailable,
                            Price = seat.Price,
                            SeatColor = color
                        });
                    }

                    var rows = new List<List<SeatDisplay>>();
                    for (int i = 0; i < displaySeats.Count; i += 6)
                    {
                        rows.Add(displaySeats.Skip(i).Take(6).ToList());
                    }

                    if (SeatsRowsItemsControl != null)
                        SeatsRowsItemsControl.ItemsSource = rows;

                    if (_selectedWagon != null && WagonTitleText != null)
                        WagonTitleText.Text = $"Схема вагона {_selectedWagon.Number_wagon}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки мест: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void WagonComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = WagonComboBox.SelectedItem as WagonDisplay;
            if (selected != null)
            {
                _selectedWagon = selected.Wagon;
                _selectedSeats.Clear();
                LoadSeats(selected.Id);
                UpdateSelectedSeatsDisplay();
            }
        }

        private void PassengerCountChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PassengerCountComboBox?.SelectedItem == null) return;

            _passengerCount = PassengerCountComboBox.SelectedIndex + 1;
            _selectedSeats.Clear();

            if (_selectedWagon != null)
                LoadSeats(_selectedWagon.ID_Wagon);

            UpdateSelectedSeatsDisplay();
        }

        private void SeatButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            int seatId = Convert.ToInt32(button.Tag);
            var seat = _currentSeats.FirstOrDefault(s => s.ID_Seat == seatId);

            if (seat == null || seat.IsAvailable == false) return;

            if (_selectedSeats.Any(s => s.ID_Seat == seat.ID_Seat))
            {
                var seatToRemove = _selectedSeats.First(s => s.ID_Seat == seat.ID_Seat);
                _selectedSeats.Remove(seatToRemove);
            }
            else
            {
                if (_selectedSeats.Count >= _passengerCount)
                {
                    MessageBox.Show($"Можно выбрать только {_passengerCount} мест",
                                  "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _selectedSeats.Add(seat);
            }

            if (_selectedWagon != null)
                LoadSeats(_selectedWagon.ID_Wagon);

            UpdateSelectedSeatsDisplay();
        }

        private void UpdateSelectedSeatsDisplay()
        {
            if (SelectedCountText == null || SelectedSeatsListText == null ||
                TotalPriceText == null || ContinueButton == null)
                return;

            if (_selectedSeats.Count == 0)
            {
                SelectedCountText.Text = "0";
                SelectedSeatsListText.Text = "(не выбраны)";
                TotalPriceText.Text = "0 ₽";
                ContinueButton.IsEnabled = false;
            }
            else
            {
                SelectedCountText.Text = _selectedSeats.Count.ToString();
                string seats = string.Join(", ", _selectedSeats.Select(s => s.Number_seats));
                SelectedSeatsListText.Text = $"({seats})";

                decimal totalPrice = _selectedSeats.Sum(s => s.Price);
                TotalPriceText.Text = $"{totalPrice:N0} ₽";

                ContinueButton.IsEnabled = _selectedSeats.Count == _passengerCount;
            }
        }

        private void QuickSelectSeat_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag == null) return;

            int seatId = Convert.ToInt32(button.Tag);
            var seat = _currentSeats.FirstOrDefault(s => s.ID_Seat == seatId);

            if (seat != null && seat.IsAvailable == true)
            {
                _selectedSeats.Clear();
                _selectedSeats.Add(seat);

                if (_selectedWagon != null)
                    LoadSeats(_selectedWagon.ID_Wagon);

                UpdateSelectedSeatsDisplay();
            }
        }

        private void PreferenceChanged(object sender, RoutedEventArgs e)
        {
            // Ничего не делаем, ждем нажатия кнопки
        }

        private void RecommendButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentSeats == null || !_currentSeats.Any())
            {
                MessageBox.Show("Сначала выберите вагон", "Информация",
                              MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var recommendations = GetRecommendations();

            if (RecommendationsItemsControl != null)
            {
                RecommendationsItemsControl.ItemsSource = recommendations;

                if (recommendations == null || !recommendations.Any())
                {
                    var emptyList = new List<SeatRecommendation>
                    {
                        new SeatRecommendation
                        {
                            SeatNumber = "—",
                            MatchReason = "Нет мест по вашим предпочтениям",
                            Price = ""
                        }
                    };
                    RecommendationsItemsControl.ItemsSource = emptyList;
                }
            }
        }

        private List<SeatRecommendation> GetRecommendations()
        {
            var recommendations = new List<SeatRecommendation>();

            if (_currentSeats == null) return recommendations;

            var availableSeats = _currentSeats.Where(s => s.IsAvailable == true).ToList();

            if (!availableSeats.Any()) return recommendations;

            foreach (var seat in availableSeats)
            {
                int score = 0;
                var reasons = new List<string>();

                if (PreferWindowCheckBox != null && PreferWindowCheckBox.IsChecked == true && IsWindowSeat(seat.Number_seats))
                {
                    score += 30;
                    reasons.Add("у окна");
                }

                if (PreferAisleCheckBox != null && PreferAisleCheckBox.IsChecked == true && !IsWindowSeat(seat.Number_seats))
                {
                    score += 30;
                    reasons.Add("у прохода");
                }

                if (PreferLowerCheckBox != null && PreferLowerCheckBox.IsChecked == true && IsLowerSeat(seat.Number_seats))
                {
                    score += 25;
                    reasons.Add("нижняя полка");
                }

                if (PreferPowerCheckBox != null && PreferPowerCheckBox.IsChecked == true && HasPowerOutlet(seat.Number_seats))
                {
                    score += 20;
                    reasons.Add("есть розетка");
                }

                if (AvoidToiletCheckBox != null && AvoidToiletCheckBox.IsChecked == true && IsNearToilet(seat.Number_seats))
                {
                    score -= 50;
                }

                if (score > 0)
                {
                    string reasonText = reasons.Any() ? string.Join(", ", reasons) : "подходит по критериям";

                    recommendations.Add(new SeatRecommendation
                    {
                        SeatId = seat.ID_Seat,
                        SeatNumber = seat.Number_seats,
                        MatchReason = reasonText,
                        Price = $"{seat.Price:N0} ₽"
                    });
                }
            }

            return recommendations.OrderByDescending(r => r.Price).Take(5).ToList();
        }

        private bool IsWindowSeat(string seatNumber)
        {
            return int.TryParse(seatNumber, out int num) && num % 2 == 1;
        }

        private bool IsLowerSeat(string seatNumber)
        {
            return int.TryParse(seatNumber, out int num) && num % 2 == 1;
        }

        private bool HasPowerOutlet(string seatNumber)
        {
            return int.TryParse(seatNumber, out int num) && (num % 4 == 1 || num % 4 == 2);
        }

        private bool IsNearToilet(string seatNumber)
        {
            return int.TryParse(seatNumber, out int num) && (num <= 4 || num >= 50);
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedSeats.Count != _passengerCount)
            {
                MessageBox.Show($"Выберите {_passengerCount} мест", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_passengerCount == 1)
            {
                NavigationService.Navigate(new PassengerInfoPage(
                    _currentSchedule,
                    _selectedSeats[0],
                    _selectedWagon,
                    _selectedDate,
                    _fromStation,
                    _toStation));
            }
            else
            {
                string seats = string.Join(", ", _selectedSeats.Select(s => s.Number_seats));
                decimal totalPrice = _selectedSeats.Sum(s => s.Price);

                MessageBox.Show($"Выбраны места: {seats}\nОбщая стоимость: {totalPrice:N0} ₽\n\n" +
                              "Покупка нескольких билетов будет доступна в следующей версии.",
                              "Информация", MessageBoxButton.OK, MessageBoxImage.Information);

                _selectedSeats.Clear();
                if (_selectedWagon != null)
                    LoadSeats(_selectedWagon.ID_Wagon);
                UpdateSelectedSeatsDisplay();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}