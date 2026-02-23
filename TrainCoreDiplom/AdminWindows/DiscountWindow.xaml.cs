using System;
using System.Windows;

namespace TrainCoreDiplom.AdminWindows
{
    public partial class DiscountWindow : Window
    {
        private string _discountName;

        public DiscountWindow(string discountName = null)
        {
            InitializeComponent();

            if (!string.IsNullOrEmpty(discountName))
            {
                _discountName = discountName;
                NameTextBox.Text = discountName;
                NameTextBox.IsEnabled = false;
                Title = "Редактирование скидки";
            }
            else
            {
                Title = "Добавление скидки";
            }

            StartDatePicker.SelectedDate = DateTime.Today;
            EndDatePicker.SelectedDate = DateTime.Today.AddYears(1);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Введите название скидки", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(ValueTextBox.Text))
            {
                MessageBox.Show("Введите размер скидки", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!StartDatePicker.SelectedDate.HasValue || !EndDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Выберите даты", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (StartDatePicker.SelectedDate > EndDatePicker.SelectedDate)
            {
                MessageBox.Show("Дата начала не может быть позже даты окончания", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Здесь можно добавить сохранение в БД
            MessageBox.Show("Скидка сохранена", "Успех",
                          MessageBoxButton.OK, MessageBoxImage.Information);

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}