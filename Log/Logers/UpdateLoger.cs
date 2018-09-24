using System;
using System.Collections.Generic;
using System.Text;

namespace CarLeasingViewer.Logers
{
    /// <summary>
    /// Логирование процесса обновления
    /// </summary>
    public class UpdateLoger : FileLoger
    {
        /// <summary>
        /// Имя файла логирования процесса обновления
        /// </summary>
        public static new string FileName = "UpdateLog_" + DateTime.Today.ToShortDateString() + ".txt";

        public UpdateLoger(string logPath = null) : base(FileName, logPath)
        {
            
        }
    }
}
