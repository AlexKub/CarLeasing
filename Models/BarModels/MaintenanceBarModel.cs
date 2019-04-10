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

        public int ZIndex => 1;

        public string Text { get; private set; }

        public Controls.LeasingChartManagers.ChartBarType BarType => Controls.LeasingChartManagers.ChartBarType.Maintenance;

        public string[] ToolTipRows { get; private set; }

        public string Comment { get; private set; }

        #region ITitled

        public string Title => Comment;

        public int VisibleDaysCount { get; private set; }

        #endregion

        public MaintenanceBarModel(LeasingSet set, ItemInfo item)
        {
            Set = set;
            Period = item.Maintenance;
            Comment = item.Maintenance.Description;

            SetTooolTip(item);
        }
        private MaintenanceBarModel()
        {

        }

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

        void SetVisibleCount()
        {
            //определение длины полоски аренды
            //для расчёта сколько букв поместится

                var lMonthes = Set?.Monthes;
                if (lMonthes != null && lMonthes.Count > 1)
                {
                    var setMonthes = Set?.Monthes;
                    if (setMonthes != null && setMonthes.Count > 0)
                    {
                        var firstVisibleMonth = setMonthes.FirstOrDefault()?.Month;

                        if (firstVisibleMonth != null)
                            if (Period.DateStart.GetMonth() < firstVisibleMonth)
                            {
                                //считаем видимую часть арены для случая, когда аренда началась в прошлом
                                //например, выборка с февраля по март, а текущая машина арендована с января(!) по февраль
                                //реальный срок аренды: 2 мес. (январь февраль); 
                                //видимый срок аренды: 1 мес. (февраль) <-- интересует это, т.к. параметр используется при расчёте отрисовки текста на полосках
                                //видимый срок аренды - длина полоски, которую видит пользователь (сколько букв поместится).
                                VisibleDaysCount = (Period.DateEnd.Date - firstVisibleMonth.FirstDate).Days + 1;

                                //возвращаемся, чтобы не сбросить полученное значение
                                return;
                            }
                    }
                }
            

            VisibleDaysCount = Period.DaysCount();
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
