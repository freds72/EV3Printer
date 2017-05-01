using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace EV3Printer.Behaviors
{
    /// <summary>
    /// This class contains a few useful extenders for the ListBox
    /// </summary>
    public class ListBoxBehavior : DependencyObject
    {
        public static readonly DependencyProperty AutoScrollToEndProperty = DependencyProperty.RegisterAttached("AutoScrollToEnd", typeof(bool), typeof(ListBoxBehavior), new PropertyMetadata(default(bool), OnAutoScrollToEndChanged));

        /// <summary>
        /// Returns the value of the AutoScrollToEndProperty
        /// </summary>
        /// <param name="obj">The dependency-object whichs value should be returned</param>
        /// <returns>The value of the given property</returns>
        public static bool GetAutoScrollToEnd(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoScrollToEndProperty);
        }

        /// <summary>
        /// Sets the value of the AutoScrollToEndProperty
        /// </summary>
        /// <param name="obj">The dependency-object whichs value should be set</param>
        /// <param name="value">The value which should be assigned to the AutoScrollToEndProperty</param>
        public static void SetAutoScrollToEnd(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoScrollToEndProperty, value);
        }

        /// <summary>
        /// This method will be called when the AutoScrollToEnd
        /// property was changed
        /// </summary>
        /// <param name="s">The sender (the ListBox)</param>
        /// <param name="e">Some additional information</param>
        public static void OnAutoScrollToEndChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            var listBox = s as ListBox;
            var listBoxItems = listBox.Items;

            var scrollToEndHandler = new VectorChangedEventHandler<object>(
                (s1, e1) =>
                {
                    if (listBox.Items.Count > 0)
                    {
                        object lastItem = listBox.Items[listBox.Items.Count - 1];
                        listBox.ScrollIntoView(lastItem);
                    }
                });

            if ((bool)e.NewValue)
                listBoxItems.VectorChanged += scrollToEndHandler;
            else
                listBoxItems.VectorChanged -= scrollToEndHandler;
        }
    }
}
