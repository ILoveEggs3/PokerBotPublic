using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PokerShared;

namespace Resources
{
    public abstract class CReferences
    {
        public List<Tuple<PokerShared.CCard.Value, Bitmap>> PHandCardValueReferenceList;
        public List<Tuple<PokerShared.CCard.Type, Bitmap>> PHandCardTypeReferenceList;
        public List<Tuple<PokerShared.CCard.Value, Bitmap>> PBoardCardValueReferenceList;
        public List<Tuple<PokerShared.CCard.Type, Bitmap>> PBoardCardTypeReferenceList;
        public List<Tuple<char, Bitmap>> PPotCharacterReferenceList;
        public List<Tuple<char, Bitmap>> PBetCharacterReferenceList;
        public List<Tuple<char, Bitmap>> PStackCharacterReferenceList;
        public Bitmap PLAYER_1_TURN_INDICATOR_BMP;
        public Bitmap PDealerButtonBmp;
        public Tuple<Bitmap, Bitmap> PPlayerEmptyBmpTuple;


        protected CReferences()
        {
            Initialize();
        }

        protected virtual void Initialize()
        {
            #region Hand
        
            #region Value
           
            PHandCardValueReferenceList = new List<Tuple<CCard.Value, Bitmap>>();
            //2
            PHandCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Two, Properties.Resources.TWOMAX_ESPACEJEUX_HAND_VALUE_TWO_RED_BMP));
            PHandCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Two, Properties.Resources.TWOMAX_ESPACEJEUX_HAND_VALUE_TWO_BLACK_BMP));
            //3
            PHandCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Three, Properties.Resources.TWOMAX_ESPACEJEUX_HAND_VALUE_THREE_RED_BMP));
            PHandCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Three, Properties.Resources.TWOMAX_ESPACEJEUX_HAND_VALUE_THREE_BLACK_BMP));
            //4
            PHandCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Four, Properties.Resources.TWOMAX_ESPACEJEUX_HAND_VALUE_FOUR_RED_BMP));
            PHandCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Four, Properties.Resources.TWOMAX_ESPACEJEUX_HAND_VALUE_FOUR_BLACK_BMP));
            //5
            PHandCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Five, Properties.Resources.TWOMAX_ESPACEJEUX_HAND_VALUE_FIVE_RED_BMP));
            PHandCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Five, Properties.Resources.TWOMAX_ESPACEJEUX_HAND_VALUE_FIVE_BLACK_BMP));
            //6
            PHandCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Six, Properties.Resources.TWOMAX_ESPACEJEUX_HAND_VALUE_SIX_RED_BMP));
            PHandCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Six, Properties.Resources.TWOMAX_ESPACEJEUX_HAND_VALUE_SIX_BLACK_BMP));
            //7
            PHandCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Seven, Properties.Resources.TWOMAX_ESPACEJEUX_HAND_VALUE_SEVEN_RED_BMP));
            PHandCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Seven, Properties.Resources.TWOMAX_ESPACEJEUX_HAND_VALUE_SEVEN_BLACK_BMP));
            //8
            PHandCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Eight, Properties.Resources.TWOMAX_ESPACEJEUX_HAND_VALUE_EIGHT_RED_BMP));
            PHandCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Eight, Properties.Resources.TWOMAX_ESPACEJEUX_HAND_VALUE_EIGHT_BLACK_BMP));
            //9
            PHandCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Nine, Properties.Resources.TWOMAX_ESPACEJEUX_HAND_VALUE_NINE_RED_BMP));
            PHandCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Nine, Properties.Resources.TWOMAX_ESPACEJEUX_HAND_VALUE_NINE_BLACK_BMP));
            //10
            PHandCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Ten, Properties.Resources.TWOMAX_ESPACEJEUX_HAND_VALUE_TEN_RED_BMP));
            PHandCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Ten, Properties.Resources.TWOMAX_ESPACEJEUX_HAND_VALUE_TEN_BLACK_BMP));
            PHandCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Ten, Properties.Resources.TWOMAX_ESPACEJEUX_HAND_VALUE_TEN_BLACK2_BMP));
            //J
            PHandCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Jack, Properties.Resources.TWOMAX_ESPACEJEUX_HAND_VALUE_JACK_RED_BMP));
            PHandCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Jack, Properties.Resources.TWOMAX_ESPACEJEUX_HAND_VALUE_JACK_BLACK_BMP));
            //Q
            PHandCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Queen, Properties.Resources.TWOMAX_ESPACEJEUX_HAND_VALUE_QUEEN_RED_BMP));
            PHandCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Queen, Properties.Resources.TWOMAX_ESPACEJEUX_HAND_VALUE_QUEEN_BLACK_BMP));
            PHandCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Queen, Properties.Resources.TWOMAX_ESPACEJEUX_HAND_VALUE_QUEEN_BLACK2_BMP));
            //K
            PHandCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.King, Properties.Resources.TWOMAX_ESPACEJEUX_HAND_VALUE_KING_RED_BMP));
            PHandCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.King, Properties.Resources.TWOMAX_ESPACEJEUX_HAND_VALUE_KING_RED2_BMP));
            PHandCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.King, Properties.Resources.TWOMAX_ESPACEJEUX_HAND_VALUE_KING_BLACK_BMP));
            PHandCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.King, Properties.Resources.TWOMAX_ESPACEJEUX_HAND_VALUE_KING_BLACK2_BMP));
            //A
            PHandCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Ace, Properties.Resources.TWOMAX_ESPACEJEUX_HAND_VALUE_ACE_RED_BMP));
            PHandCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Ace, Properties.Resources.TWOMAX_ESPACEJEUX_HAND_VALUE_ACE_BLACK_BMP));


            #endregion

            #region TYPE

            PHandCardTypeReferenceList = new List<Tuple<CCard.Type, Bitmap>>();
            //h
            PHandCardTypeReferenceList.Add(new Tuple<CCard.Type, Bitmap>(CCard.Type.Hearts, Properties.Resources.TWOMAX_ESPACEJEUX_HAND_TYPE_HEART_BMP));
            //d
            PHandCardTypeReferenceList.Add(new Tuple<CCard.Type, Bitmap>(CCard.Type.Diamonds, Properties.Resources.TWOMAX_ESPACEJEUX_HAND_TYPE_DIAMOND_BMP));
            //s
            PHandCardTypeReferenceList.Add(new Tuple<CCard.Type, Bitmap>(CCard.Type.Spades, Properties.Resources.TWOMAX_ESPACEJEUX_HAND_TYPE_SPADE_BMP));
            //c
            PHandCardTypeReferenceList.Add(new Tuple<CCard.Type, Bitmap>(CCard.Type.Clubs, Properties.Resources.TWOMAX_ESPACEJEUX_HAND_TYPE_CLUB_BMP));
            #endregion

            #endregion

            #region Board

            #region Value

            PBoardCardValueReferenceList = new List<Tuple<CCard.Value, Bitmap>>();

            //2
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Two, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_TWO_RED_BMP));
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Two, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_TWO_RED2_BMP));
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Two, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_TWO_BLACK_BMP));
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Two, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_TWO_BLACK2_BMP));
            //3
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Three, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_THREE_RED_BMP));
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Three, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_THREE_RED2_BMP));
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Three, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_THREE_BLACK_BMP));
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Three, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_THREE_BLACK2_BMP));
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Three, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_THREE_BLACK3_BMP));
            //4
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Four, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_FOUR_RED_BMP));
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Four, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_FOUR_RED2_BMP));
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Four, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_FOUR_BLACK_BMP));
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Four, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_FOUR_BLACK2_BMP));
            //5
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Five, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_FIVE_RED_BMP));
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Five, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_FIVE_RED2_BMP));
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Five, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_FIVE_BLACK_BMP));
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Five, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_FIVE_BLACK2_BMP));
            //6
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Six, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_SIX_RED_BMP));
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Six, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_SIX_RED2_BMP));
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Six, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_SIX_BLACK_BMP));
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Six, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_SIX_BLACK2_BMP));
            //7
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Seven, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_SEVEN_RED_BMP));
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Seven, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_SEVEN_BLACK_BMP));
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Seven, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_SEVEN_BLACK2_BMP));
            //8
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Eight, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_EIGHT_RED_BMP));
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Eight, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_EIGHT_RED2_BMP));
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Eight, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_EIGHT_BLACK_BMP));
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Eight, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_EIGHT_BLACK2_BMP));
            //9
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Nine, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_NINE_RED_BMP));
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Nine, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_NINE_RED2_BMP));
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Nine, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_NINE_BLACK_BMP));
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Nine, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_NINE_BLACK2_BMP));
            //10
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Ten, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_TEN_RED_BMP));
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Ten, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_TEN_RED2_BMP));
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Ten, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_TEN_BLACK_BMP));
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Ten, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_TEN_BLACK2_BMP));
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Ten, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_TEN_BLACK3_BMP));
            //J
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Jack, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_JACK_RED_BMP));
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Jack, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_JACK_RED2_BMP));
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Jack, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_JACK_BLACK_BMP));
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Jack, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_JACK_BLACK2_BMP));
            //Q
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Queen, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_QUEEN_RED_BMP));
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Queen, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_QUEEN_RED2_BMP));
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Queen, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_QUEEN_BLACK_BMP));
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Queen, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_QUEEN_BLACK2_BMP));
            //K
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.King, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_KING_RED_BMP));
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.King, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_KING_RED2_BMP));
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.King, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_KING_BLACK_BMP));
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.King, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_KING_BLACK2_BMP));
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.King, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_KING_BLACK3_BMP));
            //A
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Ace, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_ACE_RED_BMP));
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Ace, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_ACE_RED2_BMP));
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Ace, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_ACE_RED3_BMP));
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Ace, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_ACE_BLACK_BMP));
            PBoardCardValueReferenceList.Add(new Tuple<CCard.Value, Bitmap>(CCard.Value.Ace, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_VALUE_ACE_BLACK2_BMP));


            #endregion

            #region TYPE

            PBoardCardTypeReferenceList = new List<Tuple<CCard.Type, Bitmap>>();
            //h
            PBoardCardTypeReferenceList.Add(new Tuple<CCard.Type, Bitmap>(CCard.Type.Hearts, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_TYPE_HEART_BMP));
            PBoardCardTypeReferenceList.Add(new Tuple<CCard.Type, Bitmap>(CCard.Type.Hearts, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_TYPE_HEART2_BMP));
            //d
            PBoardCardTypeReferenceList.Add(new Tuple<CCard.Type, Bitmap>(CCard.Type.Diamonds, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_TYPE_DIAMOND_BMP));
            PBoardCardTypeReferenceList.Add(new Tuple<CCard.Type, Bitmap>(CCard.Type.Diamonds, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_TYPE_DIAMOND2_BMP));
            //s
            PBoardCardTypeReferenceList.Add(new Tuple<CCard.Type, Bitmap>(CCard.Type.Spades, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_TYPE_SPADE_BMP));
            PBoardCardTypeReferenceList.Add(new Tuple<CCard.Type, Bitmap>(CCard.Type.Spades, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_TYPE_SPADE2_BMP));
            PBoardCardTypeReferenceList.Add(new Tuple<CCard.Type, Bitmap>(CCard.Type.Spades, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_TYPE_SPADE3_BMP));
            //c
            PBoardCardTypeReferenceList.Add(new Tuple<CCard.Type, Bitmap>(CCard.Type.Clubs, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_TYPE_CLUB_BMP));
            PBoardCardTypeReferenceList.Add(new Tuple<CCard.Type, Bitmap>(CCard.Type.Clubs, Properties.Resources.TWOMAX_ESPACEJEUX_BOARD_TYPE_CLUB2_BMP));

            #endregion

            #endregion

            #region Numbers

            #region Pot

            PPotCharacterReferenceList = new List<Tuple<char, Bitmap>>();

            #endregion

            #region Stack

            PStackCharacterReferenceList = new List<Tuple<char, Bitmap>>();
            PStackCharacterReferenceList.Add(new Tuple<char, Bitmap>('0', Properties.Resources.TWOMAX_ESPACEJEUX_STACK_VALUE_ZERO_BMP));
            PStackCharacterReferenceList.Add(new Tuple<char, Bitmap>('1', Properties.Resources.TWOMAX_ESPACEJEUX_STACK_VALUE_ONE_BMP));
            PStackCharacterReferenceList.Add(new Tuple<char, Bitmap>('2', Properties.Resources.TWOMAX_ESPACEJEUX_STACK_VALUE_TWO_BMP));
            PStackCharacterReferenceList.Add(new Tuple<char, Bitmap>('3', Properties.Resources.TWOMAX_ESPACEJEUX_STACK_VALUE_THREE_BMP));
            PStackCharacterReferenceList.Add(new Tuple<char, Bitmap>('4', Properties.Resources.TWOMAX_ESPACEJEUX_STACK_VALUE_FOUR_BMP));
            PStackCharacterReferenceList.Add(new Tuple<char, Bitmap>('5', Properties.Resources.TWOMAX_ESPACEJEUX_STACK_VALUE_FIVE_BMP));
            PStackCharacterReferenceList.Add(new Tuple<char, Bitmap>('6', Properties.Resources.TWOMAX_ESPACEJEUX_STACK_VALUE_SIX_BMP));
            PStackCharacterReferenceList.Add(new Tuple<char, Bitmap>('7', Properties.Resources.TWOMAX_ESPACEJEUX_STACK_VALUE_SEVEN_BMP));
            PStackCharacterReferenceList.Add(new Tuple<char, Bitmap>('8', Properties.Resources.TWOMAX_ESPACEJEUX_STACK_VALUE_EIGHT_BMP));
            PStackCharacterReferenceList.Add(new Tuple<char, Bitmap>('9', Properties.Resources.TWOMAX_ESPACEJEUX_STACK_VALUE_NINE_BMP));


            #endregion

            #region Bet

            PBetCharacterReferenceList = new List<Tuple<char, Bitmap>>();
            PBetCharacterReferenceList.Add(new Tuple<char, Bitmap>('0', Properties.Resources.TWOMAX_ESPACEJEUX_TABLE_POT_VALUE_0_BMP));
            PBetCharacterReferenceList.Add(new Tuple<char, Bitmap>('1', Properties.Resources.TWOMAX_ESPACEJEUX_TABLE_POT_VALUE_1_BMP));
            PBetCharacterReferenceList.Add(new Tuple<char, Bitmap>('2', Properties.Resources.TWOMAX_ESPACEJEUX_TABLE_POT_VALUE_2_BMP));
            PBetCharacterReferenceList.Add(new Tuple<char, Bitmap>('3', Properties.Resources.TWOMAX_ESPACEJEUX_TABLE_POT_VALUE_3_BMP2));
            PBetCharacterReferenceList.Add(new Tuple<char, Bitmap>('3', Properties.Resources.TWOMAX_ESPACEJEUX_TABLE_POT_VALUE_3_BMP3));
            PBetCharacterReferenceList.Add(new Tuple<char, Bitmap>('4', Properties.Resources.TWOMAX_ESPACEJEUX_TABLE_POT_VALUE_4_BMP));
            PBetCharacterReferenceList.Add(new Tuple<char, Bitmap>('5', Properties.Resources.TWOMAX_ESPACEJEUX_TABLE_POT_VALUE_5_BMP));
            PBetCharacterReferenceList.Add(new Tuple<char, Bitmap>('6', Properties.Resources.TWOMAX_ESPACEJEUX_TABLE_POT_VALUE_6_BMP));
            PBetCharacterReferenceList.Add(new Tuple<char, Bitmap>('7', Properties.Resources.TWOMAX_ESPACEJEUX_TABLE_POT_VALUE_7_BMP));
            PBetCharacterReferenceList.Add(new Tuple<char, Bitmap>('8', Properties.Resources.TWOMAX_ESPACEJEUX_TABLE_POT_VALUE_8_BMP));
            PBetCharacterReferenceList.Add(new Tuple<char, Bitmap>('9', Properties.Resources.TWOMAX_ESPACEJEUX_TABLE_POT_VALUE_9_BMP));
            PBetCharacterReferenceList.Add(new Tuple<char, Bitmap>('9', Properties.Resources.TWOMAX_ESPACEJEUX_TABLE_POT_VALUE_9_BMP2));
            PBetCharacterReferenceList.Add(new Tuple<char, Bitmap>('.', Properties.Resources.TWOMAX_ESPACEJEUX_TABLE_POT_VALUE_DOT_BMP));
            PBetCharacterReferenceList.Add(new Tuple<char, Bitmap>('.', Properties.Resources.TWOMAX_ESPACEJEUX_TABLE_POT_VALUE_DOT_BMP2));

            #endregion

            #endregion

            #region MISC

            PLAYER_1_TURN_INDICATOR_BMP = Properties.Resources.TWOMAX_ESPACEJEUX_PLAYER_1_TURN_INDICATOR_BMP;
            PDealerButtonBmp = Properties.Resources.TWOMAX_ESPACEJEUX_PLAYER_0_DEALER_BMP;
            PPlayerEmptyBmpTuple = new Tuple<Bitmap, Bitmap>(Properties.Resources.TWOMAX_ESPACEJEUX_PLAYER_EMPTY_BMP1, Properties.Resources.TWOMAX_ESPACEJEUX_PLAYER_EMPTY_BMP2);

            #endregion
        }
    }
}
