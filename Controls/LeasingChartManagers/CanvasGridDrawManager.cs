using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CarLeasingViewer.Controls.LeasingChartManagers
{
    /// <summary>
    /// Управление отрисовкой сетки на Canvas
    /// </summary>
    public class CanvasGridDrawManager : CanvasDrawManager
    {
        const double LineWidth = 1d;
        const double LineBoldWidth = 2d;
        double m_HalfPenWidth;

        SortedDictionary<int, LineData> m_rowsData = new SortedDictionary<int, LineData>();
        SortedDictionary<int, LineData> m_columnsData = new SortedDictionary<int, LineData>();

        Pen m_pen;
        Pen m_BoldPen;

        Brush m_lineBrush;
        /// <summary>
        /// Кисть для линий
        /// </summary>
        public Brush LineBrush
        {
            get { return m_lineBrush; }
            set
            {
                m_lineBrush = value;

                m_pen = new Pen();
                m_pen.Brush = m_lineBrush;
                m_pen.Thickness = LineWidth;
                m_pen.Freeze();

                m_HalfPenWidth = m_pen.Thickness / 2;
            }
        }

        Brush m_lineBoldBrush;
        /// <summary>
        /// Кисть для толстых линий
        /// </summary>
        public Brush LineBoldBrush
        {
            get { return m_lineBoldBrush; }
            set
            {
                m_lineBoldBrush = value;

                m_BoldPen = new Pen();
                m_BoldPen.Brush = m_lineBoldBrush;
                m_BoldPen.Thickness = LineBoldWidth;
                m_BoldPen.Freeze();
            }
        }

        public double RowHeight { get; set; }

        public double ColumnWidth { get; set; }

        /// <summary>
        /// Отрисовка строки
        /// </summary>
        /// <param name="index">Индекс строки</param>
        /// <param name="height">Высота строки</param>
        public DrawingVisual DrawRow(int index, double height)
        {
            LineData rData = null;
            DrawingVisual dv = null;

            if (m_rowsData.ContainsKey(index))
                rData = m_rowsData[index];
            else
            {
                rData = new LineData(this) { Index = index, Offset = height };
                m_rowsData.Add(index, rData);
            }

            if (height > 0d && Canvas.ActualWidth > 0) //если высота нулевая - нет смысла рисовать. Она всёравно сольётся с другой
            {
                if (rData.Drawed)
                {
                    //если изменилась высота уже отрисованной строки
                    if (!rData.EqualsHeight(height))
                    {
                        rData.Offset = height;

                        //проставляем высоты у всех следующих строк
                        var level = height; //общая высота текущей строки
                                            //расчитываем отдельно, чтобы в следующем цикле пользоваться константой
                        for (int i = 0; i < index; i++)
                        {
                            level += m_rowsData[i].Offset;
                        }

                        //проставляем дельту новой высоты для всех последующих отрисованных строк
                        for (int i = (index + 1); i < m_rowsData.Count; i++)
                        {
                            var d = m_rowsData[i];
                            if (d.Drawed)
                            {
                                level += d.Offset;
                                d.Line.Y1 = level;
                                d.Line.Y2 = level;
                            }
                        }
                    }
                }
                else //отрисовка линий на Canvas
                {
                    if (index == 0)
                    {
                        dv = DrawRow(height, rData); //первую строку просто рисуем
                    }
                    else
                    {
                        //суммируем высоты всех предидущих строк
                        for (int i = 0; i < index; i++)
                        {
                            height += m_rowsData[i].Offset;
                        }

                        dv = DrawRow(height, rData);
                    }
                }
            }

            return dv;
        }

        /// <summary>
        /// Отрисовка строки по указанному индексу
        /// </summary>
        /// <param name="index">Индекс строки</param>
        public DrawingVisual DrawRow(int index)
        {
            return DrawRow(index, RowHeight);
        }

        DrawingVisual DrawRow(double offset, LineData data)
        {
            data.Line = new Line();

            GuidelineSet guideSet = new GuidelineSet();
            guideSet.GuidelinesX.Add(0d + m_HalfPenWidth);
            guideSet.GuidelinesX.Add(Canvas.ActualWidth + m_HalfPenWidth);
            guideSet.GuidelinesY.Add(offset + m_HalfPenWidth);
            guideSet.GuidelinesY.Add(offset + m_HalfPenWidth);


            var dv = data.Visual == null ? new DrawingVisual() : data.Visual;
            var dc = dv.RenderOpen();

            dc.PushGuidelineSet(guideSet);
            dc.DrawLine(m_pen, new System.Windows.Point(0d, offset), new System.Windows.Point(Canvas.ActualWidth, offset));
            dc.Pop();
            dc.Close();

            return dv;
        }

        /// <summary>
        /// Удаление линии строки с Canvas
        /// </summary>
        /// <param name="index">Индекс линии</param>
        public void RemoveRow(int index)
        {
            //просто так элементы из коллекции не удаляем
            //чтобы не тратить время на сортировку при вставке строки с тем же индексом впоследствии
            if (m_rowsData.ContainsKey(index))
            {
                m_rowsData[index].Clear();
            }
        }

        public DrawingVisual DrawColumn(int index, int rowsCount, bool bold = false)
        {
            DrawingVisual dv = null;
            if (index > 0)
            {
                var offset = ColumnWidth * index;

                if (bold)
                    offset += 1d;

                LineData ld = null;

                if (m_columnsData.ContainsKey(index))
                    ld = m_columnsData[index];
                else
                {
                    ld = new LineData(this) { Index = index };
                    m_columnsData.Add(index, ld);
                }

                ld.Offset = offset;

                if (offset > 0d && Canvas.ActualHeight > 0d)
                {
                    ld.Line = new Line();
                    dv = DrawColumn(offset, ld, rowsCount, bold);
                    ld.Visual = dv;
                }
            }

            return dv;
        }

        DrawingVisual DrawColumn(double offset, LineData ld, int rowsCount, bool bold = false)
        {
            var canvas_height = rowsCount * RowHeight;

            //SnapToDevisePixels. See https://www.wpftutorial.net/DrawOnPhysicalDevicePixels.html
            var guideSet = new GuidelineSet();

            var pen_width = bold ? m_HalfPenWidth * 2 : m_HalfPenWidth;

            guideSet.GuidelinesX.Add(offset + pen_width);
            guideSet.GuidelinesX.Add(offset + pen_width);
            guideSet.GuidelinesY.Add(0d + pen_width);
            guideSet.GuidelinesY.Add(canvas_height + pen_width);

            var drawingVisual = ld.Visual == null ? new DrawingVisual() : ld.Visual;
            var drawingContext = drawingVisual.RenderOpen();

            var pen = bold ? m_BoldPen : m_pen;
            drawingContext.PushGuidelineSet(guideSet);
            drawingContext.DrawLine(pen, new System.Windows.Point(offset, 0d), new System.Windows.Point(offset, canvas_height));
            drawingContext.Pop();
            drawingContext.Close();

            return drawingVisual;
        }

        public CanvasGridDrawManager(LeasingChart canvas) : base(canvas) { }

        public override void Dispose()
        {
            Clear();

            base.Dispose();
        }

        void ClearCollection(IDictionary<int, LineData> collection)
        {
            if (collection == null)
                return;

            foreach (var data in collection)
            {
                if (data.Value != null)
                {
                    if (data.Value.Visual != null)
                        Canvas.Remove(data.Value.Visual);

                    data.Value.Clear();
                }
            }

            collection.Clear();
        }

        public void Clear()
        {
            ClearCollection(m_rowsData);

            ClearCollection(m_columnsData);
        }

        protected override void M_canvas_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            //if (e.HeightChanged)
            //{
            //    var m_cHeight = e.NewSize.Height;
            //
            //    if (m_cHeight > 0d)
            //    {
            //        foreach (var ld in m_columnsData.Values)
            //        {
            //            if (ld.Drawed)
            //                ld.Line.Y2 = m_cHeight;
            //        }
            //    }
            //}
            //if (e.WidthChanged)
            //{
            //    var m_cWidth = e.NewSize.Width;
            //
            //    if (m_cWidth > 0d)
            //    {
            //        var height = 0d;
            //
            //        foreach (var rd in m_rowsData.Values)
            //        {
            //            height += RowHeight;
            //            if (rd.Drawed)
            //                rd.Line.X2 = m_cWidth;
            //            else
            //            {
            //                rd.Offset = height;
            //            }
            //        }
            //    }
            //}
        }

        #region RowData

        /// <summary>
        /// Динамические данные для отрисовки строки
        /// </summary>
        class LineData
        {
            CanvasGridDrawManager m_manager;
            /// <summary>
            /// Индекс строки на графике
            /// </summary>
            public int Index { get; set; }
            /// <summary>
            /// Актуальная высота одного из контролов на строек
            /// </summary>
            public double Offset { get; set; }
            /// <summary>
            /// Отрисованная линия
            /// </summary>
            public Line Line { get; set; }

            /// <summary>
            /// Флаг отрисовки линии
            /// </summary>
            public bool Drawed { get { return Line != null; } }

            /// <summary>
            /// Обёртка рисования для элемента
            /// </summary>
            public DrawingVisual Visual { get; set; }

            /// <summary>
            /// Сравнение высот строк
            /// </summary>
            /// <param name="height"></param>
            /// <returns></returns>
            public bool EqualsHeight(double height)
            {
                return height == Offset;
            }

            public LineData(CanvasGridDrawManager manager)
            {
                m_manager = manager;
            }

            /// <summary>
            /// Удаление линии
            /// </summary>
            public void Clear()
            {
                if (Drawed)
                {
                    //m_manager.Canvas.Children.Remove(Line);
                    Line = null;
                }
                Offset = 0d;

                Visual = null;
            }
        }

        #endregion
    }
}
