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

begin-module st7735s-8

  oo import
  pixmap8 import
  pixmap8-internal import
  pin import
  spi import
  armv6m import

  begin-module st7735s-8-internal

    \ Convert an 8-bit color row to a 16-bit color row
    : convert-8-to-16 ( row-addr-8 row-addr-16 cols )
      code[
      r4 1 push
      r0 1 dp ldm
      r1 1 dp ldm
      mark<
      0 r6 cmp_,#_
      gt bc>
      r6 1 dp ldm
      r4 pc 2 pop
      >mark
      0 r1 r2 ldrb_,[_,#_]
      6 r2 r3 lsrs_,_,#_  \ Blue, 7:6
      11 r3 r3 lsls_,_,#_ \ to 12:11 (bits 10:8 are zero)
      26 r2 r4 lsls_,_,#_ \ Green, 6:3
      29 r4 r4 lsrs_,_,#_ \ to 2:0 (bits 15:13 are zero)
      r4 r3 orrs_,_
      29 r2 r4 lsls_,_,#_ \ Red, 2:0
      24 r4 r4 lsrs_,_,#_ \ to 7:5 (bits 4:3 are zero)
      r4 r3 orrs_,_
      0 r0 r3 strh_,[_,#_]
      1 r1 adds_,#_
      2 r0 adds_,#_
      1 r6 subs_,#_
      b<
      ]code
    ;
    
  end-module> import
  
  <pixmap8> begin-class <st7735s-8>

    continue-module st7735s-8-internal
      
      \ SPI device
      cell member st7735s-8-device

      \ Reset pin
      cell member st7735s-8-reset-pin

      \ DC pin
      cell member st7735s-8-dc-pin

      \ CS pin
      cell member st7735s-8-cs-pin

      \ Backlight pin
      cell member st7735s-8-backlight-pin

      \ CLK pin
      cell member st7735s-8-clk-pin

      \ DIN pin
      cell member st7735s-8-din-pin
      
      \ Dirty rectangle start column
      cell member st7735s-8-dirty-start-col

      \ Dirty rectangle end column
      cell member st7735s-8-dirty-end-col

      \ Dirty rectangle start row
      cell member st7735s-8-dirty-start-row

      \ Dirty rectangle end row
      cell member st7735s-8-dirty-end-row

      \ Reset the ST7735S-8
      method reset-st7735s-8 ( self -- )

      \ Initialize the ST7735S-8
      method init-st7735s-8 ( self -- )

      \ Send a command to the ST7735S-8
      method cmd>st7735s-8 ( cmd self -- )

      \ Send a byte of data to the ST7735S-8
      method data>st7735s-8 ( data self -- )

      \ Set the ST7735S-8 window
      method st7735s-8-window! ( start-col end-col start-row end-row self -- )

      \ Update a rectangular space on the ST7735S device
      method update-area ( start-col end-col start-row end-row self -- )
      
    end-module

    \ Set the backlight
    method backlight! ( backlight self -- )
      
    \ Update the ST7735S-8 device
    method update-display ( self -- )
    
  end-class

  <st7735s-8> begin-implement

    \ Constructor
    :noname { din clk dc cs backlight reset buf cols rows device self -- }
      buf cols rows self <pixmap8>->new
      device self st7735s-8-device !
      reset self st7735s-8-reset-pin !
      dc self st7735s-8-dc-pin !
      cs self st7735s-8-cs-pin !
      backlight self st7735s-8-backlight-pin !
      clk self st7735s-8-clk-pin !
      din self st7735s-8-din-pin !
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
      self reset-st7735s-8
      self init-st7735s-8
      0 cols 0 rows self st7735s-8-window!
    ; define new

    \ Reset the ST7735S-8
    :noname { self -- }
      high self st7735s-8-reset-pin @ pin!
      200 ms
      low self st7735s-8-reset-pin @ pin!
      200 ms
      high self st7735s-8-reset-pin @ pin!
      200 ms
    ; define reset-st7735s-8

    \ Initialize the ST7735S-8
    :noname { self -- }
      $11 self cmd>st7735s-8 \ Sleep exit
      
      120 ms
      
      $21 self cmd>st7735s-8 
      $21 self cmd>st7735s-8 

      $B1 self cmd>st7735s-8
      $05 self data>st7735s-8
      $3A self data>st7735s-8
      $3A self data>st7735s-8

      $B2 self cmd>st7735s-8
      $05 self data>st7735s-8
      $3A self data>st7735s-8
      $3A self data>st7735s-8

      $B3 self cmd>st7735s-8 
      $05 self data>st7735s-8
      $3A self data>st7735s-8
      $3A self data>st7735s-8
      $05 self data>st7735s-8
      $3A self data>st7735s-8
      $3A self data>st7735s-8

      $B4 self cmd>st7735s-8
      $03 self data>st7735s-8

      $C0 self cmd>st7735s-8
      $62 self data>st7735s-8
      $02 self data>st7735s-8
      $04 self data>st7735s-8

      $C1 self cmd>st7735s-8
      $C0 self data>st7735s-8

      $C2 self cmd>st7735s-8
      $0D self data>st7735s-8
      $00 self data>st7735s-8

      $C3 self cmd>st7735s-8
      $8D self data>st7735s-8
      $6A self data>st7735s-8   

      $C4 self cmd>st7735s-8
      $BD self data>st7735s-8 
      $EE self data>st7735s-8

      $C5 self cmd>st7735s-8
      $0E self data>st7735s-8

      $E0 self cmd>st7735s-8
      $10 self data>st7735s-8
      $0E self data>st7735s-8
      $02 self data>st7735s-8
      $03 self data>st7735s-8
      $0E self data>st7735s-8
      $07 self data>st7735s-8
      $02 self data>st7735s-8
      $07 self data>st7735s-8
      $0A self data>st7735s-8
      $12 self data>st7735s-8
      $27 self data>st7735s-8
      $37 self data>st7735s-8
      $00 self data>st7735s-8
      $0D self data>st7735s-8
      $0E self data>st7735s-8
      $10 self data>st7735s-8

      $E1 self cmd>st7735s-8
      $10 self data>st7735s-8
      $0E self data>st7735s-8
      $03 self data>st7735s-8
      $03 self data>st7735s-8
      $0F self data>st7735s-8
      $06 self data>st7735s-8
      $02 self data>st7735s-8
      $08 self data>st7735s-8
      $0A self data>st7735s-8
      $13 self data>st7735s-8
      $26 self data>st7735s-8
      $36 self data>st7735s-8
      $00 self data>st7735s-8
      $0D self data>st7735s-8
      $0E self data>st7735s-8
      $10 self data>st7735s-8

      $3A self cmd>st7735s-8 
      $05 self data>st7735s-8

      $36 self cmd>st7735s-8
      $A8 self data>st7735s-8

      $29 self cmd>st7735s-8
      
    ; define init-st7735s-8

    \ Set the backlight pin
    :noname { backlight self -- }
      backlight self st7735s-8-backlight-pin @ pin!
    ; define backlight!
    
    \ Send a command to the ST7735S-8
    :noname { W^ cmd self -- }
      low self st7735s-8-dc-pin @ pin!
      low self st7735s-8-cs-pin @ pin!
      cmd 1 self st7735s-8-device @ buffer>spi
    ; define cmd>st7735s-8

    \ Send 8 bits of data to the ST7735S-8
    :noname { W^ data self -- }
      high self st7735s-8-dc-pin @ pin!
      low self st7735s-8-cs-pin @ pin!
      data 1 self st7735s-8-device @ buffer>spi
      high self st7735s-8-cs-pin @ pin!
    ; define data>st7735s-8

    \ Set the entire display to be dirty
    :noname { self -- }
      0 self st7735s-8-dirty-start-col !
      self pixmap-cols @ self st7735s-8-dirty-end-col !
      0 self st7735s-8-dirty-start-row !
      self pixmap-rows @ self st7735s-8-dirty-end-row !
    ; define set-dirty

    \ Clear dirty rectangle
    :noname { self -- }
      0 self st7735s-8-dirty-start-col !
      0 self st7735s-8-dirty-end-col !
      0 self st7735s-8-dirty-start-row !
      0 self st7735s-8-dirty-end-row !
    ; define clear-dirty
    
    \ Get whether an ST7735S-8 device is dirty
    :noname { self -- dirty? }
      self st7735s-8-dirty-start-col @ self st7735s-8-dirty-end-col @ <>
      self st7735s-8-dirty-start-row @ self st7735s-8-dirty-end-row @ <> and
    ; define dirty?
  
    \ Dirty a pixel on an ST7735S-8 device
    :noname { col row self -- }
      self dirty? if
        row self st7735s-8-dirty-start-row @ min self st7735s-8-dirty-start-row !
        row 1+ self st7735s-8-dirty-end-row @ max self st7735s-8-dirty-end-row !
        col self st7735s-8-dirty-start-col @ min self st7735s-8-dirty-start-col !
        col 1+ self st7735s-8-dirty-end-col @ max self st7735s-8-dirty-end-col !
      else
        row self st7735s-8-dirty-start-row !
        row 1+ self st7735s-8-dirty-end-row !
        col self st7735s-8-dirty-start-col !
        col 1+ self st7735s-8-dirty-end-col !
      then
    ; define dirty-pixel

    \ Dirty an area on an ST7735S-8 device
    :noname { start-col end-col start-row end-row self -- }
      start-col end-col < start-row end-row < and if
        start-col start-row self dirty-pixel
        end-col 1- end-row 1- self dirty-pixel
      then
    ; define dirty-area
    
    \ Set the ST7735S-8 window
    :noname { start-col end-col start-row end-row self -- }
      $2A self cmd>st7735s-8
      0 self data>st7735s-8
      start-col 1+ self data>st7735s-8
      0 self data>st7735s-8
      end-col 1+ self data>st7735s-8
      $2B self cmd>st7735s-8
      0 self data>st7735s-8
      start-row 26 + self data>st7735s-8
      0 self data>st7735s-8
      end-row 26 + self data>st7735s-8
      $2C self cmd>st7735s-8
    ; define st7735s-8-window!

    \ Update a rectangular space on the ST7735S device
    :noname { start-col end-col start-row end-row self -- }
      start-col end-col start-row end-row self st7735s-8-window!
      high self st7735s-8-dc-pin @ pin!
      low self st7735s-8-cs-pin @ pin!
      end-row 1+ start-row ?do
        self start-col i end-col start-col - 1+ dup 1 lshift [:
          { self start-col row cols line-buf }
          start-col row self pixel-addr line-buf cols convert-8-to-16
          line-buf cols 1 lshift self st7735s-8-device @ buffer>spi
        ;] with-aligned-allot
      loop
      high self st7735s-8-cs-pin @ pin!
    ; define update-area

    \ Update the ST7735S device
    :noname { self -- }
      self dirty? if 
        self st7735s-8-dirty-start-col @
        self st7735s-8-dirty-end-col @
        self st7735s-8-dirty-start-row @
        self st7735s-8-dirty-end-row @
        self update-area
        self clear-dirty
      then
    ; define update-display

  end-implement
  
end-module