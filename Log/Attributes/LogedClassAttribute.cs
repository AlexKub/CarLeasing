using System;
using System.Reflection;

namespace CarLeasingViewer.Log
{
    /// <summary>
    /// Флаг для логирования всех свойств класса
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class LogedClassAttribute : Attribute
    {
        /// <summary>
        /// Флаги для поиска логируемых членов класса
        /// </summary>
        public BindingFlags LogMembersSearch { get; private set; }

        public MemberTypes LogMemberTypes { get; private set; }

        public LogedClassAttribute(BindingFlags memberFlags = BindingFlags.Public | BindingFlags.Instance,
            MemberTypes memberTypes = MemberTypes.Property)
        {
            LogMembersSearch = memberFlags;
            LogMemberTypes = memberTypes;
        }
    }
}
