using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TrainCoreDiplom.DBConnection;

namespace TrainCoreDiplom.ManagerPages
{
    public partial class ManagerTrainsPage : Page
    {
        public ManagerTrainsPage()
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
                    var trains = db.Trains.ToList();
                    var displayList = trains.Select(t => new
                    {
                        t.Number_train,
                        t.Name_train,
                        TypeName = t.Type_Trains?.Name_type_train ?? "Не указан",
                        WagonsCount = db.Wagons.Count(w => w.ID_Train == t.ID_Train),
                        TotalSeats = db.Seats.Count(s => s.Wagons.ID_Train == t.ID_Train)
                    }).ToList();

                    TrainsGrid.ItemsSource = displayList;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки поездов: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}