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

        public ImageBarModel(LeasingSet set) { Set = set; }

        private ImageBarModel() { } //конструктор при клонировании

        public string[] ToolTipRows { get; private set; }

        public ChartBarType BarType => ChartBarType.Insurance;

        public IDrawableBar Clone()
        {
            var clone = new ImageBarModel();
            clone.RowIndex = RowIndex;
            clone.Period = Period;
            clone.Date = Date;
            clone.Set = Set;
            clone.Bitmap = Bitmap;
            clone.ToolTipRows = ToolTipRows;

            return clone;
        }

        public void SetTooltip(ItemInfo item)
        {
            var rows = new List<string>();
            rows.Add(item.Name);
            rows.Add("ОКОНЧАНИЕ СТРАХОВКИ");

            var setPeriod = Set as IPeriod;
            var dateStart = setPeriod.DateStart;
            if (item.OSAGO_END > dateStart)
                rows.Add("ОСАГО: " + item.OSAGO_END.ToShortDateString() + (item.OSAGO_Company != null ? (" " + item.OSAGO_Company) : ""));
            if (item.KASKO_END > dateStart)
                rows.Add("КАСКО: " + item.KASKO_END.ToShortDateString() + (item.KASKO_Company != null ? (" " + item.KASKO_Company) : ""));

            ToolTipRows = rows.ToArray();
        }
    }
}
