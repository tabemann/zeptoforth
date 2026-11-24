# Flood fill words

There is an implementation of flood filling in `extra/common/flood.fs`. The core implementation is graphics backend-agnostic, but convenience words are provided for `<bitmap>`, `<pixmap8>`, and `<pixmap16>` graphics backends.

### `flood`

This module contains the following words:

##### `draw-flood`
( x-min y-min cols rows x-seed y-seed max-depth inside-xt set-xt -- )

This is the core of flood filling.

*x-min*, *y-min*, *cols*, and *rows* define the geometry of the maximal space that flood filling will attempt to fill, with (*x-min*, *y-min*) being the upper lefthand pixel of the space and (*x-min* + *cols* - 1, *y-min* + *rows* - 1) being the lower righthand pixel of the space. Undefined behavior may result if this space extends beyond the boundaries of the graphics backend.

*x-seed* and *y-seed* define the coordinates of the seed pixel to start filling from.

*max-depth* is the maximum nesting depth for the stack used by flood filling. This stack takes up 8 bytes per frame and is temporarily alloted in the current tasks's RAM dictionary. If *max-depth* is reached flood filling may not properly function; this is primarily meant as a safety to prevent crashes caused by overrunning the end of the current task's RAM dictionary.

*inside-xt* is an execution token with the signature ( x y -- inside? ) that defines an operation to test a pixel for whether it is inside the area to be filled.

*set-xt* is an execution token with the signature ( x y -- ) that defines an operation to set a pixel as filled.

Note that any pixel's coordinates for which *set-xt* is called _must_ afterwards return `false` when those coordinates are passed to *inside-xt* or otherwise flood filling will not function properly.

Each coordinate component is limited to the range [-32768, 32767], as 16-bit signed values are used to store coordinate components in the stack in the RAM dictionary. Coordinate components provided outside this range will be clamped to this range.

##### `draw-bitmap-flood`
( set? x y max-depth dest -- )

This is a convenience word for doing flood fills for bitmaps.

*set?* is whether to set unset pixels, if `true`, or to unset set pixels, if `false`.

*x* and *y* are the seed coordinates.

*max-depth* is the maximal stack depth, like in `draw-flood`.

*dest* is the destination `<bitmap>` graphics backend

Note that this word is only available if the `bitmap` module is available.

##### `draw-pixmap8-flood`
( color x y max-depth dest -- )

This is a convenience word for doing flood fills bounded by the same color as that being set for 8-bit pixmaps.

*color* is the RGB332 color value to set and to bound the flood fill by.

*x* and *y* are the seed coordinates.

*max-depth* is the maximal stack depth, like in `draw-flood`.

*dest* is the destination `<pixmap8>` graphics backend

Note that this word is only available if the `pixmap8` module is available.

##### `draw-pixmap8-flood-inside`
( inside-color set-color x y max-depth dest -- )

This is a convenience word for doing flood fills for an area specified as that of a particular color for 8-bit pixmaps.

*inside-color* is the RGB332 color value to test for.

*set-color* is the RGB332 color value to set.

*x* and *y* are the seed coordinates.

*max-depth* is the maximal stack depth, like in `draw-flood`.

*dest* is the destination `<pixmap8>` graphics backend

Note that this word is only available if the `pixmap8` module is available.

##### `draw-pixmap16-flood`
( color x y max-depth dest -- )

This is a convenience word for doing flood fills bounded by the same color as that being set for 16-bit pixmaps.

*color* is the RGB565 color value to set and to bound the flood fill by.

*x* and *y* are the seed coordinates.

*max-depth* is the maximal stack depth, like in `draw-flood`.

*dest* is the destination `<pixmap16>` graphics backend

Note that this word is only available if the `pixmap16` module is available.

##### `draw-pixmap16-flood-inside`
( inside-color set-color x y max-depth dest -- )

This is a convenience word for doing flood fills for an area specified as that of a particular color for 16-bit pixmaps.

*inside-color* is the RGB565 color value to test for.

*set-color* is the RGB565 color value to set.

*x* and *y* are the seed coordinates.

*max-depth* is the maximal stack depth, like in `draw-flood`.

*dest* is the destination `<pixmap16>` graphics backend

Note that this word is only available if the `pixmap16` module is available.
