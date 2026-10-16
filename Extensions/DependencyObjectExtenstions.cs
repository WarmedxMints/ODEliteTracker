using System.Windows;
using System.Windows.Media;

namespace ODEliteTracker.Extensions
{
    internal static class DependencyObjectExtenstions
    {
        /// <summary>
        /// Recursively finds the specified named parent in a control hierarchy
        /// </summary>
        /// <typeparam name="T">The type of the targeted Find</typeparam>
        /// <param name="child">The child control to start with</param>
        /// <param name="parentName">The name of the parent to find</param>
        /// <returns></returns>
        public static T? FindParentByType<T>(this DependencyObject child) where T : DependencyObject
        {
            if (child == null) return null;

            T? foundParent = null;
            var currentParent = VisualTreeHelper.GetParent(child);

            do
            {
                var frameworkElement = currentParent as FrameworkElement;
                if (frameworkElement is T)
                {
                    foundParent = (T)currentParent;
                    break;
                }

                currentParent = VisualTreeHelper.GetParent(currentParent);

            } while (currentParent != null);

            return foundParent;
        }

        public static T? FindChildByName<T>(this DependencyObject parent, string childName) where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            T? foundChild = null;
            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                var tmpFoundChild = GetChildByName<T>(childName, child);
                if (tmpFoundChild != null)
                {
                    foundChild = tmpFoundChild;
                    break;
                }
            }

            return foundChild;
        }

        private static T? GetChildByName<T>(string childName, DependencyObject child) where T : DependencyObject
        {
            // If the child is not of the request child type child
            if ((child is T childType) == false)
            {
                // recursively drill down the tree
                return FindChildByName<T>(child, childName);
            }

            if (string.IsNullOrEmpty(childName) == false)
            {
                // If the child's name is set for search
                if (child is FrameworkElement frameworkElement && frameworkElement.Name == childName)
                {
                    return (T)child;
                }

                return null;
            }

            // child element found.
            return (T)child;
        }

        public static T? FindChildByTag<T>(this DependencyObject parent, string tag) where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            T? foundChild = null;
            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                var tmpFoundChild = GetChildByTag<T>(tag, child);
                if (tmpFoundChild != null)
                {
                    foundChild = tmpFoundChild;
                    break;
                }
            }

            return foundChild;
        }

        private static T? GetChildByTag<T>(string tag, DependencyObject child) where T : DependencyObject
        {
            // If the child is not of the request child type child
            if ((child is T childType) == false)
            {
                // recursively drill down the tree
                return FindChildByTag<T>(child, tag);
            }

            if (string.IsNullOrEmpty(tag) == false)
            {
                // If the child's name is set for search
                if (child is FrameworkElement frameworkElement && frameworkElement.Tag is string t && t == tag)
                {
                    return (T)child;
                }

                return null;
            }

            // child element found.
            return (T)child;
        }
    }
}
