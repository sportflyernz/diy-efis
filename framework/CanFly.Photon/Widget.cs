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
using System.Collections;

namespace CanFly
{
  /// <summary>
  /// A Widget is a class that can handle messages from the internal
  /// can message bus, as well as having child windows.
  /// </summary>
  public abstract class Widget : GdiObject
  {
    // list of events this widget is registered to receive
    private static ArrayList eventInfoTable = null;
    private class EventInfo
    {
      // the canfly msg ID of this event
      private ushort eventID;
      private CanFlyEventHandler handler;
      
      public EventInfo(Widget widget, ushort eventId)
      {
        this.eventID = eventId;
        Syscall.AddWidgetEvent(widget.InternalHandle, eventID);
      }

      public ushort EventID {  get { return eventID; } }
      public void OnMessage(CanFlyMsg msg)
      {
        if(handler != null)
          handler.Invoke(msg);
      }

      public event CanFlyEventHandler Handler
      {
        add { handler += value; }
        remove { handler -= value; }

      }
    }

    public event CanFlyEventHandler BeforePaint = null;
    public event CanFlyEventHandler AfterPaint = null;

    static Widget()
    {
      eventInfoTable = new ArrayList();
    }

    /// <summary>
    /// Private constructor for the screen
    /// </summary>
    /// <param name="hwnd"></param>
    internal Widget(uint hwnd) : base(hwnd)
    {
      Syscall.SetWindowData(Handle, OnMessage);

      ClipRect = WindowRect;
    }

    /// <summary>
    /// Create a child window of a parent
    /// </summary>
    /// <param name="parent">Parent of the window</param>
    /// <param name="bounds">Bounds relative to the parent</param>
    /// <param name="id">Window ID</param>
    protected Widget(Widget parent, Rect bounds, ushort id)
      : base(Syscall.CreateChildWindow(parent.Handle, bounds, id))
    {
      Syscall.SetWindowData(Handle, OnMessage);
      // default clipping rectangle
      ClipRect = WindowRect;

      // hook the paint message
      AddEventListener(PhotonID.id_paint, OnPaintMsg);
    }

    private void OnPaintMsg(CanFlyMsg e)
    {
      try
      {
        if (BeforePaint != null)
          BeforePaint(e);

        OnPaint(e);

        if (AfterPaint != null)
          AfterPaint(e);
      }
      catch
      {
      }
    }

    protected abstract void OnPaint(CanFlyMsg e);

    protected override void OnDispose()
    {
      Syscall.CloseWindow(Handle);
    }

    /// <summary>
    /// Add an event handler to the current widget message queue
    /// </summary>
    /// <param name="message_id">Id of message to handle</param>
    /// <param name="eventListener">Callback</param>
    public void AddEventListener(ushort message_id, CanFlyEventHandler eventListener)
    {
      EventInfo eventInfo = FindEvent(message_id);
      if (eventInfo == null)
      {
        eventInfo = new EventInfo(this, message_id);
        eventInfoTable.Add(eventInfo);
      }

      eventInfo.Handler += eventListener;
    }
    /// <summary>
    /// Remove an event handler
    /// </summary>
    /// <param name="message_id"></param>
    /// <param name="eventListener"></param>
    public void RemoveEventListener(ushort message_id, CanFlyEventHandler eventListener)
    {
      EventInfo eventInfo = FindEvent(message_id);
      if (eventInfo != null)
        eventInfo.Handler -= eventListener;
    }
 
    private EventInfo FindEvent(ushort eventID)
    {
      foreach (EventInfo theHandler in eventInfoTable)
      {
        if (eventID == theHandler.EventID)
          return theHandler;
      }

      return null;
    }

    private void OnMessage(CanFlyMsg msg)
    {
      // find the handler for the message (if any)
      EventInfo handler = FindEvent(msg.CanID);
      if (handler != null)
      {
        try
        {
          handler.OnMessage(msg);
        }
        catch
        {
        }
      }
    }
    /// <summary>
    /// 
    /// </summary>
    public Rect WindowRect
    {
      get { return Syscall.GetWindowRect(Handle); }
    }
    /// <summary>
    /// 
    /// </summary>
    public Rect WindowPos
    {
      get { return Syscall.GetWindowPos(Handle); }
      set { Syscall.SetWindowPos(Handle, value); }
    }

    public void SendMessage(CanFlyMsg msg)
    {
      Syscall.SendMessage(InternalHandle, msg);
    }

    public void PostMessage(CanFlyMsg msg, uint maxWait)
    {
      Syscall.PostMessage(InternalHandle, maxWait, msg);
    }

    public void PostMessage(CanFlyMsg msg)
    {
      Syscall.PostMessage(InternalHandle, 0, msg);
    }

    private Widget GetWidget(uint handle)
    {
      if (handle == 0)
        return null;

      CanFlyEventHandler wndData = Syscall.GetWindowData(handle);

      return (Widget)((Delegate)wndData).Target;
    }

    /// <summary>
    /// 
    /// </summary>
    public Widget Parent
    {
      get { return GetWidget(Syscall.GetParent(Handle)); }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// 
    /// <returns></returns>
    public Widget GetWidgetById(ushort id)
    {
      return GetWidget(Syscall.GetWindowById(Handle, id));
    }
    /// <summary>
    /// 
    /// </summary>
    public Widget FirstChild
    {
      get { return GetWidget(Syscall.GetFirstChild(Handle)); }
    }
    /// <summary>
    /// 
    /// </summary>
    public Widget NextSibling
    {
      get { return GetWidget(Syscall.GetNextSibling(Handle)); }
    }
    /// <summary>
    /// 
    /// </summary>
    public Widget PreviousSibling
    {
      get { return GetWidget(Syscall.GetPreviousSibling(Handle)); }
    }
    /// <summary>
    /// Insert the window before the sibling in the chain of windows
    /// </summary>
    /// <param name="widget">sibling to insert before</param>
    public void InsertBefore(Widget widget)
    {
      if(widget != null)
        Syscall.InsertBefore(Handle, widget.Handle);
    }
    /// <summary>
    /// Insert the window after the sibling in the chain of windows
    /// </summary>
    /// <param name="widget">sibling to insert after</param>
    public void InsertAfter(Widget widget)
    {
      if(widget != null)
        Syscall.InsertAfter(Handle, widget.Handle);
    }
    /// <summary>
    /// the z-order assigned to a window, default = 0
    /// </summary>
    public byte ZOrder
    {
      get { return Syscall.GetZOrder(Handle); }
      set { Syscall.SetZOrder(Handle, value); }
    }

    /// <summary>
    /// Invalidate the area of the canvas.
    /// </summary>
    /// <param name="rect">hint as to rectangle invalidated</param>
    public void InvalidateRect(Rect rect)
    {
      Syscall.InvalidateRect(Handle, rect);
    }
  
    /// <summary>
    /// Invalidate the area of the canvas.
    /// </summary>
    public void InvalidateRect()
    {
       Syscall.InvalidateRect(Handle, WindowRect);
    }
    /// <summary>
    /// Return true if the window is invalid
    /// </summary>
    public bool IsInvalid
    {
      get { return Syscall.IsInvalid(Handle); }
    }
    /// <summary>
    /// Notify the GDI a write operation is beginning
    /// </summary>
    protected void BeginPaint()
    {
      Syscall.BeginPaint(Handle);
    }
    /// <summary>
    /// Notify the canvas that the update operation is complete and
    /// clear the invalid flag
    /// </summary>
    protected void EndPaint()
    {
      Syscall.EndPaint(Handle);
    }

    public void DisplayRoller(Rect bounds, int value, int digits,
      Color bg_color, Color fg_color, Font  large_font, Font  small_font)
    {
      // we need to work out the size of the roller digits first

      Extent size_medium = TextExtent(small_font, "00");

      int top = bounds.Top;
      top += (bounds.Bottom - bounds.Top) >> 1;
      top -= (short)(size_medium.Dy >> 1);
      // calc the interval / pixel ratio
      top += (short)((value % 10) * (size_medium.Dy / 10.0));

      if (digits == 1)
        value *= 10;

      int minor = (value / 10) * 10;

      int large_value = minor / 100;
      minor %= 100;

      while (top > bounds.Top)
      {
        top -= size_medium.Dy;
        minor += 10;
      }

      string str;

      int left = bounds.Right - (digits == 1 ? size_medium.Dx >> 1 : size_medium.Dx);

      while (top <= bounds.Bottom)
      {
        // draw the text + digits first
        minor %= 100;
        if (minor < 0)
          minor += 100;

        if (minor >= 0)
        {
          if (digits == 1)
            str = ((int)(minor / 10)).ToString("d1");
          else
            str = ((int)minor).ToString("d2");

          DrawText(small_font, fg_color, bg_color, str, Point.Create(left, top), bounds, TextOutStyle.Clipped);
        }

        minor -= 10;
        top += size_medium.Dy;
      }

      // now the larger value
      str = large_value.ToString();

      // calc the size
      //cv.font(&arial_15_font);
      Extent large_size = TextExtent(large_font, str);
      
      left -= large_size.Dx;

      top = bounds.Top;
      top += (bounds.Bottom - bounds.Top) >> 1;
      top -= large_size.Dy >> 1;

      DrawText(large_font, fg_color, bg_color, str, Point.Create(left, top), bounds, TextOutStyle.Clipped);

    }
    /// <summary>
    /// Lookup a font definition based on settings in a config hive
    /// </summary>
    /// <param name="key">Configuration hive to look for font settings</param>
    /// <param name="name">Name of the Font definition registry key</param>
    /// <param name="font">Handle to the font</param>
    /// <returns>true if the font was defined</returns>
    public bool LookupFont(ushort key, string name, out Font font)
    {
      try
      {
        ushort fontKey =CanFly.Syscall.RegOpenKey(key, name);

        font = Font.Open(
          CanFly.Syscall.RegGetString(fontKey, "name"),
          CanFly.Syscall.RegGetUint16(fontKey, "size"));

        return true;
      }
      catch
      {
        return false;
      }
    }

    public bool LookupColor(ushort key, string name, out Color color)
    {
      color = Colors.Black;
      try
      {
        string color_str = CanFly.Syscall.RegGetString(key, name);

        if(color_str[0] == '0')
        {
          // parse number
          if (color_str[1] == 'x')
            color = Color.Create(Convert.ToUInt32(color_str.Substring(2), 16));
          else
            color = Color.Create(Convert.ToUInt32(color_str));
        }
        else
        {
          switch(color_str.ToLower())
          {
            case "white":
              color = Colors.White;
              break;
            case "black":
              color = Colors.Black;
              break;
            case "gray":
              color = Colors.Gray;
              break;
            case "lightgray":
              color = Colors.LightGray;
              break;
            case "darkgray":
              color = Colors.DarkGray;
              break;
            case "red":
              color = Colors.Red;
              break;
            case "pink":
              color = Colors.Pink;
              break;
            case "blue":
              color = Colors.Blue;
              break;
            case "green":
              color = Colors.Green;
              break;
            case "lightgreen":
              color = Colors.LightGreen;
              break;
            case "yellow":
              color = Colors.Yellow;
              break;
            case "magenta":
              color = Colors.Magenta;
              break;
            case "cyan":
              color = Colors.Cyan;
              break;
            case "paleyellow":
              color = Colors.PaleYellow;
              break;
            case "lightyellow":
              color = Colors.LightYellow;
              break;
            case "limegreen":
              color = Colors.White;
              break;
            case "teal":
              color = Colors.Teal;
              break;
            case "darkgreen":
              color = Colors.DarkGreen;
              break;
            case "maroon":
              color = Colors.Maroon;
              break;
            case "purple":
              color = Colors.Purple;
              break;
            case "orange":
              color = Colors.Orange;
              break;
            case "khaki":
              color = Colors.Khaki;
              break;
            case "olive":
              color = Colors.Olive;
              break;
            case "brown":
              color = Colors.Brown;
              break;
            case "navy":
              color = Colors.Navy;
              break;
            case "lightblue":
              color = Colors.LightBlue;
              break;
            case "fadedblue":
              color = Colors.FadedBlue;
              break;
            case "lightgrey":
              color = Colors.LightGrey;
              break;
            case "darkgrey":
              color = Colors.DarkGray;
              break;
            case "hollow":
              color = Colors.Hollow;
              break;

          }
        }
      }
      catch
      {
        return false;
      }

      return true;
    }

    public bool LookupPen(ushort key, string name, out Pen pen)
    {
      try
      {
        ushort penKey = CanFly.Syscall.RegOpenKey(key, name);

        ushort width = CanFly.Syscall.RegGetUint16(penKey, "width");
        string style = CanFly.Syscall.RegGetString(penKey, "style");
        Color color;
        LookupColor(penKey, "color", out color);


        PenStyle theStyle = PenStyle.Solid;

        switch (style)
        {
          case "solid":
            theStyle = PenStyle.Solid;
            break;
          case "dash":
            theStyle = PenStyle.Dash;
            break;
          case "dot":
            theStyle = PenStyle.Dot;
            break;
          case "dash_dot":
            theStyle = PenStyle.DashDot;
            break;
          case "dash_dot_dot":
            theStyle = PenStyle.DashDotDot;
            break;
          case "null":
            theStyle = PenStyle.Null;
            break;
        }


        pen = Pen.Create(color, width, theStyle);
      }
      catch
      {
        return false;
      }

      return true;
    }

    public bool OpenFont(string name, ushort size, out Font font)
    {
      try
      {
        font = Font.Open(name, size);
      }
      catch
      {
        return false;
      }

      return true;
    }

    public bool TryRegGetString(ushort key, string name, out string value)
    {
      value = null;
      try
      {
        value = Syscall.RegGetString(key, name);
      }
      catch
      {
        return false;
      }

      return true;
    }

    public bool TryOpenFont(string name, ushort pixels, out Font font)
    {
      try
      {
        font = Font.Open(name, pixels);
      }
      catch
      {
        return false;
      }

      return true;
    }

    public bool TryRegGetBool(ushort key, string name, out bool value)
    {
      value = false;
      try
      {
        value = Syscall.RegGetBool(key, name);
      }
      catch
      {
        return false;
      }

      return true;
    }

    public bool TryRegGetFloat(ushort key, string name, out float value)
    {
      value = 0;
      try
      {
        value = Syscall.RegGetFloat(key, name);
      }
      catch
      {
        return false;
      }

      return true;
    }

    public bool TryRegGetUint8(ushort key, string name, out byte value)
    {
      value = 0;
      try
      {
        value = Syscall.RegGetUint8(key, name);
      }
      catch
      {
        return false;
      }

      return true;
    }
    
    public bool TryRegGetUint16(ushort key, string name, out ushort value)
    {
      value = 0;
      try
      {
        value = Syscall.RegGetUint16(key, name);
      }
      catch
      {
        return false;
      }

      return true;
    }
    
    public bool TryRegGetUint32(ushort key, string name, out uint value)
    {
      value = 0;
      try
      {
        value = Syscall.RegGetUint32(key, name);
      }
      catch
      {
        return false;
      }

      return true;
    }

    public bool TryRegGetInt8(ushort key, string name, out sbyte value)
    {
      value = 0;
      try
      {
        value = Syscall.RegGetInt8(key, name);
      }
      catch
      {
        return false;
      }

      return true;
    }
    
    public bool TryRegGetInt16(ushort key, string name, out short value)
    {
      value = 0;
      try
      {
        value = Syscall.RegGetInt16(key, name);
      }
      catch
      {
        return false;
      }

      return true;
    }
   
    public bool TryRegGetUInt16(ushort key, string name, out ushort value)
    {
      value = 0;
      try
      {
        value = Syscall.RegGetUint16(key, name);
      }
      catch
      {
        return false;
      }

      return true;
    }
    
    public bool TryRegGetInt32(ushort key, string name, out int value)
    {
      value = 0;
      try
      {
        value = Syscall.RegGetInt32(key, name);
      }
      catch
      {
        return false;
      }

      return true;
    }

    public bool TryRegGetRect(ushort key, out Rect rect)
    {
      short x;
      if (!TryRegGetInt16(key, "x", out x))
        x = 0;

      short y;
      if (!TryRegGetInt16(key, "y", out y))
        y = 0;

      short dx;
      if (!TryRegGetInt16(key, "width", out dx))
        dx = 0;

      short dy;
      if (!TryRegGetInt16(key, "height", out dy))
        dy = 0;

      rect = Rect.Create(x, y, x + dx, y + dy);

      return true;
    }

    public bool TryRegOpenKey(ushort key, string name, out ushort child)
    {
      child = 0;
      try
      {
        child = Syscall.RegOpenKey(key, name);
      }
      catch
      {
        return false;
      }

      return true;
    }

    /// <summary>
    /// Fast integer rotate of a point
    /// </summary>
    /// <param name="center">Point around which to rotate</param>
    /// <param name="pt">The point to rotate</param>
    /// <param name="degrees">Degrees to rotate</param>
    /// <returns>the rotated point</returns>
    public Point RotatePoint(Point center, Point pt, int degrees)
    {
      return Syscall.RotatePoint(center, pt, (short) degrees);
    }

    public double RadiansToDegrees(double radians)
    {
      return radians * 0.3183098; // n * (1/ Math.PI);
    }

    public double DegressToRadians(double degrees)
    {
      return degrees * 0.0174533; //(Math.PI / 180);
    }

    public double MetersToNM(double meters)
    {
      return meters * 0.000539957; // meters / 1852
    }

    public double MetersPerSecondToKnots(double value)
    {
      return value * 1.94384;
    }
  }
}
