namespace CarLeasingViewer.Models
{
    /// <summary>
    /// Регион
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{DebugDisplay()}")]
    public class Region
    {
        static Region m_total;
        /// <summary>
        /// Общий регион
        /// </summary>
        public static Region Total
        {
            get
            {
                if(m_total == null)
                {
                    m_total = new Region();
                    m_total.Address = string.Empty;
                    m_total.DBKey = string.Empty;
                    m_total.PostCode = string.Empty;
                    m_total.DisplayName = "Все";
                    m_total.IsTotal = true;
                }

                return m_total;
            }
        }

        public string DisplayName { get; set; }

        public string DBKey { get; set; }

        public string Address { get; set; }

        public string PostCode { get; set; }

        public bool IsTotal { get; private set; }

        string DebugDisplay()
        {
            return DBKey + " | " + DisplayName;
        }
    }
}
