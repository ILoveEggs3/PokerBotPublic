// Copyright (c) 2012 DotNetAnywhere
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

#pragma once

#include "MetaData.h"
#include "Types.h"

function_space_specifier tAsyncCall* System_Array_Internal_GetValue(PTR pThis_, PTR pParams, PTR pReturnValue);
function_space_specifier tAsyncCall* System_Array_Internal_SetValue(PTR pThis_, PTR pParams, PTR pReturnValue);
function_space_specifier tAsyncCall* System_Array_Clear(PTR pThis_, PTR pParams, PTR pReturnValue);
function_space_specifier tAsyncCall* System_Array_Internal_Copy(PTR pThis_, PTR pParams, PTR pReturnValue);
function_space_specifier tAsyncCall* System_Array_Resize(PTR pThis_, PTR pParams, PTR pReturnValue);
function_space_specifier tAsyncCall* System_Array_Reverse(PTR pThis_, PTR pParams, PTR pReturnValue);

function_space_specifier HEAP_PTR SystemArray_NewVector(tMD_TypeDef *pArrayTypeDef, U32 rank, U32* lengths);
#define SystemArray_GetLength(pArray) (SystemArray_Length((void*)pArray))
function_space_specifier U32 SystemArray_Length(void * p);
function_space_specifier U32 SystemArray_LengthDim(void * p, int dim);
function_space_specifier U32 SystemArray_Rank(void * p);
function_space_specifier void SystemArray_StoreElement(HEAP_PTR pThis_, U32 index, PTR value);
function_space_specifier void SystemArray_LoadElement(HEAP_PTR pThis_, U32 index, PTR value);
function_space_specifier PTR SystemArray_GetElements(HEAP_PTR pArray);
function_space_specifier void SystemArray_LoadElementIndices(HEAP_PTR pThis_, U64* indices, U64* value);
function_space_specifier void SystemArray_LoadElementIndicesAddress(HEAP_PTR pThis_, U64* indices, HEAP_PTR * value_address);
function_space_specifier void SystemArray_StoreElementIndices(HEAP_PTR pThis_, U64* indices, U64* value);
function_space_specifier PTR SystemArray_LoadElementAddress(HEAP_PTR pThis_, U32 index);
function_space_specifier U32 SystemArray_GetNumBytes(HEAP_PTR pThis_, tMD_TypeDef *pElementType);
function_space_specifier int SystemArray_GetRank(HEAP_PTR pThis_);
function_space_specifier void SystemArray_SetRank(HEAP_PTR pThis_, int rank);
function_space_specifier U64* SystemArray_GetDims(HEAP_PTR pThis_);
