namespace CarLeasingViewer
{
    /// <summary>
    /// Параметр для логирования с указанием разделителя
    /// </summary>
    public interface ILogParamWithDelimiter : ILogParameter
    {
        /// <summary>
        /// Разделитель между именем и значением параметра
        /// </summary>
        string Delimiter { get; }
    }
}
