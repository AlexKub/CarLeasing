namespace CarLeasingViewer.Models
{
    /// <summary>
    /// Регион
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{DebugDisplay()}")]
    public class Region
    {
        public string DisplayName { get; set; }

        public string DBKey { get; set; }

        public string Address { get; set; }

        public string PostCode { get; set; }

        string DebugDisplay()
        {
            return DBKey + " | " + DisplayName;
        }
    }
}
