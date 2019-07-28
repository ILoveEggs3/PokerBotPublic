using System;
using System.Collections.Generic;
using System.Linq;

namespace Shared.Helpers
{
    public static class CListHelper
    {
        private static readonly object lockObject = new object();
        private static readonly object lockObject2 = new object();
        private static readonly object lockObject3 = new object();
        private static readonly object lockObject4 = new object();
        private static readonly object lockObject5 = new object();

        //Source: http://stackoverflow.com/questions/776725/list-get-next-element-or-get-the-first
        public static T ElemNextOf<T>(this IList<T> list, T item)
        {
            lock (lockObject)
            {
                return list[(list.IndexOf(item) + 1) == list.Count ? 0 : (list.IndexOf(item) + 1)];
            }            
        }

        public static IComparable<T> PremierElemQuiEstPlusGrandOuEgal<T>(IList<IComparable<T>> list, T item)
        {
            lock (lockObject2)
            {
                int indElem = 0;
                bool trouvePremierElemPlusGrand = false;

                while (!trouvePremierElemPlusGrand && indElem < list.Count)
                {
                    trouvePremierElemPlusGrand = (list[indElem].CompareTo(item) >= 0);
                    ++indElem;
                }

                if (trouvePremierElemPlusGrand)
                    return list[--indElem];
                else
                    return list.First();
            }
        }

        public static int IndexNext<T>(this IList<T> list, int index)
        {
            lock (lockObject3)
            {
                if (index == list.Count - 1)
                    return 0;
                else
                    return ++index;
            }
        }

        public static T ElemPrecedent<T>(this IList<T> list, T item)
        {
            lock (lockObject4)
            {
                return list[(list.IndexOf(item) - 1) == -1 ? list.Count - 1 : (list.IndexOf(item) - 1)];
            }            
        }

        public static int IndexPrecedent<T>(this IList<T> list, int index)
        {
            lock (lockObject5)
            {
                if (index == 0)
                    return (list.Count - 1);
                else
                    return --index;
            }
        }
    }
}
