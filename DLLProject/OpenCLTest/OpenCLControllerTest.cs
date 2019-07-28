using System;
using NUnit.Framework;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using ScreenScraping.ScreenCapture;

namespace NUnit.OpenCLTest
{

    [TestFixture]
    class OpenCLControllerTest
    {
        Resources.TwoMax.EspaceJeux.References FFTableReferences;
        Resources.TwoMax.EspaceJeux.Constantes FFTableConstants;
        List<Bitmap> FFSamples;
        Bitmap FFMoneySample;
        ScreenScraping.Readers.TableReader.TwoMax.EspaceJeux.CReaderController FFTableController;

        //http://dotnetpattern.com/nunit-introduction
        [SetUp]
        protected void SetUp()
        {
            FFTableReferences = new Resources.TwoMax.EspaceJeux.References();
            FFTableConstants = new Resources.TwoMax.EspaceJeux.Constantes();
            FFSamples = new List<Bitmap>()
            {
                Properties.Resources.Sample0,
                Properties.Resources.Sample1,
                Properties.Resources.Sample2,
                Properties.Resources.Sample3,
                Properties.Resources.Sample4
            };
            FFMoneySample = Properties.Resources.Sample5;

            FFTableController = new ScreenScraping.Readers.TableReader.TwoMax.EspaceJeux.CReaderController(IntPtr.Zero, DummyScreenCapture.PInstance);
        }

        [Test]
        public void TestSameImage()
        {
            var bmp = FFTableReferences.PHandCardTypeReferenceList[0].Item2;
            var bmp2 = (Bitmap)bmp.Clone();

            var samples = new List<Bitmap>();
            var references = new List<Bitmap>();
            var sampleCoords = new List<Point>();

            sampleCoords.Add(
                new Point(0, 0)
                );
            samples.Add(bmp);
            references.Add(bmp2);

            var distance1 = OpenCL.OpenCLController.CalculateDistances(bmp, bmp2);
            var distance2 = OpenCL.OpenCLController.CalculateDistances(samples, sampleCoords, references).First().Value.First().Value.First();

            Assert.That((distance1 == 0) && (distance2 == 0));
        }

        [Test]
        public void TestDifferentImage()
        {
            var bmp = FFTableReferences.PHandCardTypeReferenceList[0].Item2;
            var bmp2 = FFTableReferences.PHandCardTypeReferenceList[1].Item2;

            var samples = new List<Bitmap>();
            var references = new List<Bitmap>();
            var sampleCoords = new List<Point>();

            sampleCoords.Add(
                new Point(0, 0)
                );
            samples.Add(bmp);
            references.Add(bmp2);

            var distance1 = OpenCL.OpenCLController.CalculateDistances(bmp, bmp2);
            var distance2 = OpenCL.OpenCLController.CalculateDistances(samples, sampleCoords, references).First().Value.First().Value.First();

            Assert.That((distance1 == distance2) && (distance1 != 0));
        }

        [Test]
        public void TestBoard()
        {
            DummyScreenCapture.PInstance.SetBitmap(FFSamples[0]);

            var flopCardList = FFTableController.GetFlop();
            var turnCard = FFTableController.GetTurn();
            var riverCard = FFTableController.GetRiver();

            var c0 = new PokerShared.CCard(PokerShared.CCard.Value.Eight, PokerShared.CCard.Type.Diamonds);
            var c1 = new PokerShared.CCard(PokerShared.CCard.Value.Three, PokerShared.CCard.Type.Hearts);
            var c2 = new PokerShared.CCard(PokerShared.CCard.Value.Five, PokerShared.CCard.Type.Diamonds);
            var c3 = new PokerShared.CCard(PokerShared.CCard.Value.Jack, PokerShared.CCard.Type.Spades);
            var c4 = new PokerShared.CCard(PokerShared.CCard.Value.Ace, PokerShared.CCard.Type.Diamonds);

            Assert.That(
                (c0.PType == flopCardList[0].PType && c0.PValue == flopCardList[0].PValue) && 
                (c1.PType == flopCardList[1].PType && c1.PValue == flopCardList[1].PValue) &&
                (c2.PType == flopCardList[2].PType && c2.PValue == flopCardList[2].PValue) &&
                (c3.PType == turnCard.PType && c3.PValue == turnCard.PValue) &&
                (c4.PType == riverCard.PType && c4.PValue == riverCard.PValue));
        }

        [Test]
        public void TestMoneyRead()
        {
            DummyScreenCapture.PInstance.SetBitmap(FFMoneySample);

            var P0Bet = FFTableController.GetPlayerBet(ScreenScraping.Readers.TableReader.TwoMax.CTableReaderTwoMax.PlayerPosition.P0);
            var P1Bet = FFTableController.GetPlayerBet(ScreenScraping.Readers.TableReader.TwoMax.CTableReaderTwoMax.PlayerPosition.P1);

            var P0Stack = FFTableController.GetPlayerStack(ScreenScraping.Readers.TableReader.TwoMax.CTableReaderTwoMax.PlayerPosition.P0);
            var P1Stack = FFTableController.GetPlayerStack(ScreenScraping.Readers.TableReader.TwoMax.CTableReaderTwoMax.PlayerPosition.P1);

            var Pot = FFTableController.GetPot();

            Assert.That(
                P0Stack == 197.85m &&
                P0Bet == 0m &&
                Pot == 21.85m &&
                P1Bet == 9.35m &&
                P1Stack == 84.65m);
        }

        [TearDown]
        protected void TearDown()
        {

        }
    }
}
