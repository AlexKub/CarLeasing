using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Collections.Generic;

namespace CarLeasingViewer.Controls.LeasingChartManagers
{
    /// <summary>
    /// Управление отрисовкой сетки на Canvas
    /// </summary>
    public class CanvasGridDrawManager : CanvasDrawManager
    {
        const double LineWidth = 1d;

        SortedDictionary<int, LineData> m_rowsData = new SortedDictionary<int, LineData>();
        SortedDictionary<int, LineData> m_columnsData = new SortedDictionary<int, LineData>();

        /// <summary>
        /// Кисть для линий
        /// </summary>
        public Brush LineBrush { get; set; }

        public double RowHeight { get; set; }

        public double ColumnWidth { get; set; }

        /// <summary>
        /// Отрисовка строки
        /// </summary>
        /// <param name="index">Индекс строки</param>
        /// <param name="height">Высота строки</param>
        public void DrawRow(int index, double height, DrawingContext dc)
        {
            LineData rData = null;

            if (m_rowsData.ContainsKey(index))
                rData = m_rowsData[index];
            else
            {
                rData = new LineData(this) { Index = index, Offset = height };
                m_rowsData.Add(index, rData);
            }

            if (height > 0d && CanvasWidth > 0) //если высота нулевая - нет смысла рисовать. Она всёравно сольётся с другой
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
                        rData.Line = DrawRow(height, dc); //первую строку просто рисуем
                    }
                    else
                    {
                        //суммируем высоты всех предидущих строк
                        for (int i = 0; i < index; i++)
                        {
                            height += m_rowsData[i].Offset;
                        }

                        rData.Line = DrawRow(height, dc);
                    }
                }
            }
        }

        /// <summary>
        /// Отрисовка строки по указанному индексу
        /// </summary>
        /// <param name="index">Индекс строки</param>
        public void DrawRow(int index, DrawingContext dc)
        {
            DrawRow(index, RowHeight, dc);
        }

        Line DrawRow(double offset, DrawingContext dc)
        {
            //var l = new Line();
            //l.X1 = 0;
            //l.Y1 = offset;
            //l.X2 = CanvasWidth;
            //l.Y2 = offset;
            //l.Stroke = LineBrush;
            //l.StrokeThickness = LineWidth;
            //l.SnapsToDevicePixels = true;
            //Panel.SetZIndex(l, Z_Indexes.RowIndex);
            //
            //Canvas.Children.Add(l);
            var pen = new Pen();
            pen.Brush = LineBrush;
            pen.Thickness = LineWidth;
            pen.Freeze();

            double halfPenWidth = pen.Thickness / 2;

            GuidelineSet guideSet = new GuidelineSet();
            guideSet.GuidelinesX.Add(0d + halfPenWidth);
            guideSet.GuidelinesX.Add(CanvasWidth + halfPenWidth);
            guideSet.GuidelinesY.Add(offset + halfPenWidth);
            guideSet.GuidelinesY.Add(offset + halfPenWidth);

            dc.PushGuidelineSet(guideSet);
            dc.DrawLine(pen, new System.Windows.Point(0d, offset), new System.Windows.Point(CanvasWidth, offset));
            dc.Pop();

            return new Line();
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

        public void DrawColumns(int count, DrawingContext dc)
        {
            if (count > 0d)
            {
                LineData ld = null;

                var offset = 0d;
                for (int i = 0; i < count; i++)
                {
                    offset += ColumnWidth;
                    if (m_columnsData.ContainsKey(i))
                        ld = m_columnsData[i];
                    else
                    {
                        ld = new LineData(this) { Index = i };
                        m_columnsData.Add(i, ld);
                    }

                    ld.Offset = offset;

                    if (offset > 0d && CanvasHeight > 0d && !ld.Drawed)
                    {
                        ld.Line = DrawColumn(offset, dc);
                    }
                }
            }
        }

        Line DrawColumn(double offset, DrawingContext dc)
        {
            //var l = new Line();
            //l.X1 = offset;
            //l.Y1 = 0;
            //l.X2 = offset;
            //l.Y2 = CanvasHeight;
            //l.Stroke = LineBrush;
            //l.StrokeThickness = LineWidth;
            //l.SnapsToDevicePixels = true;
            //
            //Panel.SetZIndex(l, Z_Indexes.ColumnIndex);
            //
            //Canvas.Children.Add(l);

            var pen = new Pen();
            pen.Brush = LineBrush;
            pen.Thickness = LineWidth;
            pen.Freeze();

            //SnapToDevisePixels. See https://www.wpftutorial.net/DrawOnPhysicalDevicePixels.html
            double halfPenWidth = pen.Thickness / 2;
            GuidelineSet guideSet = new GuidelineSet();
            guideSet.GuidelinesX.Add(offset + halfPenWidth);
            guideSet.GuidelinesX.Add(offset + halfPenWidth);
            guideSet.GuidelinesY.Add(0d + halfPenWidth);
            guideSet.GuidelinesY.Add(CanvasHeight + halfPenWidth);

            dc.PushGuidelineSet(guideSet);
            dc.DrawLine(pen, new System.Windows.Point(offset, 0d), new System.Windows.Point(offset, CanvasHeight));
            dc.Pop();

            return new Line();
        }

        public CanvasGridDrawManager(LeasingChart canvas) : base(canvas) { }

        public override void Dispose()
        {
            ClearCollection(m_rowsData);

            ClearCollection(m_columnsData);

            base.Dispose();
        }

        void ClearCollection(IDictionary<int, LineData> collection)
        {
            if (collection == null)
                return;

            foreach (var data in collection)
            {
                if (data.Value != null)
                    data.Value.Clear();
            }

            collection.Clear();
        }

        protected override void M_canvas_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            if (e.HeightChanged)
            {
                var m_cHeight = e.NewSize.Height;

                if (m_cHeight > 0d)
                {
                    foreach (var ld in m_columnsData.Values)
                    {
                        if (ld.Drawed)
                            ld.Line.Y2 = m_cHeight;
                        //else
                        //    ld.Line = DrawColumn(ld.Offset); //offset для колонок расчитывается заранее
                    }
                }
            }
            if (e.WidthChanged)
            {
                var m_cWidth = e.NewSize.Width;

                if (m_cWidth > 0d)
                {
                    var height = 0d;

                    foreach (var rd in m_rowsData.Values)
                    {
                        height += RowHeight;
                        if (rd.Drawed)
                            rd.Line.X2 = m_cWidth;
                        else
                        {
                            rd.Offset = height;
                            //rd.Line = DrawRow(height);
                        }
                    }
                }
            }
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
                    m_manager.Canvas.Children.Remove(Line);
                    Line = null;
                }
                Offset = 0d;
            }
        }

        #endregion
    }
}
