# UART Words

There are a number of words for controlling UART's available on each of the supported microcontrollers on top of the console `emit`, `key`, `emit?`, and `key?` words. These are as follows:

Note that *uart* values as mentioned below vary between the different supported platforms. The following applies:

* On the RP2040 and RP2350 the available *uart* values are 0 and 1, with 0 corresponding to the console.
* On the STM32F411 the available *uart* values are 1, 2, and 6, with 2 corresponding to the console.
* On the STM32F407 the available *uart* values are 1 through 6, with 2 corresponding to the console.
* On the STM32L476 the available *uart* values are 1 through 5, with 2 corresponding to the console. Note that LPUART1 is not currently supported.
* On the STM32F746 the available *uart* values are 1 through 8, with 1 corresponding to the console.

Note that the RP2350 supports optional CTS and RTS flow control.

### `uart`

These words are in the `uart` module:

##### `uart-enabled?`
( uart -- flag )

Get whether a UART is enabled.

##### `enable-uart`
( uart -- )

Enable a UART.

##### `disable-uart`
( uart -- )

Disable a UART.

##### `with-uart-disabled`
( xt uart -- )

Execute *xt* with a UART disabled, re-enabling it afterwards if it was previously enabled.

##### `uart-baud!`
( baud uart -- )

Set the baud of a UART.

##### `>uart`
( c uart -- )

Emit a byte to a UART.

##### `uart>`
( uart -- c )

Read a byte from a UART.

##### `>uart?`
( uart -- flag )

Get whether a UART is ready to emit a byte.

##### `uart>?`
( uart -- flag )

Get whether a byte is ready to be read from a UART.

##### `flush-uart`
( uart -- )

Flush a UART's transmit buffer. This flushes both the in-RAM transmit buffer and the UART's transmit FIFO. Note that this does not flush data in any bitwise shift register, so to truly guarantee every bit has been transmitted a delay of (1 / baud rate) * 10 (for 8 data bits, 1 start bit, and 1 stop bit) after executing this is necessary.

##### `uart-alternate`
( uart -- )

The alternate function for a given UART (except for `rp2350`).

##### `uart-non-cts/rts--alternate`
( pin uart -- )

The alternate function for a non-CTS/RTS pin for a given UART (`rp2350` only).

##### `uart-cts/rts-alternate`
( pin uart -- )

The alternate function for a CTS/RTS pin for a given UART (`rp2350` only).

##### `uart-pin`
( uart pin -- )

Set the alternate function for *pin* to configure it for *uart*.

##### `uart-cts/rts-pin`
( uart pin -- )

Set the alternate function for *pin* to configure it as a CTS/RTS pin for *uart* (`rp2350` only).

##### `uart-cts-enabled?`
( uart -- flag )

Get whether CTS is enabled on a UART (`rp2350` only).

##### `enable-uart-cts`
( uart -- )

Enable CTS on a UART (`rp2350` only).

##### `disable-uart-cts`
( uart -- )

Disable CTS on a UART (`rp2350` only).

##### `uart-rts-enabled?`
( uart -- flag )

Get whether RTS is enabled on a UART (`rp2350` only).

##### `enable-uart-rts`
( uart -- )

Enable RTS on a UART (`rp2350` only).

##### `disable-uart-rts`
( uart -- )

Disable RTS on a UART (`rp2350` only).

##### `x-invalid-uart`
( -- )

Invalid UART exception.