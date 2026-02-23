using System;
using System.Linq;
using System.Windows;
using TrainCoreDiplom.DBConnection;

namespace TrainCoreDiplom.AdminWindows
{
    public partial class WagonEditWindow : Window
    {
        private int _trainId;
        private Wagons _wagon;
        private bool _isEdit;

        public WagonEditWindow(int trainId, Wagons wagon = null)
        {
            InitializeComponent();
            _trainId = trainId;

            // Загружаем типы вагонов
            using (var db = new TrainCoreDiplomEntities1())
            {
                TypeComboBox.ItemsSource = db.Type_Wagons.ToList();
            }

            if (wagon != null)
            {
                _wagon = wagon;
                _isEdit = true;
                NumberTextBox.Text = wagon.Number_wagon;
                TypeComboBox.SelectedValue = wagon.ID_type_wagon;
                TitleText.Text = "Редактирование вагона";
            }
            else
            {
                _isEdit = false;
                TitleText.Text = "Добавление вагона";
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NumberTextBox.Text))
            {
                MessageBox.Show("Введите номер вагона", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (TypeComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите тип вагона", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new TrainCoreDiplomEntities1())
                {
                    if (_isEdit)
                    {
                        var wagon = db.Wagons.Find(_wagon.ID_Wagon);
                        if (wagon != null)
                        {
                            wagon.Number_wagon = NumberTextBox.Text.Trim();
                            wagon.ID_type_wagon = (int)TypeComboBox.SelectedValue;
                        }
                    }
                    else
                    {
                        var newWagon = new Wagons
                        {
                            ID_Train = _trainId,
                            Number_wagon = NumberTextBox.Text.Trim(),
                            ID_type_wagon = (int)TypeComboBox.SelectedValue
                        };
                        db.Wagons.Add(newWagon);
                        db.SaveChanges();

                        // Автоматически создаем места для нового вагона
                        CreateSeatsForWagon(db, newWagon.ID_Wagon, (int)TypeComboBox.SelectedValue);
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

        private void CreateSeatsForWagon(TrainCoreDiplomEntities1 db, int wagonId, int typeId)
        {
            var typeWagon = db.Type_Wagons.Find(typeId);
            if (typeWagon == null) return;

            int seatsCount = typeWagon.Count_seats;
            decimal basePrice = typeWagon.Base_price;

            for (int i = 1; i <= seatsCount; i++)
            {
                var seat = new Seats
                {
                    ID_Wagon = wagonId,
                    Number_seats = i.ToString(),
                    IsAvailable = true,
                    Price = basePrice
                };
                db.Seats.Add(seat);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}