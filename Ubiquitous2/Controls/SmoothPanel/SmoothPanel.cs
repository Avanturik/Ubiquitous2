// --------------------------------------------------------------------------
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// --------------------------------------------------------------------------

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace Devart.Controls
{
    /// <summary>
    /// Panel that virtualizes child collection and supports smooth scrolling.
    /// </summary>
    public partial class SmoothPanel : VirtualizingPanel, IScrollInfo
    {
        /// <summary>
        /// Using a DependencyProperty as the backing store for Templates.
        /// </summary>
        public static readonly DependencyProperty TemplatesProperty;

        /// <summary>
        /// The line scroll value
        /// </summary>
        private const double LineScrollValue = 16;

        /// <summary>
        /// The wheel scroll value
        /// </summary>
        private const double WheelScrollValue = 64;

        /// <summary>
        /// Dependency property identifier for limited write access to a Templates property.
        /// </summary>
        private static readonly DependencyPropertyKey _templatesPropertyKey;

        /// <summary>
        /// The <see cref="SmoothPanelChildren"/>
        /// </summary>
        private readonly SmoothPanelChildren _children;

        /// <summary>
        /// A <see cref="T:ScrollViewer" /> element that controls scrolling behavior.
        /// </summary>
        private ScrollViewer _scrollOwner;

        /// <summary>
        /// The size of scroll extent.
        /// </summary>
        private Size _scrollExtent;

        /// <summary>
        /// The size of the viewport.
        /// </summary>
        private Size _scrollViewport;

        /// <summary>
        /// The vertical offset of the scrolled content.
        /// </summary>
        private double _scrollOffset;

        /// <summary>
        /// The first visible item index.
        /// </summary>
        private int _firstItemIndex;

        /// <summary>
        /// The ratio of clipped part of first visible item to its height.
        /// </summary>
        private double _firstItemClippedRatio;

        /// <summary>
        /// Initializes static members of the <see cref="SmoothPanel"/> class.
        /// </summary>
        static SmoothPanel()
        {
            _templatesPropertyKey = DependencyProperty.RegisterReadOnly(
                "Templates",
                typeof(Collection<SmoothPanelTemplate>),
                typeof(SmoothPanel),
                new FrameworkPropertyMetadata(null));
            TemplatesProperty = _templatesPropertyKey.DependencyProperty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmoothPanel"/> class.
        /// </summary>
        public SmoothPanel()
        {
            var templates = new ObservableCollection<SmoothPanelTemplate>();
            templates.CollectionChanged += Templates_CollectionChanged;
            SetValue(_templatesPropertyKey, templates);
            _children = new SmoothPanelChildren(this);
        }

        /// <summary>
        /// Gets the templates.
        /// </summary>
        /// <value>
        /// The templates.
        /// </value>
        public Collection<SmoothPanelTemplate> Templates
        {
            get
            {
                return (Collection<SmoothPanelTemplate>)GetValue(TemplatesProperty);
            }
        }

        /// <summary>
        /// Causes the item to scroll into view.
        /// </summary>
        /// <param name="itemIndex">Index of the item.</param>
        public void ScrollIntoView(int itemIndex)
        {
            ScrollIntoView(itemIndex, null);
        }

        /// <summary>
        /// Causes the item to scroll into view.
        /// </summary>
        /// <param name="itemIndex">Index of the item.</param>
        /// <param name="afterScrollAction">An action that will be called after scrolling item into view.</param>
        public void ScrollIntoView(int itemIndex, Action<FrameworkElement> afterScrollAction)
        {
            var items = _children.GetItems();

            if (items == null || itemIndex < 0 || itemIndex >= items.Count)
            {
                return;
            }

            var element = _children.GetElement(itemIndex);
            if (element != null && System.Windows.Media.VisualTreeHelper.GetParent(element) == this)
            {
                // Child already created, just ensure its visibility
                ((IScrollInfo)this).MakeVisible(element, new Rect(0, 0, element.ActualWidth, element.ActualHeight));
                if (afterScrollAction != null)
                {
                    afterScrollAction(element);
                }
            }
            else
            {
                // Scroll to specified item
                SetFirstVisibleItem(itemIndex, 0);
                InvalidateMeasure();

                if (afterScrollAction != null)
                {
                    // Item will be visible only after rearrangement, so use BeginInvoke to call the action.
                    Dispatcher.BeginInvoke(
                        (Action)(() =>
                        {
                            var newElement = _children.GetElement(itemIndex);
                            if (newElement != null)
                            {
                                afterScrollAction(newElement);
                            }
                        }),
                        DispatcherPriority.Loaded);
                }
            }
        }

        /// <summary>
        /// Sets the vertical offset.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <param name="invalidateMeasure">If set to <c>true</c> measure will be invalidated.</param>
        public void SetVerticalOffset(double offset, bool invalidateMeasure)
        {
            // Limit offset value
            if (offset < 0 || _scrollViewport.Height >= _scrollExtent.Height)
            {
                offset = 0;
            }
            else if (offset > _scrollExtent.Height - _scrollViewport.Height)
            {
                offset = _scrollExtent.Height - _scrollViewport.Height;
            }

            if (offset != _scrollOffset)
            {
                _scrollOffset = offset;

                if (_scrollOwner != null)
                {
                    // Update scrollbar
                    _scrollOwner.InvalidateScrollInfo();
                }

                if (invalidateMeasure)
                {
                    // First item should be found by new scroll position
                    _firstItemIndex = -1;
                    InvalidateMeasure();
                }
            }
        }

        /// <summary>
        /// When overridden in a derived class, measures the size in layout required for child elements and
        /// determines a size for the <see cref="T:System.Windows.FrameworkElement" />-derived class.
        /// </summary>
        /// <param name="availableSize">The available size that this element can give to child elements.</param>
        /// <returns>
        /// The size that this element determines it needs during layout, based on its calculations of child element sizes.
        /// </returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            if (double.IsInfinity(availableSize.Width))
            {
                Debug.Assert(false, "Infinite width is not supported");
                availableSize = new Size(100, availableSize.Height);
            }

            if (double.IsInfinity(availableSize.Height))
            {
                Debug.Assert(false, "Infinite height is not supported");
                availableSize = new Size(availableSize.Width, 100);
            }

            _children.AvailableWidth = availableSize.Width;

            var measurer = new SmoothPanelMeasurer(this, availableSize);
            measurer.Measure();

            return availableSize;
        }

        /// <summary>
        /// When overridden in a derived class, positions child elements and determines a size for
        /// a <see cref="T:System.Windows.FrameworkElement" /> derived class.
        /// </summary>
        /// <param name="finalSize">
        /// The final area within the parent that this element should use to arrange itself and its children.
        /// </param>
        /// <returns>
        /// The actual size used.
        /// </returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            if (_children.ItemsCount == 0)
            {
                return finalSize;
            }

            double width = finalSize.Width;
            double top = 0;
            bool isFirst = true;

            if (_firstItemIndex < 0)
            {
                Debug.Assert(false, "First visible item should be specified before arrangement.");
                SetFirstVisibleItem(0, 0);
                SetVerticalOffset(0, false);
            }

            for (int index = _firstItemIndex; index < _children.ItemsCount; index++)
            {
                FrameworkElement element = _children.GetElement(index);

                if (element != null)
                {
                    double height = element.DesiredSize.Height;

                    if (isFirst)
                    {
                        // First visible item can be clipped
                        top = -height * _firstItemClippedRatio;
                        isFirst = false;
                    }

                    element.Arrange(new Rect(0, top, width, height));

                    top += height;

                    if (top >= finalSize.Height)
                    {
                        // Out of view
                        break;
                    }
                }
            }

            return finalSize;
        }

        /// <summary>
        /// Called when the <see cref="P:System.Windows.Controls.ItemsControl.Items" />
        /// collection that is associated with the <see cref="T:System.Windows.Controls.ItemsControl" /> for
        /// this <see cref="T:System.Windows.Controls.Panel" /> changes.
        /// </summary>
        /// <param name="sender">The <see cref="T:System.Object" /> that raised the event.</param>
        /// <param name="args">Provides data for the <see cref="E:System.Windows.Controls.ItemContainerGenerator.ItemsChanged" /> event.</param>
        protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
        {
            _children.ResetItems();
            InvalidateMeasure();
        }

        /// <summary>
        /// When implemented in a derived class, generates the item at the specified index location and makes it visible.
        /// </summary>
        /// <param name="index">The index position of the item that is generated and made visible.</param>
        protected override void BringIndexIntoView(int index)
        {
            ScrollIntoView(index, null);
        }

        /// <summary>
        /// Handles the CollectionChanged event of the Templates control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        /// The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> instance
        /// containing the event data.
        /// </param>
        private void Templates_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            _children.ResetViewCaches();
        }

        /// <summary>
        /// Sets the first visible item.
        /// </summary>
        /// <param name="index">The item index.</param>
        /// <param name="clippedRatio">The ratio of clipped part to entire height.</param>
        private void SetFirstVisibleItem(int index, double clippedRatio)
        {
            _firstItemIndex = index;
            _firstItemClippedRatio = clippedRatio;
        }

        /// <summary>
        /// Updates the scroll information.
        /// </summary>
        /// <param name="newScrollViewport">The new scroll viewport.</param>
        /// <param name="totalItemsHeight">Total height of the items.</param>
        private void UpdateScrollInfo(Size newScrollViewport, double totalItemsHeight)
        {
            var newScrollExtent = new Size(newScrollViewport.Width, totalItemsHeight);

            if (newScrollExtent != _scrollExtent || newScrollViewport != _scrollViewport)
            {
                _scrollExtent = newScrollExtent;
                _scrollViewport = newScrollViewport;
                double maxOffset = _scrollExtent.Height - _scrollViewport.Height;
                _scrollOffset = Math.Max(0, Math.Min(_scrollOffset, maxOffset));

                if (_scrollOwner != null)
                {
                    _scrollOwner.InvalidateScrollInfo();
                }
            }
        }

        /// <summary>
        /// Gets the page scroll value.
        /// </summary>
        /// <returns>An <see cref="Int32"/> that represents a page scroll value.</returns>
        private double GetPageScrollValue()
        {
            return Math.Max(_scrollViewport.Height, LineScrollValue);
        }

        #region IScrollInfo Members

        /// <summary>
        /// Gets or sets a <see cref="T:ScrollViewer" /> element that controls scrolling behavior.
        /// </summary>
        /// <returns>
        /// A <see cref="T:ScrollViewer" /> element that controls scrolling behavior.
        /// </returns>
        ScrollViewer IScrollInfo.ScrollOwner
        {
            get
            {
                return _scrollOwner;
            }
            set
            {
                _scrollOwner = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether scrolling on the horizontal axis is possible.
        /// </summary>
        /// <returns><c>true</c> if scrolling is possible; otherwise, <c>false</c>.</returns>
        bool IScrollInfo.CanHorizontallyScroll
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether scrolling on the vertical axis is possible.
        /// </summary>
        /// <returns><c>true</c> if scrolling is possible; otherwise, <c>false</c>.</returns>
        bool IScrollInfo.CanVerticallyScroll
        {
            get
            {
                return true;
            }
            set
            {
            }
        }

        /// <summary>
        /// Gets the horizontal offset of the scrolled content.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Double" /> that represents, in device independent pixels, the horizontal offset.
        /// </returns>
        double IScrollInfo.HorizontalOffset
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Gets the vertical offset of the scrolled content.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Double" /> that represents, in device independent pixels,
        /// the vertical offset of the scrolled content. Valid values are between zero and
        /// the <see cref="P:IScrollInfo.ExtentHeight" /> minus
        /// the <see cref="P:IScrollInfo.ViewportHeight" />.
        /// </returns>
        double IScrollInfo.VerticalOffset
        {
            get
            {
                return _scrollOffset;
            }
        }

        /// <summary>
        /// Gets the vertical size of the extent.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Double" /> that represents, in device independent pixels,
        /// the vertical size of the extent.
        /// </returns>
        double IScrollInfo.ExtentHeight
        {
            get
            {
                return _scrollExtent.Height;
            }
        }

        /// <summary>
        /// Gets the horizontal size of the extent.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Double" /> that represents, in device independent pixels,
        /// the horizontal size of the extent.
        /// </returns>
        double IScrollInfo.ExtentWidth
        {
            get
            {
                return _scrollExtent.Width;
            }
        }

        /// <summary>
        /// Gets the vertical size of the viewport for this content.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Double" /> that represents, in device independent pixels,
        /// the vertical size of the viewport for this content.
        /// </returns>
        double IScrollInfo.ViewportHeight
        {
            get
            {
                return _scrollViewport.Height;
            }
        }

        /// <summary>
        /// Gets the horizontal size of the viewport for this content.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Double" /> that represents, in device independent pixels,
        /// the horizontal size of the viewport for this content.
        /// </returns>
        double IScrollInfo.ViewportWidth
        {
            get
            {
                return _scrollViewport.Width;
            }
        }

        /// <summary>
        /// Scrolls up within content by one logical unit.
        /// </summary>
        void IScrollInfo.LineUp()
        {
            SetVerticalOffset(_scrollOffset - LineScrollValue, true);
        }

        /// <summary>
        /// Scrolls down within content by one logical unit.
        /// </summary>
        void IScrollInfo.LineDown()
        {
            SetVerticalOffset(_scrollOffset + LineScrollValue, true);
        }

        /// <summary>
        /// Scrolls up within content by one page.
        /// </summary>
        void IScrollInfo.PageUp()
        {
            SetVerticalOffset(_scrollOffset - GetPageScrollValue(), true);
        }

        /// <summary>
        /// Scrolls down within content by one page.
        /// </summary>
        void IScrollInfo.PageDown()
        {
            SetVerticalOffset(_scrollOffset + GetPageScrollValue(), true);
        }

        /// <summary>
        /// Scrolls up within content after a user clicks the wheel button on a mouse.
        /// </summary>
        void IScrollInfo.MouseWheelUp()
        {
            SetVerticalOffset(_scrollOffset - WheelScrollValue, true);
        }

        /// <summary>
        /// Scrolls down within content after a user clicks the wheel button on a mouse.
        /// </summary>
        void IScrollInfo.MouseWheelDown()
        {
            SetVerticalOffset(_scrollOffset + WheelScrollValue, true);
        }

        /// <summary>
        /// Scrolls left within content by one logical unit.
        /// </summary>
        void IScrollInfo.LineLeft()
        {
        }

        /// <summary>
        /// Scrolls right within content by one logical unit.
        /// </summary>
        void IScrollInfo.LineRight()
        {
        }

        /// <summary>
        /// Forces content to scroll until the coordinate space of a <see cref="T:System.Windows.Media.Visual" /> object is visible.
        /// </summary>
        /// <param name="visual">A <see cref="T:System.Windows.Media.Visual" /> that becomes visible.</param>
        /// <param name="rectangle">A bounding rectangle that identifies the coordinate space to make visible.</param>
        /// <returns>
        /// A <see cref="T:System.Windows.Rect" /> that is visible.
        /// </returns>
        Rect IScrollInfo.MakeVisible(System.Windows.Media.Visual visual, Rect rectangle)
        {
            var topDelta = visual.TransformToAncestor(this).Transform(rectangle.Location).Y;
            var bottomDelta = topDelta + rectangle.Height - _scrollViewport.Height;

            if (topDelta < 0)
            {
                // Top part is out of scroll
                SetVerticalOffset(_scrollOffset + topDelta, true);
            }
            else if (bottomDelta > 0)
            {
                // Bottom part is out of scroll
                SetVerticalOffset(_scrollOffset + bottomDelta, true);
            }
            return rectangle;
        }

        /// <summary>
        /// Scrolls left within content after a user clicks the wheel button on a mouse.
        /// </summary>
        void IScrollInfo.MouseWheelLeft()
        {
        }

        /// <summary>
        /// Scrolls right within content after a user clicks the wheel button on a mouse.
        /// </summary>
        void IScrollInfo.MouseWheelRight()
        {
        }

        /// <summary>
        /// Scrolls left within content by one page.
        /// </summary>
        void IScrollInfo.PageLeft()
        {
        }

        /// <summary>
        /// Scrolls right within content by one page.
        /// </summary>
        void IScrollInfo.PageRight()
        {
        }

        /// <summary>
        /// Sets the amount of horizontal offset.
        /// </summary>
        /// <param name="offset">The degree to which content is horizontally offset from the containing viewport.</param>
        void IScrollInfo.SetHorizontalOffset(double offset)
        {
        }

        /// <summary>
        /// Sets the amount of vertical offset.
        /// </summary>
        /// <param name="offset">The degree to which content is vertically offset from the containing viewport.</param>
        void IScrollInfo.SetVerticalOffset(double offset)
        {
            SetVerticalOffset(offset, true);
        }

        #endregion IScrollInfo Members
    }
}