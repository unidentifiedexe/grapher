﻿using grapher.Controls;
using grapher.ViewModels;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace grapher.Adorners
{
    internal class RectangleAdorner : Adorner
    {
        private DesignerCanvas _designerCanvas;
        private Point? _startPoint;
        private Point? _endPoint;
        private Pen _rectanglePen;

        public RectangleAdorner(DesignerCanvas designerCanvas, Point? dragStartPoint)
            : base(designerCanvas)
        {
            _designerCanvas = designerCanvas;
            _startPoint = dragStartPoint;
            var parent = (AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel;
            var brush = new SolidColorBrush(parent.EdgeColors.First());
            brush.Opacity = 0.5;
            _rectanglePen = new Pen(brush, 1);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!this.IsMouseCaptured)
                    this.CaptureMouse();

                _endPoint = e.GetPosition(this);

                (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = $"({_startPoint.Value.X}, {_startPoint.Value.Y}) - ({_endPoint.Value.X}, {_endPoint.Value.Y}) (w, h) = ({_endPoint.Value.X - _startPoint.Value.X}, {_endPoint.Value.Y - _startPoint.Value.Y})";

                this.InvalidateVisual();
            }
            else
            {
                if (this.IsMouseCaptured) this.ReleaseMouseCapture();
            }

            e.Handled = true;
        }

        protected override void OnMouseUp(System.Windows.Input.MouseButtonEventArgs e)
        {
            // release mouse capture
            if (this.IsMouseCaptured) this.ReleaseMouseCapture();

            // remove this adorner from adorner layer
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(_designerCanvas);
            if (adornerLayer != null)
                adornerLayer.Remove(this);

            if (_startPoint.HasValue && _endPoint.HasValue)
            {
                var rand = new Random();
                var item = new NRectangleViewModel();
                item.Owner = (AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel;
                item.Left.Value = Math.Min(_startPoint.Value.X, _endPoint.Value.X);
                item.Top.Value = Math.Min(_startPoint.Value.Y, _endPoint.Value.Y);
                item.Width.Value = Math.Max(_startPoint.Value.X - _endPoint.Value.X, _endPoint.Value.X - _startPoint.Value.X);
                item.Height.Value = Math.Max(_startPoint.Value.Y - _endPoint.Value.Y, _endPoint.Value.Y - _startPoint.Value.Y);
                item.EdgeColor = item.Owner.EdgeColors.First();
                item.FillColor = item.Owner.FillColors.First();
                item.ZIndex.Value = item.Owner.Items.Count;
                item.IsSelected = true;
                item.Owner.DeselectAll();
                ((AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel).AddItemCommand.Execute(item);

                _startPoint = null;
                _endPoint = null;
            }

            (App.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "";
            (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = "";

            e.Handled = true;
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));

            if (_startPoint.HasValue && _endPoint.HasValue)
                dc.DrawRectangle(Brushes.Transparent, _rectanglePen, new Rect(_startPoint.Value, _endPoint.Value));
        }
    }
}
