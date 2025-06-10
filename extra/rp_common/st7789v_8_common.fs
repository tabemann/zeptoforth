\ Copyright (c) 2023-2025 Travis Bemann
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

begin-module st7789v-8-common

  oo import
  pixmap8 import
  pixmap8-internal import
  pin import
  armv6m import

  begin-module st7789v-8-common-internal

    \ ST7789V MADCTL bits
    %10000000 constant MADCTL_ROW_ORDER
    %01000000 constant MADCTL_COL_ORDER
    %00100000 constant MADCTL_SWAP_XY
    %00010000 constant MADCTL_SCAN_ORDER
    %00001000 constant MADCTL_RGB_BGR
    %00000100 constant MADCTL_HORIZ_ORDER

    \ ST7789V registers
    $01 constant REG_SWRESET
    $34 constant REG_TEOFF
    $35 constant REG_TEON
    $36 constant REG_MADCTL
    $3A constant REG_COLMOD
    $B7 constant REG_GCTRL
    $BB constant REG_VCOMS
    $C0 constant REG_LCMCTRL
    $C2 constant REG_VDVVRHEN
    $C3 constant REG_VRHS
    $C4 constant REG_VDVS
    $C6 constant REG_FRCTRL2
    $D0 constant REG_PWCTRL1
    $B2 constant REG_PORCTRL
    $E0 constant REG_GMCTRP1
    $E1 constant REG_GMCTRN1
    $20 constant REG_INVOFF
    $11 constant REG_SLPOUT
    $29 constant REG_DISPON
    $26 constant REG_GAMSET
    $28 constant REG_DISPOFF
    $2C constant REG_RAMWR
    $21 constant REG_INVON
    $2A constant REG_CASET
    $2B constant REG_RASET
    $CC constant REG_PWMFRSEL

    \ Convert an 8-bit color to a 16-bit color
    : do-convert-8-to-16 { color8 -- color16 }
      color8 $C0 and dup $40 and 0<> $3F and or { b }
      color8 $38 and 2 lshift dup $20 and 0<> $1F and or { g }
      color8 $07 and 5 lshift dup $20 and 0<> $1F and or { r }
      b 3 rshift 8 lshift
      g 5 rshift or
      g 2 rshift $7 and 13 lshift or
      r $F8 and or
    ;

    \ Generate a color lookup table
    : create-8-to-16-lookup ( -- ) 256 0 ?do i do-convert-8-to-16 h, loop ;

    \ The color lookup table
    create 8-to-16-lookup create-8-to-16-lookup cell align,

    \ Convert an 8-bit color row to a 16-bit color row
    : convert-8-to-16 ( row-addr-8 row-addr-16 cols )
      8-to-16-lookup
      code[
      r0 1 dp ldm
      r1 1 dp ldm
      r2 1 dp ldm
      1 r0 r3 lsls_,_,#_
      r3 r1 r1 adds_,_,_
      0 r0 cmp_,#_
      ne bc>
      tos 1 dp ldm
      pc 1 pop
      >mark
      mark<
      2 r1 subs_,#_
      1 r0 subs_,#_
      r0 r2 r3 ldrb_,[_,_]
      1 r3 r3 lsls_,_,#_
      r3 tos r3 ldrh_,[_,_]
      0 r1 r3 strh_,[_,#_]
      0 r0 cmp_,#_
      ne bc<
      tos 1 dp ldm
      ]code
    ;
    
  end-module> import
  
  <pixmap8> begin-class <st7789v-8-common>

    continue-module st7789v-8-common-internal
      
      \ Rotate setting
      cell member st7789v-8-rotate

      \ Column offset
      cell member st7789v-8-col-offset

      \ Row offset
      cell member st7789v-8-row-offset

      \ Round
      cell member st7789v-8-round

      \ DC pin
      cell member st7789v-8-dc-pin

      \ CS pin
      cell member st7789v-8-cs-pin

      \ Backlight pin
      cell member st7789v-8-backlight-pin

      \ DMA channel
      cell member st7789v-8-dma-channel
      
      \ Dirty rectangle start column
      cell member st7789v-8-dirty-start-col

      \ Dirty rectangle end column
      cell member st7789v-8-dirty-end-col

      \ Dirty rectangle start row
      cell member st7789v-8-dirty-start-row

      \ Dirty rectangle end row
      cell member st7789v-8-dirty-end-row

      \ Initialize the ST7789V-8
      method init-st7789v-8 ( self -- )

      \ Write blocking data over DMA
      method >st7789v-8 ( addr count self -- )

      \ Send a command to the ST7789V-8
      method cmd>st7789v-8 ( cmd self -- )

      \ Send a command with arguments to the ST7789V-8
      method cmd-args>st7789v-8 ( cmd addr count self -- )

      \ Send a byte of data to the ST7789V-8
      method data>st7789v-8 ( data self -- )

      \ Set the ST7789V-8 window
      method st7789v-8-window! ( start-col end-col start-row end-row self -- )

      \ Update a rectangular space on the ST7789V device
      method update-area ( start-col end-col start-row end-row self -- )
      
    end-module

    \ Set the backlight
    method backlight! ( backlight self -- )
      
    \ Update the ST7789V-8 device
    method update-display ( self -- )
    
  end-class

  <st7789v-8-common> begin-implement

    \ Constructor
    :noname
      { self }
      { dc cs backlight buf round cols rows }
      buf cols rows self <pixmap8>->new
      dc self st7789v-8-dc-pin !
      cs self st7789v-8-cs-pin !
      backlight self st7789v-8-backlight-pin !
      round self st7789v-8-round !
      0 self st7789v-8-col-offset !
      0 self st7789v-8-row-offset !
      dc output-pin
      cs output-pin
      backlight output-pin
      high dc pin!
      high cs pin!
      high backlight pin!
    ; define new

    \ Destructor
    :noname { self -- }
      self <object>->destroy
    ; define destroy
    
    \ Initialize the ST7789V-8
    :noname { self -- }
      REG_SWRESET self cmd>st7789v-8

      150 ms

      REG_TEON self cmd>st7789v-8
      REG_COLMOD s\" \x05" self cmd-args>st7789v-8

      REG_PORCTRL s\" \x0C\x0C\x00\x33\x33" self cmd-args>st7789v-8
      REG_LCMCTRL s\" \x2C" self cmd-args>st7789v-8
      REG_VDVVRHEN s\" \x01" self cmd-args>st7789v-8
      REG_VRHS s\" \x12" self cmd-args>st7789v-8
      REG_VDVS s\" \x20" self cmd-args>st7789v-8
      REG_PWCTRL1 s\" \xA4\xA1" self cmd-args>st7789v-8
      REG_FRCTRL2 s\" \x0F" self cmd-args>st7789v-8

      self dim@ 240 = swap 240 = and if
        REG_GCTRL s\" \x14" self cmd-args>st7789v-8
        REG_VCOMS s\" \x37" self cmd-args>st7789v-8
        REG_GMCTRP1
        s\" \xD0\x04\x0D\x11\x13\x2B\x3F\x54\x4C\x18\x0D\x0B\x1F\x23"
        self cmd-args>st7789v-8
        REG_GMCTRN1
        s\" \xD0\x04\x0C\x11\x13\x2C\x3F\x44\x51\x2F\x1F\x1F\x20\x23"
        self cmd-args>st7789v-8
      then

      self dim@ 240 = swap 320 = and if
        REG_GCTRL s\" \x3F" self cmd-args>st7789v-8
        REG_VCOMS s\" \x1F" self cmd-args>st7789v-8
        REG_GMCTRP1
        s\" \xD0\x08\x11\x08\x0C\x15\x39\x33\x50\x36\x13\x14\x29\x2D"
        self cmd-args>st7789v-8
        REG_GMCTRN1
        s\" \xD0\x08\x10\x08\x06\x06\x39\x44\x51\x0B\x16\x14\x2F\x31"
        self cmd-args>st7789v-8
      then

      self dim@ 135 = swap 240 = and if
        REG_VRHS s\" \x00" self cmd-args>st7789v-8
        REG_GCTRL s\" \x7F" self cmd-args>st7789v-8
        REG_VCOMS s\" \x3D" self cmd-args>st7789v-8
        $D6 s\" \xA1" self cmd-args>st7789v-8
        REG_GMCTRP1
        s\" \x70\x04\x08\x09\x09\x05\x2A\x33\x41\x07\x13\x13\x29\x2F"
        self cmd-args>st7789v-8
        REG_GMCTRN1
        s\" \x70\x03\x09\x0A\x09\x06\x2B\x34\x41\x07\x12\x14\x28\x2E"
        self cmd-args>st7789v-8
      then

      REG_INVON self cmd>st7789v-8
      REG_SLPOUT self cmd>st7789v-8
      REG_DISPON self cmd>st7789v-8

      100 ms

      0 { W^ madctl }
      
      self dim@ 240 = swap 240 = and if
        self st7789v-8-round @ if 40 else 80 then
        self st7789v-8-row-offset !
        0 self st7789v-8-col-offset !
        MADCTL_HORIZ_ORDER madctl !
      then
      
      self dim@ 135 = swap 240 = and if
        40 self st7789v-8-col-offset !
        52 self st7789v-8-row-offset !
        [ MADCTL_COL_ORDER MADCTL_SWAP_XY or MADCTL_SCAN_ORDER or ] literal
        madctl !
      then
      
      self dim@ 240 = swap 135 = and if
        52 self st7789v-8-col-offset !
        40 self st7789v-8-row-offset !
        0 madctl !
      then

      self dim@ 240 = swap 320 = and if
        0 self st7789v-8-col-offset !
        0 self st7789v-8-row-offset !
        [ MADCTL_COL_ORDER MADCTL_SWAP_XY or MADCTL_SCAN_ORDER or ]
        literal madctl !
      then

      self dim@ 320 = swap 240 = and if
        0 self st7789v-8-col-offset !
        0 self st7789v-8-row-offset !
        0 madctl !
      then

      REG_MADCTL madctl 1 self cmd-args>st7789v-8

      50 ms

      true self backlight!
    ; define init-st7789v-8

    \ Set the backlight pin
    :noname { backlight self -- }
      backlight self st7789v-8-backlight-pin @ pin!
    ; define backlight!

    \ Send a command to the ST7789V-8
    :noname { W^ cmd self -- }
      low self st7789v-8-dc-pin @ pin!
      low self st7789v-8-cs-pin @ pin!
      cmd 1 self >st7789v-8
      high self st7789v-8-cs-pin @ pin!
    ; define cmd>st7789v-8

    \ Send a command with arguments to the ST7789V-8
    :noname { W^ cmd addr count self -- }
      low self st7789v-8-dc-pin @ pin!
      low self st7789v-8-cs-pin @ pin!
      cmd 1 self >st7789v-8
      high self st7789v-8-dc-pin @ pin!
      addr count self >st7789v-8
      high self st7789v-8-cs-pin @ pin!
    ; define cmd-args>st7789v-8

    \ Set the entire display to be dirty
    :noname { self -- }
      0 self st7789v-8-dirty-start-col !
      self pixmap-cols @ self st7789v-8-dirty-end-col !
      0 self st7789v-8-dirty-start-row !
      self pixmap-rows @ self st7789v-8-dirty-end-row !
    ; define set-dirty

    \ Clear dirty rectangle
    :noname { self -- }
      0 self st7789v-8-dirty-start-col !
      0 self st7789v-8-dirty-end-col !
      0 self st7789v-8-dirty-start-row !
      0 self st7789v-8-dirty-end-row !
    ; define clear-dirty
    
    \ Get whether an ST7789V-8 device is dirty
    :noname { self -- dirty? }
      self st7789v-8-dirty-start-col @ self st7789v-8-dirty-end-col @ <>
      self st7789v-8-dirty-start-row @ self st7789v-8-dirty-end-row @ <> and
    ; define dirty?
  
    \ Dirty a pixel on an ST7789V-8 device
    :noname { col row self -- }
      self dirty? if
        row self st7789v-8-dirty-start-row @ min self st7789v-8-dirty-start-row !
        row 1+ self st7789v-8-dirty-end-row @ max self st7789v-8-dirty-end-row !
        col self st7789v-8-dirty-start-col @ min self st7789v-8-dirty-start-col !
        col 1+ self st7789v-8-dirty-end-col @ max self st7789v-8-dirty-end-col !
      else
        row self st7789v-8-dirty-start-row !
        row 1+ self st7789v-8-dirty-end-row !
        col self st7789v-8-dirty-start-col !
        col 1+ self st7789v-8-dirty-end-col !
      then
    ; define dirty-pixel

    \ Dirty an area on an ST7789V-8 device
    :noname { start-col end-col start-row end-row self -- }
      start-col end-col < start-row end-row < and if
        start-col start-row self dirty-pixel
        end-col 1- end-row 1- self dirty-pixel
      then
    ; define dirty-area
    
    \ Set the ST7789V-8 window
    :noname { start-col end-col start-row end-row self -- }
      self st7789v-8-col-offset @ +to start-col
      self st7789v-8-col-offset @ 1- +to end-col
      self st7789v-8-row-offset @ +to start-row
      self st7789v-8-row-offset @ 1- +to end-row
      0 0 { W^ col-values W^ row-values }
      start-col 8 rshift col-values c!
      start-col col-values 1 + c!
      end-col 8 rshift col-values 2 + c!
      end-col col-values 3 + c!
      start-row 8 rshift row-values c!
      start-row row-values 1 + c!
      end-row 8 rshift row-values 2 + c!
      end-row row-values 3 + c!
      REG_CASET col-values 4 self cmd-args>st7789v-8
      REG_RASET row-values 4 self cmd-args>st7789v-8
    ; define st7789v-8-window!

    \ Update a rectangular space on the ST7789V device
    :noname { start-col end-col start-row end-row self -- }
      start-col end-col start-row end-row self st7789v-8-window!
      low self st7789v-8-dc-pin @ pin!
      low self st7789v-8-cs-pin @ pin!
      REG_RAMWR { W^ cmd }
      cmd 1 self >st7789v-8
      high self st7789v-8-dc-pin @ pin!
      end-row start-row ?do
        self start-col i end-col start-col - dup 1 lshift [:
          { self start-col row cols line-buf }
          start-col row self pixel-addr line-buf cols convert-8-to-16
          line-buf cols 1 lshift self >st7789v-8
        ;] with-aligned-allot
      loop
      high self st7789v-8-cs-pin @ pin!
    ; define update-area

    \ Update the ST7789V device
    :noname { self -- }
      self dirty? if 
        self st7789v-8-dirty-start-col @
        self st7789v-8-dirty-end-col @
        self st7789v-8-dirty-start-row @
        self st7789v-8-dirty-end-row @
        self update-area
        self clear-dirty
      then
    ; define update-display

  end-implement
  
end-module