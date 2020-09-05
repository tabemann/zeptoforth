# zeptoforth

zeptoforth is a Cortex-M Forth, currently targeted at the STM32L476 and STM32F407 DISCOVERY boards even though the intention is to target more boards and MCUs soon (but do not expect Cortex-M0 MCUs to be supported any time soon, due to their using Thumb-1 rather than Thumb-2).

Its kernel is written in Thumb-2 assembly, and a body of other core code that is loaded after it is loaded is written in Forth.

To build the kernel, one first needs to install the gas and binutils arm-none-eabi toolchain, and then execute (for STM32L476):

    $ make

or:

    $ make PLATFORM=stm32l476

This will build a zeptoforth.bin and a zeptoforth.elf file; the zeptoforth.elf file is of use if one wishes to do source debugging with gdb of the zeptoforth kernel, otherwise disregard it. The same workflow is to be followed if one is assembling and linking zeptoforth for the STM32F407.

To load the zeptoforth image (whether just the kernel or an image including precompiled Forth code) onto an STM32L476 or STM32F407 DISCOVERY board, first install st-flash, then attach the DISCOVERY board to one's PC via USB and execute:

    $ st-flash erase
    $ st-flash write <location of the zeptoforth image> 0x08000000
    $ st-flash reset

<Location of the zeptoforth image> is either a freshly built zeptoforth.bin file in the root directory of zeptoforth, or a prebuilt binary named `zeptoforth_kernel-<version>.bin` (without precompiled Forth code), `zeptoforth_full-<version>.bin` (with precompiled Forth code), or `zeptoforth_swdcom-<version>.bin` (with precompiled Forth code including a swdcom driver) in `bin/<version>/<platform>/`. Note that only the STM32L476 and STM32F407 DISCOVERY boards currently have prebuilt images including precompiled Forth code, since they require working setups with the boards in question to be created.

Note the address referred to above. This will also reboot the board.

To use the board on Linux, download and install e4thcom or swdcom (at http://github.com/crest/swdcom).

The following applies if one is using e4thcom: If one is using an STM32F407 DISCOVERY board, attach a USB-to-serial converter to your machine (make sure you have the proper permissions to access its device file) and attach the RXD pin on the converter to PA2 on the board and the TXD pin on the converter to PA3 on the board with jumper cables. Then, from the zeptoforth base directory execute:

    $ e4thcom -t noforth -b B115200 -d <device, typically one of ttyACM0 or ttyUSB0>

Once e4thcom comes up, execute (including the leading '#'), for the STM32L476:

    #include src/stm32l476/forth/setup.fs

or, for the STM32F407:

    #include src/stm32f407/forth/setup.fs

This will load the auxiliary Forth routines that would be useful to have onto the MCU. This code is that is included in the `zeptoforth_full-<version>.bin` images along with the kernel itself. The last thing that is included is a "cornerstone" named `restore-state` which, when executed, as follows:

    restore-state

erases everyting compiled to Flash afterwards and then does a warm restart.

To do a warm restart by itself (which now does a full reset of the hardware, unlike previously where it only reset the zeptoforth software), execute the following:

    reboot

Note that e4thcom is Linux-specific. Another terminal emulator to use with zeptoforth is screen. Note that zeptoforth uses ACK and NAK for flow control, with ACK indicating readiness to accept a new line of input, and NAK indicating an error; these are not (to my knowledge) are ignored by screen. As a result, one would  have to use `slowpaste 5` with screen to set a proper paste speed. (This is far slower than the ACK/NAK method used with e4thcom.) Additionally, as screen does not honor directives to load files automatically, one will need to use `readbuf <path>` and `paste <path>` to paste files into the terminal manually.

A better approach than using `slowpaste`, `readbuf`, and `paste` with screen is to use `codeload3.py`, which is in the `utils` directory and which honors the e4thcom directives, so it can be used with the included `setup.fs` files without modification. It is invoked as follows:

    $ ./utils/codeload3.py [-p <device>] -B 115200 serial <Forth source file>

It has significantly better performance and functionality than screen with `slowpaste` and is the recommended method of code uploading if e4thcom is not available. Note that it requires Python 3 and pySerial, and it must be given executable permissions before it may be executed.

If one is using swdcom (assuming one has already built it and installed `swd2` in some suitable location such as `/usr/local/bin` and that one has already written the `zeptoforth_swdcom-<version>.bin` binary to the board), simply execute `swd2`. This will provide a terminal session with zeptoforth. To upload Forth code to execute to the board, execute in the directory from which `swd2` was executed:

    cat <path> > upload.fs && pkill -QUIT swd2

This will simply upload the given file to the board as-is without any support for `#include` or `#require`, unlike e4thcom.