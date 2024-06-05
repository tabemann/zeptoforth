\ Copyright (c) 2023-2024 Travis Bemann
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

begin-module st7735s-1

  oo import
  bitmap import
  bitmap-internal import
  pin import
  spi import

  \ Construct a 16-bit color from three 8-bit components
  : rgb16 { r g b -- color }
    b 3 rshift $1F and 8 lshift
    g 2 rshift $3F and dup 3 rshift $7F and swap $7 and 13 lshift or or
    r 3 rshift $1F and 3 lshift or
  ;

  <bitmap> begin-class <st7735s-1>

    begin-module st7735s-1-internal
      
      \ SPI device
      cell member st7735s-1-device

      \ Reset pin
      cell member st7735s-1-reset-pin

      \ DC pin
      cell member st7735s-1-dc-pin

      \ CS pin
      cell member st7735s-1-cs-pin

      \ Backlight pin
      cell member st7735s-1-backlight-pin

      \ CLK pin
      cell member st7735s-1-clk-pin

      \ DIN pin
      cell member st7735s-1-din-pin
      
      \ Dirty rectangle start column
      cell member st7735s-1-dirty-start-col

      \ Dirty rectangle end column
      cell member st7735s-1-dirty-end-col

      \ Dirty rectangle start row
      cell member st7735s-1-dirty-start-row

      \ Dirty rectangle end row
      cell member st7735s-1-dirty-end-row

      \ Foreground color
      cell member st7735s-1-fg-color

      \ Background color
      cell member st7735s-1-bg-color

      \ Reset the ST7735S-1
      method reset-st7735s-1 ( self -- )

      \ Initialize the ST7735S-1
      method init-st7735s-1 ( self -- )

      \ Send a command to the ST7735S-1
      method cmd>st7735s-1 ( cmd self -- )

      \ Send a byte of data to the ST7735S-1
      method data>st7735s-1 ( data self -- )

      \ Set the ST7735S-1 window
      method st7735s-1-window! ( start-col end-col start-row end-row self -- )

      \ Update a rectangular space on the ST7735S device
      method update-area ( start-col end-col start-row end-row self -- )
      
    end-module> import

    \ Set the backlight
    method backlight! ( backlight self -- )
      
    \ Update the ST7735S-1 device
    method update-display ( self -- )

    \ Set the foreground color and dirty the display
    method fg-color! ( fg-color self -- )

    \ Set the background color and dirty the display
    method bg-color! ( bg-color self -- )

    \ Get the foreground color
    method fg-color@ ( self -- fg-color )

    \ Get the foreground color
    method bg-color@ ( self -- bg-color )
    
  end-class

  <st7735s-1> begin-implement

    \ Constructor
    :noname { fg bg din clk dc cs backlight reset buf cols rows device self -- }
      buf cols rows self <bitmap>->new
      device self st7735s-1-device !
      reset self st7735s-1-reset-pin !
      dc self st7735s-1-dc-pin !
      cs self st7735s-1-cs-pin !
      backlight self st7735s-1-backlight-pin !
      clk self st7735s-1-clk-pin !
      din self st7735s-1-din-pin !
      fg self st7735s-1-fg-color !
      bg self st7735s-1-bg-color !
      dc output-pin
      cs output-pin
      backlight output-pin
      high dc pin!
      high cs pin!
      high backlight pin!
      reset output-pin
      low reset pin!
      device clk spi-pin
      device din spi-pin
      device master-spi
      10000000 device spi-baud!
      8 device spi-data-size!
      false false device motorola-spi
      device enable-spi
      self reset-st7735s-1
      self init-st7735s-1
      0 cols 0 rows self st7735s-1-window!
    ; define new

    \ Reset the ST7735S-1
    :noname { self -- }
      high self st7735s-1-reset-pin @ pin!
      200 ms
      low self st7735s-1-reset-pin @ pin!
      200 ms
      high self st7735s-1-reset-pin @ pin!
      200 ms
    ; define reset-st7735s-1

    \ Initialize the ST7735S-1
    :noname { self -- }
      $11 self cmd>st7735s-1 \ Sleep exit
      
      120 ms
      
      $21 self cmd>st7735s-1 
      $21 self cmd>st7735s-1 

      $B1 self cmd>st7735s-1
      $05 self data>st7735s-1
      $3A self data>st7735s-1
      $3A self data>st7735s-1

      $B2 self cmd>st7735s-1
      $05 self data>st7735s-1
      $3A self data>st7735s-1
      $3A self data>st7735s-1

      $B3 self cmd>st7735s-1 
      $05 self data>st7735s-1
      $3A self data>st7735s-1
      $3A self data>st7735s-1
      $05 self data>st7735s-1
      $3A self data>st7735s-1
      $3A self data>st7735s-1

      $B4 self cmd>st7735s-1
      $03 self data>st7735s-1

      $C0 self cmd>st7735s-1
      $62 self data>st7735s-1
      $02 self data>st7735s-1
      $04 self data>st7735s-1

      $C1 self cmd>st7735s-1
      $C0 self data>st7735s-1

      $C2 self cmd>st7735s-1
      $0D self data>st7735s-1
      $00 self data>st7735s-1

      $C3 self cmd>st7735s-1
      $8D self data>st7735s-1
      $6A self data>st7735s-1   

      $C4 self cmd>st7735s-1
      $BD self data>st7735s-1 
      $EE self data>st7735s-1

      $C5 self cmd>st7735s-1
      $0E self data>st7735s-1

      $E0 self cmd>st7735s-1
      $10 self data>st7735s-1
      $0E self data>st7735s-1
      $02 self data>st7735s-1
      $03 self data>st7735s-1
      $0E self data>st7735s-1
      $07 self data>st7735s-1
      $02 self data>st7735s-1
      $07 self data>st7735s-1
      $0A self data>st7735s-1
      $12 self data>st7735s-1
      $27 self data>st7735s-1
      $37 self data>st7735s-1
      $00 self data>st7735s-1
      $0D self data>st7735s-1
      $0E self data>st7735s-1
      $10 self data>st7735s-1

      $E1 self cmd>st7735s-1
      $10 self data>st7735s-1
      $0E self data>st7735s-1
      $03 self data>st7735s-1
      $03 self data>st7735s-1
      $0F self data>st7735s-1
      $06 self data>st7735s-1
      $02 self data>st7735s-1
      $08 self data>st7735s-1
      $0A self data>st7735s-1
      $13 self data>st7735s-1
      $26 self data>st7735s-1
      $36 self data>st7735s-1
      $00 self data>st7735s-1
      $0D self data>st7735s-1
      $0E self data>st7735s-1
      $10 self data>st7735s-1

      $3A self cmd>st7735s-1 
      $05 self data>st7735s-1

      $36 self cmd>st7735s-1
      $A8 self data>st7735s-1

      $29 self cmd>st7735s-1
      
    ; define init-st7735s-1

    \ Set the backlight pin
    :noname { backlight self -- }
      backlight self st7735s-1-backlight-pin @ pin!
    ; define backlight!
    
    \ Send a command to the ST7735S-1
    :noname { W^ cmd self -- }
      low self st7735s-1-dc-pin @ pin!
      low self st7735s-1-cs-pin @ pin!
      cmd 1 self st7735s-1-device @ buffer>spi
    ; define cmd>st7735s-1

    \ Send 8 bits of data to the ST7735S-1
    :noname { W^ data self -- }
      high self st7735s-1-dc-pin @ pin!
      low self st7735s-1-cs-pin @ pin!
      data 1 self st7735s-1-device @ buffer>spi
      high self st7735s-1-cs-pin @ pin!
    ; define data>st7735s-1

    \ Set the entire display to be dirty
    :noname { self -- }
      0 self st7735s-1-dirty-start-col !
      self bitmap-cols @ self st7735s-1-dirty-end-col !
      0 self st7735s-1-dirty-start-row !
      self bitmap-rows @ self st7735s-1-dirty-end-row !
    ; define set-dirty

    \ Clear dirty rectangle
    :noname { self -- }
      0 self st7735s-1-dirty-start-col !
      0 self st7735s-1-dirty-end-col !
      0 self st7735s-1-dirty-start-row !
      0 self st7735s-1-dirty-end-row !
    ; define clear-dirty
    
    \ Get whether an ST7735S-1 device is dirty
    :noname { self -- dirty? }
      self st7735s-1-dirty-start-col @ self st7735s-1-dirty-end-col @ <>
      self st7735s-1-dirty-start-row @ self st7735s-1-dirty-end-row @ <> and
    ; define dirty?
  
    \ Dirty a pixel on an ST7735S-1 device
    :noname { col row self -- }
      self dirty? if
        row self st7735s-1-dirty-start-row @ min self st7735s-1-dirty-start-row !
        row 1+ self st7735s-1-dirty-end-row @ max self st7735s-1-dirty-end-row !
        col self st7735s-1-dirty-start-col @ min self st7735s-1-dirty-start-col !
        col 1+ self st7735s-1-dirty-end-col @ max self st7735s-1-dirty-end-col !
      else
        row self st7735s-1-dirty-start-row !
        row 1+ self st7735s-1-dirty-end-row !
        col self st7735s-1-dirty-start-col !
        col 1+ self st7735s-1-dirty-end-col !
      then
    ; define dirty-pixel

    \ Dirty an area on an ST7735S-1 device
    :noname { start-col end-col start-row end-row self -- }
      start-col end-col < start-row end-row < and if
        start-col start-row self dirty-pixel
        end-col 1- end-row 1- self dirty-pixel
      then
    ; define dirty-area
    
    \ Set the ST7735S-1 window
    :noname { start-col end-col start-row end-row self -- }
      $2A self cmd>st7735s-1
      0 self data>st7735s-1
      start-col 1+ self data>st7735s-1
      0 self data>st7735s-1
      end-col 1+ self data>st7735s-1
      $2B self cmd>st7735s-1
      0 self data>st7735s-1
      start-row 26 + self data>st7735s-1
      0 self data>st7735s-1
      end-row 26 + self data>st7735s-1
      $2C self cmd>st7735s-1
    ; define st7735s-1-window!

    \ Update a rectangular space on the ST7735S device
    :noname { start-col end-col start-row end-row self -- }
      start-col end-col start-row end-row self st7735s-1-window!
      high self st7735s-1-dc-pin @ pin!
      low self st7735s-1-cs-pin @ pin!
      end-row 1+ start-row ?do
        self start-col i end-col start-col - 1+ dup 1 lshift [:
          { self start-col row cols line-buf }
          row 3 rshift self page-addr start-col + { page-buf }
          self st7735s-1-fg-color @ self st7735s-1-bg-color @ { fg bg }
          row 7 and bit { mask }
          0 begin dup cols < while
            dup page-buf + c@ mask and if fg else bg then
            over 1 lshift line-buf + h!
            1+
          repeat
          drop
          line-buf cols 1 lshift self st7735s-1-device @ buffer>spi
        ;] with-aligned-allot
      loop
      high self st7735s-1-cs-pin @ pin!
    ; define update-area

    \ Update the ST7735S device
    :noname { self -- }
      self dirty? if 
        self st7735s-1-dirty-start-col @
        self st7735s-1-dirty-end-col @
        self st7735s-1-dirty-start-row @
        self st7735s-1-dirty-end-row @
        self update-area
        self clear-dirty
      then
    ; define update-display

    \ Set the foreground color and dirty the display
    :noname { fg-color self -- }
      fg-color self st7735s-1-fg-color !
      self set-dirty
    ; define fg-color!

    \ Set the background color and dirty the display
    :noname { bg-color self -- }
      bg-color self st7735s-1-bg-color !
      self set-dirty
    ; define bg-color!

    \ Get the foreground color
    :noname ( self -- ) st7735s-1-fg-color @ ; define fg-color@

    \ Get the background color
    :noname ( self -- ) st7735s-1-bg-color @ ; define bg-color@

  end-implement
  
end-module