using Amigo.Models;
using System;

namespace Amigo.Helpers
{
    public static class CStringHelper
    {
        private static readonly object lockObject = new object();

        public static CCard ToCCarte(this string chaineActuel)
        {
            lock (lockObject)
            {
                if ((chaineActuel.Length == 2) &&
                    (Enum.IsDefined(typeof(CCard.Value), (int)chaineActuel[0]) && Enum.IsDefined(typeof(CCard.Type), (int)chaineActuel[1])))
                    return new CCard((CCard.Value)chaineActuel[0], (CCard.Type)chaineActuel[1]);
                else
                    throw new ArgumentException();
            }
        }
    }
}
