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
using System.Windows;

namespace Devart.Controls
{
    /// <content>
    /// Nested type is used.
    /// </content>
    public partial class SmoothPanel
    {
        /// <summary>
        /// A cache of visual elements.
        /// </summary>
        private class SmoothPanelViewCache
        {
            /// <summary>
            /// The type of visual elements.
            /// </summary>
            private readonly Type _viewType;

            /// <summary>
            /// The weak references to cached visual elements
            /// </summary>
            private readonly List<WeakReference> _cachedElements;

            /// <summary>
            /// Initializes a new instance of the <see cref="SmoothPanelViewCache"/> class.
            /// </summary>
            /// <param name="viewType">The type of visual elements.</param>
            public SmoothPanelViewCache(Type viewType)
            {
                if (viewType == null || !typeof(FrameworkElement).IsAssignableFrom(viewType))
                {
                    throw new ArgumentException("The type of View should be inherited from FrameworkElement.");
                }

                _viewType = viewType;
                _cachedElements = new List<WeakReference>();
            }

            /// <summary>
            /// Gets the element from cache or create new element.
            /// </summary>
            /// <returns>A <see cref="FrameworkElement"/> instance.</returns>
            public FrameworkElement GetElement()
            {
                while (_cachedElements.Count > 0)
                {
                    var elementRef = _cachedElements[0];
                    var element = elementRef.Target as FrameworkElement;

                    // Remove both alive or not references
                    _cachedElements.RemoveAt(0);

                    if (element != null)
                    {
                        return element;
                    }
                }

                // Create new instance.
                return Activator.CreateInstance(_viewType) as FrameworkElement;
            }

            /// <summary>
            /// Returns the element to cache.
            /// </summary>
            /// <param name="element">The element.</param>
            internal void ReturnElement(FrameworkElement element)
            {
                element.DataContext = null;
                _cachedElements.Add(new WeakReference(element));
            }
        }
    }
}