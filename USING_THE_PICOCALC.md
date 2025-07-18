# Using zeptoforth on the PicoCalc

There is driver and terminal emulator support in this branch for the PicoCalc. This will enable you to use the keyboard, display, and FAT32 filesystems in on-board flash, SD cards, and, if you are using a Pimoroni Pico Plus 2 or Pico Plus 2 W, the board's PSRAM (not the PicoCalc's carrier board PSRAM mind you, which is unsupported) of a PicoCalc without requiring the use of a terminal emulator on a PC.

## A note about modules and namespaces

In the file below module paths of the form `foo::bar`, where `bar` is a word in the wordlist/module `foo`, are frequently used. In all these cases, alternatively the wordlist/module, e.g. `foo`, can be imported with `foo import` and then the second word `bar` can be referenced directly. Note that with `import` _all_ words in, say, `foo` are imported into the current namespace.

Also note that these paths can be chained, e.g. `foo::bar::baz`, where `baz` is a word in the wordlist/module `foo::bar`, where in turn `bar` is a wordlist/module in the wordlist/module `foo`.

## Notes to those new to the PicoCalc

There are two important notes to keep in mind if you are new to the PicoCalc:

The first is that the screen on the PicoCalc is very fragile, and if it becomes misaligned and the case of the PicoCalc is screwed down or its connector is improperly flexed it may easily be damaged. As a result, it is highly recommended you tape down (minding the light channel for the power LED) your screen when assembling your PicoCalc to prevent it from moving during assembly or when swapping out boards within the PicoCalc.

The second is that the zeptoforth BIOS driver assumes that you have upgraded the firmware on the STM32 in your PicoCalc to the latest (i.e. 1.2 or newer) firmware. There is no good way of checking the version of the firmware, so it is recommended that you update the firmware first thing when assembling your PicoCalc. Directions to do so can be found [here](https://github.com/clockworkpi/PicoCalc/blob/master/wiki/Setting-Up-Arduino-Development-for-PicoCalc-keyboard.md).

## Automated zeptoforth installation from source

The instructions in this section pertain to if you are using the script `utils/build_picocalc.sh` to install zeptoforth on your PicoCalc.

Unlike the instructions in the next section, these instructions assume you are using a Unix-like system and involve building zeptoforth, except for the kernel, from sources. This is more time-consuming, because zeptoforth outside of the kernel is built from the ground up, but at the same time is automated, such that when it is complete you will have a PicoCalc system ready for use without requiring further user interactions

First, you must download the [latest release of zeptoforth](https://github.com/tabemann/zeptoforth/releases) and install a `kernel` build (not a `full` or `full_usb` build ─ we will be building a `full` build on top of this `kernel` build) on your PicoCalc with an RP2040 or RP2350 board installed. Note that for new installations the `rp2040` platform is not recommended as it does not leave much flash dictionary space for user programs.

If you already have a kernel installed and you merely want to reinstall a zeptoforth build with PicoCalc support on top of it, issue at the zeptoforth console:

```
erase-all
```

This will erase the entire flash dictionary of your board except for the kernel. Note that block storage in internal flash will not be touched (unless you select a different platform from your previously installed platform, where then it may be corrupted in the following installation process).

To build zeptoforth for the PicoCalc without zeptoIP, at a shell prompt at the root of the zeptoforth directory tree execute:

```
$ utils/build_picocalc.sh <platform> <port> <font> [<graphical?> [<pico-plus?> [<core>]]]
```

where:

- `<platform>` is one of `rp2040`, `rp2040_big`, `rp2350`, or `rp2350_16mib`, matching the kernel you have installed on your PicoCalc.
- `<port>` is the tty device for the USB-C port on your PicoCalc connected internally to UART0 on the board in your PicoCalc.
- `<font>` is one of `5x8`, `6x8`, or `7x8` for the font size in pixels; if you cannot choose `6x8` is recommended for best results.
- `<graphical?>` is an optional argument that may have values of `graphical` or `text`, to select the graphical PicoCalc terminal emulator or the text-only PicoCalc terminal emulator, respectively; this defaults to `graphical`. The main purpose of this argument is if you wish to save RAM you may want to select the text-only PicoCalc terminal emulator as the graphical PicoCalc terminal emulator uses large quantities of RAM space for its framebuffer; this is particularly a concern on the RP2040 as the graphical PicoCalc terminal emulator leaves little RAM space for user applications on it.
- `<pico-plus?>` is an optional argument that may have values of `not_pico_plus`, if the target board is not a Pimoroni Pico Plus 2 or Pico Plus 2 W, or `pico_plus`, if the target board is a Pimoroni Pico Plus 2 or Pico Plus 2 W; this defaults to `not_pico_plus`. The effect of selecting `pico_plus` is to enable a FAT32 filesystem in PSRAM with the PSRAM Chip Select pin tied to GPIO 47.
- `<core>` is an optional argument that may have values of `core_0`, for the zeptoforth PicoCalc tasks executing on core 0 of the RP2040 or RP2350, or `core_1`, for the zeptoforth PicoCalc tasks executing on core 1 of the RP2040 or RP2350; this defaults to `core_1`. The main purpose of this argument is to enable the zeptoforth PicoCalc tasks to execute on a different core than the zeptoIP frame handler and CYW43xxx driver tasks, if you wish to install zeptoIP on your board.

Once you have done this the following will be installed on your PicoCalc:

- a `full` build for the selected platform
- a graphical or text-only PicoCalc terminal emulator, as selected
- zeptoed, tools for transferring files with your PC, and tools for transferring files between filesystems, unless the platform is `rp2040` where these are omitted to save limited flash dictionary space
- FAT32 filesystem support for the FAT32 filesystems in on-board flash (a.k.a. 'blocks'), on the SDHC card, and if a Pimoroni Pico Plus 2 or Pico Plus 2 W has been selected, in PSRAM
- a screenshot tool as appropriate for the graphical or text-only PicoCalc terminal emulator

When complete, your PicoCalc will reboot and be ready for use!

To build zeptoforth for the PicoCalc _with_ zeptoIP, instead execute:


```
$ utils/build_picocalc_zeptoip.sh <platform> <port> <ipv> <fw> <fw-clm> <font> [<graphical?> [<pico-plus?> [<core>]]]
```

The parameters are the same as for `utils/build_picocalc.sh` except:

- `<ipv>` is either `ipv4` or `ipv6` depending on whether you want to select zeptoIPv4 or zeptoIPv6.
- `<fw>` is the path of the primary CYW43439 firmware.
- `<fw-clm>` is the path of the CLM CYW43439 firmware.

The primary and CLM CYW43439 firmware are non-free so are not in this repository. The primary firmware is mirrored [here](https://github.com/tabemann/cyw43-firmware/raw/master/cyw43439-firmware/43439A0.bin). The CLM firmware is mirrored [here](https://github.com/tabemann/cyw43-firmware/raw/master/cyw43439-firmware/43439A0_clm.bin).

After this completes, everything installed with `utils/build_picocalc.sh` will be installed but the CYW43439 firmware and either zeptoIPv4 or zeptoIPv6 will also be installed.

For more information on zeptoIP consult `BUILDING_AND_USING_ZEPTOIP.md`.

## Manual zeptoforth installation and preparation

First, you must download the [latest release of zeptoforth](https://github.com/tabemann/zeptoforth/releases) and install a `full` build (_not_ a `full_usb` build) on the RP2040 or RP2350 board in your PicoCalc. Note that a `full` build is needed because the PicoCalc redirects UART0 to an USB tty device on its USB-C port, and it is highly recommended that you use the USB-C port on the PicoCalc for extended usage rather than the USB port on the RP2040 or RP2350 board itself as there have been reports of battery overcharging in such extended usage of the board's USB port (some even recommend removing the batteries from your PicoCalc prior to applying power to the board's USB port).

After flashing a `full` build to your RP2040 or RP2350 board, disconnect USB from the RP2040 or RP2350 board itself and connect USB to the USB-C port on your PicoCalc itself because the serial console that the `full` build defaults to will be redirected to your PicoCalc's USB-C port! If you do not do this, your PicoCalc will appear to be unresponsive and the steps discussed later in this document will fail!

## PicoCalc terminal emulator installation

Then, if you are using a shell prompt you should execute the following commands from the root of the zeptoforth directory tree:

    $ TTY=<your tty device> # Replace <your tty device> with your actual TTY device
    $ echo 'compile-to-flash' > prefix.fs
    $ echo 'initializer picocalc-term::term-console' > suffix.fs
    $ echo 'reboot' >> suffix.fs
    $ utils/codeload3.sh -B 115200 -p ${TTY} serial prefix.fs
    $ utils/codeload3.sh -B 115200 -p ${TTY} serial extra/common/st7365p_spi_8_6x8_font_all.fs
    $ utils/codeload3.sh -B 115200 -p ${TTY} serial extra/rp_common/picocalc_bios.fs
    $ utils/codeload3.sh -B 115200 -p ${TTY} serial extra/rp_common/picocalc_sound.fs
    $ utils/codeload3.sh -B 115200 -p ${TTY} serial extra/rp_common/picocalc_term_common.fs
    $ utils/codeload3.sh -B 115200 -p ${TTY} serial extra/rp_common/picocalc_term.fs
    $ utils/codeload3.sh -B 115200 -p ${TTY} serial suffix.fs

If you are using zeptocom.js, you should do the following:

- Connect to the TTY device for the PicoCalc. The connection settings can be left at their defaults for zeptoforth.
- Issue `compile-to-flash`.
- Set the working directory to the root of the zeptoforth directory tree.
- Upload `extra/common/st7365p_spi_8_6x8_font_all.fs`.
- Upload `extra/rp_common/picocalc_bios.fs`.
- Upload `extra/rp_common/picocalc_sound.fs`.
- Upload `extra/rp_common/picocalc_term_common.fs`
- Upload `extra/rp_common/picocalc_term.fs`.
- Issue `initializer picocalc-term::term-console`.
- Issue 'reboot'.

After these steps your PicoCalc will be ready for use!

However, read further before you do the above because below are instructions for things such as installing the text-only terminal emulator, selecting alternate fonts, installing FAT32 support, and installing the text editor zeptoed.

## Text-only terminal emulators

While the above directions provides 8-bit RGB graphics and is relatively fast, it has the downside that a lot of memory is used for the framebuffer, which may prove problematic if you want to use an RP2040 (e.g. Raspberry Pi Pico, Raspberry Pi Pico W) board, and especially if you want to use zeptoIP (as zeptoIP by itself is very memory-costly) or zeptoed (as zeptoed requires a good-sized amount of memory for storage of text in memory).

Luckily, there is a less memory-expensive option available! This is the text-only PicoCalc terminal emulator. This only stores the 8-bit text and its attributes in memory, so it uses far less RAM than the graphical PicoCalc terminal emulator.

To install the text-only PicoCalc terminal emulator, execute the above steps except substitute `extra/rp_common/picocalc_term_text.fs` for `extra/rp_common/picocalc_term.fs` and one of `extra/common/st7365p_spi_text_5x8_font_all.fs`, `extra/common/st7365p_spi_text_6x8_font_all.fs`, or `extra/common/st7365p_spi_text_7x8_font_all.fs` for `extra/common/st7365p_spi_8_6x8_font_all.fs` (see 'Font selection' below).

Note however that the text-only PicoCalc terminal emulator is a bit slower in scrolling than the graphical PicoCalc terminal emulator because it has to redraw each character when updating the screen rather than merely moving the pixel data in th e framebuffer and then converting an 8-bit RGB pixmap to 16-bit RGB and sending it to the screen, which is simpler and thus faster. Currently hardware scrolling is not supported, even though it may be supported in the future.

## Font selection

Note, however, that you may want to select a font other than the default 6x8-pixel font. The other available fonts are 5x8-pixel and 7x8-pixel fonts. A 5x8-pixel font may be desired if you want the terminal emulator display to be 64 characters wide, while a 7x8-pixel font may be desired if you think that the default 6x8-pixel font is too small.

To do so, in the steps above substitute `extra/common/st7365p_spi_8_5x8_font_all.fs` or `extra/common/st7365p_spi_8_7x8_font_all.fs`, or if you are installing a text-only terminal emulator, `extra/common/st7365p_spi_text_5x8_font_all.fs` or `extra/common/st7365p_spi_text_7x8_font_all.fs`. Note that if more than one of these fonts are available at a time the smallest font will be used.

## Core selection

The core that the PicoCalc tasks run on defaults to core 1 of the RP2040 or RP2350, but can be manually set by defining a globally-visible constant in flash named `select-picocalc-tasks-core` prior to compiling PicoCalc support. This is useful if you are also installing zeptoIP on your PicoCalc, as zeptoIP runs very CPU-heavy tasks on core 1 which will significantly slow down the PicoCalc terminal emulator if it is also running on core 1.

## Installing FAT32 support

There is optional support for FAT32 filesystems in on-board flash (using block storage as a backend), on your SD card, and, if you are using a Pimoroni Pico Plus 2 or Pico Plus 2 W, as a RAM disk in PSRAM.

To enable support for FAT32 filesystems in on-board flash, with the serial console enabled, execute the following at the shell prompt at the base of the zeptoforth directory tree:

    $ utils/codeload3.sh -B 115200 -p <your tty device> serial extra/common/setup_blocks_fat32.fs

or with zeptocom.js upload `extra/common/setup_blocks_fat32.fs`.

There is no need to specify compiling to flash, and actually initially the board should be in its default of compiling to RAM.

This will automatically reboot your PicoCalc; note that if you are using `utils/codeload3.sh` an error about not finding the TTY device may appear at the end ─ please ignore this error.

Note that if this is the first time you are setting up a FAT32 filesystem in block storage and you are using a Pimoroni Pico Plus 2 or Pico Plus 2 W with an `rp2350_16mib` build I highly suggest using zeptocom.js to do so, to avoid timeout issues with `utils/codeload3.sh` (as zeptocom.js does not use a timeout). This is because the first time you do this the block storage is first erased and then a FAT32 filesystem is initialized in it, which takes a good bit of time due to the 14 MiB size of the filesystem.

Afterwards, if you are using a Pimoroni Pico Plus 2 or Pico Plus 2 W, it is highly recommended that you set up support for a FAT32 filesystem in PSRAM as a RAM disk. This is accomplished by, with the serial console enabled, executing the following at the shell prompt at the base of the zeptoforth directory tree:

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

## Taking screenshots

Screenshots can be taken by first loading `extra/rp_common/picocalc_screenshot.fs` for the graphical PicoCalc terminal emulator or `extra/rp_common/picocalc_screenshot_text.fs` for the text-only PicoCalc terminal emulator with the following at a shell prompt in the root of the zeptoforth source tree:

    $ utils/codeload3.sh -B 115200 -p <your tty device> serial extra/rp_common/picocalc_screenshot.fs

or:

    $ utils/codeload3.sh -B 115200 -p <your tty device> serial extra/rp_common/picocalc_screenshot_text.fs

or otherwise upload them with zeptocom.js.

If you want these to be loaded permanently execute the following at a shell prompt in the root of the zeptoforth source tree:

    $ TTY=<your tty device>
    $ echo 'compile-to-flash' > prefix.fs
    $ echo 'reboot' > suffix.fs
    $ utils/codeload3.sh -B 115200 -p ${TTY} serial prefix.fs
    $ utils/codeload3.sh -B 115200 -p ${TTY} serial extra/rp_common/picocalc_screenshot.fs
    $ utils/codeload3.sh -B 115200 -p ${TTY} serial suffix.fs

or:

    $ TTY=<your tty device>
    $ echo 'compile-to-flash' > prefix.fs
    $ echo 'reboot' > suffix.fs
    $ utils/codeload3.sh -B 115200 -p ${TTY} serial prefix.fs
    $ utils/codeload3.sh -B 115200 -p ${TTY} serial extra/rp_common/picocalc_screenshot_text.fs
    $ utils/codeload3.sh -B 115200 -p ${TTY} serial suffix.fs

If using zeptocom.js, precede uploading the source file with `compile-to-flash` and follow it with `reboot.

In all these cases you must have zeptoforth REPL on the serial console, which can be reached with executing `serial::serial-console` at the REPL; once you are done return the PicoCalc terminal emulator, if desired, with `picocalc-term::term-console`.

Do this after loading `extra/rp_common/picocalc_fat32.fs` for best results, as if this is done it will default to using the SD card FAT32 filesystem first, if configured, or the PSRAM FAT32 filesystem second, if configured, or the on-board flash storage FAT32 filesystem third, if configured.

If this is not done it will require the user to specify a FAT32 filesystem with `picocalc-screenshot::screenshot-fs!` ( fs -- ) before being able to take screenshots.

The path for the directory in which screenshots will be `/SCREEN` by default. It can be changed with `picocalc-screenshot::screenshot-path!` ( path-addr path-bytes -- ).

Once all this is done, you will be able to take screenshots by entering Attention (on the PicoCalc terminal emulator Break (Shift-Escape), on the serial console Control-T) followed by 's'. The fact that you have done this successfully will be signified by flashing the display prior to actually taking the screenshot. There will be a short moment where the PicoCalc terminal emulator will not be responsive afterwards while it actually saves the screenshot to file.

The screenshots will be saved in the selected directory, which will be created (but not its parent directory, which must exist), as an 8-bit RLE BMP file with afilename in the format `SCRxxxxx.BMP`, where `xxxxx` is a 5-digit decimal number starting at `00000` and counting upwards.

Alternatively, screenshots can be taken with `picocalc-screenshot::take-screenshot` ( path-addr path-bytes fs -- ). This is the actual word that implements the taking of screenshots. Note that when this word is executed directly the display is _not_ flashed.

## Bells!

zeptoforth by default generates BEL characters by default when `abort` gets called, e.g. by an uncaught exception in the main task. The word which generates these BEL's is `bel`.

This behavior has a master control in `bel-enabled`, which defaults to `true`. It can be disabled by executing:

    false bel-enabled !

You can permanently disable the generation of BEL characters by `bel`, and thus `abort`, by executing:

    compile-to-flash
    : init-disable-bel false bel-enabled ! ;
    initializer init-disable-bel
    reboot

By default the PicoCalc terminal emulator ignores these BEL characters. However, _visual bells_ and _audible_bells_ can be enabled so these BEL characters can alert you.

Visual bells can be enabled by executing:

    true picocalc-term::visual-bell-enabled!

These visual bells consist of momentarily inverting the screen when a BEL character is output on the PicoCalc terminal emulator console. This may be annoying to some, hence why it is disabled by default.

You can permanently enable visual bells through executing:

    compile-to-flash
    : init-enable-visual-bell true picocalc-term::visual-bell-enabled! ;
    initializer init-enable-visual-bell
    reboot

Audible bells can similarly be enabled by executing:

    true picocalc-term::audible-bell-enabled!

These audible bells consist of momentarily generating a beep from the PicoCalc's speakers when a BEL character is output on the PicoCalc terminal emulator console. This may also be annoying to some, hence why it is disabled by default.

You can permanently enable audible bells through executing:

    compile-to-flash
    : init-enable-audible-bell true picocalc-term::audible-bell-enabled! ;
    initializer init-enable-audible-bell
    reboot

## Managing the BIOS

zeptoforth on the PicoCalc has the capability to interface with the STM32 microcontroller which manages the battery, display backlight, and keyboard backlight of the PicoCalc, also known as the BIOS.

For this purpose there are the following words in the `picocalc-term` module, for both the graphical and the text-only PicoCalc terminal emulators:

### `read-battery`
( -- battery-level )

Read the battery level.

### `set-backlight`
( desired-backlight-level -- actual-backlight-level )

Set the backlight level.

### `read-backlight`
( -- backlight-level )

Read the backlight level.

### `set-kbd-backlight`
( desired-kbd-backlight-level -- actual-kbd-backlight-level )

Set the keyboard backlight level.

### `read-kbd-backlight`
( -- kbd-backlight-level )

Read the keyboard backlight level.

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

One important note, though, is if you are using the graphical terminal emulator on an RP2040 (e.g. a Raspberry Pi Pico) there will not be enough RAM available for the default heap size for zeptoed, so starting zeptoed will result in a crash. As a result, adjust the heap size prior to starting zeptoed with:

    <desired heap size> to zeptoed-heap-size

To figure out a heap size to use execute:

    unused

which will output, amongst other things, the amount of RAM available in the RAM dictionary for the main task. Use this amount of RAM minus about 10 KiB as a rule of thumb for the maximum heap size supported.

Note that you can adjust `zeptoed-heap-size` multiple times in a session as you load more code and data into RAM. Also note that it does not by default persist across boots; to make it persist execute:

    compile-to-flash
    : init-zeptoed-heap-size <desired heap size> to zeptoed-heap-size ;
    initializer init-zeptoed-heap-size
    reboot

## Emulating a PicoCalc

For those such as myself who do not yet possess a PicoCalc, some measures may be taken in its place. The PicoCalc terminal emulator can support an SPI ST7789V display in the place of the SPI ST7365P display used by the PicoCalc. When loading the code into flash substitute `extra/common/st7365p_spi_8_5x8_font_all.fs`, `extra/common/st7365p_spi_8_6x8_font_all.fs`, or `extra/common/st7365p_spi_8_7x8_font_all.fs` with `extra/common/st7789v_spi_8_5x8_font_all.fs`, `extra/common/st7789v_spi_8_6x8_font_all.fs`, or `extra/common/st7789v_spi_8_7x8_font_all.fs`. Also, prior to loading edit `extra/rp_common/picocalc_term.fs` to set the GPIO selections for your particular setup. Also, omit sending `initializer picocalc-term::term-console` to the board in the PicoCalc.

Then, after the PicoCalc has rebooted, execute the following at the zeptoforth REPL via the serial console as a single line of code:

    true to picocalc-bios::emulate-keys? picocalc-term::term-console

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
        st7365p-8-common import
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

Afterwards, it calls `update-display` to copy the updated portion of the framebuffer to the display. Note that `update-display` is specific to the class `st7365p-8-common::<st7365p-8-common>` or `st7789v-8-common::<st7789v-8-common>` and does not belong to a shared superclass, so you will have to specifically import `st7365p-8-common` or `st7789v-8-common` depending on whether an actual PicoCalc is in use or not, respectively.

Note that `picocalc-term::with-term-display` locks the terminal emulator within the xt that it calls, so do not execute any code that may output text with `emit`, `type`, `.`, `."`, `.s`, `h.8`, or like within this xt as it may block forever because the output stream used by the terminal emulator is not emptied while it is locked and thus may become full.

## Playing sound on the PicoCalc

Currently only generating fixed tones for durations is supported by the PicoCalc. These can be generated through calling `picocalc-sound::play-tone-for-duration` ( duration-in-ticks D: pitch-in-hz -- ). The pitch is a S31.32 fixed-point value in Hz, which can be specified via the `x,y` notation, e.g. `1047,0` is C6.

Note that this word is _not_ blocking; it will return immediately even though the tone will play for the set duration in ticks (which are 100 microsecond increments).

Multiple layered tones are supported, up to 8 tones at once. Note that the tone that will expire the soonest is the one which will play at any given moment.

For convenience's sake, a `picocalc-sound::beep` word is provided, which generates a 1245 Hz, i.e. D6#, tone for 125 milliseconds. This word is used internally to implement the audible bell.
