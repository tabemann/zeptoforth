# ANSI Terminal words

These are a number of word used to manage interaction with ANSI terminals. Note that no other terminals are supported.

### `forth`

The following words are in the `forth` (i.e. default) module:

##### `page`
( -- )

Clear the console and return the cursor to the home position.

### `ansi-term`

The following words are in `ansi-term`:

##### `escape`
( -- b )

The escape character.

##### `(dec.)`
( n -- )

Output a decimal number without any padding.

##### `csi`
( -- )

Output the CSI sequence.

##### `show-cursor`
( -- )

Show the cursor. Note that multiple `show-cursor` and `hide-cursor` pairs can be nested.

##### `hide-cursor`
( -- )

Show the cursor. Note that multiple `show-cursor` and `hide-cursor` pairs can be nested.

##### `reset-terminal-cursor`
( -- )

Force the cursor to be shown, resetting the `show-cursor`/`hide-cursor` count.

##### `reset-terminal-color`
( -- )

Reset the terminal color and style attributes.

##### `save-cursor`
( -- )

Save the cursor position. Note that `save-cursor` / `restore-cursor` pairs *cannot* be nested.

##### `restore-cursor`
( -- )

Restore the cursor position. Note that `save-cursor` / `restore-cursor` pairs *cannot* be nested.

##### `scroll-up`
( lines -- )

Scroll up the screen by a given number of lines.

##### `go-to-coord`
( row column -- )

Go to the specified row and column (each starting from zero).

##### `erase-end-of-line`
( -- )

Erase from the cursor to the end of the current line.

##### `erase-down`
( -- )

Erase the lines below the current line.

##### `query-cursor-position`
( -- )

Output the sequence to query the cursor position. Note that this is used by `get-cursor-position` and is not intended for use by the user.

##### `execute-hide-cursor`
( xt -- )

Hide the cursor, execute the given xt, then show the cursor. Note that calls to this can be nested. Also note that exceptions that occur within the xt will cause the cursor to be shown, if appropriate, again before the exceptions are re-raised.

##### `clear-key`
( -- )

Clear the saved input byte.

##### `get-key`
( -- b )

If there is a saved input byte, return it, clearing it in the process, otherwise get an input byte from the console.

##### `set-key`
( b -- )

Save an input byte, overwriting any which has already been saved.

##### `wait-number`
( -- n matches )

Attempt to read a number from the console, returning it and true if a parseable number is found, otherwise returning 0 and false.

##### `wait-char`
( b -- )

Wait for the given byte to be inputted from the console.

##### `expect-char`
( b -- flag )

Wait for a byte to be inputted from the console; if it is the given byte, drop it and return true, else save it and return false.

##### `get-cursor-position`
( -- row column  )

Get the current cursor position.

##### `execute-preserve-cursor`
( xt -- )

Save the cursor position, execute the given xt, then restore the cursor position. Note that calls to this can be nested. Also note that exceptions that occur within the xt will cause the cursor position to be restored before the exceptions are re-raised.

##### `get-terminal-size`
( -- rows columns )

Get the terminal size in rows and columns.

##### `reset-ansi-term`

Reset the state of the ANSI terminal. Note that if the cursor is currently hidden, it will not show it automatically.
