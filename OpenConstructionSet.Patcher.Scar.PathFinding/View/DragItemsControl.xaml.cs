using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace OpenConstructionSet.Patcher.Scar.PathFinding.View
{
    /// <summary>
    /// Interaction logic for DragItemsControl.xaml
    /// </summary>
    public partial class DragItemsControl : ItemsControl
    {
        const int DropDelay = 200;

        int lastDrop = Environment.TickCount;

        public DragItemsControl()
        {
            InitializeComponent();
        }

        private void ItemsControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            if (!(sender is DragItemsControl control))
            {
                return;
            }

            var data = (Mouse.DirectlyOver as FrameworkElement)?.DataContext;

            if (data == null)
            {
                return;
            }

            DragDrop.DoDragDrop(control, data, DragDropEffects.Move);
        }

        private void ItemsControl_Drop(object sender, DragEventArgs e)
        {
            if (Environment.TickCount - lastDrop < DropDelay)
            {
                Trace.WriteLine($"Drop blocked. {Environment.TickCount - lastDrop}");
                return;
            }

            lastDrop = Environment.TickCount;

            if (!(sender is ItemsControl ic))
            {
                return;
            }

            var point = e.GetPosition(ic);

            var dragItem = e.Data.GetData(GetDataFormat());

            var dropItem = (VisualTreeHelper.HitTest(ic, point).VisualHit as FrameworkElement)?.DataContext;

            IList list = ic.ItemsSource as IList;

            if (list == null || dropItem == null || dragItem == null || !IsObservableCollection(list.GetType()))
            {
                return;
            }

            var oldIndex = list.IndexOf(dragItem);
            var newIndex = list.IndexOf(dropItem);

            if (newIndex == -1 || oldIndex == -1 || newIndex == oldIndex)
            {
                return;
            }

            dynamic observableCollection = list;

            observableCollection.Move(oldIndex, newIndex);

            e.Handled = true;

            bool IsObservableCollection(Type type)
            {
                if (type == null)
                {
                    throw new ArgumentNullException("type");
                }

                return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ObservableCollection<>);
            }
        }

        private string GetDataFormat()
        {
            var itemType = ItemsSource.GetType();

            if (IsGenericList(itemType))
            {
                var type = itemType.GenericTypeArguments[0];

                if (type == typeof(string))
                {
                    return DataFormats.Text;
                }
                else
                {
                    return type.FullName;
                }
            }

            return null;

            bool IsGenericList(Type type)
            {
                if (type == null)
                {
                    throw new ArgumentNullException("type");
                }
                foreach (Type @interface in type.GetInterfaces())
                {
                    if (@interface.IsGenericType)
                    {
                        if (@interface.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                        {
                            // if needed, you can also return the type used as generic argument
                            return true;
                        }
                    }
                }
                return false;
            }
        }
    }
}
