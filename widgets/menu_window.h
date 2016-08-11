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
#ifndef __menu_window_h__
#define __menu_window_h__

#include "widget.h"

// TODO: push into hal as this is linux specific
#include "regex.h"
#include <deque>

namespace kotuku {

/*
# A menu is composed of settings with the following types

# menu:<text>,<menu name>
# popup:<text>,<menu name>,<key menu name>
# cancel:<text/resourceid>      defaults to 'Cancel' if no text
# enter:<text/resourceid>     defaults to 'Enter' if no text
# event:<text/resourceid>,<id>,<datatype>,[param],[<options>] sends a canaerospace message (see options below)
#
# <text/resourceid> can be one of:
# Text      Surround text with ""
# resourceid  One of
#   none  Display nothing
#   up  Draws an up glyph
#   dn  Draws a down glyph
#   direct  Draws a direct-to glyph
#
# options can be one of:
# no_publish  Does not publish the event on the can-bus.  Is local to the display only
#
# edit:<text/resourceid>,<menu>,<type>,[<param=value>,]   Edits a can value
#
# <text/resourceid> is as in an event
#
# <type> can be one of:
#
# NUMERIC         A number editor
#   Params are:
#     type=<type>   Can aerospace type, usually SHORT or FLOAT
#     value=id    A canerospace value the will be the current value
#     set_value=id    Message to set the value
#     precision=num   Optional precision, if ommitted 0 is assumed and an integer value
#     digits=num    Number of digits, if precision is included will be <digits>.<precision>
#     min_value=num
#     max_value=num
#     circualar=<true/false>  If provided the editor will swap to min on overflow and max on underflow
#     decka_increment=<value> Normally decka increments the digits, this sets by how much.  defaults to 1
#     deckb_increment=<value> If a precision is set then this sets how much the minor value changes, otherwise allows for
#           incrementing the digits.
#   example for a frequency:  type=FLOAT,value=nnn,set_value=nnn,precision=3,digits=2,min_value=108.0,max_value=136.75,circular=true,decka_increment=1,deckb_increment=0.25
#
# A popup menu can have any number of items that can be an edit, or a menu.  When an edit is defined the popup stays visible
# and the edit appears to the right of the item.
# If a menu is selected, then a new menu is displayed, waiting for input.  If cancel is pressed the previous menu
# is entered.  Pressing Enter on an edit closes all menus
 */

class menu_window_t;

enum menu_item_type
  {
  mi_menu, // item is a menu
  mi_popup, // item is a popup menu
  mi_cancel, // item is a cancel option
  mi_enter, // item is an accept option
  mi_event, // item is an event generator
  mi_edit, // item is a property editor
  mi_checklist, // item is a selection item
  };

enum menu_item_action_result
  {
  mia_nothing, // no change to menu state
  mia_cancel, // event causes a cancel
  mia_enter, // event causes a done, all menu close
  mia_close_item, // either a popup or edit is closed
  };

class menu_item_t : public canaerospace_provider_t::can_parameter_handler_t
  {
public:
  menu_item_t(menu_window_t *parent);
  virtual ~menu_item_t();

  /**
   * Return the generic type of the menu item
   * @return  type of menu item
   */
  virtual menu_item_type item_type() const = 0;
  /**
   * Handle a message from the menu system
   * @param can message generated by a menu button
   */
  virtual menu_item_action_result evaluate_action(const msg_t &) const = 0;

  /**
   * Return the menu to display when editing or selecting a popup menu
   * @return  name of the menu to show when the item is edited or selected
   */
  virtual const std::string &item_menu() const
    {
    return _item_menu;
    }
  /**
   * render the menu item
   * @param window  Window to draw on
   * @param area    Area assigned for menu item
   */
  virtual void paint(const rect_t &area, bool is_selected) const;

  /**
   * Return the parent window that holds the menu
   * @return
   */
  menu_window_t *parent() const
    {
    return _parent;
    }
  /**
   * Assign the expression that is used to test if this menu item is
   * enabled
   * @param expr expression in the form <param>:<expression>
   */
  void assign_enabler(const char *expr);
  /**
   * Evaluate the expression and return true if this is a
   * enabled
   * @return
   */
  bool is_enabled() const;
  /**
   * Set the caption of the menu item
   * @param cpation or resource id
   */
  void caption(const std::string &);

  /**
   * Cpation or resource ID assigned
   * @return
   */
  const std::string &caption() const
    {
    return _caption;
    }
  void receive_parameter(canaerospace_provider_t *, const msg_t &);
  bool is_equal(const can_parameter_handler_t &) const;
protected:
  void item_menu(const char *);
private:
  std::string _caption;
  const bitmap_t *_bitmap;
  std::string _item_menu;
  menu_window_t *_parent;
  std::string _enable_regex;
  std::string _enable_format;
  uint16_t _controlling_param;
  msg_t _controlling_variable;
  TRex *_pat_buff;
  };

typedef std::vector<menu_item_t *> menu_items_t;
class root_menu_t;

class menu_item_menu_t : public menu_item_t
  {
public:
  menu_item_menu_t(menu_window_t *parent, const char *options, const char *expr);
  virtual ~menu_item_menu_t();
  virtual menu_item_type item_type() const;
  virtual menu_item_action_result evaluate_action(const msg_t &) const;
private:
  root_menu_t *_menu;
  };

class popup_menu_t;

class menu_item_popup_t : public menu_item_t
  {
public:
  menu_item_popup_t(menu_window_t *parent, const char *options, const char *expr);
  virtual ~menu_item_popup_t();
  virtual menu_item_type item_type() const;
  virtual menu_item_action_result evaluate_action(const msg_t &) const;

  const popup_menu_t *popup() const
    {
    return _popup;
    }
private:
  popup_menu_t *_popup;
  // root menu to display when selected
  root_menu_t *_menu;
  };

class menu_item_cancel_t : public menu_item_t
  {
public:
  menu_item_cancel_t(menu_window_t *parent, const char *options, const char *expr);
  virtual ~menu_item_cancel_t();
  virtual menu_item_type item_type() const;
  virtual menu_item_action_result evaluate_action(const msg_t &) const;
  };

class menu_item_enter_t : public menu_item_t
  {
public:
  menu_item_enter_t(menu_window_t *parent, const char *options, const char *expr);
  virtual ~menu_item_enter_t();
  virtual menu_item_type item_type() const;
  virtual menu_item_action_result evaluate_action(const msg_t &) const;
  };

class menu_item_event_t : public menu_item_t
  {
public:
  menu_item_event_t(menu_window_t *parent, const char *options, const char *expr);
  virtual ~menu_item_event_t();
  virtual menu_item_type item_type() const;
  virtual menu_item_action_result evaluate_action(const msg_t &) const;
private:
  typedef std::vector<msg_t> msgs_t;
  msgs_t _events;
  };

class msg_editor_t : public canaerospace_provider_t::can_parameter_handler_t
  {
public:
  msg_editor_t();
  virtual menu_item_action_result evaluate_action(const msg_t &) const = 0;
  };

class menu_item_edit_t : public menu_item_t
  {
public:
  menu_item_edit_t(menu_window_t *parent, const char *options, const char *expr);
  virtual ~menu_item_edit_t();
  virtual menu_item_type item_type() const;
  virtual menu_item_action_result evaluate_action(const msg_t &) const;

  const root_menu_t *root_menu() const
    {
    return _menu;
    }
private:
  root_menu_t *_menu;
  msg_editor_t *_editor;
  };

class menu_item_checklist_t : public menu_item_t
  {
public:
  menu_item_checklist_t(menu_window_t *parent, const char *options, const char *expr);
  virtual ~menu_item_checklist_t();
  virtual menu_item_type item_type() const;
  virtual menu_item_action_result evaluate_action(const msg_t &) const;
  };

enum e_menu_type
  {
  mt_root,
  mt_popup
  };

/**
 * This class forms the base menu for the system.  Always displayed
 * at the bottom of the menu screen.
 */
class menu_t
  {
public:
  menu_t(menu_window_t *parent);
  virtual ~menu_t();

  /**
   * Get the next up menu.
   * @return  The next up menu, if 0 then the menu is closed.
   */
  menu_t *parent_menu() const
    {
    return _parent_menu;
    }

  void parent_menu(menu_t *value)
    {
    _parent_menu = value;
    }

  menu_window_t *parent() const
    {
    return _parent;
    }

  virtual size_t size() const = 0;

  const menu_item_t *operator[](int index) const
    {
    return at(index);
    }

  virtual const menu_item_t *at(int index) const = 0;
  virtual e_menu_type menu_type() const = 0;
private:
  menu_window_t *_parent;
  menu_t *_parent_menu;
  };

class root_menu_t : public menu_t
  {
public:
  root_menu_t(menu_window_t *parent, const std::string &section);
  ~root_menu_t();
  virtual size_t size() const;
  virtual const menu_item_t *at(int index) const;
  virtual e_menu_type menu_type() const;

  const menu_item_t *key0() const
    {
    return _key0;
    }

  const menu_item_t *key1() const
    {
    return _key1;
    }

  const menu_item_t *key2() const
    {
    return _key2;
    }

  const menu_item_t *key3() const
    {
    return _key3;
    }

  const menu_item_t *key4() const
    {
    return _key4;
    }

  const menu_item_t *decka_up() const
    {
    return _decka_up;
    }

  const menu_item_t *decka_dn() const
    {
    return _decka_dn;
    }

  const menu_item_t *deckb_up() const
    {
    return _deckb_up;
    }

  const menu_item_t *deckb_dn() const
    {
    return _deckb_dn;
    }
private:
  menu_item_t *_key0;
  menu_item_t *_key1;
  menu_item_t *_key2;
  menu_item_t *_key3;
  menu_item_t *_key4;
  menu_item_t *_decka_up;
  menu_item_t *_decka_dn;
  menu_item_t *_deckb_up;
  menu_item_t *_deckb_dn;

  };

class popup_menu_t : public menu_t
  {
public:
  popup_menu_t(menu_window_t *parent, const std::string &section);
  ~popup_menu_t();
  virtual size_t size() const;
  virtual const menu_item_t *at(int index) const;
  virtual e_menu_type menu_type() const;
  /**
   * Draw the selected popup item, if applicable
   * @param selected_item     Index of the selected popup item
   * @param parent
   * @param popup_item_origin
   */
  void draw_popup(size_t selected_item, menu_window_t *parent, const point_t &popup_item_origin) const;
private:
  menu_items_t _menu_items;
  };

class menu_window_t : public widget_t
  {
public:
  // create window with the passed in registry key to set the root hive
  menu_window_t(screen_t *screen, const char *);
  menu_window_t(widget_t &parent, const rect_t &, const char *section);
  /**
   * Show a new root menu on the window
   * @param Menu to display
   */
  void show_menu(const root_menu_t *);
  /**
   * Close the current root menu and popup menu
   * and restore the previous menu
   */
  void pop_menu();
  /**
   * Close all menu's.  Called by the cancel: menu item
   */
  void close_menu();
  /**
   * Show a popup menu at the left of the menu window.
   * @param Popup           menu to display
   * @param option_selected Option that is selected. Will be the bottom option
   */
  void show_popup_menu(const popup_menu_t *, size_t option_selected);
  /**
   * Change the first display item to list item-1 if possible
   */
  void scroll_popup_up();
  /**
   * Change the first display item to list item +1 if possible
   */
  void scroll_popup_dn();
  /**
   * Close the current popup menu.
   */
  void close_popup_menu();
  /**
   * Open the popup menu editor and show the edit menu
   */
  void show_popup_menu_editor();

  virtual void update_window();

  /**
   * Load a menu and store it.
   * @param name  Name of the menu
   * @return the menu, 0 if not found
   * @remarks The menu may not be loaded if it is already cached.
   */
  root_menu_t *load_menu(const std::string &name);

  /**
   * Load a menu and store it.
   * @param name  Name of the menu
   * @return the menu, 0 if not found
   * @remarks The menu may not be loaded if it is already cached.
   */
  popup_menu_t *load_popup(const std::string &name);

  void update_background(canvas_t &background_canvas);
private:
  virtual bool ev_msg(const msg_t &);

  /**
   * This holds the global root menu for the system
   */
  const root_menu_t *_root_menu;
  /**
   * This holds the menu that is currently displayed in the bottom screen
   */
  const root_menu_t *_active_menu;

  /**
   * This holds the current popup menu that can be a property editor or
   * action items
   */
  const popup_menu_t *_popup_menu;
  // bottom menu item displayed.  The menu will expand to fit the
  // vertical height of the screen.
  size_t _popup_menu_index;
  // option that is selected
  size_t _popup_menu_option_selected;
  bool _editor_open; // if true the popup menu editor is active

  const menu_item_edit_t *_edit_item;

  short _menu_timer; // as a press/rotate is given this sets the tick-timeout

  std::string _section;

  void load_menus();

  typedef std::map<std::string, menu_t *> menus_t;
  menus_t _menus;

  gdi_dim_t _menu_rect_x;
  gdi_dim_t _menu_rect_y;
  gdi_dim_t _menu_start_x;
  gdi_dim_t _menu_start_y;

  typedef std::deque<const root_menu_t *> menu_stack_t;
  menu_stack_t _menu_stack;

  void init(const char *);
  };
  };
  
#endif