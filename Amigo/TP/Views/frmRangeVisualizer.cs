using HoldemHand;
using Shared.Poker.Helpers;
using Shared.Poker.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Amigo.Views
{
    public partial class frmRangeVisualizer : Form
    {
        public frmRangeVisualizer()
        {
            InitializeComponent();
        }

        public void UpdateRange(List<(ulong, double)> _range)
        {
            if (!IsDisposed)
            {
                // If we are NOT on the UI thread
                if (InvokeRequired)
                {
                    Invoke((Action)(() =>
                    {                        
                        var dicCards = new Dictionary<string, List<Tuple<ulong, double>>>(1326);
                        double sumOfProbabilities = 0;

                        foreach ((ulong, double) currentInfos in _range)
                        {
                            string currentCombo = new Hand() { PocketMask = currentInfos.Item1 }.PocketCards;
                            char firstCard = currentCombo[0];
                            char secondCard = currentCombo[3];                            
                            bool isSuited = Hand.IsSuited(currentInfos.Item1);
                            string key = isSuited ? string.Concat(firstCard, secondCard, 's') : string.Concat(firstCard, secondCard);                            

                            if (!dicCards.ContainsKey(key))                            
                                dicCards.Add(key, new List<Tuple<ulong, double>>(16));
                                
                            dicCards[key].Add(new Tuple<ulong, double>(currentInfos.Item1, currentInfos.Item2));
                            sumOfProbabilities += currentInfos.Item2;
                        }

                        foreach (var infos in dicCards)
                        {
                            double realProbability = infos.Value.Sum(x => x.Item2);

                            chartRange[infos.Key[0], infos.Key[1], infos.Key.Length == 3].DisplayValue = infos.Key + Environment.NewLine + Math.Round(realProbability * 100, 2).ToString() + " %";
                        }

                        if (sumOfProbabilities < 0.99 && sumOfProbabilities > 1.01)
                            throw new Exception("Probabilities are not at 1");

                        Update();
                        Refresh();
                    }));
                }
            }
        }
    }
}
