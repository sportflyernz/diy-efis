// Basic can aerospace framework support files
#include "../../libs/electron/electron.h"
#include "../../libs/proton/proton.h"
#include "../../libs/muon/muon.h"
#include "../../libs/neutron/stream.h"
#include "../../libs/neutron/slcan.h"

#include "../../libs/ion/interpreter.h"
#include "msh_cli.h"

#include <stdio.h>
#include <sys/types.h>
#include <fcntl.h>
#ifdef __linux__
#include <termios.h>
#include <sys/stat.h>
#else
#include <Windows.h>
#endif

#ifndef __linux__

#pragma comment(lib, "muon")
#pragma comment(lib, "neutron")
#pragma comment(lib, "krypton")

#endif

extern cli_node_t cli_root;

#define CAN_TX_BUFFER_LEN 1024
#define CAN_RX_BUFFER_LEN 1024

const char *node_name = "muon";

static FILE *ci;
static FILE *co;

typedef struct _service_channel_t
  {
  stream_handle_t stream;
  // this MUST be first so we can cast a parser to a channel !!!
  // parser handling the commands
  cli_t parser;
  } service_channel_t;


  static result_t css_stream_eof(stream_handle_t *hndl)
    {
    return feof(ci) ? s_ok : s_false;
    }

  static result_t css_stream_read(stream_handle_t *hndl, void *buffer, uint16_t size, uint16_t *read)
    {
    if(hndl == 0 || buffer == 0 || size == 0)
      return e_bad_parameter;



#ifdef __linux__
    char *buf = (char *)buffer;
    int ch;
    fflush(ci);
    fflush(co);

    while(size-- > 0)
      {
      ch = getc(ci);
      if(ch == EOF)
        break;

      if(ch == '\r')
        ch= '\n';

      *buf++ = ch;
      }

    if(read != 0)
      *read = 1;
#else
    DWORD bytesRead;
    ReadFile(ci, buffer, size, &bytesRead, NULL);
    if (read != 0)
      *read = (uint16_t)bytesRead;

    char *str = (char *)buffer;
    while (bytesRead--)
      if (*str == '\r')
        *str = '\n';
#endif

    return s_ok;
    }

  static result_t css_stream_write(stream_handle_t *hndl, const void *buffer, uint16_t size)
    {
    if(hndl == 0 || buffer == 0 || size == 0)
      return e_bad_parameter;
#ifdef __linux__

    const char *buf = buffer;
    while(size-- > 0)
      putc(*buf++, co);

    fflush(co);
#else
    WriteFile(co, buffer, size, NULL, NULL);
#endif

    return s_ok;
    }

  static result_t css_stream_close(stream_handle_t *hndl)
    {
    return s_ok;
    }

int main(int argc, char **argv)
  {
	// The command line can pass in the name of the registry used to set us up.  In any
  // case we need to implement some code
  const char *ini_path;
  if(argc > 1)
    ini_path = argv[1];
  else
    ini_path = "diy-efis.reg";


  // TODO: handle this better
  bool factory_reset = false;

  electron_init(ini_path, factory_reset);

  uint16_t node_id;
  if(failed(reg_get_uint16(0, "node-id", &node_id)))
    node_id = mfd_node_id;

  neutron_parameters_t init_params;
  init_params.node_id = (uint8_t) node_id;
  init_params.node_type = unit_pi;
  init_params.hardware_revision = 0x11;
  init_params.software_revision = 0x10;
  init_params.tx_length = CAN_TX_BUFFER_LEN;
  init_params.tx_stack_length = 4096;
  init_params.rx_length = CAN_RX_BUFFER_LEN;
  init_params.rx_stack_length = 4096;
  init_params.publisher_stack_length = 4096;

#ifdef __linux__
    struct termios tio;
    tcgetattr(0, &tio);

    //
    // Input flags - Turn off input processing
    // convert break to null byte, no CR to NL translation,
    // no NL to CR translation, don't mark parity errors or breaks
    // no input parity check, don't strip high bit off,
    // no XON/XOFF software flow control
    //
    tio.c_iflag &= ~(IGNBRK | BRKINT | ICRNL | INLCR | PARMRK | INPCK | ISTRIP | IXON);
    //
    // Output flags - Turn off output processing
    // no CR to NL translation, no NL to CR-NL translation,
    // no NL to CR translation, no column 0 CR suppression,
    // no Ctrl-D suppression, no fill characters, no case mapping,
    // no local output processing
    //
    tio.c_oflag &= ~(OCRNL | ONLCR | ONLRET | ONOCR | OFILL | OLCUC | OPOST);
    //
    // No line processing:
    // echo off, echo newline off, canonical mode off,
    // extended input processing off, signal chars off
    //
    tio.c_lflag &= ~(ECHO | ECHONL | ICANON | IEXTEN | ISIG);
    //
    // Turn off character processing
    // clear current char size mask, no parity checking,
    // no output processing, force 8 bit input
    //
    tio.c_cflag &= ~(CSIZE | PARENB);
    tio.c_cflag |= CS8;
    //
    // One input byte is enough to return from read()
    // Inter-character timer off
    //
    tio.c_cc[VMIN] = 1;
    tio.c_cc[VTIME] = 0;
    //tio.c_cc[VEOL] = '\r';

    tio.c_cflag |= CREAD;
    speed_t speed  = B115200;

    cfsetospeed(&tio, speed);
    cfsetispeed(&tio, speed);

    tcflush(0, TCIFLUSH);

    tcsetattr(0, TCSANOW, &tio);

    ci = stdin;
    co = stdout;
 #else
  ci = GetStdHandle(STD_INPUT_HANDLE);
  DWORD dwMode;
  GetConsoleMode(ci, &dwMode);
  dwMode &= ~ENABLE_LINE_INPUT;
  dwMode |= ENABLE_VIRTUAL_TERMINAL_INPUT;
  SetConsoleMode(ci, dwMode);

  co = GetStdHandle(STD_OUTPUT_HANDLE);
  GetConsoleMode(co, &dwMode);
  dwMode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;
  SetConsoleMode(co, dwMode);
#endif

  // start the canbus stuff working
  neutron_init(&init_params, factory_reset);

  service_channel_t channel;
  memset(&channel, 0, sizeof(service_channel_t));

  channel.stream.version = sizeof(service_channel_t);
  channel.stream.stream_eof = css_stream_eof;
  channel.stream.stream_read = css_stream_read;
  channel.stream.stream_write = css_stream_write;
  channel.stream.stream_close = css_stream_close;

  channel.parser.cfg.root = &msh_cli_root;
  channel.parser.cfg.ch_complete = '\t';
  channel.parser.cfg.ch_erase = '\b';
  channel.parser.cfg.ch_del = 127;
  channel.parser.cfg.ch_help = '?';
  channel.parser.cfg.flags = 0;
  channel.parser.cfg.prompt = string_create(node_name);

  channel.parser.cfg.console_in = &channel.stream;
  channel.parser.cfg.console_out = &channel.stream;
  channel.parser.cfg.console_err = &channel.stream;

  if (failed(ion_init()))
    {
    stream_printf(&channel.stream, "Unable to start ion\r\n");
    return - 1;
    }

  if (failed(cli_init(&channel.parser.cfg, &channel.parser)))
    {
    stream_printf(&channel.stream, "Unable to start muon\r\n");
    return -1;
    }

  return cli_run(&channel.parser);
  }

static handle_t driver;

result_t bsp_can_init(handle_t rx_queue)
  {
  result_t result;
  memid_t key;
  if(failed(result = reg_open_key(0, "slcan", &key)))
    return result;

  return slcan_create(key, rx_queue, &driver);
  }

result_t bsp_send_can(const canmsg_t *msg)
  {
  if(msg == 0 || driver == 0)
    return e_bad_parameter;

  return slcan_send(driver, msg);
  }
