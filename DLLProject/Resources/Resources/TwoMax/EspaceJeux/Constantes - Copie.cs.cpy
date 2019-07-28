using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resources.TwoMax.EspaceJeux
{
	 public sealed class Constantes : CConstantsTable
	 {
		#region ACTION_BUTTON_ALLIN
		
		private static readonly Rectangle FFACTION_BUTTON_ALLIN = new Rectangle(569, 405, 26, 9);
		public override Rectangle ACTION_BUTTON_ALLIN { get { return FFACTION_BUTTON_ALLIN; } }
		
		#endregion
		
		#region ACTION_BUTTON_CHECK
		
		private static readonly Rectangle FFACTION_BUTTON_CHECK = new Rectangle(435, 442, 85, 15);
		public override Rectangle ACTION_BUTTON_CHECK { get { return FFACTION_BUTTON_CHECK; } }
		
		#endregion
		
		#region ACTION_BUTTON_FOLD
		
		private static readonly Rectangle FFACTION_BUTTON_FOLD = new Rectangle(435, 476, 85, 15);
		public override Rectangle ACTION_BUTTON_FOLD { get { return FFACTION_BUTTON_FOLD; } }
		
		#endregion
		
		#region ACTION_BUTTON_RAISE
		
		private static readonly Rectangle FFACTION_BUTTON_RAISE = new Rectangle(532, 442, 45, 15);
		public override Rectangle ACTION_BUTTON_RAISE { get { return FFACTION_BUTTON_RAISE; } }
		
		#endregion
		
		#region ACTION_TXT_RAISE
		
		private static readonly Rectangle FFACTION_TXT_RAISE = new Rectangle(579, 444, 34, 11);
		public override Rectangle ACTION_TXT_RAISE { get { return FFACTION_TXT_RAISE; } }
		
		#endregion
		
		#region BET_DOT_DIMENSION
		
		private static readonly Rectangle FFBET_DOT_DIMENSION = new Rectangle(0, 0, 3, 6);
		public override Rectangle BET_DOT_DIMENSION { get { return FFBET_DOT_DIMENSION; } }
		
		#endregion
		
		#region BET_NUMBER_DIMENSION
		
		private static readonly Rectangle FFBET_NUMBER_DIMENSION = new Rectangle(0, 0, 5, 6);
		public override Rectangle BET_NUMBER_DIMENSION { get { return FFBET_NUMBER_DIMENSION; } }
		
		#endregion
		
		#region BOARD_CARD1_TYPE
		
		private static readonly Rectangle FFBOARD_CARD1_TYPE = new Rectangle(208, 155, 15, 15);
		public override Rectangle BOARD_CARD1_TYPE { get { return FFBOARD_CARD1_TYPE; } }
		
		#endregion
		
		#region BOARD_CARD1_VALUE
		
		private static readonly Rectangle FFBOARD_CARD1_VALUE = new Rectangle(209, 139, 15, 15);
		public override Rectangle BOARD_CARD1_VALUE { get { return FFBOARD_CARD1_VALUE; } }
		
		#endregion
		
		#region BOARD_CARD2_TYPE
		
		private static readonly Rectangle FFBOARD_CARD2_TYPE = new Rectangle(253, 155, 15, 15);
		public override Rectangle BOARD_CARD2_TYPE { get { return FFBOARD_CARD2_TYPE; } }
		
		#endregion
		
		#region BOARD_CARD2_VALUE
		
		private static readonly Rectangle FFBOARD_CARD2_VALUE = new Rectangle(254, 139, 15, 15);
		public override Rectangle BOARD_CARD2_VALUE { get { return FFBOARD_CARD2_VALUE; } }
		
		#endregion
		
		#region BOARD_CARD3_TYPE
		
		private static readonly Rectangle FFBOARD_CARD3_TYPE = new Rectangle(298, 155, 15, 15);
		public override Rectangle BOARD_CARD3_TYPE { get { return FFBOARD_CARD3_TYPE; } }
		
		#endregion
		
		#region BOARD_CARD3_VALUE
		
		private static readonly Rectangle FFBOARD_CARD3_VALUE = new Rectangle(299, 139, 15, 15);
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
		
		#region PLAYER_0_BET
		
		private static readonly Rectangle FFPLAYER_0_BET = new Rectangle(140, 261, 50, 6);
		public override Rectangle PLAYER_0_BET { get { return FFPLAYER_0_BET; } }
		
		#endregion
		
		#region PLAYER_0_DEALER
		
		private static readonly Rectangle FFPLAYER_0_DEALER = new Rectangle(141, 289, 10, 10);
		public override Rectangle PLAYER_0_DEALER { get { return FFPLAYER_0_DEALER; } }
		
		#endregion
		
		#region PLAYER_0_EMPTY
		
		private static readonly Rectangle FFPLAYER_0_EMPTY = new Rectangle(77, 316, 6, 6);
		public override Rectangle PLAYER_0_EMPTY { get { return FFPLAYER_0_EMPTY; } }
		
		#endregion
		
		#region PLAYER_0_NAME
		
		private static readonly Rectangle FFPLAYER_0_NAME = new Rectangle(62, 308, 57, 10);
		public override Rectangle PLAYER_0_NAME { get { return FFPLAYER_0_NAME; } }
		
		#endregion
		
		#region PLAYER_0_STACK
		
		private static readonly Rectangle FFPLAYER_0_STACK = new Rectangle(68, 319, 45, 7);
		public override Rectangle PLAYER_0_STACK { get { return FFPLAYER_0_STACK; } }
		
		#endregion
		
		#region PLAYER_1_BET
		
		private static readonly Rectangle FFPLAYER_1_BET = new Rectangle(440, 261, 50, 6);
		public override Rectangle PLAYER_1_BET { get { return FFPLAYER_1_BET; } }
		
		#endregion
		
		#region PLAYER_1_DEALER
		
		private static readonly Rectangle FFPLAYER_1_DEALER = new Rectangle(491, 288, 10, 10);
		public override Rectangle PLAYER_1_DEALER { get { return FFPLAYER_1_DEALER; } }
		
		#endregion
		
		#region PLAYER_1_EMPTY
		
		private static readonly Rectangle FFPLAYER_1_EMPTY = new Rectangle(535, 316, 6, 6);
		public override Rectangle PLAYER_1_EMPTY { get { return FFPLAYER_1_EMPTY; } }
		
		#endregion
		
		#region PLAYER_1_STACK
		
		private static readonly Rectangle FFPLAYER_1_STACK = new Rectangle(526, 319, 45, 7);
		public override Rectangle PLAYER_1_STACK { get { return FFPLAYER_1_STACK; } }
		
		#endregion
		
		#region PLAYER_1_TURN
		
		private static readonly Rectangle FFPLAYER_1_TURN = new Rectangle(530, 273, 7, 7);
		public override Rectangle PLAYER_1_TURN { get { return FFPLAYER_1_TURN; } }
		
		#endregion
		
		#region POT
		
		private static readonly Rectangle FFPOT = new Rectangle(321, 254, 50, 6);
		public override Rectangle POT { get { return FFPOT; } }
		
		#endregion
		
		#region POT_DOT_DIMENSION
		
		private static readonly Rectangle FFPOT_DOT_DIMENSION = new Rectangle(0, 0, 3, 6);
		public override Rectangle POT_DOT_DIMENSION { get { return FFPOT_DOT_DIMENSION; } }
		
		#endregion
		
		#region POT_NUMBER_DIMENSION
		
		private static readonly Rectangle FFPOT_NUMBER_DIMENSION = new Rectangle(0, 0, 5, 6);
		public override Rectangle POT_NUMBER_DIMENSION { get { return FFPOT_NUMBER_DIMENSION; } }
		
		#endregion
		
		#region STACK_DOT_DIMENSION
		
		private static readonly Rectangle FFSTACK_DOT_DIMENSION = new Rectangle(0, 0, 3, 7);
		public override Rectangle STACK_DOT_DIMENSION { get { return FFSTACK_DOT_DIMENSION; } }
		
		#endregion
		
		#region STACK_NUMBER_DIMENSION
		
		private static readonly Rectangle FFSTACK_NUMBER_DIMENSION = new Rectangle(0, 0, 6, 7);
		public override Rectangle STACK_NUMBER_DIMENSION { get { return FFSTACK_NUMBER_DIMENSION; } }
		
		#endregion
		
		#region TEST_REGION
		
		private static readonly Rectangle FFTEST_REGION = new Rectangle(554, 319, 6, 7);
		public override Rectangle TEST_REGION { get { return FFTEST_REGION; } }
		
		#endregion
		
		#region WINDOW_BUTTON_X
		
		private static readonly Rectangle FFWINDOW_BUTTON_X = new Rectangle(601, 12, 25, 13);
		public override Rectangle WINDOW_BUTTON_X { get { return FFWINDOW_BUTTON_X; } }
		
		#endregion
		
		#region WINDOW_DIMENSION
		
		private static readonly Rectangle FFWINDOW_DIMENSION = new Rectangle(0, 0, 640, 507);
		public override Rectangle WINDOW_DIMENSION { get { return FFWINDOW_DIMENSION; } }
		
		#endregion
		
		
	 }
}













