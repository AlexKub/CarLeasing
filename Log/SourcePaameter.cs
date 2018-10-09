using System;

namespace CarLeasingViewer.Log
{
    public class SourceParameter : LogParameter
    {
        public SourceParameter(string value) : base("Source", value) { }

        public SourceParameter(Type type) : this(GetTypeName(type)) { }

        public SourceParameter(object obj) : this(obj == null ? "NULL" : GetTypeName(obj.GetType())) { }

        static string GetTypeName(Type t)
        {
            return t.FullName;
        }
    }
}
