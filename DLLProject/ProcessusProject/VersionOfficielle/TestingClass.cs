using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VersionOfficielle.Properties;

namespace VersionOfficielle
{
    static class TestingClass
    {

        static public void testOpenCLControllerCalculateDistancesFunction()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
           /* Resources.CConstants c = new Resources.TwoMax.EspaceJeux.Constantes();
            Resources.CReferences r = new Resources.TwoMax.EspaceJeux.References();*/

            List<Bitmap> bmpRefList = new List<Bitmap>();


           // bmpRefList = r.PHandCardTYPEReferenceList.ConvertAll(x => x.Item2);

            bmpRefList.Add(Properties.Resources.R2);
            bmpRefList.Add(Properties.Resources.R3);
            bmpRefList.Add(Properties.Resources.R4);
            bmpRefList.Add(Properties.Resources.R5);
            bmpRefList.Add(Properties.Resources.R6);
            bmpRefList.Add(Properties.Resources.R7);
            bmpRefList.Add(Properties.Resources.R8);
            bmpRefList.Add(Properties.Resources.R9);
            bmpRefList.Add(Properties.Resources.R10);
            bmpRefList.Add(Properties.Resources.RJ);
            bmpRefList.Add(Properties.Resources.RQ);
            bmpRefList.Add(Properties.Resources.RK);
            bmpRefList.Add(Properties.Resources.RA);

            bmpRefList.Add(Properties.Resources.B2);
            bmpRefList.Add(Properties.Resources.B3);
            bmpRefList.Add(Properties.Resources.B4);
            bmpRefList.Add(Properties.Resources.B5);
            bmpRefList.Add(Properties.Resources.B6);
            bmpRefList.Add(Properties.Resources.B7);
            bmpRefList.Add(Properties.Resources.B8);
            bmpRefList.Add(Properties.Resources.B9);
            bmpRefList.Add(Properties.Resources.B10);
            bmpRefList.Add(Properties.Resources.BJ);
            bmpRefList.Add(Properties.Resources.BQ);
            bmpRefList.Add(Properties.Resources.BK);
            bmpRefList.Add(Properties.Resources.BA);


            List<Bitmap> bmpSamplesList = new List<Bitmap>();

            List<Point> coordList = new List<Point>();

            
            bmpSamplesList.Add(Properties.Resources.Sample0);
            coordList.Add(new Point(Consantes.HAND_OFFSET1_WIDTH, Consantes.HAND_OFFSET1_HEIGHT));
            coordList.Add(new Point(Consantes.HAND_OFFSET2_WIDTH, Consantes.HAND_OFFSET1_HEIGHT));

            /*
            Bitmap sample = new Bitmap(@"C:\Users\olibb\Desktop\ReferencesBMP\asd\ref636717108611124189.bmp");
            bmpSamplesList.Add(sample);
            coordList.Add(new Point(c.HAND_CARD1_VALUE_X, c.HAND_CARD1_VALUE_Y));
            coordList.Add(new Point(c.HAND_CARD2_VALUE_X, c.HAND_CARD2_VALUE_Y));
            */
            //var qwe = OpenCLController.CalculateDistances(bmpSamplesList, coordList, bmpRefList);
            var qwe = OpenCLController.CalculateDistances(bmpSamplesList, coordList, bmpRefList);

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            MessageBox.Show("Time taken for SingleThread: " + elapsedMs + "ms");
        }

    }

}
