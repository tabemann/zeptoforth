# Fonts

Under `extra/common/font.fs` there is optional code for supporting seven and eight-bit bitmap fonts for use with the bitmaps implemented in `extra/common/bitmap.fs` and displays based upon such bitmaps. The `<font>` class is defined under the `font` module in `extra/common/font.fs` Under `extra/common/simple_font.fs` there is a simple seven-bit ASCII font, defined as `a-simple-font` in the `simple-font` module.

### `font`

The `font` module contains the following words:

##### `font-buf-size`
( char-columns char-rows min-char-index max-char-index -- bytes )

This gets the number of bytes needed for the buffer for a font with glyphs from character *min-char-index* to character *max-char-index*, inclusive, with glyphs of *char-columns* columns and *char-rows* rows.

##### `<font>`
( -- class )

This class constitutes a bitmap font. It is backed by a bitmap which is not directly exposed to the user.

The `<font>` class includes the following constructor:

##### `new`
( buffer-addr default-char-index char-columns char-rows min-char-index max-char-index font -- )

This constructor initializes a `<font>` instance with a minimum character index of *min-char-index*, a maximum character index of *max-char-index*, glyphs of *char-columns* columns and *char-rows* rows, a default character *default-char-index* for out of range characters, and a backing bitmap buffer address *buffer-addr*. The backing bitmap buffer is set to zero at this time.

The `<font>` class includes the following members:

##### `char-cols`
( font -- addr )

The address of a cell containing the number of columns in each glyph in a font.

##### `char-rows`
( font -- addr )

The address of a cell containing the number of rows in each glyph in a font.

##### `min-char-index`
( font -- addr )

The address of a cell containing the minimum character index in a font, below which the character specified by `default-char-index` is to be substituted.

##### `max-char-index
( font -- addr )

The address of a cell containing the maximum character index in a font, above which the character specified by `default-char-index` is to be substituted.

##### `default-char-index`
( font -- addr )

The address of a cell containing the default substituting character index.

The `<font>` class includes the following methods:

##### `char-row!`
( xn ... x0 row character font -- )

This populates row *row* of the glyph *character* in font *font* with bits from one or more cells taken off the stack, where the bits are populated right to left from least significant bit to most significant bit of the cell on the top-most cell on the stack to the bottom-most cell on the stack, with extra bits being discarded. This is not meant for direct user use but by use by fonts to populate glyphs when they are initialized.

##### `draw-char`
( character column row op bitmap font -- )

This draws a glyph *character* of font *font* with drawing operation *op* to *bitmap* with the top left corner of the glyph set to the bitmap being at column *column* and row *row*.

##### `draw-string`
( c-addr bytes column row op bitmap font -- )

This draws a string at address *c-addr* of size *bytes* with glyphs from font *font* with drawing operation *op* to *bitmap* with the top left corner of the string's glyphs set to the bitmap being at column *column* and row *row*.

##### `draw-char-to-pixmap16`
( color character column row pixmap font -- )

This draws a glyph *character* of font *font* to *pixmap* with the top left corner of the glyph set to the pixmap being at column *column* and row *row*. Note that the glyph is treated as a mask for a bitmap-to-pixmap drawing operation, so if one wants to change the background color one must do so with a separate drawing operation.

##### `draw-string-to-pixmap16`
( color c-addr bytes column row pixmap font -- )

This draws a string at address *c-addr* of size *bytes* with glyphs from font *font* to *pixmap* with the top left corner of the string's glyphs set to the pixmap being at column *column* and row *row*. Note that the glyph is treated as a mask for a bitmap-to-pixmap drawing operation, so if one wants to change the background color one must do so with a separate drawing operation.

### `simple-font`

The `simple-font` module contains the following words:

##### `a-simple-font`
( -- font )

This, once initialized, is an instance of `<font>` that provides a simple seven-bit ASCII font.

##### `init-simple-font`
( -- )

This initializes `a-simple-font`. It must be called before `a-simple-font` may be used.

