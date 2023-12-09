# Bitmap Utilities

Under `extra/common/bitmap_utils.fs` there is optional code for supporting additional bitmap operations, particularly line and circle-drawing.

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
