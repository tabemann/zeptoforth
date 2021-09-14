# zeptoforth

zeptoforth is a Cortex-M Forth, currently targeted at the STM32L476, STM32F407, and STM32F746 DISCOVERY boards even though the intention is to target more boards and MCUs soon (but do not expect Cortex-M0 MCUs to be supported any time soon, due to their using Thumb-1 rather than Thumb-2).

Its kernel is written in Thumb-2 assembly, and a body of other core code that is loaded after it is loaded is written in Forth.

To load the zeptoforth image (whether just the kernel or an image including precompiled Forth code) onto an STM32L476, STM32F407, or STM32F746 DISCOVERY board, first install st-flash, then attach the DISCOVERY board to one's PC via USB and execute:

    $ st-flash erase
    $ st-flash write <location of the zeptoforth image> 0x08000000
    $ st-flash reset

Prebuilt binaries are in `bin/<version>/<platform>/`.

\<Location of the zeptoforth image> is either:

* a freshly built zeptoforth.<platform>.bin file in the root directory of zeptoforth
* `zeptoforth_kernel-<version>.bin` (without precompiled Forth code)
* `zeptoforth_<type>-<version>.bin` (with full precompiled Forth code)

where \<type> is one of:

* `full` (full functionality compiled in except for swdcom support with a cornerstone to enable resetting functionality back to "factory" settings)
* `full_swdcom` (full functionality compiled in including swdcom support with a cornerstone to enable resetting functionality back to "factory" settings)
* `mini` (i.e. without fixed number, allocator, scheduler, or disassembler support, without swdcom support)
* `mini_swdcom` (i.e. without fixed number, allocator, scheduler, or disassembler support, including swdcom support)

and where \<platform> is one of

* `stm32f407`
* `stm32f746`
* `stm32l476`
* `rp2040`

Note that for the `rp2040` platform, to load code with the bootloader onto the Raspberry Pi Pico one needs a `.uf2` file rather than a `.bin` file, which will be located in the same location. Note that these files contain a boot block with a CRC32 checksum.

To build the kernel for each of the supported platforms, one first needs to install the gas and binutils arm-none-eabi toolchain along with Python 3.9, and then execute:

    $ make

to use the default version:

    $ make VERSION=<version>

This build a `zeptoforth.<platform>.bin`, a `zeptoforth.<platform>.ihex`, and a `zeptoforth.<platform>.elf` file for each supported platform. Additionally a zeptoforth.rp2040.uf2 file will be built for the rp2040 platform. The zeptoforth.<platform>.elf file is of use if one wishes to do source debugging with gdb of the zeptoforth kernel, otherwise disregard it. The same workflow is to be followed if one is assembling and linking zeptoforth for the STM32F407.

Note the address referred to above. This will also reboot the board.

To use the board on Linux, download and install e4thcom (at https://wiki.forth-ev.de/doku.php/en:projects:e4thcom), swdcom (at http://github.com/crest/swdcom), GNU Screen (at https://www.gnu.org/software/screen/), or picocom (at https://github.com/npat-efault/picocom).

The following applies if one is using e4thcom: If one is using an STM32F407 DISCOVERY board, attach a USB-to-serial converter to your machine (make sure you have the proper permissions to access its device file) and attach the RXD pin on the converter to PA2 on the board and the TXD pin on the converter to PA3 on the board with jumper cables. Then, from the zeptoforth base directory execute:

    $ e4thcom -t noforth -b B115200 -d <device, typically one of ttyACM0 or ttyUSB0>

Once e4thcom comes up, execute (including the leading '#'), for the STM32L476:

    #include src/stm32l476/forth/setup_<type>.fs

or, for the STM32F407:

    #include src/stm32f407/forth/setup_<type>.fs

or, for the STM32F746:

    #include src/stm32f746/forth/setup_<type>.fs

where \<type> is one of the types given above, with the meanings given above.

This will load the auxiliary Forth routines that would be useful to have onto the MCU. This code is that is included in the `zeptoforth_<type>-<version>.bin` images along with the kernel itself. The last thing that is included for full builds is a "cornerstone" named `restore-state` which, when executed, as follows:

    restore-state

erases everyting compiled to Flash afterwards and then does a restart.

To do a restart by itself (which now does a full reset of the hardware), execute the following:

    reboot

Note that e4thcom is Linux-specific. Another terminal emulator to use with zeptoforth is GNU Screen. One must configure it to use 115200 baud, 8 data bits, 1 stop bit, and currently there is no support for flow control with GNU Screen. Note that zeptoforth uses ACK and NAK for flow control, with ACK indicating readiness to accept a new line of input, and NAK indicating an error; these are not supported by GNU Screen. As a result, one would  have to use `slowpaste 5` with screen to set a proper paste speed. (This is far slower than the ACK/NAK method used with e4thcom.) Additionally, as screen does not honor directives to load files automatically, one will need to use `readbuf <path>` and `paste <path>` to paste files into the terminal manually.

A better approach than using `slowpaste`, `readbuf`, and `paste` with screen is to use `codeload3.py`, which is in the `utils` directory and which honors the e4thcom directives, so it can be used with the included `setup.fs` files without modification. It is invoked as follows:

    $ ./utils/codeload3.py [-p <device>] -B 115200 serial <Forth source file>

It has significantly better performance and functionality than screen with `slowpaste` and is the recommended method of code uploading if e4thcom is not available. Note that it requires Python 3 and pySerial, and it must be given executable permissions before it may be executed.

Another terminal emulator one may use is picocom, which has many of the same considerations here as GNU Screen. For this reason it is not recommended for mass code uploads, for which `codeload3.py` is a better choice, and rather is limited in practice to interactive usage.

If one is using swdcom (assuming one has already built it and installed `swd2` in some suitable location such as `/usr/local/bin` and that one has already written the `zeptoforth_swdcom-<version>.bin` binary to the board), simply execute `swd2`. This will provide a terminal session with zeptoforth. To upload Forth code to execute to the board, execute in the directory from which `swd2` was executed:

    cat <path> > upload.fs && pkill -QUIT swd2

This will simply upload the given file to the board as-is without any support for `#include` or `#require`, unlike e4thcom.
