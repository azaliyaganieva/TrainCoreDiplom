using System;
using System.Windows;
using System.Windows.Controls;
using TrainCoreDiplom.DBConnection;

namespace TrainCoreDiplom.Pages
{
    public partial class PaymentPage : Page
    {
        private Schedule _schedule;
        private Seats _seat;
        private Wagons _wagon;
        private DateTime _date;
        private string _lastName;
        private string _firstName;
        private string _passport;
        private string _email;
        private Stations _fromStation;
        private Stations _toStation;

        public PaymentPage(Schedule schedule, Seats seat, Wagons wagon, DateTime date,
                          string lastName, string firstName, string passport, string email,
                          Stations fromStation, Stations toStation)
        {
            InitializeComponent();
            _schedule = schedule;
            _seat = seat;
            _wagon = wagon;
            _date = date;
            _lastName = lastName;
            _firstName = firstName;
            _passport = passport;
            _email = email;
            _fromStation = fromStation;
            _toStation = toStation;

            DisplayInfo();
        }

        private void DisplayInfo()
        {
            TicketNumberText.Text = "Новый билет";
            RouteText.Text = $"{_fromStation?.Name_Station} → {_toStation?.Name_Station}";
            PassengerText.Text = $"{_lastName} {_firstName}";
            PriceText.Text = $"{_seat.Price:N0} ₽";
        }

        private void PayButton_Click(object sender, RoutedEventArgs e)
        {
            if (CardRadioButton.IsChecked == true)
            {
                if (!ValidateCardData())
                {
                    MessageBox.Show("Проверьте правильность введенных данных карты", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            MessageBox.Show("Оплата прошла успешно!", "Успех",
                          MessageBoxButton.OK, MessageBoxImage.Information);

            NavigationService.Navigate(new TicketConfirmationPage(
                _schedule,
                _seat,
                _wagon,
                _date,
                _lastName,
                _firstName,
                _passport,
                _email,
                _fromStation,
                _toStation));
        }

        private bool ValidateCardData()
        {
            if (CardNumberTextBox.Text.Replace(" ", "").Length != 16)
                return false;
            if (MonthTextBox.Text.Length != 2 || YearTextBox.Text.Length != 2)
                return false;
            if (CvvTextBox.Text.Length != 3)
                return false;
            return true;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}