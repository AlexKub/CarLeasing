namespace CarLeasingViewer.Models
{
    public class StatisticItemLabel : StatisticItemModel
    {
        public StatisticItemLabel(string label)
            : base(label, "") { }

        public override string Text => Name;
    }
}
