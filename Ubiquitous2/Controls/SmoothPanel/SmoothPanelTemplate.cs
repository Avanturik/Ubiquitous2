// --------------------------------------------------------------------------
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// --------------------------------------------------------------------------

using System;
using System.Windows;

namespace Devart.Controls
{
    /// <summary>
    /// Maps type of view model to type of visual element.
    /// </summary>
    public class SmoothPanelTemplate : DependencyObject
    {
        /// <summary>
        /// Using a DependencyProperty as the backing store for ViewModel.
        /// </summary>
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            "ViewModel",
            typeof(Type),
            typeof(SmoothPanelTemplate),
            new PropertyMetadata(null, null, ViewModelChanging));

        /// <summary>
        /// Using a DependencyProperty as the backing store for View.
        /// </summary>
        public static readonly DependencyProperty ViewProperty = DependencyProperty.Register(
            "View",
            typeof(Type),
            typeof(SmoothPanelTemplate),
            new PropertyMetadata(null, null, ViewChanging));

        /// <summary>
        /// Gets or sets the type of view model.
        /// </summary>
        /// <value>
        /// The type of view model.
        /// </value>
        public Type ViewModel
        {
            get
            {
                return (Type)GetValue(ViewModelProperty);
            }
            set
            {
                SetValue(ViewModelProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the type of view.
        /// </summary>
        /// <value>
        /// The type of view.
        /// </value>
        public Type View
        {
            get
            {
                return (Type)GetValue(ViewProperty);
            }
            set
            {
                SetValue(ViewProperty, value);
            }
        }

        /// <summary>
        /// Called whenever a ViewModel property value is being re-evaluated, or coercion is specifically requested.
        /// </summary>
        /// <param name="d">The dependency object.</param>
        /// <param name="baseValue">The base value.</param>
        /// <returns>The coerced value (with appropriate type).</returns>
        private static object ViewModelChanging(DependencyObject d, object baseValue)
        {
            var type = baseValue as Type;
            if (type == null)
            {
                throw new ArgumentException("The type of ViewModel should be specified.");
            }
            return type;
        }

        /// <summary>
        /// Called whenever a View property value is being re-evaluated, or coercion is specifically requested.
        /// </summary>
        /// <param name="d">The dependency object.</param>
        /// <param name="baseValue">The base value.</param>
        /// <returns>The coerced value (with appropriate type).</returns>
        private static object ViewChanging(DependencyObject d, object baseValue)
        {
            var type = baseValue as Type;
            if (type == null || !typeof(FrameworkElement).IsAssignableFrom(type))
            {
                throw new ArgumentException("The type of View should be inherited from FrameworkElement.");
            }
            return type;
        }
    }
}