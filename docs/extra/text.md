# Text display words

The text-only PicoCalc terminal emulator exposes _text displays_ that can be accessed directly via `picocalc-term::with-term-display`. These text displays inherit from the `text8::<text8>` class, which implements a means of storing and fetching 8-bit characters with RGB332 color for the foreground and background and optional underlining. These text displays can often be accessed directly faster in a random-access fashion than generating ANSI terminal control characters to generate the equivalent characters.

### `text8`:

The `text8` module contains the following words:

##### `rgb8`:
( r g b -- color )

A convenience word for constructing an RGB332 color from red, green, and blue components from 0 to 255.

##### `text8-buf-size`
( columns rows -- bytes )

Get the size of a `<text8>` buffer in bytes for a given number of columns and rows.

The `text8` module contains the following class:

#### `<text8>`

The `<text8>` class has the following constructor:

##### `new`
( buffer columns rows text -- )

Construct a `<text8>` object with the address of a buffer of the size returned by `text8-buf-size` for the given numbers of columns and rows, a number of columns, and a number of rows.

The `<text8>` class has the following methods:

##### `dim@`
( text -- columns rows )

Get text dimensions.

##### `clear-text`
( text -- )

Clear the text.

##### `dirty?`
( text -- dirty? )

Get whether the text is dirty.

##### `whole-char!`
( c fg-color bk-color underlined? column row text -- )

Quickly write a whole character at once at a column and row.

##### `char@`
( column row text -- c )

Get a character at a column and row.

##### `char!`
( c column row text -- )

Set a character at a column and row.

##### `fg-color@`
( column row text -- color )

Get a character's foreground color at a column and row.

##### `fg-color!`
( color column row text -- )

Set a character's foreground color at a column and row.

##### `bk-color@`
( column row text -- color )

Get a character's background color at a column and row.

##### `bk-color!`
( color column row text -- )

Set a character's background color at a column and row.

##### `underlined@`
( column row text -- underlined? )

Get whether a character is underlined at a column and row.
    
##### `raw-underlined@`
( column row text -- underlined? )

Get whether a character is underlined without testing for bounds.

##### `underlined!`
( underlined? column row text -- )

Set whether a character is underlined at a column and row.

##### `scroll-up`
( lines text -- )

Scroll up a number of lines (note that this will *not* fill in the new lines).

##### `scroll-down`
( lines text -- )

Scroll down a number of lines (note that this will *not* fill in the new lines).

##### `invert-text`
( text -- )

Invert the text.

### `st7365p-text-common`

The `st7365p-text-common` module contains the following class:

#### `<st7365p-text-common>`

This class inherits from `text8::<text8>`.

The `<st7365p-text-common>` class has the following constructor:

##### `new`
( dc-pin cs-pin invert the-font buffer columns rows physical-columns physical-rows display -- )

Construct an `<st7365p-text-common>` object for a given DC pin, CS pin, invert boolean, font, `<text8>` buffer, number of character columns, number of character rows, physical number of pixel columns, and physical number of pixel rows.

The `<st7365p-text-common>` class has the following methods:

##### `char-dim@`
( display -- columns rows )

Get the character dimensions.

##### `backlight!`
( backlight display -- )

Set the backlight (this may be a no-op).

##### `update-display`
( display -- )

Update the display.

##### `clear-display`
( display -- )

Clear the display, including pixels that do not have corresponding characters at the edges of the display.

### `st7789v-text-common`

The `st7789v-text-common` module contains the following class:

#### `<st7789v-text-common>`

This class inherits from `text8::<text8>`.

The `<st7789v-text-common>` class has the following constructor:

##### `new`
( dc-pin cs-pin backlight-pin the-font buffer round columns rows physical-columns physical-rows display -- )

Construct an `<st7789v-text-common>` object for a given DC pin, CS pin, backlight pin, font, `<text8>` buffer, round boolean, number of character columns, number of character rows, physical number of pixel columns, and physical number of pixel rows.

The `<st7789v-text-common>` class has the following methods:

##### `char-dim@`
( display -- columns rows )

Get the character dimensions.

##### `backlight!`
( backlight display -- )

Set the backlight (this may be a no-op).

##### `update-display`
( display -- )

Update the display.

##### `clear-display`
( display -- )

Clear the display, including pixels that do not have corresponding characters at the edges of the display.
