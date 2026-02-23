using System;
using System.Windows;
using TrainCoreDiplom.DBConnection;

namespace TrainCoreDiplom.AdminWindows
{
    public partial class RoleChangeWindow : Window
    {
        private int _userId;
        private int _currentRole;
        private string _userLogin;

        public RoleChangeWindow(int userId)
        {
            InitializeComponent();
            _userId = userId;
            LoadUserData();
        }

        private void LoadUserData()
        {
            try
            {
                using (var db = new TrainCoreDiplomEntities1())
                {
                    var user = db.Users.Find(_userId);
                    if (user != null)
                    {
                        _userLogin = user.Login;
                        _currentRole = user.Role; // Role - это int, не nullable

                        switch (_currentRole)
                        {
                            case 1:
                                AdminRadioButton.IsChecked = true;
                                break;
                            case 2:
                                ManagerRadioButton.IsChecked = true;
                                break;
                            case 3:
                                UserRadioButton.IsChecked = true;
                                break;
                        }

                        Title = $"Смена роли - {_userLogin}";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int newRole = 3;

                if (AdminRadioButton.IsChecked == true)
                    newRole = 1;
                else if (ManagerRadioButton.IsChecked == true)
                    newRole = 2;

                if (newRole == _currentRole)
                {
                    DialogResult = false;
                    Close();
                    return;
                }

                using (var db = new TrainCoreDiplomEntities1())
                {
                    var user = db.Users.Find(_userId);
                    if (user != null)
                    {
                        user.Role = newRole;
                        db.SaveChanges();

                        string roleName = newRole == 1 ? "Администратор" :
                                         newRole == 2 ? "Менеджер" : "Пользователь";

                        MessageBox.Show($"✅ Роль пользователя {_userLogin} изменена на {roleName}",
                                      "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                        DialogResult = true;
                        Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Ошибка сохранения: {ex.Message}", "Ошибка",
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