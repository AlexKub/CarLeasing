﻿using System;
using CarLeasingViewer.Interfaces;

namespace CarLeasingViewer.Models
{
    public class MaintenanceBarModel : Interfaces.IDrawableBar
    {
        public int RowIndex { get; set; }

        public IPeriod Period { get; private set; }

        public LeasingSet Set { get; private set; }

        public string Text { get; private set; }

        public string[] ToolTipRows { get; private set; }

        public MaintenanceBarModel(LeasingSet set, ItemInfo item)
        {
            Set = set;
            Period = item.Maintenance;

            SetTooolTip(item);
        }
        private MaintenanceBarModel()
        {

        }

        void SetTooolTip(ItemInfo item)
        {

        }

        public IDrawableBar Clone()
        {
            var copy = new MaintenanceBarModel();

            copy.Set = Set;
            copy.RowIndex = RowIndex;
            copy.Period = Period;
            copy.Text = Text;
            copy.ToolTipRows = ToolTipRows;

            return copy;
        }
    }
}
