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

begin-module st7735s-text

  oo import
  text-display import
  pin import
  spi import

  \ Construct a 16-bit color from three 8-bit components
  : rgb16 { r g b -- color }
    b 3 rshift $1F and 8 lshift
    g 2 rshift $3F and dup 3 rshift $7F and swap $7 and 13 lshift or or
    r 3 rshift $1F and 3 lshift or
  ;

  <text-display> begin-class <st7735s-text>

    begin-module st7735s-text-internal
      
      \ SPI device
      cell member st7735s-text-device

      \ Reset pin
      cell member st7735s-text-reset-pin

      \ DC pin
      cell member st7735s-text-dc-pin

      \ CS pin
      cell member st7735s-text-cs-pin

      \ Backlight pin
      cell member st7735s-text-backlight-pin

      \ CLK pin
      cell member st7735s-text-clk-pin

      \ DIN pin
      cell member st7735s-text-din-pin

      \ Foreground color
      cell member st7735s-text-fg-color

      \ Background color
      cell member st7735s-text-bg-color

      \ Reset the ST7735S-TEXT
      method reset-st7735s-text ( self -- )

      \ Initialize the ST7735S-TEXT
      method init-st7735s-text ( self -- )

      \ Send a command to the ST7735S-TEXT
      method cmd>st7735s-text ( cmd self -- )

      \ Send a byte of data to the ST7735S-TEXT
      method data>st7735s-text ( data self -- )

      \ Set the ST7735S-TEXT window
      method st7735s-text-window! ( start-col end-col start-row end-row self -- )

      \ Update a rectangular space on the ST7735S device
      method update-area ( start-col end-col start-row end-row self -- )
      
    end-module> import

    \ Set the backlight
    method backlight! ( backlight self -- )
      
    \ Update the ST7735S-TEXT device
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

  <st7735s-text> begin-implement

    \ Constructor
    :noname
      { fg bg din clk dc cs back reset tbuf ibuf font cols rows device self -- }
      tbuf ibuf font cols rows self <text-display>->new
      device self st7735s-text-device !
      reset self st7735s-text-reset-pin !
      dc self st7735s-text-dc-pin !
      cs self st7735s-text-cs-pin !
      back self st7735s-text-backlight-pin !
      clk self st7735s-text-clk-pin !
      din self st7735s-text-din-pin !
      fg self st7735s-text-fg-color !
      bg self st7735s-text-bg-color !
      dc output-pin
      cs output-pin
      back output-pin
      high dc pin!
      high cs pin!
      high back pin!
      reset output-pin
      low reset pin!
      device clk spi-pin
      device din spi-pin
      device master-spi
      10000000 device spi-baud!
      8 device spi-data-size!
      false false device motorola-spi
      device enable-spi
      self reset-st7735s-text
      self init-st7735s-text
      0 cols 0 rows self st7735s-text-window!
    ; define new

    \ Reset the ST7735S-TEXT
    :noname { self -- }
      high self st7735s-text-reset-pin @ pin!
      200 ms
      low self st7735s-text-reset-pin @ pin!
      200 ms
      high self st7735s-text-reset-pin @ pin!
      200 ms
    ; define reset-st7735s-text

    \ Initialize the ST7735S-TEXT
    :noname { self -- }
      $11 self cmd>st7735s-text \ Sleep exit
      
      120 ms
      
      $21 self cmd>st7735s-text 
      $21 self cmd>st7735s-text 

      $B1 self cmd>st7735s-text
      $05 self data>st7735s-text
      $3A self data>st7735s-text
      $3A self data>st7735s-text

      $B2 self cmd>st7735s-text
      $05 self data>st7735s-text
      $3A self data>st7735s-text
      $3A self data>st7735s-text

      $B3 self cmd>st7735s-text 
      $05 self data>st7735s-text
      $3A self data>st7735s-text
      $3A self data>st7735s-text
      $05 self data>st7735s-text
      $3A self data>st7735s-text
      $3A self data>st7735s-text

      $B4 self cmd>st7735s-text
      $03 self data>st7735s-text

      $C0 self cmd>st7735s-text
      $62 self data>st7735s-text
      $02 self data>st7735s-text
      $04 self data>st7735s-text

      $C1 self cmd>st7735s-text
      $C0 self data>st7735s-text

      $C2 self cmd>st7735s-text
      $0D self data>st7735s-text
      $00 self data>st7735s-text

      $C3 self cmd>st7735s-text
      $8D self data>st7735s-text
      $6A self data>st7735s-text   

      $C4 self cmd>st7735s-text
      $BD self data>st7735s-text 
      $EE self data>st7735s-text

      $C5 self cmd>st7735s-text
      $0E self data>st7735s-text

      $E0 self cmd>st7735s-text
      $10 self data>st7735s-text
      $0E self data>st7735s-text
      $02 self data>st7735s-text
      $03 self data>st7735s-text
      $0E self data>st7735s-text
      $07 self data>st7735s-text
      $02 self data>st7735s-text
      $07 self data>st7735s-text
      $0A self data>st7735s-text
      $12 self data>st7735s-text
      $27 self data>st7735s-text
      $37 self data>st7735s-text
      $00 self data>st7735s-text
      $0D self data>st7735s-text
      $0E self data>st7735s-text
      $10 self data>st7735s-text

      $E1 self cmd>st7735s-text
      $10 self data>st7735s-text
      $0E self data>st7735s-text
      $03 self data>st7735s-text
      $03 self data>st7735s-text
      $0F self data>st7735s-text
      $06 self data>st7735s-text
      $02 self data>st7735s-text
      $08 self data>st7735s-text
      $0A self data>st7735s-text
      $13 self data>st7735s-text
      $26 self data>st7735s-text
      $36 self data>st7735s-text
      $00 self data>st7735s-text
      $0D self data>st7735s-text
      $0E self data>st7735s-text
      $10 self data>st7735s-text

      $3A self cmd>st7735s-text 
      $05 self data>st7735s-text

      $36 self cmd>st7735s-text
      $A8 self data>st7735s-text

      $29 self cmd>st7735s-text
      
    ; define init-st7735s-text

    \ Set the backlight pin
    :noname { backlight self -- }
      backlight self st7735s-text-backlight-pin @ pin!
    ; define backlight!
    
    \ Send a command to the ST7735S-TEXT
    :noname { W^ cmd self -- }
      low self st7735s-text-dc-pin @ pin!
      low self st7735s-text-cs-pin @ pin!
      cmd 1 self st7735s-text-device @ buffer>spi
    ; define cmd>st7735s-text

    \ Send 8 bits of data to the ST7735S-TEXT
    :noname { W^ data self -- }
      high self st7735s-text-dc-pin @ pin!
      low self st7735s-text-cs-pin @ pin!
      data 1 self st7735s-text-device @ buffer>spi
      high self st7735s-text-cs-pin @ pin!
    ; define data>st7735s-text

    \ Set the ST7735S-TEXT window
    :noname { start-col end-col start-row end-row self -- }
      $2A self cmd>st7735s-text
      0 self data>st7735s-text
      start-col 1+ self data>st7735s-text
      0 self data>st7735s-text
      end-col 1+ self data>st7735s-text
      $2B self cmd>st7735s-text
      0 self data>st7735s-text
      start-row 26 + self data>st7735s-text
      0 self data>st7735s-text
      end-row 26 + self data>st7735s-text
      $2C self cmd>st7735s-text
    ; define st7735s-text-window!

    \ Update a rectangular space on the ST7735S device
    :noname { start-col end-col start-row end-row self -- }
      start-col end-col start-row end-row self st7735s-text-window!
      high self st7735s-text-dc-pin @ pin!
      low self st7735s-text-cs-pin @ pin!
      end-row start-row ?do
        self start-col i end-col start-col - dup 1 lshift [:
          { self start-col row cols line-buf }
          self st7735s-text-fg-color @ self st7735s-text-bg-color @ { fg bg }
          0 begin dup cols < while
            start-col over + row self pixel@ if fg else bg then
            over 1 lshift line-buf + h!
            1+
          repeat
          drop
          line-buf cols 1 lshift self st7735s-text-device @ buffer>spi
        ;] with-aligned-allot
      loop
      high self st7735s-text-cs-pin @ pin!
    ; define update-area

    \ Update the ST7735S device
    :noname { self -- }
      self dirty? if
        self dirty-rect@ { start-col start-row end-col end-row }
        start-col end-col start-row end-row self update-area
        self clear-dirty
      then
    ; define update-display

    \ Set the foreground color and dirty the display
    :noname { fg-color self -- }
      fg-color self st7735s-text-fg-color !
      self set-dirty
    ; define fg-color!

    \ Set the background color and dirty the display
    :noname { bg-color self -- }
      bg-color self st7735s-text-bg-color !
      self set-dirty
    ; define bg-color!

    \ Get the foreground color
    :noname ( self -- ) st7735s-text-fg-color @ ; define fg-color@

    \ Get the background color
    :noname ( self -- ) st7735s-text-bg-color @ ; define bg-color@

  end-implement
  
end-module