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

begin-module st7365p-text-common

  oo import
  bitmap import
  font import
  text8 import
  text8-internal import
  pin import
  armv6m import

  begin-module st7365p-text-common-internal

    \ ST7365P registers
    $01 constant REG_SWRESET
    $11 constant REG_SLPOUT
    $20 constant REG_INVOFF
    $21 constant REG_INVON
    $29 constant REG_DISPON
    $2A constant REG_CASET
    $2B constant REG_RASET
    $2C constant REG_RAMWR
    $35 constant REG_TEON
    $36 constant REG_MADCTL
    $3A constant REG_COLMOD
    $B1 constant REG_FRMCTR1
    $B4 constant REG_INVCTR
    $B7 constant REG_ETMOD
    $B9 constant REG_CECTRL1
    $C0 constant REG_PWCTR1
    $C1 constant REG_PWCTR2
    $C2 constant REG_PWCTR3
    $C5 constant REG_VMCTR1
    $E0 constant REG_PGAMCTRL
    $E1 constant REG_NGAMCTRL
    
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
  
  <text8> begin-class <st7365p-text-common>

    continue-module st7365p-text-common-internal

      \ The font
      cell member st7365p-text-font

      \ The physical number of columns
      cell member st7365p-text-phys-cols
      
      \ The physical number of rows
      cell member st7365p-text-phys-rows

      \ Inversion enabled setting
      cell member st7365p-text-invert

      \ Column offset
      cell member st7365p-text-col-offset

      \ Row offset
      cell member st7365p-text-row-offset

      \ DC pin
      cell member st7365p-text-dc-pin

      \ CS pin
      cell member st7365p-text-cs-pin

      \ Dirty rectangle start column
      cell member st7365p-text-dirty-start-col

      \ Dirty rectangle end column
      cell member st7365p-text-dirty-end-col

      \ Dirty rectangle start row
      cell member st7365p-text-dirty-start-row

      \ Dirty rectangle end row
      cell member st7365p-text-dirty-end-row

      \ Initialize the ST7365P-text
      method init-st7365p-text ( self -- )

      \ Write blocking data
      method >st7365p-text ( addr count self -- )

      \ Send a command to the ST7365P-text
      method cmd>st7365p-text ( cmd self -- )

      \ Send a command with arguments to the ST7365P-text
      method cmd-args>st7365p-text ( cmd addr count self -- )

      \ Send a byte of data to the ST7365P-text
      method data>st7365p-text ( data self -- )

      \ Set the ST7365P-text window
      method st7365p-text-window! ( start-col end-col start-row end-row self -- )

      \ Update a rectangular space on the ST7365P device
      method update-area ( start-col end-col start-row end-row self -- )

      \ Populate a row
      method populate-row ( start-col row cols line-buf self -- )

    end-module

    \ Get the character dimensions
    method char-dim@ ( self -- cols rows )

    \ Set the backlight (this may be a no-op)
    method backlight! ( backlight self -- )

    \ Update the ST7365P-text device
    method update-display ( self -- )

    \ Clear the ST7365P-text device
    method clear-display ( self -- )
    
  end-class

  <st7365p-text-common> begin-implement

    \ Constructor
    :noname { dc cs invert the-font buf cols rows phys-cols phys-rows self -- }
      buf cols rows self <text8>->new
      phys-cols self st7365p-text-phys-cols !
      phys-rows self st7365p-text-phys-rows !
      the-font self st7365p-text-font !
      invert self st7365p-text-invert !
      dc self st7365p-text-dc-pin !
      cs self st7365p-text-cs-pin !
      0 self st7365p-text-col-offset !
      0 self st7365p-text-row-offset !
      dc output-pin
      cs output-pin
      high dc pin!
      high cs pin!
    ; define new

    \ Destructor
    :noname { self -- }
      self <object>->destroy
    ; define destroy
    
    \ Initialize the ST7365P-text
    :noname { self -- }
      REG_SWRESET self cmd>st7365p-text

      150 ms

      REG_SLPOUT self cmd>st7365p-text

      500 ms

      $F0 s\" \xC3" self cmd-args>st7365p-text
      $F0 s\" \x96" self cmd-args>st7365p-text
      REG_MADCTL s\" \x48" self cmd-args>st7365p-text
      REG_COLMOD s\" \x65" self cmd-args>st7365p-text
      REG_FRMCTR1 s\" \xA0" self cmd-args>st7365p-text
      REG_INVCTR s\" \x00" self cmd-args>st7365p-text
      REG_ETMOD s\" \xC6" self cmd-args>st7365p-text
      REG_CECTRL1 s\" \x02\xE0" self cmd-args>st7365p-text
      REG_PWCTR1 s\" \x80\x06" self cmd-args>st7365p-text
      REG_PWCTR2 s\" \x15" self cmd-args>st7365p-text
      REG_PWCTR3 s\" \xA7" self cmd-args>st7365p-text
      REG_VMCTR1 s\" \x04" self cmd-args>st7365p-text
      $E8 s\" \x40\x8A\x00\x00\x29\x19\xAA\x33" self cmd-args>st7365p-text
      REG_PGAMCTRL
      s\" \xF0\x06\x0F\x05\x04\x20\x37\x33\x4C\x37\x13\x14\x2B\x31"
      self cmd-args>st7365p-text
      REG_NGAMCTRL
      s\" \xF0\x11\x1B\x11\x0F\x0A\x37\x43\x4C\x37\x13\x13\x2C\x32"
      self cmd-args>st7365p-text


      $F0 s\" \xC3" self cmd-args>st7365p-text
      $F0 s\" \x96" self cmd-args>st7365p-text

      self st7365p-text-invert @ if
        REG_INVON self cmd>st7365p-text
      else
        REG_INVOFF self cmd>st7365p-text
      then
      
      REG_TEON s\" \x00" self cmd-args>st7365p-text
      REG_SLPOUT self cmd>st7365p-text

      120 ms
      
      REG_DISPON self cmd>st7365p-text

      120 ms

      self char-dim@ { char-cols char-rows }
      self dim@ { cols rows }
      0 char-cols cols * 0 char-rows rows * self st7365p-text-window!

      \ This may be a no-op
      true self backlight!
      
    ; define init-st7365p-text

    \ Send a command to the ST7365P-text
    :noname { W^ cmd self -- }
      low self st7365p-text-dc-pin @ pin!
      low self st7365p-text-cs-pin @ pin!
      cmd 1 self >st7365p-text
      high self st7365p-text-cs-pin @ pin!
    ; define cmd>st7365p-text

    \ Send a command with arguments to the ST7365P-text
    :noname { W^ cmd addr count self -- }
      low self st7365p-text-dc-pin @ pin!
      low self st7365p-text-cs-pin @ pin!
      cmd 1 self >st7365p-text
      high self st7365p-text-dc-pin @ pin!
      addr count self >st7365p-text
      high self st7365p-text-cs-pin @ pin!
    ; define cmd-args>st7365p-text

    \ Set the entire display to be dirty
    :noname { self -- }
      self dim@ { cols rows }
      0 self st7365p-text-dirty-start-col !
      cols self st7365p-text-dirty-end-col !
      0 self st7365p-text-dirty-start-row !
      rows self st7365p-text-dirty-end-row !
    ; define set-dirty

    \ Clear dirty rectangle
    :noname { self -- }
      0 self st7365p-text-dirty-start-col !
      0 self st7365p-text-dirty-end-col !
      0 self st7365p-text-dirty-start-row !
      0 self st7365p-text-dirty-end-row !
    ; define clear-dirty
    
    \ Get whether an ST7365P-text device is dirty
    :noname { self -- dirty? }
      self st7365p-text-dirty-start-col @ self st7365p-text-dirty-end-col @ <>
      self st7365p-text-dirty-start-row @ self st7365p-text-dirty-end-row @ <> and
    ; define dirty?
  
    \ Dirty a character on an ST7365P-text device
    :noname { col row self -- }
      self dirty? if
        row self st7365p-text-dirty-start-row @ min
        self st7365p-text-dirty-start-row !
        row 1+ self st7365p-text-dirty-end-row @ max
        self st7365p-text-dirty-end-row !
        col self st7365p-text-dirty-start-col @ min
        self st7365p-text-dirty-start-col !
        col 1+ self st7365p-text-dirty-end-col @ max
        self st7365p-text-dirty-end-col !
      else
        row self st7365p-text-dirty-start-row !
        row 1+ self st7365p-text-dirty-end-row !
        col self st7365p-text-dirty-start-col !
        col 1+ self st7365p-text-dirty-end-col !
      then
    ; define dirty-char

    \ Dirty an area on an ST7365P-text device
    :noname { start-col end-col start-row end-row self -- }
      start-col end-col < start-row end-row < and if
        start-col start-row self dirty-char
        end-col 1- end-row 1- self dirty-char
      then
    ; define dirty-area
    
    \ Set the ST7365P-text window
    :noname { start-col end-col start-row end-row self -- }
      self st7365p-text-col-offset @ +to start-col
      self st7365p-text-col-offset @ 1- +to end-col
      self st7365p-text-row-offset @ +to start-row
      self st7365p-text-row-offset @ 1- +to end-row
      0 0 { W^ col-values W^ row-values }
      start-col 8 rshift col-values c!
      start-col col-values 1 + c!
      end-col 8 rshift col-values 2 + c!
      end-col col-values 3 + c!
      start-row 8 rshift row-values c!
      start-row row-values 1 + c!
      end-row 8 rshift row-values 2 + c!
      end-row row-values 3 + c!
      REG_CASET col-values 4 self cmd-args>st7365p-text
      REG_RASET row-values 4 self cmd-args>st7365p-text
    ; define st7365p-text-window!

    \ Update a rectangular space on the ST7365P device
    :noname { start-col end-col start-row end-row self -- }
      self char-dim@ { char-cols char-rows }
      start-col char-cols * to start-col
      end-col char-cols * to end-col
      start-row char-rows * to start-row
      end-row char-rows * to end-row
      start-col end-col start-row end-row self st7365p-text-window!
      low self st7365p-text-dc-pin @ pin!
      low self st7365p-text-cs-pin @ pin!
      REG_RAMWR { W^ cmd }
      cmd 1 self >st7365p-text
      high self st7365p-text-dc-pin @ pin!
      end-row start-row ?do
        self start-col i end-col start-col - dup 1 lshift [:
          { self start-col row cols line-buf }
          start-col row cols line-buf self populate-row
          line-buf cols 1 lshift self >st7365p-text
        ;] with-aligned-allot
      loop
      high self st7365p-text-cs-pin @ pin!
    ; define update-area
    
    \ Populate a row
    :noname { start-col row cols line-buf self -- }
      self st7365p-text-font @ { the-font }
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

    \ Clear the ST7365P-text device
    :noname { self -- }
      self st7365p-text-phys-cols @ { cols }
      self st7365p-text-phys-rows @ { rows }
      0 cols 0 rows self st7365p-text-window!
      low self st7365p-text-dc-pin @ pin!
      low self st7365p-text-cs-pin @ pin!
      REG_RAMWR { W^ cmd }
      cmd 1 self >st7365p-text
      high self st7365p-text-dc-pin @ pin!
      self rows cols dup 1 lshift [: { self rows cols line-buf }
        line-buf cols 1 lshift 0 fill
        rows 0 ?do line-buf cols 1 lshift self >st7365p-text loop
      ;] with-allot
      high self st7365p-text-cs-pin @ pin!
    ; define clear-display

    \ Get the character dimensions
    :noname { self -- cols rows }
      self st7365p-text-font @ { the-font }
      the-font char-cols @ the-font char-rows @
    ; define char-dim@

    \ Set the backlight (this may be a no-op)
    :noname { backlight self -- }
    ; define backlight!

    \ Update the ST7365P device
    :noname { self -- }
      self dirty? if 
        self st7365p-text-dirty-start-col @
        self st7365p-text-dirty-end-col @
        self st7365p-text-dirty-start-row @
        self st7365p-text-dirty-end-row @
        self update-area
        self clear-dirty
      then
    ; define update-display

  end-implement
  
end-module