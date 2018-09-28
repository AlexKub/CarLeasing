using System;
using System.Collections.Generic;

namespace CarLeasingViewer.Models
{
    partial class LeasingElementModel
    {
        /// <summary>
        /// Логика генерации и управления ID элементов Canvas
        /// </summary>
        public class CanvasElementID_Manager
        {
            Dictionary<int, int> m_rowsCounters = new Dictionary<int, int>();

            /// <summary>
            /// Генерация нового ID для следующего в строке элемента
            /// </summary>
            /// <param name="model">Модель элемента</param>
            /// <returns>Возвращает следующий порядковый ID для текущей строки</returns>
            public int GenerateID(LeasingElementModel model)
            {
                if (model == null)
                    throw new ArgumentNullException("Попытка генерации ID по пустой ссылке на элемент Canvas'а");
                
                var a = model.RowIndex + 1; //прибавляем единицу, т.к. при индексации с 0 будут наложения индекса в строках
                var b = 0;

                //получаем порядковый номер элемента в строке
                if (m_rowsCounters.ContainsKey(a))
                    b = ++m_rowsCounters[a];
                else
                    m_rowsCounters.Add(a, ++b);

                int pow = 1;

                while (pow < b)
                {
                    pow = ((pow << 2) + pow) << 1;
                    a = ((a << 2) + a) << 1;
                }

                return a + b;
            }

            /// <summary>
            /// Сброс счётчика элементов
            /// </summary>
            public void Reset()
            {
                m_rowsCounters.Clear();
            }
        }
    }
}
