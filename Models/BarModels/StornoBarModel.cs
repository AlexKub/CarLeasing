using CarLeasingViewer.Controls.LeasingChartManagers;
using CarLeasingViewer.Interfaces;
using System.Collections.Generic;

namespace CarLeasingViewer.Models
{
    /// <summary>
    /// Модель плашки Сторно
    /// </summary>
    public class StornoBarModel : IDrawableBar
    {
        public int RowIndex { get; set; }

        public int ZIndex => 1;

        public IPeriod Period { get; private set; }

        public LeasingSet Set { get; private set; }

        public string[] ToolTipRows { get; private set; }

        public ChartBarType BarType => ChartBarType.Storno;

        public StornoBarModel(LeasingSet set, Storno s)
        {
            Set = set;
            Period = s;
            var rows = new List<string>() {
                "СТОРНО"
                , "от " + s.DocumentDate.ToString()
                , Period.TooltipRow()
            };

            if (!string.IsNullOrEmpty(s.Comment))
                rows.Add(s.Comment);

            ToolTipRows = rows.ToArray();
        }

        private StornoBarModel() { }

        public IDrawableBar Clone()
        {
            var clone = new StornoBarModel();
            clone.RowIndex = RowIndex;
            clone.Period = Period;
            clone.Set = Set;
            clone.ToolTipRows = ToolTipRows;

            return clone;
        }
    }
}
