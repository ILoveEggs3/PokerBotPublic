using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenScraping.Readers.TableReader
{
    public interface ITableReader
    {
        bool IsOurTurn();

        Tuple<PokerShared.CCard, PokerShared.CCard> GetHand();

        List<PokerShared.CCard> GetFlop();
        PokerShared.CCard GetTurn();
        PokerShared.CCard GetRiver();

        decimal GetPot(PokerShared.CPokerPositionModel.TenMax _playerPos);
        decimal GetPlayerBet(PokerShared.CPokerPositionModel.TenMax _playerPos);
        decimal GetPlayerStack(PokerShared.CPokerPositionModel.TenMax _playerPos);

        bool IsHeroDealer();
    }
}
