using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Threading;
using System.Windows.Forms;

using Amigo.Controllers;
using Amigo.Events;
using Amigo.Helpers;
using Amigo.Interfaces;
using Shared.Poker.Helpers;
using static Amigo.Helpers.CDBHelperHandInfos;

namespace Amigo
{
    public static class CApplication
    {
        private static Form FFirstView;

        public static Thread PMainThread;

        private static Dictionary<Form, Form> FFOtherViews;
        public static int toto = 0;

        public static void Initialize()
        {
            GCSettings.LatencyMode = GCLatencyMode.LowLatency;
            // Activer les contrôles visuelles de la classe.
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            #region Load all DBs informations
            CDBHelperHandInfos.LoadAllBoards();
            CDBHelper.LoadAllHandsByCard();
            CDBHelper.LoadAllGameStates();
                      
            CDBHelper.LoadAllAveragePlayerPreflopRanges();            
            CDBHelper.LoadAllAveragePlayerBluffsFlop();
            CDBHelper.LoadAllAveragePlayerWithALotsOfEquityFlop();
            CDBHelper.LoadAllAveragePlayerMadeHandSDAndFDFlop();
            CDBHelper.LoadAllAveragePlayerMadeHandSDFlop();
            CDBHelper.LoadAllAveragePlayerMadeHandFDFlop();
            CDBHelper.LoadAllAveragePlayerValueHandsFlop();


            CDBHelper.LoadAllFlopGameStatesOtherStats();

            CDBHelper.LoadAllAveragePlayerBluffsTurn();
            CDBHelper.LoadAveragePlayerBluffsWithALotsOfEquityTurn();
            CDBHelper.LoadAverageMadeHandSDAndFDTurn();
            CDBHelper.LoadAverageMadeHandSDTurnTable();
            CDBHelper.LoadAverageMadeHandFDTurnTable();
            CDBHelper.LoadAveragePlayerValueHandsTurnTable();

            CDBHelper.LoadAllTurnGameStatesOtherStats();

            CDBHelper.LoadAllAveragePlayerBluffsRiver();
            CDBHelper.LoadAllAveragePlayerWithALotsOfEquityRiver();
            CDBHelper.LoadAveragePlayerValueHandsRiverTable();

            CDBHelper.LoadAllRiverGameStatesOtherStats();

            CDBHelper.LoadAllRangesTotalSamples();
            //CDBHelper.LoadAllRiverBoardTypesByGroupType();
            //CDBHelper.LoadAllRiverGameStatesWithSample();


            //CDBHelper.LoadAllStatesTransitions();

            CDBHelper.LoadAllFlopGameStatesFoldStats();
            CDBHelper.LoadAllTurnGameStatesFoldStats();
            CDBHelper.LoadAllRiverGameStatesFoldStats();

            #endregion

            frmCreerPartie view = new frmCreerPartie();
            CGamesManagerController mainClass = new CGamesManagerController(view);

            FFirstView = view;            
            PMainThread = Thread.CurrentThread;
            FFOtherViews = new Dictionary<Form, Form>(10);

            view.PJeu = mainClass;
            // Affichage de l'interface principale.
            Application.Run(view);
        }

        public static void ExecuteOnMainThread(Action _delegate, object _parameters)
        {
            if (FFirstView == null)
                throw new Exception("Must call Initialize in the class CApplication first!");

            if (_parameters == null)
                FFirstView.Invoke(_delegate);
            else
                FFirstView.Invoke(_delegate, _parameters);
        }

        public static void CreateNewView(Form _view)
        {
            ExecuteOnMainThread(() => { FFOtherViews.Add(_view, _view); }, null);
        }

        public static void ShowNewView(Form _view)
        {
            ExecuteOnMainThread(() => { FFOtherViews[_view].Show(); }, null);
        }

        [STAThread]
        static void Main()
        {
            /*
                Objectif: Démarrer le programme.
            */
            Initialize();
        }
    }
}
