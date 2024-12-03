# Pico clock control words

These words allow the RP2040/RP2350 system clock (normally 125 MHz on the RP2040, 150 MHz on the RP2350) to be changed.  This is sometimes desirable, for example if it is necessary to run PIO operations at an integer multiple of 10 MHz on an RP2040.  In that case, the system clock can be changed to 120 MHz.  Other values are allowed, subject to the limitations outlined in the PLL chapter of the RP2040 and RP2350 data sheets.

### `forth`

The following words are built into the kernel:

##### `xosc-frequency`
( -- u )

Returns the crystal oscillator frequency in Hz.  This is set at built time; currently only 12 MHz is supported.

##### `sysclk`
( -- addr )

Get the address of the system clock frequency in Hz.  This is initialized to 125 MHz on the RP2040, or 150 MHz on the RP2350, and will be changed to the new value by `set-sysclk`.

### `clocks`

The following words are in `clocks`:

##### `set-sysclk`
( refdiv fbdiv pdiv1 pdiv2 -- )

This changes the RP2040 system clock ("SYSCLK") using the PLL parameters given.  The resulting system clock frequency is `xosc-frequency refdiv / fbdiv * pdiv1 / pdiv2 /`.  Refer to the RP2040 data sheet for the rules on which values are allowed and what restrictions apply on the internally generated frequencies that result.

Since SYSCLK is the clock source for the UART, I2C, and SPI subsystems, if this word is used to change the SYSCLK frequency, those interfaces will run at a different rate than before.  The appropriate rate setting words (`uart-baud!`, `i2c-clock!`, or `spi-baud!`) should be called after any call to `set-sysclk` to reload the devices with the divider values needed for the updated system clock frequency.

##### `x-bad-refdiv`
( -- )

Exception for reference divider value out of range.

##### `x-bad-fbdiv`
( -- )

Exception for VCO feedback divider value out of range.

##### `x-bad-postdiv1`
( -- )

Exception for post divider 1 value out of range.

##### `x-bad-postdiv2`
( -- )

Exception for post divider 2 value out of range.

##### `x-bad-refclk`
( -- )

Exception for VCO refclock frequency out of range.

##### `x-bad-vcofreq`
( -- )

Exception for VCO frequency out of range.

##### `x-bad-sysclk`
( -- )

Exception for sysclk frequency out of range.

