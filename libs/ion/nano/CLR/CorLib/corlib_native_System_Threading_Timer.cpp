//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//
#include "CorLib.h"


static bool CheckDisposed(CLR_RT_StackFrame &stack)
  {
 

  return CLR_RT_HeapBlock_Timer::CheckDisposed(stack);
  }

static HRESULT SetValues(CLR_RT_StackFrame &stack, uint32_t flags)
  {
 
  HRESULT hr;

  NANOCLR_SET_AND_LEAVE(CLR_RT_HeapBlock_Timer::ConfigureObject(stack, flags));

  NANOCLR_NOCLEANUP();
  }

HRESULT Library_corlib_native_CanFly_Runtime::TimerDispose___STATIC__VOID__SystemThreadingTimer(CLR_RT_StackFrame& stack)
  {
 
  HRESULT hr;

  (void)SetValues(stack, CLR_RT_HeapBlock_Timer::c_ACTION_Destroy);

  NANOCLR_NOCLEANUP_NOLABEL();
  }

HRESULT Library_corlib_native_CanFly_Runtime::TimerCtor___STATIC__VOID__SystemThreadingTimer__SystemThreadingTimerCallback__OBJECT__I4__I4(CLR_RT_StackFrame& stack)
  {
 
  HRESULT hr;

  NANOCLR_SET_AND_LEAVE(SetValues(stack, CLR_RT_HeapBlock_Timer::c_ACTION_Create | CLR_RT_HeapBlock_Timer::c_INPUT_Int32));

  NANOCLR_NOCLEANUP();
  }

/*
HRESULT Library_corlib_native_CanFly_Runtime::_ctor___VOID__SystemThreadingTimerCallback__OBJECT__SystemTimeSpan__SystemTimeSpan(CLR_RT_StackFrame& stack)
  {
 
  HRESULT hr;

  NANOCLR_SET_AND_LEAVE(SetValues(stack, CLR_RT_HeapBlock_Timer::c_ACTION_Create | CLR_RT_HeapBlock_Timer::c_INPUT_TimeSpan));

  NANOCLR_NOCLEANUP();
  }
  */

HRESULT Library_corlib_native_CanFly_Runtime::TimerChange___STATIC__BOOLEAN__SystemThreadingTimer__I4__I4(CLR_RT_StackFrame& stack)
  {
 
  HRESULT hr;

  if (CheckDisposed(stack))
    {
    NANOCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);
    }

  stack.SetResult_Boolean(true);

  NANOCLR_SET_AND_LEAVE(SetValues(stack, CLR_RT_HeapBlock_Timer::c_ACTION_Change | CLR_RT_HeapBlock_Timer::c_INPUT_Int32));

  NANOCLR_NOCLEANUP();
  }

/*
HRESULT Library_corlib_native_CanFly_Runtime::Change___BOOLEAN__SystemTimeSpan__SystemTimeSpan(CLR_RT_StackFrame& stack)
  {
 
  HRESULT hr;

  if (CheckDisposed(stack))
    {
    NANOCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);
    }

  stack.SetResult_Boolean(true);

  NANOCLR_SET_AND_LEAVE(SetValues(stack, CLR_RT_HeapBlock_Timer::c_ACTION_Change | CLR_RT_HeapBlock_Timer::c_INPUT_TimeSpan));

  NANOCLR_NOCLEANUP();
  }
  */

//--//
