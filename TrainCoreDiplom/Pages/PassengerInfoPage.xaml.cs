using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TrainCoreDiplom.DBConnection;

namespace TrainCoreDiplom.Pages
{
    public partial class PassengerInfoPage : Page
    {
        private Schedule _schedule;
        private Seats _seat;
        private Wagons _wagon;
        private DateTime _date;
        private Stations _fromStation;
        private Stations _toStation;

        public PassengerInfoPage(Schedule schedule, Seats seat, Wagons wagon, DateTime date,
                                 Stations fromStation, Stations toStation)
        {
            InitializeComponent();
            _schedule = schedule;
            _seat = seat;
            _wagon = wagon;
            _date = date;
            _fromStation = fromStation;
            _toStation = toStation;

            DisplayInfo();
            LoadUserData();
        }

        private void DisplayInfo()
        {
            try
            {
                if (TrainInfoText != null)
                {
                    string trainNumber = _schedule.Trains?.Number_train ?? "";
                    string trainName = _schedule.Trains?.Name_train ?? "";
                    TrainInfoText.Text = $"{trainNumber} {trainName}".Trim();
                }

                if (RouteInfoText != null)
                {
                    RouteInfoText.Text = $"{_fromStation?.Name_Station} → {_toStation?.Name_Station}";
                }

                if (DateInfoText != null)
                {
                    DateInfoText.Text = _date.ToString("dd.MM.yyyy");
                }

                if (SeatInfoText != null)
                {
                    SeatInfoText.Text = $"Вагон {_wagon.Number_wagon}, место {_seat.Number_seats}";
                }

                if (PriceText != null)
                {
                    PriceText.Text = $"{_seat.Price:N0} ₽";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка отображения: {ex.Message}");
            }
        }

        private void LoadUserData()
        {
            try
            {
                if (App.CurrentUser != null && !string.IsNullOrEmpty(App.CurrentUser.Email))
                {
                    using (var db = new TrainCoreDiplomEntities1())
                    {
                        var passenger = db.Passangers
                            .FirstOrDefault(p => p.Email == App.CurrentUser.Email);

                        if (passenger != null)
                        {
                            if (LastNameTextBox != null)
                                LastNameTextBox.Text = passenger.Fam_Pas ?? "";

                            if (FirstNameTextBox != null)
                                FirstNameTextBox.Text = passenger.Name_Pas ?? "";

                            if (EmailTextBox != null)
                                EmailTextBox.Text = passenger.Email ?? "";

                            if (PhoneTextBox != null)
                                PhoneTextBox.Text = passenger.Phone ?? "";

                            if (PassportTextBox != null)
                                PassportTextBox.Text = passenger.Number_passport ?? "";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки данных пользователя: {ex.Message}");
            }
        }

        private void FieldChanged(object sender, TextChangedEventArgs e)
        {
            ValidateForm();
        }

        private bool ValidateForm()
        {
            bool isValid = true;

            if (LastNameTextBox == null || string.IsNullOrWhiteSpace(LastNameTextBox.Text))
                isValid = false;

            if (FirstNameTextBox == null || string.IsNullOrWhiteSpace(FirstNameTextBox.Text))
                isValid = false;

            if (PassportTextBox == null || string.IsNullOrWhiteSpace(PassportTextBox.Text))
                isValid = false;

            if (ContinueButton != null)
                ContinueButton.IsEnabled = isValid;

            return isValid;
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
            {
                MessageBox.Show("Заполните все обязательные поля", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                NavigationService.Navigate(new PaymentPage(
                    _schedule,
                    _seat,
                    _wagon,
                    _date,
                    LastNameTextBox?.Text.Trim() ?? "",
                    FirstNameTextBox?.Text.Trim() ?? "",
                    PassportTextBox?.Text.Trim() ?? "",
                    EmailTextBox?.Text.Trim() ?? "",
                    _fromStation,
                    _toStation));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SavePassenger()
        {
            try
            {
                using (var db = new TrainCoreDiplomEntities1())
                {
                    string passportNumber = PassportTextBox?.Text.Trim() ?? "";

                    if (string.IsNullOrEmpty(passportNumber))
                        return;

                    var existingPassenger = db.Passangers
                        .FirstOrDefault(p => p.Number_passport == passportNumber);

                    if (existingPassenger == null)
                    {
                        var passenger = new Passangers
                        {
                            Fam_Pas = LastNameTextBox?.Text.Trim() ?? "",
                            Name_Pas = FirstNameTextBox?.Text.Trim() ?? "",
                            Number_passport = passportNumber,
                            Email = EmailTextBox?.Text.Trim() ?? "",
                            Phone = PhoneTextBox?.Text.Trim() ?? ""
                        };
                        db.Passangers.Add(passenger);
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения пассажира: {ex.Message}");
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}