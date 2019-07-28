
#include "../Native/_BCL_.h"
#include "../Native/MetaData.h"
#include "../Native/MetaData_Search.h"
#include "../Native/System.Array.h"
#include "../Native/System.String.h"
#include "../Native/Type.h"
#include "../Native/Types.h"
#include "../Native/basics.h"
#include "../Native/Heap.h"
#include "../Native/CLIFile.h"
#include "../Native/Generics.h"
#include "../Native/Types.h"
#include "../Native/System.RuntimeType.h"
#include "../Native/Type.h"
#include "../Native/basics.h"

#ifdef __cplusplus
extern "C" {
#endif

EXPORT void InitTheBcl(void * g, size_t size, size_t first_overhead, int count)
{
	if (_bcl_ && _bcl_->options & BCL_DEBUG_FUNCTION_ENTRY)
		Gprintf("InitTheBcl\n");

	InternalInitTheBcl(g, size, first_overhead, count);
}

EXPORT void BclCheckHeap()
{
	if (_bcl_ && _bcl_->options & BCL_DEBUG_FUNCTION_ENTRY)
		Gprintf("CheckHeap\n");

	InternalCheckHeap();
}

EXPORT void BclSetOptions(U64 options)
{
	if (_bcl_ && _bcl_->options & BCL_DEBUG_FUNCTION_ENTRY)
		Gprintf("SetOptions\n");

	InternalSetOptions(options);
}

EXPORT void BclInitFileSystem()
{
	InternalInitFileSystem();
}

EXPORT void BclAddFile(void * name, void * file, size_t length, void * result)
{
	if (_bcl_ && _bcl_->options & BCL_DEBUG_FUNCTION_ENTRY)
		Gprintf("GfsAddFile\n");

	//printf("Adding File to GFS %s 0x%08llx %x\n",
	//	name, file, length);
	InternalGfsAddFile(name, file, length, result);
}

EXPORT void BclContinueInit()
{
	InternalInitializeBCL1();
	InternalInitializeBCL2();
}

EXPORT void* BclHeapAlloc(void* type_def)
{
	if (_bcl_ && _bcl_->options & BCL_DEBUG_FUNCTION_ENTRY)
		Gprintf("BclHeapAlloc\n");
	void * result = (void*)Heap_AllocType((tMD_TypeDef *)type_def);
	return result;
}

EXPORT int BclSizeOf(void * bcl_type)
{
	if (_bcl_ && _bcl_->options & BCL_DEBUG_FUNCTION_ENTRY)
		Gprintf("BclSizeOf\n");
	tMD_TypeDef * pType = (tMD_TypeDef *)bcl_type;
	return pType->instanceMemSize;
}

EXPORT void* BclConstructArrayType(void* element_type_def, int rank)
{
	if (_bcl_ && _bcl_->options & BCL_DEBUG_FUNCTION_ENTRY)
		Gprintf("BclArrayAlloc\n");
	tMD_TypeDef* array_type_def = Type_GetArrayTypeDef((tMD_TypeDef*)element_type_def, rank, NULL, NULL);
	return (void*)array_type_def;
}

EXPORT void* BclArrayAlloc(void* element_type_def, int rank, unsigned int* lengths)
{
	if (_bcl_ && _bcl_->options & BCL_DEBUG_FUNCTION_ENTRY)
		Gprintf("BclArrayAlloc\n");
	tMD_TypeDef* array_type_def = Type_GetArrayTypeDef((tMD_TypeDef*)element_type_def, rank, NULL, NULL);
	return (void*)SystemArray_NewVector(array_type_def, rank, lengths);
}

EXPORT int BclArrayLength(void* obj)
{
	if (_bcl_ && _bcl_->options & BCL_DEBUG_FUNCTION_ENTRY)
		Gprintf("BclArrayAlloc\n");
	return SystemArray_Length(obj);
}

EXPORT int BclArrayLengthDim(void* obj, int dim)
{
	if (_bcl_ && _bcl_->options & BCL_DEBUG_FUNCTION_ENTRY)
		Gprintf("BclArrayAlloc\n");
	return SystemArray_LengthDim(obj, dim);
}

EXPORT int BclArrayRank(void* obj)
{
	if (_bcl_ && _bcl_->options & BCL_DEBUG_FUNCTION_ENTRY)
		Gprintf("BclArrayAlloc\n");
	return SystemArray_Rank(obj);
}

EXPORT void* BclGetMetaOfType(char* assemblyName, char* nameSpace, char* name, void* nested)
{
	if (_bcl_ && _bcl_->options & BCL_DEBUG_FUNCTION_ENTRY)
		Gprintf("BclGetMetaOfType\n");
	tMD_TypeDef* result = MetaData_GetTypeDefFromFullNameAndNestedType(assemblyName, nameSpace, name, (tMD_TypeDef*)nested);
	MetaData_Fill_TypeDef(result, NULL, NULL);
	return (void*)result;
}

EXPORT void* BclConstructGenericInstanceType(void * c, U32 numTypeArgs, void * a)
{
	tMD_TypeDef * pCoreType = (tMD_TypeDef *)c;
	tMD_TypeDef ** ppTypeArgs = (tMD_TypeDef **)a;
	tMD_TypeDef * result = Generics_GetGenericTypeFromCoreType(pCoreType, numTypeArgs, ppTypeArgs);
	return (void*)result;
}

EXPORT void BclGcCollect()
{
	if (_bcl_ && _bcl_->options & BCL_DEBUG_FUNCTION_ENTRY)
		Gprintf("BclGcCollect\n");
	Heap_GarbageCollect();
}

EXPORT void * STDCALL BclGetMeta(char * file_name)
{
	if (_bcl_ && _bcl_->options & BCL_DEBUG_FUNCTION_ENTRY)
		Gprintf("BclGetMeta\n");
	tCLIFile* result = CLIFile_Load(file_name);
	return (void*)result;
}

EXPORT void STDCALL BclPrintMeta(void* meta)
{
	if (_bcl_ && _bcl_->options & BCL_DEBUG_FUNCTION_ENTRY)
		Gprintf("BclPrintMeta\n");
	if (meta == 0) return;
	tCLIFile* clifile = (tCLIFile*)meta;
	Gprintf("%s\n", clifile->pFileName);
	Gprintf("%s\n", clifile->pVersion);
	Gprintf("\n");
	MetaData_PrintMetaData(clifile->pMetaData);
}

EXPORT void * STDCALL BclAllocString(int len, void * chars)
{
	if (_bcl_ && _bcl_->options & BCL_DEBUG_FUNCTION_ENTRY)
		Gprintf("BclAllocString\n");
	return Internal_SystemString_FromCharPtrUTF16(len, (U16*)chars);
}

EXPORT void * BclHeapGetType(void * heapEntry)
{
	tMD_TypeDef* type = Heap_GetType((HEAP_PTR)heapEntry);
	return (void*)type;
}

EXPORT void * BclFindFieldInType(void * bcl_type, char * name)
{
	return (void *)MetaData_FindFieldInType((tMD_TypeDef *)bcl_type, name);
}

EXPORT void * BclFindFieldInTypeAll(void * bcl_type, char * name)
{
	return (void *)MetaData_FindFieldInTypeAll((tMD_TypeDef *)bcl_type, name);
}

EXPORT void * BclGetField(void * bcl_object, void * bcl_field)
{
	return (void *)MetaData_GetField((HEAP_PTR)bcl_object, (tMD_FieldDef *)bcl_field);
}

EXPORT int BclGetFieldSize(void * bcl_field)
{
	return MetaData_GetFieldSize((tMD_FieldDef *)bcl_field);
}

EXPORT int BclGetFieldOffset(void * bcl_field)
{
	return MetaData_GetFieldOffset((tMD_FieldDef *)bcl_field);
}

EXPORT void * BclGetStaticField(void * bcl_field)
{
	return (void *)MetaData_GetStaticField((tMD_FieldDef *)bcl_field);
}

EXPORT void BclGetFields(void * bcl_type, void * out_buf, void * out_len)
{
	MetaData_GetFields((tMD_TypeDef*)bcl_type, (tMD_FieldDef ***)out_buf, (int*)out_len);
}

EXPORT char * BclGetFieldName(void * bcl_field)
{
	char * name = MetaData_GetFieldName((tMD_FieldDef*)bcl_field);
	return name;
}

EXPORT void * BclGetFieldType(void * bcl_field)
{
	tMD_TypeDef* bcl_type = MetaData_GetFieldType((tMD_FieldDef*)bcl_field);
	return (void *)bcl_type;
}

EXPORT int BclSystemArrayGetRank(void * bcl_object)
{
	return SystemArray_GetRank((HEAP_PTR)bcl_object);
}

EXPORT void BclSystemArraySetRank(void * bcl_object, int rank)
{
	SystemArray_SetRank((HEAP_PTR)bcl_object, rank);
}

EXPORT void * BclSystemArrayGetDims(void * bcl_object)
{
	return SystemArray_GetDims((HEAP_PTR) bcl_object);
}

EXPORT void BclSystemArrayLoadElementIndices(void * bcl_object, void * indices, void * value)
{
	SystemArray_LoadElementIndices((HEAP_PTR)bcl_object, (U64*)indices, (U64*)value);
}

EXPORT void BclSystemArrayLoadElementIndicesAddress(void * bcl_object, void * indices, void * value_address)
{
	SystemArray_LoadElementIndicesAddress((HEAP_PTR)bcl_object, (U64*)indices, (HEAP_PTR*)value_address);
}

EXPORT void * BclMetaDataGetMethodJit(void * bcl_object, int table_id)
{
	return MetaData_GetMethodJit(bcl_object, table_id);
}

EXPORT void BclMetaDataSetMethodJit(void * method_ptr, void * bcl_type, int table_id)
{
	MetaData_SetMethodJit(method_ptr, bcl_type, table_id);
}

#ifdef __cplusplus
}
#endif
