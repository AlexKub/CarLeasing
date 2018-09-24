namespace CarLeasingViewer
{
    /// <summary>
    /// Набор свойств для логирования
    /// </summary>
    public interface ILogParametersCollection
    {
        /// <summary>
        /// Получение коллекции параметров для логирования
        /// </summary>
        /// <returns>Коллекция параметров для логирования</returns>
        ILogParameter[] GetParamters();
    }
}
