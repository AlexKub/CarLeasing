using System;

namespace CarLeasingViewer
{
    [Flags]
    public enum GridOrientation
    {
        None = 0,
        Vertical = 1,
        Horizontal = 2,
        Both = Vertical | Horizontal
    }
}
