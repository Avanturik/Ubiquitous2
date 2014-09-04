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
    public class RichTextBoxAttached
    {
        /// <summary>
        /// The Document attached property's name.
        /// </summary>
        public const string DocumentPropertyName = "Document";

        /// <summary>
        /// Gets the value of the Document attached property 
        /// for a given dependency object.
        /// </summary>
        /// <param name="obj">The object for which the property value
        /// is read.</param>
        /// <returns>The value of the Document property of the specified object.</returns>
        public static FlowDocument GetDocument(DependencyObject obj)
        {
            return (FlowDocument)obj.GetValue(DocumentProperty);
        }

        /// <summary>
        /// Sets the value of the Document attached property
        /// for a given dependency object. 
        /// </summary>
        /// <param name="obj">The object to which the property value
        /// is written.</param>
        /// <param name="value">Sets the Document value of the specified object.</param>
        public static void SetDocument(DependencyObject obj, FlowDocument value)
        {
            obj.SetValue(DocumentProperty, value);
        }

        /// <summary>
        /// Identifies the Document attached property.
        /// </summary>
        public static readonly DependencyProperty DocumentProperty = DependencyProperty.RegisterAttached(
            DocumentPropertyName,
            typeof(FlowDocument),
            typeof(RichTextBoxAttached),
            new UIPropertyMetadata(null, (obj, e) => {
                if (e.NewValue == null)
                    return;

                RichTextBox richTextBox = obj as RichTextBox;
                richTextBox.Document = (FlowDocument)e.NewValue;
            }));
    }
}
