using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace WebPad.Dependencies.General.Extensions.WPF
{

    public static class DependencyObjectExtensions
    {


        public static T GetValueThreadSafe<T>(this System.Windows.DependencyObject obj, System.Windows.DependencyProperty p)
        {
            return (T)obj.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                (System.Windows.Threading.DispatcherOperationCallback)delegate
                {
                    return obj.GetValue(p);
                }, p);
        }


        public static void SetValueThreadSafe(this System.Windows.DependencyObject obj, System.Windows.DependencyProperty p, object value)
        {
            obj.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
                (System.Threading.SendOrPostCallback)delegate
                {
                    obj.SetValue(p, value);
                }, value);
        }



        /// <summary>
        /// This works like FrameworkElement.FindName only in reverse up the tree instead of down
        ///         - http://msdn.microsoft.com/en-us/library/system.windows.frameworkelement.findname.aspx
        ///  
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static object FindNameUpTree(this System.Windows.DependencyObject child, string controlToFindName)
        {
            System.Windows.DependencyObject current = child;
            string currentName = null;

            do
            {
                current = current.GetParent();
                currentName = current.GetValue(System.Windows.Controls.Control.NameProperty) as string; // if it doesn't have one this should be null right???
            } while ((current != null) && !string.Equals(currentName, controlToFindName));

            return current;
        }


        public static System.Windows.FrameworkElement FindNameDownTree(this System.Windows.DependencyObject parent, string controlToFindName, List<System.Windows.FrameworkElement> matchingElements = null)
        {
            // Confirm parent and childName are valid. 
            if (parent == null || string.IsNullOrEmpty(controlToFindName)) return null;

            if (matchingElements == null) matchingElements = new List<System.Windows.FrameworkElement>(); // this is where we start out the success list

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is System.Windows.FrameworkElement && string.Equals((child as System.Windows.FrameworkElement).Name, controlToFindName))
                {
                    return child as System.Windows.FrameworkElement;
                }
                else
                {
                    // keep going down tree   
                    var result = FindNameDownTree(child, controlToFindName, matchingElements);

                    if (result != null)
                    {
                        matchingElements.Add(result);
                    }
                }

            }

            if (matchingElements.Any())
            {
                return matchingElements.First();
            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// Found problems with VisualTreeHelper.GetParent so researched and found this site: https://iimaginec.wordpress.com/2011/05/13/wpf-find-visual-tree-ancestor-by-type-the-better-way-of-getparent/
        /// It's got the below algorithm that accounts for alot more
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static DependencyObject GetParent(this DependencyObject obj)
        {
            if (obj == null)
                return null;

            ContentElement ce = obj as ContentElement;
            if (ce != null)
            {
                DependencyObject parent = ContentOperations.GetParent(ce);
                if (parent != null)
                    return parent;

                FrameworkContentElement fce = ce as FrameworkContentElement;
                return fce != null ? fce.Parent : null;
            }

            FrameworkElement fe = obj as FrameworkElement;
            if (fe != null)
            {
                DependencyObject parent = fe.Parent;
                if (parent != null)
                    return parent;
            }

            return System.Windows.Media.VisualTreeHelper.GetParent(obj);
        }


        /// <summary>
        /// Finds a parent of a given item on the visual tree.
        /// Example usage:
        /// Window owner = UIHelper.FindVisualParent<Window>(myControl);
        /// 
        /// Originally from: http://stackoverflow.com/questions/636383/how-can-i-find-wpf-controls-by-name-or-type
        /// </summary>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="child">A direct or indirect child of the queried item.</param>
        /// <returns>The first parent item that matches the submitted type parameter. 
        /// If not matching item can be found, a null reference is being returned.</returns>
        public static T FindVisualParent<T>(this DependencyObject child)
          where T : DependencyObject
        {
            // get parent item
            DependencyObject parentObject = child.GetParent();

            // we’ve reached the end of the tree
            if (parentObject == null) return null;

            // check if the parent matches the type we’re looking for
            T parent = parentObject as T;
            if (parent != null)
            {
                return parent;
            }
            else
            {
                // use recursion to proceed with next level
                return FindVisualParent<T>(parentObject);
            }
        }










    }
}
