﻿using Amigo.Models.MyModels.GameState;
using Shared.Models.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amigo.Models.MyModels.OtherStats
{
    public class CRiverOtherStats : AOtherStats
    {
        public CRiverGameState PGameState { get; }


        public CRiverOtherStats(CRiverGameState _riverGameState, ushort _boardType, double _boardHeat, long _sampleCount) : base(_boardType, _boardHeat, _sampleCount)
        {
            PGameState = _riverGameState;
        }
    }
}
