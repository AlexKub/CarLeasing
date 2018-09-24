using System;

namespace CarLeasingViewer.Log
{
    /// <summary>
    /// Пропуск текущего члена при логировании
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class LogIgnoreAttribute : Attribute { }
}
