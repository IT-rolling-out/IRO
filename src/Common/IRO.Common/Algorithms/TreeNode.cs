using System;
using System.Collections.Generic;
using System.Text;
using IRO.Common.Text;

namespace IRO.Common.Algorithms
{
    public class TreeNode<T> where T : class
    {
        public TreeNode<T> Parent { get; set; }

        public T Element { get; set; }

        bool _childrenWasRequested;
        List<TreeNode<T>> _children;
        public List<TreeNode<T>> Children
        {
            get
            {
                if (!_childrenWasRequested)
                {
                    _children = new List<TreeNode<T>>();
                    _childrenWasRequested = true;
                }
                return _children;
            }
            set
            {
                _children = value;
            }
        }

        public List<T> ToList()
        {
            var res = new List<T>();
            FillList(this, res);
            return res;
        }

        /// <summary>
        /// Visualize tree in text.
        /// </summary>
        public string ToString(Func<T, string> serializer = null)
        {
            if (serializer == null)
            {
                serializer = el => el.ToString();
            }
            var stringBuilder = new StringBuilder();
            FillStringVisualization(
                this,
                stringBuilder,
                serializer,
                0
                );
            return stringBuilder.ToString();

        }

        void FillStringVisualization(
            TreeNode<T> node,
            StringBuilder stringBuilder,
            Func<T, string> serializer,
            int tabsCount
            )
        {
            var nodeElStr = serializer(node.Element);
            nodeElStr = nodeElStr.AddTabs(tabsCount);
            stringBuilder.AppendLine(nodeElStr);
            foreach (var childNode in node.Children)
            {
                FillStringVisualization(childNode, stringBuilder, serializer, tabsCount + 1);
            }

        }


        void FillList(TreeNode<T> node, List<T> outList)
        {
            outList.Add(node.Element);
            foreach (var childNode in node.Children)
            {
                FillList(childNode, outList);
            }
        }
    }
}
