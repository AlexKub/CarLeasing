using System;
using System.Linq;
using CarLeasingViewer.Interfaces;

namespace CarLeasingViewer.Models
{
    /// <summary>
    /// Модель для отрисовки периода Ремонта авто на ГРафике
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{DebugDisplay()}")]
    public class MaintenanceBarModel : Interfaces.IDrawableBar, ITitledBar
    {
        public int RowIndex { get; set; }

        public IPeriod Period { get; private set; }

        public LeasingSet Set { get; private set; }

        public int ZIndex => 0;

        public string Text { get; private set; }

        public Controls.LeasingChartManagers.ChartBarType BarType => Controls.LeasingChartManagers.ChartBarType.Maintenance;

        public string[] ToolTipRows { get; private set; }

        public string Comment { get; private set; }

        public bool Visible => App.SearchSettings.IncludeNotActive;

        #region ITitled

        public string Title => Comment;

        #endregion

        public MaintenanceBarModel(LeasingSet set, ItemInfo item)
        {
            Set = set;
            Period = item.Maintenance;
            Comment = item.Maintenance.Description;
            SetTooolTip(item);
        }
        private MaintenanceBarModel() { }

        void SetTooolTip(ItemInfo item)
        {
            var period = Period.TooltipRow();

            Text = "Не активен " + period;

            ToolTipRows = new string[] {
                item.Name,
                "не активен / в ремонте",
                period,
                Comment
            };
        }

        public IDrawableBar Clone()
        {
            var copy = new MaintenanceBarModel();

            copy.Set = Set;
            copy.RowIndex = RowIndex;
            copy.Period = Period;
            copy.Text = Text;
            copy.ToolTipRows = ToolTipRows;
            copy.Comment = Comment;

            return copy;
        }
    }
}
