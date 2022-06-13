# UART Words

There are a number of words for controlling UART0 and UART1 available on the RP2040, STM32F411, and STM32F407 on top of the general `emit`, key`, `emit?`, and `key?` words. These are as follows:

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

##### `emit-uart`
( c uart -- )

Emit a byte to a UART.

##### `key-uart`
( uart -- c )

Read a byte from a UART.

##### `emit-uart?`
( uart -- flag )

Get whether a UART is ready to emit a byte.

##### `key-uart?`
( uart -- flag )

Get whether a byte is ready to be read from a UART.

##### `flush-uart`
( uart -- )

Flush a UART's transmit buffer.

##### `uart-alternate`
( uart -- )

The alternate function for a given UART.

##### `uart-pin`
( uart pin -- )

Set the alternate function for *pin* to configure it for *uart*.

##### `x-invalid-uart`
( -- )

Invalid UART exception, i.e. a UART other than 0 or 1.