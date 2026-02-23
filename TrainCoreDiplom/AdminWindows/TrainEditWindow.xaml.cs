using System;
using System.Linq;
using System.Windows;
using TrainCoreDiplom.DBConnection;

namespace TrainCoreDiplom.AdminWindows
{
    public partial class TrainEditWindow : Window
    {
        private Trains _train;
        private bool _isEdit;

        public TrainEditWindow(Trains train = null)
        {
            InitializeComponent();

            using (var db = new TrainCoreDiplomEntities1())
            {
                TypeComboBox.ItemsSource = db.Type_Trains.ToList();
                TypeComboBox.DisplayMemberPath = "Name_type_train";
                TypeComboBox.SelectedValuePath = "ID_type_train";
            }

            if (train != null)
            {
                _train = train;
                _isEdit = true;
                NumberTextBox.Text = train.Number_train;
                NameTextBox.Text = train.Name_train;
                TypeComboBox.SelectedValue = train.ID_type_train;
                Title = "Редактирование поезда";
            }
            else
            {
                _isEdit = false;
                Title = "Добавление поезда";
            }
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