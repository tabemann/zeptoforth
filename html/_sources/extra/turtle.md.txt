# Turtle graphics

Optional support for turtle graphics is in `extra/rp2040/turtle.fs`. It is designed for compatibility with Thurtle, which is based on WAForth and runs in web browsers, and Forthtoise, which is on top of gforth and uses SDL. It allows changing the color of both the lines drawn and the turtle, but does not currently support changing the line thickness (even though that will likely change in the near future).

It is currently setup up for use with 16-bit graphics on the 160x80 ST7735S-based display on the Waveshare RP2040-LCD-0.96 but can be readily modified for other ST7735S-based displays, including ones on non-RP2040 based boards. Porting to SSD1306-based displays should be trivial and is an exercise left to the user.

The display the turtle is in is such that the (0, 0) lies in the lower left-hand corner of the display (all distances are specified in pixels) and a heading of 0 degrees (all angles here are specified in degrees, not radians) is pointing directly right along the X dimension. However, the starting position of the turtle is in the center of the display, i.e. at (80, 40) with the turtle pointing straight up, i.e. with a heading of 90 degrees. Also note that the turtle begins pen down, the turtle starts out with the color (0, 255, 0), i.e. green, and the lined drawn by the turtle starts out with the color (255, 255, 255), i.e white. Note however that not all possibly-specified colors can be distinguished due to the 16-bit color of the display.

### `turtle`

The `turtle` module contains the following words:

##### `setpencolor`
( r g b -- )

Set the current pen color, where *r*, *g*, and *b* are values between 0 to 255 (but note that 16-bit displays do not permit all possible colors in this color space to be distinguished from one another).

##### `setturtlecolor`
( r g b -- )

Set the current turtle color, where *r*, *g*, and *b* are values between 0 and 255 (likewise not all colors so specified may be distinguished in 16-bit color).

##### `forward`
( pixels -- )

Move the turtle forward a given number of *pixels*.

##### `backward`
( pixel -- )

Move the turtle backward a given number of *pixels*.

##### `left`
( angle -- )

Turn the turtle left *angle* degrees (not radians).

##### `right`
( right -- )

Turn the turtle right *angle* degrees (not radians).

##### `penup`
( -- )

Pen up, i.e. stop drawing lines with turtle movement.

##### `pendown`
( -- )

Pen down, i.e. start drawing lines with turtle movement.

##### `setxy`
( x y -- )

Set the position of the turtle to *x* and *y*, where (0, 0) is in the lower lefthand corner of the display.

##### `setheading`
( angle -- )

Set the heading of the turtnle to *angle* degrees, where 0 is pointing right along the X dimension and 90 is pointing up along the Y dimension.

##### `hideturtle`
( -- )

Hide the turtle.

##### `showturtle`
( -- )

Show the turtle.

##### `clear`
( -- )

Clear the display but leave the turtle in the same position and heading (and it will remain visible unless the turtle is hidden).

##### `setpensize`
( pixels -- )

This is currently a no-op but that will likely change in the near future.
