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
    $ utils/codeload3.sh -B 115200 -p ${TTY} serial extra/rp_common/picocalc_term.fs
    $ utils/codeload3.sh -B 115200 -p ${TTY} serial suffix.fs

If you are using zeptocom.js, one should do the following:

- Connect to the TTY device for the PicoCalc. The connection settings can be left at their defaults for zeptoforth.
- Issue compile-to-flash.
- Set the working directory to the root of the zeptoforth directory tree.
- Upload extra/common/ili9488_spi_8_6x8_font_all.fs.
- Upload extra/rp_common/picocalc_key.fs.
- Upload extra/rp_common/picocalc_term.fs.
- Issue initializer picocalc-term::term-console.
- Issue 'reboot'.

After these steps your PicoCalc will be ready for use!

## Font selection

Note, however, that one may want to select a font other than the default 6x8-pixel font. The other available fonts are 5x8-pixel and 7x8-pixel fonts. A 5x8-pixel font may be desired if one wants the terminal emulator display to be 64 characters wide, while a 7x8-pixel font may be desired if one thinks that the default 6x8-pixel font is too small.

To do so, in the steps above substitute `extra/common/ili9488_spi_8_5x8_font_all.fs` or `extra/common/ili9488_spi_8_7x8_font_all.fs`, and prior to loading edit `extra/rp_common/picocalc_term.fs` to set `use-5x8-font?` or `use-7x8-font?` to `true` and `use-6x8-font?` to `false` (note that only one of these three may be set to `true` at a time).

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
