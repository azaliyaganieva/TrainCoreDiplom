using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using TrainCoreDiplom.DBConnection;

namespace TrainCoreDiplom.AdminWindows
{
    public partial class TrainEditWindow : Window, INotifyPropertyChanged
    {
        private Trains _train;
        private bool _isEdit;
        private int _wagonsCount = 6;
        private int _totalSeats;

        public event PropertyChangedEventHandler PropertyChanged;

        public int WagonsCount
        {
            get => _wagonsCount;
            set
            {
                _wagonsCount = value;
                OnPropertyChanged();
                WagonsCountTextBox.Text = value.ToString();
                CalculateTotalSeats();
            }
        }

        public int TotalSeats
        {
            get => _totalSeats;
            set
            {
                _totalSeats = value;
                OnPropertyChanged();
                TotalSeatsText.Text = value.ToString();
            }
        }

        public TrainEditWindow(Trains train = null)
        {
            InitializeComponent();
            DataContext = this;
            LoadComboBoxes();

            if (train != null)
            {
                _train = train;
                _isEdit = true;
                NumberTextBox.Text = train.Number_train;
                NameTextBox.Text = train.Name_train;
                TypeComboBox.SelectedValue = train.ID_type_train;
                TitleText.Text = "Редактирование поезда";
                LoadWagonsInfo();
            }
            else
            {
                _isEdit = false;
                TitleText.Text = "Добавление поезда";
                CalculateTotalSeats();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void LoadComboBoxes()
        {
            using (var db = new TrainCoreDiplomEntities1())
            {
                TypeComboBox.ItemsSource = db.Type_Trains.ToList();
                TypeComboBox.DisplayMemberPath = "Name_type_train";
                TypeComboBox.SelectedValuePath = "ID_type_train";

                WagonTypeComboBox.ItemsSource = db.Type_Wagons.ToList();
                WagonTypeComboBox.DisplayMemberPath = "Name_type_wagon";
                WagonTypeComboBox.SelectedValuePath = "ID_type_wagon";
                WagonTypeComboBox.SelectedValue = 1;
            }
        }

        private void LoadWagonsInfo()
        {
            try
            {
                using (var db = new TrainCoreDiplomEntities1())
                {
                    WagonsCount = db.Wagons.Count(w => w.ID_Train == _train.ID_Train);

                    var firstWagon = db.Wagons.FirstOrDefault(w => w.ID_Train == _train.ID_Train);
                    if (firstWagon != null)
                    {
                        WagonTypeComboBox.SelectedValue = firstWagon.ID_type_wagon;
                    }

                    TotalSeats = db.Seats.Count(s => s.Wagons.ID_Train == _train.ID_Train);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки вагонов: {ex.Message}");
            }
        }

        private void CalculateTotalSeats()
        {
            int seatsPerWagon = 54;
            if (WagonTypeComboBox.SelectedItem != null)
            {
                var selectedType = WagonTypeComboBox.SelectedItem as Type_Wagons;
                seatsPerWagon = selectedType?.Count_seats ?? 54;
            }

            TotalSeats = WagonsCount * seatsPerWagon;
        }

        private void IncreaseWagons_Click(object sender, RoutedEventArgs e)
        {
            WagonsCount++;
        }

        private void DecreaseWagons_Click(object sender, RoutedEventArgs e)
        {
            if (WagonsCount > 1)
            {
                WagonsCount--;
            }
        }

        private void WagonTypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            CalculateTotalSeats();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NumberTextBox.Text))
            {
                MessageBox.Show("Введите номер поезда", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (TypeComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите тип поезда", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new TrainCoreDiplomEntities1())
                {
                    if (_isEdit)
                    {
                        var train = db.Trains.Find(_train.ID_Train);
                        if (train != null)
                        {
                            train.Number_train = NumberTextBox.Text.Trim();
                            train.Name_train = NameTextBox.Text.Trim();
                            train.ID_type_train = (int)TypeComboBox.SelectedValue;
                            UpdateWagons(db, train.ID_Train);
                        }
                    }
                    else
                    {
                        var newTrain = new Trains
                        {
                            Number_train = NumberTextBox.Text.Trim(),
                            Name_train = NameTextBox.Text.Trim(),
                            ID_type_train = (int)TypeComboBox.SelectedValue
                        };
                        db.Trains.Add(newTrain);
                        db.SaveChanges();
                        CreateWagons(db, newTrain.ID_Train);
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

        private void CreateWagons(TrainCoreDiplomEntities1 db, int trainId)
        {
            int wagonTypeId = (int)(WagonTypeComboBox.SelectedValue ?? 1);

            for (int i = 1; i <= _wagonsCount; i++)
            {
                var wagon = new Wagons
                {
                    ID_Train = trainId,
                    Number_wagon = i.ToString("00"),
                    ID_type_wagon = wagonTypeId
                };
                db.Wagons.Add(wagon);
                db.SaveChanges();
                CreateSeatsForWagon(db, wagon.ID_Wagon, wagonTypeId);
            }
        }

        private void CreateSeatsForWagon(TrainCoreDiplomEntities1 db, int wagonId, int typeId)
        {
            var typeWagon = db.Type_Wagons.Find(typeId);
            if (typeWagon == null) return;

            for (int i = 1; i <= typeWagon.Count_seats; i++)
            {
                db.Seats.Add(new Seats
                {
                    ID_Wagon = wagonId,
                    Number_seats = i.ToString(),
                    IsAvailable = true,
                    Price = typeWagon.Base_price,
                    Type_seats = i % 2 == 1 ? "Нижнее" : "Верхнее"
                });
            }
        }

        private void UpdateWagons(TrainCoreDiplomEntities1 db, int trainId)
        {
            var currentWagons = db.Wagons.Where(w => w.ID_Train == trainId).ToList();
            int currentCount = currentWagons.Count;

            if (currentCount < _wagonsCount)
            {
                int wagonTypeId = (int)(WagonTypeComboBox.SelectedValue ?? 1);
                for (int i = currentCount + 1; i <= _wagonsCount; i++)
                {
                    var wagon = new Wagons
                    {
                        ID_Train = trainId,
                        Number_wagon = i.ToString("00"),
                        ID_type_wagon = wagonTypeId
                    };
                    db.Wagons.Add(wagon);
                    db.SaveChanges();
                    CreateSeatsForWagon(db, wagon.ID_Wagon, wagonTypeId);
                }
            }
            else if (currentCount > _wagonsCount)
            {
                var wagonsToRemove = currentWagons.Skip(_wagonsCount).ToList();
                foreach (var wagon in wagonsToRemove)
                {
                    var seats = db.Seats.Where(s => s.ID_Wagon == wagon.ID_Wagon).ToList();
                    db.Seats.RemoveRange(seats);
                    db.Wagons.Remove(wagon);
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}