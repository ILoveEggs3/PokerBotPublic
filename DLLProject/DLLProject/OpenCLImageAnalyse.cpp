// MathLibrary.cpp : Defines the exported functions for the DLL application.
// Compile by using: cl /EHsc /DMATHLIBRARY_EXPORTS /LD MathLibrary.cpp

#include "stdafx.h"
#include "OpenCLImageAnalyse.h"
#include <stdio.h>
#include <stdlib.h>
#include <math.h>
#include <map>
#include <thread>
#include <vector>
#include <atomic>
#include <mutex>


#ifdef __APPLE__
#include <OpenCL/opencl.h>
#else
#include <CL/cl.h>
#endif

#define MAX_SOURCE_SIZE (0x100000)

namespace OpenCLImageAnalyse
{

#pragma region KERNEL_CODE_FILES

	static std::string PROJECT_ROOT_PATH;

	static std::pair<std::string, std::string> KERNEL_BITMAP_ANALYSE_V2;
	static std::pair<std::string, std::string> KERNEL_BITMAP_ANALYSE_V3;

	static std::pair<std::string, std::string> KERNEL_MATRIX_MUL_V1;
	static std::pair<std::string, std::string> KERNEL_MATRIX_MUL_V2;
	static std::pair<std::string, std::string> KERNEL_MATRIX_MUL_V3;

#pragma endregion

#pragma region OPENCL_VARIABLES

	static cl_context context = NULL;
	static cl_device_id device_id = NULL;
	static std::map<std::pair<std::string, std::string>, cl_kernel> kernels;

#pragma endregion

#pragma region Prototypes

	cl_kernel GetKernelFromFile(std::pair<std::string, std::string> key);
	void GetKernelFromFile(std::string filePath, std::string kernelName);

#pragma endregion

#pragma region UtilitiesFunction

	void Initialize()
	{

#pragma region OPENCL_VARIABLES

		cl_uint ret_num_devices;
		cl_uint ret_num_platforms;
		cl_int ret = clGetPlatformIDs(0, NULL, &ret_num_platforms);
		cl_platform_id *platforms = NULL;
		platforms = (cl_platform_id*)malloc(ret_num_platforms * sizeof(cl_platform_id));
		ret = clGetPlatformIDs(ret_num_platforms, platforms, NULL);
		ret = clGetDeviceIDs(platforms[0], CL_DEVICE_TYPE_ALL, 1,
			&device_id, &ret_num_devices);
		context = clCreateContext(NULL, 1, &device_id, NULL, NULL, &ret);

		free(platforms);

#pragma endregion

#pragma region KERNEL_CODE_FILES

		std::string s = __FILE__;

		auto qwe = s.find("amigo");
		s = s.substr(0, qwe + 6);
		s += "dllproject\\dllproject\\";
		PROJECT_ROOT_PATH = s;

		KERNEL_BITMAP_ANALYSE_V2 = std::pair<std::string, std::string>(PROJECT_ROOT_PATH + "BitmapDistancesV2.cl", "BitmapDistances");
		KERNEL_BITMAP_ANALYSE_V3 = std::pair<std::string, std::string>(PROJECT_ROOT_PATH + "BitmapDistancesV3.cl", "BitmapDistances");

		KERNEL_MATRIX_MUL_V1 = std::pair<std::string, std::string>(PROJECT_ROOT_PATH + "MatrixMulV1.cl", "MatMulKernel");
		KERNEL_MATRIX_MUL_V2 = std::pair<std::string, std::string>(PROJECT_ROOT_PATH + "MatrixMulV2.cl", "MatMulKernel");
		KERNEL_MATRIX_MUL_V3 = std::pair<std::string, std::string>(PROJECT_ROOT_PATH + "MatrixMulV3.cl", "MatMulKernel");

		GetKernelFromFile(KERNEL_BITMAP_ANALYSE_V2);
		GetKernelFromFile(KERNEL_BITMAP_ANALYSE_V3);

		GetKernelFromFile(KERNEL_MATRIX_MUL_V1);
		GetKernelFromFile(KERNEL_MATRIX_MUL_V2);
		GetKernelFromFile(KERNEL_MATRIX_MUL_V3);

#pragma endregion


	}

	cl_kernel GetKernelFromFile(std::pair<std::string, std::string> key)
	{
		auto it = kernels.find(key);
		if (it == kernels.end())
		{
			// Load the kernel source code into the array source_str
			FILE *fp;
			char *source_str;
			size_t source_size;

			auto err = fopen_s(&fp, key.first.c_str(), "r");

			if (err != 0) {
				fprintf(stderr, "Failed to load kernel.\n");
				exit(1);
			}
			source_str = (char*)malloc(MAX_SOURCE_SIZE);
			source_size = fread(source_str, 1, MAX_SOURCE_SIZE, fp);
			fclose(fp);
			cl_program program = clCreateProgramWithSource(context, 1,
				(const char **)&source_str, (const size_t *)&source_size, NULL);
			clBuildProgram(program, 1, &device_id, NULL, NULL, NULL);
			cl_kernel kernel = clCreateKernel(program, key.second.c_str(), NULL);
			kernels[key] = kernel;
			clReleaseProgram(program);
			free(source_str);
		}
		return kernels[key];
	}

	void GetKernelFromFile(std::string filePath, std::string kernelName)
	{
		auto key = std::pair<std::string, std::string>(filePath, kernelName);
		GetKernelFromFile(key);
	}

#pragma endregion

#pragma region OpenCL

#pragma region Util

	void CLPrintDevInfo(cl_device_id device) {
		char device_string[1024];

		// CL_DEVICE_NAME
		clGetDeviceInfo(device, CL_DEVICE_NAME, sizeof(device_string), &device_string, NULL);
		printf("  CL_DEVICE_NAME: \t\t\t%s\n", device_string);

		// CL_DEVICE_VENDOR
		clGetDeviceInfo(device, CL_DEVICE_VENDOR, sizeof(device_string), &device_string, NULL);
		printf("  CL_DEVICE_VENDOR: \t\t\t%s\n", device_string);

		// CL_DRIVER_VERSION
		clGetDeviceInfo(device, CL_DRIVER_VERSION, sizeof(device_string), &device_string, NULL);
		printf("  CL_DRIVER_VERSION: \t\t\t%s\n", device_string);

		// CL_DEVICE_INFO
		cl_device_type type;
		clGetDeviceInfo(device, CL_DEVICE_TYPE, sizeof(type), &type, NULL);
		if (type & CL_DEVICE_TYPE_CPU)
			printf("  CL_DEVICE_TYPE:\t\t\t%s\n", "CL_DEVICE_TYPE_CPU");
		if (type & CL_DEVICE_TYPE_GPU)
			printf("  CL_DEVICE_TYPE:\t\t\t%s\n", "CL_DEVICE_TYPE_GPU");
		if (type & CL_DEVICE_TYPE_ACCELERATOR)
			printf("  CL_DEVICE_TYPE:\t\t\t%s\n", "CL_DEVICE_TYPE_ACCELERATOR");
		if (type & CL_DEVICE_TYPE_DEFAULT)
			printf("  CL_DEVICE_TYPE:\t\t\t%s\n", "CL_DEVICE_TYPE_DEFAULT");

		// CL_DEVICE_MAX_COMPUTE_UNITS
		cl_uint compute_units;
		clGetDeviceInfo(device, CL_DEVICE_MAX_COMPUTE_UNITS, sizeof(compute_units), &compute_units, NULL);
		printf("  CL_DEVICE_MAX_COMPUTE_UNITS:\t\t%u\n", compute_units);

		// CL_DEVICE_MAX_WORK_ITEM_DIMENSIONS
		size_t workitem_dims;
		clGetDeviceInfo(device, CL_DEVICE_MAX_WORK_ITEM_DIMENSIONS, sizeof(workitem_dims), &workitem_dims, NULL);
		printf("  CL_DEVICE_MAX_WORK_ITEM_DIMENSIONS:\t%u\n", workitem_dims);

		// CL_DEVICE_MAX_WORK_ITEM_SIZES
		size_t workitem_size[3];
		clGetDeviceInfo(device, CL_DEVICE_MAX_WORK_ITEM_SIZES, sizeof(workitem_size), &workitem_size, NULL);
		printf("  CL_DEVICE_MAX_WORK_ITEM_SIZES:\t%u / %u / %u \n", workitem_size[0], workitem_size[1], workitem_size[2]);

		// CL_DEVICE_MAX_WORK_GROUP_SIZE
		size_t workgroup_size;
		clGetDeviceInfo(device, CL_DEVICE_MAX_WORK_GROUP_SIZE, sizeof(workgroup_size), &workgroup_size, NULL);
		printf("  CL_DEVICE_MAX_WORK_GROUP_SIZE:\t%u\n", workgroup_size);

		// CL_DEVICE_MAX_CLOCK_FREQUENCY
		cl_uint clock_frequency;
		clGetDeviceInfo(device, CL_DEVICE_MAX_CLOCK_FREQUENCY, sizeof(clock_frequency), &clock_frequency, NULL);
		printf("  CL_DEVICE_MAX_CLOCK_FREQUENCY:\t%u MHz\n", clock_frequency);

		// CL_DEVICE_ADDRESS_BITS
		cl_uint addr_bits;
		clGetDeviceInfo(device, CL_DEVICE_ADDRESS_BITS, sizeof(addr_bits), &addr_bits, NULL);
		printf("  CL_DEVICE_ADDRESS_BITS:\t\t%u\n", addr_bits);

		// CL_DEVICE_MAX_MEM_ALLOC_SIZE
		cl_ulong max_mem_alloc_size;
		clGetDeviceInfo(device, CL_DEVICE_MAX_MEM_ALLOC_SIZE, sizeof(max_mem_alloc_size), &max_mem_alloc_size, NULL);
		printf("  CL_DEVICE_MAX_MEM_ALLOC_SIZE:\t\t%u MByte\n", (unsigned int)(max_mem_alloc_size / (1024 * 1024)));

		// CL_DEVICE_GLOBAL_MEM_SIZE
		cl_ulong mem_size;
		clGetDeviceInfo(device, CL_DEVICE_GLOBAL_MEM_SIZE, sizeof(mem_size), &mem_size, NULL);
		printf("  CL_DEVICE_GLOBAL_MEM_SIZE:\t\t%u MByte\n", (unsigned int)(mem_size / (1024 * 1024)));

		// CL_DEVICE_ERROR_CORRECTION_SUPPORT
		cl_bool error_correction_support;
		clGetDeviceInfo(device, CL_DEVICE_ERROR_CORRECTION_SUPPORT, sizeof(error_correction_support), &error_correction_support, NULL);
		printf("  CL_DEVICE_ERROR_CORRECTION_SUPPORT:\t%s\n", error_correction_support == CL_TRUE ? "yes" : "no");

		// CL_DEVICE_LOCAL_MEM_TYPE
		cl_device_local_mem_type local_mem_type;
		clGetDeviceInfo(device, CL_DEVICE_LOCAL_MEM_TYPE, sizeof(local_mem_type), &local_mem_type, NULL);
		printf("  CL_DEVICE_LOCAL_MEM_TYPE:\t\t%s\n", local_mem_type == 1 ? "local" : "global");

		// CL_DEVICE_LOCAL_MEM_SIZE
		clGetDeviceInfo(device, CL_DEVICE_LOCAL_MEM_SIZE, sizeof(mem_size), &mem_size, NULL);
		printf("  CL_DEVICE_LOCAL_MEM_SIZE:\t\t%u KByte\n", (unsigned int)(mem_size / 1024));

		// CL_DEVICE_MAX_CONSTANT_BUFFER_SIZE
		clGetDeviceInfo(device, CL_DEVICE_MAX_CONSTANT_BUFFER_SIZE, sizeof(mem_size), &mem_size, NULL);
		printf("  CL_DEVICE_MAX_CONSTANT_BUFFER_SIZE:\t%u KByte\n", (unsigned int)(mem_size / 1024));

		// CL_DEVICE_QUEUE_PROPERTIES
		cl_command_queue_properties queue_properties;
		clGetDeviceInfo(device, CL_DEVICE_QUEUE_PROPERTIES, sizeof(queue_properties), &queue_properties, NULL);
		if (queue_properties & CL_QUEUE_OUT_OF_ORDER_EXEC_MODE_ENABLE)
			printf("  CL_DEVICE_QUEUE_PROPERTIES:\t\t%s\n", "CL_QUEUE_OUT_OF_ORDER_EXEC_MODE_ENABLE");
		if (queue_properties & CL_QUEUE_PROFILING_ENABLE)
			printf("  CL_DEVICE_QUEUE_PROPERTIES:\t\t%s\n", "CL_QUEUE_PROFILING_ENABLE");

		// CL_DEVICE_IMAGE_SUPPORT
		cl_bool image_support;
		clGetDeviceInfo(device, CL_DEVICE_IMAGE_SUPPORT, sizeof(image_support), &image_support, NULL);
		printf("  CL_DEVICE_IMAGE_SUPPORT:\t\t%u\n", image_support);

		// CL_DEVICE_MAX_READ_IMAGE_ARGS
		cl_uint max_read_image_args;
		clGetDeviceInfo(device, CL_DEVICE_MAX_READ_IMAGE_ARGS, sizeof(max_read_image_args), &max_read_image_args, NULL);
		printf("  CL_DEVICE_MAX_READ_IMAGE_ARGS:\t%u\n", max_read_image_args);

		// CL_DEVICE_MAX_WRITE_IMAGE_ARGS
		cl_uint max_write_image_args;
		clGetDeviceInfo(device, CL_DEVICE_MAX_WRITE_IMAGE_ARGS, sizeof(max_write_image_args), &max_write_image_args, NULL);
		printf("  CL_DEVICE_MAX_WRITE_IMAGE_ARGS:\t%u\n", max_write_image_args);

		// CL_DEVICE_IMAGE2D_MAX_WIDTH, CL_DEVICE_IMAGE2D_MAX_HEIGHT, CL_DEVICE_IMAGE3D_MAX_WIDTH, CL_DEVICE_IMAGE3D_MAX_HEIGHT, CL_DEVICE_IMAGE3D_MAX_DEPTH
		size_t szMaxDims[5];
		printf("\n  CL_DEVICE_IMAGE <dim>");
		clGetDeviceInfo(device, CL_DEVICE_IMAGE2D_MAX_WIDTH, sizeof(size_t), &szMaxDims[0], NULL);
		printf("\t\t\t2D_MAX_WIDTH\t %u\n", szMaxDims[0]);
		clGetDeviceInfo(device, CL_DEVICE_IMAGE2D_MAX_HEIGHT, sizeof(size_t), &szMaxDims[1], NULL);
		printf("\t\t\t\t\t2D_MAX_HEIGHT\t %u\n", szMaxDims[1]);
		clGetDeviceInfo(device, CL_DEVICE_IMAGE3D_MAX_WIDTH, sizeof(size_t), &szMaxDims[2], NULL);
		printf("\t\t\t\t\t3D_MAX_WIDTH\t %u\n", szMaxDims[2]);
		clGetDeviceInfo(device, CL_DEVICE_IMAGE3D_MAX_HEIGHT, sizeof(size_t), &szMaxDims[3], NULL);
		printf("\t\t\t\t\t3D_MAX_HEIGHT\t %u\n", szMaxDims[3]);
		clGetDeviceInfo(device, CL_DEVICE_IMAGE3D_MAX_DEPTH, sizeof(size_t), &szMaxDims[4], NULL);
		printf("\t\t\t\t\t3D_MAX_DEPTH\t %u\n", szMaxDims[4]);

		// CL_DEVICE_PREFERRED_VECTOR_WIDTH_<type>
		printf("  CL_DEVICE_PREFERRED_VECTOR_WIDTH_<t>\t");
		cl_uint vec_width[6];
		clGetDeviceInfo(device, CL_DEVICE_PREFERRED_VECTOR_WIDTH_CHAR, sizeof(cl_uint), &vec_width[0], NULL);
		clGetDeviceInfo(device, CL_DEVICE_PREFERRED_VECTOR_WIDTH_SHORT, sizeof(cl_uint), &vec_width[1], NULL);
		clGetDeviceInfo(device, CL_DEVICE_PREFERRED_VECTOR_WIDTH_INT, sizeof(cl_uint), &vec_width[2], NULL);
		clGetDeviceInfo(device, CL_DEVICE_PREFERRED_VECTOR_WIDTH_LONG, sizeof(cl_uint), &vec_width[3], NULL);
		clGetDeviceInfo(device, CL_DEVICE_PREFERRED_VECTOR_WIDTH_FLOAT, sizeof(cl_uint), &vec_width[4], NULL);
		clGetDeviceInfo(device, CL_DEVICE_PREFERRED_VECTOR_WIDTH_DOUBLE, sizeof(cl_uint), &vec_width[5], NULL);
		printf("CHAR %u, SHORT %u, INT %u, FLOAT %u, DOUBLE %u\n\n\n",
			vec_width[0], vec_width[1], vec_width[2], vec_width[3], vec_width[4]);
	}

#pragma warning(disable : 4996)
	int OCL()
	{
		printf("started running\n");


		// Create the two input vectors
		int i;
		const int LIST_SIZE = 1024;
		int *A = (int*)malloc(sizeof(int)*LIST_SIZE);
		int *B = (int*)malloc(sizeof(int)*LIST_SIZE);
		for (i = 0; i < LIST_SIZE; i++) {
			A[i] = i;
			B[i] = LIST_SIZE - i;
		}

		// Load the kernel source code into the array source_str
		FILE *fp;
		char *source_str;
		size_t source_size;

		fp = fopen("C:\\Users\\dinf0014_admin\\Desktop\\Amigo\\DLLProject\\DLLProject\\Kernel.cl", "r");
		if (!fp) {
			fprintf(stderr, "Failed to load kernel.\n");
			exit(1);
		}
		source_str = (char*)malloc(MAX_SOURCE_SIZE);
		source_size = fread(source_str, 1, MAX_SOURCE_SIZE, fp);
		fclose(fp);
		//printf("kernel loading done\n");
		// Get platform and device information
		cl_device_id device_id = NULL;
		cl_uint ret_num_devices;
		cl_uint ret_num_platforms;


		cl_int ret = clGetPlatformIDs(0, NULL, &ret_num_platforms);
		cl_platform_id *platforms = NULL;
		platforms = (cl_platform_id*)malloc(ret_num_platforms * sizeof(cl_platform_id));

		ret = clGetPlatformIDs(ret_num_platforms, platforms, NULL);
		//printf("ret at %d is %d\n", __LINE__, ret);

		ret = clGetDeviceIDs(platforms[0], CL_DEVICE_TYPE_ALL, 1,
			&device_id, &ret_num_devices);

		cl_uint retInfo;
		size_t sizeRet;
		//printf("ret at %d is %d\n", __LINE__, ret);
		// Create an OpenCL context
		cl_context context = clCreateContext(NULL, 1, &device_id, NULL, NULL, &ret);
		//printf("ret at %d is %d\n", __LINE__, ret);

		// Create a command queue
		cl_command_queue command_queue = clCreateCommandQueue(context, device_id, 0, &ret);
		//printf("ret at %d is %d\n", __LINE__, ret);

		// Create memory buffers on the device for each vector
		cl_mem a_mem_obj = clCreateBuffer(context, CL_MEM_READ_ONLY,
			LIST_SIZE * sizeof(int), NULL, &ret);
		cl_mem b_mem_obj = clCreateBuffer(context, CL_MEM_READ_ONLY,
			LIST_SIZE * sizeof(int), NULL, &ret);
		cl_mem c_mem_obj = clCreateBuffer(context, CL_MEM_WRITE_ONLY,
			LIST_SIZE * sizeof(int), NULL, &ret);

		// Copy the lists A and B to their respective memory buffers
		ret = clEnqueueWriteBuffer(command_queue, a_mem_obj, CL_TRUE, 0,
			LIST_SIZE * sizeof(int), A, 0, NULL, NULL);
		//printf("ret at %d is %d\n", __LINE__, ret);

		ret = clEnqueueWriteBuffer(command_queue, b_mem_obj, CL_TRUE, 0,
			LIST_SIZE * sizeof(int), B, 0, NULL, NULL);
		//printf("ret at %d is %d\n", __LINE__, ret);

		//printf("before building\n");
		// Create a program from the kernel source
		cl_program program = clCreateProgramWithSource(context, 1,
			(const char **)&source_str, (const size_t *)&source_size, &ret);
		//printf("ret at %d is %d\n", __LINE__, ret);

		// Build the program
		ret = clBuildProgram(program, 1, &device_id, NULL, NULL, NULL);
		//printf("ret at %d is %d\n", __LINE__, ret);

		//printf("after building\n");
		// Create the OpenCL kernel
		cl_kernel kernel = clCreateKernel(program, "vector_add", &ret);
		//printf("ret at %d is %d\n", __LINE__, ret);

		// Set the arguments of the kernel
		ret = clSetKernelArg(kernel, 0, sizeof(cl_mem), (void *)&a_mem_obj);
		//printf("ret at %d is %d\n", __LINE__, ret);

		ret = clSetKernelArg(kernel, 1, sizeof(cl_mem), (void *)&b_mem_obj);
		//printf("ret at %d is %d\n", __LINE__, ret);

		ret = clSetKernelArg(kernel, 2, sizeof(cl_mem), (void *)&c_mem_obj);
		//printf("ret at %d is %d\n", __LINE__, ret);

		//added this to fix garbage output problem
		//ret = clSetKernelArg(kernel, 3, sizeof(int), &LIST_SIZE);

		//printf("before execution\n");
		// Execute the OpenCL kernel on the list
		size_t global_item_size = LIST_SIZE; // Process the entire lists
		size_t local_item_size = 64; // Divide work items into groups of 64
		ret = clEnqueueNDRangeKernel(command_queue, kernel, 1, NULL,
			&global_item_size, &local_item_size, 0, NULL, NULL);
		//printf("after execution\n");
		// Read the memory buffer C on the device to the local variable C
		int *C = (int*)malloc(sizeof(int)*LIST_SIZE);
		ret = clEnqueueReadBuffer(command_queue, c_mem_obj, CL_TRUE, 0,
			LIST_SIZE * sizeof(int), C, 0, NULL, NULL);

		size_t localPrefSize;
		ret = clGetKernelWorkGroupInfo(kernel, device_id, CL_KERNEL_PREFERRED_WORK_GROUP_SIZE_MULTIPLE, sizeof(localPrefSize), &localPrefSize, 0);
		CLPrintDevInfo(device_id);
		printf("preferred work group size for kernel: %d", localPrefSize);
		//printf("after copying\n");
		// Display the result to the screen
		//for (i = 0; i < LIST_SIZE; i++)
		//printf("%d + %d = %d\n", A[i], B[i], C[i]);

		// Clean up
		ret = clFlush(command_queue);
		ret = clFinish(command_queue);
		ret = clReleaseKernel(kernel);
		ret = clReleaseProgram(program);
		ret = clReleaseMemObject(a_mem_obj);
		ret = clReleaseMemObject(b_mem_obj);
		ret = clReleaseMemObject(c_mem_obj);
		ret = clReleaseCommandQueue(command_queue);
		ret = clReleaseContext(context);
		free(A);
		free(B);
		free(C);
		return 0;
	}

	int Test()
	{
		int i, j;
		char* value;
		size_t valueSize;
		cl_uint platformCount;
		cl_platform_id* platforms;
		cl_uint deviceCount;
		cl_device_id* devices;
		cl_uint maxComputeUnits;

		// get all platforms
		clGetPlatformIDs(0, NULL, &platformCount);
		platforms = (cl_platform_id*)malloc(sizeof(cl_platform_id) * platformCount);
		clGetPlatformIDs(platformCount, platforms, NULL);

		for (i = 0; i < platformCount; i++) {

			// get all devices
			clGetDeviceIDs(platforms[i], CL_DEVICE_TYPE_ALL, 0, NULL, &deviceCount);
			devices = (cl_device_id*)malloc(sizeof(cl_device_id) * deviceCount);
			clGetDeviceIDs(platforms[i], CL_DEVICE_TYPE_ALL, deviceCount, devices, NULL);

			// for each device print critical attributes
			for (j = 0; j < deviceCount; j++) {

				// print device name
				clGetDeviceInfo(devices[j], CL_DEVICE_NAME, 0, NULL, &valueSize);
				value = (char*)malloc(valueSize);
				clGetDeviceInfo(devices[j], CL_DEVICE_NAME, valueSize, value, NULL);
				printf("%d. Device: %s\n", j + 1, value);
				free(value);

				// print hardware device version
				clGetDeviceInfo(devices[j], CL_DEVICE_VERSION, 0, NULL, &valueSize);
				value = (char*)malloc(valueSize);
				clGetDeviceInfo(devices[j], CL_DEVICE_VERSION, valueSize, value, NULL);
				printf(" %d.%d Hardware version: %s\n", j + 1, 1, value);
				free(value);

				// print software driver version
				clGetDeviceInfo(devices[j], CL_DRIVER_VERSION, 0, NULL, &valueSize);
				value = (char*)malloc(valueSize);
				clGetDeviceInfo(devices[j], CL_DRIVER_VERSION, valueSize, value, NULL);
				printf(" %d.%d Software version: %s\n", j + 1, 2, value);
				free(value);

				// print c version supported by compiler for device
				clGetDeviceInfo(devices[j], CL_DEVICE_OPENCL_C_VERSION, 0, NULL, &valueSize);
				value = (char*)malloc(valueSize);
				clGetDeviceInfo(devices[j], CL_DEVICE_OPENCL_C_VERSION, valueSize, value, NULL);
				printf(" %d.%d OpenCL C version: %s\n", j + 1, 3, value);
				free(value);

				// print parallel compute units
				clGetDeviceInfo(devices[j], CL_DEVICE_MAX_COMPUTE_UNITS,
					sizeof(maxComputeUnits), &maxComputeUnits, NULL);
				printf(" %d.%d Parallel compute units: %d\n", j + 1, 4, maxComputeUnits);

			}

			free(devices);

		}

		free(platforms);
		return 0;
	}

	int Test2()
	{
		int i, j;
		char* info;
		size_t infoSize;
		cl_uint platformCount;
		cl_platform_id *platforms;
		const char* attributeNames[5] = { "Name", "Vendor",
			"Version", "Profile", "Extensions" };
		const cl_platform_info attributeTypes[5] = { CL_PLATFORM_NAME, CL_PLATFORM_VENDOR,
			CL_PLATFORM_VERSION, CL_PLATFORM_PROFILE, CL_PLATFORM_EXTENSIONS };
		const int attributeCount = sizeof(attributeNames) / sizeof(char*);

		// get platform count
		clGetPlatformIDs(5, NULL, &platformCount);

		// get all platforms
		platforms = (cl_platform_id*)malloc(sizeof(cl_platform_id) * platformCount);
		clGetPlatformIDs(platformCount, platforms, NULL);

		// for each platform print all attributes
		for (i = 0; i < platformCount; i++) {

			printf("\n %d. Platform \n", i + 1);

			for (j = 0; j < attributeCount; j++) {

				// get platform attribute value size
				clGetPlatformInfo(platforms[i], attributeTypes[j], 0, NULL, &infoSize);
				info = (char*)malloc(infoSize);

				// get platform attribute value
				clGetPlatformInfo(platforms[i], attributeTypes[j], infoSize, info, NULL);

				printf("  %d.%d %-11s: %s\n", i + 1, j + 1, attributeNames[j], info);
				free(info);

			}

			printf("\n");

		}

		free(platforms);
		return 0;
	}

#pragma endregion

#pragma region Matrix

	typedef struct {
		int width;
		int height;
		cl_mem elements;
	} MatrixV1;

	// Host code
	// Matrices are stored in row-major order:
	// M(row, col) = *(M.elements + row * M.stride + col)
	typedef struct {
		int width;
		int height;
		int stride;
		cl_mem elements;
	} MatrixV2;


#define BLOCK_SIZE 32
#define BLOCK_SIZE_V3 32
#define MATRIX_DIM 8192

	// Host code
	// Matrices are stored in row-major order:
	// M(row, col) = *(M.elements + row * M.width + col)

	// Thread block size
	// Matrix multiplication - Host code
	// Matrix dimensions are assumed to be multiples of BLOCK_SIZE
	void MatMulHostV1(const MatrixV1 A, const MatrixV1 B, MatrixV1 C,
		const cl_context context,
		const cl_kernel matMulKernel,
		const cl_command_queue queue)
	{
		// Invoke kernel
		cl_uint i = 0;
		clSetKernelArg(matMulKernel, i++,
			sizeof(A.width), (void*)&A.width);
		clSetKernelArg(matMulKernel, i++,
			sizeof(A.height), (void*)&A.height);
		clSetKernelArg(matMulKernel, i++,
			sizeof(A.elements), (void*)&A.elements);
		clSetKernelArg(matMulKernel, i++,
			sizeof(B.width), (void*)&B.width);
		clSetKernelArg(matMulKernel, i++,
			sizeof(B.height), (void*)&B.height);
		clSetKernelArg(matMulKernel, i++,
			sizeof(B.elements), (void*)&B.elements);
		clSetKernelArg(matMulKernel, i++,
			sizeof(C.width), (void*)&C.width);
		clSetKernelArg(matMulKernel, i++,
			sizeof(C.height), (void*)&C.height);
		clSetKernelArg(matMulKernel, i++,
			sizeof(C.elements), (void*)&C.elements);
		size_t localWorkSize[] = { BLOCK_SIZE, BLOCK_SIZE };
		size_t globalWorkSize[] = { A.width, A.width };
		auto ret = clEnqueueNDRangeKernel(queue, matMulKernel, 2, 0,
			globalWorkSize, localWorkSize,
			0, 0, 0);
		auto size = C.width*C.height * sizeof(float);
		/*float *Cret = (float*)malloc(size);
		ret = clEnqueueReadBuffer(queue, C.elements, CL_TRUE, 0,
		size, Cret, 0, NULL, NULL);
		for (size_t i = 0; i < size / sizeof(float); i++)
		{
		float qwe = *(Cret + i);
		if ((int)qwe != A.width)
		{
		int asd = 2;
		}
		}
		free(Cret);*/
		int asd = 2;
	}

	void MatMulV1Call()
	{

		// Load the kernel source code into the array source_str
		FILE *fp;
		char *source_str;
		size_t source_size;

		fp = fopen("C:\\Users\\dinf0014_admin\\Desktop\\Amigo\\DLLProject\\DLLProject\\MatrixMulV1.cl", "r");
		if (!fp) {
			fprintf(stderr, "Failed to load kernel.\n");
			exit(1);
		}
		source_str = (char*)malloc(MAX_SOURCE_SIZE);
		source_size = fread(source_str, 1, MAX_SOURCE_SIZE, fp);
		fclose(fp);
		// Get platform and device information
		cl_device_id device_id = NULL;
		cl_uint ret_num_devices;
		cl_uint ret_num_platforms;


		cl_int ret = clGetPlatformIDs(0, NULL, &ret_num_platforms);
		cl_platform_id *platforms = NULL;
		platforms = (cl_platform_id*)malloc(ret_num_platforms * sizeof(cl_platform_id));

		ret = clGetPlatformIDs(ret_num_platforms, platforms, NULL);

		ret = clGetDeviceIDs(platforms[0], CL_DEVICE_TYPE_ALL, 1,
			&device_id, &ret_num_devices);

		cl_uint retInfo;
		size_t sizeRet;
		// Create an OpenCL context
		cl_context context = clCreateContext(NULL, 1, &device_id, NULL, NULL, &ret);

		// Create a command queue
		cl_command_queue command_queue = clCreateCommandQueue(context, device_id, 0, &ret);

		MatrixV1 A, B, C;
		int dim = MATRIX_DIM;
		A.width = C.width = dim;
		A.height = B.width = dim;
		B.height = C.height = dim;
		size_t LIST_SIZE = dim * dim;
		float *floatArrayA = (float*)malloc(sizeof(float) * LIST_SIZE);

		for (size_t i = 0; i < dim * dim; i++)
		{
			*(floatArrayA + i) = 1.0f;
			//*(floatArrayB + i) = 1.0f;
		}

		// Create memory buffers on the device for each vector
		A.elements = clCreateBuffer(context, CL_MEM_READ_ONLY,
			A.width * A.height * sizeof(float), NULL, &ret);
		// Copy the lists A and B to their respective memory buffers
		ret = clEnqueueWriteBuffer(command_queue, A.elements, CL_TRUE, 0,
			LIST_SIZE * sizeof(float), floatArrayA, 0, NULL, NULL);

		free(floatArrayA);

		float *floatArrayB = (float*)malloc(sizeof(float) * LIST_SIZE);
		for (size_t i = 0; i < dim * dim; i++)
		{
			*(floatArrayB + i) = 1.0f;
		}

		B.elements = clCreateBuffer(context, CL_MEM_READ_ONLY,
			B.width * B.height * sizeof(float), NULL, &ret);

		ret = clEnqueueWriteBuffer(command_queue, B.elements, CL_TRUE, 0,
			LIST_SIZE * sizeof(float), floatArrayB, 0, NULL, NULL);


		free(floatArrayB);
		C.elements = clCreateBuffer(context, CL_MEM_WRITE_ONLY,
			C.width * C.height * sizeof(float), NULL, &ret);


		// Create a program from the kernel source
		cl_program program = clCreateProgramWithSource(context, 1,
			(const char **)&source_str, (const size_t *)&source_size, &ret);

		// Build the program
		ret = clBuildProgram(program, 1, &device_id, NULL, NULL, NULL);

		// Create the OpenCL kernel
		cl_kernel kernel = clCreateKernel(program, "MatMulKernel", &ret);


		MatMulHostV1(A, B, C, context, kernel, command_queue);

		clReleaseMemObject(A.elements);
		clReleaseMemObject(C.elements);
		clReleaseMemObject(B.elements);
	}

	// Matrix multiplication - Host code
	// Matrix dimensions are assumed to be multiples of BLOCK_SIZE
	void MatMulHostV2(const MatrixV2 A, const MatrixV2 B, MatrixV2 C,
		const cl_context context,
		const cl_kernel matMulKernel,
		const cl_command_queue queue)
	{
		cl_int ret;
		// Invoke kernel
		cl_uint i = 0;
		ret = clSetKernelArg(matMulKernel, i++,
			sizeof(A.width), (void*)&A.width);
		ret = clSetKernelArg(matMulKernel, i++,
			sizeof(A.height), (void*)&A.height);
		ret = clSetKernelArg(matMulKernel, i++,
			sizeof(A.stride), (void*)&A.stride);
		ret = clSetKernelArg(matMulKernel, i++,
			sizeof(A.elements), (void*)&A.elements);
		ret = clSetKernelArg(matMulKernel, i++,
			sizeof(B.width), (void*)&B.width);
		ret = clSetKernelArg(matMulKernel, i++,
			sizeof(B.height), (void*)&B.height);
		ret = clSetKernelArg(matMulKernel, i++,
			sizeof(B.stride), (void*)&B.stride);
		ret = clSetKernelArg(matMulKernel, i++,
			sizeof(B.elements), (void*)&B.elements);
		ret = clSetKernelArg(matMulKernel, i++,
			sizeof(C.width), (void*)&C.width);
		ret = clSetKernelArg(matMulKernel, i++,
			sizeof(C.height), (void*)&C.height);
		ret = clSetKernelArg(matMulKernel, i++,
			sizeof(C.stride), (void*)&C.stride);
		ret = clSetKernelArg(matMulKernel, i++,
			sizeof(C.elements), (void*)&C.elements);
		ret = clSetKernelArg(matMulKernel, i++,
			BLOCK_SIZE * BLOCK_SIZE * sizeof(float), NULL);
		ret = clSetKernelArg(matMulKernel, i++,
			BLOCK_SIZE * BLOCK_SIZE * sizeof(float), NULL);
		size_t localWorkSize[] = { BLOCK_SIZE, BLOCK_SIZE };
		size_t globalWorkSize[] = { A.width, A.width };
		ret = clEnqueueNDRangeKernel(queue, matMulKernel, 2, 0,
			globalWorkSize, localWorkSize,
			0, 0, 0);
		auto size = C.width*C.height * sizeof(float);
		float *Cret = (float*)malloc(size);
		ret = clEnqueueReadBuffer(queue, C.elements, CL_TRUE, 0,
			size, Cret, 0, NULL, NULL);
		for (size_t i = 0; i < size / sizeof(float); i++)
		{
			float qwe = *(Cret + i);
			if ((int)qwe != A.width)
			{
				int asd = 2;
			}
		}
		int asd = 2;
		free(Cret);
		//CL_INVALID_PROGRAM_EXECUTABLE
	}

	void MatMulV2Call()
	{

		// Load the kernel source code into the array source_str
		FILE *fp;
		char *source_str;
		size_t source_size;

		fp = fopen("C:\\Users\\dinf0014_admin\\Desktop\\Amigo\\DLLProject\\DLLProject\\MatrixMulV2.cl", "r");
		if (!fp) {
			fprintf(stderr, "Failed to load kernel.\n");
			exit(1);
		}
		source_str = (char*)malloc(MAX_SOURCE_SIZE);
		source_size = fread(source_str, 1, MAX_SOURCE_SIZE, fp);
		fclose(fp);
		// Get platform and device information
		cl_device_id device_id = NULL;
		cl_uint ret_num_devices;
		cl_uint ret_num_platforms;


		cl_int ret = clGetPlatformIDs(0, NULL, &ret_num_platforms);
		cl_platform_id *platforms = NULL;
		platforms = (cl_platform_id*)malloc(ret_num_platforms * sizeof(cl_platform_id));

		ret = clGetPlatformIDs(ret_num_platforms, platforms, NULL);

		ret = clGetDeviceIDs(platforms[0], CL_DEVICE_TYPE_ALL, 1,
			&device_id, &ret_num_devices);

		cl_uint retInfo;
		size_t sizeRet;
		// Create an OpenCL context
		cl_context context = clCreateContext(NULL, 1, &device_id, NULL, NULL, &ret);

		// Create a command queue
		cl_command_queue command_queue = clCreateCommandQueue(context, device_id, 0, &ret);

		MatrixV2 A, B, C;
		int dim = MATRIX_DIM;
		A.width = C.width = A.stride = C.stride = dim;
		A.height = B.width = B.stride = dim;
		B.height = C.height = dim;
		size_t LIST_SIZE = dim * dim;
		float *floatArrayA = (float*)malloc(sizeof(float) * LIST_SIZE);
		float *floatArrayB = (float*)malloc(sizeof(float) * LIST_SIZE);
		for (size_t i = 0; i < dim * dim; i++)
		{
			*(floatArrayA + i) = 1.0f;
			*(floatArrayB + i) = 1.0f;
		}

		// Create memory buffers on the device for each vector
		A.elements = clCreateBuffer(context, CL_MEM_READ_ONLY,
			A.width * A.height * sizeof(float), NULL, &ret);

		B.elements = clCreateBuffer(context, CL_MEM_READ_ONLY,
			B.width * B.height * sizeof(float), NULL, &ret);

		// Copy the lists A and B to their respective memory buffers
		ret = clEnqueueWriteBuffer(command_queue, A.elements, CL_TRUE, 0,
			LIST_SIZE * sizeof(float), floatArrayA, 0, NULL, NULL);

		ret = clEnqueueWriteBuffer(command_queue, B.elements, CL_TRUE, 0,
			LIST_SIZE * sizeof(float), floatArrayB, 0, NULL, NULL);
		free(floatArrayA);
		free(floatArrayB);


		C.elements = clCreateBuffer(context, CL_MEM_WRITE_ONLY,
			C.width * C.height * sizeof(float), NULL, &ret);


		// Create a program from the kernel source
		cl_program program = clCreateProgramWithSource(context, 1,
			(const char **)&source_str, (const size_t *)&source_size, &ret);

		// Build the program
		ret = clBuildProgram(program, 1, &device_id, NULL, NULL, NULL);

		// Create the OpenCL kernel
		cl_kernel kernel = clCreateKernel(program, "MatMulKernel", &ret);

		size_t qweRet;
		size_t qweRetSize;
		ret = clGetKernelWorkGroupInfo(kernel, device_id, CL_KERNEL_PREFERRED_WORK_GROUP_SIZE_MULTIPLE, sizeof(qweRet), &qweRet, &qweRetSize);

		MatMulHostV2(A, B, C, context, kernel, command_queue);

		clReleaseMemObject(A.elements);
		clReleaseMemObject(C.elements);
		clReleaseMemObject(B.elements);
	}

	// Matrix multiplication - Host code
	// Matrix dimensions are assumed to be multiples of BLOCK_SIZE
	void MatMulHostV3(const MatrixV2 A, const MatrixV2 B, MatrixV2 C,
		const cl_context context,
		const cl_kernel matMulKernel,
		const cl_command_queue queue)
	{
		cl_int ret;
		// Invoke kernel
		cl_uint i = 0;
		ret = clSetKernelArg(matMulKernel, i++,
			sizeof(A.width), (void*)&A.width);
		ret = clSetKernelArg(matMulKernel, i++,
			sizeof(A.height), (void*)&A.height);
		ret = clSetKernelArg(matMulKernel, i++,
			sizeof(A.stride), (void*)&A.stride);
		ret = clSetKernelArg(matMulKernel, i++,
			sizeof(A.elements), (void*)&A.elements);
		ret = clSetKernelArg(matMulKernel, i++,
			sizeof(B.width), (void*)&B.width);
		ret = clSetKernelArg(matMulKernel, i++,
			sizeof(B.height), (void*)&B.height);
		ret = clSetKernelArg(matMulKernel, i++,
			sizeof(B.stride), (void*)&B.stride);
		ret = clSetKernelArg(matMulKernel, i++,
			sizeof(B.elements), (void*)&B.elements);
		ret = clSetKernelArg(matMulKernel, i++,
			sizeof(C.width), (void*)&C.width);
		ret = clSetKernelArg(matMulKernel, i++,
			sizeof(C.height), (void*)&C.height);
		ret = clSetKernelArg(matMulKernel, i++,
			sizeof(C.stride), (void*)&C.stride);
		ret = clSetKernelArg(matMulKernel, i++,
			sizeof(C.elements), (void*)&C.elements);
		ret = clSetKernelArg(matMulKernel, i++,
			BLOCK_SIZE_V3 * BLOCK_SIZE_V3 * sizeof(float), NULL);
		ret = clSetKernelArg(matMulKernel, i++,
			BLOCK_SIZE_V3 * BLOCK_SIZE_V3 * sizeof(float), NULL);

		size_t localWorkSize[] = { BLOCK_SIZE_V3, BLOCK_SIZE_V3, 1 };
		size_t globalWorkSize[] = { A.width, A.width,  1 };
		ret = clEnqueueNDRangeKernel(queue, matMulKernel, 2, 0,
			globalWorkSize, localWorkSize,
			0, 0, 0);
		printf("ret: %d\n", ret);
		auto size = C.width*C.height * sizeof(float);
		float *Cret = (float*)malloc(size);
		ret = clEnqueueReadBuffer(queue, C.elements, CL_TRUE, 0,
			size, Cret, 0, NULL, NULL);
		for (size_t i = 0; i < size / sizeof(float); i++)
		{
			float qwe = *(Cret + i);
			//printf("val: %f\n", qwe);
		}
		int asd = 2;
		free(Cret);
		//CL_INVALID_PROGRAM_EXECUTABLE
	}

	void MatMulV3Call()
	{
		// Load the kernel source code into the array source_str
		FILE *fp;
		char *source_str;
		size_t source_size;

		//TODO: change this path
		fp = fopen("C:\\Users\\dinf0014_admin\\Desktop\\Amigo\\DLLProject\\DLLProject\\MatrixMulV3.cl", "r");
		if (!fp) {
			fprintf(stderr, "Failed to load kernel.\n");
			exit(1);
		}
		source_str = (char*)malloc(MAX_SOURCE_SIZE);
		source_size = fread(source_str, 1, MAX_SOURCE_SIZE, fp);
		fclose(fp);
		// Get platform and device information
		cl_device_id device_id = NULL;
		cl_uint ret_num_devices;
		cl_uint ret_num_platforms;


		cl_int ret = clGetPlatformIDs(0, NULL, &ret_num_platforms);
		cl_platform_id *platforms = NULL;
		platforms = (cl_platform_id*)malloc(ret_num_platforms * sizeof(cl_platform_id));

		ret = clGetPlatformIDs(ret_num_platforms, platforms, NULL);

		ret = clGetDeviceIDs(platforms[0], CL_DEVICE_TYPE_ALL, 1,
			&device_id, &ret_num_devices);

		cl_uint retInfo;
		size_t sizeRet;
		// Create an OpenCL context
		cl_context context = clCreateContext(NULL, 1, &device_id, NULL, NULL, &ret);

		// Create a command queue
		cl_command_queue command_queue = clCreateCommandQueue(context, device_id, 0, &ret);

		MatrixV2 A, B, C;
		int dim = MATRIX_DIM;
		A.width = C.width = A.stride = C.stride = dim;
		A.height = B.width = B.stride = dim;
		B.height = C.height = dim;
		size_t LIST_SIZE = dim * dim;
		float *floatArrayA = (float*)malloc(sizeof(float) * LIST_SIZE);
		//float *floatArrayB = (float*)malloc(sizeof(float) * LIST_SIZE);
		for (size_t i = 0; i < dim * dim; i++)
		{
			*(floatArrayA + i) = 1.0f;
			//*(floatArrayB + i) = 1.0f;
		}


		// Create memory buffers on the device for each vector
		A.elements = clCreateBuffer(context, CL_MEM_READ_ONLY,
			A.width * A.height * sizeof(float), NULL, &ret);



		// Copy the lists A and B to their respective memory buffers
		ret = clEnqueueWriteBuffer(command_queue, A.elements, CL_TRUE, 0,
			LIST_SIZE * sizeof(float), floatArrayA, 0, NULL, NULL);
		free(floatArrayA);

		float *floatArrayB = (float*)malloc(sizeof(float) * LIST_SIZE);
		for (size_t i = 0; i < dim * dim; i++)
		{
			//*(floatArrayA + i) = 1.0f;
			*(floatArrayB + i) = 1.0f;
		}
		B.elements = clCreateBuffer(context, CL_MEM_READ_ONLY,
			B.width * B.height * sizeof(float), NULL, &ret);
		ret = clEnqueueWriteBuffer(command_queue, B.elements, CL_TRUE, 0,
			LIST_SIZE * sizeof(float), floatArrayB, 0, NULL, NULL);

		free(floatArrayB);


		C.elements = clCreateBuffer(context, CL_MEM_WRITE_ONLY,
			C.width * C.height * sizeof(float), NULL, &ret);


		// Create a program from the kernel source
		cl_program program = clCreateProgramWithSource(context, 1,
			(const char **)&source_str, (const size_t *)&source_size, &ret);

		// Build the program
		ret = clBuildProgram(program, 1, &device_id, NULL, NULL, NULL);

		// Create the OpenCL kernel
		cl_kernel kernel = clCreateKernel(program, "MatMulKernel", &ret);

		size_t qweRet;
		size_t qweRetSize;
		ret = clGetKernelWorkGroupInfo(kernel, device_id, CL_KERNEL_PREFERRED_WORK_GROUP_SIZE_MULTIPLE, sizeof(qweRet), &qweRet, &qweRetSize);
		CLPrintDevInfo(device_id);
		MatMulHostV3(A, B, C, context, kernel, command_queue);

		clReleaseMemObject(A.elements);
		clReleaseMemObject(C.elements);
		clReleaseMemObject(B.elements);
	}

#pragma endregion

#pragma region Bitmap

	//for imgs of same sizes
	int BitmapAnalyse(cl_kernel kernel, void** samples, int samplesSize, int sampleStride, void** references, int referencesSize, int referenceStride, int imgWidth, int imgHeigth, int** retValues)
	{
#pragma region Initialization

		cl_int ret = 0;
		int retVal = 0;
		if (context == NULL)
		{
			Initialize();
		}

		size_t mem_img_size = referenceStride * imgHeigth * sizeof(char);
		size_t mem_retDiffs_size = referenceStride * imgHeigth * sizeof(int) * referencesSize * samplesSize;

		cl_command_queue command_queue = clCreateCommandQueue(context, device_id, 0, &ret);

#pragma endregion

#pragma region MemoryStuff

		char* referencesData = (char*)malloc(referencesSize * mem_img_size);
		memset(referencesData, 0, referencesSize * mem_img_size);
		char* samplesData = (char*)malloc(samplesSize * mem_img_size);
		memset(samplesData, 0, samplesSize * mem_img_size);
		int *diff = (int*)malloc(mem_retDiffs_size);
		memset(diff, 0, mem_retDiffs_size);

		cl_mem mem_sample = clCreateBuffer(context, CL_MEM_READ_ONLY,
			mem_img_size * samplesSize, NULL, &ret);
		cl_mem mem_refs = clCreateBuffer(context, CL_MEM_READ_ONLY,
			mem_img_size * referencesSize, NULL, &ret);
		cl_mem mem_retDiffs = clCreateBuffer(context, CL_MEM_WRITE_ONLY,
			mem_retDiffs_size, NULL, &ret);

		for (size_t i = 0; i < referencesSize; i++)
		{
			for (size_t j = 0; j < imgHeigth; j++)
			{
				memcpy((referencesData + i * mem_img_size + j * referenceStride), ((char*)(*(((char**)references) + i)) + j * referenceStride), imgWidth * 3);
			}
		}
		for (size_t i = 0; i < samplesSize; i++)
		{
			for (size_t j = 0; j < imgHeigth; j++)
			{
				memcpy((samplesData + i * mem_img_size + j * referenceStride), ((char*)(*(((char**)samples) + i)) + j * sampleStride), imgWidth * 3);
			}
		}

		ret = clEnqueueWriteBuffer(command_queue, mem_sample, CL_TRUE, 0,
			mem_img_size * samplesSize, samplesData, 0, NULL, NULL);
		ret = clEnqueueWriteBuffer(command_queue, mem_refs, CL_TRUE, 0,
			mem_img_size * referencesSize, referencesData, 0, NULL, NULL);

#pragma endregion

#pragma region OpenCLCall

		size_t localPrefSize;
		ret = clGetKernelWorkGroupInfo(kernel, device_id, CL_KERNEL_PREFERRED_WORK_GROUP_SIZE_MULTIPLE, sizeof(localPrefSize), &localPrefSize, 0);
		int stride = sqrt(referencesSize * mem_img_size / sizeof(int));
		stride = (((stride / localPrefSize) + min(1, stride % localPrefSize)) * localPrefSize);
		int argInd = 0;
		ret = clSetKernelArg(kernel, argInd++, sizeof(mem_sample), (void *)&mem_sample);
		ret = clSetKernelArg(kernel, argInd++, sizeof(mem_refs), (void *)&mem_refs);
		ret = clSetKernelArg(kernel, argInd++, sizeof(mem_retDiffs), (void *)&mem_retDiffs);
		ret = clSetKernelArg(kernel, argInd++, sizeof(mem_retDiffs_size), (void *)&mem_retDiffs_size);
		ret = clSetKernelArg(kernel, argInd++, sizeof(samplesSize), (void *)&samplesSize);
		ret = clSetKernelArg(kernel, argInd++, sizeof(stride), (void *)&stride);
		ret = clSetKernelArg(kernel, argInd++, sizeof(mem_img_size), (void *)&mem_img_size);
		size_t localWorkSize[] = { localPrefSize, localPrefSize };
		size_t globalWorkSize[] = { stride, stride };
		ret = clEnqueueNDRangeKernel(command_queue, kernel, 2, 0,
			globalWorkSize, localWorkSize, 0, 0, 0);

#pragma endregion

#pragma region ReturnValue

		ret = clEnqueueReadBuffer(command_queue, mem_retDiffs, CL_TRUE, 0,
			mem_retDiffs_size, diff, 0, NULL, NULL);

		for (size_t i = 0; i < samplesSize; i++)
		{
			for (size_t j = 0; j < referencesSize; j++)
			{
				*(*(retValues + i) + j) = 0;
			}
		}

		for (size_t i = 0; i < mem_retDiffs_size / sizeof(int); i++)
		{
			*(((int*)(*(((int**)retValues) + i / (mem_retDiffs_size / samplesSize / sizeof(int))))) + ((i / mem_img_size) % referencesSize)) += *(diff + i);
		}

#pragma endregion

#pragma region CleanUp

		ret = clFlush(command_queue);
		ret = clFinish(command_queue);
		ret = clReleaseMemObject(mem_sample);
		ret = clReleaseMemObject(mem_refs);
		ret = clReleaseMemObject(mem_retDiffs);
		ret = clReleaseCommandQueue(command_queue);
		free(diff);
		free(referencesData);
		free(samplesData);

#pragma endregion

		return ret;
	}

	int BitmapAnalyseV2(void** samples, int samplesSize, int sampleStride, void** references, int referencesSize, int referenceStride, int imgWidth, int imgHeigth, int** retValues)
	{
		if (context == NULL)
			Initialize();
		return BitmapAnalyse(GetKernelFromFile(KERNEL_BITMAP_ANALYSE_V2), samples, samplesSize, sampleStride, references, referencesSize, referenceStride, imgWidth, imgHeigth, retValues);
	}

	int BitmapAnalyseV3(void** samples, int samplesSize, int sampleStride, void** references, int referencesSize, int referenceStride, int imgWidth, int imgHeigth, int** retValues)
	{
		if (context == NULL)
			Initialize();
		return BitmapAnalyse(GetKernelFromFile(KERNEL_BITMAP_ANALYSE_V3), samples, samplesSize, sampleStride, references, referencesSize, referenceStride, imgWidth, imgHeigth, retValues);
	}

	int SingleThreadBitmapAnalyse(void** samples, int samplesSize, int sampleStride, void** references, int referencesSize, int referenceStride, int imgWidth, int imgHeigth, int** retValues)
	{
		size_t mem_img_size = referenceStride * imgHeigth * sizeof(char);

		int retVal = 0;
		for (size_t l = 0; l < samplesSize; l++)
		{
			for (size_t k = 0; k < referencesSize; k++)
			{
				for (size_t i = 0; i < imgWidth * 3; i++)
				{
					for (size_t j = 0; j < imgHeigth; j++)
					{
						int diff = 0;
						diff = (unsigned char)*(((char*)(*(((char**)references) + k))) + i + j * referenceStride);
						diff -= (unsigned char)*(((char*)(*(((char**)samples) + l))) + i + j * sampleStride);
						diff *= diff;
						retVal += diff;
					}
				}
				//printf("(%d, %d): %d\n", l, k, retVal);
				*(*(retValues + l) + k) = retVal;
				retVal = 0;
			}
		}
		return 0;
	}

	int MultiThreadBitmapAnalyse(void** samples, int samplesSize, int sampleStride, void** references, int referencesSize, int referenceStride, int imgWidth, int imgHeigth, int** retValues)
	{
		const auto nbThread = std::thread::hardware_concurrency();
		std::mutex mtx;

		auto workerThreadV1 = [&](int i)
		{

			const auto nbIterationsPerImg = imgHeigth * imgWidth * 3;
			const auto nbIterationsTotal = nbIterationsPerImg * samplesSize * referencesSize;
			const auto nbIterationsPerThread = nbIterationsTotal / nbThread + 1;
			//this code is multithreaded, but is a hell of a lot slower
			auto globalId0 = i * nbIterationsPerThread;
			auto globalId1 = (i + 1) * nbIterationsPerThread;
			auto retVals = std::map<std::pair<int, int>, int>();

			while (globalId0 < min(nbIterationsTotal, globalId1))
			{
				auto sInd = globalId0 / (nbIterationsTotal / samplesSize);
				auto rInd = (globalId0 / nbIterationsPerImg) % referencesSize;
				auto pixelInd = globalId0 % nbIterationsPerImg;
				auto rowInd = pixelInd / (imgWidth * 3);
				auto colInd = pixelInd % (imgWidth * 3);

				auto diff = *((*((char**)samples) + sInd) + rowInd * sampleStride + colInd);
				diff -= *((*((char**)references) + rInd) + rowInd * referenceStride + colInd);
				diff *= diff;

				auto key = std::pair<int, int>(sInd, rInd);
				if (retVals.find(key) != retVals.end())
				{
					retVals[key] = diff;
				}
				else
				{
					retVals[key] += diff;
				}
				globalId0++;
				if ((globalId0 % (1024)) == 0)
					printf("globalId0: %d\n", globalId0 / 100000);
			}

			mtx.lock();
			for each (const auto &var in retVals)
			{
				*(*(retValues + var.first.first) + var.first.second) += var.second;
			}
			mtx.unlock();

		};

		auto workerThreadV2 = [&](int i)
		{
			//this code is multithreaded, but is a hell of a lot slower
			auto nbComparaison = referencesSize * samplesSize;
			auto retVals = std::map<std::pair<int, int>, int>();
			auto nbComparaisonsPerThread = (nbComparaison / nbThread) + 1;
			auto globalId0 = i * nbComparaisonsPerThread;
			auto globalId1 = (i + 1) * nbComparaisonsPerThread;

			for (size_t i = 0; i < nbComparaisonsPerThread; i++)
			{
				auto ind = globalId0 + i;
				if (ind >= nbComparaison)
					break;
				auto l = ind / referencesSize;
				auto k = ind % referencesSize;
				auto val = 0;
				for (size_t i = 0; i < imgWidth * 3; i++)
				{
					for (size_t j = 0; j < imgHeigth; j++)
					{
						int diff = 0;
						diff = (unsigned char)*(((char*)(*(((char**)references) + k))) + i + j * referenceStride);
						diff -= (unsigned char)*(((char*)(*(((char**)samples) + l))) + i + j * sampleStride);
						diff *= diff;
						val += diff;
					}
				}
				auto key = std::pair<int, int>(l, k);
				retVals[key] = val;
			}

			mtx.lock();
			for each (const auto &var in retVals)
			{
				*(*(retValues + var.first.first) + var.first.second) += var.second;
			}
			mtx.unlock();

		};

		auto workerThreadV3 = [&](int i)
		{
			//this code is multithreaded, but is a hell of a lot slower
			auto nbComparaison = referencesSize * samplesSize;
			auto retVals = std::map<std::pair<int, int>, int>();
			auto nbComparaisonsPerThread = (nbComparaison / nbThread) + 1;
			auto globalId0 = i * nbComparaisonsPerThread;
			auto globalId1 = (i + 1) * nbComparaisonsPerThread;


			for (size_t i = 0; i < nbComparaisonsPerThread; i++)
			{
				auto ind = globalId0 + i;
				if (ind >= nbComparaison)
					break;
				auto l = ind / referencesSize;
				auto k = ind % referencesSize;
				auto rPtr0 = *((char**)references + k);
				auto sPtr0 = *((char**)samples + l);

				auto val = 0;
				auto length = imgHeigth * imgWidth * 3;
				for (size_t i = 0; i < length; i++)
				{
					auto row = i / (imgWidth * 3);
					auto col = i % (imgWidth * 3);
					auto diff = (unsigned)*(sPtr0 + row * sampleStride + col) - (unsigned)*(rPtr0 + row * referenceStride + col);
					diff *= diff;
					val += diff;
				}
				auto key = std::pair<int, int>(l, k);
				retVals[key] = val;
			}

			mtx.lock();
			for each (const auto &var in retVals)
			{
				*(*(retValues + var.first.first) + var.first.second) += var.second;
			}
			mtx.unlock();

		};

		auto threadVector = std::vector<std::thread>();

		for (size_t j = 0; j < samplesSize; j++)
		{
			for (size_t i = 0; i < referencesSize; i++)
			{
				*(*(retValues + j) + i) = 0;
			}
		}

		auto &workerThread = workerThreadV3;

		for (size_t i = 0; i < nbThread; i++)
		{
			threadVector.emplace_back(std::thread(workerThread, i));
		}

		for (size_t i = 0; i < threadVector.size(); i++)
		{
			threadVector.at(i).join();
		}
		threadVector.clear();
		return 0;
	}

#pragma endregion

#pragma endregion

}