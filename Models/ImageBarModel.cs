using CarLeasingViewer.Controls.LeasingChartManagers;
using CarLeasingViewer.Interfaces;
using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

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

        public BitmapImage Bitmap { get; set; }

        public int ZIndex => 99; //последний

        public ImageBarModel(LeasingSet set)
        {
            Set = set;
        }

        public string[] ToolTipRows { get; private set; }

        public ChartBarType BarType => ChartBarType.Insurance;

        public IDrawableBar Clone()
        {
            var newModel = new ImageBarModel(Set);
            newModel.RowIndex = RowIndex;
            newModel.Date = Date;
            newModel.ToolTipRows = ToolTipRows;

            return newModel;
        }

        public void SetTooltip(ItemInfo item)
        {
            var rows = new List<string>();
            rows.Add(item.Name);
            rows.Add("ОКОНЧАНИЕ СТРАХОВКИ");
            if (item.OSAGO_END > Set.DateStart)
                rows.Add("ОСАГО: " + item.OSAGO_END.ToShortDateString() + (item.OSAGO_Company != null ? (" " + item.OSAGO_Company) : ""));
            if (item.KASKO_END > Set.DateStart)
                rows.Add("КАСКО: " + item.KASKO_END.ToShortDateString() + (item.KASKO_Company != null ? (" " + item.KASKO_Company) : ""));

            ToolTipRows = rows.ToArray();
        }
    }
}
