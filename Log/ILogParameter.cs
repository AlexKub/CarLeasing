namespace CarLeasingViewer
{
    /// <summary>
    /// Параметр для логирования
    /// </summary>
    public interface ILogParameter
    {
        /// <summary>
        /// Имя параметра
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Значение параметра
        /// </summary>
        string Value { get; }

        /// <summary>
        /// Логика строкового представления Name + Value (логика по умолчанию задана в LogMessageBuilder)
        /// </summary>
        /// <param name="sb">Экземпляр StringBuilder для объединения</param>
        /// <param name="indent">Отступ</param>
        /// <returns>Возвращает тот же экземпляр StringBuilder с добавленным параметром</returns>
        System.Text.StringBuilder AppendParameter(System.Text.StringBuilder sb, string indent = null);
    }
}
