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

begin-module st7789v-text-common

  oo import
  bitmap import
  font import
  text8 import
  text8-internal import
  pin import
  armv6m import

  begin-module st7789v-text-common-internal

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
    
  end-module> import
  
  <text8> begin-class <st7789v-text-common>

    continue-module st7789v-text-common-internal

      \ The font
      cell member st7789v-text-font

      \ The physical number of columns
      cell member st7789v-text-phys-cols
      
      \ The physical number of rows
      cell member st7789v-text-phys-rows

      \ Rotate setting
      cell member st7789v-text-rotate

      \ Column offset
      cell member st7789v-text-col-offset

      \ Row offset
      cell member st7789v-text-row-offset

      \ Round
      cell member st7789v-text-round

      \ DC pin
      cell member st7789v-text-dc-pin

      \ CS pin
      cell member st7789v-text-cs-pin

      \ Backlight pin
      cell member st7789v-text-backlight-pin

      \ DMA channel
      cell member st7789v-text-dma-channel
      
      \ Dirty rectangle start column
      cell member st7789v-text-dirty-start-col

      \ Dirty rectangle end column
      cell member st7789v-text-dirty-end-col

      \ Dirty rectangle start row
      cell member st7789v-text-dirty-start-row

      \ Dirty rectangle end row
      cell member st7789v-text-dirty-end-row

      \ Initialize the ST7789V-text
      method init-st7789v-text ( self -- )

      \ Write blocking data over DMA
      method >st7789v-text ( addr count self -- )

      \ Send a command to the ST7789V-text
      method cmd>st7789v-text ( cmd self -- )

      \ Send a command with arguments to the ST7789V-text
      method cmd-args>st7789v-text ( cmd addr count self -- )

      \ Send a byte of data to the ST7789V-text
      method data>st7789v-text ( data self -- )

      \ Set the ST7789V-text window
      method st7789v-text-window!
      ( start-col end-col start-row end-row self -- )

      \ Update a rectangular space on the ST7789V device
      method update-area ( start-col end-col start-row end-row self -- )

      \ Populate a row
      method populate-row ( start-col row cols line-buf self -- )
      
    end-module

    \ Get the character dimensions
    method char-dim@ ( self -- cols rows )

    \ Set the backlight
    method backlight! ( backlight self -- )
      
    \ Update the ST7789V-text device
    method update-display ( self -- )

    \ Clear the ST7789v-text device
    method clear-display ( self -- )
    
  end-class

  <st7789v-text-common> begin-implement

    \ Constructor
    :noname
      { dc cs backlight the-font buf round cols rows phys-cols phys-rows self }
      buf cols rows self <text8>->new
      phys-cols self st7789v-text-phys-cols !
      phys-rows self st7789v-text-phys-rows !
      the-font self st7789v-text-font !
      dc self st7789v-text-dc-pin !
      cs self st7789v-text-cs-pin !
      backlight self st7789v-text-backlight-pin !
      round self st7789v-text-round !
      0 self st7789v-text-col-offset !
      0 self st7789v-text-row-offset !
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
    
    \ Initialize the ST7789V-text
    :noname { self -- }
      REG_SWRESET self cmd>st7789v-text

      150 ms

      REG_TEON self cmd>st7789v-text
      REG_COLMOD s\" \x05" self cmd-args>st7789v-text

      REG_PORCTRL s\" \x0C\x0C\x00\x33\x33" self cmd-args>st7789v-text
      REG_LCMCTRL s\" \x2C" self cmd-args>st7789v-text
      REG_VDVVRHEN s\" \x01" self cmd-args>st7789v-text
      REG_VRHS s\" \x12" self cmd-args>st7789v-text
      REG_VDVS s\" \x20" self cmd-args>st7789v-text
      REG_PWCTRL1 s\" \xA4\xA1" self cmd-args>st7789v-text
      REG_FRCTRL2 s\" \x0F" self cmd-args>st7789v-text

      self st7789v-text-phys-cols @ { cols }
      self st7789v-text-phys-rows @ { rows }
      
      rows 240 = cols 240 = and if
        REG_GCTRL s\" \x14" self cmd-args>st7789v-text
        REG_VCOMS s\" \x37" self cmd-args>st7789v-text
        REG_GMCTRP1
        s\" \xD0\x04\x0D\x11\x13\x2B\x3F\x54\x4C\x18\x0D\x0B\x1F\x23"
        self cmd-args>st7789v-text
        REG_GMCTRN1
        s\" \xD0\x04\x0C\x11\x13\x2C\x3F\x44\x51\x2F\x1F\x1F\x20\x23"
        self cmd-args>st7789v-text
      then

      rows 240 = cols 320 = and if
        REG_GCTRL s\" \x3F" self cmd-args>st7789v-text
        REG_VCOMS s\" \x1F" self cmd-args>st7789v-text
        REG_GMCTRP1
        s\" \xD0\x08\x11\x08\x0C\x15\x39\x33\x50\x36\x13\x14\x29\x2D"
        self cmd-args>st7789v-text
        REG_GMCTRN1
        s\" \xD0\x08\x10\x08\x06\x06\x39\x44\x51\x0B\x16\x14\x2F\x31"
        self cmd-args>st7789v-text
      then

      rows 135 = cols 240 = and if
        REG_VRHS s\" \x00" self cmd-args>st7789v-text
        REG_GCTRL s\" \x7F" self cmd-args>st7789v-text
        REG_VCOMS s\" \x3D" self cmd-args>st7789v-text
        $D6 s\" \xA1" self cmd-args>st7789v-text
        REG_GMCTRP1
        s\" \x70\x04\x08\x09\x09\x05\x2A\x33\x41\x07\x13\x13\x29\x2F"
        self cmd-args>st7789v-text
        REG_GMCTRN1
        s\" \x70\x03\x09\x0A\x09\x06\x2B\x34\x41\x07\x12\x14\x28\x2E"
        self cmd-args>st7789v-text
      then

      REG_INVON self cmd>st7789v-text
      REG_SLPOUT self cmd>st7789v-text
      REG_DISPON self cmd>st7789v-text

      100 ms

      0 { W^ madctl }
      
      rows 240 = cols 240 = and if
        self st7789v-text-round @ if 40 else 80 then
        self st7789v-text-row-offset !
        0 self st7789v-text-col-offset !
        MADCTL_HORIZ_ORDER madctl !
      then
      
      rows 135 = cols 240 = and if
        40 self st7789v-text-col-offset !
        52 self st7789v-text-row-offset !
        [ MADCTL_COL_ORDER MADCTL_SWAP_XY or MADCTL_SCAN_ORDER or ] literal
        madctl !
      then
      
      rows 240 = cols 135 = and if
        52 self st7789v-text-col-offset !
        40 self st7789v-text-row-offset !
        0 madctl !
      then

      rows 240 = cols 320 = and if
        0 self st7789v-text-col-offset !
        0 self st7789v-text-row-offset !
        [ MADCTL_COL_ORDER MADCTL_SWAP_XY or MADCTL_SCAN_ORDER or ]
        literal madctl !
      then

      rows 320 = cols 240 = and if
        0 self st7789v-text-col-offset !
        0 self st7789v-text-row-offset !
        0 madctl !
      then

      REG_MADCTL madctl 1 self cmd-args>st7789v-text

      50 ms

      true self backlight!
    ; define init-st7789v-text

    \ Get the character dimensions
    :noname { self -- cols rows }
      self st7789v-text-font @ { the-font }
      the-font char-cols @ the-font char-rows @
    ; define char-dim@

    \ Set the backlight pin
    :noname { backlight self -- }
      backlight self st7789v-text-backlight-pin @ pin!
    ; define backlight!

    \ Send a command to the ST7789V-text
    :noname { W^ cmd self -- }
      low self st7789v-text-dc-pin @ pin!
      low self st7789v-text-cs-pin @ pin!
      cmd 1 self >st7789v-text
      high self st7789v-text-cs-pin @ pin!
    ; define cmd>st7789v-text

    \ Send a command with arguments to the ST7789V-text
    :noname { W^ cmd addr count self -- }
      low self st7789v-text-dc-pin @ pin!
      low self st7789v-text-cs-pin @ pin!
      cmd 1 self >st7789v-text
      high self st7789v-text-dc-pin @ pin!
      addr count self >st7789v-text
      high self st7789v-text-cs-pin @ pin!
    ; define cmd-args>st7789v-text

    \ Set the entire display to be dirty
    :noname { self -- }
      self dim@ { cols rows }
      0 self st7789v-text-dirty-start-col !
      cols self st7789v-text-dirty-end-col !
      0 self st7789v-text-dirty-start-row !
      rows self st7789v-text-dirty-end-row !
    ; define set-dirty

    \ Clear dirty rectangle
    :noname { self -- }
      0 self st7789v-text-dirty-start-col !
      0 self st7789v-text-dirty-end-col !
      0 self st7789v-text-dirty-start-row !
      0 self st7789v-text-dirty-end-row !
    ; define clear-dirty
    
    \ Get whether an ST7789V-text device is dirty
    :noname { self -- dirty? }
      self st7789v-text-dirty-start-col @ self st7789v-text-dirty-end-col @ <>
      self st7789v-text-dirty-start-row @ self st7789v-text-dirty-end-row @ <> and
    ; define dirty?
  
    \ Dirty a character on an ST7789V-text device
    :noname { col row self -- }
      self dirty? if
        row self st7789v-text-dirty-start-row @ min self st7789v-text-dirty-start-row !
        row 1+ self st7789v-text-dirty-end-row @ max self st7789v-text-dirty-end-row !
        col self st7789v-text-dirty-start-col @ min self st7789v-text-dirty-start-col !
        col 1+ self st7789v-text-dirty-end-col @ max self st7789v-text-dirty-end-col !
      else
        row self st7789v-text-dirty-start-row !
        row 1+ self st7789v-text-dirty-end-row !
        col self st7789v-text-dirty-start-col !
        col 1+ self st7789v-text-dirty-end-col !
      then
    ; define dirty-char

    \ Dirty an area on an ST7789V-text device
    :noname { start-col end-col start-row end-row self -- }
      start-col end-col < start-row end-row < and if
        start-col start-row self dirty-char
        end-col 1- end-row 1- self dirty-char
      then
    ; define dirty-area
    
    \ Set the ST7789V-text window
    :noname { start-col end-col start-row end-row self -- }
      self st7789v-text-col-offset @ +to start-col
      self st7789v-text-col-offset @ 1- +to end-col
      self st7789v-text-row-offset @ +to start-row
      self st7789v-text-row-offset @ 1- +to end-row
      0 0 { W^ col-values W^ row-values }
      start-col 8 rshift col-values c!
      start-col col-values 1 + c!
      end-col 8 rshift col-values 2 + c!
      end-col col-values 3 + c!
      start-row 8 rshift row-values c!
      start-row row-values 1 + c!
      end-row 8 rshift row-values 2 + c!
      end-row row-values 3 + c!
      REG_CASET col-values 4 self cmd-args>st7789v-text
      REG_RASET row-values 4 self cmd-args>st7789v-text
    ; define st7789v-text-window!

    \ Update a rectangular space on the ST7789V device
    :noname { start-col end-col start-row end-row self -- }
      self char-dim@ { char-cols char-rows }
      start-col char-cols * to start-col
      end-col char-cols * to end-col
      start-row char-rows * to start-row
      end-row char-rows * to end-row
      start-col end-col start-row end-row self st7789v-text-window!
      low self st7789v-text-dc-pin @ pin!
      low self st7789v-text-cs-pin @ pin!
      REG_RAMWR { W^ cmd }
      cmd 1 self >st7789v-text
      high self st7789v-text-dc-pin @ pin!
      self char-dim@ { char-cols char-rows }
      end-row start-row ?do
        self start-col i end-col start-col - dup 1 lshift [:
          { self start-col row cols line-buf }
          start-col row cols line-buf self populate-row
          line-buf cols 1 lshift self >st7789v-text
        ;] with-aligned-allot
      loop
      high self st7789v-text-cs-pin @ pin!
    ; define update-area

    \ Populate a row
    :noname { start-col row cols line-buf self -- }
      self st7789v-text-font @ { the-font }
      self dim@ { text-cols text-rows }
      self char-dim@ { char-cols char-rows }
      row char-rows u/mod { font-row text-row }
      self text-char-buf @ { char-buf }
      self text-fg-color-buf @ { fg-color-buf }
      self text-bk-color-buf @ { bk-color-buf }
      0 { col }
      start-col char-cols u/mod { font-col text-col }
      font-row 3 rshift the-font font-internal::font-bitmap
      bitmap-internal::page-addr { page }
      font-row 7 and bit { font-bit }
      text-cols text-row * text-col + { offset }
      char-buf offset + c@ the-font font-internal::find-char-col page + { addr }
      fg-color-buf offset + c@ 1 lshift 8-to-16-lookup + h@ { fg-color }
      bk-color-buf offset + c@ 1 lshift 8-to-16-lookup + h@ { bk-color }
      font-row char-rows 1- <> if
        begin col cols < while
          addr c@ font-bit and if fg-color else bk-color then
          col 1 lshift line-buf + h!
          1 +to col
          1 +to font-col
          1 +to addr
          font-col char-cols = if
            0 to font-col
            1 +to text-col
            1 +to offset
            text-col text-cols < if
              char-buf offset + c@ the-font font-internal::find-char-col page +
              to addr
              fg-color-buf offset + c@ 1 lshift 8-to-16-lookup + h@ to fg-color
              bk-color-buf offset + c@ 1 lshift 8-to-16-lookup + h@ to bk-color
            then
          then
        repeat
      else
        0 { col }
        text-col text-row self raw-underlined@ { underlined }
        begin col cols < while
          addr c@ font-bit and 0<> underlined or if fg-color else bk-color then
          col 1 lshift line-buf + h!
          1 +to col
          1 +to font-col
          1 +to addr
          font-col char-cols = if
            0 to font-col
            1 +to text-col
            1 +to offset
            text-col text-cols < if
              char-buf offset + c@ the-font font-internal::find-char-col page +
              to addr
              fg-color-buf offset + c@ 1 lshift 8-to-16-lookup + h@ to fg-color
              bk-color-buf offset + c@ 1 lshift 8-to-16-lookup + h@ to bk-color
              text-col text-row self raw-underlined@ to underlined
            then
          then
        repeat
      then
    ; define populate-row

    \ Clear the ST7789V-text device
    :noname { self -- }
      self st7789v-text-phys-cols @ { cols }
      self st7789v-text-phys-rows @ { rows }
      0 cols 0 rows self st7789v-text-window!
      low self st7789v-text-dc-pin @ pin!
      low self st7789v-text-cs-pin @ pin!
      REG_RAMWR { W^ cmd }
      cmd 1 self >st7789v-text
      high self st7789v-text-dc-pin @ pin!
      self rows cols dup 1 lshift [: { self rows cols line-buf }
        line-buf cols 1 lshift 0 fill
        rows 0 ?do line-buf cols 1 lshift self >st7789v-text loop
      ;] with-allot
      high self st7789v-text-cs-pin @ pin!
    ; define clear-display

    \ Update the ST7789V device
    :noname { self -- }
      self dirty? if 
        self st7789v-text-dirty-start-col @
        self st7789v-text-dirty-end-col @
        self st7789v-text-dirty-start-row @
        self st7789v-text-dirty-end-row @
        self update-area
        self clear-dirty
      then
    ; define update-display

  end-implement
  
end-module