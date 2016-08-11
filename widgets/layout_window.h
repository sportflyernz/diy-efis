/*
diy-efis
Copyright (C) 2016 Kotuku Aerospace Limited

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.

If a file does not contain a copyright header, either because it is incomplete
or a binary file then the above copyright notice will apply.

Portions of this repository may have further copyright notices that may be
identified in the respective files.  In those cases the above copyright notice and
the GPL3 are subservient to that copyright notice.

Portions of this repository contain code fragments from the following
providers.


If any file has a copyright notice or portions of code have been used
and the original copyright notice is not yet transcribed to the repository
then the origional copyright notice is to be respected.

If any material is included in the repository that is not open source
it must be removed as soon as possible after the code fragment is identified.
*/
#ifndef __layout_window_h__
#define __layout_window_h__

#include "widget.h"

class canaerospace_provider_t;
class menu_window_t;

// this class is the display window for an application.
namespace kotuku {

class layout_window_t : public widget_t
  {
public:
  layout_window_t(screen_t *);

  void set_layout(canaerospace_provider_t *can_provider);

  /**
   * Update the child windows
   */
  virtual void update_window();

  /**
   * called to update the screen.
   * @param force Always redraw the screen
   * @return  true if any windows or child windows were drawn
   */
  bool repaint(bool force);

private:
  virtual bool ev_msg(const msg_t &);
  };
  };

#endif