namespace CarLeasingViewer
{
    /// <summary>
    /// Видимая область графика
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{DebugDisplay()}")]
    public class VisibleArea
    {
        private double m_ChartWith;

        /// <summary>
        /// Актуальная ширина графика
        /// </summary>
        public double ChartWith
        {
            get { return m_ChartWith; }
            set
            {
                if (m_ChartWith != value)
                {
                    m_ChartWith = value;
                    Width = value + HorisontalScrollOffset;
                }
            }
        }

        private double m_ChartHeight;
        /// <summary>
        /// Акутальная высота графика
        /// </summary>
        public double ChartHeight
        {
            get { return m_ChartHeight; }
            set
            {
                if (value != m_ChartHeight)
                {
                    m_ChartHeight = value;
                    Height = m_VerticalScrollOffset + value;
                }
            }
        }

        private double m_HorisontalScrollOffset;
        /// <summary>
        /// Смещение скролла по горизонтали
        /// </summary>
        public double HorisontalScrollOffset
        {
            get { return m_HorisontalScrollOffset; }
            set
            {
                if (value != m_HorisontalScrollOffset)
                {
                    m_HorisontalScrollOffset = value;
                    Width = m_ChartWith + value;
                }
            }
        }

        private double m_VerticalScrollOffset;
        /// <summary>
        /// Смещение скролла по вертикали
        /// </summary>
        public double VerticalScrollOffset
        {
            get { return m_VerticalScrollOffset; }
            set
            {
                if (m_VerticalScrollOffset != value)
                {
                    m_VerticalScrollOffset = value;
                    Height = value + ChartHeight;
                }
            }
        }

        /// <summary>
        /// Высота
        /// </summary>
        public double Height { get; private set; }
        /// <summary>
        /// Ширина
        /// </summary>
        public double Width { get; private set; }

        string DebugDisplay()
        {
            return Height.ToString() + " | " + Width.ToString();
        }
    }
}
