using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CarLeasingViewer.XamlExtensions
{
    public static class StickyScrollHeader
    {
        static readonly LogSet m_loger = LogManager.GetDefaultLogSet(nameof(StickyScrollHeader));

        static FrameworkElement m_parent;
        static FrameworkElement m_targetControl;

        public static FrameworkElement GetAttachToControl(FrameworkElement obj)
        {
            return (FrameworkElement)obj.GetValue(AttachToControlProperty);
        }

        public static void SetAttachToControl(FrameworkElement obj, FrameworkElement value)
        {
            obj.SetValue(AttachToControlProperty, value);
        }

        private static ScrollViewer FindScrollViewer(FrameworkElement item)
        {
            FrameworkElement treeItem = item;
            FrameworkElement directItem = item;

            while (treeItem != null)
            {
                treeItem = VisualTreeHelper.GetParent(treeItem) as FrameworkElement;
                if (treeItem is ScrollViewer)
                {
                    return treeItem as ScrollViewer;
                }
                else if (treeItem is ScrollContentPresenter)
                {
                    return (treeItem as ScrollContentPresenter).ScrollOwner;
                }
            }

            while (directItem != null)
            {
                directItem = directItem.Parent as FrameworkElement;

                if (directItem is ScrollViewer)
                {
                    return directItem as ScrollViewer;
                }
                else if (directItem is ScrollContentPresenter)
                {
                    return (directItem as ScrollContentPresenter).ScrollOwner;
                }
            }

            return null;
        }

        private static ScrollContentPresenter FindScrollContentPresenter(FrameworkElement sv)
        {
            int childCount = VisualTreeHelper.GetChildrenCount(sv);

            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(sv, i);
                if (child is FrameworkElement && child is ScrollContentPresenter)
                {
                    return child as ScrollContentPresenter;
                }

            }

            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(sv, i);
                if ((child as FrameworkElement) is FrameworkElement && child is ScrollContentPresenter)
                {
                    return child as ScrollContentPresenter;
                }
            }

            return null;
        }

        public static readonly DependencyProperty AttachToControlProperty =
            DependencyProperty.RegisterAttached("AttachToControl", typeof(FrameworkElement), typeof(StickyScrollHeader), new PropertyMetadata(null, (s, e) =>
            {
                try
                {
                    m_targetControl = s as FrameworkElement;
                    if (m_targetControl == null)
                    {
                        return;
                    }

                    Canvas.SetZIndex(m_targetControl, 999);

                    ScrollViewer sv;

                    var oldParentControl = e.OldValue as FrameworkElement;
                    if (oldParentControl != null)
                    {
                        ScrollViewer oldSv = FindScrollViewer(oldParentControl);
                        m_parent = oldParentControl;
                        oldSv.ScrollChanged -= Sv_ScrollChanged;
                    }

                    var newParentControl = e.NewValue as FrameworkElement;
                    if (newParentControl != null)
                    {
                        sv = FindScrollViewer(newParentControl);
                        m_parent = newParentControl;
                        sv.ScrollChanged += Sv_ScrollChanged;
                    }
                }
                catch (Exception ex)
                {
                    m_loger.Log("Возникло исключение при привязке AttachToControl", ex);
                }
            }));

        static void Sv_ScrollChanged(object sender, ScrollChangedEventArgs sce)
        {
            if (m_parent == null || (!m_parent.IsVisible))
                return;

            try
            {

                ScrollViewer isv = sender as ScrollViewer;
                ScrollContentPresenter scp = FindScrollContentPresenter(isv);

                var relativeTransform = m_parent.TransformToAncestor(scp);
                Rect parentRenderRect = relativeTransform.TransformBounds(new Rect(new Point(0, 0), m_parent.RenderSize));
                Rect intersectingRect = Rect.Intersect(new Rect(new Point(0, 0), scp.RenderSize), parentRenderRect);
                if (intersectingRect != Rect.Empty)
                {
                    TranslateTransform targetTransform = new TranslateTransform();

                    if (parentRenderRect.Top < 0)
                    {
                        double tempTop = (parentRenderRect.Top * -1);

                        if (tempTop + m_targetControl.RenderSize.Height < m_parent.RenderSize.Height)
                        {
                            targetTransform.Y = tempTop;
                        }
                        else if (tempTop < m_parent.RenderSize.Height)
                        {
                            targetTransform.Y = tempTop - (m_targetControl.RenderSize.Height - intersectingRect.Height);
                        }
                    }
                    else
                    {
                        targetTransform.Y = 0;
                    }
                    m_targetControl.RenderTransform = targetTransform;
                }
            }
            catch (Exception ex)
            {
                m_loger.Log("Возникло исключение при обработке события скролла", ex);
            }
        }

    }


}
