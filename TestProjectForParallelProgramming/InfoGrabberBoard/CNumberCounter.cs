using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoGrabberBoard
{
    public class CNumberCounter
    {
        public const long NUMBER_OF_OPERATIONS = 50000000;

        public CNumberCounter()
        {

        }
        public static void Run(ulong[] _countOut, long _numberOfOperations, long _numberOfOperationsPerThread)
        {                        
            /*
            int globalMemoryIndex = (threadIdx.x + (blockIdx.x * blockDim.x));          
            
            if (globalMemoryIndex < _numberOfOperations)
            {
                ulong number = 0;

                for (long i = 0; i < _numberOfOperationsPerThread; ++i)
                    ++number;                

                CGPUHelper.Add(ref _countOut[0], number);
            }       */                                                          
        }
        public static void RunWithClass(CNumber[] _countOut, long _numberOfOperations, long _numberOfOperationsPerThread)
        {
            /*
            int globalMemoryIndex = (threadIdx.x + (blockIdx.x * blockDim.x));

            if (globalMemoryIndex < _numberOfOperations)
            {
                ulong number = 0;

                for (long i = 0; i < _numberOfOperationsPerThread; ++i)
                    ++number;

                ref ulong number2 = ref _countOut[0].PNumber;
                //CGPUHelper.Add(ref numberToAdd[0].PNumber, number);                                
            }*/
        }
    }
}
