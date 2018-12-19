using ItRollingOut.Common.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ItRollingOut.MvcExceptionHandler.Services
{
    public class TypeInheritanceTreeBuilder
    {
        public static List<TreeNode<Type>> BuildTrees(IEnumerable<Type> types)
        {
            Func<Type, Type> getParent = (t) =>
            {
                var res = GetClosestParent(t, types);
                return res;
            };
            return TreeNodeAlgorithms.TreesFromEnumerable(types, getParent);
        }

        static Type GetFirstWithoutChildren(List<Type> onlyParentsTypes)
        {
            if (onlyParentsTypes.Count==0)
            {
                return null;
            }
            var firstType = onlyParentsTypes[0];
            bool firstHasChildren = false;
            for (int i = 1; i < onlyParentsTypes.Count; i++)
            {
                var item = onlyParentsTypes[i];
                if (firstType.IsAssignableFrom(item))
                {
                    firstHasChildren = true;
                    break;
                }
            }
            if (firstHasChildren)
            {
                onlyParentsTypes.RemoveAt(0);
                return GetFirstWithoutChildren(onlyParentsTypes);
            }
            else
            {
                return firstType;
            }
        }

        static Type GetClosestParent(Type childType, IEnumerable<Type> allTypes)
        {
            var onlyParentsTypes = allTypes.ToList();
            try
            {
                onlyParentsTypes.Remove(childType);
            }
            catch { }

            foreach (var t in allTypes)
            {
                if (!t.IsAssignableFrom(childType))
                {
                    onlyParentsTypes.Remove(t);
                }
            }
            return GetFirstWithoutChildren(onlyParentsTypes);
        }


    }



}
