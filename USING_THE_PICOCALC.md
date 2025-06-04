# Using zeptoforth on the PicoCalc

There is driver and terminal emulator support in this branch for the PicoCalc. This will enable one to use the keyboard, display, and FAT32 filesystems in on-board flash, SD cards, and, if one is using a Pimoroni Pico Plus 2 or Pico Plus 2 W, the board's PSRAM (not the PicoCalc's carrier board PSRAM mind you, which is unsupported) of a PicoCalc without requiring the use of a terminal emulator on a PC. Note that this is currently beta quality, and may still have outstanding issues.

## zeptoforth installation and preparation

First, one must download the [latest release of zeptoforth](https://github.com/tabemann/zeptoforth/releases) and install a `full` build (_not_ a `full_usb` build) on the RP2040 or RP2350 board in your PicoCalc. Note that a `full` build is needed because the PicoCalc redirects UART0 to an ACM device on its USB-C port, and it is highly recommended that one use the USB-C port on the PicoCalc for extended usage rather than the USB port on the RP2040 or RP2350 board itself as there have been reports of battery overcharging in such extended usage of the board's USB port (some even recommend removing the batteries from one's PicoCalc prior to applying power to the board's USB port).

Second, one must pull the [picocalc-devel branch](https://github.com/tabemann/zeptoforth/tree/picocalc-devel) of the zeptoforth repository. As you are reading this you may have already done so.

## PicoCalc terminal emulator installation

Then, if one is using a shell prompt one should execute the following commands from the root of the zeptoforth directory tree:

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
- Issue `compile-to-flash`.
- Set the working directory to the root of the zeptoforth directory tree.
- Upload `extra/common/ili9488_spi_8_6x8_font_all.fs`.
- Upload `extra/rp_common/picocalc_key.fs`.
- Upload `extra/rp_common/picocalc_term_common.fs`
- Upload `extra/rp_common/picocalc_term.fs`.
- Issue `initializer picocalc-term::term-console`.
- Issue 'reboot'.

After these steps your PicoCalc will be ready for use!

However, read further before you do the above because below are instructions for things such as installing the text-only terminal emulator, selecting alternate fonts, installing FAT32 support, and installing the text editor zeptoed.

## Text-only terminal emulators

While the above directions provides 8-bit RGB graphics and is relatively fast, it has the downside that a lot of memory is used for the framebuffer, which may prove problematic if you want to use an RP2040 (e.g. Raspberry Pi Pico, Raspberry Pi Pico W) board, and especially if you want to use zeptoIP (as zeptoIP by itself is very memory-costly) or zeptoed (as zeptoed requires a good-sized amount of memory for storage of text in memory).

Luckily, there is a less memory-expensive option available! This is the text-only PicoCalc terminal emulator. This only stores the 8-bit text and its attributes in memory, so it uses far less RAM than the graphical PicoCalc terminal emulator.

To install the text-only PicoCalc terminal emulator, execute the above steps except substitute `extra/rp_common/picocalc_term_text.fs` for `extra/rp_common/picocalc_term.fs` and one of `extra/common/ili9488_spi_text_5x8_font_all.fs`, `extra/common/ili9488_spi_text_6x8_font_all.fs`, or `extra/common/ili9488_spi_text_7x8_font_all.fs` for `extra/common/ili9488_spi_8_6x8_font_all.fs` (see 'Font selection' below).

Note however that the text-only PicoCalc terminal emulator is a bit slower in scrolling than the graphical PicoCalc terminal emulator because it has to redraw each character when updating the screen rather than merely moving the pixel data in th e framebuffer and then converting an 8-bit RGB pixmap to 16-bit RGB and sending it to the screen, which is simpler and thus faster. Currently hardware scrolling is not supported, even though it may be supported in the future.

## Font selection

Note, however, that one may want to select a font other than the default 6x8-pixel font. The other available fonts are 5x8-pixel and 7x8-pixel fonts. A 5x8-pixel font may be desired if one wants the terminal emulator display to be 64 characters wide, while a 7x8-pixel font may be desired if one thinks that the default 6x8-pixel font is too small.

To do so, in the steps above substitute `extra/common/ili9488_spi_8_5x8_font_all.fs` or `extra/common/ili9488_spi_8_7x8_font_all.fs`, or if one is installing a text-only terminal emulator, `extra/common/ili9488_spi_text_5x8_font_all.fs` or `extra/common/ili9488_spi_text_7x8_font_all.fs`. Note that if more than one of these fonts are available at a time the smallest font will be used.

## Installing FAT32 support

There is optional support for FAT32 filesystems in on-board flash (using block storage as a backend), on one's SD card, and, if one is using a Pimoroni Pico Plus 2 or Pico Plus 2 W, as a RAM disk in PSRAM.

To enable support for FAT32 filesystems in on-board flash, with the serial console enabled, execute the following at the shell prompt at the base of the zeptoforth directory tree:

    $ utils/codeload3.sh -B 115200 -p <your tty device> serial extra/common/setup_blocks_fat32.fs

or with zeptocom.js upload `extra/common/setup_blocks_fat32.fs`.

There is no need to specify compiling to flash, and actually initially the board should be in its default of compiling to RAM.

This will automatically reboot your PicoCalc; note that if one is using `utils/codeload3.sh` an error about not finding the TTY device may appear at the end ─ please ignore this error.

Note that if this is the first time one is setting up a FAT32 filesystem in block storage and one is using a Pimoroni Pico Plus 2 or Pico Plus 2 W with an `rp2350_16mib` build I highly suggest using zeptocom.js to do so, to avoid timeout issues with `utils/codeload3.sh` (as zeptocom.js does not use a timeout). This is because the first time one does this the block storage is first erased and then a FAT32 filesystem is initialized in it, which takes a good bit of time due to the 14 MiB size of the filesystem.

Afterwards, if one is using a Pimoroni Pico Plus 2 or Pico Plus 2 W, it is highly recommended that one set up support for a FAT32 filesystem in PSRAM as a RAM disk. This is accomplished by, with the serial console enabled, executing the following at the shell prompt at the base of the zeptoforth directory tree:

    $ utils/codeload3.sh -B 115200 -p <your tty device> serial extra/rp2350/setup_pico_plus_2_psram_fat32.fs

or with zeptocom.js upload `extra/rp2350/setup_pico_plus_2_psram_fat32.fs`.

Finally, with the serial console enabled, execute the following at the shell prompt at the base of the zeptoforth directory tree:

    $ utils/codeload3.sh -B 115200 -p <your tty device> serial extra/rp_common/picocalc_fat32.fs

or with zeptocom.js upload `extra/rp_common/picocalc_fat32.fs`.

Note that if you do _not_ want to install support for FAT32 filesystems on the SD card (as this will cause a hang on boot if the SD card is not inserted, is not initialized to having a FAT32 filesystem as the first partition, is corrupt, or is a traditional SD (not SDHC or SDXC) card), you can disable SD card support by before you execute the above connecting to your PicoCalc's serial console with a terminal emulator and executing the following:

    compile-to-flash
    true constant no-sd-fs
    reboot

## Using FAT32 filesystems

Once you have installed FAT32 support any or all of the following words will be created in the `fat32-tools` module depending on your particular configuration:

- `blocks-fs@` ( -- fs )
- `blocks-fs:` ( -- )
- `sd-fs@` ( -- fs )
- `sd-fs:` ( -- )
- `psram-fs@` ( -- fs )
- `psram-fs:` ( -- )

`blocks-fs@`, `sd-fs@`, and `psram-fs@` return FAT32 filesystem objects.

`blocks-fs:`, `sd-fs:`, and `psram-fs:` change the current filesystem.

`blocks-fs@` and `blocks-fs:` pertain to the FAT32 filesystem in block storage in on-board flash, and are available if `extra/common/setup_blocks_fat32.fs` has been installed.

`sd-fs@` and `sd-fs:` pertain to the FAT32 filesystem on the SD card inserted into the PicoCalc, and are available if `no-sd-fs` has not been defined.

`psram-fs@` and `psram-fs:` pertain to the FAT32 filesystem in PSRAM on a Pimoroni Pico Plus 2 or Pico Plus 2 W, and are available if `extra/rp2350/setup_pico_plus_2_psram_fat32.fs` has been installed.

Note that you will likely want to import the `fat32-tools` module prior to use with the following at the REPL:

    fat32-tools import

You may even want to persistently import it on boot with:

    compile-to-flash
    : init-import-fat32-tools fat32-tools import ;
    initializer init-import-fat32-tools
    reboot

For more information on how to use `fat32-tools` consult the [`fat32-tools` documentation](https://github.com/tabemann/zeptoforth/blob/master/docs/words/fat32_tools.md).

## Rebooting/interrupting your PicoCalc

An important note is that to reboot your PicoCalc without power-cycling it, press Control-Break (Control-Shift-Escape). This is used in the place of the Control-C used at the serial or USB CDC consoles, which is handled like a normal Control-C character by the PicoCalc terminal emulator.

Similarly, the attention key is mapped to Break (Shift-Escape). This is used in t he place of the Control-T used at the serial or USB CDC consoles. Like Control-C, Control-T is handled like a normal control character by the PicoCalc terminal emulator.

Unlike Control-C and Control-T at the serial and USB CDC consoles, Control-Break and Break are handled by ordinary tasks, so if there has been a crash, the processor is stuck in a critical section, or a task with a higher priority than the keyboard driver or terminal emulator tasks is executing without reliquishing control of the processor Control-Break or Break may not work. In this event, the only real way to regain control of your PicoCalc is to power-cycle it.

## Switching between consoles

Once you have your PicoCalc set up to boot into the PicoCalc terminal emulator, you may wonder how to get back to the serial console, in order to do things like upload code from or transfer files to or from your computer. This can be accomplished with:

    serial::serial-console

Then you may wonder how to get back to the PicoCalc terminal emulator without rebooting. This can be accomplished with:

    picocalc-term::term-console

Last but not least, if you were one of the misguided people who installed a `full_usb` build on your PicoCalc, you can switch to the USB CDC console with:

    usb::usb-console

## Editing the current line

You may perhaps want to be able to edit code on the current line beyond merely backspacing, and you likely will want history as well. These are available to you! All you have to do is enter at the REPL:

    enable-line

The interface that is available is very similar to that of GNU Readline, albeit somewhat simplified (e.g. there is no searching in history).

One important note is that by default uploading code with `utils/codeload3.sh` or zeptocom.js will _not_ work while the line editor is enabled. Therefore, if one wants to upload code either disable it with:

    disable-line

or put it into mass upload mode with F1. To exit out of mass upload mode enter F2 as the first key on a line.

To persistently boot into the line editor execute:

    compile-to-flash
    initializer enable-line
    reboot

For information on the line editor, consult [its documentation](https://github.com/tabemann/zeptoforth/blob/master/docs/words/line.md).

## Editing text in files

Once you have FAT32 filesystem(s) set up, you may wonder what the next step is. That next step is `zeptoed`, which is a screen-oriented multi-buffer text editor for zeptoforth which can be used on the PicoCalc.

To load zeptoed on your PicoCalc execute the following at a shell prompt at the root of the zeptoforth directory tree:

    $ TTY=<your tty device> # Replace <your tty device> with your actual TTY device
    $ echo 'compile-to-flash' > prefix.fs
    $ echo 'reboot' > suffix.fs
    $ utils/codeload3.sh -B 115200 -p ${TTY} serial prefix.fs
    $ utils/codeload3.sh -B 115200 -p ${TTY} serial extra/common/zeptoed_all.fs
    $ utils/codeload3.sh -B 115200 -p ${TTY} serial suffix.fs

or do the following with zeptocom.js:

- Connect to the TTY device for the PicoCalc. The connection settings can be left at their defaults for zeptoforth.
- Issue `compile-to-flash`.
- Set the working directory to the root of the zeptoforth directory tree.
- Upload `extra/common/zeptoed_all.fs`.
- Issue 'reboot'.

You can then invoke zeptoed with `zeptoed` ( path-addr path-bytes -- ) or, for short, `zed` ( path-addr path-bytes -- ) where *path-addr* *path-bytes* refers to a string literal denoting the path of the file in the current filesystem.

For more information on using zeptoed consult its online help or [the zeptoed documentation](https://github.com/tabemann/zeptoforth/blob/master/docs/extra/zeptoed.md).

One important note, though, is one is using the graphical terminal emulator on an RP2040 (e.g. a Raspberry Pi Pico) there will not be enough RAM available for the default heap size for zeptoed, so starting zeptoed will result in a crash. As a result, adjust the heap size prior to starting zeptoed with:

    <desired heap size> to zeptoed-heap-size

To figure out a heap size to use execute:

    unused

which will output, amongst other things, the amount of RAM available in the RAM dictionary for the main task. Use this amount of RAM minus about 10 KiB as a rule of thumb for the maximum heap size supported.

Note that you can adjust `zeptoed-heap-size` multiple times in a session as you load more code and data into RAM. Also note that it does not by default persist across boots; to make it persist execute:

    compile-to-flash
    : init-zeptoed-heap-size <desired heap size> to zeptoed-heap-size ;
    initializer init-zeptoed-heap-size
    reboot

## Development status

Also note that some things currently may not work as expected ─ edit assumes a screen wider than 64 characters currently, and some other issues have been reported with it on the PicoCalc as well, words assumes a screen of at least 80 characters wide and will line wrap with rather un-cosmetic results, the copyright notice and license displayed by license likewise is hard-formatted to 80 characters wide, and the zeptoed help is hard-formatted to 80 characters wide.

However, I would say that zeptoforth's PicoCalc spport is still already very functional, especially since I do not yet have a PicoCalc in my possession and have been relying on the help of others who already possess PicoCalcs along with compile-time selection of an ST7789V display (which I do own) and runtime-selectable limited PicoCalc keyboard emulation (however the emulation is partial as it does not currently directly emulate many of the keys on the actual PicoCalc, rather passing through the characters entered from one's terminal emulator connected to one's board via the serial console).

If you do decide to try out zeptoforth on your PicoCalc, please give feedback on what works, what doesn't work, what suggestions you have, and even just the fact that you opted to try it. This will be extremely helpful in furthering the development of zeptoforth support for the PicoCalc.

I hope you enjoy trying out zeptoforth on your PicoCalc!

## Emulating a PicoCalc

For those such as myself who do not yet possess a PicoCalc, some measures may be taken in its place. The PicoCalc terminal emulator can support an SPI ST7789V display in the place of the SPI ILI9488 display used by the PicoCalc. When loading the code into flash substitute `extra/common/ili9488_spi_8_5x8_font_all.fs`, `extra/common/ili9488_spi_8_6x8_font_all.fs`, or `extra/common/ili9488_spi_8_7x8_font_all.fs` with `extra/common/st7789v_spi_8_5x8_font_all.fs`, `extra/common/st7789v_spi_8_6x8_font_all.fs`, or `extra/common/st7789v_spi_8_7x8_font_all.fs`. Also, prior to loading edit `extra/rp_common/picocalc_term.fs` to set the GPIO selections for your particular setup. Also, omit sending `initializer picocalc-term::term-console` to the board in the PicoCalc.

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
