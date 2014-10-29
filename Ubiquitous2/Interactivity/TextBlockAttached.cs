using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using UB.Utils;

namespace UB.Interactivity
{
    public class TextBlockAttached
    {
        private static object lockInlines = new object();

        /// <summary>
        /// The InlineListPrefix attached property's name.
        /// </summary>
        public const string InlineListPrefixPropertyName = "InlineListPrefix";

        /// <summary>
        /// Gets the value of the InlineListPrefix attached property 
        /// for a given dependency object.
        /// </summary>
        /// <param name="obj">The object for which the property value
        /// is read.</param>
        /// <returns>The value of the InlineListPrefix property of the specified object.</returns>
        public static InlineCollection GetInlineListPrefix(DependencyObject obj)
        {
            return (InlineCollection)obj.GetValue(InlineListPrefixProperty);
        }

        /// <summary>
        /// Sets the value of the InlineListPrefix attached property
        /// for a given dependency object. 
        /// </summary>
        /// <param name="obj">The object to which the property value
        /// is written.</param>
        /// <param name="value">Sets the InlineListPrefix value of the specified object.</param>
        public static void SetInlineListPrefix(DependencyObject obj, InlineCollection value)
        {
            obj.SetValue(InlineListPrefixProperty, value);
        }

        /// <summary>
        /// Identifies the InlineListPrefix attached property.
        /// </summary>
        public static readonly DependencyProperty InlineListPrefixProperty = DependencyProperty.RegisterAttached(
            InlineListPrefixPropertyName,
            typeof(InlineCollection),
            typeof(TextBlockAttached),
            new UIPropertyMetadata(default(InlineCollection), (obj, e) => {

                if (obj == null)
                    return;
                var textBlock = obj as TextBlock;
                var prefixInlines = e.NewValue as InlineCollection;
                if (prefixInlines == null)
                    return;

                var mainInlines = GetInlineList(textBlock);

                FillInlines(textBlock, prefixInlines, mainInlines);
            }));
        
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
                if (obj == null)
                    return;
                var textBlock = obj as TextBlock;
                var mainInlines = e.NewValue as InlineCollection;
                if (mainInlines == null)
                    return;

                var prefixInlines = GetInlineListPrefix(textBlock);

                FillInlines(textBlock, prefixInlines, mainInlines);

            }));


        private static void FillInlines( TextBlock textBlock, InlineCollection prefixInlines, InlineCollection mainInlines )
        {
            var textBlockMain = new TextBlock();
            var textBlockPrefix = new TextBlock();

            if( prefixInlines == null || prefixInlines.Count <= 0 || mainInlines == null || mainInlines.Count <= 0 )
            {
                foreach (Inline inline in textBlock.Inlines.ToList())
                {
                    var inlineKind = (inline.Tag as string);
                    switch( inlineKind )
                    {
                        case "main":
                            if (mainInlines == null || mainInlines.Count <= 0)
                                textBlockMain.Inlines.Add(inline);
                            break;
                        case "prefix":
                            if (prefixInlines == null || prefixInlines.Count <= 0)
                                textBlockPrefix.Inlines.Add(inline);
                            break;
                    }

                }
            }
            
            textBlock.Inlines.Clear();

            if (prefixInlines != null && prefixInlines.Count > 0)
                foreach (Inline inline in prefixInlines.ToList())
                {
                    inline.Tag = "prefix";
                    textBlock.Inlines.Add(inline);
                }
            else
                textBlock.Inlines.AddRange(textBlockPrefix.Inlines.ToList());

            if (mainInlines != null && mainInlines.Count > 0)
                foreach (Inline inline in mainInlines.ToList())
                {
                    inline.Tag = "main";
                    textBlock.Inlines.Add(inline);
                }
            else
                textBlock.Inlines.AddRange(textBlockMain.Inlines.ToList());
        }

    }
}
