using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCLTestApp
{
    static class TestingClass
    {
        static public unsafe void testBitmapDistancesV1()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            List<Bitmap> bmpRefList = new List<Bitmap>();

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


            Dictionary<Bitmap, BitmapData> refbmpDataList = new Dictionary<Bitmap, BitmapData>();

            foreach (var item in bmpRefList)
            {
                refbmpDataList.Add(item, item.LockBits(new Rectangle(0, 0, item.Width, item.Height), ImageLockMode.ReadOnly, item.PixelFormat));
            }

            var bmp2 = Properties.Resources.R2;
            BitmapData bmp = bmp2.LockBits(new Rectangle(0, 0, bmp2.Width, bmp2.Height), ImageLockMode.ReadOnly, bmp2.PixelFormat);


            int[] refBmpPtrs = new int[refbmpDataList.Count];
            int ind = 0;
            foreach (var item in refbmpDataList)
            {
                refBmpPtrs[ind++] = (int)item.Value.Scan0.ToPointer();
            }

            int asdasd = (int)((byte*)bmp.Scan0)[0];


            IntPtr qwe = Marshal.AllocHGlobal(Marshal.SizeOf(ind) * refBmpPtrs.Length);
            Marshal.Copy(refBmpPtrs, 0, qwe, refBmpPtrs.Length);

            IntPtr retValues = Marshal.AllocHGlobal(Marshal.SizeOf(ind) * refBmpPtrs.Length);


            int lol = (int)MyClass.BitmapAnalyse((byte*)bmp.Scan0, bmp.Width, bmp.Height, qwe.ToPointer(), refBmpPtrs.Length, (int*)retValues.ToPointer());

            int[] asd = new int[refBmpPtrs.Length];

            Marshal.Copy(retValues, asd, 0, refBmpPtrs.Length);


            Marshal.FreeHGlobal(qwe);
            Marshal.FreeHGlobal(retValues);

            bmp2.UnlockBits(bmp);
            foreach (var item in refbmpDataList)
            {
                item.Key.UnlockBits(item.Value);
            }

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            MessageBox.Show("Time taken for SingleThread: " + elapsedMs + "ms");
        }

        static public unsafe void testBitmapDistancesV2()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            List<Bitmap> bmpRefList = new List<Bitmap>();

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


            Dictionary<Bitmap, BitmapData> refbmpDataList = new Dictionary<Bitmap, BitmapData>();

            foreach (var item in bmpRefList)
            {
                refbmpDataList.Add(item, item.LockBits(new Rectangle(0, 0, item.Width, item.Height), ImageLockMode.ReadOnly, item.PixelFormat));
            }

            List<Bitmap> bmpSamplesList = new List<Bitmap>();

            bmpSamplesList.Add(Properties.Resources.R2);
            bmpSamplesList.Add(Properties.Resources.B2);
            bmpSamplesList.Add(Properties.Resources.B4);
            bmpSamplesList.Add(Properties.Resources.B9);
            bmpSamplesList.Add(Properties.Resources.RA);
            bmpSamplesList.Add(Properties.Resources.R5);

            Dictionary<Bitmap, BitmapData> samplesbmpDataList = new Dictionary<Bitmap, BitmapData>();

            foreach (var item in bmpSamplesList)
            {
                samplesbmpDataList.Add(item, item.LockBits(new Rectangle(0, 0, item.Width, item.Height), ImageLockMode.ReadOnly, item.PixelFormat));
            }

            int samplesSize = samplesbmpDataList.Count;
            int referencesSize = refbmpDataList.Count;
            int width = Properties.Resources.B10.Width;
            int height = Properties.Resources.B10.Height;

            int[] refBmpPtrs = new int[refbmpDataList.Count];
            int ind = 0;
            foreach (var item in refbmpDataList)
            {
                refBmpPtrs[ind++] = (int)item.Value.Scan0.ToPointer();
            }
            IntPtr references = Marshal.AllocHGlobal(Marshal.SizeOf(ind) * refBmpPtrs.Length);
            Marshal.Copy(refBmpPtrs, 0, references, refBmpPtrs.Length);

            int[] sampleBmpPtrs = new int[samplesbmpDataList.Count];
            ind = 0;
            foreach (var item in samplesbmpDataList)
            {
                sampleBmpPtrs[ind++] = (int)item.Value.Scan0.ToPointer();
            }
            IntPtr samples = Marshal.AllocHGlobal(Marshal.SizeOf(ind) * sampleBmpPtrs.Length);
            Marshal.Copy(sampleBmpPtrs, 0, samples, sampleBmpPtrs.Length);

            int[] retValsPtrs = new int[referencesSize * samplesSize];
            ind = 0;
            for (int i = 0; i < samplesSize; i++)
            {
                retValsPtrs[i] = (int)Marshal.AllocHGlobal(Marshal.SizeOf(i) * referencesSize).ToPointer();
            }
            IntPtr retVals = Marshal.AllocHGlobal(Marshal.SizeOf(ind) * retValsPtrs.Length);
            Marshal.Copy(retValsPtrs, 0, retVals, retValsPtrs.Length);

            int ret = (int)MyClass.BitmapAnalyseV2(
                (void**)samples.ToPointer(), samplesSize,
                (void**)references.ToPointer(), referencesSize,
                width, height, (int**)retVals.ToPointer());

            int[] ptrs = new int[samplesSize];
            Marshal.Copy(retVals, ptrs, 0, samplesSize);

            List<int[]> values = new List<int[]>();

            for (int i = 0; i < samplesSize; i++)
            {
                int[] tempPtr = new int[referencesSize];
                Marshal.Copy((IntPtr)ptrs[i], tempPtr, 0, referencesSize);
                values.Add(tempPtr);
            }

            for (int i = 0; i < samplesSize; i++)
            {
                Marshal.FreeHGlobal((IntPtr)retValsPtrs[i]);
            }
            Marshal.FreeHGlobal(retVals);
            Marshal.FreeHGlobal(samples);
            Marshal.FreeHGlobal(references);

            foreach (var item in samplesbmpDataList)
            {
                item.Key.UnlockBits(item.Value);
            }
            foreach (var item in refbmpDataList)
            {
                item.Key.UnlockBits(item.Value);
            }

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            MessageBox.Show("Time taken for SingleThread: " + elapsedMs + "ms");
        }
    }
}
