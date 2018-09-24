using System;
using System.IO;

namespace CarLeasingViewer
{
    /// <summary>
    /// Логирование в отдельный каталог для текущего авторизованного пользователя
    /// </summary>
    public class UserFileLoger : FileLoger
    {
        /// <summary>
        /// папка логирования сообщений текущего пользователя
        /// </summary>
        public static readonly string UserLogFolder = Path.Combine(DefaultLogFolder, Environment.UserName); //добавляем отдельный каталог для каждого пользователя

        public UserFileLoger(string fileName = null, string pathName = null) : base(fileName, pathName)
        {
            LogFolder = UserLogFolder;

            InitFolder(LogFolder);
        }
    }
}
