using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OpenCL
{
    public static class OpenCLController
    {
        private static readonly object lockObject = new object();

        // Note that if the functions is using the GPU behind, it might be necessary to use locks
        // Mother function of them all
        public static unsafe Dictionary<Bitmap, Dictionary<Point, List<int>>> CalculateDistances(List<Bitmap> samplesList, List<Point> sampleCoordsList, List<Bitmap> referencesList)
        {
            List<BitmapData> refbmpDataList = new List<BitmapData>();

            foreach (var item in referencesList)
            {
                refbmpDataList.Add(item.LockBits(new Rectangle(0, 0, item.Width, item.Height), ImageLockMode.ReadOnly, item.PixelFormat));
            }
            int referenceStride = refbmpDataList.First().Stride;
            List<BitmapData> samplesbmpDataList = new List<BitmapData>();

            foreach (var item in samplesList)
            {
                samplesbmpDataList.Add(item.LockBits(new Rectangle(0, 0, item.Width, item.Height), ImageLockMode.ReadOnly, item.PixelFormat));
            }

            int sampleStride = samplesbmpDataList[0].Stride;
            int[] refBmpPtrs = new int[refbmpDataList.Count];
            int ind = 0;
            foreach (var item in refbmpDataList)
            {
                refBmpPtrs[ind++] = (int)item.Scan0.ToPointer();
            }

            IntPtr references = Marshal.AllocHGlobal(Marshal.SizeOf(ind) * refBmpPtrs.Length);
            Marshal.Copy(refBmpPtrs, 0, references, refBmpPtrs.Length);
            int samplesSize = samplesbmpDataList.Count * sampleCoordsList.Count;
            int referencesSize = refbmpDataList.Count;
            int width = referencesList[0].Width;
            int height = referencesList[0].Height;
            int[] sampleBmpPtrs = new int[samplesSize];
            ind = 0;
            foreach (var item in samplesbmpDataList)
            {
                foreach (var rec in sampleCoordsList)
                {
                    sampleBmpPtrs[ind++] = (int)(((byte*)item.Scan0.ToPointer()) + (rec.Y * item.Stride + rec.X * 3));
                }
            }

            IntPtr samples = Marshal.AllocHGlobal(sizeof(int) * sampleBmpPtrs.Length);
            Marshal.Copy(sampleBmpPtrs, 0, samples, sampleBmpPtrs.Length);

            int[] retValsPtrs = new int[samplesSize];
            ind = 0;
            for (int i = 0; i < samplesSize; i++)
            {
                retValsPtrs[i] = (int)Marshal.AllocHGlobal(sizeof(int) * referencesSize).ToPointer();
            }
            IntPtr retVals = Marshal.AllocHGlobal(Marshal.SizeOf(retValsPtrs[0]) * retValsPtrs.Length);
            Marshal.Copy(retValsPtrs, 0, retVals, retValsPtrs.Length);

            int ret = (int)OpenCLImageAnalyseDLLImports.SingleThreadBitmapAnalyse(
                (void**)samples.ToPointer(), samplesSize, sampleStride,
                (void**)references.ToPointer(), referencesSize, referenceStride,
                width, height, (int**)retVals.ToPointer());

            //Console.WriteLine("ret: {0}", ret);
            int[] ptrs = new int[samplesSize];
            Marshal.Copy(retVals, ptrs, 0, samplesSize);

            List<int[]> values = new List<int[]>();
            Dictionary<Bitmap, Dictionary<Point, List<int>>> retDic =
                new Dictionary<Bitmap, Dictionary<Point, List<int>>>();
            for (int i = 0; i < samplesSize; i++)
            {
                Tuple<int, int> retDicKey = new Tuple<int, int>(i / sampleCoordsList.Count, i % sampleCoordsList.Count);
                if (i % sampleCoordsList.Count == 0)
                    retDic.Add(samplesList[i / sampleCoordsList.Count], new Dictionary<Point, List<int>>());
                int[] tempPtr = new int[referencesSize];
                Marshal.Copy((IntPtr)ptrs[i], tempPtr, 0, referencesSize);
                retDic[samplesList[i / sampleCoordsList.Count]].Add(sampleCoordsList[i % sampleCoordsList.Count], tempPtr.OfType<int>().ToList());
            }

            for (int i = 0; i < samplesbmpDataList.Count; i++)
            {
                samplesList[i].UnlockBits(samplesbmpDataList[i]);
            }

            for (int i = 0; i < refbmpDataList.Count; i++)
            {
                referencesList[i].UnlockBits(refbmpDataList[i]);
            }

            for (int i = 0; i < samplesSize; i++)
            {
                Marshal.FreeHGlobal((IntPtr)retValsPtrs[i]);
            }
            Marshal.FreeHGlobal(retVals);
            Marshal.FreeHGlobal(samples);
            Marshal.FreeHGlobal(references);

            return retDic;

        }

        public static unsafe Dictionary<Bitmap, Dictionary<Bitmap, int>> CalculateDistances(List<Bitmap> samplesList, List<Bitmap> referencesList)
        {
            var sampleCoordList = new List<Point>();

            sampleCoordList.Add(new Point(0, 0));

            var res = CalculateDistances(samplesList, sampleCoordList, referencesList);

            var ret = new Dictionary<Bitmap, Dictionary<Bitmap, int>>();

            foreach (var sample in samplesList)
            {
                ret.Add(sample, new Dictionary<Bitmap, int>());
                for (int i = 0; i < referencesList.Count; i++)
                {
                    ret[sample].Add(referencesList[i], res[sample].First().Value[i]);
                }
            }

            for (int i = 0; i < referencesList.Count; i++)
            {
                ret.Add(referencesList[i], new Dictionary<Bitmap, int>());
                foreach (var item in samplesList)
                {
                    ret[referencesList[i]].Add(item, res[item].First().Value[i]);
                }
            }

            return ret;
        }

        public static unsafe Dictionary<Point, List<int>> CalculateDistances(Bitmap sample, List<Point> sampleCoordList, List<Bitmap> referenceList)
        {
            var sampleList = new List<Bitmap>();

            sampleList.Add(sample);

            return CalculateDistances(sampleList, sampleCoordList, referenceList).First().Value;
        }

        public static unsafe Dictionary<Point, int> CalculateDistances(Bitmap sample, List<Point> sampleCoordList, Bitmap reference)
        {
            var sampleList = new List<Bitmap>();
            var referenceList = new List<Bitmap>();

            sampleList.Add(sample);
            referenceList.Add(reference);

            var res = CalculateDistances(sampleList, sampleCoordList, referenceList);

            var ret = new Dictionary<Point, int>();
            for (int i = 0; i < res.First().Value.Count; i++)
            {
                ret.Add(sampleCoordList[i], res.First().Value[sampleCoordList[i]].Min());
            }

            return ret;
        }

        public static unsafe int CalculateDistances(Bitmap sample, Point coord, Bitmap reference)
        {
            var sampleCoordList = new List<Point>();

            sampleCoordList.Add(coord);

            var res = CalculateDistances(sample, sampleCoordList, reference);

            return res.First().Value;
        }

        public static unsafe List<int> CalculateDistances(Bitmap sample, Point coord, List<Bitmap> referenceList)
        {
            var sampleCoordList = new List<Point>();

            sampleCoordList.Add(coord);

            return CalculateDistances(sample, sampleCoordList, referenceList).First().Value;
        }

        public static unsafe List<int> CalculateDistances(Bitmap sample, List<Bitmap> referenceList)
        {
            return CalculateDistances(sample, new Point(0, 0), referenceList);
        }

        public static unsafe int CalculateDistances(Bitmap sample, Bitmap reference)
        {
            return CalculateDistances(sample, new Point(0, 0), reference);
        }
    }
}
