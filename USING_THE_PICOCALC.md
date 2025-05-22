# Using zeptoforth on the PicoCalc

There is driver and terminal emulator support in this branch for the PicoCalc. This will enable one to use the keyboard and display of a PicoCalc without requiring the use of a terminal emulator on a PC. Note that this is currently beta quality, and may still have outstanding issues. Also, currently the Alt key is not supported beyond the baked-in support provided by the STM32 keyboard controller itself.

## Installation

First, one must download the [latest release of zeptoforth](https://github.com/tabemann/zeptoforth/releases) and install a `full` build (_not_ a `full_usb` build) on the RP2040 or RP2350 board in your PicoCalc. Note that a `full` build is needed because the PicoCalc redirects UART0 to an ACM device on its USB-C port, and it is highly recommended that one use the USB-C port on the PicoCalc for extended usage rather than the USB port on the RP2040 or RP2350 board itself as there have been reports of battery overcharging in such extended usage of the board's USB port (some even recommend removing the batteries from one's PicoCalc prior to applying power to the board's USB port).

Second, one must pull the [picocalc-devel branch](https://github.com/tabemann/zeptoforth/tree/picocalc-devel) of the zeptoforth repository. As you are reading this you may have already done so.

Third, if one is using a shell prompt one should execute the following commands from the root of the zeptoforth directory tree:

    $ TTY=<your tty device> # Replace <your tty device> with your actual TTY device
    $ echo 'compile-to-flash' > prefix.fs
    $ echo 'initializer picocalc-term::term-console' > suffix.fs
    $ echo 'reboot' >> suffix.fs
    $ utils/codeload3.sh -B 115200 -p ${TTY} serial prefix.fs
    $ utils/codeload3.sh -B 115200 -p ${TTY} serial extra/common/ili9488_spi_8_6x8_font_all.fs
    $ utils/codeload3.sh -B 115200 -p ${TTY} serial extra/rp_common/picocalc_keys.fs
    $ utils/codeload3.sh -B 115200 -p ${TTY} serial extra/rp_common/picocalc_term_common.fs
    $ utils/codeload3.sh -B 115200 -p ${TTY} serial extra/rp_common/picocalc_term.fs
    $ utils/codeload3.sh -B 115200 -p ${TTY} serial suffix.fs

If you are using zeptocom.js, one should do the following:

- Connect to the TTY device for the PicoCalc. The connection settings can be left at their defaults for zeptoforth.
- Issue compile-to-flash.
- Set the working directory to the root of the zeptoforth directory tree.
- Upload extra/common/ili9488_spi_8_6x8_font_all.fs.
- Upload extra/rp_common/picocalc_key.fs.
- Upload extra/rp_common/picocalc_term_common.fs
- Upload extra/rp_common/picocalc_term.fs.
- Issue initializer picocalc-term::term-console.
- Issue 'reboot'.

After these steps your PicoCalc will be ready for use!

## Text-only terminal emulators

While the above directions provides 8-bit RGB graphics and is relatively fast, it has the downside that a lot of memory is used for the framebuffer, which may prove problematic if one wants to use an RP2040 (e.g. Raspberry Pi Pico, Raspberry Pi Pico W) board, and especially if one wants to use zeptoIP (as zeptoIP by itself is very memory-costly) or zeptoed (as zeptoed requires a good-sized amount of memory for storage of text in memory).

Luckily, there is a less memory-expensive option available! This is the text-only PicoCalc terminal emulator. This only stores the 8-bit text and its attributes in memory, so it uses far less RAM than the graphical PicoCalc terminal emulator.

To install the text-only PicoCalc terminal emulator, execute the above steps except substitute `extra/rp_common/picocalc_term_text.fs` for `extra/rp_common/picocalc_term.fs` and one of `extra/common/ili9488_spi_text_5x8_font_all.fs`, `extra/common/ili9488_spi_text_6x8_font_all.fs`, or `extra/common/ili9488_spi_text_7x8_font_all.fs` for `extra/common/ili9488_spi_8_6x8_font_all.fs` (see 'Font selection' below).

Note however that the text-only PicoCalc terminal emulator is markedly slower, especially in scrolling, than the graphical PicoCalc terminal emulator because it has to redraw each character when updating the screen rather than merely converting an 8-bit RGB pixmap to 16-bit RGB and sending it to the screen, which is significantly simpler and thus faster. Currently hardware scrolling is not supported, even though it may be supported in the future.

## Font selection

Note, however, that one may want to select a font other than the default 6x8-pixel font. The other available fonts are 5x8-pixel and 7x8-pixel fonts. A 5x8-pixel font may be desired if one wants the terminal emulator display to be 64 characters wide, while a 7x8-pixel font may be desired if one thinks that the default 6x8-pixel font is too small.

To do so, in the steps above substitute `extra/common/ili9488_spi_8_5x8_font_all.fs` or `extra/common/ili9488_spi_8_7x8_font_all.fs`, or if one is installing a text-only terminal emulator, `extra/common/ili9488_spi_text_5x8_font_all.fs` or `extra/common/ili9488_spi_text_7x8_font_all.fs`. Note that if more than one of these fonts are available at a time the smallest font will be used.

## Rebooting/interrupting your PicoCalc

An important note is that to reboot your PicoCalc without power-cycling it, press Control-Break (Control-Shift-Escape). This is used in the place of the Control-C used at the serial or USB CDC consoles, which is handled like a normal Control-C character by the PicoCalc terminal emulator.

Similarly, the attention key is mapped to Break (Shift-Escape). This is used in t he place of the Control-T used at the serial or USB CDC consoles. Like Control-C, Control-T is handled like a normal control character by the PicoCalc terminal emulator. One important note, though, is that there appears to be a bug in Break followed by `z` to send an exception to the main task, as this currently sometimes causes a crash.

Unlike Control-C and Control-T at the serial and USB CDC consoles, Control-Break and Break are handled by ordinary tasks, so if there has been a crash, the processor is stuck in a critical section, or a task with a higher priority than the keyboard driver or terminal emulator tasks is executing without reliquishing control of the processor Control-Break or Break may not work. In this event, the only real way to regain control of your PicoCalc is to power-cycle it.

## Development status

Also note that some things currently may not work as expected ─ when I tried out the line editor I noticed some minor display issues with it that I have yet to adequately characterize, edit assumes a screen wider than 64 characters currently, and some other issues have been reported with it on the PicoCalc as well, words assumes a screen of at least 80 characters wide and will line wrap with rather un-cosmetic results, the copyright notice and license displayed by license likewise is hard-formatted to 80 characters wide, zeptoed relies on Meta (Alt) key combos that are not yet supported by the terminal emulator, and the zeptoed help is hard-formatted to 80 characters wide.

 However, I would say that zeptoforth's PicoCalc spport is still already very functional, especially since I do not yet have a PicoCalc in my possession and have been relying on the help of others who already possess PicoCalcs along with compile-time selection of an ST7789V display (which I do own) and runtime-selectable limited PicoCalc keyboard emulation (however the emulation is partial as it does not currently emulate many of the keys on the actual PicoCalc).

If you do decide to try out zeptoforth on your PicoCalc, please give feedback on what works, what doesn't work, what suggestions you have, and even just the fact that you opted to try it. This will be extremely helpful in furthering the development of zeptoforth support for the PicoCalc.

I hope you enjoy trying out zeptoforth on your PicoCalc!

## Emulating a PicoCalc

For those such as myself who do not yet possess a PicoCalc, some measures may be taken in its place. The PicoCalc terminal emulator can support an SPI ST7789V display in the place of the SPI ILI9488 display used by the PicoCalc. When loading the code into flash substitute `extra/common/ili9488_spi_8_5x8_font_all.fs`, `extra/common/ili9488_spi_8_6x8_font_all.fs`, or `extra/common/ili9488_spi_8_7x8_font_all.fs` with `extra/common/st7789v_spi_8_5x8_font_all.fs`, `extra/common/st7789v_spi_8_6x8_font_all.fs`, or `extra/common/st7789v_spi_8_7x8_font_all.fs`. Also, prior to loading edit `extra/rp_common/picocalc_term.fs` to set `use-st7789v?` to `true` and modify the GPIO selections for your particular setup. Also, omit sending `initializer picocalc-term::term-console` to the board in the PicoCalc.

Then, after the PicoCalc has rebooted, execute the following at the zeptoforth REPL via the serial console as a single line of code:

    true to picocalc-keys::emulate-keys? picocalc-term::term-console

This will result in input to the serial console being handled as if it were typed on the PicoCalc's keyboard. Note, however, that only non-control ASCII characters, newlines, and backspaces are currently emulated, so you will not be able to enter control characters or keys such as arrow or function keys.

## Drawing to the screen of a PicoCalc

The graphical (but not text-only) PicoCalc terminal emulator exposes a display that can be drawn to with `picocalc-term::with-term-display` ( xt -- ) where xt has the signature ( ??? display -- ??? ). An example routine to do so is:

    begin-module picocalc-hello

      picocalc-term-common import
      picocalc-term import
      oo import
      pixmap8 import
      font import
      rng import
    
      use-st7789v? not [if]
        ili9488-8-common import
      [else]
        st7789v-8-common import
      [then]
    
      use-5x8-font? [if]
        simple-font-5x8 import
      [then]
      use-6x8-font? [if]
        simple-font-6x8 import
      [then]
      use-7x8-font? [if]
        simple-font import
      [then]
      
      : hello ( r g b x y -- )
        [: { r g b x y display }
          r g b rgb8 s" Hello, world!" x y display
          [ use-5x8-font? ] [if] a-simple-font-5x8 [then]
          [ use-6x8-font? ] [if] a-simple-font-6x8 [then]
          [ use-7x8-font? ] [if] a-simple-font [then]
          draw-string-to-pixmap8
          display update-display
        ;] with-term-display
      ;
    
    end-module

In this example routine, `picocalc-hello::hello` ( r g b x y -- ), `with-term-display` calls a quotation which takes the color (with elements from 0 to 255) and the (x, y) coordinate of the upper left-hand corner of the "Hello, world!" string to draw and draws that string with the specified coordinate and color on the display with the selected font.

Afterwards, it calls `update-display` to copy the updated portion of the framebuffer to the display. Note that `update-display` is specific to the class `ili9488-8-common::<ili9488-8-common>` or `st7789v-8-common::<st7789v-8-common>` and does not belong to a shared superclass, so one will have to specifically import `ili9488-8-common` or `st7789v-8-common` depending on whether an actual PicoCalc is in use or not, respectively.

Note that `picocalc-term::with-term-display` locks the terminal emulator within the xt that it calls, so do not execute any code that may output text with `emit`, `type`, `.`, `."`, `.s`, `h.8`, or like within this xt as it may block forever because the output stream used by the terminal emulator is not emptied while it is locked and thus may become full.
