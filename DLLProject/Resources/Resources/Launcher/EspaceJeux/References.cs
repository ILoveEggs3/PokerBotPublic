using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resources.Launcher.EspaceJeux
{
    public class References
    {
        public Bitmap PFiltersChkEnabledBmp;
        public Bitmap PFiltersChkDisabledBmp;

        public Bitmap PFiltersDropDownButtonBmp;

        public Bitmap PFiltersDropDownUpArrowBmp;
        public Bitmap PFiltersDropDownDownArrowBmp;

        public Bitmap PPopupQuestionBmp;
        public Tuple<Bitmap, Bitmap> PPopupExclamationBmp;

        public List<Tuple<char, Bitmap>> PTableGridCharacterList;

        [Flags]
        public enum Filters_Drop_Down_Values
        {
            None = 0,
            NL4 = 0x1 << 0,
            NL10 = 0x1 << 1,
            NL20 = 0x1 << 2,
            NL30 = 0x1 << 3,
            NL50 = 0x1 << 4,
            NL100 = 0x1 << 5,
            NL200 = 0x1 << 6,
            NL400 = 0x1 << 7,
            NL500 = 0x1 << 8,
            NL600 = 0x1 << 9,
            NL800 = 0x1 << 10,
            NL1000 = 0x1 << 11,
            NL1600 = 0x1 << 12,
            NL2000 = 0x1 << 13,
            NL3000 = 0x1 << 14,
            NL4000 = 0x1 << 15,
            NL5000 = 0x1 << 16,
            All = -1
        }

        public Dictionary<Filters_Drop_Down_Values, Bitmap> PFiltersDropDownValues;

        public Bitmap PFiltersTxtBoxEmptyBmp;

        public Tuple<Bitmap, Bitmap> PTableGridEmptyRowBmp;

        public Tuple<Bitmap, Bitmap> PTableGridPlrs_0_2;
        public Tuple<Bitmap, Bitmap, Bitmap> PTableGridPlrs_2_2;
        public Bitmap PTableGridPlrsSortingDecrementing;

        public References()
        {
            PFiltersDropDownValues = new Dictionary<Filters_Drop_Down_Values, Bitmap>();
            PTableGridCharacterList = new List<Tuple<char, Bitmap>>();
            Initialize();
        }

        public void Initialize()
        {
            #region FILTERS

            #region CHK_BOX

            PFiltersChkEnabledBmp = Properties.Resources.LAUNCHER_ESPACEJEUX_FILTERS_CHK_ENABLED;
            PFiltersChkDisabledBmp = Properties.Resources.LAUNCHER_ESPACEJEUX_FILTERS_CHK_DISABLED;

            #endregion

            #region DROP_DOWN

            PFiltersDropDownButtonBmp = Properties.Resources.LAUNCHER_ESPACEJEUX_FILTERS_DROP_DOWN_BUTTON_BMP;

            #region VALUE

            PFiltersDropDownValues.Add(Filters_Drop_Down_Values.None, Properties.Resources.LAUNCHER_ESPACEJEUX_FILTERS_VALUE_NONE_BMP);
            PFiltersDropDownValues.Add(Filters_Drop_Down_Values.NL4, Properties.Resources.LAUNCHER_ESPACEJEUX_FILTERS_VALUE_002_04_BMP);
            PFiltersDropDownValues.Add(Filters_Drop_Down_Values.NL10, Properties.Resources.LAUNCHER_ESPACEJEUX_FILTERS_VALUE_005_01_BMP);
            PFiltersDropDownValues.Add(Filters_Drop_Down_Values.NL20, Properties.Resources.LAUNCHER_ESPACEJEUX_FILTERS_VALUE_01_02_BMP);
            PFiltersDropDownValues.Add(Filters_Drop_Down_Values.NL30, Properties.Resources.LAUNCHER_ESPACEJEUX_FILTERS_VALUE_015_03_BMP);
            PFiltersDropDownValues.Add(Filters_Drop_Down_Values.NL50, Properties.Resources.LAUNCHER_ESPACEJEUX_FILTERS_VALUE_025_05_BMP);
            PFiltersDropDownValues.Add(Filters_Drop_Down_Values.NL100, Properties.Resources.LAUNCHER_ESPACEJEUX_FILTERS_VALUE_05_1_BMP);
            PFiltersDropDownValues.Add(Filters_Drop_Down_Values.NL200, Properties.Resources.LAUNCHER_ESPACEJEUX_FILTERS_VALUE_1_2_BMP);
            PFiltersDropDownValues.Add(Filters_Drop_Down_Values.NL400, Properties.Resources.LAUNCHER_ESPACEJEUX_FILTERS_VALUE_2_4_BMP);
            PFiltersDropDownValues.Add(Filters_Drop_Down_Values.NL500, Properties.Resources.LAUNCHER_ESPACEJEUX_FILTERS_VALUE_25_5_BMP);
            PFiltersDropDownValues.Add(Filters_Drop_Down_Values.NL600, Properties.Resources.LAUNCHER_ESPACEJEUX_FILTERS_VALUE_3_6_BMP);
            PFiltersDropDownValues.Add(Filters_Drop_Down_Values.NL800, Properties.Resources.LAUNCHER_ESPACEJEUX_FILTERS_VALUE_4_8_BMP);
            PFiltersDropDownValues.Add(Filters_Drop_Down_Values.NL1000, Properties.Resources.LAUNCHER_ESPACEJEUX_FILTERS_VALUE_5_10_BMP);
            PFiltersDropDownValues.Add(Filters_Drop_Down_Values.NL1600, Properties.Resources.LAUNCHER_ESPACEJEUX_FILTERS_VALUE_8_16_BMP);
            PFiltersDropDownValues.Add(Filters_Drop_Down_Values.NL2000, Properties.Resources.LAUNCHER_ESPACEJEUX_FILTERS_VALUE_10_20_BMP);
            PFiltersDropDownValues.Add(Filters_Drop_Down_Values.NL3000, Properties.Resources.LAUNCHER_ESPACEJEUX_FILTERS_VALUE_15_30_BMP);
            PFiltersDropDownValues.Add(Filters_Drop_Down_Values.NL4000, Properties.Resources.LAUNCHER_ESPACEJEUX_FILTERS_VALUE_20_40_BMP);
            PFiltersDropDownValues.Add(Filters_Drop_Down_Values.NL5000, Properties.Resources.LAUNCHER_ESPACEJEUX_FILTERS_VALUE_25_50_BMP);

            #endregion

            #region TXT_BOX

            PFiltersTxtBoxEmptyBmp = Properties.Resources.LAUNCHER_ESPACEJEUX_FILTERS_TXT_BOX_EMPTY_BMP;

            #endregion

            #endregion

            #endregion

            #region TABLE_GRID

            PTableGridEmptyRowBmp = new Tuple<Bitmap, Bitmap>(Properties.Resources.LAUNCHER_ESPACEJEUX_TABLEGRID_EMPTY_ROW_1, Properties.Resources.LAUNCHER_ESPACEJEUX_TABLEGRID_EMPTY_ROW_2);
            PTableGridPlrs_0_2 = new Tuple<Bitmap, Bitmap>(Properties.Resources.LAUNCHER_ESPACEJEUX_TABLEGRID_PLRS_0_2_BMP1, Properties.Resources.LAUNCHER_ESPACEJEUX_TABLEGRID_PLRS_0_2_BMP1);
            PTableGridPlrs_2_2 = new Tuple<Bitmap, Bitmap, Bitmap>(Properties.Resources.LAUNCHER_ESPACEJEUX_TABLEGRID_PLRS_2_2_BMP, Properties.Resources.LAUNCHER_ESPACEJEUX_TABLEGRID_PLRS_2_2_BMP2, Properties.Resources.LAUNCHER_ESPACEJEUX_TABLEGRID_PLRS_2_2_BMP3);
            PTableGridPlrsSortingDecrementing = Properties.Resources.LAUNCHER_ESPACEJEUX_TABLEGRID_PLRS_SORTING_DECREMENTING_BMP;

            #region CHARACTERS

            PTableGridCharacterList.Add(new Tuple<char, Bitmap>('0', Properties.Resources.LAUNCHER_ESPACEJEUX_TABLEGRID_TABLENAME_CHARACTER_0_BMP1));
            PTableGridCharacterList.Add(new Tuple<char, Bitmap>('0', Properties.Resources.LAUNCHER_ESPACEJEUX_TABLEGRID_TABLENAME_CHARACTER_0_BMP2));
            PTableGridCharacterList.Add(new Tuple<char, Bitmap>('0', Properties.Resources.LAUNCHER_ESPACEJEUX_TABLEGRID_TABLENAME_CHARACTER_0_BMP3));

            PTableGridCharacterList.Add(new Tuple<char, Bitmap>('1', Properties.Resources.LAUNCHER_ESPACEJEUX_TABLEGRID_TABLENAME_CHARACTER_1_BMP1));
            PTableGridCharacterList.Add(new Tuple<char, Bitmap>('1', Properties.Resources.LAUNCHER_ESPACEJEUX_TABLEGRID_TABLENAME_CHARACTER_1_BMP2));
            PTableGridCharacterList.Add(new Tuple<char, Bitmap>('1', Properties.Resources.LAUNCHER_ESPACEJEUX_TABLEGRID_TABLENAME_CHARACTER_1_BMP3));

            PTableGridCharacterList.Add(new Tuple<char, Bitmap>('2', Properties.Resources.LAUNCHER_ESPACEJEUX_TABLEGRID_TABLENAME_CHARACTER_2_BMP1));
            PTableGridCharacterList.Add(new Tuple<char, Bitmap>('2', Properties.Resources.LAUNCHER_ESPACEJEUX_TABLEGRID_TABLENAME_CHARACTER_2_BMP2));
            PTableGridCharacterList.Add(new Tuple<char, Bitmap>('2', Properties.Resources.LAUNCHER_ESPACEJEUX_TABLEGRID_TABLENAME_CHARACTER_2_BMP3));

            PTableGridCharacterList.Add(new Tuple<char, Bitmap>('3', Properties.Resources.LAUNCHER_ESPACEJEUX_TABLEGRID_TABLENAME_CHARACTER_3_BMP1));
            PTableGridCharacterList.Add(new Tuple<char, Bitmap>('3', Properties.Resources.LAUNCHER_ESPACEJEUX_TABLEGRID_TABLENAME_CHARACTER_3_BMP2));
            PTableGridCharacterList.Add(new Tuple<char, Bitmap>('3', Properties.Resources.LAUNCHER_ESPACEJEUX_TABLEGRID_TABLENAME_CHARACTER_3_BMP3));

            PTableGridCharacterList.Add(new Tuple<char, Bitmap>('4', Properties.Resources.LAUNCHER_ESPACEJEUX_TABLEGRID_TABLENAME_CHARACTER_4_BMP1));
            PTableGridCharacterList.Add(new Tuple<char, Bitmap>('4', Properties.Resources.LAUNCHER_ESPACEJEUX_TABLEGRID_TABLENAME_CHARACTER_4_BMP2));
            PTableGridCharacterList.Add(new Tuple<char, Bitmap>('4', Properties.Resources.LAUNCHER_ESPACEJEUX_TABLEGRID_TABLENAME_CHARACTER_4_BMP3));

            PTableGridCharacterList.Add(new Tuple<char, Bitmap>('5', Properties.Resources.LAUNCHER_ESPACEJEUX_TABLEGRID_TABLENAME_CHARACTER_5_BMP1));
            PTableGridCharacterList.Add(new Tuple<char, Bitmap>('5', Properties.Resources.LAUNCHER_ESPACEJEUX_TABLEGRID_TABLENAME_CHARACTER_5_BMP2));
            PTableGridCharacterList.Add(new Tuple<char, Bitmap>('5', Properties.Resources.LAUNCHER_ESPACEJEUX_TABLEGRID_TABLENAME_CHARACTER_5_BMP3));

            PTableGridCharacterList.Add(new Tuple<char, Bitmap>('6', Properties.Resources.LAUNCHER_ESPACEJEUX_TABLEGRID_TABLENAME_CHARACTER_6_BMP1));
            PTableGridCharacterList.Add(new Tuple<char, Bitmap>('6', Properties.Resources.LAUNCHER_ESPACEJEUX_TABLEGRID_TABLENAME_CHARACTER_6_BMP2));
            PTableGridCharacterList.Add(new Tuple<char, Bitmap>('6', Properties.Resources.LAUNCHER_ESPACEJEUX_TABLEGRID_TABLENAME_CHARACTER_6_BMP3));

            PTableGridCharacterList.Add(new Tuple<char, Bitmap>('7', Properties.Resources.LAUNCHER_ESPACEJEUX_TABLEGRID_TABLENAME_CHARACTER_7_BMP1));
            PTableGridCharacterList.Add(new Tuple<char, Bitmap>('7', Properties.Resources.LAUNCHER_ESPACEJEUX_TABLEGRID_TABLENAME_CHARACTER_7_BMP2));
            PTableGridCharacterList.Add(new Tuple<char, Bitmap>('7', Properties.Resources.LAUNCHER_ESPACEJEUX_TABLEGRID_TABLENAME_CHARACTER_7_BMP3));

            PTableGridCharacterList.Add(new Tuple<char, Bitmap>('8', Properties.Resources.LAUNCHER_ESPACEJEUX_TABLEGRID_TABLENAME_CHARACTER_8_BMP1));
            PTableGridCharacterList.Add(new Tuple<char, Bitmap>('8', Properties.Resources.LAUNCHER_ESPACEJEUX_TABLEGRID_TABLENAME_CHARACTER_8_BMP2));
            PTableGridCharacterList.Add(new Tuple<char, Bitmap>('8', Properties.Resources.LAUNCHER_ESPACEJEUX_TABLEGRID_TABLENAME_CHARACTER_8_BMP3));

            PTableGridCharacterList.Add(new Tuple<char, Bitmap>('9', Properties.Resources.LAUNCHER_ESPACEJEUX_TABLEGRID_TABLENAME_CHARACTER_9_BMP1));
            PTableGridCharacterList.Add(new Tuple<char, Bitmap>('9', Properties.Resources.LAUNCHER_ESPACEJEUX_TABLEGRID_TABLENAME_CHARACTER_9_BMP2));
            PTableGridCharacterList.Add(new Tuple<char, Bitmap>('9', Properties.Resources.LAUNCHER_ESPACEJEUX_TABLEGRID_TABLENAME_CHARACTER_9_BMP3));

            #endregion

            #endregion

            #region OTHER

            PPopupExclamationBmp = new Tuple<Bitmap, Bitmap>(Properties.Resources.LAUNCHER_ESPACEJEUX_POPUP_CIRCLE_EXCLAMATION_BMP, Properties.Resources.LAUNCHER_ESPACEJEUX_POPUP_CIRCLE_EXCLAMATION_BMP2);
            PPopupQuestionBmp = Properties.Resources.LAUNCHER_ESPACEJEUX_POPUP_CIRCLE_QUESTION_BMP;

            #endregion
        }
    }
}
