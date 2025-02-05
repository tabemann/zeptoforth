\ Copyright (c) 2025 tmsgthb (GitHub)
\ Copyright (c) 2025 Travis Bemann
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

\ This is a driver for the LCD1602 I2C module.

begin-module lcd1602

  i2c import
    
  begin-module lcd1602-internal

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
    
    \ Default I2C device's addr
    $7c 1 rshift constant I2C_ADDR

    \ LCD I2C variables
    variable v-i2c-device
    variable v-need-reset

    \ Size of the LCD buffer
    17 constant lcd-buffer-size
    
    \ Check flag
    : need-reset? ( -- reset? ) v-need-reset @ 0<> ;
    
    \ Set flag
    : need-reset! ( reset? -- ) v-need-reset ! ;
    
    \ Calculate mode
    : set-mode ( -- mode ) LCD_FUNCTIONSET LCD_2LINE or ;
    
    \ Calculate display mode
    : set-display ( -- display )
      LCD_DISPLAYON LCD_CURSOROFF or LCD_BLINKOFF or LCD_DISPLAYCONTROL or
    ;
    
    \ Clear display constant to stack
    : clear-display ( -- display ) LCD_CLEARDISPLAY ;
    
    \ Calculate direction mode
    : set-direction ( -- direction )
      LCD_ENTRYLEFT LCD_ENTRYSHIFTDECREMENT or LCD_ENTRYMODESET or
    ;
    
    \ Calculate cursor code
    : (set-cursor) ( row col - ) swap 0= if $80 or else $C0 or then ;
    
    \ Copy bytes from stack to address, started by dest-addr
    : cr-buffer ( x0 x1 .. xn dest-addr u -- u' )
      { dest-addr size }
      size 0 ?do dest-addr i + c! loop
      dest-addr size reverse
      size
    ;
    
    \ Send data to i2c device
    : send-data ( c-addr u i2c -- u' )
      need-reset? if >i2c-restart false need-reset! else >i2c then
      drop
      10 ms
    ;

    \ Send string to i2c device 
    \ !! String must be started with @ !!
    : send-text ( c-addr bytes i2c -- u' )
      over 1+ [: { c-addr bytes i2c-device buf }
        [char] @ buf c!
        c-addr buf 1+ bytes move
        buf bytes 1+ i2c-device
        need-reset? if
          >i2c-restart-stop
        else
          >i2c-stop
        then
        true need-reset!
        drop
        10 ms
      ;] with-allot
    ;

    \ Set cursor to row col position 
    : set-cursor ( row col -- )
      lcd-buffer-size [: { lcd-buffer }
        $80 rot rot
        (set-cursor) lcd-buffer 2 cr-buffer
        lcd-buffer swap v-i2c-device @ send-data
      ;] with-allot
    ;

  end-module> import
  
  \ Activate I2C for this device
  : activate-lcd-i2c ( -- ) I2C_ADDR v-i2c-device @ i2c-target-addr! ;
    
  \ Clear lcd display
  : clear-display ( -- )
    lcd-buffer-size [: { lcd-buffer }
      $80 clear-display lcd-buffer 2 cr-buffer
      lcd-buffer swap v-i2c-device @
      send-data
    ;] with-allot
  ;

  \ Put string at row col position
  : put-text { row col c-addr bytes -- }
    row col set-cursor
    c-addr bytes v-i2c-device @ send-text 
  ;

  \ Set the I2C device
  : lcd-i2c-device! ( i2c-device -- ) v-i2c-device ! ;
  
  \ Init I2C device
  : init-lcd-i2c { pin1 pin2 i2c-device -- }
    i2c-device lcd-i2c-device!
    i2c-device master-i2c
    i2c-device 7-bit-i2c-addr
    100000 i2c-device i2c-clock!
    i2c-device pin1 i2c-pin
    i2c-device pin2 i2c-pin
    activate-lcd-i2c
    i2c-device enable-i2c
  ;

  \ Init LCD display
  : init-lcd ( -- )
    lcd-buffer-size [: { lcd-buffer }
      false need-reset!
      $80 set-mode lcd-buffer 2 cr-buffer
      lcd-buffer swap v-i2c-device @ send-data
      20 ms
      $80 set-display lcd-buffer 2 cr-buffer
      lcd-buffer swap v-i2c-device @
      send-data
      20 ms
    ;] with-allot
  ;

end-module
