# Bezier Curves

There is an implementation of Bézier curves using De Casteljau's algorithm in `extra/common/bezier.fs`

It contains the following module:

### `bezier`

This module contains the following word:

##### `draw-bezier`
( coef-addr coef-count max-depth xt -- )

where:

- *coef-addr* is an array of x,y pairs of S31.32 fixed point values corresponding to Bézier control points.
- *coef-count* is the number of control points.
- *max-depth* is the maximum depth to evaluate the Bézier curve to, based on successively subdividing *t* from an initial interval of [0, 1] by 2 until either the interval is smaller than 2^-*max-depth* or the distance of the segment formed in x,y space is smaller than 2.
- *xt* is an execution token to execute to draw each individual line segment from x0,y0 to x1,y1 with the signature ( D: x0 D: y0 D: x1 D: y1 -- ) where each value is a S31.32 fixed-point value.
