using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TrainCoreDiplom.DBConnection;

namespace TrainCoreDiplom.AdminWindows
{
    public partial class WagonsManagementPage : Page
    {
        private int _trainId;
        private Trains _train;

        public class WagonDisplay
        {
            public int ID_Wagon { get; set; }
            public string Number_wagon { get; set; }
            public string TypeName { get; set; }
            public int SeatsCount { get; set; }
            public int FreeSeats { get; set; }
            public string BasePrice { get; set; }
            public int TypeId { get; set; }
        }

        public WagonsManagementPage(int trainId)
        {
            InitializeComponent();
            _trainId = trainId;
            LoadTrainInfo();
            LoadWagons();
        }

        private void LoadTrainInfo()
        {
            try
            {
                using (var db = new TrainCoreDiplomEntities1())
                {
                    _train = db.Trains.Find(_trainId);
                    if (_train != null)
                    {
                        TitleText.Text = $"Управление вагонами";
                        TrainInfoText.Text = $"Поезд: {_train.Number_train} {_train.Name_train}";

                        var type = db.Type_Trains.Find(_train.ID_type_train);
                        string trainType = type?.Name_type_train ?? "Неизвестен";

                        TrainDetailsText.Text = $"Номер: {_train.Number_train} | Название: {_train.Name_train} | Тип: {trainType}";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки информации о поезде: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadWagons()
        {
            try
            {
                using (var db = new TrainCoreDiplomEntities1())
                {
                    var wagons = db.Wagons
                        .Where(w => w.ID_Train == _trainId)
                        .OrderBy(w => w.Number_wagon)
                        .ToList();

                    var wagonList = new List<WagonDisplay>();
                    int totalWagons = 0;
                    int totalSeats = 0;

                    foreach (var w in wagons)
                    {
                        // Загружаем тип вагона
                        db.Entry(w).Reference(x => x.Type_Wagons).Load();

                        // Считаем места
                        int seatsCount = db.Seats.Count(s => s.ID_Wagon == w.ID_Wagon);
                        int freeSeats = db.Seats.Count(s => s.ID_Wagon == w.ID_Wagon && s.IsAvailable == true);

                        totalWagons++;
                        totalSeats += seatsCount;

                        wagonList.Add(new WagonDisplay
                        {
                            ID_Wagon = w.ID_Wagon,
                            Number_wagon = w.Number_wagon,
                            TypeName = w.Type_Wagons?.Name_type_wagon ?? "Неизвестен",
                            SeatsCount = seatsCount,
                            FreeSeats = freeSeats,
                            BasePrice = w.Type_Wagons?.Base_price.ToString("N0") + " ₽" ?? "0 ₽",
                            TypeId = w.ID_type_wagon
                        });
                    }

                    WagonsGrid.ItemsSource = wagonList;
                    WagonsCountText.Text = $"Всего вагонов: {totalWagons} | Всего мест: {totalSeats}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки вагонов: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddWagon_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new WagonEditWindow(_trainId, null);
            if (dialog.ShowDialog() == true)
            {
                LoadWagons();
            }
        }

        private void EditWagon_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                int wagonId = Convert.ToInt32(button.Tag);
                using (var db = new TrainCoreDiplomEntities1())
                {
                    var wagon = db.Wagons.Find(wagonId);
                    if (wagon != null)
                    {
                        var dialog = new WagonEditWindow(_trainId, wagon);
                        if (dialog.ShowDialog() == true)
                        {
                            LoadWagons();
                        }
                    }
                }
            }
        }

        private void DeleteWagon_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                int wagonId = Convert.ToInt32(button.Tag);

                var result = MessageBox.Show("Вы уверены, что хотите удалить этот вагон?\nВсе места в вагоне также будут удалены!",
                                           "Подтверждение удаления",
                                           MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var db = new TrainCoreDiplomEntities1())
                        {
                            var wagon = db.Wagons.Find(wagonId);
                            if (wagon != null)
                            {
                                // Удаляем все места в вагоне
                                var seats = db.Seats.Where(s => s.ID_Wagon == wagonId).ToList();
                                db.Seats.RemoveRange(seats);

                                // Удаляем вагон
                                db.Wagons.Remove(wagon);
                                db.SaveChanges();

                                MessageBox.Show("Вагон успешно удален", "Успех",
                                              MessageBoxButton.OK, MessageBoxImage.Information);
                                LoadWagons();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void ManageSeats_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                int wagonId = Convert.ToInt32(button.Tag);
                NavigationService.Navigate(new AdminPages.SeatsManagementPage(wagonId)); // ← исправлено
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}