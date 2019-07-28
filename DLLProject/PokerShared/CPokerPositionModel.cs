using System;
using System.Collections.Generic;
using System.Text;

namespace PokerShared
{
    public static class CPokerPositionModel
    {
        public enum TwoMax
        {
            SB = 0,
            BB = 1
        }

        public enum ThreeMax
        {
            SB = 0,
            BB = 1,
            BTN = 9,
        }

        public enum FourMax
        {
            SB = 0,
            BB = 1,
            CO = 8,
            BTN = 9
        }

        public enum FiveMax
        {
            SB = 0,
            BB = 1,
            HJ = 7,
            CO = 8,
            BTN = 9
        }

        public enum SixMax
        {
            SB = 0,
            BB = 1,
            LJ = 6,
            HJ = 7,
            CO = 8,
            BTN = 9
        }

        public enum SevenMax
        {
            SB = 0,
            BB = 1,
            UTG = 2,
            LJ = 6,
            HJ = 7,
            CO = 8,
            BTN = 9
        }

        public enum EightMax
        {
            SB = 0,
            BB = 1,
            UTG = 2,
            UTG1 = 3,
            LJ = 6,
            HJ = 7,
            CO = 8,
            BTN = 9
        }

        public enum NineMax
        {
            SB = 0,
            BB = 1,
            UTG = 2,
            UTG1 = 3,         
            UTG2 = 4,
            LJ = 6,
            HJ = 7,
            CO = 8,
            BTN = 9
        }

        public enum TenMax
        {
            SB = 0,
            BB = 1,
            UTG = 2,
            UTG1 = 3,
            UTG2 = 4,
            UTG3 = 5,
            LJ = 6,
            HJ = 7,
            CO = 8,
            BTN = 9
        }
    }
}
