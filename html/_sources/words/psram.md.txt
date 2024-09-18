# PSRAM words

The `rp2350` platform optionally supports Quad SPI pseudo-static RAM (PSRAM for short) tied to a GPIO selected as QMI CS1n. PSRAM when enabled is transparent to the user and may be arbitrarily accessed as a block of RAM. PSRAM starts at `psram-base` and is `psram-size` bytes in size once enabled.

Note that PSRAM is slower than SRAM, being external to the RP2350 and tied to the RP2350 through a Quad SPI interface, so one should use SRAM for things which are performance-critical, especially since it competes with Quad SPI flash for XIP cache space.

Only certain GPIO's may be used for the QMI CS1n pin. These are GPIO's 0, 8, 19, and 47. Note that many boards with RP2350B chips such as the Pimoroni Pico Plus 2 use GPIO 47 for this purpose.

### `forth`

The following words in this module pertain to PSRAM usage:

##### `init-psram`
( psram-cs-pin -- )

Initialize PSRAM with QMI CS1n set to GPIO *psram-cs-pin*, which must be one of 0, 8, 19, or 47 or otherwise `x-invalid-qmi-cs1n-pin` will be raised. If no PSRAM is available tied to the selected GPIO, `x-no-psram` will be raised.

##### `psram-size`
( -- bytes )

Get the number of bytes of PSRAM. This is initialized on boot to 0, and is set by `init-psram`.

##### `psram-base`
( -- addr )

Get the base address of PSRAM.

##### `x-invalid-qmi-cs1n-pin`
( -- )

The exception raised if `init-psram` is called with an invalid QMI CS1n GPIO index.

##### `x-no-psram`
( -- )

The exception raised if no PSRAM is available on the selected QMI CS1n GPIO.
