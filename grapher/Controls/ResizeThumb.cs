﻿using grapher.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace grapher.Controls
{
    public class ResizeThumb : Thumb
    {
        public ResizeThumb()
        {
            base.DragDelta += new DragDeltaEventHandler(ResizeThumb_DragDelta);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            (App.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "リサイズ";
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            (App.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "";
            (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = "";
        }

        private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var designerItem = this.DataContext as DesignerItemViewModelBase;

            if (designerItem != null && designerItem.IsSelected)
            {
                double minLeft, minTop, minDeltaHorizontal, minDeltaVertical;
                double dragDeltaVertical, dragDeltaHorizontal;

                // only resize DesignerItems
                var selectedDesignerItems = from item in designerItem.Owner.SelectedItems
                                            where item is DesignerItemViewModelBase
                                            select item;

                CalculateDragLimits(selectedDesignerItems, out minLeft, out minTop,
                                    out minDeltaHorizontal, out minDeltaVertical);

                foreach (var item in selectedDesignerItems)
                {
                    if (item is DesignerItemViewModelBase)
                    {
                        var viewModel = item as DesignerItemViewModelBase;
                        if (viewModel is PictureDesignerItemViewModel &&
                            ((Keyboard.GetKeyStates(Key.LeftShift) & KeyStates.Down) == KeyStates.Down ||
                             (Keyboard.GetKeyStates(Key.RightShift) & KeyStates.Down) == KeyStates.Down))
                        {
                            var picViewModel = viewModel as PictureDesignerItemViewModel;
                            if (base.VerticalAlignment == VerticalAlignment.Top && base.HorizontalAlignment == HorizontalAlignment.Left)
                            {
                                //????????????????
                            }
                            else if (base.VerticalAlignment == VerticalAlignment.Top && base.HorizontalAlignment == HorizontalAlignment.Right)
                            {
                                double top = picViewModel.Top.Value;
                                dragDeltaVertical = Math.Min(Math.Max(-minTop, e.VerticalChange), minDeltaVertical);
                                picViewModel.Top.Value = top + dragDeltaVertical;
                                picViewModel.Height.Value = picViewModel.Height.Value - dragDeltaVertical;
                                picViewModel.Width.Value = (picViewModel.Height.Value / picViewModel.FileHeight) * picViewModel.FileWidth;
                            }
                            else if (base.VerticalAlignment == VerticalAlignment.Bottom && base.HorizontalAlignment == HorizontalAlignment.Left)
                            {
                                double left = picViewModel.Left.Value;
                                dragDeltaHorizontal = Math.Min(Math.Max(-minLeft, e.HorizontalChange), minDeltaHorizontal);
                                picViewModel.Left.Value = left + dragDeltaHorizontal;
                                picViewModel.Width.Value = picViewModel.Width.Value - dragDeltaHorizontal;
                                picViewModel.Height.Value = (picViewModel.Width.Value / picViewModel.FileWidth) * picViewModel.FileHeight;
                            }
                            else if (base.VerticalAlignment == VerticalAlignment.Bottom && base.HorizontalAlignment == HorizontalAlignment.Right)
                            {
                                dragDeltaVertical = Math.Min(-e.VerticalChange, minDeltaVertical);
                                picViewModel.Height.Value = picViewModel.Height.Value - dragDeltaVertical;
                                picViewModel.Width.Value = (picViewModel.Height.Value / picViewModel.FileHeight) * picViewModel.FileWidth;
                            }
                        }
                        else
                        {
                            switch (base.VerticalAlignment)
                            {
                                case VerticalAlignment.Bottom:
                                    dragDeltaVertical = Math.Min(-e.VerticalChange, minDeltaVertical);
                                    viewModel.Height.Value = viewModel.Height.Value - dragDeltaVertical;
                                    break;
                                case VerticalAlignment.Top:
                                    double top = viewModel.Top.Value;
                                    dragDeltaVertical = Math.Min(Math.Max(-minTop, e.VerticalChange), minDeltaVertical);
                                    viewModel.Top.Value = top + dragDeltaVertical;
                                    viewModel.Height.Value = viewModel.Height.Value - dragDeltaVertical;
                                    break;
                                default:
                                    break;
                            }

                            switch (base.HorizontalAlignment)
                            {
                                case HorizontalAlignment.Left:
                                    double left = viewModel.Left.Value;
                                    dragDeltaHorizontal = Math.Min(Math.Max(-minLeft, e.HorizontalChange), minDeltaHorizontal);
                                    viewModel.Left.Value = left + dragDeltaHorizontal;
                                    viewModel.Width.Value = viewModel.Width.Value - dragDeltaHorizontal;
                                    break;
                                case HorizontalAlignment.Right:
                                    dragDeltaHorizontal = Math.Min(-e.HorizontalChange, minDeltaHorizontal);
                                    viewModel.Width.Value = viewModel.Width.Value - dragDeltaHorizontal;
                                    break;
                                default:
                                    break;
                            }
                        }

                        (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = $"(w, h) = ({viewModel.Width.Value}, {viewModel.Height.Value})";
                    }
                }
                e.Handled = true;
            }
        }

        private static void CalculateDragLimits(IEnumerable<SelectableDesignerItemViewModelBase> selectedDesignerItems, out double minLeft, out double minTop, out double minDeltaHorizontal, out double minDeltaVertical)
        {
            minLeft = double.MaxValue;
            minTop = double.MaxValue;
            minDeltaHorizontal = double.MaxValue;
            minDeltaVertical = double.MaxValue;

            // drag limits are set by these parameters: canvas top, canvas left, minHeight, minWidth
            // calculate min value for each parameter for each item
            foreach (var item in selectedDesignerItems)
            {
                if (item is DesignerItemViewModelBase)
                {
                    var viewModel = item as DesignerItemViewModelBase;
                    double left = viewModel.Left.Value;
                    double top = viewModel.Top.Value;

                    minLeft = double.IsNaN(left) ? 0 : Math.Min(left, minLeft);
                    minTop = double.IsNaN(top) ? 0 : Math.Min(top, minTop);

                    minDeltaVertical = Math.Min(minDeltaVertical, viewModel.Height.Value - viewModel.MinHeight);
                    minDeltaHorizontal = Math.Min(minDeltaHorizontal, viewModel.Width.Value - viewModel.MinWidth);
                }
                else if (item is ConnectorBaseViewModel)
                {
                    var viewModel = item as ConnectorBaseViewModel;
                    double left = Math.Min(viewModel.SourceA.X, viewModel.SourceB.X);
                    double top = Math.Min(viewModel.SourceA.Y, viewModel.SourceB.Y);

                    double width = Math.Max(viewModel.SourceA.X, viewModel.SourceB.X) - Math.Min(viewModel.SourceA.X, viewModel.SourceB.X);
                    double height = Math.Max(viewModel.SourceA.Y, viewModel.SourceB.Y) - Math.Min(viewModel.SourceA.Y, viewModel.SourceB.Y);

                    minDeltaVertical = Math.Min(minDeltaVertical, height);
                    minDeltaHorizontal = Math.Min(minDeltaHorizontal, width);
                }
            }
        }
    }
}
