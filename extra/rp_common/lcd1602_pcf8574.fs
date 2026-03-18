\ Copyright (c) 2022-2026 Travis Bemann
\ Copyright (c) 2026 Ken Mitton
\ 
\ Permission is hereby granted, free of charge, to any person obtaining a copy
\ of this software and associated documentation files (the "Software"), to deal
\ in the Software without restriction, including without limitation the rights
\ to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
\ copies of the Software, and to permit persons to whom the Software is
\ furnished to do so, subject to the following conditions:
\ 
\ The above copyright notice and this permission notice shall be included in
\ all copies or substantial portions of the Software.
\ 
\ THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
\ IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
\ FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
\ AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
\ LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
\ OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
\ SOFTWARE.

\ This is a driver for the LCD1602 I2C module, with PCF8574 interface driver.
\ Based on:
\ * https://cdn-shop.adafruit.com/datasheets/TC1602A-01T.pdf
\ * https://www.ti.com/lit/ds/symlink/pcf8574.pdf
\ * https://github.com/johnrickman/LiquidCrystal_I2C

begin-module lcd1602-pcf8574

  oo import
  i2c import

  \ An I2C LCD1602-PCF8574 device
  <object> begin-class <lcd1602-pcf8574>
    
    begin-module lcd1602-pcf8574-internal

      \ I2C device, often 0 or 1
      cell member lcd1602-pcf8574-device

      \ I2C address, often $27 or $3B
      cell member lcd1602-pcf8574-addr

      \ I2C pin 0 (SCK or SDA, does not matter)
      cell member lcd1602-pcf8574-pin0

      \ I2C pin 1 (SCK or SDA, does not matter)
      cell member lcd1602-pcf8574-pin1
      
      \ Cols, 16 or 20
      cell member lcd1602-pcf8574-cols

      \ Rows, 2 or 4
      cell member lcd1602-pcf8574-rows

      \ Command base register
      cell member lcd1602-pcf8574-cmd-base

      \ Backlight state
      cell member lcd1602-pcf8574-backlightval

      \ Display on / off / blink / scroll state
      cell member lcd1602-pcf8574-displaycontrol

      \ Display lines / font size / etc
      cell member lcd1602-pcf8574-displayfunction

      \ Text direction, etc
      cell member lcd1602-pcf8574-displaymode

      \ OLED versions need slightly different startup than LCD
      cell member lcd1602-pcf8574-oled?

    end-module> import

    method put-text ( row col c-addr u lcd1602-pcf8574 -- )

    method set-cursor ( row col lcd1602-pcf8574 -- )

    method set-text ( c-addr u lcd1602-pcf8574 -- )

    method home ( lcd1602-pcf8574 -- )

    method clear ( lcd1602-pcf8574 -- )

    method display! ( flag lcd1602-pcf8574 -- )

    method backlight! ( flag lcd1602-pcf8574 -- )

    method blink! ( flag lcd1602-pcf8574 -- )

    method cursor! ( flag lcd1602-pcf8574 -- )

    method autoscroll! ( flag lcd1602-pcf8574 -- )

    method scroll-display-left ( lcd1602-pcf8574 -- )
    method scroll-display-right ( lcd1602-pcf8574 -- )

    method left-to-right ( lcd1602-pcf8574 -- )
    method right-to-left ( lcd1602-pcf8574 -- )

    \ Not yet implemented, custom characters:
    \   create-char ( location data - )
    \   create-char ( location charmap - )

  end-class

  \ Default I2C address
  $27 constant LCD1602_PCF8574_I2C_ADDR

  continue-module lcd1602-pcf8574-internal
    
    \ LCD1602-PCF8574 Commands - see Datasheet

    \ Common constants
    $01 constant LCD_CLEARDISPLAY
    $02 constant LCD_RETURNHOME
    $04 constant LCD_ENTRYMODESET
    $08 constant LCD_DISPLAYCONTROL
    $10 constant LCD_CURSORSHIFT
    $20 constant LCD_FUNCTIONSET
    $40 constant LCD_SETCGRAMADDR
    $80 constant LCD_SETDDRAMADDR
    
    \ Display entry mode constants
    $00 constant LCD_ENTRYRIGHT
    $02 constant LCD_ENTRYLEFT
    $01 constant LCD_ENTRYSHIFTINCREMENT
    $00 constant LCD_ENTRYSHIFTDECREMENT
    
    \ Display on/off control constants
    $04 constant LCD_DISPLAYON
    $00 constant LCD_DISPLAYOFF
    $02 constant LCD_CURSORON
    $00 constant LCD_CURSOROFF
    $01 constant LCD_BLINKON
    $00 constant LCD_BLINKOFF
    
    \ Display/cursor shift constants
    $08 constant LCD_DISPLAYMOVE
    $00 constant LCD_CURSORMOVE
    $04 constant LCD_MOVERIGHT
    $00 constant LCD_MOVELEFT
    
    \ Function set constants
    $10 constant LCD_8BITMODE
    $00 constant LCD_4BITMODE
    $08 constant LCD_2LINE
    $00 constant LCD_1LINE
    $00 constant LCD_5x8DOTS
    
    \ flags for backlight control
    $08 constant LCD_BACKLIGHT
    $00 constant LCD_NOBACKLIGHT

    $04 constant En  \ Enable bit
    $02 constant Rw  \ Read/Write bit
    $01 constant Rs  \ Register select bit

    \ Initialize I2C for an LCD1602-PCF8574 device
    : init-i2c { self -- }
      self lcd1602-pcf8574-device @ { device }
      device self lcd1602-pcf8574-pin0 @ i2c-pin
      device self lcd1602-pcf8574-pin1 @ i2c-pin
      device master-i2c
      device 7-bit-i2c-addr
      self lcd1602-pcf8574-addr @ device i2c-target-addr!
      device enable-i2c
    ;

    \ Begin constructing a command to send to the LCD1602-PCF8574
    : begin-cmd { self -- }
      ram-here self lcd1602-pcf8574-cmd-base !
    ;
    
    \ Write a byte to the current command being constructed
    : >cmd ( c -- ) cram, ;
    
    \ Send the current command that has been constructed
    : send-cmd { self -- }
      self [: dup { self }
        self lcd1602-pcf8574-cmd-base @ dup ram-here swap -
        self lcd1602-pcf8574-device @ >i2c-stop drop
      ;] try nip self lcd1602-pcf8574-cmd-base @ ram-here! ?raise
    ;

    : expander-write { _data self -- }
      self begin-cmd
      _data self lcd1602-pcf8574-backlightval @ or >cmd
      self send-cmd
    ;

    : pulse-enable { _data self -- }
        _data En or self expander-write	\ En high
        1 ms \ enable pulse must be >450ns, this is overkill

        _data En not and self expander-write 	\ En low
        1 ms \ commands need > 37us to settle
    ;

    : write-4-bits { val self -- }
      val dup self expander-write self pulse-enable
    ;

    \ write either command or data
    : send { val mode self -- }
      val $f0 and mode or self write-4-bits
      val 4 lshift $f0 and mode or self write-4-bits
    ;

    \ One-piece command
    : >command { val self -- }
      val 0 self send
    ;

    : write { val self -- }
      val Rs self send
    ;

    \ Initialize an LCD1602-PCF8574 display
    : init-display { self -- }
      LCD_4BITMODE LCD_1LINE or LCD_5x8DOTS or
      self lcd1602-pcf8574-rows @ 1 > if
        LCD_2LINE or
      then
      self lcd1602-pcf8574-displayfunction !
      \ SEE PAGE 45/46 FOR INITIALIZATION SPECIFICATION!
      \ according to datasheet, we need at least 40ms after power rises above 2.7V
      \ before sending commands. Arduino can turn on way before 4.5V so we'll wait 50
      \ delay(50); 
      50 ms
    
      \ Now we pull both RS and R/W low to begin commands
      \ reset expander and turn backlight off (Bit 8 =1)
      self lcd1602-pcf8574-backlightval @ self >command
      1000 ms
  
      \ put the LCD into 4 bit mode
      \ this is according to the hitachi HD44780 datasheet
      \ figure 24, pg 46

      \ we start in 8bit mode, try to set 4 bit mode
      $03 4 lshift self write-4-bits
      5 ms \ wait min 4.1ms

      \ second try
      $03 4 lshift self write-4-bits
      5 ms \ wait min 4.1ms
   
      \ third go!
      $03 4 lshift self write-4-bits
      1 ms \ 150 microseconds
   
      \ finally, set to 4-bit interface
      $02 4 lshift self write-4-bits

      \ set # lines, font size, etc.
      LCD_FUNCTIONSET self lcd1602-pcf8574-displayfunction @ or
      self >command

      \ turn the display on with no cursor or blinking default
      LCD_DISPLAYON LCD_CURSOROFF or LCD_BLINKOFF or
      dup self lcd1602-pcf8574-displaycontrol !
      LCD_DISPLAYCONTROL or self >command

      \ clear it off
      self clear

      \ Initialize to default text direction (for roman languages)
      LCD_ENTRYLEFT LCD_ENTRYSHIFTDECREMENT or
      dup self lcd1602-pcf8574-displaymode !

      \ set the entry mode
      LCD_ENTRYMODESET or self >command

      self home
    ;

    \ Update a stored bitset with a new value for a certain bit, and trigger display update
    : update-flag! { flag bitmask-constant property-name action-constant self -- }
      \ Like: true LCD_DISPLAYON lcd1602-pcf8574-displaycontrol LCD_DISPLAYCONTROL self
      self property-name @
      bitmask-constant
      flag if
        or
      else
        not and
      then
      dup self property-name !
      action-constant or self >command
    ;

  end-module

  <lcd1602-pcf8574> begin-implement
  
    \ Initialize an LCD1602-PCF8574 device
    :noname { pin0 pin1 cols rows oled? i2c-addr i2c-device self -- }
      i2c-device self lcd1602-pcf8574-device !
      i2c-addr self lcd1602-pcf8574-addr !
      pin1 self lcd1602-pcf8574-pin1 !
      pin0 self lcd1602-pcf8574-pin0 !
      cols self lcd1602-pcf8574-cols !
      rows self lcd1602-pcf8574-rows !
      oled? self lcd1602-pcf8574-oled? !
      LCD_NOBACKLIGHT self lcd1602-pcf8574-backlightval !
      self init-i2c
      self init-display
    ; define new

    :noname { c-addr u self -- }
      u 0 ?do c-addr i + c@ self write loop
    ; define set-text

    :noname { row col c-addr u self -- }
      row col self set-cursor
      c-addr u self set-text
    ; define put-text

    \ Turn the display on/off (quickly)
    :noname { flag self -- }
      flag LCD_DISPLAYON lcd1602-pcf8574-displaycontrol LCD_DISPLAYCONTROL
      self update-flag!
    ; define display!

    \ Backlight on and off
    :noname { flag self -- }
      flag if
        LCD_BACKLIGHT
      else
        LCD_NOBACKLIGHT
      then
      self lcd1602-pcf8574-backlightval !
      0 self expander-write
    ; define backlight!

    \ Blinking cursor on and off
    :noname { flag self -- }
      flag LCD_BLINKON lcd1602-pcf8574-displaycontrol LCD_DISPLAYCONTROL
      self update-flag!
    ; define blink!

    \ Turns the underline cursor on/off
    :noname { flag self -- }
      flag LCD_CURSORON lcd1602-pcf8574-displaycontrol LCD_DISPLAYCONTROL
      self update-flag!
    ; define cursor!

    \ This will 'right justify' text from the cursor, or 'left justify' if false
    :noname { flag self -- }
      flag LCD_ENTRYSHIFTINCREMENT lcd1602-pcf8574-displaymode LCD_ENTRYMODESET
      self update-flag!
    ; define autoscroll!

    \ These commands scroll the display without changing the RAM.
    \ Each call moves the text one position.
    :noname { self -- }
	  LCD_CURSORSHIFT LCD_DISPLAYMOVE or LCD_MOVELEFT or self >command
    ; define scroll-display-left

    :noname { self -- }
	  LCD_CURSORSHIFT LCD_DISPLAYMOVE or LCD_MOVERIGHT or self >command
    ; define scroll-display-right

    \ This is for text that flows Left to Right
    :noname { self -- }
      self lcd1602-pcf8574-displaymode @
      LCD_ENTRYLEFT or
      dup self lcd1602-pcf8574-displaymode !
      LCD_ENTRYMODESET or self >command
    ; define left-to-right

    \ This is for text that flows Right to Left
    :noname { self -- }
      self lcd1602-pcf8574-displaymode @
      LCD_ENTRYLEFT not and
      dup self lcd1602-pcf8574-displaymode !
      LCD_ENTRYMODESET or self >command
    ; define right-to-left

    \ Clear display, set cursor position to zero
    :noname { self -- }
      LCD_CLEARDISPLAY self >command
      2 ms  \ this command takes a long time!

      \ OLED variants need an extra cursor setup beyond LCD variants
      self lcd1602-pcf8574-oled? @ if
        0 0 self set-cursor
      then
    ; define clear

    :noname { self -- }
      LCD_RETURNHOME self >command  \ clear display, set cursor position to zero
      2 ms  \ this command takes a long time!
    ; define home

    :noname { row col self -- }
      \ Calculate row offset
      \ int row_offsets[] = { 0x00, 0x40, 0x14, 0x54 };
      \ if ( row > _numlines ) {
      \     row = _numlines-1;    // we count rows starting w/0
      \ }
      row self lcd1602-pcf8574-rows @ tuck > if
        1-
      else
        drop row
      then
      case
        0 of $00 endof
        1 of $40 endof
        2 of $14 endof
        3 of $54 endof
      endcase
      col + 
      LCD_SETDDRAMADDR or

      self >command
    ; define set-cursor

  end-implement
  
end-module
