# Fonts

Under `extra/common/font.fs` there is optional code for supporting seven and eight-bit bitmap fonts for use with the bitmaps implemented in `extra/common/bitmap.fs` and displays based upon such bitmaps. The `<font>` class is defined under the `font` module in `extra/common/font.fs` Under `extra/common/simple_font.fs` there is a simple seven-bit ASCII font, defined as `a-simple-font` in the `simple-font` module.

### `font`

The `font` module contains the following words:

##### `font-buf-size`
( char-columns char-rows char-count -- bytes )

This gets the number of bytes needed for the buffer for a font with *char-count* glyphs of *char-columns* columns and *char-rows* rows.

##### `<font>`
( -- class )

This class constitutes a bitmap font. It is backed by a bitmap which is not directly exposed to the user.

It has the following constructor:

##### `new`
( buffer-addr default-char-index char-columns char-rows char-count font -- )

This constructor initializes a `<font>` instance with *char-count* glyphs, glyphs of *char-columns* columns and *char-rows* rows, a default character *default-char-index* for out of range characters, and a backing bitmap buffer address *buffer-addr*. The backing bitmap buffer is set to zero at this time.

It has the following methods:

##### `char-row!`
( xn ... x0 row character font -- )

This populates row *row* of the glyph *character* in font *font* with bits from one or more cells taken off the stack, where the bits are populated right to left from least significant bit to most significant bit of the cell on the top-most cell on the stack to the bottom-most cell on the stack, with extra bits being discarded. This is not meant for direct user use but by use by fonts to populate glyphs when they are initialized.

##### `set-char`
( character column row bitmap font -- )

This sets a bitmap *bitmap* to a glyph *character* of font *font* with the top left corner of the glyph set to the bitmap being at column *column* and row *row*.

##### `or-char`
( character column row bitmap font -- )

This ors a bitmap *bitmap* to a glyph *character* of font *font* with the top left corner of the glyph ored to the bitmap being at column *column* and row *row*.

##### `and-char`
( character column row bitmap font -- )

This ands a bitmap *bitmap* to a glyph *character* of font *font* with the top left corner of the glyph anded on the bitmap being at column *column* and row *row*.

##### `bic-char`
( character column row bitmap font -- )

This bit-clears a bitmap *bitmap* to a glyph *character* of font *font* with the top left corner of the glyph bit-cleared on the bitmap being at column *column* and row *row*.

##### `xor-char`
( character column row bitmap font -- )

This exclusive-ors a bitmap *bitmap* to a glyph *character* of font *font* with the top left corner of the glyph exclusive-ored on the bitmap being at column *column* and row *row*.

##### `set-string`
( c-addr bytes column row bitmap font -- )

This sets a bitmap *bitmap* to a string at address *c-addr* of size *bytes* of font *font* with the top left corner of the string's glyphs set to the bitmap being at column *column* and row *row*.

##### `or-string`
( c-addr bytes column row bitmap font -- )

This ors a bitmap *bitmap* to a string at address *c-addr* of size *bytes* of font *font* with the top left corner of the string's glyphs ored to the bitmap being at column *column* and row *row*.

##### `and-string`
( c-addr bytes column row bitmap font -- )

This ands a bitmap *bitmap* to a string at address *c-addr* of size *bytes* of font *font* with the top left corner of the string's glyphs anded to the bitmap being at column *column* and row *row*.

##### `bic-string`
( c-addr bytes column row bitmap font -- )

This bit-clears a bitmap *bitmap* to a string at address *c-addr* of size *bytes* of font *font* with the top left corner of the string's glyphs bit-cleared to the bitmap being at column *column* and row *row*.

##### `xor-string`
( c-addr bytes column row bitmap font -- )

This exclusive-ors a bitmap *bitmap* to a string at address *c-addr* of size *bytes* of font *font* with the top left corner of the string's glyphs exclusive-ored to the bitmap being at column *column* and row *row*.

### `simple-font`

The `simple-font` module contains the following words:

##### `a-simple-font`
( -- font )

This, once initialized, is an instance of `<font>` that provides a simple seven-bit ASCII font.

##### `init-simple-font`
( -- )

This initializes `a-simple-font`. It must be called before `a-simple-font` may be used.

