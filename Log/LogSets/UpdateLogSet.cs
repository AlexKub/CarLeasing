using System;


namespace CarLeasingViewer.LogSets
{
    /// <summary>
    /// Набор для логирования обновления
    /// </summary>
    public class UpdateLogSet : LogSet
    {
        public UpdateLogSet()
        {
            AddParameter(new LogParameter("UserName", Environment.UserName));
            AddParameter(new LogParameter("MachineName", Environment.MachineName));
            AddLoger(new Logers.UpdateLoger());
        }
    }
}
