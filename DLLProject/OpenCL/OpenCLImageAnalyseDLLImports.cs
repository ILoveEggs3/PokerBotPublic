using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OpenCL
{
    public static class OpenCLImageAnalyseDLLImports
    {
        [DllImport(@"..\..\..\Debug\OpenCLDLL.dll", EntryPoint = "BitmapAnalyseV3", CallingConvention = CallingConvention.Cdecl)]
        //[DllImport(@"C:\Users\admin\Desktop\dawda\Amigo\DLLProject\OpenCLTest\bin\Debug\OpenCL.dll", EntryPoint = "BitmapAnalyseV3", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern UInt32 BitmapAnalyseV3(void** samples, int samplesSize, int sampleStride, void** references,
            int referencesSize, int referenceStride, int imgWidth, int imgHeigth, int** retValues);

        [DllImport(@"..\..\..\Debug\OpenCLDLL.dll", EntryPoint = "SingleThreadBitmapAnalyse", CallingConvention = CallingConvention.Cdecl)]
        //[DllImport(@"C:\Users\admin\Desktop\dawda\Amigo\DLLProject\OpenCLTest\bin\Debug\OpenCL.dll", EntryPoint = "SingleThreadBitmapAnalyse", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern UInt32 SingleThreadBitmapAnalyse(void** samples, int samplesSize, int sampleStride, void** references,
            int referencesSize, int referenceStride, int imgWidth, int imgHeigth, int** retValues);

        [DllImport(@"..\..\..\Debug\OpenCLDLL.dll", EntryPoint = "MultiThreadBitmapAnalyse", CallingConvention = CallingConvention.Cdecl)]
        //[DllImport(@"C:\Users\admin\Desktop\dawda\Amigo\DLLProject\OpenCLTest\bin\Debug\OpenCL.dll", EntryPoint = "MultiThreadBitmapAnalyse", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern UInt32 MultihreadBitmapAnalyse(void** samples, int samplesSize, int sampleStride, void** references,
            int referencesSize, int referenceStride, int imgWidth, int imgHeigth, int** retValues);
    }

}
