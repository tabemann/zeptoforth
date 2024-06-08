# Text Displays

In addition to bitmap and pixmap framebuffer-based graphics, there is also optional support for pure text displays with character matrices along with inverted video character maps. Currently support exists for SSD1306 and ST7735S-based displays with monospace bitmap fonts.

Support for SSD1306-based text displays is in `extra/common/ssd1306_text.fs`. With SSD1306-baed displays the user can select contrast settings from 0 to 255.

Support for ST7735S-based text displays is in `extra/common/st7735s_text.fs`. With ST7735S-based displays the user can select 16-bit (5-bit red, 6-bit green, and 5-bit blue) foreground and background colors and backlight on/off.

Note that these require `extra/common/clip.fs`, `extra/common/bitmap.fs`, `extra/common/font.fs`, and `extra/common/text_display.fs` to be loaded.

### `text-display`

The `text-display` module contains the following standalone words:

##### `text-buf-size`
( cols rows char-cols char-rows -- bytes )

This word calculates a buffer size for a text display's text buffer with *cols* columns and *rows* rows in display pixel dimensions and *char-cols* columns and *char-rows* rows in font pixel dimensions.

##### `invert-buf-size`
( cols rows char-cols char-rows -- bytes )

This word calculates a buffer size for a text display's inverted video buffer with *cols* columns and *rows* rows in display pixel dimensions and *char-cols* columns and *char-rows* rows in font pixel dimensions.

The `text-display` module contains the following class:

#### `<text-display>`

This class has the following constructor:

##### `new`
( text-buffer-addr invert-buffer-addr font cols rows self -- )

This constructs a text display with a text buffer *text-buffer-addr* with a size calculated with `text-buf-size`, an inverted video buffer *invert-buffer-addr* with a size calculated with `invert-buf-size`, a font *font*, and *cols* columns and *rows* rows in display pixel dimensions.

This class has the following methods:

##### `clear-display`
( self -- )

This clears a display and dirties it.

##### `set-dirty`
( self -- )

This dirties an entire display.

##### `dirty-char`
( col row self -- )

This dirties a character at column *col* and row *row*.

##### `dirty?`
( self -- dirty? )

This gets whether a display is dirty.

##### `dirty-rect@`
( self -- start-col start-row end-col end-row )

This gets a display's dirty rectangle. If the display is not dirty, *start-col* will equal *end-col* and *start-row* will equal *end-row*.

##### `clear-dirty`
( self -- )

This clears a display's dirty state.

##### `dim@`
( self -- cols rows )

This gets a display's dimensions in characters.

##### `char!`
( c col row self -- )

This sets column *col* and row *row* of the display to character *c*, dirtying the display.

##### `char@`
( col row self -- c )

This gets the character at column *col* and row *row* of the display.

##### `string!`
( c-addr u col row self -- )

This sets a horizontal line of characters starting from column *col* on the left at row *row* to the string represented by the address *c-addr* and count *u*.

##### `invert!`
( invert? col row self -- )

This sets the inverted video state of column *col* and row *row* to *invert?*.

##### `toggle-invert!`
( col row self -- )

This toggles the inverted video state of column *col* and row *row*.

##### `invert@`
( col row self -- invert? )

This gets the inverted video state of column *col* and row *row*.

##### `pixel@`
( pixel-col pixel-row self -- pixel-set? )

This gets the pixel value of a *pixel* at pixel column *pixel-col* and pixel row *pixel-row*.

### `ssd1306-text`

This module contains the following class:

#### `<ssd1306-text>`

This class has the following constructor:

##### `new`
( pin0 pin1 text-buffer-addr invert-buffer-addr font columns rows i2c-addr i2c-device ssd1306 -- )

This constructor initializes an I2C SSD1306 display with the SDA and SCK pins specified as GPIO pins *pin0* and *pin1* (it does not matter which is which), a text backing buffer at *text-buffer-addr* (with the same considerations as backing buffers for other `<text-display>` instances), an inverted video backing buffer at *invert-buffer-addr* (with the same considerations as backing buffers for other `<text-display>` instances), *columns* columns, *rows* rows, the I2C address *i2c-addr*, the I2C device index *i2c-device* (note that this must match the I2C device index for pins *pin0* and *pin1*), and the `<ssd1306-text>` instance being initialized, *ssd1306*.

This class has the following methods:

##### `update-display`
( ssd1306 -- )

This updates the SSD1306-based display with the current contents of its dirty rectangle, and then clears its dirty state. This must be called to update the display's contents after drawing to the display, which otherwise has no effect on the display itself.

##### `display-contrast!`
( constrast ssd1306 -- )

This sets the contrast of an SSD1306-based display to a value from 0 to 255.

### `st7735s-text`

The `st7735s-text` module contains the following words:

##### `rgb16`
( r g b -- color )

Create a 16-bit color with 5 bits for red, 6 bits for green, and 5 bits for blue from a red/green/blue triplet of values from 0 to 255 each.

##### `<st7735s-text>`
( -- class )

The `<st7735s-text>` class is the class for 16-bit SPI ST7735S-based displays with 5-bit red, 6-bit green, 5-bit blue foreground and background colors and bitmap backing buffers. It inherits from the `<bitmap>` class and can be drawn to using the operations defined in that class. It maintains a dirty rectangle, which is updated when the user invokes its `update-display` method. Note that column zero is on the lefthand side of the display and row zero is on the top of the display.

The `<st7735s-text>` class includes the following constructor:

##### `new`
( fg-color bg-color din-pin clk-pin dc-pin cs-pin backlight-pin reset-pin text-buffer-addr invert-buffer-addr font columns rows spi-device st7735s -- )

This constructor initializes an SPI ST7735S display at the SPI device *spi-device*, a text backing buffer at *text-buffer-addr* (with the same considerations as backing buffers for other `<text-display>` instances), an inverted video backing buffer at *invert-buffer-addr* (with the same considerations as backing buffers for other `<text-display>` instances), *columns* columns, *rows* rows, the DIN pin *din-pin*, the CLK pin *clk-pin*, the DC pin *dc-pin*, the chip-select pin *cs-pin*, the backlight pin *backlight-pin*, the reset pin *reset-pin*, a 16-bit foreground color *fg-color*, a 16-bit background color *bg-color*, and the `<st7735s>` instance being initialized, *st7735s*. Note that *din-pin* and *clk-pin* must match the SPI device *spi-device* specified.

The `<st7735s>` class includes the following method:

##### `update-display`
( st7735s -- )

This updates the ST7735S-based display with the current contents of its dirty rectangle, and then clears its dirty state. This must be called to update the display's contents after drawing to the display, which otherwise has no effect on the display itself.

##### `backlight!`
( backlight st7735s -- )

Set the on/off state of the ST7735S-based display's backlight.

##### `fg-color!`
( fg-color st7735s -- )

Set the foreground color of the ST7735S-based display and dirty the display.

##### `bg-color!`
( bg-color st7735s -- )

Set the background color of the ST7735S-based display and dirty the display.

##### `fg-color@`
( st7735s -- fg-color )

Get the foreground color of the ST7735S-based display.

##### `bg-color@`
( st7735s -- bg-color )

Get the background color of the ST7735S-based display.