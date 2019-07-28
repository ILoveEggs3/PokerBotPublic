using HoldemHand;
using System;
using System.Collections.Generic;

namespace Shared.Models.Database
{
    public class CHandModel
    {
        public const byte IS_BACK_DOOR_FLUSH_DRAW_MASK     = 0b1 << 0;
        public const byte IS_BACK_DOOR_STRAIGHT_DRAW_MASK  = 0b1 << 1;
        public const byte IS_FLUSH_DRAW_MASK               = 0b1 << 2;
        public const byte IS_STRAIGHT_DRAW_MASK            = 0b1 << 3;
        public const byte INDEX_HIGH_CARD_MASK             = 0b1111 << 4;
        public const int NB_OUT_OFFSET                     = 56;
        public const ulong OUTS_MASK                       = (ulong.MaxValue << (64 - NB_OUT_OFFSET)) >> (64 - NB_OUT_OFFSET);
        public const ulong NB_OUT_MASK                     = (ulong)0xFF << NB_OUT_OFFSET;

        private ulong FFRefPocketMask;
        public ulong PRefPocketMask
        {
            get { return FFRefPocketMask; }
            set
            {
                FFRefPocketMask = value;
                CalculateHandStrength(PRefPocketMask, PRefBoardMask);
                CalculateOuts(PRefPocketMask, PRefBoardMask);
            }
        }
        private ulong FFRefBoardMask;
        public ulong PRefBoardMask
        {
            get { return FFRefBoardMask; }
            set
            {
                FFRefBoardMask = value;
                CalculateHandStrength(PRefPocketMask, PRefBoardMask);
                CalculateOuts(PRefPocketMask, PRefBoardMask);
            }
        }
        public double PHandStrength { get; private set; }
        private ulong FFOuts;
        public ulong POutsMask
        {
            get
            {
                return FFOuts & OUTS_MASK;
            }
        }
        public int PNbOuts
        {
            get
            {
                return (int)(FFOuts >> NB_OUT_OFFSET);
            }
        }
        public byte PMetaData;
        public bool PIsBackDoorFlushDraw
        {
            get
            {
                return (PMetaData & IS_BACK_DOOR_FLUSH_DRAW_MASK) != 0;
            }
        }
        public bool PIsBackDoorStraightDraw
        {
            get
            {
                return (PMetaData & IS_BACK_DOOR_STRAIGHT_DRAW_MASK) != 0;
            }
        }
        public bool PIsFlushDraw
        {
            get
            {
                return (PMetaData & IS_FLUSH_DRAW_MASK) != 0;
            }
        }
        public bool PIsStraightDraw
        {
            get
            {
                return (PMetaData & IS_STRAIGHT_DRAW_MASK) != 0;
            }
        }
        public int PIndexHighCard
        {
            get
            {
                return PMetaData >> 4;
            }
        }

        public CHandModel(ulong _refPocketMask, ulong _refBoardMask)
        {
            FFRefPocketMask = _refPocketMask;
            FFRefBoardMask = _refBoardMask;
            PHandStrength = CalculateHandStrength(PRefPocketMask, PRefBoardMask);
            FFOuts = CalculateOuts(PRefPocketMask, PRefBoardMask);
            PMetaData = CalculateMetaData(_refPocketMask, _refBoardMask);
        }

        public CHandModel(ulong _refPocketMask, ulong _refBoardMask, double _handStrength, ulong _outsMask, byte _metaData)
        {
            FFRefPocketMask = _refPocketMask;
            FFRefBoardMask = _refBoardMask;
            PHandStrength = _handStrength;
            FFOuts = _outsMask;
            PMetaData = _metaData;
        }

        public static double CalculateHandStrength(ulong _pocketMask, ulong _boardMask)
        {
            return Hand.HandStrength(_pocketMask, _boardMask);
        }

        public static ulong CalculateOuts(ulong _pocketMask, ulong _boardMask)
        {
            if (Hand.BitCount(_boardMask) == 5)
                return 0;
            
            var retMask = ulong.MinValue;

            var outsMask = 0UL;

            outsMask = Hand.OutsMaskDiscounted(_pocketMask, _boardMask, new ulong[0]);

            retMask = (ulong)Hand.BitCount(outsMask);
            retMask = retMask << NB_OUT_OFFSET;
            retMask |= outsMask;

            return retMask;
        }

        public static byte CalculateMetaData(ulong _pocketMask, ulong _boardMask)
        {

            int maxPocketCardRank = 0;

            foreach (var item in Hand.Cards(_pocketMask))
            {
                maxPocketCardRank = Math.Max(Hand.CardRank(Hand.ParseCard(item)), maxPocketCardRank);
            }

            var cardIdx = new List<byte>() { 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 };
            var visitedCardMask = UInt16.MinValue;

            foreach (var cardString in Hand.Cards(_boardMask))
            {
                var cardNumber = Hand.ParseCard(cardString);
                var cardSuit = Hand.CardSuit(cardNumber);
                var cardRank = Hand.CardRank(cardNumber);
                if ((visitedCardMask & (1 << cardRank)) == 0)
                {
                    for (int i = 0; i < cardRank; --cardIdx[i++]) ;
                    visitedCardMask |= (ushort)(1 << cardRank);
                }
            }

            var retMask = (byte)(cardIdx[maxPocketCardRank] << 4);

            if (Hand.BitCount(_boardMask) < 5)
            {
                var boardCardCount = Hand.BitCount(_boardMask);
                var bIsFlushDraw = Hand.IsFlushDraw(_pocketMask | _boardMask, 0);
                var bIsStraightDraw = Hand.IsStraightDraw(_pocketMask | _boardMask, 0);

                var tempBoard = new CBoardModel(_pocketMask | _boardMask, 0);

                var bIsBackdoorFlushDraw = tempBoard.PIsFlush;
                var bIsBackdoorStraightDraw = tempBoard.PIsStraight;

                if (boardCardCount < 5)
                {
                    if (bIsBackdoorFlushDraw && (boardCardCount == 3)) retMask |= IS_BACK_DOOR_FLUSH_DRAW_MASK;
                    else if (bIsFlushDraw) retMask |= IS_FLUSH_DRAW_MASK;
                    if (bIsBackdoorStraightDraw & (boardCardCount == 3)) retMask |= IS_BACK_DOOR_STRAIGHT_DRAW_MASK;
                    else if (bIsStraightDraw) retMask |= IS_STRAIGHT_DRAW_MASK;
                }
            }

            return retMask;
        }
    }
}