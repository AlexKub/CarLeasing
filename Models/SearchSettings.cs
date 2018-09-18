using System;
using System.Collections.Generic;
using System.Linq;

namespace CarLeasingViewer.Models
{
    /// <summary>
    /// Настройки поиска
    /// </summary>
    public class SearchSettings : ViewModels.ViewModelBase
    {
        public static ApplicationSearchSettings GlobalSettings { get { return ApplicationSearchSettings.Instance; } }

        public IEnumerable<DBSearchType> DBSearchTypes { get { return Enum.GetValues(typeof(DBSearchType)).OfType<DBSearchType>(); } }

        protected DBSearchType _SelectedDBSearchType;
        /// <summary>
        /// Возвращает или задаёт Текущий тип поиска по БД
        /// </summary>
        public DBSearchType SelectedDBSearchType { get { return _SelectedDBSearchType; } set { _SelectedDBSearchType = value; OnPropertyChanged(); } }

        protected bool _IncludeBlocked;

        /// <summary>
        /// Возвращает или задаёт флаг поиска по заблокированным
        /// </summary>
        public bool IncludeBlocked { get { return _IncludeBlocked; } set { _IncludeBlocked = value; OnPropertyChanged(); } }

        protected bool _TestData;
        /// <summary>
        /// Возвращает или задаёт Флаг использования случайных данных
        /// </summary>
        public bool TestData { get { return _TestData; } set { _TestData = value; OnPropertyChanged(); } }


        public SearchSettings() { }

        public SearchSettings(SearchSettings settings)
        {
            _SelectedDBSearchType = settings.SelectedDBSearchType;
            _IncludeBlocked = settings.IncludeBlocked;
            _TestData = settings.TestData;
        }
    }
}
