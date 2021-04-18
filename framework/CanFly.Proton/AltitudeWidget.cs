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
using System;

namespace CanFly.Proton
{
  public sealed class AltitudeWidget : Widget
  {
    private struct VsiMarkers
    {
      public string text;
      public int pos;

      public VsiMarkers(string _text, int _pos)
      {
        text = _text;
        pos = _pos;
      }
    };

    private short altitude;
    private short vertical_speed;
    private float scale;
    private float offset;

    private ushort qnh;
    private uint background_color;
    private uint text_color;
    private Pen pen;
    private Font font;      // 9 pixel
    private Font small_roller;  // 12 pixel
    private Font large_roller;  // 15 pixel


    internal AltitudeWidget(Widget parent, Rect bounds, ushort id, ushort key)
      : base(parent, bounds, id)
    {
      if (!TryRegGetFloat(key, "scale", out scale))
        scale = 1.0f;

      if (!TryRegGetFloat(key, "offset", out offset))
        offset = 0.0f;

      if (!LookupFont(key, "font", out font))
      {
        // we always have the neo font.
        if (!OpenFont("neo", 9, out font))
          throw new ApplicationException();
      }

      if (!LookupFont(key, "large-font", out large_roller))
      {
        // we always have the neo font.
        if (!OpenFont("neo", 12, out large_roller))
          throw new ApplicationException();
      }

      if (!LookupFont(key, "small-font", out small_roller))
      {
        // we always have the neo font.
        if (!OpenFont("neo", 9, out small_roller))
          throw new ApplicationException();
      }

      if (!LookupColor(key, "back-color", out background_color))
        background_color = Colors.Black;

      if (!LookupColor(key, "text-color", out text_color))
        text_color = Colors.White;

      if (!LookupPen(key, "pen", out pen))
        pen = Pens.WhitePen;

      qnh = 1013;

      AddEventListener(CanFlyID.id_baro_corrected_altitude, OnBaroCorrectedAltitude);
      AddEventListener(CanFlyID.id_altitude_rate, on_altitude_rate);
      AddEventListener(CanFlyID.id_qnh, on_qnh);

      InvalidateRect();
    }

    protected override void OnPaint(CanFlyMsg e)
    {
      BeginPaint();

      Rect wnd_rect = WindowRect;
      // reset clipping
      ClipRect = wnd_rect;

      Extent ex = wnd_rect.Extent;
      Extent size;

      int median_y = ex.Dy >> 1;

      Rectangle(Pens.Hollow, background_color, Rect.Create(8, 8, ex.Dx - 8, ex.Dy - 8));

      int i;

      VsiMarkers[] marks = {
        new VsiMarkers("3", median_y-88),
        new VsiMarkers( "2", median_y-68 ),
        new VsiMarkers( "1", median_y-48 ),
        new VsiMarkers( "1", median_y+47 ),
        new VsiMarkers( "2", median_y+67 ),
        new VsiMarkers( "3", median_y+87 )
        };

      for (i = 0; i < 6; i++)
      {
        size = TextExtent(font, marks[i].text);
        DrawText(font, Colors.Yellow, background_color, marks[i].text,
                 Point.Create(ex.Dx - 9 - size.Dx, marks[i].pos - (size.Dy >> 1)),
                 wnd_rect, TextOutStyle.Clipped);
      }

      Rect paint_area = Rect.Create(8, 8, ex.Dx - 8, ex.Dy - 8);

      // the vertical tape displays 250 ft = 20 pixels
      int num_pixels = paint_area.Height >> 1;
      float num_grads = num_pixels / 20.0f;
      num_grads *= 250;           // altitude offset

      num_grads += 8;

      int top_altitude = altitude + (int)num_grads;

      // Math.Round the height to 10 pixels
      top_altitude = (top_altitude - ((top_altitude / 10) * 10)) > 5
          ? ((top_altitude / 10) + 1) * 10
          : ((top_altitude / 10) * 10);

      // assign the first line altitude
      int line_altitude = (top_altitude / 250) * 250;
      // work out how many lines to the next lowest marker
      int marker_line;
      for (marker_line = ((top_altitude - line_altitude) / 10) + 10;
          marker_line < (ex.Dy - 8); marker_line += 20)
      {
        // draw a line from 10 pixels to 20 pixels then the text
        Point[] pts = { Point.Create(10, marker_line), Point.Create(20, marker_line) };

        Polyline(paint_area, pen, pts);

        if (line_altitude == ((line_altitude / 500) * 500))
        {
          string altStr = line_altitude.ToString();
          size = TextExtent(font, altStr);
          DrawText(font, text_color, background_color, altStr, Point.Create(23, marker_line - (size.Dy >> 1)), paint_area, TextOutStyle.Clipped);
        }

        line_altitude -= 250;

        if (line_altitude < 0)
          break;
      }

      median_y = ex.Dy >> 1;

      Point[] roller =
        {
    Point.Create( 23,  median_y ),
    Point.Create( 35,  median_y+12 ),
    Point.Create( 35,  median_y+20 ),
    Point.Create( ex.Dx-8,   median_y+20 ),
    Point.Create( ex.Dx-8,   median_y-20 ),
    Point.Create( 35,  median_y-20 ),
    Point.Create( 35,  median_y-12 ),
    Point.Create( 23,  median_y )
    };

      Polygon(Pens.WhitePen, Colors.Black, roller);

      Rect text_rect = Rect.Create(36, median_y - 19, 88, median_y + 19);
      DisplayRoller(Rect.Create(36, median_y - 19, ex.Dx - 8, median_y + 19),
                     altitude, 2, Colors.Black, text_color, large_roller, small_roller);

      /////////////////////////////////////////////////////////////////////////////
      //
      //	Now display the vertical speed.
      //
      int vs;
      // draw the marker.  There is a non-linear scale
      if (vertical_speed < 1000 && vertical_speed > -1000)
        // +/- 48 pixels
        vs = median_y - ((int)((double)(vertical_speed) * (48.0 / 1000.0)));
      else
      {
        vs = Math.Min(3000, Math.Max(-3000, vertical_speed));

        // make absolute
        vs = Math.Abs(vs);

        vs -= 1000;       // makes pixels from 1000 ft mark
        vs /= 50;         // 40 pixels = 2000 ft
        vs += 48;         // add the 1000 ft marks

        if (vertical_speed < 0)
          vs *= -1;

        vs = median_y - vs;        // add the base marker
      }

      int vs_base = median_y;
      if (vs == median_y)
      {
        vs_base--;
        vs++;
      }

      if (vs_base < vs)
      {
        int tmp = vs;
        vs = vs_base;
        vs_base = tmp;
      }

      Rectangle(Pens.Hollow, Colors.White, Rect.Create(ex.Dx - 8, vs, ex.Dx - 1, vs_base));

      // draw the text at the top of the VSI
      Rect vsi_rect = Rect.Create(23, 0, ex.Dx - 8, 18);

      Rectangle(Pens.WhitePen, Colors.Black, vsi_rect);

      vsi_rect = Rect.Create(vsi_rect.Left + 1, vsi_rect.Top + 1, vsi_rect.Right - 1, vsi_rect.Bottom - 1);

      // Math.Round the vs to 10 feet
      vs = vertical_speed;
      if (Math.Abs((vs / 5) % 5) == 1)
        vs = ((vs / 10) * 10) + (vs < 0 ? -10 : 10);
      else
        vs = (vs / 10) * 10;

      string vsStr = vs.ToString();

      size = TextExtent(font, vsStr);

      DrawText(vsi_rect, font, Colors.Green, Colors.Black, vsStr,
               Point.Create(vsi_rect.Left + (vsi_rect.Width >> 1) - (size.Dx >> 1),
                   vsi_rect.Top + 1), vsi_rect, TextOutStyle.Clipped);

      // draw the current QNH
      vsi_rect = Rect.Create(23, ex.Dy - 19, ex.Dx - 8, ex.Dy - 1);

      Rectangle(Pens.WhitePen, Colors.Black, vsi_rect);

      vsi_rect = Rect.Create(vsi_rect.Left + 1, vsi_rect.Top + 1, vsi_rect.Right - 1, vsi_rect.Bottom - 1);

      string qnhStr = qnh.ToString();
      size = TextExtent(font, qnhStr);

      DrawText(vsi_rect, font, Colors.Green, Colors.Black, qnhStr,
               Point.Create(vsi_rect.Left + (vsi_rect.Width >> 1) - (size.Dx >> 1),
                   vsi_rect.Top + 1), vsi_rect, TextOutStyle.Clipped);

      EndPaint();
    }

    private void OnBaroCorrectedAltitude(CanFlyMsg msg)
    {
      try
      {
        bool changed = false;

        float v = msg.GetFloat();

        v *= scale;
        v += offset;

        short value = (short)Math.Round(v);

        changed = altitude != value;
        altitude = value;

        if (changed)
          InvalidateRect();
      }
      catch
      {

      }
    }

    private void on_altitude_rate(CanFlyMsg msg)
    {
      try
      {
        bool changed = false;

        float v = msg.GetFloat();

        v *= scale / 60;
        v += offset;

        short value = (short)Math.Round(v);
        changed = vertical_speed != value;
        vertical_speed = value;

        if (changed)
          InvalidateRect();
      }
      catch
      {

      }
    }

    private void on_qnh(CanFlyMsg msg)
    {
      try
      {
        bool changed = false;

        ushort value = msg.GetUInt16();

        changed = qnh != value;
        qnh = value;

        if (changed)
          InvalidateRect(); ;

      }
      catch
      {

      }
    }
  }
}
