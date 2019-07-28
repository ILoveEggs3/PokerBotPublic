using HoldemHand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Poker.Models
{
    public class CBoard
    {
        private const ulong  Left32BitUlongMask = unchecked((ulong)-1 << 32); //god damnit c#
        private const ulong Right32BitUlongMask = unchecked((ulong)-1 >> 32);

        public ulong PMask { private set; get; }

        public List<CCard> PBoardList { private set; get; }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_board"></param>
        public CBoard(CCard _card1, CCard _card2, CCard _card3, CCard _card4 = null, CCard _card5 = null)
        {
            PBoardList = new List<CCard>(5);
            PBoardList.Add(_card1);
            PBoardList.Add(_card2);
            PBoardList.Add(_card3);
            PBoardList.Add(_card4);
            PBoardList.Add(_card5);

            #region Setting the board mask
            ulong value = 0;
            foreach (CCard card in PBoardList)
            {
                if (card != null)
                    value |= card.PMask;
            }
            PMask = value;
            #endregion
        }

        public bool IsPaired()
        {
            return PBoardList.Any(x => PBoardList.Any(y => x == y));
        }

        public bool IsFlushPossible()
        {
            int numberOfHearts = 0;
            int numberOfClubs = 0;
            int numberOfDiamonds = 0;
            int numberOfSpades = 0;

            for(int i = 0; i < PBoardList.Count; ++i)
            {
                switch (PBoardList[i].PType)
                {
                    case CCard.Type.Hearts:
                        ++numberOfHearts;
                        break;
                    case CCard.Type.Clubs:
                        ++numberOfClubs;
                        break;
                    case CCard.Type.Diamonds:
                        ++numberOfDiamonds;
                        break;
                    case CCard.Type.Spades:
                        ++numberOfSpades;
                        break;
                }                
            }

            bool foundFlush = (numberOfHearts == 3 || numberOfClubs == 3 || numberOfDiamonds == 3 || numberOfSpades == 3);

            return foundFlush;
        }

        public bool IsOneCardFlushPossible()
        {
            int numberOfHearts = 0;
            int numberOfClubs = 0;
            int numberOfDiamonds = 0;
            int numberOfSpades = 0;

            for (int i = 0; i < PBoardList.Count; ++i)
            {
                switch (PBoardList[i].PType)
                {
                    case CCard.Type.Hearts:
                        ++numberOfHearts;
                        break;
                    case CCard.Type.Clubs:
                        ++numberOfClubs;
                        break;
                    case CCard.Type.Diamonds:
                        ++numberOfDiamonds;
                        break;
                    case CCard.Type.Spades:
                        ++numberOfSpades;
                        break;
                }
            }

            bool foundFlush = (numberOfHearts == 4 || numberOfClubs == 4 || numberOfDiamonds == 4 || numberOfSpades == 4);

            return foundFlush;
        }

        public int IndexHighCard(ulong _pocket, ulong _board)
        {
            int maxPocketCardRank = 0;

            foreach (var item in Hand.Cards(_pocket))
            {
                maxPocketCardRank = Math.Max(Hand.CardRank(Hand.ParseCard(item)), maxPocketCardRank);
            }

            var visitedValues = new Dictionary<int, int>();
            int value = Hand.RankAce - maxPocketCardRank;

            foreach (var item in Hand.Cards(_board))
            {
                var boardCardValue = Hand.CardRank(Hand.ParseCard(item));
                if (boardCardValue > maxPocketCardRank && !visitedValues.ContainsKey(boardCardValue))
                {
                    visitedValues.Add(boardCardValue, 0);
                    value -= 1;
                }
            }

            return value;
        }

        // Gets cards that matches the "IndexHighestCardExcludingBoard"
        public Dictionary<ulong, CComboCard> GetCardsThatMatchTheIndexExcludingBoard(Dictionary<ulong, CComboCard> _rangeToStartFrom, int _indexHighestCardExcludingBoard)
        {
            return _rangeToStartFrom.Where(x => IndexHighCard(x.Key, PMask) == _indexHighestCardExcludingBoard).ToDictionary(x => x.Key, x => x.Value);
        }

        public override string ToString()
        {
            string board = "";

            if (PBoardList[0] != null)
            {
                board += (PBoardList[0].ToString() + " " + PBoardList[1].ToString() + " " +  PBoardList[2].ToString());

                if (PBoardList[3] != null)
                    board += " " + PBoardList[3].ToString();

                if (PBoardList[4] != null)
                    board += " " + PBoardList[4].ToString();
            }

            return board;
        }
    }
}
