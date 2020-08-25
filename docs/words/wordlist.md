# Wordlist Words

These words are in `forth-wordlist`.

##### `forth-wordlist`
( -- wid )

Get the default wordlist, which is both a flash and a RAM wordlist.

##### `internal-wordlist`
( -- wid )

Get the internal wordlist, which includes both words defined in the kernel and words define outside the kernel

##### `wordlist`
( -- wid )

Create a wordlist, a RAM wordlist (starting at 32768) if the current compilation mode is RAM mode, or a flash wordlist (starting at 1) if the current compilation mode is flash mode.

##### `ram-wordlist`
( -- wid )

Create a RAM wordlist, regardless of the current compilation mode.

##### `flash-wordlist`
( -- wid )

Create a flash wordlist, regardless of the current compilation mode.

##### `set-current`
( wid -- )

Set the current compilation wordlist, into which new words will be added.

##### `get-current`
( -- wid )

Get the current compilation wordlist.

##### `set-order`
( widn ... wid1 u -- )

Pop the new wordlist order size off the stack, then pop that number of wordlist IDs off the stack and set the wordlist order to them, with the wordlist ID closest to the top of the stack being the first wordlist in the wordlist order and the wordlist ID furthest from the top of the stack being the last wordlist in the wordlist order.

##### `get-order`
( -- widn ... wid1 u )

Push the wordlist order onto the stack, with the last wordlist ID in the order being furthest from the top of the stack and the first wordlist ID in the order being closest to the top of the stack, followed by pushing the wordlist order size onto the top of the stack.
