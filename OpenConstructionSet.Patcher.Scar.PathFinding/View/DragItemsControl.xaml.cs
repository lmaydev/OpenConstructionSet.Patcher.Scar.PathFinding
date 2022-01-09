using OpenConstructionSet.Patcher.Scar.PathFinding.ViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace OpenConstructionSet.Patcher.Scar.PathFinding.View
{
    /// <summary>
    /// Interaction logic for DragItemsControl.xaml
    /// </summary>
    public partial class DragItemsControl : HeaderedItemsControl
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

            var dc = (Mouse.DirectlyOver as FrameworkElement)!;

            if (!TryGetData(dc, out var data))
            {
                return;
            }

            DragDrop.DoDragDrop(control, new DataObject(data), DragDropEffects.Move);
        }

        private bool TryGetData(FrameworkElement? element, [MaybeNullWhen(false)] out ModViewModel data)
        {
            var template = element?.TemplatedParent as FrameworkElement;

            var parent = template?.Parent as FrameworkElement;

            data = parent?.DataContext as ModViewModel;

            return data is not null;
        }

        private void ItemsControl_Drop(object sender, DragEventArgs e)
        {
            if (Environment.TickCount - lastDrop < DropDelay)
            {
                Trace.WriteLine($"Drop blocked. {Environment.TickCount - lastDrop}");
                return;
            }

            lastDrop = Environment.TickCount;

            if (sender is not ItemsControl ic)
            {
                return;
            }

            var point = e.GetPosition(ic);

            var dragItem = e.Data.GetData(GetDataFormat());

            if (!TryGetData(VisualTreeHelper.HitTest(ic, point).VisualHit as FrameworkElement, out var dropItem))
            {
                return;
            }

            IList list = (IList)ic.ItemsSource;

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

        private string? GetDataFormat()
        {
            var itemType = ItemsSource.GetType();

            if (IsGenericList(itemType))
            {
                Type type = itemType.GenericTypeArguments[0];

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
