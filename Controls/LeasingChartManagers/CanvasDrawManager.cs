using System;

namespace CarLeasingViewer.Controls.LeasingChartManagers
{
    /// <summary>
    /// Общая логика при управлении Canvas
    /// </summary>
    public abstract class CanvasDrawManager : IDisposable
        {
            /// <summary>
            /// ЧСВ-counter элементов
            /// </summary>
            public int DrawCounter { get; protected set; }

            /// <summary>
            /// Порядок отрисовки элементов
            /// </summary>
            public int DrawingOrder { get; set; }

            /// <summary>
            /// Canvas для отрисовки
            /// </summary>
            protected LeasingChart Canvas { get; private set; }

            public CanvasDrawManager(LeasingChart canvas)
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
                public const int RowIndex = 0;
                /// <summary>
                /// Полоски (самый высокий приоритет)
                /// </summary>
                public const int BarIndex = 1;

            }

        
    }
}
