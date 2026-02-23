using System;
using System.Windows;
using System.Windows.Controls;
using TrainCoreDiplom.DBConnection;

namespace TrainCoreDiplom.AdminWindows
{
    public partial class SeatEditWindow : Window
    {
        private int _wagonId;
        private Seats _seat;
        private bool _isEdit;

        public SeatEditWindow(int wagonId, Seats seat = null)
        {
            InitializeComponent();
            _wagonId = wagonId;

            if (seat != null)
            {
                _seat = seat;
                _isEdit = true;
                NumberTextBox.Text = seat.Number_seats;
                PriceTextBox.Text = seat.Price.ToString();
                IsAvailableCheckBox.IsChecked = seat.IsAvailable;

                // Устанавливаем тип места
                if (!string.IsNullOrEmpty(seat.Type_seats))
                {
                    foreach (ComboBoxItem item in TypeComboBox.Items)
                    {
                        if (item.Content.ToString() == seat.Type_seats)
                        {
                            item.IsSelected = true;
                            break;
                        }
                    }
                }

                TitleText.Text = "Редактирование места";
            }
            else
            {
                _isEdit = false;
                TitleText.Text = "Добавление места";
                PriceTextBox.Text = "1000";
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NumberTextBox.Text))
            {
                MessageBox.Show("Введите номер места", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(PriceTextBox.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("Введите корректную цену", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new TrainCoreDiplomEntities1())
                {
                    if (_isEdit)
                    {
                        var seat = db.Seats.Find(_seat.ID_Seat);
                        if (seat != null)
                        {
                            seat.Number_seats = NumberTextBox.Text.Trim();
                            seat.Type_seats = (TypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                            seat.Price = price;
                            seat.IsAvailable = IsAvailableCheckBox.IsChecked;
                        }
                    }
                    else
                    {
                        var newSeat = new Seats
                        {
                            ID_Wagon = _wagonId,
                            Number_seats = NumberTextBox.Text.Trim(),
                            Type_seats = (TypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString(),
                            Price = price,
                            IsAvailable = IsAvailableCheckBox.IsChecked
                        };
                        db.Seats.Add(newSeat);
                    }
                    db.SaveChanges();
                }

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