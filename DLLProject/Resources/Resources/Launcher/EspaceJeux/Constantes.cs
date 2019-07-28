using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resources.Launcher.EspaceJeux
{
	 public sealed class Constantes : AbstractConstantes
	 {
		#region FILTERS_BUTTON
		
		private static readonly Rectangle FFFILTERS_BUTTON = new Rectangle(423, 547, 107, 19);
		public override Rectangle FILTERS_BUTTON { get { return FFFILTERS_BUTTON; } }
		
		#endregion
		
		#region FILTERS_CHK_EIGHT_SEATS
		
		private static readonly Rectangle FFFILTERS_CHK_EIGHT_SEATS = new Rectangle(557, 397, 10, 10);
		public override Rectangle FILTERS_CHK_EIGHT_SEATS { get { return FFFILTERS_CHK_EIGHT_SEATS; } }
		
		#endregion
		
		#region FILTERS_CHK_EMPTY
		
		private static readonly Rectangle FFFILTERS_CHK_EMPTY = new Rectangle(677, 277, 10, 10);
		public override Rectangle FILTERS_CHK_EMPTY { get { return FFFILTERS_CHK_EMPTY; } }
		
		#endregion
		
		#region FILTERS_CHK_FIVE_SEATS
		
		private static readonly Rectangle FFFILTERS_CHK_FIVE_SEATS = new Rectangle(557, 337, 10, 10);
		public override Rectangle FILTERS_CHK_FIVE_SEATS { get { return FFFILTERS_CHK_FIVE_SEATS; } }
		
		#endregion
		
		#region FILTERS_CHK_FOUR_SEATS
		
		private static readonly Rectangle FFFILTERS_CHK_FOUR_SEATS = new Rectangle(557, 317, 10, 10);
		public override Rectangle FILTERS_CHK_FOUR_SEATS { get { return FFFILTERS_CHK_FOUR_SEATS; } }
		
		#endregion
		
		#region FILTERS_CHK_FULL
		
		private static readonly Rectangle FFFILTERS_CHK_FULL = new Rectangle(677, 297, 10, 10);
		public override Rectangle FILTERS_CHK_FULL { get { return FFFILTERS_CHK_FULL; } }
		
		#endregion
		
		#region FILTERS_CHK_LIMIT
		
		private static readonly Rectangle FFFILTERS_CHK_LIMIT = new Rectangle(677, 447, 10, 10);
		public override Rectangle FILTERS_CHK_LIMIT { get { return FFFILTERS_CHK_LIMIT; } }
		
		#endregion
		
		#region FILTERS_CHK_NINE_SEATS
		
		private static readonly Rectangle FFFILTERS_CHK_NINE_SEATS = new Rectangle(557, 417, 10, 10);
		public override Rectangle FILTERS_CHK_NINE_SEATS { get { return FFFILTERS_CHK_NINE_SEATS; } }
		
		#endregion
		
		#region FILTERS_CHK_NO_LIMIT
		
		private static readonly Rectangle FFFILTERS_CHK_NO_LIMIT = new Rectangle(677, 407, 10, 10);
		public override Rectangle FILTERS_CHK_NO_LIMIT { get { return FFFILTERS_CHK_NO_LIMIT; } }
		
		#endregion
		
		#region FILTERS_CHK_POT_LIMIT
		
		private static readonly Rectangle FFFILTERS_CHK_POT_LIMIT = new Rectangle(677, 427, 10, 10);
		public override Rectangle FILTERS_CHK_POT_LIMIT { get { return FFFILTERS_CHK_POT_LIMIT; } }
		
		#endregion
		
		#region FILTERS_CHK_REGULAR
		
		private static readonly Rectangle FFFILTERS_CHK_REGULAR = new Rectangle(677, 342, 10, 10);
		public override Rectangle FILTERS_CHK_REGULAR { get { return FFFILTERS_CHK_REGULAR; } }
		
		#endregion
		
		#region FILTERS_CHK_SEVEN_SEATS
		
		private static readonly Rectangle FFFILTERS_CHK_SEVEN_SEATS = new Rectangle(557, 377, 10, 10);
		public override Rectangle FILTERS_CHK_SEVEN_SEATS { get { return FFFILTERS_CHK_SEVEN_SEATS; } }
		
		#endregion
		
		#region FILTERS_CHK_SIX_SEATS
		
		private static readonly Rectangle FFFILTERS_CHK_SIX_SEATS = new Rectangle(557, 357, 10, 10);
		public override Rectangle FILTERS_CHK_SIX_SEATS { get { return FFFILTERS_CHK_SIX_SEATS; } }
		
		#endregion
		
		#region FILTERS_CHK_TEN_SEATS
		
		private static readonly Rectangle FFFILTERS_CHK_TEN_SEATS = new Rectangle(557, 437, 10, 10);
		public override Rectangle FILTERS_CHK_TEN_SEATS { get { return FFFILTERS_CHK_TEN_SEATS; } }
		
		#endregion
		
		#region FILTERS_CHK_THREE_SEATS
		
		private static readonly Rectangle FFFILTERS_CHK_THREE_SEATS = new Rectangle(557, 297, 10, 10);
		public override Rectangle FILTERS_CHK_THREE_SEATS { get { return FFFILTERS_CHK_THREE_SEATS; } }
		
		#endregion
		
		#region FILTERS_CHK_TURBO
		
		private static readonly Rectangle FFFILTERS_CHK_TURBO = new Rectangle(677, 362, 10, 10);
		public override Rectangle FILTERS_CHK_TURBO { get { return FFFILTERS_CHK_TURBO; } }
		
		#endregion
		
		#region FILTERS_CHK_TWO_SEATS
		
		private static readonly Rectangle FFFILTERS_CHK_TWO_SEATS = new Rectangle(557, 277, 10, 10);
		public override Rectangle FILTERS_CHK_TWO_SEATS { get { return FFFILTERS_CHK_TWO_SEATS; } }
		
		#endregion
		
		#region FILTERS_MAX_DROP_DOWN_BUTTON
		
		private static readonly Rectangle FFFILTERS_MAX_DROP_DOWN_BUTTON = new Rectangle(754, 489, 10, 17);
		public override Rectangle FILTERS_MAX_DROP_DOWN_BUTTON { get { return FFFILTERS_MAX_DROP_DOWN_BUTTON; } }
		
		#endregion
		
		#region FILTERS_MAX_DROP_DOWN_DOWN_ARROW
		
		private static readonly Rectangle FFFILTERS_MAX_DROP_DOWN_DOWN_ARROW = new Rectangle(756, 561, 7, 7);
		public override Rectangle FILTERS_MAX_DROP_DOWN_DOWN_ARROW { get { return FFFILTERS_MAX_DROP_DOWN_DOWN_ARROW; } }
		
		#endregion
		
		#region FILTERS_MAX_DROP_DOWN_OPTION_1
		
		private static readonly Rectangle FFFILTERS_MAX_DROP_DOWN_OPTION_1 = new Rectangle(679, 513, 74, 9);
		public override Rectangle FILTERS_MAX_DROP_DOWN_OPTION_1 { get { return FFFILTERS_MAX_DROP_DOWN_OPTION_1; } }
		
		#endregion
		
		#region FILTERS_MAX_DROP_DOWN_OPTION_2
		
		private static readonly Rectangle FFFILTERS_MAX_DROP_DOWN_OPTION_2 = new Rectangle(679, 534, 74, 9);
		public override Rectangle FILTERS_MAX_DROP_DOWN_OPTION_2 { get { return FFFILTERS_MAX_DROP_DOWN_OPTION_2; } }
		
		#endregion
		
		#region FILTERS_MAX_DROP_DOWN_OPTION_3
		
		private static readonly Rectangle FFFILTERS_MAX_DROP_DOWN_OPTION_3 = new Rectangle(679, 555, 74, 9);
		public override Rectangle FILTERS_MAX_DROP_DOWN_OPTION_3 { get { return FFFILTERS_MAX_DROP_DOWN_OPTION_3; } }
		
		#endregion
		
		#region FILTERS_MAX_DROP_DOWN_UP_ARROW
		
		private static readonly Rectangle FFFILTERS_MAX_DROP_DOWN_UP_ARROW = new Rectangle(756, 509, 7, 7);
		public override Rectangle FILTERS_MAX_DROP_DOWN_UP_ARROW { get { return FFFILTERS_MAX_DROP_DOWN_UP_ARROW; } }
		
		#endregion
		
		#region FILTERS_MAX_VALUE
		
		private static readonly Rectangle FFFILTERS_MAX_VALUE = new Rectangle(679, 494, 74, 9);
		public override Rectangle FILTERS_MAX_VALUE { get { return FFFILTERS_MAX_VALUE; } }
		
		#endregion
		
		#region FILTERS_MIN_DROP_DOWN_BUTTON
		
		private static readonly Rectangle FFFILTERS_MIN_DROP_DOWN_BUTTON = new Rectangle(654, 489, 10, 17);
		public override Rectangle FILTERS_MIN_DROP_DOWN_BUTTON { get { return FFFILTERS_MIN_DROP_DOWN_BUTTON; } }
		
		#endregion
		
		#region FILTERS_MIN_DROP_DOWN_DOWN_ARROW
		
		private static readonly Rectangle FFFILTERS_MIN_DROP_DOWN_DOWN_ARROW = new Rectangle(656, 561, 7, 7);
		public override Rectangle FILTERS_MIN_DROP_DOWN_DOWN_ARROW { get { return FFFILTERS_MIN_DROP_DOWN_DOWN_ARROW; } }
		
		#endregion
		
		#region FILTERS_MIN_DROP_DOWN_OPTION_1
		
		private static readonly Rectangle FFFILTERS_MIN_DROP_DOWN_OPTION_1 = new Rectangle(579, 513, 74, 9);
		public override Rectangle FILTERS_MIN_DROP_DOWN_OPTION_1 { get { return FFFILTERS_MIN_DROP_DOWN_OPTION_1; } }
		
		#endregion
		
		#region FILTERS_MIN_DROP_DOWN_OPTION_2
		
		private static readonly Rectangle FFFILTERS_MIN_DROP_DOWN_OPTION_2 = new Rectangle(579, 534, 74, 9);
		public override Rectangle FILTERS_MIN_DROP_DOWN_OPTION_2 { get { return FFFILTERS_MIN_DROP_DOWN_OPTION_2; } }
		
		#endregion
		
		#region FILTERS_MIN_DROP_DOWN_OPTION_3
		
		private static readonly Rectangle FFFILTERS_MIN_DROP_DOWN_OPTION_3 = new Rectangle(579, 555, 74, 9);
		public override Rectangle FILTERS_MIN_DROP_DOWN_OPTION_3 { get { return FFFILTERS_MIN_DROP_DOWN_OPTION_3; } }
		
		#endregion
		
		#region FILTERS_MIN_DROP_DOWN_UP_ARROW
		
		private static readonly Rectangle FFFILTERS_MIN_DROP_DOWN_UP_ARROW = new Rectangle(656, 509, 7, 7);
		public override Rectangle FILTERS_MIN_DROP_DOWN_UP_ARROW { get { return FFFILTERS_MIN_DROP_DOWN_UP_ARROW; } }
		
		#endregion
		
		#region FILTERS_MIN_VALUE
		
		private static readonly Rectangle FFFILTERS_MIN_VALUE = new Rectangle(579, 494, 74, 9);
		public override Rectangle FILTERS_MIN_VALUE { get { return FFFILTERS_MIN_VALUE; } }
		
		#endregion
		
		#region FILTERS_TXT_BOX
		
		private static readonly Rectangle FFFILTERS_TXT_BOX = new Rectangle(613, 534, 155, 21);
		public override Rectangle FILTERS_TXT_BOX { get { return FFFILTERS_TXT_BOX; } }
		
		#endregion
		
		#region FILTERS_USE_FILTERS
		
		private static readonly Rectangle FFFILTERS_USE_FILTERS = new Rectangle(302, 552, 10, 10);
		public override Rectangle FILTERS_USE_FILTERS { get { return FFFILTERS_USE_FILTERS; } }
		
		#endregion
		
		#region POPUP_CANCEL
		
		private static readonly Rectangle FFPOPUP_CANCEL = new Rectangle(459, 421, 30, 8);
		public override Rectangle POPUP_CANCEL { get { return FFPOPUP_CANCEL; } }
		
		#endregion
		
		#region POPUP_CIRCLE
		
		private static readonly Rectangle FFPOPUP_CIRCLE = new Rectangle(281, 261, 10, 10);
		public override Rectangle POPUP_CIRCLE { get { return FFPOPUP_CIRCLE; } }
		
		#endregion
		
		#region POPUP_YES
		
		private static readonly Rectangle FFPOPUP_YES = new Rectangle(329, 421, 30, 8);
		public override Rectangle POPUP_YES { get { return FFPOPUP_YES; } }
		
		#endregion
		
		#region TABLE_GRID_TABLE_1_TABLENAME
		
		private static readonly Rectangle FFTABLE_GRID_TABLE_1_TABLENAME = new Rectangle(56, 283, 100, 10);
		public override Rectangle TABLE_GRID_TABLE_1_TABLENAME { get { return FFTABLE_GRID_TABLE_1_TABLENAME; } }
		
		#endregion
		
		#region TABLE_GRID_TABLE_1_TABLENAME_NUMBERS
		
		private static readonly Rectangle FFTABLE_GRID_TABLE_1_TABLENAME_NUMBERS = new Rectangle(56, 283, 100, 7);
		public override Rectangle TABLE_GRID_TABLE_1_TABLENAME_NUMBERS { get { return FFTABLE_GRID_TABLE_1_TABLENAME_NUMBERS; } }
		
		#endregion
		
		#region TABLEGRID_PLRS_SORTING_DECREMENTING
		
		private static readonly Rectangle FFTABLEGRID_PLRS_SORTING_DECREMENTING = new Rectangle(315, 269, 7, 5);
		public override Rectangle TABLEGRID_PLRS_SORTING_DECREMENTING { get { return FFTABLEGRID_PLRS_SORTING_DECREMENTING; } }
		
		#endregion
		
		#region TABLEGRID_ROW_1
		
		private static readonly Rectangle FFTABLEGRID_ROW_1 = new Rectangle(50, 283, 450, 10);
		public override Rectangle TABLEGRID_ROW_1 { get { return FFTABLEGRID_ROW_1; } }
		
		#endregion
		
		#region TABLEGRID_ROW_DIM
		
		private static readonly Rectangle FFTABLEGRID_ROW_DIM = new Rectangle(1, 15, 450, 16);
		public override Rectangle TABLEGRID_ROW_DIM { get { return FFTABLEGRID_ROW_DIM; } }
		
		#endregion
		
		#region TABLEGRID_TABLE_1_PLRS
		
		private static readonly Rectangle FFTABLEGRID_TABLE_1_PLRS = new Rectangle(285, 283, 19, 10);
		public override Rectangle TABLEGRID_TABLE_1_PLRS { get { return FFTABLEGRID_TABLE_1_PLRS; } }
		
		#endregion
		
		#region WINDOW_DIMENSION
		
		private static readonly Rectangle FFWINDOW_DIMENSION = new Rectangle(0, 0, 816, 639);
		public override Rectangle WINDOW_DIMENSION { get { return FFWINDOW_DIMENSION; } }
		
		#endregion
		
		
	 }
}


