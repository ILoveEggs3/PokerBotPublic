using System;
using System.Threading;
using System.Windows.Forms;

using Amigo.Controllers;

namespace Amigo
{
    public static class CApplication
    {        
        private static Form FFirstView;

        public static Thread PMainThread;

        public static void Initialize()
        {
            // Activer les contrôles visuelles de la classe.
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            frmCreerPartie view = new frmCreerPartie();
            CGamesManagerController mainClass = new CGamesManagerController(view);

            FFirstView = view;
            PMainThread = Thread.CurrentThread;
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
