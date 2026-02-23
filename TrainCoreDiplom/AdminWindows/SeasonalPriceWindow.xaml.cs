using System;
using System.Windows;

namespace TrainCoreDiplom.AdminWindows
{
    public partial class SeasonalPriceWindow : Window
    {
        private string _season;

        public SeasonalPriceWindow(string season = null)
        {
            InitializeComponent();

            if (!string.IsNullOrEmpty(season))
            {
                _season = season;
                SeasonTextBox.Text = season;
                SeasonTextBox.IsEnabled = false;
                Title = "Редактирование сезона";
            }
            else
            {
                Title = "Добавление сезона";
            }

            StartDatePicker.SelectedDate = DateTime.Today;
            EndDatePicker.SelectedDate = DateTime.Today.AddMonths(3);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SeasonTextBox.Text))
            {
                MessageBox.Show("Введите название сезона", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(CoeffTextBox.Text, out decimal coeff) || coeff <= 0)
            {
                MessageBox.Show("Введите корректный коэффициент (больше 0)", "Ошибка",
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
            MessageBox.Show("Сезонный коэффициент сохранен", "Успех",
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