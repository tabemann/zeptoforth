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

begin-module sh1122-text

  oo import
  text-display import
  pin import
  spi import

  <text-display> begin-class <sh1122-text>

    begin-module sh1122-text-internal
      
      \ SPI device
      cell member sh1122-text-device

      \ Reset pin
      cell member sh1122-text-reset-pin

      \ DC pin
      cell member sh1122-text-dc-pin

      \ CS pin
      cell member sh1122-text-cs-pin

      \ CLK pin
      cell member sh1122-text-clk-pin

      \ DIN pin
      cell member sh1122-text-din-pin

      \ Foreground color (grayscale from 0 to 15)
      cell member sh1122-text-fg-gray

      \ Background color (grayscale from 0 to 15)
      cell member sh1122-text-bg-gray

      \ Display columns
      cell member sh1122-text-cols

      \ Display rows
      cell member sh1122-text-rows
      
      \ Reset the SH1122
      method reset-sh1122-text ( self -- )

      \ Initialize the SH1122
      method init-sh1122-text ( self -- )

      \ Erase the SH1122
      method erase-sh1122-text ( self -- )
      
      \ Start a transfer
      method start-sh1122-transfer ( self -- )

      \ End a transfer
      method end-sh1122-transfer ( self -- )

      \ Send a command to the SH1122
      method cmd>sh1122-text ( cmd self -- )

      \ Send a command and argument to the SH1122
      method cmd-arg>sh1122-text ( cmd arg self -- )

      \ Update a rectangular space on the SH1122 device
      method update-area ( start-col end-col start-row end-row self -- )
      
    end-module> import

    \ Update the SH1122 device
    method update-display ( self -- )
    
    \ Change the SH1122 device contrast
    method display-contrast! ( contrast self -- )

    \ Set the foreground color and dirty the display
    method fg-gray! ( fg-gray self -- )

    \ Set the background color and dirty the display
    method bg-gray! ( bg-gray self -- )

    \ Get the foreground color
    method fg-gray@ ( self -- fg-gray )

    \ Get the foreground color
    method bg-gray@ ( self -- bg-gray )

  end-class

  <sh1122-text> begin-implement

    \ Constructor
    :noname
      { fg bg din clk dc cs reset tbuf ibuf font cols rows device self -- }
      tbuf ibuf font cols rows self <text-display>->new
      device self sh1122-text-device !
      reset self sh1122-text-reset-pin !
      dc self sh1122-text-dc-pin !
      cs self sh1122-text-cs-pin !
      clk self sh1122-text-clk-pin !
      din self sh1122-text-din-pin !
      fg self sh1122-text-fg-gray !
      bg self sh1122-text-bg-gray !
      cols self sh1122-text-cols !
      rows self sh1122-text-rows !
      dc output-pin
      cs output-pin
      low dc pin!
      high cs pin!
      reset output-pin
      low reset pin!
      device clk spi-pin
      device din spi-pin
      device master-spi
      40000000 device spi-baud!
      8 device spi-data-size!
      false false device motorola-spi
      device enable-spi
      self reset-sh1122-text
      self init-sh1122-text
      self erase-sh1122-text
    ; define new

    \ Reset the SH1122
    :noname { self -- }
      high self sh1122-text-reset-pin @ pin!
      200 ms
      low self sh1122-text-reset-pin @ pin!
      200 ms
      high self sh1122-text-reset-pin @ pin!
      200 ms
    ; define reset-sh1122-text

    \ Initialize the SH1122
    :noname { self -- }
      120 ms

      self start-sh1122-transfer
      
      120 ms

      $AE self cmd>sh1122-text \ display off
      $40 self cmd>sh1122-text \ display start line
      $A0 self cmd>sh1122-text \ remap
      $C0 self cmd>sh1122-text \ remap
      $81 $80 self cmd>sh1122-text \ set display contrast
      $A8 $3F self cmd>sh1122-text \ multiplex ratio 1/64 Duty (0x0F~0x3F)
      $AD $81 self cmd>sh1122-text \ use buildin DC-DC with 0.6 * 500 kHz
      $D5 $50 self cmd>sh1122-text \ set display clock divide ratio (lower 4 bit)/oscillator frequency (upper 4 bit)
      $D3 $00 self cmd>sh1122-text \ display offset, shift mapping ram counter
      $D9 $22 self cmd>sh1122-text \ pre charge (lower 4 bit) and discharge(higher 4 bit) period
      $DB $35 self cmd>sh1122-text \ VCOM deselect level
      $DC $35 self cmd>sh1122-text \ Pre Charge output voltage
      $30 self cmd>sh1122-text \ discharge level

      100 ms

      self end-sh1122-transfer
      
    ; define init-sh1122-text

    \ Start a transfer
    :noname ( self -- )
      low swap sh1122-text-cs-pin @ pin!
    ; define start-sh1122-transfer

    \ End a transfer
    :noname ( self -- )
      high swap sh1122-text-cs-pin @ pin!
    ; define end-sh1122-transfer

    \ Send a command to the SH1122
    :noname { W^ cmd self -- }
      low self sh1122-text-dc-pin @ pin!
      cmd 1 self sh1122-text-device @ buffer>spi
    ; define cmd>sh1122-text

    \ Send a command and argument to the SH1122
    :noname { W^ cmd arg self -- }
      cmd @ arg 8 lshift or cmd !
      low self sh1122-text-dc-pin @ pin!
      cmd 2 self sh1122-text-device @ buffer>spi
    ; define cmd-arg>sh1122-text
    
    \ Erase the SH1122
    :noname { self -- }
      self start-sh1122-transfer
      self sh1122-text-rows @ 0 ?do
        self i self sh1122-text-cols @ dup 1 rshift [:
          { self row cols line-buf }
          $B0 row self cmd-arg>sh1122-text
          0 self cmd>sh1122-text
          $10 self cmd>sh1122-text
          self sh1122-text-bg-gray @ dup 4 lshift or { gray }
          0 begin dup cols < while
            gray over 1 rshift line-buf + c!
            2 +
          repeat
          drop
          high self sh1122-text-dc-pin @ pin!
          line-buf cols 1 rshift self sh1122-text-device @ buffer>spi
          low self sh1122-text-dc-pin @ pin!
        ;] with-aligned-allot
      loop
      $AF self cmd>sh1122-text \ set display on
      self end-sh1122-transfer
    ; define erase-sh1122-text

    \ Update a rectangular space on the SH1122 device
    :noname { start-col end-col start-row end-row self -- }
      start-col $1 and if
        start-col $1 bic to start-col
      then
      end-col $1 and if
        end-col 1+ to end-col
      then
      self start-sh1122-transfer
      end-row start-row ?do
        self start-col i end-col start-col - 2 align dup 1 rshift [:
          { self start-col row cols line-buf }
          $B0 row self cmd-arg>sh1122-text
          start-col 1 rshift $F and self cmd>sh1122-text
          start-col 5 rshift $10 or self cmd>sh1122-text
          self sh1122-text-fg-gray @ self sh1122-text-bg-gray @ { fg bg }
          0 begin dup cols < while
            start-col over + row self pixel@ if fg else bg then 4 lshift
            start-col 2 pick + 1+ row self pixel@ if fg else bg then or
            over 1 rshift line-buf + c!
            2 +
          repeat
          drop
          high self sh1122-text-dc-pin @ pin!
          line-buf cols 1 rshift self sh1122-text-device @ buffer>spi
          low self sh1122-text-dc-pin @ pin!
        ;] with-aligned-allot
      loop
      self end-sh1122-transfer
    ; define update-area

    \ Update the SH1122 device
    :noname { self -- }
      self dirty? if
        self dirty-rect@ { start-col start-row end-col end-row }
        start-col end-col start-row end-row self update-area
        self clear-dirty
      then
    ; define update-display

    \ Change the SH1122 device contrast
    :noname { contrast self -- }
      self start-sh1122-transfer
      $81 contrast $FF and self cmd-arg>sh1122-text
      self end-sh1122-transfer
    ; define display-contrast!

    \ Set the foreground color and dirty the display
    :noname { fg-gray self -- }
      fg-gray $F and self sh1122-text-fg-gray !
      self set-dirty
    ; define fg-gray!

    \ Set the background color and dirty the display
    :noname { bg-gray self -- }
      bg-gray $F and self sh1122-text-bg-gray !
      self set-dirty
    ; define bg-gray!

    \ Get the foreground color
    :noname ( self -- ) sh1122-text-fg-gray @ ; define fg-gray@

    \ Get the background color
    :noname ( self -- ) sh1122-text-bg-gray @ ; define bg-gray@

  end-implement
  
end-module