# Exposed Kernel Variables

### `forth`

These words are in `forth`.

##### `state`

Get the STATE variable address

##### `base`

Get the BASE variable address

##### `pause-enabled`

Get the PAUSE enabled variable address.

##### `xon-xoff-enabled`

Get the XON/XOFF enabled variable address; note that this variable defaults to 0 (disabled).

##### `ack-nak-enabled`

Get the ACK/NAK enabled variable address; note that this variable defaults to -1 (enabled).

##### `bel-enabled`

Get the BEL enabled variable address; note that this variable defaults to -1 (enabled).

##### `color-enabled`

Get the color enabled variable address; note that this variable defaults to -1 (enabled).

##### `uart-special-enabled`

Get the UART special character (control-C and control-T) handling enabled variable address; note that this variable defaults to -1 (enabled).

##### `dict-base`

Get the RAM dictionary base variable address
	
##### `ram-base`

Get the RAM base

##### `ram-end`

Get the RAM end

##### `flash-base`

Get the flash base

##### `flash-end`

Get the flash end
	
##### `stack-base`

Get the current stack base variable address

##### `stack-end`

Get the current stack end variable address

##### `rstack-base`

Get the current return stack base variable address

##### `rstack-end`

Get the current returns stack end variable address

##### `handler`

Get the current exception handler variable address

##### `>parse`
( -- parse-index-addr )

The parse index; note that the address of the parse index is returned and not the parse index itself, and it may be modified by the user (it must remain in the range from 0 to the size of the parse buffer returned by `source` - 1, and reducing its value is not recommended).

##### `source`
( -- parse-buffer-addr parse-buffer-size )

The current string in the parse buffer, as defined by its starting address and its size in bytes.

##### `build-target`

Get the address to store a literal in for the word currently being
built

##### `sys-ram-dict-base`

Get the base of the system RAM dictionary space

##### `>in`

The input buffer index

##### `input#`

The input buffer count

##### `input`

The input buffer

##### `order-count`

The wordlist count

##### `order`

The wordlist order
	
##### `prompt-hook`

The prompt hook

##### `handle-number-hook`

The handle number hook

##### `failed-parse-hook`

The failed parse hook

##### `refill-hook`

The refill hook

##### `pause-hook`

The pause hook

##### `validate-dict-hook`

The dictionary size validation hook

##### `fault-handler-hook`

Get the FAULT-HANDLER-HOOK variable address

##### `null-handler-hook`

Get the NULL-HANDLER-HOOK variable address

##### `svcall-handler-hook`

Get the SVCALL-HANDLER-HOOK variable address

##### `pendsv-handler-hook`

Get the PENDSV-HANDLER-HOOK variable address

##### `systick-handler-hook`

Get the SYSTICK-HANDLER-HOOK variable address

##### `cortex-m7?`
( -- flag )

Get whether zeptoforth is built for ARM Cortex-M7.

##### `cortex-m33?`
( -- flag )

Get whether zeptoforth is built for ARM Cortex-M33.

##### `chip`
( -- chip1 chip0 )

Get ID codes indicating the CPU make and model.

* *chip0* of `$7270` indicates Raspberry Pi Ltd.
* *chip0* of `$73746D` indicates STMicroelectronics.

* *chip1* of `2040` indicates RP2040.
* *chip1* of `2350` indicates RP2350.
* *chip1* of `$66000197` indicates STM32F407.
* *chip1* of `$6600019B` indicates STM32F411.
* *chip1* of `$660002EA` indicates STM32F746.
* *chip1* of `$6C0001DC` indicates STM32L476.

##### `rp2040?`
( -- flag )

Get whether zeptoforth is built for RP2040. (Only available for `rp2040` and `rp2350`.)

##### `rp2350?`
( -- flag )

Get whether zeptoforth is built for RP2350. (Only available for `rp2040` and `rp2350`.)

## RP2040 and RP2350 Words

### `forth`

This word is in `forth`.

##### `unique-id`
( -- d )

Returns the 64-bit board unique ID value, as a double number.
