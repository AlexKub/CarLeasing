using System;
using System.Drawing.Imaging;
using System.IO;
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
        public static BitmapImage MaintenanceIconPath { get; private set; }

        /// <summary>
        /// Иконка страховки
        /// </summary>
        public static BitmapImage InsuranceIconPath { get; private set; }

        /// <summary>
        /// Иконка страховки на грфике
        /// </summary>
        public static BitmapImage InsuranceDay { get; private set; }

        static IconsInfo()
        {
            MaintenanceIconPath = LoadImage(Properties.Resources.maintenance_image);
            InsuranceIconPath = LoadImage(Properties.Resources.Insurance_left);
            InsuranceDay = LoadImage(Properties.Resources.Insurance_day);
        }

        public static BitmapImage LoadImage(System.Drawing.Bitmap bitmap)
        {
            //https://docs.microsoft.com/ru-ru/dotnet/framework/wpf/graphics-multimedia/how-to-use-a-bitmapimage
            try
            {
                var bitmapImage = new BitmapImage();
                using (MemoryStream memory = new MemoryStream())
                {
                    bitmap.Save(memory, ImageFormat.Png);
                    memory.Position = 0;
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                }

                return bitmapImage;
            }
            catch
            {

                return null;
            }
        }

    }
}
