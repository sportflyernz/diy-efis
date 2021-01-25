//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//
#include "Core.h"

////////////////////////////////////////////////////////////////////////////////////////////////////

void CLR_RT_GetVersion(unsigned short int *pMajor, unsigned short int *pMinor, unsigned short int *pBuild, unsigned short int *pRevision)
  {
 
  if (pMajor) *pMajor = VERSION_MAJOR;
  if (pMinor) *pMinor = VERSION_MINOR;
  if (pBuild) *pBuild = VERSION_BUILD;
  if (pRevision) *pRevision = VERSION_REVISION;
  }

////////////////////////////////////////////////////////////////////////////////////////////////////

HRESULT CLR_RT_ArrayListHelper::PrepareArrayList(CLR_RT_HeapBlock &thisRef, int count, int capacity)
  {
 
  HRESULT hr;

  CLR_RT_HeapBlock *pThis = thisRef.Dereference(); FAULT_ON_NULL(pThis);

  if (count > capacity || capacity < 1)
    {
    NANOCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);
    }

  NANOCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance(pThis[FIELD___items], capacity, g_CLR_RT_WellKnownTypes.m_Object));

  pThis[FIELD___size].NumericByRef().s4 = count;

  NANOCLR_NOCLEANUP();
  }

HRESULT CLR_RT_ArrayListHelper::ExtractArrayFromArrayList(CLR_RT_HeapBlock &thisRef, CLR_RT_HeapBlock_Array *&array, int &count, int &capacity)
  {
 
  HRESULT hr;

  CLR_RT_HeapBlock *pThis;
  CLR_RT_HeapBlock *items;

  pThis = thisRef.Dereference(); FAULT_ON_NULL(pThis);
  items = &pThis[FIELD___items];

  if (items->DataType() != DATATYPE_OBJECT)
    {
    NANOCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }

  array = items->DereferenceArray(); FAULT_ON_NULL(array);

  if (array->DataType() != DATATYPE_SZARRAY)
    {
    NANOCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }

  capacity = array->m_numOfElements;
  count = pThis[FIELD___size].NumericByRef().s4;

  NANOCLR_NOCLEANUP();
  }

//--//

HRESULT CLR_RT_ByteArrayReader::Init(const unsigned char *src, unsigned int srcSize)
  {
 
  HRESULT hr;

  if (src == NULL || srcSize == 0) NANOCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);

  source = src;
  sourceSize = srcSize;

  NANOCLR_NOCLEANUP();
  }

HRESULT CLR_RT_ByteArrayReader::Read(void *dst, unsigned int size)
  {
 
  HRESULT hr;

  if (size > sourceSize) NANOCLR_SET_AND_LEAVE(CLR_E_FAIL);

  memcpy(dst, source, size);

  source += size;
  sourceSize -= size;

  NANOCLR_NOCLEANUP();
  }

HRESULT CLR_RT_ByteArrayReader::Read1Byte(void *dst)
  {
 
  HRESULT hr;

  if (1 > sourceSize) NANOCLR_SET_AND_LEAVE(CLR_E_FAIL);

  *(unsigned char *)dst = *source;

  source++;
  sourceSize--;

  NANOCLR_NOCLEANUP();
  }

HRESULT CLR_RT_ByteArrayReader::Skip(unsigned int size)
  {
 
  HRESULT hr;

  if (size > sourceSize) NANOCLR_SET_AND_LEAVE(CLR_E_FAIL);

  source += size;
  sourceSize -= size;

  NANOCLR_NOCLEANUP();
  }

int64_t *GetValuePtr(CLR_RT_HeapBlock &ref)
  {
 
  CLR_RT_HeapBlock *obj = &ref;
  CLR_DataType dt = obj->DataType();

  if (dt == DATATYPE_OBJECT || dt == DATATYPE_BYREF)
    {
    obj = obj->Dereference();
    if (!obj)
      return NULL;

    dt = obj->DataType();
    }

  if (dt == DATATYPE_TIMESPAN)
    {
    return (int64_t *)&obj->NumericByRef().s8;
    }

  if (dt == DATATYPE_I8)
    {
    return (int64_t *)&obj->NumericByRef().s8;
    }

  if (dt == DATATYPE_VALUETYPE && obj->ObjectCls().m_data == g_CLR_RT_WellKnownTypes.m_TimeSpan.m_data)
    {
    return (int64_t *)&obj[Library_corlib_native_System_TimeSpan::FIELD___ticks].NumericByRef().s8;
    }

  return NULL;
  }