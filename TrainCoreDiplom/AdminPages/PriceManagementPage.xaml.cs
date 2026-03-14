using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TrainCoreDiplom.AdminWindows;
using TrainCoreDiplom.DBConnection;

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

        public class SeasonalPriceItem : INotifyPropertyChanged
        {
            private decimal _coefficient;
            private DateTime _startDate;
            private DateTime _endDate;

            public string Season { get; set; }

            public decimal Coefficient
            {
                get => _coefficient;
                set { _coefficient = value; OnPropertyChanged(); }
            }

            public DateTime StartDate
            {
                get => _startDate;
                set { _startDate = value; OnPropertyChanged(); }
            }

            public DateTime EndDate
            {
                get => _endDate;
                set { _endDate = value; OnPropertyChanged(); }
            }

            public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
            {
                PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
            }
        }

        public class DiscountItem : INotifyPropertyChanged
        {
            private string _name;
            private string _type;
            private string _value;
            private DateTime _validFrom;
            private DateTime _validTo;

            public string Name
            {
                get => _name;
                set { _name = value; OnPropertyChanged(); }
            }

            public string Type
            {
                get => _type;
                set { _type = value; OnPropertyChanged(); }
            }

            public string Value
            {
                get => _value;
                set { _value = value; OnPropertyChanged(); }
            }

            public DateTime ValidFrom
            {
                get => _validFrom;
                set { _validFrom = value; OnPropertyChanged(); }
            }

            public DateTime ValidTo
            {
                get => _validTo;
                set { _validTo = value; OnPropertyChanged(); }
            }

            public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
            {
                PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
            }
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
                    new SeasonalPriceItem {
                        Season = "Лето",
                        Coefficient = 1.3m,
                        StartDate = new DateTime(DateTime.Now.Year, 6, 1),
                        EndDate = new DateTime(DateTime.Now.Year, 8, 31)
                    },
                    new SeasonalPriceItem {
                        Season = "Зима",
                        Coefficient = 1.2m,
                        StartDate = new DateTime(DateTime.Now.Year, 12, 20),
                        EndDate = new DateTime(DateTime.Now.Year + 1, 1, 10)
                    },
                    new SeasonalPriceItem {
                        Season = "Весна",
                        Coefficient = 1.0m,
                        StartDate = new DateTime(DateTime.Now.Year, 3, 1),
                        EndDate = new DateTime(DateTime.Now.Year, 5, 31)
                    },
                    new SeasonalPriceItem {
                        Season = "Осень",
                        Coefficient = 1.1m,
                        StartDate = new DateTime(DateTime.Now.Year, 9, 1),
                        EndDate = new DateTime(DateTime.Now.Year, 11, 30)
                    }
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
                    new DiscountItem {
                        Name = "Детский билет",
                        Type = "Процентная",
                        Value = "50%",
                        ValidFrom = DateTime.Today,
                        ValidTo = DateTime.Today.AddYears(1)
                    },
                    new DiscountItem {
                        Name = "Студенческий",
                        Type = "Процентная",
                        Value = "25%",
                        ValidFrom = DateTime.Today,
                        ValidTo = DateTime.Today.AddYears(1)
                    },
                    new DiscountItem {
                        Name = "Пенсионный",
                        Type = "Процентная",
                        Value = "30%",
                        ValidFrom = DateTime.Today,
                        ValidTo = DateTime.Today.AddYears(1)
                    },
                    new DiscountItem {
                        Name = "Раннее бронирование",
                        Type = "Процентная",
                        Value = "15%",
                        ValidFrom = DateTime.Today,
                        ValidTo = DateTime.Today.AddMonths(3)
                    }
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

        private void CreateDiscount_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new DiscountWindow(null);
            if (dialog.ShowDialog() == true)
            {
                LoadDiscounts();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}