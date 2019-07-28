using System;

namespace Amigo
{
    public static class CEnumHelper
    {
        private static readonly object lockObject = new object();

        public static T Next<T>(this T src) where T : struct
        {
            lock (lockObject)
            {
                if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argumnent {0} is not an Enum", typeof(T).FullName));

                T[] Arr = (T[])Enum.GetValues(src.GetType());
                int j = Array.IndexOf<T>(Arr, src) + 1;
                return (Arr.Length == j) ? Arr[0] : Arr[j];
            }
        }
    }
}
