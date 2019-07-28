namespace Shared.Poker.Helpers
{
    /// <summary>
    /// Convert a range into all combinaisons possible
    /// </summary>
    public static class CDecimalHelper
    {
        private static readonly object lockObject = new object();

        public static double ToBB(this double _amountToConvert, double _bigBlind)
        {
            lock (lockObject)
            {
                return (_amountToConvert / _bigBlind);
            }
        }
    }
}
