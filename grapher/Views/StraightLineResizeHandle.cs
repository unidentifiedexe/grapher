﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace grapher.Views
{
    /*
     * How to arrange Thumbs with a Line in a WPF custom Adorner https://stackoverflow.com/questions/10821556/how-to-arrange-thumbs-with-a-line-in-a-wpf-custom-adorner
     * quizzer canha https://stackoverflow.com/users/614815/canha
     * answerer Trevor Elliott https://stackoverflow.com/users/852555/trevor-elliott
     */

    class StraightLineResizeHandle : Adorner
    {
        private Point start;
        private Point end;
        private Thumb startThumb;
        private Thumb endThumb;
        private Line selectedLine;
        private VisualCollection visualChildren;

        public StraightLineResizeHandle(UIElement adornedElement)
            : base(adornedElement)
        {
            visualChildren = new VisualCollection(this);

            var template = (ControlTemplate)App.Current.MainWindow.FindResource("ResizeHandleTemplate");
            startThumb = new Thumb
            {
                Width = 7,
                Height = 7,
                Template = template,
            };
            endThumb = new Thumb
            {
                Width = 7,
                Height = 7,
                Template = template,
            };

            startThumb.DragDelta += StartThumb_DragDelta;
            endThumb.DragDelta += EndThumb_DragDelta;

            visualChildren.Add(startThumb);
            visualChildren.Add(endThumb);

            selectedLine = AdornedElement as Line;
        }

        private void StartThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Point position = Mouse.GetPosition(this);

            selectedLine.X1 = position.X;
            selectedLine.Y1 = position.Y;
        }

        private void EndThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Point position = Mouse.GetPosition(this);

            selectedLine.X2 = position.X;
            selectedLine.Y2 = position.Y;
        }

        protected override int VisualChildrenCount => visualChildren.Count;

        protected override Visual GetVisualChild(int index)
        {
            return visualChildren[index];
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (AdornedElement is Line)
            {
                selectedLine = AdornedElement as Line;
                start = new Point(selectedLine.X1, selectedLine.Y1);
                end = new Point(selectedLine.X2, selectedLine.Y2);
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            selectedLine = AdornedElement as Line;

            double left = Math.Min(selectedLine.X1, selectedLine.X2);
            double top = Math.Min(selectedLine.Y1, selectedLine.Y2);

            var startRect = new Rect(selectedLine.X1 - (startThumb.Width / 2), selectedLine.Y1 - (startThumb.Height / 2), startThumb.Width, startThumb.Height);
            startThumb.Arrange(startRect);

            var endRect = new Rect(selectedLine.X2 - (endThumb.Width / 2), selectedLine.Y2 - (endThumb.Height / 2), endThumb.Width, endThumb.Height);
            endThumb.Arrange(endRect);

            return finalSize;
        }
    }
}