# zeptoforth

zeptoforth is a Cortex-M Forth, currently targeted at the STM32L476 DISCOVERY board even though the intention is to target more boards and MCUs soon (but do not expect Cortex-M0 MCUs to be supported any time soon, due to their using Thumb-1 rather than Thumb-2).

Its kernel is written in Thumb-2 assembly, and a body of other core code that is loaded after it is loaded is written in Forth.

To build the kernel, one first needs to install the gas and binutils arm-none-eabi toolchain, and then execute (for STM32L476):

    $ make

or:

    $ make PLATFORM=stm32l476

This will build a zeptoforth.bin and a zeptoforth.elf file; the zeptoforth.elf file is of use if one wishes to do source debugging with gdb of the zeptoforth kernel, otherwise disregard it.

To load the zeptoforth kernel onto an STM32L476 DISCOVERY board, first install st-flash, then attach the DISCOVERY board to one's PC via USB and execute:

    $ st-flash erase
    $ st-flash write zeptoforth.bin 08000000

Note the address referred to above. This will also reboot the board.

To use the board on Linux, download and install e4thcom, then, from the zeptoforth base directory execute:

    $ e4thcom -t noforth -b B115200

Once e4thcom comes up, execute (including the leading '#'):

    #include src/stm32l476/forth/setup.fs

This will load the auxiliary Forth routines that would be useful to have onto the MCU, along with a "cornerstone" named `restore-state` to return zeptoforth to this state.

Note that e4thcom is Linux-specific. Another terminal emulator to use with zeptoforth is screen. Note that zeptoforth uses ACK and NAK for flow control, with ACK indicating readiness to accept a new line of input, and NAK indicating an error; these are not (to my knowledge) are ignored by screen. As a result, one will  have to use `slowpaste 5` with screen to set a proper paste speed. (This is far slower than the ACK/NAK method used with e4thcom.) Additionally, as screen does not honor directives to load files automatically, one will need to use `readbuf <path>` and `paste <path>` to paste files into the terminal manually.