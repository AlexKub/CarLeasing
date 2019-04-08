using CarLeasingViewer.Controls.LeasingChartManagers;
using CarLeasingViewer.Interfaces;
using System;

namespace CarLeasingViewer.Models
{
    /// <summary>
    /// Иконка
    /// </summary>
    public class ImageBarModel : IDrawableBar
    {
        public int RowIndex { get; set; }

        public IPeriod Period { get; private set; }

        DateTime m_date;
        public DateTime Date { get { return m_date; } set { m_date = value; Period = new Period(value, value); } }

        public LeasingSet Set { get; private set; }

        public System.Drawing.Bitmap Bitmap { get; set; }

        public ImageBarModel(LeasingSet set)
        {
            Set = set;
        }

        public string[] ToolTipRows { get; set; }

        public ChartBarType BarType => ChartBarType.Insurance;

        public IDrawableBar Clone()
        {
            var newModel = new ImageBarModel(Set);
            newModel.RowIndex = RowIndex;
            newModel.Date = Date;
            newModel.ToolTipRows = ToolTipRows;

            return newModel;
        }
    }
}
