using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resources.Launcher.EspaceJeux
{
	 public abstract class AbstractConstantes
	 {
		#region WINDOW_DIMENSION
		
		public abstract Rectangle WINDOW_DIMENSION { get; }
		
		#endregion
		
		#region FILTERS_BUTTON
		
		public abstract Rectangle FILTERS_BUTTON { get; }
		
		#endregion
		
		#region FILTERS_CHK_BOX
		
		public abstract Rectangle FILTERS_USE_FILTERS { get; }
		
		#endregion
		
		#region FILTERS_CHK_TWO_SEATS
		
		public abstract Rectangle FILTERS_CHK_TWO_SEATS { get; }
		
		#endregion
		
		#region FILTERS_CHK_REGULAR
		
		public abstract Rectangle FILTERS_CHK_REGULAR { get; }
		
		#endregion
		
		#region FILTERS_CHK_EMPTY
		
		public abstract Rectangle FILTERS_CHK_EMPTY { get; }
		
		#endregion
		
		#region FILTERS_CHK_FULL
		
		public abstract Rectangle FILTERS_CHK_FULL { get; }
		
		#endregion
		
		#region TABLEGRID_ROW_1
		
		public abstract Rectangle TABLEGRID_ROW_1 { get; }
		
		#endregion
		
		#region TABLEGRID_TABLE_1_PLRS
		
		public abstract Rectangle TABLEGRID_TABLE_1_PLRS { get; }
		
		#endregion
		
		#region FILTERS_MIN_DROP_DOWN_BUTTON
		
		public abstract Rectangle FILTERS_MIN_DROP_DOWN_BUTTON { get; }
		
		#endregion
		
		#region FILTERS_MAX_DROP_DOWN_BUTTON
		
		public abstract Rectangle FILTERS_MAX_DROP_DOWN_BUTTON { get; }
		
		#endregion
		
		#region FILTERS_CHK_THREE_SEATS
		
		public abstract Rectangle FILTERS_CHK_THREE_SEATS { get; }
		
		#endregion
		
		#region FILTERS_CHK_FOUR_SEATS
		
		public abstract Rectangle FILTERS_CHK_FOUR_SEATS { get; }
		
		#endregion
		
		#region FILTERS_CHK_FIVE_SEATS
		
		public abstract Rectangle FILTERS_CHK_FIVE_SEATS { get; }
		
		#endregion
		
		#region FILTERS_CHK_SIX_SEATS
		
		public abstract Rectangle FILTERS_CHK_SIX_SEATS { get; }
		
		#endregion
		
		#region FILTERS_CHK_SEVEN_SEATS
		
		public abstract Rectangle FILTERS_CHK_SEVEN_SEATS { get; }
		
		#endregion
		
		#region FILTERS_CHK_EIGHT_SEATS
		
		public abstract Rectangle FILTERS_CHK_EIGHT_SEATS { get; }
		
		#endregion
		
		#region FILTERS_CHK_NINE_SEATS
		
		public abstract Rectangle FILTERS_CHK_NINE_SEATS { get; }
		
		#endregion
		
		#region FILTERS_CHK_TEN_SEATS
		
		public abstract Rectangle FILTERS_CHK_TEN_SEATS { get; }
		
		#endregion
		
		#region FILTERS_CHK_TURBO
		
		public abstract Rectangle FILTERS_CHK_TURBO { get; }
		
		#endregion
		
		#region FILTERS_CHK_POT_LIMIT
		
		public abstract Rectangle FILTERS_CHK_POT_LIMIT { get; }
		
		#endregion
		
		#region FILTERS_CHK_NO_LIMIT
		
		public abstract Rectangle FILTERS_CHK_NO_LIMIT { get; }
		
		#endregion
		
		#region FILTERS_CHK_LIMIT
		
		public abstract Rectangle FILTERS_CHK_LIMIT { get; }
		
		#endregion
		
		#region FILTERS_MIN_VALUE
		
		public abstract Rectangle FILTERS_MIN_VALUE { get; }
		
		#endregion
		
		#region FILTERS_MAX_VALUE
		
		public abstract Rectangle FILTERS_MAX_VALUE { get; }
		
		#endregion
		
		#region FILTERS_TXT_BOX
		
		public abstract Rectangle FILTERS_TXT_BOX { get; }
		
		#endregion
		
		#region FILTERS_MIN_DROP_DOWN_OPTION_1
		
		public abstract Rectangle FILTERS_MIN_DROP_DOWN_OPTION_1 { get; }
		
		#endregion
		
		#region FILTERS_MIN_DROP_DOWN_OPTION_2
		
		public abstract Rectangle FILTERS_MIN_DROP_DOWN_OPTION_2 { get; }
		
		#endregion
		
		#region FILTERS_MIN_DROP_DOWN_OPTION_3
		
		public abstract Rectangle FILTERS_MIN_DROP_DOWN_OPTION_3 { get; }
		
		#endregion
		
		#region FILTERS_MIN_DROP_DOWN_UP_ARROW
		
		public abstract Rectangle FILTERS_MIN_DROP_DOWN_UP_ARROW { get; }
		
		#endregion
		
		#region FILTERS_MAX_DROP_DOWN_UP_ARROW
		
		public abstract Rectangle FILTERS_MAX_DROP_DOWN_UP_ARROW { get; }
		
		#endregion
		
		#region FILTERS_MIN_DROP_DOWN_DOWN_ARROW
		
		public abstract Rectangle FILTERS_MIN_DROP_DOWN_DOWN_ARROW { get; }
		
		#endregion
		
		#region FILTERS_MAX_DROP_DOWN_DOWN_ARROW
		
		public abstract Rectangle FILTERS_MAX_DROP_DOWN_DOWN_ARROW { get; }
		
		#endregion
		
		#region FILTERS_MAX_DROP_DOWN_OPTION_1
		
		public abstract Rectangle FILTERS_MAX_DROP_DOWN_OPTION_1 { get; }
		
		#endregion
		
		#region FILTERS_MAX_DROP_DOWN_OPTION_2
		
		public abstract Rectangle FILTERS_MAX_DROP_DOWN_OPTION_2 { get; }
		
		#endregion
		
		#region FILTERS_MAX_DROP_DOWN_OPTION_3
		
		public abstract Rectangle FILTERS_MAX_DROP_DOWN_OPTION_3 { get; }
		
		#endregion
		
		#region TABLEGRID_ROW_DIM
		
		public abstract Rectangle TABLEGRID_ROW_DIM { get; }
		
		#endregion
		
		#region TABLEGRID_PLRS_SORTING_DECREMENTING
		
		public abstract Rectangle TABLEGRID_PLRS_SORTING_DECREMENTING { get; }

        #endregion

        #region TABLE_GRID_TABLE_1_TABLENAME

        public abstract Rectangle TABLE_GRID_TABLE_1_TABLENAME { get; }

        #endregion

        #region POPUP_CANCEL

        public abstract Rectangle POPUP_CANCEL { get; }

        #endregion

        #region POPUP_CIRCLE

        public abstract Rectangle POPUP_CIRCLE { get; }

        #endregion

        #region POPUP_YES

        public abstract Rectangle POPUP_YES { get; }

        #endregion

        #region TABLE_GRID_TABLE_1_TABLENAME_NUMBERS

        public abstract Rectangle TABLE_GRID_TABLE_1_TABLENAME_NUMBERS { get; }

        #endregion

    }
}