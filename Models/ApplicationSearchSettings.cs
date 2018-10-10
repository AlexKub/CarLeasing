using System.Runtime.CompilerServices;

namespace CarLeasingViewer.Models
{

    /// <summary>
    /// Общие настройки поиска приложения
    /// </summary>
    public class ApplicationSearchSettings : SearchSettings
    {
        static ApplicationSearchSettings m_instance;

        public static ApplicationSearchSettings Instance
        {
            get
            {
                if (m_instance == null)
                    m_instance = new ApplicationSearchSettings();

                return m_instance;
            }
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            SaveSettings();

            var vm = App.GetMainWindow().DataContext as ViewModels.LeasingViewViewModel;

            if (vm != null)
                vm.Update();
        }

        public ApplicationSearchSettings()
        {
            LoadSettings();
        }

        void LoadSettings()
        {
            _SelectedDBSearchType = (DBSearchType)Properties.Settings.Default.SearchTypeIndex;
            _TestData = Properties.Settings.Default.TestData;
            _IncludeBlocked = Properties.Settings.Default.IncludeBlocked;

            //не вызываем OnPropertyChanged лишний раз, т.к. вызовет перерисовку UI
        }

        void SaveSettings()
        {
            Properties.Settings.Default.SearchTypeIndex = (int)_SelectedDBSearchType;
            Properties.Settings.Default.TestData = _TestData;
            Properties.Settings.Default.IncludeBlocked = _IncludeBlocked;

            Properties.Settings.Default.Save();
        }
    }
}
