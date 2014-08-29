// --------------------------------------------------------------------------
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// --------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;

namespace Devart.Controls
{
    /// <content>
    /// Nested type is used.
    /// </content>
    public partial class SmoothPanel
    {
        /// <summary>
        /// Temporary instances of this type are used for measuring in
        /// <see cref="M:SmoothPanel.MeasureOverride"/> method.
        /// </summary>
        private class SmoothPanelMeasurer
        {
            /// <summary>
            /// The panel.
            /// </summary>
            private readonly SmoothPanel _panel;

            /// <summary>
            /// The collection of children.
            /// </summary>
            private readonly SmoothPanelChildren _children;

            /// <summary>
            /// The view model collection.
            /// </summary>
            private readonly System.Collections.IList _items;

            /// <summary>
            /// The available size.
            /// </summary>
            private readonly Size _availableSize;

            /// <summary>
            /// Value indicating whether first visible item should remain in its position or first
            /// item should be based on scroll position.
            /// </summary>
            private bool _keepFirstItem;

            /// <summary>
            /// The total estimated height of all visual elements.
            /// </summary>
            private double _totalHeight;

            /// <summary>
            /// The last visible item index.
            /// </summary>
            private int _lastItemIndex;

            /// <summary>
            /// The ratio of clipped part of last visible item to its height.
            /// </summary>
            private double _lastItemClippedRatio;

            /// <summary>
            /// Initializes a new instance of the <see cref="SmoothPanelMeasurer"/> class.
            /// </summary>
            /// <param name="panel">The panel.</param>
            /// <param name="availableSize">The available size.</param>
            public SmoothPanelMeasurer(SmoothPanel panel, Size availableSize)
            {
                _panel = panel;
                _children = panel._children;
                _items = _children.GetItems();
                _keepFirstItem = FirstItemIndex >= 0;
                _lastItemIndex = -1;
                _availableSize = availableSize;
            }

            /// <summary>
            /// Gets the index of first visible item.
            /// </summary>
            /// <value>
            /// The index of first visible item.
            /// </value>
            private int FirstItemIndex
            {
                get
                {
                    return _panel._firstItemIndex;
                }
            }

            /// <summary>
            /// Gets the ratio of clipped part of last visible item to its height.
            /// </summary>
            /// <value>
            /// The ratio of clipped part of last visible item to its height.
            /// </value>
            private double FirstItemClippedRatio
            {
                get
                {
                    return _panel._firstItemClippedRatio;
                }
            }

            /// <summary>
            /// Gets or sets the reverse vertical offset of the scrolled content.
            /// </summary>
            /// <value>
            /// The reverse vertical scroll offset.
            /// </value>
            private double ReverseScrollOffset
            {
                get
                {
                    return _panel._scrollExtent.Height - _availableSize.Height - _panel._scrollOffset;
                }
                set
                {
                    double offset = _panel._scrollExtent.Height - _availableSize.Height - value;
                    _panel.SetVerticalOffset(offset, false);
                }
            }

            /// <summary>
            /// Gets or sets the vertical offset of the scrolled content.
            /// </summary>
            /// <value>
            /// The vertical scroll offset.
            /// </value>
            private double ScrollOffset
            {
                get
                {
                    return _panel._scrollOffset;
                }
                set
                {
                    _panel.SetVerticalOffset(value, false);
                }
            }

            /// <summary>
            /// Measures child elements and prepares panel layout.
            /// </summary>
            public void Measure()
            {
                // Some unexpected artifacts are better than infinite loop.
                bool lastChance = false;
                int lastIndex;
                do
                {
                    if (_items == null || _items.Count == 0)
                    {
                        // No items - do nothing
                        _panel.UpdateScrollInfo(_availableSize, 0);
                        return;
                    }

                    _children.CreateTopmostElements(_availableSize);

                    _totalHeight = GetTotalHeight(out _lastItemIndex, out _lastItemClippedRatio);

                    if (!_keepFirstItem && _lastItemIndex >= 0)
                    {
                        // Get first visible item by last one
                        GetFirstItem();
                    }

                    if (FirstItemIndex < 0)
                    {
                        Debug.Assert(false, "First visible item should be determined");

                        // Some unexpected result - just reset to top.
                        _panel.SetFirstVisibleItem(0, 0);
                        _panel.SetVerticalOffset(0, false);
                    }

                    // Generate all visible items.
                    double bottom;
                    GenerateChildren(out lastIndex, out bottom);

                    bool scrollChanged = false;

                    double extent = _panel._scrollExtent.Height;
                    double viewPort = _panel._scrollViewport.Height;

                    if (extent < viewPort)
                    {
                        if (FirstItemIndex != 0 || FirstItemClippedRatio != 0 || ScrollOffset != 0)
                        {
                            // Scrollbar disappears
                            _panel.SetFirstVisibleItem(0, 0);
                            _panel.SetVerticalOffset(0, false);
                            scrollChanged = true;
                        }
                    }
                    else
                    {
                        if (_lastItemIndex < 0)
                        {
                            if (extent > viewPort)
                            {
                                // Prevent precision problems
                                if (Math.Abs(bottom - viewPort) >= 0.5)
                                {
                                    ReverseScrollOffset = 0;
                                    _keepFirstItem = false;
                                    scrollChanged = true;
                                }
                            }
                        }
                        else if (_keepFirstItem)
                        {
                            // Scroll position can be changed when first visible item is fixed.
                            double newReverseScrollOffset = 0;
                            if (lastIndex < _items.Count)
                            {
                                newReverseScrollOffset = _lastItemClippedRatio * _children.GetEstimatedHeight(lastIndex);
                                for (int i = lastIndex + 1; i < _items.Count; i++)
                                {
                                    newReverseScrollOffset += _children.GetEstimatedHeight(i);
                                }
                            }

                            double oldScrollOffset = ScrollOffset;
                            ReverseScrollOffset = newReverseScrollOffset;
                            scrollChanged = oldScrollOffset != ScrollOffset;
                        }
                    }

                    if (!scrollChanged)
                    {
                        break;
                    }

                    // Recalculate positions if scroll changed.
                    lastChance = !lastChance;
                    Debug.Assert(lastChance, "SmoothPanel measuring failed.");
                }
                while (lastChance);

                // Remove odd children.
                _children.TrimElements(FirstItemIndex, lastIndex);
            }

            /// <summary>
            /// Generates the children.
            /// </summary>
            /// <param name="lastIndex">The last index.</param>
            /// <param name="bottom">The bottom.</param>
            private void GenerateChildren(out int lastIndex, out double bottom)
            {
                _lastItemIndex = -1;
                double top = 0;
                int itemIndex;
                for (itemIndex = FirstItemIndex; itemIndex < _items.Count; itemIndex++)
                {
                    var child = _children.GetMeasuredChild(_items, itemIndex);

                    if (itemIndex == FirstItemIndex)
                    {
                        top = -child.DesiredSize.Height * FirstItemClippedRatio;
                    }

                    double childHeight = child.DesiredSize.Height;
                    top += childHeight;
                    if (top >= _availableSize.Height)
                    {
                        _lastItemIndex = itemIndex;
                        _lastItemClippedRatio = (top - _availableSize.Height) / childHeight;
                        break;
                    }
                }
                bottom = top;
                lastIndex = itemIndex;
            }

            /// <summary>
            /// Gets the first item by last one.
            /// </summary>
            private void GetFirstItem()
            {
                double bottomHeight = 0;
                for (int i = _lastItemIndex; i >= 0; i--)
                {
                    var child = _children.GetMeasuredChild(_items, i);
                    double childHeight = child.DesiredSize.Height;
                    if (i == _lastItemIndex)
                    {
                        // Consider clipping part
                        bottomHeight = -childHeight * _lastItemClippedRatio;
                    }
                    bottomHeight += childHeight;
                    if (bottomHeight >= _availableSize.Height)
                    {
                        // All above elements are out of visible area
                        _panel.SetFirstVisibleItem(i, (bottomHeight - _availableSize.Height) / childHeight);
                        return;
                    }
                }

                _panel.SetFirstVisibleItem(0, 0);
            }

            /// <summary>
            /// Gets the total height.
            /// </summary>
            /// <param name="lastItemIndex">Last visible item index.</param>
            /// <param name="lastItemClippedRatio">The ratio of clipped part of last visible item to its height.</param>
            /// <returns>A <see cref="double"/> that represents a total estimated height of all children.</returns>
            private double GetTotalHeight(out int lastItemIndex, out double lastItemClippedRatio)
            {
                double totalHeight = 0;
                if (_keepFirstItem)
                {
                    // No need to determine last visible item
                    for (int i = 0; i < _items.Count; i++)
                    {
                        double itemHeight = _children.GetEstimatedHeight(i);
                        totalHeight += itemHeight;
                    }
                    _panel.UpdateScrollInfo(_availableSize, totalHeight);

                    lastItemIndex = -1;
                    lastItemClippedRatio = 0;
                    return totalHeight;
                }

                bool lastChance = false;
                do
                {
                    totalHeight = 0;
                    lastItemIndex = -1;
                    lastItemClippedRatio = 0;

                    double reverseScrollOffset = ReverseScrollOffset;

                    // Determine last visible item
                    for (int i = _items.Count - 1; i >= 0; i--)
                    {
                        double itemHeight = _children.GetEstimatedHeight(i);
                        totalHeight += itemHeight;
                        if (lastItemIndex < 0 && totalHeight > reverseScrollOffset)
                        {
                            lastItemIndex = i;
                            lastItemClippedRatio = Math.Max(0, 1 - ((totalHeight - reverseScrollOffset) / itemHeight));
                        }
                    }

                    // Scrolled to bottom
                    if (lastItemIndex < 0)
                    {
                        lastItemIndex = _items.Count - 1;
                        lastItemClippedRatio = 0;
                    }

                    var oldScrollOffset = ScrollOffset;
                    _panel.UpdateScrollInfo(_availableSize, totalHeight);
                    if (oldScrollOffset == _panel._scrollOffset)
                    {
                        break;
                    }
                    lastChance = !lastChance;
                    Debug.Assert(lastChance, "Total height is not determined.");
                }
                while (lastChance);

                return totalHeight;
            }
        }
    }
}