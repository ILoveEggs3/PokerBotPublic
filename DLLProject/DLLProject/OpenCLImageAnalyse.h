// MathLibrary.h - Contains declaration of Function class  
#pragma once  

#ifdef MYEXECREFSDll_EXPORTS
#define MYEXECREFSDll_API __declspec(dllexport) 
#else
#define MYEXECREFSDll_API __declspec(dllimport) 
#endif

namespace OpenCLImageAnalyse
{  
	//test functions
	extern "C" MYEXECREFSDll_API int OCL();
	extern "C" MYEXECREFSDll_API void MatMulV1Call();
	extern "C" MYEXECREFSDll_API void MatMulV2Call();
	extern "C" MYEXECREFSDll_API void MatMulV3Call();
	extern "C" MYEXECREFSDll_API int Test();
	extern "C" MYEXECREFSDll_API int Test2();
	extern "C" MYEXECREFSDll_API void Initialize();

	extern "C" MYEXECREFSDll_API int BitmapAnalyseV2(void** samples, int samplesSize, int sampleStride, void** references, int referencesSize, int referenceStride, int imgWidth, int imgHeigth, int** retValues);
	extern "C" MYEXECREFSDll_API int BitmapAnalyseV3(void** samples, int samplesSize, int sampleStride, void** references, int referencesSize, int referenceStride, int imgWidth, int imgHeigth, int** retValues);
	extern "C" MYEXECREFSDll_API int SingleThreadBitmapAnalyse(void** samples, int samplesSize, int sampleStride, void** references, int referencesSize, int referenceStride, int imgWidth, int imgHeigth, int** retValues);
	extern "C" MYEXECREFSDll_API int MultiThreadBitmapAnalyse(void** samples, int samplesSize, int sampleStride, void** references, int referencesSize, int referenceStride, int imgWidth, int imgHeigth, int** retValues);

}  
