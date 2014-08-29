// --------------------------------------------------------------------------
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// --------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Devart.Controls
{
    /// <content>
    /// Nested type is used.
    /// </content>
    public partial class SmoothPanel
    {
        /// <summary>
        /// Manages the child visual elements.
        /// </summary>
        private class SmoothPanelChildren
        {
            /// <summary>
            /// The default item height.
            /// </summary>
            private const double DefaultItemHeight = 150;

            /// <summary>
            /// The <see cref="SmoothPanel"/>.
            /// </summary>
            private readonly SmoothPanel _panel;

            /// <summary>
            /// The last collection for which measuring is performed.
            /// </summary>
            private System.Collections.IList _items;

            /// <summary>
            /// The count of permanently created elements from which a real but not an estimated height is used.
            /// </summary>
            private int _topmostElementsCount;

            /// <summary>
            /// The array contains created elements or <c>null</c>.
            /// </summary>
            private FrameworkElement[] _elementsByIndex = new FrameworkElement[0];

            /// <summary>
            /// The collection of element caches.
            /// </summary>
            private Dictionary<Type, SmoothPanelViewCache> _viewModelToView;

            /// <summary>
            /// Initializes a new instance of the <see cref="SmoothPanelChildren"/> class.
            /// </summary>
            /// <param name="panel">The panel.</param>
            public SmoothPanelChildren(SmoothPanel panel)
            {
                _panel = panel;
            }

            /// <summary>
            /// Gets the number of items from the last collection for which measuring is performed.
            /// </summary>
            /// <value>
            /// The number of items from the last collection for which measuring is performed.
            /// </value>
            public int ItemsCount
            {
                get
                {
                    return _items == null ? 0 : _items.Count;
                }
            }

            /// <summary>
            /// Gets or sets the available width.
            /// </summary>
            /// <value>
            /// The available width.
            /// </value>
            public double AvailableWidth
            {
                get;
                set;
            }

            /// <summary>
            /// Gets a collection of child elements
            /// </summary>
            /// <value>
            /// A <see cref="UIElementCollection"/> of child elements.
            /// </value>
            private UIElementCollection Children
            {
                get
                {
                    return _panel.InternalChildren;
                }
            }

            /// <summary>
            /// Gets the element.
            /// </summary>
            /// <param name="index">The index.</param>
            /// <returns>A <see cref="FrameworkElement"/> or <c>null</c>.</returns>
            public FrameworkElement GetElement(int index)
            {
                if (index < 0 || index >= _elementsByIndex.Length)
                {
                    Debug.Assert(false, "Element is out of range");
                    return null;
                }

                return _elementsByIndex[index];
            }

            /// <summary>
            /// Gets the height of the estimated.
            /// </summary>
            /// <param name="itemIndex">Index of the item.</param>
            /// <returns>A <see cref="Double"/> that represents an estimated height.</returns>
            public double GetEstimatedHeight(int itemIndex)
            {
                if (itemIndex < _topmostElementsCount)
                {
                    // Use real height for first items.
                    var element = GetElement(itemIndex);
                    if (element != null)
                    {
                        return element.DesiredSize.Height;
                    }
                }

                // Get estimated height if ViewModel implements IHeightMeasurer
                var model = _items[itemIndex] as IHeightMeasurer;
                if (model != null)
                {
                    return model.GetEstimatedHeight(AvailableWidth);
                }

                return DefaultItemHeight;
            }

            /// <summary>
            /// Resets the items.
            /// </summary>
            public void ResetItems()
            {
                if (Children.Count > 0)
                {
                    // Remove all visual elements
                    _panel.RemoveInternalChildRange(0, Children.Count);
                }

                foreach (var element in _elementsByIndex)
                {
                    if (element != null)
                    {
                        // Return elements to cache
                        var viewCache = GetViewCache(element.DataContext);
                        if (viewCache != null)
                        {
                            viewCache.ReturnElement(element);
                        }
                    }
                }

                _elementsByIndex = new FrameworkElement[_items != null ? _items.Count : 0];
                _topmostElementsCount = 0;
            }

            /// <summary>
            /// Gets the measured child.
            /// </summary>
            /// <param name="items">The items collection.</param>
            /// <param name="index">The item index.</param>
            /// <returns>A <see cref="FrameworkElement"/> that already added and measured.</returns>
            public FrameworkElement GetMeasuredChild(System.Collections.IList items, int index)
            {
                FrameworkElement element = null;
                DataBindHelper.PerformWithInstantBinding(() =>
                {
                    element = _elementsByIndex[index];
                    if (element == null)
                    {
                        // Get or create element from cache
                        var item = items[index];
                        var viewCache = GetViewCache(item);
                        element = viewCache.GetElement();
                        element.DataContext = item;
                        _elementsByIndex[index] = element;
                    }

                    if (!Children.Contains(element))
                    {
                        // Determine index to insert in Children collection.
                        int visualElementIndex = Children.Count;
                        var lastCreatedIndex = GetItemIndex(items, visualElementIndex - 1);

                        if (index < lastCreatedIndex)
                        {
                            for (visualElementIndex = 0; visualElementIndex < Children.Count; visualElementIndex++)
                            {
                                if (GetItemIndex(items, visualElementIndex) > index)
                                {
                                    break;
                                }
                            }
                        }

                        // Add child element to panel.
                        _panel.InsertInternalChild(visualElementIndex, element);
                    }
                });

                // Measure and return element
                element.Measure(new Size(AvailableWidth, double.PositiveInfinity));
                return element;
            }

            /// <summary>
            /// Gets the items collection bound to <see cref="SmoothPanel"/>.
            /// </summary>
            /// <returns>A <see cref="System.Collections.IList"/>.</returns>
            public System.Collections.IList GetItems()
            {
                System.Collections.IList items = null;
                ItemsControl itemsControl = ItemsControl.GetItemsOwner(_panel);
                if (itemsControl != null)
                {
                    items = itemsControl.Items;
                }

                // Reset items if items collection replaced.
                if (_items != items)
                {
                    _items = items;
                    ResetItems();
                }
                return _items;
            }

            /// <summary>
            /// Creates the topmost elements.
            /// </summary>
            /// <param name="availableSize">The available size.</param>
            public void CreateTopmostElements(Size availableSize)
            {
                var items = GetItems();
                double firstItemsHeight = 0;
                _topmostElementsCount = 0;

                // Get first items until them fitted in available area
                for (int i = 0; i < items.Count; i++)
                {
                    var child = GetMeasuredChild(items, i);
                    _topmostElementsCount++;
                    firstItemsHeight += child.DesiredSize.Height;
                    if (firstItemsHeight >= availableSize.Height)
                    {
                        break;
                    }
                }
            }

            /// <summary>
            /// Trims odd child elements.
            /// </summary>
            /// <param name="firstVisibleIndex">The first visible item index.</param>
            /// <param name="lastVisibleIndex">The last visible item index.</param>
            public void TrimElements(int firstVisibleIndex, int lastVisibleIndex)
            {
                for (int itemIndex = 0; itemIndex < _elementsByIndex.Length; itemIndex++)
                {
                    var element = _elementsByIndex[itemIndex];

                    if (element != null)
                    {
                        if (itemIndex < firstVisibleIndex || itemIndex > lastVisibleIndex)
                        {
                            // Do not delete topmost elements.
                            bool deleteControl = itemIndex >= _topmostElementsCount;
                            if (deleteControl)
                            {
                                _elementsByIndex[itemIndex] = null;
                            }

                            // Remove all odd children
                            var index = Children.IndexOf(element);
                            if (index >= 0)
                            {
                                _panel.RemoveInternalChildRange(index, 1);
                                if (deleteControl)
                                {
                                    if (element.DataContext != null)
                                    {
                                        var viewCache = GetViewCache(element.DataContext);
                                        viewCache.ReturnElement(element);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// Resets the view caches.
            /// </summary>
            public void ResetViewCaches()
            {
                _viewModelToView = null;
            }

            /// <summary>
            /// Gets the item index from visual element index.
            /// </summary>
            /// <param name="items">The items.</param>
            /// <param name="visualElementIndex">Index of the visual element.</param>
            /// <returns>An index in items collection or <c>-1</c>.</returns>
            private int GetItemIndex(System.Collections.IList items, int visualElementIndex)
            {
                if (items != null && Children.Count > 0 && visualElementIndex >= 0 && visualElementIndex < Children.Count)
                {
                    var element = Children[visualElementIndex] as FrameworkElement;
                    if (element != null)
                    {
                        return items.IndexOf(element.DataContext);
                    }
                }
                return -1;
            }

            /// <summary>
            /// Gets the view cache.
            /// </summary>
            /// <param name="viewModel">The view model.</param>
            /// <returns>A <see cref="SmoothPanelViewCache"/> or <c>null</c>.</returns>
            private SmoothPanelViewCache GetViewCache(object viewModel)
            {
                if (viewModel == null)
                {
                    return null;
                }

                Type viewModelType = viewModel.GetType();

                if (_viewModelToView == null)
                {
                    // Create cache for each template
                    _viewModelToView = new Dictionary<Type, SmoothPanelViewCache>();
                    foreach (var template in _panel.Templates)
                    {
                        _viewModelToView.Add(template.ViewModel, new SmoothPanelViewCache(template.View));
                    }
                }

                // Get cache by view model type.
                SmoothPanelViewCache result;
                _viewModelToView.TryGetValue(viewModelType, out result);
                return result;
            }
        }
    }
}