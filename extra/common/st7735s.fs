\ Copyright (c) 2023 Travis Bemann
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

begin-module st7735s

  oo import
  pixmap16 import
  pixmap16-internal import
  pin import
  spi import
  armv6m import

  <pixmap16> begin-class <st7735s>

    begin-module st7735s-internal
      
      \ SPI device
      cell member st7735s-device

      \ Reset pin
      cell member st7735s-reset-pin

      \ DC pin
      cell member st7735s-dc-pin

      \ CS pin
      cell member st7735s-cs-pin

      \ Backlight pin
      cell member st7735s-backlight-pin

      \ CLK pin
      cell member st7735s-clk-pin

      \ DIN pin
      cell member st7735s-din-pin
      
      \ Dirty rectangle start column
      cell member st7735s-dirty-start-col

      \ Dirty rectangle end column
      cell member st7735s-dirty-end-col

      \ Dirty rectangle start row
      cell member st7735s-dirty-start-row

      \ Dirty rectangle end row
      cell member st7735s-dirty-end-row

      \ Reset the ST7735S
      method reset-st7735s ( self -- )

      \ Initialize the ST7735S
      method init-st7735s ( self -- )

      \ Set the backlight
      method backlight! ( backlight self -- )
      
      \ Send a command to the ST7735S
      method cmd>st7735s ( cmd self -- )

      \ Send a byte of data to the ST7735S
      method data>st7735s ( data self -- )

      \ Set the ST7735S window
      method st7735s-window! ( start-col end-col start-row end-row self -- )

      \ Update a rectangular space on the SSD1306 device
      method update-area ( start-col end-col start-row end-row self -- )
      
    end-module> import

    \ Update the ST7735S device
    method update-display ( self -- )
    
  end-class

  <st7735s> begin-implement

    \ Constructor
    :noname { din clk dc cs backlight reset buf cols rows device self -- }
      buf cols rows self <pixmap16>->new
      device self st7735s-device !
      reset self st7735s-reset-pin !
      dc self st7735s-dc-pin !
      cs self st7735s-cs-pin !
      backlight self st7735s-backlight-pin !
      clk self st7735s-clk-pin !
      din self st7735s-din-pin !
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
      self reset-st7735s
      self init-st7735s
      0 cols 0 rows self st7735s-window!
    ; define new

    \ Reset the ST7735S
    :noname { self -- }
      high self st7735s-reset-pin @ pin!
      200 ms
      low self st7735s-reset-pin @ pin!
      200 ms
      high self st7735s-reset-pin @ pin!
      200 ms
    ; define reset-st7735s

    \ Initialize the ST7735S
    :noname { self -- }
      $11 self cmd>st7735s \ Sleep exit
      
      120 ms
      
      $21 self cmd>st7735s 
      $21 self cmd>st7735s 

      $B1 self cmd>st7735s
      $05 self data>st7735s
      $3A self data>st7735s
      $3A self data>st7735s

      $B2 self cmd>st7735s
      $05 self data>st7735s
      $3A self data>st7735s
      $3A self data>st7735s

      $B3 self cmd>st7735s 
      $05 self data>st7735s
      $3A self data>st7735s
      $3A self data>st7735s
      $05 self data>st7735s
      $3A self data>st7735s
      $3A self data>st7735s

      $B4 self cmd>st7735s
      $03 self data>st7735s

      $C0 self cmd>st7735s
      $62 self data>st7735s
      $02 self data>st7735s
      $04 self data>st7735s

      $C1 self cmd>st7735s
      $C0 self data>st7735s

      $C2 self cmd>st7735s
      $0D self data>st7735s
      $00 self data>st7735s

      $C3 self cmd>st7735s
      $8D self data>st7735s
      $6A self data>st7735s   

      $C4 self cmd>st7735s
      $BD self data>st7735s 
      $EE self data>st7735s

      $C5 self cmd>st7735s
      $0E self data>st7735s

      $E0 self cmd>st7735s
      $10 self data>st7735s
      $0E self data>st7735s
      $02 self data>st7735s
      $03 self data>st7735s
      $0E self data>st7735s
      $07 self data>st7735s
      $02 self data>st7735s
      $07 self data>st7735s
      $0A self data>st7735s
      $12 self data>st7735s
      $27 self data>st7735s
      $37 self data>st7735s
      $00 self data>st7735s
      $0D self data>st7735s
      $0E self data>st7735s
      $10 self data>st7735s

      $E1 self cmd>st7735s
      $10 self data>st7735s
      $0E self data>st7735s
      $03 self data>st7735s
      $03 self data>st7735s
      $0F self data>st7735s
      $06 self data>st7735s
      $02 self data>st7735s
      $08 self data>st7735s
      $0A self data>st7735s
      $13 self data>st7735s
      $26 self data>st7735s
      $36 self data>st7735s
      $00 self data>st7735s
      $0D self data>st7735s
      $0E self data>st7735s
      $10 self data>st7735s

      $3A self cmd>st7735s 
      $05 self data>st7735s

      $36 self cmd>st7735s
      $A8 self data>st7735s

      $29 self cmd>st7735s
      
    ; define init-st7735s

    \ Set the backlight pin
    :noname { backlight self -- }
      backlight self st7735s-backlight-pin @ pin!
    ; define backlight!
    
    \ Send a command to the ST7735S
    :noname { W^ cmd self -- }
      low self st7735s-dc-pin @ pin!
      low self st7735s-cs-pin @ pin!
      cmd 1 self st7735s-device @ buffer>spi
    ; define cmd>st7735s

    \ Send 8 bits of data to the ST7735S
    :noname { W^ data self -- }
      high self st7735s-dc-pin @ pin!
      low self st7735s-cs-pin @ pin!
      data 1 self st7735s-device @ buffer>spi
      high self st7735s-cs-pin @ pin!
    ; define data>st7735s

    \ Set the entire display to be dirty
    :noname { self -- }
      0 self st7735s-dirty-start-col !
      self pixmap-cols @ self st7735s-dirty-end-col !
      0 self st7735s-dirty-start-row !
      self pixmap-rows @ self st7735s-dirty-end-row !
    ; define set-dirty

    \ Clear dirty rectangle
    :noname { self -- }
      0 self st7735s-dirty-start-col !
      0 self st7735s-dirty-end-col !
      0 self st7735s-dirty-start-row !
      0 self st7735s-dirty-end-row !
    ; define clear-dirty
    
    \ Get whether an ST7735S device is dirty
    :noname { self -- dirty? }
      self st7735s-dirty-start-col @ self st7735s-dirty-end-col @ <>
      self st7735s-dirty-start-row @ self st7735s-dirty-end-row @ <> and
    ; define dirty?
  
    \ Dirty a pixel on an ST7735S device
    :noname { col row self -- }
      self dirty? if
        row self st7735s-dirty-start-row @ min self st7735s-dirty-start-row !
        row 1+ self st7735s-dirty-end-row @ max self st7735s-dirty-end-row !
        col self st7735s-dirty-start-col @ min self st7735s-dirty-start-col !
        col 1+ self st7735s-dirty-end-col @ max self st7735s-dirty-end-col !
      else
        row self st7735s-dirty-start-row !
        row 1+ self st7735s-dirty-end-row !
        col self st7735s-dirty-start-col !
        col 1+ self st7735s-dirty-end-col !
      then
    ; define dirty-pixel

    \ Dirty an area on an ST7735S device
    :noname { start-col end-col start-row end-row self -- }
      start-col end-col < start-row end-row < and if
        start-col start-row self dirty-pixel
        end-col 1- end-row 1- self dirty-pixel
      then
    ; define dirty-area
    
    \ Set the ST7735S window
    :noname { start-col end-col start-row end-row self -- }
      $2A self cmd>st7735s
      0 self data>st7735s
      start-col 1+ self data>st7735s
      0 self data>st7735s
      end-col 1+ self data>st7735s
      $2B self cmd>st7735s
      0 self data>st7735s
      start-row 26 + self data>st7735s
      0 self data>st7735s
      end-row 26 + self data>st7735s
      $2C self cmd>st7735s
    ; define st7735s-window!

    \ Update a rectangular space on the SSD1306 device
    :noname { start-col end-col start-row end-row self -- }
      start-col end-col start-row end-row self st7735s-window!
      high self st7735s-dc-pin @ pin!
      low self st7735s-cs-pin @ pin!
      end-row start-row ?do
        start-col i self pixel-addr end-col start-col - 1+ 1 lshift
        self st7735s-device @ buffer>spi
      loop
      high self st7735s-cs-pin @ pin!
    ; define update-area

    \ Update the SSD1306 device
    :noname { self -- }
      self dirty? if 
        self st7735s-dirty-start-col @
        self st7735s-dirty-end-col @
        self st7735s-dirty-start-row @
        self st7735s-dirty-end-row @
        self update-area
        self clear-dirty
      then
    ; define update-display

  end-implement
  
end-module