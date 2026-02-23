using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TrainCoreDiplom.DBConnection;
using TrainCoreDiplom.AdminWindows;

namespace TrainCoreDiplom.AdminPages
{
    public partial class TrainsManagementPage : Page
    {
        public class TrainDisplay
        {
            public int ID_Train { get; set; }
            public string Number_train { get; set; }
            public string Name_train { get; set; }
            public string TypeName { get; set; }
            public int WagonsCount { get; set; }
            public int TotalSeats { get; set; }
        }

        public TrainsManagementPage()
        {
            InitializeComponent();
            LoadTrains();
        }

        private void LoadTrains()
        {
            try
            {
                using (var db = new TrainCoreDiplomEntities1())
                {
                    var trains = db.Trains
                        .Include("Type_Trains")
                        .ToList();

                    var displayList = new List<TrainDisplay>();

                    foreach (var t in trains)
                    {
                        int wagonsCount = db.Wagons.Count(w => w.ID_Train == t.ID_Train);
                        int totalSeats = 0;

                        var wagons = db.Wagons.Where(w => w.ID_Train == t.ID_Train).ToList();
                        foreach (var w in wagons)
                        {
                            totalSeats += db.Seats.Count(s => s.ID_Wagon == w.ID_Wagon);
                        }

                        displayList.Add(new TrainDisplay
                        {
                            ID_Train = t.ID_Train,
                            Number_train = t.Number_train,
                            Name_train = t.Name_train,
                            TypeName = t.Type_Trains?.Name_type_train ?? "Не указан",
                            WagonsCount = wagonsCount,
                            TotalSeats = totalSeats
                        });
                    }

                    TrainsGrid.ItemsSource = displayList;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки поездов: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddTrain_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new TrainEditWindow(null);
            if (dialog.ShowDialog() == true)
            {
                LoadTrains();
            }
        }

        private void EditTrain_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                int trainId = Convert.ToInt32(button.Tag);
                using (var db = new TrainCoreDiplomEntities1())
                {
                    var train = db.Trains.Find(trainId);
                    if (train != null)
                    {
                        var dialog = new TrainEditWindow(train);
                        if (dialog.ShowDialog() == true)
                        {
                            LoadTrains();
                        }
                    }
                }
            }
        }

        private void DeleteTrain_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                int trainId = Convert.ToInt32(button.Tag);

                var result = MessageBox.Show("Вы уверены, что хотите удалить этот поезд?",
                                           "Подтверждение удаления",
                                           MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var db = new TrainCoreDiplomEntities1())
                        {
                            var train = db.Trains.Find(trainId);
                            if (train != null)
                            {
                                // Удаляем связанные вагоны и места
                                var wagons = db.Wagons.Where(w => w.ID_Train == trainId).ToList();
                                foreach (var wagon in wagons)
                                {
                                    var seats = db.Seats.Where(s => s.ID_Wagon == wagon.ID_Wagon).ToList();
                                    db.Seats.RemoveRange(seats);
                                }
                                db.Wagons.RemoveRange(wagons);
                                db.Trains.Remove(train);
                                db.SaveChanges();

                                MessageBox.Show("Поезд успешно удален", "Успех",
                                              MessageBoxButton.OK, MessageBoxImage.Information);
                                LoadTrains();
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

        private void ManageWagons_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                int trainId = Convert.ToInt32(button.Tag);
                NavigationService.Navigate(new WagonsManagementPage(trainId));
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadTrains();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}