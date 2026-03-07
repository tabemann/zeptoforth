# Bitmap Utilities

Under `extra/common/bitmap_utils.fs` there is optional code for supporting additional bitmap operations, particularly line and circle-drawing.

Under `extra/common/bitmap_lit.fs` there is optional code for defining bitmap literals. Bitmap literals specify static blocks of pixels which are used as buffers for instances of `bitmap::<bitmap-no-clear>` with the same width and height. Note that they are constrained by the maximum line length of 255 bytes that zeptoforth supports.

### `bitmap-utils`

The `bitmap-utils` module contains the following words:

##### `draw-pixel-line`
( constant x0 y0 x1 y1 op dst-bitmap -- )

This draws a pixel line with drawing operation *op* on *dst-bitmap* from (*x0*, *y0) to (*x1*, *y1*) using the *constant* value as used by `bitmap::draw-pixel-const`.

##### `draw-rect-line`
( constant width height x0 y0 x1 y1 op dst-bitmap -- )

This draws a line with a rectangular pen of *width* and *height* with drawing operation *op* on *dst-bitmap* from (*x0*, *y0*) to (*x1*, *y1*) with the pen centered on the line using the *constant* value as used by `bitmap::draw-rect-const`. Note that the drawing operation `op-xor` is not recommended because it will most likely give undesirable results.

##### `draw-bitmap-line`
( src-x src-y width height x0 y0 x1 y1 op src-bitmap dst-bitmap -- )

This draws a line with the rectangle from (*src-x*, *src-y*) to (*src-x* + *width*, *src-y* + *height*) in *src-bitmap* as a pen with drawing operation *op* on *dst-bitmap* from (*x0*, *y0*) to (*x1*, *y1*) with the pen centered on the line. Note that the drawing operation `op-xor` is not recommended because it will most likely give undesirable results. The same may be true of `op-set` in some cases as well.

##### `draw-pixel-circle`
( constant x y radius op dst-bitmap -- )

This draws an unfilled pixel circle with drawing operation *op* on *dst-bitmap* centered at (*x*, *y*) with *radius* using the *constant* value as used by *bitmap::draw-pixel-const`.

##### `draw-rect-circle`
( constant width height x y radius op dst-bitmap -- )

This draws an unfilled circle with a rectangular pen of *width* and *height* with drawing operation *op* on *dst-bitmap* centered at (*x*, *y*) with *radius* with the pen centered on the edge of the circle using the *constant* value as used by `bitmap::draw-rect-const`. Note that the drawing operation `op-xor` is not recommended because it will most likely give undesirable results.

##### `draw-bitmap-circle`
( src-x src-y width height x y radius op src-bitmap dst-bitmap -- )

This draws an unfilled circle with the rectangle from (*src-x*, *src-y*) to (*src-x* + *width*, *src-y* + *height*) in *src-bitmap* as a pen with drawing operation *op* on *dst-bitmap* centered at (*x*, *y*) with *radius* with the pen centered on the edge of the circle. Note that the drawing operation `op-xor` is not recommended because it will most likely give undesirable results. The same may be true of `op-set` in some cases as well.

##### `draw-filled-circle`
( constant x y radius op dst-bitmap -- )

This draws a filled pixel circle with drawing operation *op* on *dst-bitmap* centered at (*x*, *y*) with *radius* using the *constant* value as used by *bitmap::draw-pixel-const`.

### `bitmap-lit`

The `bitmap-lit` class contains the following words:

##### `begin-bitmap-lit`
( width height "name" -- addr width height y line-addr )

Begin the definition of a bitmap literal.

##### `!!`
( addr width height y line-addr "pixels" -- addr width height y line-addr )

Define a row of a bitmap literal; all characters until the end of the line above $20 are treated as 1 pixels, all characters until the end of the line from $20 and below are treated as 0 pixels, starting after a single character of whitespace after `!!`. Note that if the full width of the line is not defined the remaining pixels are set to 0.

##### `end-bitmap-lit`
( addr width height y line-addr -- )

End the definition of a bitmap literal. Any remaining undefined rows are set to 0.