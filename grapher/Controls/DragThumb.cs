﻿using grapher.ViewModels;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace grapher.Controls
{
    public class DragThumb : Thumb
    {
        public DragThumb()
        {
            base.DragDelta += new DragDeltaEventHandler(DragThumb_DragDelta);
        }

        private void DragThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            DesignerItemViewModelBase designerItem = this.DataContext as DesignerItemViewModelBase;

            if (designerItem != null && designerItem.IsSelected)
            {
                (App.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "移動";

                double minLeft = double.MaxValue;
                double minTop = double.MaxValue;

                // we only move DesignerItems
                var designerItems = designerItem.SelectedItems.OfType<DesignerItemViewModelBase>();

                foreach (var item in designerItems)
                {
                    double left = item.Left.Value;
                    double top = item.Top.Value;

                    minLeft = double.IsNaN(left) ? 0 : Math.Min(left, minLeft);
                    minTop = double.IsNaN(top) ? 0 : Math.Min(top, minTop);
                }

                double deltaHorizontal = Math.Max(-minLeft, e.HorizontalChange);
                double deltaVertical = Math.Max(-minTop, e.VerticalChange);

                foreach (var item in designerItems)
                {
                    var matrixTransform = (this.Parent as Grid).RenderTransform as MatrixTransform;
                    double left = item.Left.Value;
                    double top = item.Top.Value;

                    if (double.IsNaN(left)) left = 0;
                    if (double.IsNaN(top)) top = 0;

                    if (matrixTransform != null)
                    {
                        var dragDelta = new Point(e.HorizontalChange, e.VerticalChange);
                        dragDelta = matrixTransform.Transform(dragDelta);
                        item.Left.Value = left + dragDelta.X;
                        item.Top.Value = top + dragDelta.Y;
                        (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = $"(x, y) = ({item.Left.Value}, {item.Top.Value}) (x+, y+) = ({dragDelta.X}, {dragDelta.Y})";
                    }
                    else
                    {
                        item.Left.Value = left + deltaHorizontal;
                        item.Top.Value = top + deltaVertical;
                        (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = $"(x, y) = ({item.Left.Value}, {item.Top.Value}) (x+, y+) = ({deltaHorizontal}, {deltaVertical})";
                    }
                }
                e.Handled = true;
            }
        }
    }
}
