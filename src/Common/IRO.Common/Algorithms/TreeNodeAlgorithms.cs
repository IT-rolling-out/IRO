using System;
using System.Collections.Generic;

namespace IRO.Common.Algorithms
{
    public class TreeNodeAlgorithms
    {
        /// <summary>
        /// Return all trees without parent (top level trees).
        /// </summary>
        public static List<TreeNode<T>> TreesFromEnumerable<T>(IEnumerable<T> enumerable, Func<T, T> getParent)
            where T : class
        {
            var elementToNodeDict = new Dictionary<T, TreeNode<T>>();
            foreach (var el in enumerable)
            {
                if (elementToNodeDict.ContainsKey(el))
                    continue;
                var treeNode = new TreeNode<T>()
                {
                    Element = el
                };
                elementToNodeDict.Add(el, treeNode);
            }

            foreach (var item in elementToNodeDict)
            {
                var treeNode = item.Value;
                var parentEl = getParent(treeNode.Element);
                TreeNode<T> parentNode = null;
                if(parentEl!=null)
                    elementToNodeDict.TryGetValue(parentEl, out parentNode);
                if (parentNode != null)
                {
                    treeNode.Parent = parentNode;
                    parentNode.Children.Add(treeNode);
                }
            }

            var topLevelTrees = new List<TreeNode<T>>();
            foreach (var item in elementToNodeDict)
            {
                var treeNode = item.Value;
                if (treeNode.Parent == null)
                {
                    topLevelTrees.Add(treeNode);
                }
            }
            return topLevelTrees;
        }
    }

}
