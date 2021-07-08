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
using CanFly;
using CanFly.Proton;

namespace Kotuku.Efi
{
  class EFIWidget : Widget
  {
    private MenuWidget _menu;
    private EFIDialGauge _map;
    private EFIDialGauge _rpm;
    private TempWidget _temps;
    private SliderGauge _oilp;
    private SliderGauge _oilt;
    private SliderGauge _fuelp;
    private SliderGauge _fuelf;
    private SliderGauge _leftf;
    private SliderGauge _rightf;
    private AnnunciatorWidget _annunciators;
    private MarqueeWidget _marquee;

    private OverlayWidget _overlay;

    public EFIWidget(Widget parent, ushort orientation, ushort id)
      : base(parent, parent.WindowRect, id)
    {
      _menu = new MenuWidget(parent, orientation, (ushort)(id + 1));
      _menu.ZOrder = 0x80;    // make sure on top layer

      Font fontNeo9;
      OpenFont("neo", 9, out fontNeo9);
      Font fontNeo15;
      OpenFont("neo", 15, out fontNeo15);

 
      TempWidget.CHTBar cht1 = new TempWidget.CHTBar(CanFlyID.id_cylinder_head_temperature1, Colors.LightGreen,
                        CanFlyID.id_cylinder_head_temperature1_alarm, Colors.Red,
                        CanFlyID.id_cylinder_head_temperature1_status, 0, "CHT1");
      
      TempWidget.EGTBar egt1 = new TempWidget.EGTBar(CanFlyID.id_exhaust_gas_temperature1, Colors.LightBlue,
                        CanFlyID.id_exhaust_gas_temperature1_alarm, Colors.Red,
                        CanFlyID.id_exhaust_gas_temperature1_status, 0, "EGT1");

      TempWidget.CHTBar cht2 = new TempWidget.CHTBar(CanFlyID.id_cylinder_head_temperature2, Colors.LightGreen,
                        CanFlyID.id_cylinder_head_temperature2_alarm, Colors.Red,
                        CanFlyID.id_cylinder_head_temperature2_status, 0, "CHT2");

      TempWidget.EGTBar egt2 = new TempWidget.EGTBar(CanFlyID.id_exhaust_gas_temperature2, Colors.LightBlue,
                        CanFlyID.id_exhaust_gas_temperature2_alarm, Colors.Red,
                        CanFlyID.id_exhaust_gas_temperature2_status, 0, "EGT2");

     TempWidget.CHTBar cht3 = new TempWidget.CHTBar(CanFlyID.id_cylinder_head_temperature3, Colors.LightGreen,
                        CanFlyID.id_cylinder_head_temperature3_alarm, Colors.Red,
                        CanFlyID.id_cylinder_head_temperature3_status, 0, "CHT3");

      TempWidget.EGTBar egt3 = new TempWidget.EGTBar(CanFlyID.id_exhaust_gas_temperature3, Colors.LightBlue,
                        CanFlyID.id_exhaust_gas_temperature3_alarm, Colors.Red,
                        CanFlyID.id_exhaust_gas_temperature3_status, 0, "EGT3");

     TempWidget.CHTBar cht4 = new TempWidget.CHTBar(CanFlyID.id_cylinder_head_temperature4, Colors.LightGreen,
                        CanFlyID.id_cylinder_head_temperature4_alarm, Colors.Red,
                        CanFlyID.id_cylinder_head_temperature4_status, 0, "CHT4");

      TempWidget.EGTBar egt4 = new TempWidget.EGTBar(CanFlyID.id_exhaust_gas_temperature4, Colors.LightBlue,
                        CanFlyID.id_exhaust_gas_temperature4_alarm, Colors.Red,
                        CanFlyID.id_exhaust_gas_temperature4_status, 0, "EGT4");

      _map = AddMAPWiget(Rect.Create(0, 0, 110, 80), fontNeo9, fontNeo15, 100, 0x40);
      _rpm = AddRPMWiget(Rect.Create(110, 0, 220, 80), fontNeo9, fontNeo15, 101, 0x40);
      _temps = new TempWidget(this, Rect.Create(0, 80, 220, 200), 102,
        cht1, egt1, cht2, egt2, cht3, egt3, cht4, egt4);

      _oilp = new SliderGauge(this, Rect.Create(220, 0, 320, 40), 103, CanFlyID.id_oil_pressure, 0.145038f, 0.0f);
      _oilp.DrawBorder = false;
      _oilp.ValueFont = fontNeo9;
      _oilp.ValueBoxVisible = false;
      _oilp.ValueRect = Rect.Create(3, 20, 20, 39);
      _oilp.DrawValue = true;
      _oilp.Name = "OIL-P";
      _oilp.NameFont = fontNeo9;
      _oilp.NamePt = Point.Create(50, 30);
      _oilp.NameColor = Colors.White;
      _oilp.DrawName = true;
      _oilp.AddStep(new Step(0, Pens.LightRedPen, Colors.LightRed, Pens.LightRedPen));
      _oilp.AddStep(new Step(20, Pens.LightRedPen, Colors.LightRed, Pens.LightRedPen));
      _oilp.AddStep(new Step(100, Pens.LightGreenPen, Colors.LightGreen, Pens.LightGreenPen));
      _oilp.AddTick(new TickMark(20, "20", 8));
      _oilp.ZOrder = 10;
      _oilp.Initialize();

      _oilt = new SliderGauge(this, Rect.Create(220, 40, 320, 80), 104, CanFlyID.id_oil_temperature, 1.0f, -273.2f);
      _oilt.DrawBorder = false;
      _oilt.ValueFont = fontNeo9;
      _oilt.ValueBoxVisible = false;
      _oilt.ValueRect = Rect.Create(3, 20, 20, 39);
      _oilt.DrawValue = true;
      _oilt.Name = "OIL-T";
      _oilt.NameFont = fontNeo9;
      _oilt.NamePt = Point.Create(50, 30);
      _oilt.NameColor = Colors.White;
      _oilt.DrawName = true;
      _oilt.AddStep(new Step(0, Pens.LightRedPen, Colors.LightRed, Pens.LightRedPen));
      _oilt.AddStep(new Step(32, Pens.LightRedPen, Colors.LightRed, Pens.LightRedPen));
      _oilt.AddStep(new Step(110, Pens.LightGreenPen, Colors.LightGreen, Pens.LightGreenPen));
      _oilt.AddStep(new Step(120, Pens.LightRedPen, Colors.LightRed, Pens.LightRedPen));
      _oilt.AddTick(new TickMark(32, "32", 8));
      _oilt.AddTick(new TickMark(110, "110", 8));
      _oilt.ZOrder = 10;
      _oilt.Initialize();

      _fuelp = new SliderGauge(this, Rect.Create(220, 80, 320, 120), 105, CanFlyID.id_fuel_pressure, 0.145038f, 0.0f);
      _fuelp.DrawBorder = false;
      _fuelp.ValueFont = fontNeo9;
      _fuelp.ValueBoxVisible = false;
      _fuelp.ValueRect = Rect.Create(3, 20, 20, 39);
      _fuelp.DrawValue = true;
      _fuelp.Name = "FUEL-P";
      _fuelp.NameFont = fontNeo9;
      _fuelp.NamePt = Point.Create(50, 30);
      _fuelp.NameColor = Colors.White;
      _fuelp.DrawName = true;
      _fuelp.AddStep(new Step(0, Pens.LightGreenPen, Colors.LightRed, Pens.LightGreenPen));
      _fuelp.AddStep(new Step(32, Pens.LightGreenPen, Colors.LightRed, Pens.LightGreenPen));
      _fuelp.ZOrder = 10;
      _fuelp.Initialize();

      // fuel flow is l/h * 100
      _fuelf = new SliderGauge(this, Rect.Create(220, 120, 320, 160), 106, CanFlyID.id_fuel_flow_rate, 0.01f, 0);
      _fuelf.DrawBorder = false;
      _fuelf.ValueFont = fontNeo9;
      _fuelf.ValueBoxVisible = false;
      _fuelf.ValueRect = Rect.Create(3, 20, 20, 39);
      _fuelf.DrawValue = true;
      _fuelf.Name = "LPH";
      _fuelf.NameFont = fontNeo9;
      _fuelf.NamePt = Point.Create(50, 30);
      _fuelf.NameColor = Colors.White;
      _fuelf.DrawName = true;
      _fuelf.AddStep(new Step(0, Pens.LightGreenPen, Colors.LightRed, Pens.LightGreenPen));
      _fuelf.AddStep(new Step(45, Pens.LightGreenPen, Colors.LightRed, Pens.LightGreenPen));
      _fuelf.ZOrder = 10;
      _fuelf.Initialize();

      _leftf = new SliderGauge(this, Rect.Create(220, 160, 320, 200), 107, CanFlyID.id_left_fuel_quantity, 1.0f, 0);
      _leftf.DrawBorder = false;
      _leftf.DrawValue = false;
      _leftf.Name = "L-TANK";
      _leftf.NameFont = fontNeo9;
      _leftf.NamePt = Point.Create(50, 30);
      _leftf.NameColor = Colors.White;
      _leftf.DrawName = true;
      _leftf.AddStep(new Step(0, Pens.LightGreenPen, Colors.LightRed, Pens.LightGreenPen));
      _leftf.AddStep(new Step(80, Pens.LightGreenPen, Colors.LightRed, Pens.LightGreenPen));
      _leftf.ZOrder = 10;
      _leftf.Initialize();

      _rightf = new SliderGauge(this, Rect.Create(220, 200, 320, 240), 107, CanFlyID.id_right_fuel_quantity, 1.0f, 0);
      _rightf.DrawBorder = false;
      _rightf.DrawValue = false;
      _rightf.Name = "L-TANK";
      _rightf.NameFont = fontNeo9;
      _rightf.NamePt = Point.Create(50, 30);
      _rightf.NameColor = Colors.White;
      _rightf.DrawName = true;
      _rightf.AddStep(new Step(0, Pens.LightGreenPen, Colors.LightRed, Pens.LightGreenPen));
      _rightf.AddStep(new Step(80, Pens.LightGreenPen, Colors.LightRed, Pens.LightGreenPen));
      _rightf.ZOrder = 10;
      _rightf.Initialize();

      _annunciators = new AnnunciatorWidget(this, Rect.Create(140, 200, 220, 240), 108, 0);
      _annunciators.ValueFont = fontNeo9;
      _annunciators.ValueColor = Colors.White;
      _annunciators.NameFont = fontNeo9;
      _annunciators.NameColor = Colors.Magenta;
      _annunciators.BackgroundColor = Colors.Black;

      _marquee = new MarqueeWidget(this, Rect.Create(0, 220, 140, 240), 109, 0);

      _overlay = new OverlayWidget(this, WindowRect, 110);
      _overlay.ZOrder = 100;


      // TODO: read from config
      // TODO: read from settings
      _temps.ChtRedline = 505;      // 232 celcius
      _temps.EgtRedline = 1173;     // 900 celcius
      _temps.ChtMin = 363;      // 90 celcius
      _temps.EgtMin = 526;      // 538 celcius
    }

    private EFIDialGauge CreateDialGauge(Rect pos, Font textFont, Font valueFont, ushort id,
      ushort leftCanId, ushort rightCanId, byte zOrder, string name, bool showStatus)
    {
      EFIDialGauge widget = new EFIDialGauge(this, pos, id, leftCanId, rightCanId, showStatus);
      widget.DrawStyle = DialWidget.Style.Point;
      widget.ZOrder = zOrder;
      widget.BackgroundColor = Colors.Black;
      widget.BorderPen = Pens.GrayPen;
      widget.DrawBorder = false;
      widget.Font = textFont;

      widget.NameFont = textFont;
      widget.Name = name;
      widget.NamePt = Point.Create(55, 8);
      widget.DrawName = true;
      widget.NameColor = Colors.White;

      // arc drawn below center
      widget.Center = Point.Create(55, 128);
      widget.ArcBegin = 245;
      widget.ArcRange = 50;
      widget.GaugeRadii = 105;

      widget.DrawValue = true;
      widget.ValueBoxVisible = false;
      widget.ValueRect = Rect.Create(10, 48, 100, 80);
      widget.ValueAlign = GaugeWidget.ValueOutStyle.CenterAlign;

      return widget;
    }

    private EFIDialGauge AddMAPWiget(Rect pos, Font textFont, Font valueFont, ushort id, byte zOrder)
    {
      // create the gauge
      EFIDialGauge result = CreateDialGauge(pos, textFont, valueFont, id, 
      CanFlyID.id_left_manifold_pressure, CanFlyID.id_right_manifold_pressure,
       zOrder, "MAP", false);

      // convert bar to in/hg
      result.Scale = 0.0295299830714F;

      result.AddStep(new Step(5, Pens.LightGreenPen5, Colors.White, Pens.Hollow));
      result.AddStep(new Step(31, Pens.LightGreenPen5, Colors.White, Pens.Hollow));
      result.AddStep(new Step(35, Pens.LightRedPen5, Colors.Red, Pens.Hollow));

      result.AddTick(new TickMark(10, "10", 15));
      result.AddTick(new TickMark(15, null, 7));
      result.AddTick(new TickMark(20, "20", 15));
      result.AddTick(new TickMark(25, null, 7));
      result.AddTick(new TickMark(30, "30", 15));

      result.DrawValue = true;
      result.ValueFont = valueFont;

      result.Initialize();

      return result;
    }

    private EFIDialGauge AddRPMWiget(Rect pos, Font textFont, Font valueFont, ushort id, byte zOrder)
    {
      // create the gauge
      EFIDialGauge result = CreateDialGauge(pos, textFont, valueFont, id, 
      CanFlyID.id_left_engine_rpm, CanFlyID.id_right_engine_rpm,
      zOrder, "RPM", true);

      // convert bar to in/hg
      result.Scale = 1.0F;

      result.MaxDivergence = 150;

      result.AddStep(new Step(100, Pens.LightGreenPen5, Colors.White, Pens.Hollow));
      result.AddStep(new Step(2750, Pens.LightGreenPen5, Colors.White, Pens.Hollow));
      result.AddStep(new Step(3100, Pens.LightRedPen5, Colors.Red, Pens.Hollow));

      result.AddTick(new TickMark(500, null, 7));
      result.AddTick(new TickMark(1000, "1", 10));
      result.AddTick(new TickMark(1500, null, 7));
      result.AddTick(new TickMark(2000, "2", 10));
      result.AddTick(new TickMark(2500, null, 7));
      result.AddTick(new TickMark(3000, "3", 10));

      result.DrawValue = true;
      result.ValueFont = valueFont;

      result.Initialize();

      return result;
    }

    protected override void OnPaint(CanFlyMsg msg)
    {
    }
  }

  public class OverlayWidget : Widget
  {
    public OverlayWidget(Widget parent, Rect bounds, ushort id)
    : base(parent, bounds, id)
    {

    }

    protected override void OnPaint(CanFlyMsg e)
    {
      BeginPaint();

      Line(Pens.WhitePen, Point.Create(0, 80), Point.Create(220, 80));
      Line(Pens.WhitePen, Point.Create(220, 0), Point.Create(220, 240));
      Line(Pens.WhitePen, Point.Create(220, 160), Point.Create(320, 160));
      Line(Pens.WhitePen, Point.Create(0, 200), Point.Create(220, 200));
      Line(Pens.WhitePen, Point.Create(140, 200), Point.Create(140, 240));

      EndPaint();
    }
  }
}