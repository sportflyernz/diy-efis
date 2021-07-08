/*
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
namespace CanFly.Proton
{
  public class Step
  {
    private short _value;
    private Pen _pen;
    private Color _indicatorColor;
    private Pen _indicatorPen;

    public Step(short value, Pen pen, Color indicatorColor, Pen indicatorPen)
    {
      _value = value;
      _pen = pen;
      _indicatorColor = indicatorColor;
      _indicatorPen = indicatorPen;
    }

    public short Value { get { return _value; } }
    public Pen Pen { get { return _pen; } }
    public Color IndicatorColor { get { return _indicatorColor; } }
    public Pen IndicatorPen { get { return _indicatorPen; } }
  }

}