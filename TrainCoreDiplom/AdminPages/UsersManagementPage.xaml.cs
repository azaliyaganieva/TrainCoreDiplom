using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TrainCoreDiplom.DBConnection;
using TrainCoreDiplom.AdminWindows; // Добавь это!

namespace TrainCoreDiplom.AdminPages
{
    public partial class UsersManagementPage : Page
    {
        public class UserDisplay
        {
            public int ID_User { get; set; }
            public string Login { get; set; }
            public string Email { get; set; }
            public string RoleName { get; set; }
            public int RoleValue { get; set; }
            public string LastLogin { get; set; }
            public bool IsActive { get; set; }
            public string CreatedAt { get; set; }
        }

        public UsersManagementPage()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers()
        {
            try
            {
                using (var db = new TrainCoreDiplomEntities1())
                {
                    var usersList = db.Users.OrderBy(u => u.ID_User).ToList();
                    var users = new List<UserDisplay>();

                    foreach (var u in usersList)
                    {
                        users.Add(new UserDisplay
                        {
                            ID_User = u.ID_User,
                            Login = u.Login,
                            Email = u.Email,
                            RoleName = GetRoleName(u.Role), // u.Role - это int, не nullable
                            RoleValue = u.Role, // просто присваиваем
                            LastLogin = u.LastLogin.HasValue ? u.LastLogin.Value.ToString("dd.MM.yyyy HH:mm") : "никогда",
                            IsActive = u.IsActive ?? true, // IsActive nullable? проверь в модели
                            CreatedAt = u.CreatedAt.HasValue ? u.CreatedAt.Value.ToString("dd.MM.yyyy") : ""
                        });
                    }

                    UsersGrid.ItemsSource = users;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetRoleName(int role) // role - просто int
        {
            switch (role)
            {
                case 1: return "Администратор";
                case 2: return "Менеджер";
                case 3: return "Пользователь";
                default: return "Неизвестно";
            }
        }

        private void FilterChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var comboBox = sender as ComboBox;
                if (comboBox == null) return;

                using (var db = new TrainCoreDiplomEntities1())
                {
                    var usersList = db.Users.ToList();

                    if (comboBox.SelectedIndex > 0)
                    {
                        int roleFilter = comboBox.SelectedIndex;
                        usersList = usersList.Where(u => u.Role == roleFilter).ToList(); // прямое сравнение
                    }

                    var users = new List<UserDisplay>();
                    foreach (var u in usersList)
                    {
                        users.Add(new UserDisplay
                        {
                            ID_User = u.ID_User,
                            Login = u.Login,
                            Email = u.Email,
                            RoleName = GetRoleName(u.Role),
                            RoleValue = u.Role,
                            LastLogin = u.LastLogin.HasValue ? u.LastLogin.Value.ToString("dd.MM.yyyy HH:mm") : "никогда",
                            IsActive = u.IsActive ?? true,
                            CreatedAt = u.CreatedAt.HasValue ? u.CreatedAt.Value.ToString("dd.MM.yyyy") : ""
                        });
                    }

                    UsersGrid.ItemsSource = users;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка фильтрации: {ex.Message}");
            }
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new UserEditWindow(null);
            if (dialog.ShowDialog() == true)
            {
                LoadUsers();
            }
        }

        private void EditUser_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                int userId = Convert.ToInt32(button.Tag);
                using (var db = new TrainCoreDiplomEntities1())
                {
                    var user = db.Users.Find(userId);
                    if (user != null)
                    {
                        var dialog = new UserEditWindow(user);
                        if (dialog.ShowDialog() == true)
                        {
                            LoadUsers();
                        }
                    }
                }
            }
        }

        private void ToggleUserStatus_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                int userId = Convert.ToInt32(button.Tag);

                var result = MessageBox.Show("Изменить статус пользователя?",
                                           "Подтверждение",
                                           MessageBoxButton.YesNo,
                                           MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var db = new TrainCoreDiplomEntities1())
                        {
                            var user = db.Users.Find(userId);
                            if (user != null)
                            {
                                user.IsActive = !(user.IsActive ?? true);
                                db.SaveChanges();

                                MessageBox.Show("Статус пользователя изменен", "Успех",
                                              MessageBoxButton.OK, MessageBoxImage.Information);
                                LoadUsers();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void ChangeRole_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                int userId = Convert.ToInt32(button.Tag);
                var dialog = new RoleChangeWindow(userId);
                if (dialog.ShowDialog() == true)
                {
                    LoadUsers();
                }
            }
        }

        private void ViewUserHistory_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                int userId = Convert.ToInt32(button.Tag);
                NavigationService.Navigate(new UserHistoryPage(userId));
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}