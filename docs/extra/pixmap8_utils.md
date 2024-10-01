# Pixmap Utilities

Under `extra/common/pixmap8_utils.fs` there is optional code for supporting additional pixmap operations, particularly line and circle-drawing.

### `pixmap8-utils`

The `pixmap8-utils` module contains the following words:

##### `draw-pixel-line`
( color x0 y0 x1 y1 dst-pixmap -- )

This draws a pixel line on *dst-pixmap* from (*x0*, *y0) to (*x1*, *y1*) using the *color* value as used by `pixmap8::draw-pixel-const`.

##### `draw-rect-line`
( color width height x0 y0 x1 y1 dst-pixmap -- )

This draws a line with a rectangular pen of *width* and *height* on *dst-pixmap* from (*x0*, *y0*) to (*x1*, *y1*) with the pen centered on the line using the *color* value as used by `pixmap8::draw-rect-const`. Note that the drawing operation `op-xor` is not recommended because it will most likely give undesirable results.

##### `draw-bitmap-line`
( color mask-x mask-y width height x0 y0 x1 y1 mask-bitmap dst-pixmap -- )

This draws a line with a rectangle from (*mask-x*, *mask-y*) to (*mask-x* + *width*, *mask-y* + *height*) in *mask-bitmap* as a pen on *dst-pixmap* from (*x0*, *y0*) to (*x1*, *y1*) with the pen centered on the line, where only bits in *mask-bitmap* which are set to one are colored with *color*.

##### `draw-pixmap-line`
( src-x src-y width height x0 y0 x1 y1 src-pixmap dst-pixmap -- )

This draws a line with the rectangle from (*src-x*, *src-y*) to (*src-x* + *width*, *src-y* + *height*) in *src-pixmap* as a pen on *dst-pixmap* from (*x0*, *y0*) to (*x1*, *y1*) with the pen centered on the line.

##### `draw-mask-line`
( color mask-x mask-y src-x src-y width height x0 y0 x1 y1 mask-bitmap src-pixmap dst-pixmap -- )

This draws a line with a rectangle from (*src-x*, *src-y*) to (*src-x* + *width*, *src-y* + *height*) in *src-pixmap* as a pen and (*mask-x*, *mask-y*) to (*mask-x* + *width*, *mask-y* + *height*) in *mask-bitmap* as a mask for said pen on *dst-pixmap* from (*x0*, *y0*) to (*x1*, *y1*) with the pen centered on the line, where only bits in *mask-bitmap* which are set to one have the corresponding pixels copied from *src-pixmap* to *dst-pixmap*.

##### `draw-pixel-circle`
( color x y radius dst-pixmap -- )

This draws an unfilled pixel circle on *dst-pixmap* centered at (*x*, *y*) with *radius* using the *color* value as used by `pixmap8::draw-pixel-const`.

##### `draw-rect-circle`
( color width height x y radius dst-pixmap -- )

This draws an unfilled circle with a rectangular pen of *width* and *height* on *dst-pixmap* centered at (*x*, *y*) with *radius* with the pen centered on the edge of the circle using the *color* value as used by `pixmap8::draw-rect-const`.

##### `draw-bitmap-circle`
( color mask-x mask-y width height x y radius mask-bitmap dst-pixmap -- )

This draws an unfilled circle with the rectangle from (*mask-x*, *mask-y*) to (*mask-x* + *width*, *mask-y* + *height*) in *mask-bitmap* as a pen on *dst-pixmap* centered at (*x*, *y*) with *radius* with the pen centered on the edge of the circle, where only bits in *mask-bitmap* which are set to one are colored with *color*.

##### `draw-pixmap-circle`
( src-x src-y width height x y radius src-pixmap dst-pixmap -- )

This draws an unfilled circle with the rectangle from (*src-x*, *src-y*) to (*src-x* + *width*, *src-y* + *height*) in *src-pixmap* as a pen on *dst-pixmap* centered at (*x*, *y*) with *radius* with the pen centered on the edge of the circle.

##### `draw-mask-circle`
( mask-x mask-y src-x src-y width height x y radius mask-bitmap src-pixmap dst-pixmap -- )

This draws an unfilled circle with a rectangle from (*src-x*, *src-y*) to (*src-x* + *width*, *src-y* + *height*) in *src-pixmap* as a pen and (*mask-x*, *mask-y*) to (*mask-x* + *width*, *mask-y* + *height*) in *mask-bitmap* as a mask for said pen on *dst-pixmap* centered at (*x*, *y*) with *radius* with the pen centered on the edge of the circle.

##### `draw-filled-circle`
( color x y radius dst-pixmap -- )

This draws a filled pixel circle on *dst-pixmap* centered at (*x*, *y*) with *radius* using the *color* value as used by `pixmap8::draw-pixel-const`.
