using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace UB.Interactivity
{
    public class TextBlockAttached
    {
        private object lockInlines = new object();
        /// <summary>
        /// The InlineList attached property's name.
        /// </summary>
        public const string InlineListPropertyName = "InlineList";

        /// <summary>
        /// Gets the value of the InlineList attached property 
        /// for a given dependency object.
        /// </summary>
        /// <param name="obj">The object for which the property value
        /// is read.</param>
        /// <returns>The value of the InlineList property of the specified object.</returns>
        public static InlineCollection GetInlineList(DependencyObject obj)
        {
            return (InlineCollection)obj.GetValue(InlineListProperty);
        }

        /// <summary>
        /// Sets the value of the InlineList attached property
        /// for a given dependency object. 
        /// </summary>
        /// <param name="obj">The object to which the property value
        /// is written.</param>
        /// <param name="value">Sets the InlineList value of the specified object.</param>
        public static void SetInlineList(DependencyObject obj, InlineCollection value)
        {
            obj.SetValue(InlineListProperty, value);
        }

        /// <summary>
        /// Identifies the InlineList attached property.
        /// </summary>
        public static readonly DependencyProperty InlineListProperty = DependencyProperty.RegisterAttached(
            InlineListPropertyName,
            typeof(InlineCollection),
            typeof(TextBlockAttached),
            new UIPropertyMetadata(null, (obj,e) => {
                var textBlock = obj as TextBlock;
                var inlines = e.NewValue as InlineCollection;
                var oldInlines = e.OldValue as InlineCollection;
                if( inlines != null && oldInlines == null )
                {
                    textBlock.Inlines.Clear();
                    foreach (Inline inline in inlines.ToList())
                    {
                        textBlock.Inlines.Add(inline);

                    }
                }
            }));
    }
}
