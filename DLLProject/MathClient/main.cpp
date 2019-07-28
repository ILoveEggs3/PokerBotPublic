// MathClient.cpp : Defines the entry point for the console application.  
// Compile by using: cl /EHsc /link MathLibrary.lib MathClient.cpp  

#include "stdafx.h"  
#include <iostream>  
#include "OpenCLImageAnalyse.h"  
#include <time.h>
#include <chrono>
#include <thread>
#include <iostream>
#include <string>
#include <stdio.h>

using namespace std;
using namespace OpenCLImageAnalyse;

#pragma region Variables

const int sampleWidth = 640 / 3;
const int sampleHeight = 507;
const int sampleStride = sampleWidth * 3 + sampleWidth % (8 * 5);
const int referenceWidth = 400 / 3;
const int referenceHeight = 400;
const int referenceStride = referenceWidth * 3 + referenceWidth % (8 * 5);

const int samplesSize = 4;
char* samplePtrs[samplesSize];
pair<int, int> coord[4];
const int referencesSize = 120;

char* samples = (char*)malloc(sampleStride * sampleHeight);
char** references = (char**)malloc(sizeof(char*) * referencesSize);
int** retValues = (int**)malloc(sizeof(int) * samplesSize);

#pragma endregion

void SetUpMatrices()
{
	for (size_t i = 0; i < samplesSize; i++)
	{
		*(retValues + i) = (int*)malloc(referencesSize * sizeof(int));
	}
	for (size_t i = 0; i < referencesSize; i++)
	{
		*(references + i) = (char*)malloc(referenceStride * referenceHeight);
	}


	for (size_t j = 0; j < sampleHeight; j++)
	{
		for (size_t i = 0; i < sampleWidth * 3; i++)
		{
			*(samples + (j * sampleStride + i)) = 2;
		}
	}

	for (size_t k = 0; k < referencesSize; k++)
	{
		for (size_t j = 0; j < referenceHeight; j++)
		{
			for (size_t i = 0; i < referenceWidth * 3; i++)
			{
				*(*(references + k) + (j * referenceStride + i)) = k + 1;
			}
		}
	}
	for (size_t k = 0; k < samplesSize; k++)
	{
		for (size_t i = 0; i < referencesSize; i++)
		{
			*(*(retValues + k) + i) = 3;
		}
	}

	coord[0].first = 0;
	coord[0].second = 0;
	coord[1].first = 0;
	coord[1].second = 100;
	coord[2].first = 100;
	coord[2].second = 0;
	coord[3].first = 100;
	coord[3].second = 100;

	for (size_t i = 0; i < 4; i++)
	{
		samplePtrs[i] = samples + coord[i].first * sampleStride + 3 * coord[i].second;
	}
		//OpenCLImageAnalyse::SingleThreadBitmapAnalyse((void**)&samplePtrs[0], samplesSize, sampleStride, (void**)references, referencesSize, referenceStride, referenceWidth, referenceHeight, retValues);
}

void FreeMatrices()
{
	for (size_t i = 0; i < samplesSize; i++)
	{
		free(*(retValues + i));
	}
	for (size_t i = 0; i < referencesSize; i++)
	{
		free(*(references + i));
	}

	free(samples);
	free(references);
	free(retValues);
}

void deviceInfo()
{
	clock_t tStart = clock();
	OpenCLImageAnalyse::OCL();
	printf("Time taken: %.2fs\n", (double)(clock() - tStart) / CLOCKS_PER_SEC);
}

void testMatMul()
{
	clock_t tStart = clock();
	//OpenCLImageAnalyse::MatMulV1Call();
	printf("Time taken: %.2fs\n", (double)(clock() - tStart) / CLOCKS_PER_SEC);
	//std::this_thread::sleep_for(std::chrono::milliseconds(1000));
	tStart = clock();
	OpenCLImageAnalyse::MatMulV2Call();
	printf("Time taken: %.2fs\n", (double)(clock() - tStart) / CLOCKS_PER_SEC);
	//std::this_thread::sleep_for(std::chrono::milliseconds(1000));
	tStart = clock();
	OpenCLImageAnalyse::MatMulV3Call();
	printf("Time taken: %.2fs\n", (double)(clock() - tStart) / CLOCKS_PER_SEC);
}

void testBitmapAnalyseV2()
{
	OpenCLImageAnalyse::BitmapAnalyseV2((void**)&samplePtrs[0], samplesSize, sampleStride, (void**)references, referencesSize, referenceStride, referenceWidth, referenceHeight, retValues);

	int qwe[samplesSize][referencesSize];
	for (size_t k = 0; k < samplesSize; k++)
	{
		for (size_t i = 0; i < referencesSize; i++)
		{
			qwe[k][i] = *((*(retValues)+k) + i);
		}
	}
}

void testBitmapAnalyseV3()
{
	OpenCLImageAnalyse::BitmapAnalyseV3((void**)&samplePtrs[0], samplesSize, sampleStride, (void**)references, referencesSize, referenceStride, referenceWidth, referenceHeight, retValues);

	int qwe[samplesSize][referencesSize];
	for (size_t k = 0; k < samplesSize; k++)
	{
		for (size_t i = 0; i < referencesSize; i++)
		{
			qwe[k][i] = *((*(retValues)+k) + i);
		}
	}
	int asd = 2;
}

void testSingleThreadBitmapAnalyse()
{
	OpenCLImageAnalyse::SingleThreadBitmapAnalyse((void**)&samplePtrs[0], samplesSize, sampleStride, (void**)references, referencesSize, referenceStride, referenceWidth, referenceHeight, retValues);

	int qwe[samplesSize][referencesSize];
	for (size_t k = 0; k < samplesSize; k++)
	{
		for (size_t i = 0; i < referencesSize; i++)
		{
			qwe[k][i] = *((*(retValues)+k) + i);
		}
	}
}

void testMultiThreadBitmapAnalyse()
{
	OpenCLImageAnalyse::MultiThreadBitmapAnalyse((void**)&samplePtrs[0], samplesSize, sampleStride, (void**)references, referencesSize, referenceStride, referenceWidth, referenceHeight, retValues);

	int qwe[samplesSize][referencesSize];
	for (size_t k = 0; k < samplesSize; k++)
	{
		for (size_t i = 0; i < referencesSize; i++)
		{
			qwe[k][i] = *((*(retValues)+k) + i);
		}
	}
}

void BenchMarkTest()
{
	printf("\n*** STARTING BENCHMARK TESTS ***\n\n");
	clock_t tStart0 = clock();
	SetUpMatrices();
	OpenCLImageAnalyse::Initialize();
	clock_t tStart = clock();

	clock_t tStartST = clock();
	printf("***SINGLETHREAD***\n");
	for (size_t i = 0; i < 5; i++)
	{
		clock_t tStart2 = clock();
		testSingleThreadBitmapAnalyse();
		printf("Iteration: %d; Time taken: %.3fs\n", i, (double)(clock() - tStart2) / CLOCKS_PER_SEC);
	}
	printf("---SingleThread Total: Time taken: %.3fs\n\n", (double)(clock() - tStartST) / CLOCKS_PER_SEC);

	clock_t tStartMT = clock();
	printf("***MULTITHREAD***\n");
	for (size_t i = 0; i < 5; i++)
	{
		clock_t tStart2 = clock();
		testMultiThreadBitmapAnalyse();
		printf("Iteration: %d; Time taken: %.3fs\n", i, (double)(clock() - tStart2) / CLOCKS_PER_SEC);
	}
	printf("---MultiThread Total: Time taken: %.3fs\n\n", (double)(clock() - tStartMT) / CLOCKS_PER_SEC);

	clock_t tStartCL = clock();
	printf("***OPENCL***\n");
	for (size_t i = 0; i < 5; i++)
	{
		clock_t tStart2 = clock();
		testBitmapAnalyseV3();
		printf("Iteration: %d; Time taken: %.3fs\n", i, (double)(clock() - tStart2) / CLOCKS_PER_SEC);
	}
	printf("---OpenCL Total: Time taken: %.3fs\n\n", (double)(clock() - tStartCL) / CLOCKS_PER_SEC);

	printf("Only Work; Time taken: %.3fs\n", (double)(clock() - tStart) / CLOCKS_PER_SEC);
	FreeMatrices();
	printf("SetUp + Work; Time taken: %.3fs\n", (double)(clock() - tStart0) / CLOCKS_PER_SEC);
	printf("\n*** END OF BENCHMARK TESTS ***");
}

int main()
{
	BenchMarkTest();
	std::string s;
	std::cin >> s;
	return 0;
}
