using System;
using System.Windows.Media.Imaging;

namespace CarLeasingViewer
{
    public static class IconsInfo
    {

        /// <summary>
        /// Стандартный размер
        /// </summary>
        public static double StandartSize { get { return 18d; } }

        /// <summary>
        /// Иконка Ремонта
        /// </summary>
        public static BitmapImage MaintenanceIconPath { get { return LoadImage("/Resources/maintenance.png", UriKind.Relative); } }

        /// <summary>
        /// Иконка страховки
        /// </summary>
        public static BitmapImage InsuranceIconPath { get { return LoadImage("/Resources/osago_kasko.png", UriKind.Relative); } }

        static BitmapImage LoadImage(string path, UriKind pathType)
        {
            //https://docs.microsoft.com/ru-ru/dotnet/framework/wpf/graphics-multimedia/how-to-use-a-bitmapimage
            try
            {
                var image = new BitmapImage();
                image.BeginInit();

                image.UriSource = new Uri(path, pathType);

                image.DecodePixelHeight = (int)StandartSize;
                image.EndInit();

                return image;
            }
            catch (Exception ex)
            {
                App.Loger.Log("Возникло исключение при загрузке изображения", ex,
                    new LogParameter("Путь", path.LogValue())
                    , new LogParameter("Тип", pathType.ToString()));

                return null;
            }
        }

    }
}
