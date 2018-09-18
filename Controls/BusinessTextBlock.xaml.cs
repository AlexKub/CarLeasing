using CarLeasingViewer.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace CarLeasingViewer.Controls
{
    /// <summary>
    /// Interaction logic for BusinessTextBlock.xaml
    /// </summary>
    public partial class BusinessTextBlock : TextBlock
    {
        static LogSet m_loger = LogManager.GetDefaultLogSet<BusinessTextBlock>();

        public static readonly DependencyProperty dp_ColumnWidthProperty = DependencyProperty.Register(nameof(ColumnWidth), typeof(double), typeof(BusinessTextBlock)
            , new FrameworkPropertyMetadata()
            {
                DefaultValue = default(double),
                PropertyChangedCallback = (s, e) => (s as BusinessTextBlock).OnWidthChanged((double)e.NewValue)
            });
        public double ColumnWidth { get { return (double)GetValue(dp_ColumnWidthProperty); } set { SetValue(dp_ColumnWidthProperty, value); } }

        public static readonly DependencyProperty dp_BusinessProperty = DependencyProperty.Register(nameof(Business), typeof(Business), typeof(BusinessTextBlock)
            , new FrameworkPropertyMetadata()
            {
                DefaultValue = default(Business),
                PropertyChangedCallback = (s, e) => (s as BusinessTextBlock).OnBuisenessSetted((Business)e.NewValue)
            });
        public Business Business { get { return (Business)GetValue(dp_BusinessProperty); } set { SetValue(dp_BusinessProperty, value); } }

        public BusinessTextBlock()
        {
            InitializeComponent();
        }

        void OnWidthChanged(double newWidth)
        {
            //где newWidth - ширина колонки дня
            
            if (newWidth <= 0d)
                return;

            Business b = Business;

            if (b == null)
                return;

            #region расчёт отступа слева

            var offsetLeft = 0d;
            var startDay = b.DateStart.Day;

            //если начало в текущем месяце
            if (b.MonthCount == 1 || b.DateStart.Month == b.CurrentMonth.Index)
            {
                if (startDay > 1)
                {
                    //т.к. дня нумеруются с единицы, то для первого дня отступ будет 0 дней, для второго - 1 и т.д.
                    var dayOffsetCount = startDay - 1;
                    offsetLeft = dayOffsetCount * (newWidth) + (dayOffsetCount * 1); //1 - ширина границ у колонок
                }
            }
            //для продолжения / сквозного месяца смещения нет

            //изменяем отступ элемента от левого края
            if (offsetLeft > 0)
                if (offsetLeft != Margin.Left)
                    Margin = new Thickness(offsetLeft, Margin.Top, Margin.Right, Margin.Bottom);

            #endregion

            #region Расчёт ширины TextBlock'a

            var width = 0d;

            var dayCount = 1; //прибавляем единичку, так как при сложении/вычитании теряем день

            //если машину взяли/вернули в течении 1 месяца
            if (b.MonthCount == 1)
                dayCount += (b.DateEnd - b.DateStart).Days;

            //если машина занята несколько месяцев
            else
            {
                var currentMonth = b.CurrentMonth.Index;

                //для месяца, в котором начали съём
                if (b.DateStart.Month == currentMonth)
                    //отсчитываем от конца начального месяца
                    dayCount += (b.CurrentMonth.DayCount - b.DateStart.Day);

                //для месяца в котором закончили съём
                else if (b.DateEnd.Month == currentMonth)
                    dayCount = b.DateEnd.Day; //индекс дня - количество дней от начала месяца

                //если период начинается и заканчивается за пределами текущего месяца
                else
                {
                    //берём первую дату месяца
                    var curentDate = b.CurrentMonth[1];
                    //если 'начало' < 'текущая дата' < 'конец'
                    dayCount = ((b.DateStart < curentDate) && (curentDate < b.DateEnd))
                        ? b.CurrentMonth.DayCount //берём количество дней в текущем месяце (закрашиваем всё)
                        : 0; //0 - хз чего ещё делать. В этом месяце занятости не было, хз как сюда попало
                }
            }

            if (dayCount < 0)
            {
                m_loger.Log("Получен отрицательный период аренды. Значение сброшено в 0", MessageType.Debug
                    , new LogParameter("Съёмщик", b.Title)
                    , new LogParameter("Комментарий", b.Comment)
                    , new LogParameter("Дата начала", b.DateStart.ToShortDateString())
                    , new LogParameter("Дата окончания", b.DateEnd.ToShortDateString()));

                dayCount = 0;
            }

            width = (newWidth * dayCount) + dayCount; //прибавляем количество дней, т.к. ширина границ - 1

            Width = width;
            b.Width = width.ToString() + " | " + ActualWidth.ToString();

            #endregion
        }

        void OnBuisenessSetted(Business business)
        {
            if (business == null)
                return;

            Text = business.Title;

            OnWidthChanged(ColumnWidth);
        }

    }
}
