﻿using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;

namespace CarLeasingViewer.Controls
{
    /// <summary>
    /// Interaction logic for CachedTabControl.xaml
    /// </summary>
    [TemplatePart(Name = "PART_ItemsHolder", Type = typeof(Panel))]
    public partial class CachedTabControl : TabControl
    {
        //https://stackoverflow.com/questions/3601125/wpf-tabcontrol-preventing-unload-on-tab-change
        // Holds all items, but only marks the current tab's item as visible
        private Panel _itemsHolder = null;

        // Temporaily holds deleted item in case this was a drag/drop operation
        private object _deletedObject = null;

        public CachedTabControl()
        : base()
        {
            // this is necessary so that we get the initial databound selected item
            this.ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
            //this.Loaded += CachedTabControl_Loaded; ;
        }

        private void CachedTabControl_Loaded(object sender, RoutedEventArgs e)
        {
            TabItem myListBoxItem = (TabItem)(ItemContainerGenerator.ContainerFromItem(Items.CurrentItem));

            //// Getting the ContentPresenter of myListBoxItem
            //ContentPresenter myContentPresenter = FindVisualChild<ContentPresenter>(myListBoxItem);
            //myContentPresenter.ApplyTemplate();
            //// Finding textBlock from the DataTemplate that is set on that ContentPresenter
            //DataTemplate myDataTemplate = myContentPresenter.ContentTemplate;
            //var myTextBlock = myDataTemplate.FindName("PART_ItemsHolder", myContentPresenter);
            //
            //if (myTextBlock == null)
            //    MessageBox.Show("sdfsdf");
        }

        /// <summary>
        /// if containers are done, generate the selected item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        {
            if (this.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
            {
                this.ItemContainerGenerator.StatusChanged -= ItemContainerGenerator_StatusChanged;

                UpdateSelectedItem();
            }
        }


        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            //TabItem myListBoxItem = (TabItem)(ItemContainerGenerator.ContainerFromItem(Items.CurrentItem));
            //
            //// Getting the ContentPresenter of myListBoxItem
            //ContentPresenter myContentPresenter = FindVisualChild<ContentPresenter>(myListBoxItem);
            //myContentPresenter.ApplyTemplate();
            //// Finding textBlock from the DataTemplate that is set on that ContentPresenter
            //DataTemplate myDataTemplate = myContentPresenter.ContentTemplate;
            //
            //var myTextBlock = myDataTemplate?.FindName("PART_ItemsHolder", myContentPresenter);
            //
            //if (myTextBlock != null)
            //    MessageBox.Show("sdfsdf");
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            //TabItem myListBoxItem = (TabItem)(ItemContainerGenerator.ContainerFromItem(Items.CurrentItem));
            //
            //// Getting the ContentPresenter of myListBoxItem
            //ContentPresenter myContentPresenter = FindVisualChild<ContentPresenter>(myListBoxItem);
            //myContentPresenter.ApplyTemplate();
            //// Finding textBlock from the DataTemplate that is set on that ContentPresenter
            //DataTemplate myDataTemplate = myContentPresenter.ContentTemplate;
            //
            //var myTextBlock = myDataTemplate?.FindName("PART_ItemsHolder", myContentPresenter);
            //
            //if (myTextBlock != null)
            //    MessageBox.Show("sdfsdf");
        }
        /// <summary>
        /// get the ItemsHolder and generate any children
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            //var child = GetTemplateChild("PART_ItemsHolder");

            //var myListBoxItem = ItemContainerGenerator.ContainerFromItem(Items.CurrentItem);
            //ContentPresenter myContentPresenter = FindVisualChild<ContentPresenter>(myListBoxItem);
            //DataTemplate myDataTemplate = myContentPresenter.ContentTemplate;
            //var child = myDataTemplate.FindName("PART_ItemsHolder", myContentPresenter);

            //_itemsHolder = child as Panel;
            UpdateSelectedItem();
        }

        private childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
        {
            var c = LogicalTreeHelper.GetChildren(obj);
            var n = LogicalTreeHelper.FindLogicalNode(obj, "PART_ItemsHolder");

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                    return (childItem)child;
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        /// <summary>
        /// when the items change we remove any generated panel children and add any new ones as necessary
        /// </summary>
        /// <param name="e"></param>
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            if (_itemsHolder == null)
            {
                return;
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    _itemsHolder.Children.Clear();

                    if (base.Items.Count > 0)
                    {
                        base.SelectedItem = base.Items[0];
                        UpdateSelectedItem();
                    }

                    break;

                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:

                    // Search for recently deleted items caused by a Drag/Drop operation
                    if (e.NewItems != null && _deletedObject != null)
                    {
                        foreach (var item in e.NewItems)
                        {
                            if (_deletedObject == item)
                            {
                                // If the new item is the same as the recently deleted one (i.e. a drag/drop event)
                                // then cancel the deletion and reuse the ContentPresenter so it doesn't have to be 
                                // redrawn. We do need to link the presenter to the new item though (using the Tag)
                                ContentPresenter cp = FindChildContentPresenter(_deletedObject);
                                if (cp != null)
                                {
                                    int index = _itemsHolder.Children.IndexOf(cp);

                                    (_itemsHolder.Children[index] as ContentPresenter).Tag =
                                        (item is TabItem) ? item : (this.ItemContainerGenerator.ContainerFromItem(item));
                                }
                                _deletedObject = null;
                            }
                        }
                    }

                    if (e.OldItems != null)
                    {
                        foreach (var item in e.OldItems)
                        {

                            _deletedObject = item;

                            // We want to run this at a slightly later priority in case this
                            // is a drag/drop operation so that we can reuse the template
                            this.Dispatcher.BeginInvoke(DispatcherPriority.DataBind,
                                new Action(delegate ()
                                {
                                    if (_deletedObject != null)
                                    {
                                        ContentPresenter cp = FindChildContentPresenter(_deletedObject);
                                        if (cp != null)
                                        {
                                            this._itemsHolder.Children.Remove(cp);
                                        }
                                    }
                                }
                            ));
                        }
                    }

                    UpdateSelectedItem();
                    break;

                case NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException("Replace not implemented yet");
            }
        }

        /// <summary>
        /// update the visible child in the ItemsHolder
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            UpdateSelectedItem();
        }

        /// <summary>
        /// generate a ContentPresenter for the selected item
        /// </summary>
        void UpdateSelectedItem()
        {
            if (_itemsHolder == null)
            {
                return;
            }

            // generate a ContentPresenter if necessary
            TabItem item = GetSelectedTabItem();
            if (item != null)
            {
                CreateChildContentPresenter(item);
            }

            // show the right child
            foreach (ContentPresenter child in _itemsHolder.Children)
            {
                child.Visibility = ((child.Tag as TabItem).IsSelected) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// <summary>
        /// create the child ContentPresenter for the given item (could be data or a TabItem)
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        ContentPresenter CreateChildContentPresenter(object item)
        {
            if (item == null)
            {
                return null;
            }

            ContentPresenter cp = FindChildContentPresenter(item);

            if (cp != null)
            {
                return cp;
            }

            // the actual child to be added.  cp.Tag is a reference to the TabItem
            cp = new ContentPresenter();
            cp.Content = (item is TabItem) ? (item as TabItem).Content : item;
            cp.ContentTemplate = this.SelectedContentTemplate;
            cp.ContentTemplateSelector = this.SelectedContentTemplateSelector;
            cp.ContentStringFormat = this.SelectedContentStringFormat;
            cp.Visibility = Visibility.Collapsed;
            cp.Tag = (item is TabItem) ? item : (this.ItemContainerGenerator.ContainerFromItem(item));
            _itemsHolder.Children.Add(cp);
            return cp;
        }

        /// <summary>
        /// Find the CP for the given object.  data could be a TabItem or a piece of data
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        ContentPresenter FindChildContentPresenter(object data)
        {
            if (data is TabItem)
            {
                data = (data as TabItem).Content;
            }

            if (data == null)
            {
                return null;
            }

            if (_itemsHolder == null)
            {
                return null;
            }

            foreach (ContentPresenter cp in _itemsHolder.Children)
            {
                if (cp.Content == data)
                {
                    return cp;
                }
            }

            return null;
        }

        /// <summary>
        /// copied from TabControl; wish it were protected in that class instead of private
        /// </summary>
        /// <returns></returns>
        protected TabItem GetSelectedTabItem()
        {
            object selectedItem = base.SelectedItem;
            if (selectedItem == null)
            {
                return null;
            }

            if (_deletedObject == selectedItem)
            {

            }

            TabItem item = selectedItem as TabItem;
            if (item == null)
            {
                item = base.ItemContainerGenerator.ContainerFromIndex(base.SelectedIndex) as TabItem;
            }
            return item;
        }
    }
}
