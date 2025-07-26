# PicoCalc words

zeptoforth has support for the PicoCalc, a device that provides a display, keyboard, speakers, and an SD card slot for boards that fit the pinout of a Raspberry Pi Pico 1 (including a Raspberry Pi Pico 1 W, Raspberry Pi Pico 2, Raspberry Pi Pico 2 W, Pimoroni Pico Plus 2, and Pimoroni Pico Plus 2 W). General directions as to the installation and use of zeptoforth on the PicoCalc are detailed in `USING_THE_PICOCALC.md` at the root of this directory tree. This file contains API documentation for the API exposed by the graphical and text-only PicoCalc terminal emulators.

Note that the core that the PicoCalc tasks run on defaults to core 1 of the RP2040 or RP2350, but can be manually set by defining a globally-visible constant in flash named `select-picocalc-tasks-core` prior to compiling PicoCalc support. This is useful if you are also installing zeptoIP on your PicoCalc, as zeptoIP runs very CPU-heavy tasks on core 1 which will significantly slow down the PicoCalc terminal emulator if it is also running on core 1.

### `picocalc-term`

The `picocalc-term` module contains the following words:

##### `with-term-lock`
( xt -- )

Carry out an operation with the PicoCalc terminal locked. Note that executing operations that print to the PicoCalc terminal should be avoided because they may block indefinitely.

##### `with-term-display`
( xt -- ) xt: ( display -- )

Carry out an operation against the PicoCalc terminal's display with the PicoCalc terminal locked. It is highly recommended that the user update the terminal's display or graphics drawn to it may not be displayed. Note that executing operations that print to the PicoCalc terminal should be avoided because they may block indefinitely.

##### `term-pixels-dim@`
( -- width height )

Get the terminal dimensions in pixels.

##### `term-dim@`
( -- width height )

Get the terminal dimensions in characters.

##### `term-char-dim@`
( -- width height )

Get the terminal character dimensions in pixels.

##### `term-font@`
( -- font )

Get the terminal font.

##### `read-battery`
( -- battery-level )

Read the battery level.

##### `set-backlight`
( desired-backlight-level -- actual-backlight-level )

Set the backlight level.

##### `read-backlight`
( -- backlight-level )

Read the backlight level.

##### `set-kbd-backlight`
( desired-kbd-backlight-level -- actual-kbd-backlight-level )

Set the keyboard backlight level.

##### `read-kbd-backlight`
( -- kbd-backlight-level )

Read the keyboard backlight level.

##### `visual-bell-enabled!`
( enabled -- )

Set visual bell enabled. This defaults to false.

##### `visual-bell-enabled@`
( -- enabled )

Get visual bell enabled. This defaults to false.

##### `audible-bell-enabled!`
( enabled -- )

Set audible bell enabled. This defaults to false.

##### `audible-bell-enabled@`
( -- enabled )

Get audible bell enabled. This defaults to false.

##### `screenshot-hook!`
( xt -- )

Set the screenshot hook. This defaults to 0, indicating no hook set.

##### `screenshot-hook@`
( -- xt )

Get the screenshot hook. This defaults to 0, indicating no hook set.

### `picocalc-sound`

The `picocalc-sound` module contains the following words:

##### `play-tone`
( end-tick D: pitch -- )

Generate a tone of *pitch* (an S31.32 value in Hz) ending at *end-tick* relative to `systick::systick-counter`, with ticks being 100 microsecond increments. Note that tones ending earlier override tones ending later, and a total of up to 8 tones are tracked at any one time.

##### `play-tone-for-duration`
( duration D: pitch -- )

Generate a tone of *pitch* (an S31.32 value in Hz) of *duration* ticks, with ticks being 100 microsecond increments. Note that tones ending earlier override tones ending later, and a total of up to 8 tones are tracked at any one time.

##### `beep`
( -- )

This is a convenience word to generate a 1245 Hz, i.e. D6#, tone for 125 milliseconds. This word is used internally to generate the audible bell.

### `picocalc-bios`

The `picocalc-bios` module contains the following words:

##### `emulate-keys?`
( -- flag )

This is a `value` that defaults to `false` that can be set to `true` to enable emulating key presses using the serial console as input. This is primarily of use for testing PicoCalc support without using an actual PicoCalc.

### `picocalc-screenshot`

Compiling `extra/rp_common/picocalc_screenshoto.fs` makes the following words available in the `picocalc-screenshot` module:

##### `screenshot-fs!`
( fs -- )

Set the FAT32 filesystem object for saving screenshots with Attention (Break (Shift-Escape) on the PicoCalc keyboard, Control-T on the serial and USB CDC consoles) `s`. This filesystem defaults to the SD card FAT32 filesystem first, if configured, or the PSRAM FAT32 filesystem second, if configured, or the on-board flash storage FAT32 filesystem third, if configured.

##### `screenshot-fs@`
( -- fs )

Get the FAT32 filesystem object for saving screenshots with the attention key combo.

##### `screenshot-path!`
( path-addr path-bytes -- )

Set the base path for saving screenshots with Attention (Break (Shift-Escape) on the PicoCalc keyboard, Control-T on the serial and USB CDC consoles) `s`. This path defaults to `/SCREEN`. Note that this directory will be automatically created if it does not exist when taking a screenshot with the console attention key combo, but its parent directorie(s) will not be automatically created.

##### `screenshot-path@`
( -- path-addr path-bytes )

Get the base path for saving screenshots with the attention key combo.

##### `take-screenshot`
( path-addr path-bytes fs -- )

This is the actual word that implements the taking of screenshots. Note that when this word is executed directly the display is _not_ flashed unlike when taking a screenshot with the console attention key combo.

### `fat32-tools`

Compiling `extra/rp_common/picocalc_fat32.fs` adds the following words to the `fat32-tools` module:

##### `blocks-fs@`
( -- fs )

This returns the FAT32 filesystem object for the on-board flash 'blocks' FAT32 filesystem if `extra/common/setup_blocks_fat32.fs` has been compiled before `extra/rp_common/picocalc_fat32.fs`.

##### `blocks-fs:`
( -- )

This sets the current FAT32 filesystem to the FAT32 filesystem object for the on-board flash 'blocks' FAT32 filesystem if `extra/common/setup_blocks_fat32.fs` has been compiled before `extra/rp_common/picocalc_fat32.fs`.

##### `sd-fs@`
( -- fs )

This returns the FAT32 filesystem object for the FAT32 filesystem in partition 0 of the SDHC or SDXC card inserted in your PicoCalc unless the constant `no-sd-fs` was defined in flash prior to compiling `extra/rp_common/picocalc_fat32.fs`.

##### `sd-fs:`
( -- )

This sets the current FAT32 filesystem to the FAT32 filesystem object for the FAT32 filesystem in partition 0 of the SDHC or SDXC card inserted in your PicoCalc unless the constant `no-sd-fs` was defined in flash prior to compiling `extra/rp_common/picocalc_fat32.fs`.

##### `psram-fs@`
( -- fs )

This returns the FAT32 filesystem object for the FAT32 filesystem in PSRAM if `extra/rp2350/setup_pico_plus_2_psram_fat32.fs` was compiled before compiling `extra/rp_common/picocalc_fat32.fs`.

##### `psram-fs:
( -- )

This sets the current FAT32 filesystem to the FAT32 filesystem object for the FAT32 filesystem in PSRAM if `extra/rp2350/setup_pico_plus_2_psram_fat32.fs` was compiled before compiling `extra/rp_common/picocalc_fat32.fs`.
