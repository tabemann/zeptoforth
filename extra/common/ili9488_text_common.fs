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

begin-module ili9488-text-common

  oo import
  bitmap import
  font import
  text8 import
  text8-internal import
  pin import
  armv6m import

  begin-module ili9488-text-common-internal

    \ ILI9488 registers
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
    : convert-8-to-16 ( rgb8 -- rgb16 )
      code[
      6 tos r3 lsrs_,_,#_  \ Blue, 7:6
      11 r3 r3 lsls_,_,#_ \ to 12:11 (bits 10:8 are zero)
      26 tos r4 lsls_,_,#_ \ Green, 6:3
      29 r4 r4 lsrs_,_,#_ \ to 2:0 (bits 15:13 are zero)
      r4 r3 orrs_,_
      29 tos r4 lsls_,_,#_ \ Red, 2:0
      24 r4 r4 lsrs_,_,#_ \ to 7:5 (bits 4:3 are zero)
      r4 r3 orrs_,_
      r3 tos movs_,_
      ]code
    ;
    
  end-module> import
  
  <text8> begin-class <ili9488-text-common>

    continue-module ili9488-text-common-internal

      \ The font
      cell member ili9488-text-font

      \ The physical number of columns
      cell member ili9488-text-phys-cols
      
      \ The physical number of rows
      cell member ili9488-text-phys-rows

      \ Inversion enabled setting
      cell member ili9488-text-invert

      \ Column offset
      cell member ili9488-text-col-offset

      \ Row offset
      cell member ili9488-text-row-offset

      \ DC pin
      cell member ili9488-text-dc-pin

      \ CS pin
      cell member ili9488-text-cs-pin

      \ Dirty rectangle start column
      cell member ili9488-text-dirty-start-col

      \ Dirty rectangle end column
      cell member ili9488-text-dirty-end-col

      \ Dirty rectangle start row
      cell member ili9488-text-dirty-start-row

      \ Dirty rectangle end row
      cell member ili9488-text-dirty-end-row

      \ Initialize the ILI9488-text
      method init-ili9488-text ( self -- )

      \ Write blocking data
      method >ili9488-text ( addr count self -- )

      \ Send a command to the ILI9488-text
      method cmd>ili9488-text ( cmd self -- )

      \ Send a command with arguments to the ILI9488-text
      method cmd-args>ili9488-text ( cmd addr count self -- )

      \ Send a byte of data to the ILI9488-text
      method data>ili9488-text ( data self -- )

      \ Set the ILI9488-text window
      method ili9488-text-window! ( start-col end-col start-row end-row self -- )

      \ Update a rectangular space on the ILI9488 device
      method update-area ( start-col end-col start-row end-row self -- )

      \ Populate a row
      method populate-row ( start-col row cols line-buf self -- )

    end-module

    \ Get the character dimensions
    method char-dim@ ( self -- cols rows )

    \ Set the backlight (this may be a no-op)
    method backlight! ( backlight self -- )

    \ Update the ILI9488-text device
    method update-display ( self -- )

    \ Clear the ILI9488-text device
    method clear-display ( self -- )
    
  end-class

  <ili9488-text-common> begin-implement

    \ Constructor
    :noname { dc cs invert the-font buf cols rows phys-cols phys-rows self -- }
      buf cols rows self <text8>->new
      phys-cols self ili9488-text-phys-cols !
      phys-rows self ili9488-text-phys-rows !
      the-font self ili9488-text-font !
      invert self ili9488-text-invert !
      dc self ili9488-text-dc-pin !
      cs self ili9488-text-cs-pin !
      0 self ili9488-text-col-offset !
      0 self ili9488-text-row-offset !
      dc output-pin
      cs output-pin
      high dc pin!
      high cs pin!
    ; define new

    \ Destructor
    :noname { self -- }
      self <object>->destroy
    ; define destroy
    
    \ Initialize the ILI9488-text
    :noname { self -- }
      REG_SWRESET self cmd>ili9488-text

      150 ms

      REG_SLPOUT self cmd>ili9488-text

      500 ms

      $F0 s\" \xC3" self cmd-args>ili9488-text
      $F0 s\" \x96" self cmd-args>ili9488-text
      REG_MADCTL s\" \x48" self cmd-args>ili9488-text
      REG_COLMOD s\" \x65" self cmd-args>ili9488-text
      REG_FRMCTR1 s\" \xA0" self cmd-args>ili9488-text
      REG_INVCTR s\" \x00" self cmd-args>ili9488-text
      REG_ETMOD s\" \xC6" self cmd-args>ili9488-text
      REG_CECTRL1 s\" \x02\xE0" self cmd-args>ili9488-text
      REG_PWCTR1 s\" \x80\x06" self cmd-args>ili9488-text
      REG_PWCTR2 s\" \x15" self cmd-args>ili9488-text
      REG_PWCTR3 s\" \xA7" self cmd-args>ili9488-text
      REG_VMCTR1 s\" \x04" self cmd-args>ili9488-text
      $E8 s\" \x40\x8A\x00\x00\x29\x19\xAA\x33" self cmd-args>ili9488-text
      REG_PGAMCTRL
      s\" \xF0\x06\x0F\x05\x04\x20\x37\x33\x4C\x37\x13\x14\x2B\x31"
      self cmd-args>ili9488-text
      REG_NGAMCTRL
      s\" \xF0\x11\x1B\x11\x0F\x0A\x37\x43\x4C\x37\x13\x13\x2C\x32"
      self cmd-args>ili9488-text


      $F0 s\" \xC3" self cmd-args>ili9488-text
      $F0 s\" \x96" self cmd-args>ili9488-text

      self ili9488-text-invert @ if
        REG_INVON self cmd>ili9488-text
      else
        REG_INVOFF self cmd>ili9488-text
      then
      
      REG_TEON s\" \x00" self cmd-args>ili9488-text
      REG_SLPOUT self cmd>ili9488-text

      120 ms
      
      REG_DISPON self cmd>ili9488-text

      120 ms

      self char-dim@ { char-cols char-rows }
      self dim@ { cols rows }
      0 char-cols cols * 0 char-rows rows * self ili9488-text-window!

      \ This may be a no-op
      true self backlight!
      
    ; define init-ili9488-text

    \ Send a command to the ILI9488-text
    :noname { W^ cmd self -- }
      low self ili9488-text-dc-pin @ pin!
      low self ili9488-text-cs-pin @ pin!
      cmd 1 self >ili9488-text
      high self ili9488-text-cs-pin @ pin!
    ; define cmd>ili9488-text

    \ Send a command with arguments to the ILI9488-text
    :noname { W^ cmd addr count self -- }
      low self ili9488-text-dc-pin @ pin!
      low self ili9488-text-cs-pin @ pin!
      cmd 1 self >ili9488-text
      high self ili9488-text-dc-pin @ pin!
      addr count self >ili9488-text
      high self ili9488-text-cs-pin @ pin!
    ; define cmd-args>ili9488-text

    \ Set the entire display to be dirty
    :noname { self -- }
      self dim@ { cols rows }
      0 self ili9488-text-dirty-start-col !
      cols self ili9488-text-dirty-end-col !
      0 self ili9488-text-dirty-start-row !
      rows self ili9488-text-dirty-end-row !
    ; define set-dirty

    \ Clear dirty rectangle
    :noname { self -- }
      0 self ili9488-text-dirty-start-col !
      0 self ili9488-text-dirty-end-col !
      0 self ili9488-text-dirty-start-row !
      0 self ili9488-text-dirty-end-row !
    ; define clear-dirty
    
    \ Get whether an ILI9488-text device is dirty
    :noname { self -- dirty? }
      self ili9488-text-dirty-start-col @ self ili9488-text-dirty-end-col @ <>
      self ili9488-text-dirty-start-row @ self ili9488-text-dirty-end-row @ <> and
    ; define dirty?
  
    \ Dirty a character on an ILI9488-text device
    :noname { col row self -- }
      self dirty? if
        row self ili9488-text-dirty-start-row @ min
        self ili9488-text-dirty-start-row !
        row 1+ self ili9488-text-dirty-end-row @ max
        self ili9488-text-dirty-end-row !
        col self ili9488-text-dirty-start-col @ min
        self ili9488-text-dirty-start-col !
        col 1+ self ili9488-text-dirty-end-col @ max
        self ili9488-text-dirty-end-col !
      else
        row self ili9488-text-dirty-start-row !
        row 1+ self ili9488-text-dirty-end-row !
        col self ili9488-text-dirty-start-col !
        col 1+ self ili9488-text-dirty-end-col !
      then
    ; define dirty-char

    \ Dirty an area on an ILI9488-text device
    :noname { start-col end-col start-row end-row self -- }
      start-col end-col < start-row end-row < and if
        start-col start-row self dirty-char
        end-col 1- end-row 1- self dirty-char
      then
    ; define dirty-area
    
    \ Set the ILI9488-text window
    :noname { start-col end-col start-row end-row self -- }
      self ili9488-text-col-offset @ +to start-col
      self ili9488-text-col-offset @ 1- +to end-col
      self ili9488-text-row-offset @ +to start-row
      self ili9488-text-row-offset @ 1- +to end-row
      0 0 { W^ col-values W^ row-values }
      start-col 8 rshift col-values c!
      start-col col-values 1 + c!
      end-col 8 rshift col-values 2 + c!
      end-col col-values 3 + c!
      start-row 8 rshift row-values c!
      start-row row-values 1 + c!
      end-row 8 rshift row-values 2 + c!
      end-row row-values 3 + c!
      REG_CASET col-values 4 self cmd-args>ili9488-text
      REG_RASET row-values 4 self cmd-args>ili9488-text
    ; define ili9488-text-window!

    \ Update a rectangular space on the ILI9488 device
    :noname { start-col end-col start-row end-row self -- }
      self char-dim@ { char-cols char-rows }
      start-col char-cols * to start-col
      end-col char-cols * to end-col
      start-row char-rows * to start-row
      end-row char-rows * to end-row
      start-col end-col start-row end-row self ili9488-text-window!
      low self ili9488-text-dc-pin @ pin!
      low self ili9488-text-cs-pin @ pin!
      REG_RAMWR { W^ cmd }
      cmd 1 self >ili9488-text
      high self ili9488-text-dc-pin @ pin!
      end-row start-row ?do
        self start-col i end-col start-col - dup 1 lshift [:
          { self start-col row cols line-buf }
          start-col row cols line-buf self populate-row
          line-buf cols 1 lshift self >ili9488-text
        ;] with-aligned-allot
      loop
      high self ili9488-text-cs-pin @ pin!
    ; define update-area
    
    \ Populate a row
    :noname { start-col row cols line-buf self -- }
      self ili9488-text-font @ { the-font }
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
      fg-color-buf offset + c@ convert-8-to-16 { fg-color }
      bk-color-buf offset + c@ convert-8-to-16 { bk-color }
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
              fg-color-buf offset + c@ convert-8-to-16 to fg-color
              bk-color-buf offset + c@ convert-8-to-16 to bk-color
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
          font-col char-cols = if
            0 to font-col
            1 +to text-col
            1 +to offset
            text-col text-cols < if
              char-buf offset + c@ the-font font-internal::find-char-col page +
              to addr
              fg-color-buf offset + c@ convert-8-to-16 to fg-color
              bk-color-buf offset + c@ convert-8-to-16 to bk-color
              text-col text-row self raw-underlined@ to underlined
            then
          then
        repeat
      then
    ; define populate-row

    \ Clear the ILI9488-text device
    :noname { self -- }
      self ili9488-text-phys-cols @ { cols }
      self ili9488-text-phys-rows @ { rows }
      0 cols 0 rows self ili9488-text-window!
      low self ili9488-text-dc-pin @ pin!
      low self ili9488-text-cs-pin @ pin!
      REG_RAMWR { W^ cmd }
      cmd 1 self >ili9488-text
      high self ili9488-text-dc-pin @ pin!
      self rows cols dup 1 lshift [: { self rows cols line-buf }
        line-buf cols 1 lshift 0 fill
        rows 0 ?do line-buf cols 1 lshift self >ili9488-text loop
      ;] with-allot
      high self ili9488-text-cs-pin @ pin!
    ; define clear-display

    \ Get the character dimensions
    :noname { self -- cols rows }
      self ili9488-text-font @ { the-font }
      the-font char-cols @ the-font char-rows @
    ; define char-dim@

    \ Set the backlight (this may be a no-op)
    :noname { backlight self -- }
    ; define backlight!

    \ Update the ILI9488 device
    :noname { self -- }
      self dirty? if 
        self ili9488-text-dirty-start-col @
        self ili9488-text-dirty-end-col @
        self ili9488-text-dirty-start-row @
        self ili9488-text-dirty-end-row @
        self update-area
        self clear-dirty
      then
    ; define update-display

  end-implement
  
end-module