using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoGrabberBoard
{
    
    public class Program
    {
        const int Length = 5;
        public Program()
        {
        }


        public static int AnotherTest()
        {
           // var gpu = Gpu.Default;

            int tata = 1;

            
                tata += 1;
            

            return tata;
        }

        static void Main(string[] args)
        {
            Program program = new Program();
            // program.NumberIncrementTest();
            // program.NumberIncrementUsingClassTest();
            Stopwatch ts = new Stopwatch();
            Console.WriteLine(Program.AnotherTest());
            ts.Stop();
            Console.WriteLine(ts.ElapsedMilliseconds);
            Console.Read();
        }
        /*
        /// <summary>
        /// Test the GPU for a number increment test
        /// </summary>
        private void NumberIncrementTest(long _numberOfIncrementToDo = CNumberCounter.NUMBER_OF_OPERATIONS)
        {
            cudaDeviceProp prop;

            cuda.GetDeviceProperties(out prop, 0);
            cuda.DeviceSynchronize();
            HybRunner runner = HybRunner.Cuda().SetDistrib(CGPUConstants.DEFAULT_NUMBER_OF_BLOCKS_ALL_SP, CGPUConstants.NUMBER_OF_THREADS_ONE_BLOCK);

            // create a wrapper object to call GPU methods instead of C#
            dynamic numberCounterDLL = runner.Wrap(new CNumberCounter());
            Stopwatch watcher = new Stopwatch();

            ulong[] countOut = new ulong[1];
            countOut[0] = 0;

            watcher.Start();
            #region GPU test
            long numberOfOperationsPerThread = Convert.ToInt64((CNumberCounter.NUMBER_OF_OPERATIONS / CGPUConstants.NUMBER_OF_THREADS_ONE_BLOCK) / CGPUConstants.DEFAULT_NUMBER_OF_BLOCKS_ALL_SP);
            long numberOfOperationsLeft = _numberOfIncrementToDo % (numberOfOperationsPerThread * CGPUConstants.NUMBER_OF_THREADS_ONE_BLOCK * CGPUConstants.DEFAULT_NUMBER_OF_BLOCKS_ALL_SP);

            numberCounterDLL.Run(countOut, _numberOfIncrementToDo, numberOfOperationsPerThread);            
            numberCounterDLL.Run(countOut, numberOfOperationsLeft, 1);
            watcher.Stop();

            Console.Out.WriteLine("Length from GPU: {0}", countOut[0]);
            Console.WriteLine("Time elapsed: {0} ms", watcher.ElapsedMilliseconds);
            #endregion

            countOut[0] = 0;

            #region CPU test
            watcher.Reset();
            watcher.Start();
            for (ulong j = 0; j < CNumberCounter.NUMBER_OF_OPERATIONS; ++j)
                ++countOut[0];
            watcher.Stop();

            Console.Out.WriteLine("length from CPU: {0}", countOut[0]);
            Console.WriteLine("Time elapsed: {0} ms", watcher.ElapsedMilliseconds);
            #endregion
        }

        /// <summary>
        /// Test the GPU for a number increment test
        /// </summary>
        private void NumberIncrementUsingClassTest(long _numberOfIncrementToDo = CNumberCounter.NUMBER_OF_OPERATIONS)
        {
            cudaDeviceProp prop;

            cuda.GetDeviceProperties(out prop, 0);
            cuda.DeviceSynchronize();
            HybRunner runner = HybRunner.Cuda().SetDistrib(CGPUConstants.DEFAULT_NUMBER_OF_BLOCKS_ALL_SP, CGPUConstants.NUMBER_OF_THREADS_ONE_BLOCK);

            // create a wrapper object to call GPU methods instead of C#
            dynamic numberCounterDLL = runner.Wrap(new CNumberCounter());
            Stopwatch watcher = new Stopwatch();

            CNumber[] countOut = new CNumber[1];
            countOut[0] = new CNumber(0);

            watcher.Start();
            #region GPU test
            long numberOfOperationsPerThread = Convert.ToInt64((CNumberCounter.NUMBER_OF_OPERATIONS / CGPUConstants.NUMBER_OF_THREADS_ONE_BLOCK) / CGPUConstants.DEFAULT_NUMBER_OF_BLOCKS_ALL_SP);
            long numberOfOperationsLeft = _numberOfIncrementToDo % (numberOfOperationsPerThread * CGPUConstants.NUMBER_OF_THREADS_ONE_BLOCK * CGPUConstants.DEFAULT_NUMBER_OF_BLOCKS_ALL_SP);

            numberCounterDLL.RunWithClass(countOut, _numberOfIncrementToDo, numberOfOperationsPerThread);
            numberCounterDLL.RunWithClass(countOut, numberOfOperationsLeft, 1);
            watcher.Stop();

            Console.Out.WriteLine("Length from GPU: {0}", countOut[0].PNumber);
            Console.WriteLine("Time elapsed: {0} ms", watcher.ElapsedMilliseconds);
            #endregion

            countOut[0].PNumber = 0;

            #region CPU test
            watcher.Reset();
            watcher.Start();
            ulong number = 0;
            for (ulong j = 0; j < CNumberCounter.NUMBER_OF_OPERATIONS; ++j)
                ++number;
            countOut[0].PNumber = number;
            watcher.Stop();

            Console.Out.WriteLine("length from CPU: {0}", countOut[0].PNumber);
            Console.WriteLine("Time elapsed: {0} ms", watcher.ElapsedMilliseconds);
            #endregion
        }*/
    }
}
