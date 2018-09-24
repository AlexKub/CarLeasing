using System;
using System.Windows.Controls;

namespace CarLeasingViewer
{
    /// <summary>
    /// Общая логика при управлении Canvas
    /// </summary>
    public abstract class CanvasManager : IDisposable
    {

        /// <summary>
        /// Canvas для отрисовки
        /// </summary>
        protected Canvas Canvas { get; private set; }

        /// <summary>
        /// Текущая длина Canvas
        /// </summary>
        protected double CanvasWidth => Canvas.ActualWidth;

        /// <summary>
        /// Текущая высота Canvas
        /// </summary>
        protected double CanvasHeight => Canvas.ActualHeight;

        public CanvasManager(Canvas canvas)
        {
            if (canvas == null)
                throw new ArgumentNullException("Менеджеру сетки не передан Canvas");

            Canvas = canvas;

            Subscribe(true);
        }

        protected virtual void Subscribe(bool subscribe)
        {
            if (subscribe)
            {
                if (Canvas != null)
                {
                    Canvas.SizeChanged += M_canvas_SizeChanged;
                }
            }
            else
            {
                if (Canvas != null)
                {
                    Canvas.SizeChanged -= M_canvas_SizeChanged;
                }
            }
        }

        public virtual void Dispose()
        {
            Subscribe(false);

            //чистим канвас в последнюю очередь, используется при очистке
            if (Canvas != null)
                Canvas = null;
        }


        protected virtual void M_canvas_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {

        }

        /// <summary>
        /// Индексы приоритета отрисовки контролов на гриде
        /// </summary>
        protected static class Z_Indexes
        {
            /// <summary>
            /// Колонки (самый низкий приоритет)
            /// </summary>
            public const int ColumnIndex = 0;
            /// <summary>
            /// Строки
            /// </summary>
            public const int RowIndex = 2;
            /// <summary>
            /// Полоски (самый высокий приоритет)
            /// </summary>
            public const int BarIndex = 3;

        }

    }
}
