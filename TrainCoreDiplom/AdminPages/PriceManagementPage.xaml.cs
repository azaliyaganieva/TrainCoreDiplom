using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TrainCoreDiplom.DBConnection;
using TrainCoreDiplom.AdminWindows;

namespace TrainCoreDiplom.AdminPages
{
    public partial class PriceManagementPage : Page
    {
        public class BasePriceItem
        {
            public int ID_type_wagon { get; set; }
            public string Name_type_wagon { get; set; }
            public int Count_seats { get; set; }
            public decimal Base_price { get; set; }
        }

        public class SeasonalPriceItem
        {
            public string Season { get; set; }
            public decimal Coefficient { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
        }

        public class DiscountItem
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public string Value { get; set; }
            public DateTime ValidFrom { get; set; }
            public DateTime ValidTo { get; set; }
        }

        public PriceManagementPage()
        {
            InitializeComponent();
            LoadBasePrices();
            LoadSeasonalPrices();
            LoadDiscounts();
        }

        private void LoadBasePrices()
        {
            try
            {
                using (var db = new TrainCoreDiplomEntities1())
                {
                    var prices = db.Type_Wagons
                        .Select(t => new BasePriceItem
                        {
                            ID_type_wagon = t.ID_type_wagon,
                            Name_type_wagon = t.Name_type_wagon,
                            Count_seats = t.Count_seats,
                            Base_price = t.Base_price
                        })
                        .ToList();

                    BasePricesGrid.ItemsSource = prices;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки цен: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadSeasonalPrices()
        {
            try
            {
                var seasonal = new List<SeasonalPriceItem>
                {
                    new SeasonalPriceItem { Season = "Лето", Coefficient = 1.3m,
                        StartDate = new DateTime(DateTime.Now.Year, 6, 1),
                        EndDate = new DateTime(DateTime.Now.Year, 8, 31) },
                    new SeasonalPriceItem { Season = "Зима", Coefficient = 1.2m,
                        StartDate = new DateTime(DateTime.Now.Year, 12, 20),
                        EndDate = new DateTime(DateTime.Now.Year + 1, 1, 10) },
                    new SeasonalPriceItem { Season = "Весна", Coefficient = 1.0m,
                        StartDate = new DateTime(DateTime.Now.Year, 3, 1),
                        EndDate = new DateTime(DateTime.Now.Year, 5, 31) },
                    new SeasonalPriceItem { Season = "Осень", Coefficient = 1.0m,
                        StartDate = new DateTime(DateTime.Now.Year, 9, 1),
                        EndDate = new DateTime(DateTime.Now.Year, 11, 30) }
                };

                SeasonalPricesGrid.ItemsSource = seasonal;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки сезонных цен: {ex.Message}");
            }
        }

        private void LoadDiscounts()
        {
            try
            {
                var discounts = new List<DiscountItem>
                {
                    new DiscountItem { Name = "Детский билет", Type = "Процентная", Value = "50%",
                        ValidFrom = DateTime.Today, ValidTo = DateTime.Today.AddYears(1) },
                    new DiscountItem { Name = "Студенческий", Type = "Процентная", Value = "25%",
                        ValidFrom = DateTime.Today, ValidTo = DateTime.Today.AddYears(1) },
                    new DiscountItem { Name = "Пенсионный", Type = "Процентная", Value = "30%",
                        ValidFrom = DateTime.Today, ValidTo = DateTime.Today.AddYears(1) },
                    new DiscountItem { Name = "Раннее бронирование", Type = "Процентная", Value = "15%",
                        ValidFrom = DateTime.Today, ValidTo = DateTime.Today.AddMonths(3) }
                };

                DiscountsGrid.ItemsSource = discounts;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки скидок: {ex.Message}");
            }
        }

        private void SaveBasePrice_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                int typeId = Convert.ToInt32(button.Tag);

                // Находим строку в DataGrid
                var item = BasePricesGrid.ItemsSource.Cast<BasePriceItem>()
                    .FirstOrDefault(x => x.ID_type_wagon == typeId);

                if (item != null)
                {
                    try
                    {
                        using (var db = new TrainCoreDiplomEntities1())
                        {
                            var typeWagon = db.Type_Wagons.Find(typeId);
                            if (typeWagon != null)
                            {
                                typeWagon.Base_price = item.Base_price;
                                db.SaveChanges();

                                MessageBox.Show("Цена сохранена", "Успех",
                                              MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void AddSeasonalCoeff_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SeasonalPriceWindow(null);
            if (dialog.ShowDialog() == true)
            {
                LoadSeasonalPrices();
            }
        }

        private void EditSeasonalCoeff_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                string season = button.Tag.ToString();
                var dialog = new SeasonalPriceWindow(season);
                if (dialog.ShowDialog() == true)
                {
                    LoadSeasonalPrices();
                }
            }
        }

        private void SaveSeasonalCoeff_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                string season = button.Tag.ToString();
                // Здесь логика сохранения
                MessageBox.Show($"Сохранено для {season}", "Успех",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CreateDiscount_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new DiscountWindow(null);
            if (dialog.ShowDialog() == true)
            {
                LoadDiscounts();
            }
        }

        private void EditDiscount_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                string discountName = button.Tag.ToString();
                var dialog = new DiscountWindow(discountName);
                if (dialog.ShowDialog() == true)
                {
                    LoadDiscounts();
                }
            }
        }

        private void DeleteDiscount_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                string discountName = button.Tag.ToString();
                var result = MessageBox.Show($"Удалить скидку '{discountName}'?",
                                           "Подтверждение", MessageBoxButton.YesNo,
                                           MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    MessageBox.Show("Скидка удалена", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadDiscounts();
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}