﻿/*
diy-efis
Copyright (C) 2021 Kotuku Aerospace Limited

Permission is hereby granted, free of charge, to any person obtaining
a copy of this software and associated documentation files (the "Software"),
to deal in the Software without restriction, including without limitation
the rights to use, copy, modify, merge, publish, distribute, sublicense,
and/or sell copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included
in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.

If a file does not contain a copyright header, either because it is incomplete
or a binary file then the above copyright notice will apply.

Portions of this repository may have further copyright notices that may be
identified in the respective files.  In those cases the above copyright notice is
subservient to that copyright notice.

Portions of this repository contain code fragments from the following
providers.

If any file has a copyright notice or portions of code have been used
and the original copyright notice is not yet transcribed to the repository
then the original copyright notice is to be respected.

If any material is included in the repository that is not open source
it must be removed as soon as possible after the code fragment is identified.
*/
using System;

namespace CanFly
{
  public enum PenStyle
  {
    Solid,
    Dash,
    Dot,
    DashDot,
    DashDotDot,
    Null
  };

  /// <summary>
  /// A pen is a value type, however the system will share definitions
  /// of the pen
  /// </summary>
  public struct Pen
  {
    /// <summary>
    /// Construct a new pen
    /// </summary>
    /// <param name="color">Color of the pen</param>
    /// <param name="width">Width of the line drawn</param>
    /// <param name="style">Style for the pen</param>
    public static Pen Create(Color color, ushort width, PenStyle style)
    {
      return Syscall.CreatePen(color, width, style);
    }
    /// <summary>
    /// Width of the pen
    /// </summary>
    /// <value>Pen width in pixels</value>
    public ushort Width
    {
      get { return Syscall.GetPenWidth(this); }
    }
    /// <summary>
    /// Color of the pen
    /// </summary>
    /// <value>RGBA color</value>
    public Color Color
    {
      get { return Syscall.GetPenColor(this); }
    }
    /// <summary>
    /// Style of the pen
    /// </summary>
    /// <value>Style</value>
    public PenStyle Style
    {
      get { return Syscall.GetPenStyle(this); }
    }
  };
}
