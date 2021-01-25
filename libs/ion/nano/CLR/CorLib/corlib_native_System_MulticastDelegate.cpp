//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//
#include "CorLib.h"


HRESULT Library_corlib_native_CanFly_Runtime::Equals___STATIC__BOOLEAN__SystemMulticastDelegate__SystemMulticastDelegate(CLR_RT_StackFrame& stack)
  {
 
  HRESULT hr;

  NANOCLR_SET_AND_LEAVE(DelegateEquals___STATIC__BOOLEAN__SystemDelegate__OBJECT(stack));

  NANOCLR_NOCLEANUP();
  }

HRESULT Library_corlib_native_CanFly_Runtime::NotEquals___STATIC__BOOLEAN__SystemMulticastDelegate__SystemMulticastDelegate(CLR_RT_StackFrame& stack)
  {
 
  HRESULT hr;

  NANOCLR_SET_AND_LEAVE(DelegateEquals___STATIC__BOOLEAN__SystemDelegate__OBJECT(stack));

  stack.NegateResult();

  NANOCLR_NOCLEANUP();
  }
