# Bitmaps and Displays

Under `extra/common/bitmap.fs` there is optional code for supporting bitmap operations, and under `extra/common/ssd1306.fs` there is optional code for supporting I2C SSD1306-based displays. The `<bitmap>` class is defined under the `bitmap` module in `exttra/common/bitmap.fs`. The `<ssd1306>` class inherits from the `<bitmap>` class and is defined under the `ssd1306` module in `extra/common/ssd1306`.

The `<bitmap>` class is a general class for bitmaps and supports both drawing (including setting, or-ing, and-ing, bit-clearing, and exclusive or-ing) individual pixels and rectangles to bitmaps and drawing (including setting, or-ing, and-ing, bit-clearing, and exclusive or-ing) image data from one bitmap onto another bitmap. For bitmaps with dirty state information, i.e. `<ssd1306>` objects, drawing operations on bitmaps automatically update their dirty state. Note that the user must provide their own backing bitmap buffer for bitmap objects, whose size must be the number of columns in the bitmap times the number of rows divided by eight (as eight bits are in a byte) rounded up to the next full byte.

The `<ssd1306>` class implements an SSD1306 device interface and supports all the drawing operations implemented by the `<bitmap>` superclass along with maintaining dirty rectangles for optimizing updates. Drawing operations upon `<ssd1306>` objects do not immediately update the display; rather the display must be manually updated after drawing to its backing bitmap. This allows the user to carry out multiple drawing operations in sequence before updating the display at once.

### `bitmap`

The `bitmap` module contains the following words:

##### `op-set`
( -- operation )

The set bits operation.

##### `op-or`
( -- operation )

The or bits operation.

##### `op-and`
( -- operation )

The and bits operation.

##### `op-bic`
( -- operation )

The clear bits (i.e not-and) operation.

##### `op-xor`
( -- operation )

The exclusive-or bits operation.

##### `x-invalid-op`
( -- )

This exception is raised if an invalid drawing operation is specified.

##### `bitmap-buf-size`
( columns rows -- bytes )

Get the size of a bitmap buffer in bytes for a given number of columsn and rows.

##### `<bitmap>`
( -- class )

The `<bitmap>` class is the base class for bitmaps. It can be used directly, e.g. for offscreen bitmaps, or through its subclass `<ssd1306>`.

The `<bitmap>` class includes the following constructor:

##### `new`
( buffer-addr columns rows bitmap -- )

This constructor initializes a `<bitmap>` instance to have *columns* columns, *rows* rows, and a bitmap buffer at address *buffer-addr*. Note that the buffer pointed to by *buffer-addr* must be of size *columns* times *rows* divided by eight rounded up to the next integer. When this is called, the buffer will be zeroed and the entire bitmap will be marked as dirty.

The `<bitmap>` class includes the following members:

##### `bitmap-cols`
( bitmap -- addr )

The address of a cell containing the number of columns in a bitmap.

##### `bitmap-rows`
( bitmap -- addr )

The address of a cell containing the number of rows in a bitmap.

The `<bitmap>` class includes the following methods:

##### `clear-bitmap`
( bitmap -- )

Set an entire bitmap to zero and mark it as dirty.

##### `dirty?`
( bitmap -- dirty? )

Get whether a bitmap is dirty. Note that if a bitmap does not have support for dirty state tracking this will always return true.

##### `pixel@`
( column row bitmap -- state )

Get whether a pixel at *column* and *row* in *bitmap* is on or off, returning true or false respectively. If the pixel is outside the bounds of *bitmap*, false is returned.

##### `draw-pixel-const`
( constant dst-column dst-row op dst-bitmap -- )

Apply an operation *op* to a pixel at *dst-column* and *dst-row* of *dst-bitmap* with a constant value consisting of the bit *dst-row* modulo eight of *constant* and mark that pixel as dirty.

##### `draw-rect-const`
( constant dst-column column-count dst-row row-count op dst-bitmap -- )

Apply an operation *op* to a rectangle at *dst-column* to *dst-column* plus *column-count* minus one and *dst-row* to *dst-row* plus *row-count* minus one of *dst-bitmap* with a constant value consisting of the bit row modulo eight of *constant* and mark that pixel as dirty.

##### `draw-rect`
( src-column dst-column column-count src-row dst-row row-count op src-bitmap dst-bitmap -- )

Apply an operation *op* to a rectangle in *dst-bitmap* at *dst-column* to *dst-column* plus *column-count* minus one and *dst-row* to *dst-row* plus *row-count* minus one of *dst-bitmap* with the contents of a rectangle in *src-bitmap* at *src-column* to *src-column* plus *column-count* minus one and *src-row* to *src-row* plus *row-count* minus one.

### `ssd1306`

The `ssd1306` module contains the following words:

##### `SSD1306_I2C_ADDR`
( -- i2c-addr )

The default I2C address for SSD1306-based displays, $3C.

##### `<ssd1306>`
( -- class )

The `<ssd1306>` class is the class for I2C SSD1306-based displays. It inherits from the `<bitmap>` class and can be drawn to using the operations defined in that class. It maintains a dirty rectangle, which is updated when the user invokes its `update-display` method. Note that column zero is on the lefthand side of the display and row zero is on the top of the display.

The `<ssd1306>` class includes the following constructor:

##### `new`
( pin0 pin1 buffer-addr columns rows i2c-addr i2c-device ssd1306 -- )

This constructor initializes an I2C SSD1306 display with the SDA and SCK pins specified as GPIO pins *pin0* and *pin1* (it does not matter which is which), a backing buffer at *buffer-addr* (with the same considerations as backing buffers for other `<bitmap>` instances), *columns* columns, *rows* rows, the I2C address *i2c-addr*, the I2C device index *i2c-device* (note that this must match the I2C device index for pins *pin0* and *pin1*), and the `<ssd1306>` instance being initialized, *ssd1306*.

The <ssd1306> class includes the following method:

##### `update-display`
( ssd1306 -- )

This updates the SSD1306-based display with the current contents of its dirty rectangle, and then clears its dirty state. This must be called to update the display's contents after drawing to the display, which otherwise has no effect on the display itself.
