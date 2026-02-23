using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TrainCoreDiplom.DBConnection;

namespace TrainCoreDiplom
{
    public partial class App : Application
    {
        // Текущий пользователь (доступен везде)
        public static Users CurrentUser { get; set; }
    }
}
