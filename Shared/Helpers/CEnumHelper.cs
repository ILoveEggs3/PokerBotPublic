using System;
using System.Data;
using System.Data.SQLite;
using System.Data.SqlTypes;

namespace Shared.Helpers
{
    public static class CEnumHelper
    {
        private static readonly object lockObject = new object();

        public static T Next<T>(this T src) where T : struct
        {
            lock (lockObject)
            {
                T[] Arr = (T[])Enum.GetValues(src.GetType());
                int j = Array.IndexOf<T>(Arr, src) + 1;
                return (Arr.Length == j) ? Arr[0] : Arr[j];
            }
        }

        public static T GetFieldData<T>(this SQLiteDataReader _reader, int _index)
        {
            lock (lockObject)
            {
                if (_reader.IsDBNull(_index))
                    return default(T);
                else
                {
                    Type typeWithoutNullable = typeof(T);
                    bool typeIsNullable = false;

                    // If the type received is nullable, then we need to remove the nullable to verify if it's an enum later
                    if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        typeWithoutNullable = typeof(T).GetGenericArguments()[0];
                        typeIsNullable = true;
                    }                        

                    if (typeWithoutNullable.IsEnum)
                        return (T)(Enum.ToObject(typeWithoutNullable, _reader[_index]));
                    else
                    {
                        if (typeIsNullable)
                            return (T)Convert.ChangeType(_reader[_index], Nullable.GetUnderlyingType(typeof(T)));
                        else
                            return (T)Convert.ChangeType(_reader[_index], typeof(T));
                    }                        
                }
            }
        }
    }
}
