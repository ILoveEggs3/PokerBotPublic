using HoldemHand;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Database
{
    public class CBoardModel
    {
        [Flags]
        public enum BoardMetaDataFlags
        {
            None = 0,
            Paired = 0x1 << 0,
            TwoPaired = 0x1 << 1,
            Trips = 0x1 << 2,
            FullHouse = 0x1 << 3,
            Quads = 0x1 << 4,
            StraightDrawPossible = 0x1 << 5,
            StraightPossible = 0x1 << 6,
            OneCardStraightPossible = 0x1 << 7,
            StraightComplete = 0x1 << 8,
            FlushDrawPossible = 0x1 << 9,
            FlushPossible = 0x1 << 10,
            OneCardFlushPossible = 0x1 << 11,
            FlushComplete = 0x1 << 12,
            StraightFlushComplete = 0x1 << 13,
            All = -1
        }

        private ulong FFBoardMask;
        public ulong PBoardMask
        {
            get
            {
                return FFBoardMask;
            }
            set
            {
                FFBoardMask = value;
                //PHeat = CalculateHeat(FFBoardMask);
            }
        }
        public double PHeat { get; private set; }
        private BoardMetaDataFlags FFMetaData;
        public BoardMetaDataFlags PMetaDataMask
        {
            get
            {
                return FFMetaData;
            }
        }
        public bool PIsPaired
        {
            get
            {
                return (FFMetaData & BoardMetaDataFlags.Paired) != 0;
            }
        }
        public bool PIs2Paired
        {
            get
            {
                return (FFMetaData & BoardMetaDataFlags.TwoPaired) != 0;
            }
        }
        public bool PIsTrips
        {
            get
            {
                return (FFMetaData & BoardMetaDataFlags.Trips) != 0;
            }
        }
        public bool PIsFullHouse
        {
            get
            {
                return (FFMetaData & BoardMetaDataFlags.FullHouse) != 0;
            }
        }
        public bool PIsQuads
        {
            get
            {
                return (FFMetaData & BoardMetaDataFlags.Quads) != 0;
            }
        }
        public bool PIsStraight
        {
            get
            {
                return (FFMetaData & BoardMetaDataFlags.StraightPossible) != 0;
            }
        }
        public bool PIsOneCardStraight
        {
            get
            {
                return (FFMetaData & BoardMetaDataFlags.OneCardStraightPossible) != 0;
            }
        }
        public bool PIsStraightComplete
        {
            get
            {
                return (FFMetaData & BoardMetaDataFlags.StraightComplete) != 0;
            }
        }
        public bool PIsFlush
        {
            get
            {
                return (FFMetaData & BoardMetaDataFlags.FlushPossible) != 0;
            }
        }
        public bool PIsOneCardFlush
        {
            get
            {
                return (FFMetaData & BoardMetaDataFlags.OneCardFlushPossible) != 0;
            }
        }
        public bool PIsFlushComplete
        {
            get
            {
                return (FFMetaData & BoardMetaDataFlags.FlushComplete) != 0;
            }
        }

        public CBoardModel(ulong _boardMask)
        {
            PBoardMask = _boardMask;
            //PHeat = CalculateHeat(_boardMask);
        }

        public CBoardModel(ulong _boardMask, double _heat)
        {
            FFBoardMask = _boardMask;
            PHeat = _heat;
            FFMetaData = CalculateMetaData(_boardMask);
        }

        public CBoardModel(ulong _boardMask, double _heat, BoardMetaDataFlags _metaDataMask)
        {
            FFBoardMask = _boardMask;
            PHeat = _heat;
            FFMetaData = _metaDataMask;
        }

        static public double CalculateHeat(ulong _boardMask)
        {
            throw new NotImplementedException();
        }

        public BoardMetaDataFlags CalculateMetaData()
        {
            return CalculateMetaData(PBoardMask);
        }

        static public BoardMetaDataFlags CalculateMetaData(ulong _boardMask)
        {
            var cardValues = new List<char>() { 'A', '2', '3', '4', '5', '6', '7', '8', '9', 'T', 'J', 'Q', 'K', 'A' };
            var numberOfCards = 0;
            var cardValuesCount = new Dictionary<char, int>();
            var cardTypesCount = new Dictionary<char, int>();

            foreach (var card in Hand.Cards(_boardMask))
            {
                var cardValue = card.ToCharArray()[0];
                var cardType = card.ToCharArray()[1];

                if (cardValuesCount.ContainsKey(cardValue)) cardValuesCount[cardValue]++;
                else cardValuesCount.Add(cardValue, 1);
                if (cardTypesCount.ContainsKey(cardType)) cardTypesCount[cardType]++;
                else cardTypesCount.Add(cardType, 1);

                numberOfCards++;
            }

            var numberOfPairs = cardValuesCount.LongCount(x => x.Value == 2);
            var numberOfTrips = cardValuesCount.LongCount(x => x.Value == 3);
            var numberOfQuads = cardValuesCount.LongCount(x => x.Value == 4);
            var numberOfColor = cardTypesCount.Max(x => x.Value);

            var bStraightDraw = false;
            var bStraight = false;
            var bOneCardStraight = false;
            var bStraightComplete = false;

            for (int i = 2; i < cardValues.Count - 2; i++)
            {
                var hitCount = 0;

                for (int j = i - 2; j < i + 3; j++)
                {
                    if (cardValuesCount.ContainsKey(cardValues[j])) hitCount++;
                }
                if (hitCount > 1) bStraightDraw = true;
                if (hitCount > 2) bStraight = true;
                if (hitCount > 3) bOneCardStraight = true;
                if (hitCount > 4) bStraightComplete = true;
            }
            var retMask = BoardMetaDataFlags.None;

            if (numberOfPairs > 0) retMask = BoardMetaDataFlags.Paired;
            if (numberOfPairs > 1) retMask = BoardMetaDataFlags.TwoPaired;
            if (numberOfTrips > 0) retMask = BoardMetaDataFlags.Trips;
            if (numberOfTrips > 0 && numberOfPairs > 1) retMask = BoardMetaDataFlags.FullHouse;

            if (bStraightComplete) retMask |= BoardMetaDataFlags.StraightComplete;
            else if (bOneCardStraight) retMask |= BoardMetaDataFlags.OneCardStraightPossible;
            else if (bStraight) retMask |= BoardMetaDataFlags.StraightPossible;
            else if (bStraightDraw) retMask |= BoardMetaDataFlags.StraightDrawPossible;

            if (numberOfColor > 4) retMask |= BoardMetaDataFlags.FlushComplete;
            else if (numberOfColor > 3) retMask |= BoardMetaDataFlags.OneCardFlushPossible;
            else if (numberOfColor > 2) retMask |= BoardMetaDataFlags.FlushPossible;
            else if (numberOfColor > 1) retMask |= BoardMetaDataFlags.FlushDrawPossible;

            if (bStraightComplete && (numberOfColor > 4))
            {
                retMask ^= BoardMetaDataFlags.FlushComplete | BoardMetaDataFlags.StraightComplete | BoardMetaDataFlags.StraightFlushComplete;
            }

            return retMask;
        }

        public override bool Equals(object obj)
        {
            var model = obj as CBoardModel;
            return model != null &&
                   PBoardMask == model.PBoardMask;
        }

        public override int GetHashCode()
        {
            return -1032876007 + PBoardMask.GetHashCode();
        }
    }
}
