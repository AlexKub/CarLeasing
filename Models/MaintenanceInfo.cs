﻿using System;

namespace CarLeasingViewer.Models
{
    /// <summary>
    /// Информация о ремонте авто
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{DebugDisplay()}")]
    public class MaintenanceInfo : IPeriod
    {
        DateTime m_DateStart;
        /// <summary>
        /// Дата начала
        /// </summary>
        public DateTime DateStart
        {
            get { return m_DateStart; }
            set
            {
                m_DateStart = value;
                MonthCount = PeriodManaging.CalculateMonthCount(this);
            }
        }

        DateTime m_DateEnd;
        /// <summary>
        /// Дата окончания
        /// </summary>
        public DateTime DateEnd
        {
            get { return m_DateEnd; }
            set
            {
                m_DateEnd = value;
                MonthCount = PeriodManaging.CalculateMonthCount(this);
            }
        }

        /// <summary>
        /// Количество месяцев
        /// </summary>
        public int MonthCount { get; private set; }

        /// <summary>
        /// Описание
        /// </summary>
        public string Description { get; set; }

        string DebugDisplay()
        {
            return $"{DateStart.ToString()} - {DateEnd.ToString()}";
        }
    }
}
