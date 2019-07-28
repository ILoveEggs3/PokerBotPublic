using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resources.SixMax.EspaceJeux
{
	 public sealed class Constantes
	 {
		#region BOARD_CARD1_TYPE
		
		private static readonly Rectangle FFBOARD_CARD1_TYPE = new Rectangle(208, 180, 15, 15);
		public override Rectangle BOARD_CARD1_TYPE { get { return FFBOARD_CARD1_TYPE; } }
		
		#endregion
		
		#region BOARD_CARD1_VALUE
		
		private static readonly Rectangle FFBOARD_CARD1_VALUE = new Rectangle(209, 164, 15, 15);
		public override Rectangle BOARD_CARD1_VALUE { get { return FFBOARD_CARD1_VALUE; } }
		
		#endregion
		
		#region BOARD_CARD2_TYPE
		
		private static readonly Rectangle FFBOARD_CARD2_TYPE = new Rectangle(252, 180, 15, 15);
		public override Rectangle BOARD_CARD2_TYPE { get { return FFBOARD_CARD2_TYPE; } }
		
		#endregion
		
		#region BOARD_CARD2_VALUE
		
		private static readonly Rectangle FFBOARD_CARD2_VALUE = new Rectangle(253, 164, 15, 15);
		public override Rectangle BOARD_CARD2_VALUE { get { return FFBOARD_CARD2_VALUE; } }
		
		#endregion
		
		#region BOARD_CARD3_TYPE
		
		private static readonly Rectangle FFBOARD_CARD3_TYPE = new Rectangle(297, 180, 15, 15);
		public override Rectangle BOARD_CARD3_TYPE { get { return FFBOARD_CARD3_TYPE; } }
		
		#endregion
		
		#region BOARD_CARD3_VALUE
		
		private static readonly Rectangle FFBOARD_CARD3_VALUE = new Rectangle(298, 164, 15, 15);
		public override Rectangle BOARD_CARD3_VALUE { get { return FFBOARD_CARD3_VALUE; } }
		
		#endregion
		
		#region BOARD_CARD4_TYPE
		
		private static readonly Rectangle FFBOARD_CARD4_TYPE = new Rectangle(343, 155, 15, 15);
		public override Rectangle BOARD_CARD4_TYPE { get { return FFBOARD_CARD4_TYPE; } }
		
		#endregion
		
		#region BOARD_CARD4_VALUE
		
		private static readonly Rectangle FFBOARD_CARD4_VALUE = new Rectangle(344, 139, 15, 15);
		public override Rectangle BOARD_CARD4_VALUE { get { return FFBOARD_CARD4_VALUE; } }
		
		#endregion
		
		#region BOARD_CARD5_TYPE
		
		private static readonly Rectangle FFBOARD_CARD5_TYPE = new Rectangle(389, 155, 15, 15);
		public override Rectangle BOARD_CARD5_TYPE { get { return FFBOARD_CARD5_TYPE; } }
		
		#endregion
		
		#region BOARD_CARD5_VALUE
		
		private static readonly Rectangle FFBOARD_CARD5_VALUE = new Rectangle(390, 139, 15, 15);
		public override Rectangle BOARD_CARD5_VALUE { get { return FFBOARD_CARD5_VALUE; } }
		
		#endregion
		
		#region BUTTON_ALLIN
		
		private static readonly Rectangle FFBUTTON_ALLIN = new Rectangle(569, 405, 26, 9);
		public override Rectangle BUTTON_ALLIN { get { return FFBUTTON_ALLIN; } }
		
		#endregion
		
		#region BUTTON_CHECK
		
		private static readonly Rectangle FFBUTTON_CHECK = new Rectangle(435, 442, 85, 15);
		public override Rectangle BUTTON_CHECK { get { return FFBUTTON_CHECK; } }
		
		#endregion
		
		#region BUTTON_FOLD
		
		private static readonly Rectangle FFBUTTON_FOLD = new Rectangle(435, 476, 85, 15);
		public override Rectangle BUTTON_FOLD { get { return FFBUTTON_FOLD; } }
		
		#endregion
		
		#region BUTTON_RAISE
		
		private static readonly Rectangle FFBUTTON_RAISE = new Rectangle(532, 442, 45, 15);
		public override Rectangle BUTTON_RAISE { get { return FFBUTTON_RAISE; } }
		
		#endregion
		
		#region HAND_CARD1_TYPE
		
		private static readonly Rectangle FFHAND_CARD1_TYPE = new Rectangle(525, 253, 13, 13);
		public override Rectangle HAND_CARD1_TYPE { get { return FFHAND_CARD1_TYPE; } }
		
		#endregion
		
		#region HAND_CARD1_VALUE
		
		private static readonly Rectangle FFHAND_CARD1_VALUE = new Rectangle(525, 240, 13, 13);
		public override Rectangle HAND_CARD1_VALUE { get { return FFHAND_CARD1_VALUE; } }
		
		#endregion
		
		#region HAND_CARD2_TYPE
		
		private static readonly Rectangle FFHAND_CARD2_TYPE = new Rectangle(541, 253, 13, 13);
		public override Rectangle HAND_CARD2_TYPE { get { return FFHAND_CARD2_TYPE; } }
		
		#endregion
		
		#region HAND_CARD2_VALUE
		
		private static readonly Rectangle FFHAND_CARD2_VALUE = new Rectangle(541, 240, 13, 13);
		public override Rectangle HAND_CARD2_VALUE { get { return FFHAND_CARD2_VALUE; } }
		
		#endregion
		
		#region HERO_BET
		
		private static readonly Rectangle FFHERO_BET = new Rectangle(440, 261, 50, 6);
		public override Rectangle HERO_BET { get { return FFHERO_BET; } }
		
		#endregion
		
		#region HERO_DEALER
		
		private static readonly Rectangle FFHERO_DEALER = new Rectangle(491, 288, 10, 10);
		public override Rectangle HERO_DEALER { get { return FFHERO_DEALER; } }
		
		#endregion
		
		#region HERO_TURN_INDICATOR
		
		private static readonly Rectangle FFHERO_TURN_INDICATOR = new Rectangle(530, 273, 7, 7);
		public override Rectangle HERO_TURN_INDICATOR { get { return FFHERO_TURN_INDICATOR; } }
		
		#endregion
		
		#region POPUP_CIRCLE
		
		private static readonly Rectangle FFPOPUP_CIRCLE = new Rectangle(281, 261, 10, 10);
		public override Rectangle POPUP_CIRCLE { get { return FFPOPUP_CIRCLE; } }
		
		#endregion
		
		#region STACK_PLAYER_0
		
		private static readonly Rectangle FFSTACK_PLAYER_0 = new Rectangle(41, 225, 45, 7);
		public override Rectangle STACK_PLAYER_0 { get { return FFSTACK_PLAYER_0; } }
		
		#endregion
		
		#region STACK_PLAYER_1
		
		private static readonly Rectangle FFSTACK_PLAYER_1 = new Rectangle(82, 358, 45, 7);
		public override Rectangle STACK_PLAYER_1 { get { return FFSTACK_PLAYER_1; } }
		
		#endregion
		
		#region STACK_PLAYER_2
		
		private static readonly Rectangle FFSTACK_PLAYER_2 = new Rectangle(222, 385, 45, 7);
		public override Rectangle STACK_PLAYER_2 { get { return FFSTACK_PLAYER_2; } }
		
		#endregion
		
		#region STACK_PLAYER_3
		
		private static readonly Rectangle FFSTACK_PLAYER_3 = new Rectangle(369, 385, 45, 7);
		public override Rectangle STACK_PLAYER_3 { get { return FFSTACK_PLAYER_3; } }
		
		#endregion
		
		#region STACK_PLAYER_4
		
		private static readonly Rectangle FFSTACK_PLAYER_4 = new Rectangle(509, 358, 45, 7);
		public override Rectangle STACK_PLAYER_4 { get { return FFSTACK_PLAYER_4; } }
		
		#endregion
		
		#region STACK_PLAYER_5
		
		private static readonly Rectangle FFSTACK_PLAYER_5 = new Rectangle(556, 225, 45, 7);
		public override Rectangle STACK_PLAYER_5 { get { return FFSTACK_PLAYER_5; } }
		
		#endregion
		
		#region TABLE_DIMENSION
		
		private static readonly Rectangle FFTABLE_DIMENSION = new Rectangle(0, 0, 640, 507);
		public override Rectangle TABLE_DIMENSION { get { return FFTABLE_DIMENSION; } }
		
		#endregion
		
		#region TABLE_DOT_DIMENSION
		
		private static readonly Rectangle FFTABLE_DOT_DIMENSION = new Rectangle(0, 0, 3, 6);
		public override Rectangle TABLE_DOT_DIMENSION { get { return FFTABLE_DOT_DIMENSION; } }
		
		#endregion
		
		#region TABLE_NUMBER_DIMENSION
		
		private static readonly Rectangle FFTABLE_NUMBER_DIMENSION = new Rectangle(0, 0, 5, 6);
		public override Rectangle TABLE_NUMBER_DIMENSION { get { return FFTABLE_NUMBER_DIMENSION; } }
		
		#endregion
		
		#region TABLE_TOTAL_POT
		
		private static readonly Rectangle FFTABLE_TOTAL_POT = new Rectangle(321, 254, 50, 6);
		public override Rectangle TABLE_TOTAL_POT { get { return FFTABLE_TOTAL_POT; } }
		
		#endregion
		
		#region TXT_RAISE
		
		private static readonly Rectangle FFTXT_RAISE = new Rectangle(579, 444, 34, 11);
		public override Rectangle TXT_RAISE { get { return FFTXT_RAISE; } }
		
		#endregion
		
		#region VILAIN_BET
		
		private static readonly Rectangle FFVILAIN_BET = new Rectangle(140, 261, 50, 6);
		public override Rectangle VILAIN_BET { get { return FFVILAIN_BET; } }
		
		#endregion
		
		#region VILAIN_DEALER
		
		private static readonly Rectangle FFVILAIN_DEALER = new Rectangle(141, 289, 10, 10);
		public override Rectangle VILAIN_DEALER { get { return FFVILAIN_DEALER; } }
		
		#endregion
		
		#region VILAIN_NAME
		
		private static readonly Rectangle FFVILAIN_NAME = new Rectangle(62, 308, 57, 10);
		public override Rectangle VILAIN_NAME { get { return FFVILAIN_NAME; } }
		
		#endregion
		
		#region VILAIN_STACK
		
		private static readonly Rectangle FFVILAIN_STACK = new Rectangle(68, 319, 45, 8);
		public override Rectangle VILAIN_STACK { get { return FFVILAIN_STACK; } }
		
		#endregion
		
		#region WINDOW_BUTTON_X
		
		private static readonly Rectangle FFWINDOW_BUTTON_X = new Rectangle(601, 12, 25, 13);
		public override Rectangle WINDOW_BUTTON_X { get { return FFWINDOW_BUTTON_X; } }
		
		#endregion
		
		
	 }
}





























