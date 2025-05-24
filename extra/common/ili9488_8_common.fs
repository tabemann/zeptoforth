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

begin-module ili9488-8-common

  oo import
  pixmap8 import
  pixmap8-internal import
  pin import
  armv6m import

  begin-module ili9488-8-common-internal

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
    : do-convert-8-to-16 ( rgb8 -- rgb16 )
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
  
  <pixmap8> begin-class <ili9488-8-common>

    continue-module ili9488-8-common-internal
      
      \ Inversion enabled setting
      cell member ili9488-8-invert

      \ Column offset
      cell member ili9488-8-col-offset

      \ Row offset
      cell member ili9488-8-row-offset

      \ DC pin
      cell member ili9488-8-dc-pin

      \ CS pin
      cell member ili9488-8-cs-pin

      \ Dirty rectangle start column
      cell member ili9488-8-dirty-start-col

      \ Dirty rectangle end column
      cell member ili9488-8-dirty-end-col

      \ Dirty rectangle start row
      cell member ili9488-8-dirty-start-row

      \ Dirty rectangle end row
      cell member ili9488-8-dirty-end-row

      \ Initialize the ILI9488-8
      method init-ili9488-8 ( self -- )

      \ Write blocking data
      method >ili9488-8 ( addr count self -- )

      \ Send a command to the ILI9488-8
      method cmd>ili9488-8 ( cmd self -- )

      \ Send a command with arguments to the ILI9488-8
      method cmd-args>ili9488-8 ( cmd addr count self -- )

      \ Send a byte of data to the ILI9488-8
      method data>ili9488-8 ( data self -- )

      \ Set the ILI9488-8 window
      method ili9488-8-window! ( start-col end-col start-row end-row self -- )

      \ Update a rectangular space on the ILI9488 device
      method update-area ( start-col end-col start-row end-row self -- )
      
    end-module

    \ Set the backlight (this may be a no-op)
    method backlight! ( backlight self -- )

    \ Update the ILI9488-8 device
    method update-display ( self -- )
    
  end-class

  <ili9488-8-common> begin-implement

    \ Constructor
    :noname { dc cs invert buf cols rows self -- }
      buf cols rows self <pixmap8>->new
      invert self ili9488-8-invert !
      dc self ili9488-8-dc-pin !
      cs self ili9488-8-cs-pin !
      0 self ili9488-8-col-offset !
      0 self ili9488-8-row-offset !
      dc output-pin
      cs output-pin
      high dc pin!
      high cs pin!
    ; define new

    \ Destructor
    :noname { self -- }
      self <object>->destroy
    ; define destroy
    
    \ Initialize the ILI9488-8
    :noname { self -- }
      REG_SWRESET self cmd>ili9488-8

      150 ms

      REG_SLPOUT self cmd>ili9488-8

      500 ms

      $F0 s\" \xC3" self cmd-args>ili9488-8
      $F0 s\" \x96" self cmd-args>ili9488-8
      REG_MADCTL s\" \x48" self cmd-args>ili9488-8
      REG_COLMOD s\" \x65" self cmd-args>ili9488-8
      REG_FRMCTR1 s\" \xA0" self cmd-args>ili9488-8
      REG_INVCTR s\" \x00" self cmd-args>ili9488-8
      REG_ETMOD s\" \xC6" self cmd-args>ili9488-8
      REG_CECTRL1 s\" \x02\xE0" self cmd-args>ili9488-8
      REG_PWCTR1 s\" \x80\x06" self cmd-args>ili9488-8
      REG_PWCTR2 s\" \x15" self cmd-args>ili9488-8
      REG_PWCTR3 s\" \xA7" self cmd-args>ili9488-8
      REG_VMCTR1 s\" \x04" self cmd-args>ili9488-8
      $E8 s\" \x40\x8A\x00\x00\x29\x19\xAA\x33" self cmd-args>ili9488-8
      REG_PGAMCTRL
      s\" \xF0\x06\x0F\x05\x04\x20\x37\x33\x4C\x37\x13\x14\x2B\x31"
      self cmd-args>ili9488-8
      REG_NGAMCTRL
      s\" \xF0\x11\x1B\x11\x0F\x0A\x37\x43\x4C\x37\x13\x13\x2C\x32"
      self cmd-args>ili9488-8


      $F0 s\" \xC3" self cmd-args>ili9488-8
      $F0 s\" \x96" self cmd-args>ili9488-8

      self ili9488-8-invert @ if
        REG_INVON self cmd>ili9488-8
      else
        REG_INVOFF self cmd>ili9488-8
      then
      
      REG_TEON s\" \x00" self cmd-args>ili9488-8
      REG_SLPOUT self cmd>ili9488-8

      120 ms
      
      REG_DISPON self cmd>ili9488-8

      120 ms
      
      \ REG_INVON self cmd>ili9488-8

      self dim@ { cols rows }
      0 cols 0 rows self ili9488-8-window!

      \ This may be a no-op
      true self backlight!
      
    ; define init-ili9488-8

    \ Send a command to the ILI9488-8
    :noname { W^ cmd self -- }
      low self ili9488-8-dc-pin @ pin!
      low self ili9488-8-cs-pin @ pin!
      cmd 1 self >ili9488-8
      high self ili9488-8-cs-pin @ pin!
    ; define cmd>ili9488-8

    \ Send a command with arguments to the ILI9488-8
    :noname { W^ cmd addr count self -- }
      low self ili9488-8-dc-pin @ pin!
      low self ili9488-8-cs-pin @ pin!
      cmd 1 self >ili9488-8
      high self ili9488-8-dc-pin @ pin!
      addr count self >ili9488-8
      high self ili9488-8-cs-pin @ pin!
    ; define cmd-args>ili9488-8

    \ Set the entire display to be dirty
    :noname { self -- }
      0 self ili9488-8-dirty-start-col !
      self pixmap-cols @ self ili9488-8-dirty-end-col !
      0 self ili9488-8-dirty-start-row !
      self pixmap-rows @ self ili9488-8-dirty-end-row !
    ; define set-dirty

    \ Clear dirty rectangle
    :noname { self -- }
      0 self ili9488-8-dirty-start-col !
      0 self ili9488-8-dirty-end-col !
      0 self ili9488-8-dirty-start-row !
      0 self ili9488-8-dirty-end-row !
    ; define clear-dirty
    
    \ Get whether an ILI9488-8 device is dirty
    :noname { self -- dirty? }
      self ili9488-8-dirty-start-col @ self ili9488-8-dirty-end-col @ <>
      self ili9488-8-dirty-start-row @ self ili9488-8-dirty-end-row @ <> and
    ; define dirty?
  
    \ Dirty a pixel on an ILI9488-8 device
    :noname { col row self -- }
      self dirty? if
        row self ili9488-8-dirty-start-row @ min
        self ili9488-8-dirty-start-row !
        row 1+ self ili9488-8-dirty-end-row @ max
        self ili9488-8-dirty-end-row !
        col self ili9488-8-dirty-start-col @ min
        self ili9488-8-dirty-start-col !
        col 1+ self ili9488-8-dirty-end-col @ max
        self ili9488-8-dirty-end-col !
      else
        row self ili9488-8-dirty-start-row !
        row 1+ self ili9488-8-dirty-end-row !
        col self ili9488-8-dirty-start-col !
        col 1+ self ili9488-8-dirty-end-col !
      then
    ; define dirty-pixel

    \ Dirty an area on an ILI9488-8 device
    :noname { start-col end-col start-row end-row self -- }
      start-col end-col < start-row end-row < and if
        start-col start-row self dirty-pixel
        end-col 1- end-row 1- self dirty-pixel
      then
    ; define dirty-area
    
    \ Set the ILI9488-8 window
    :noname { start-col end-col start-row end-row self -- }
      self ili9488-8-col-offset @ +to start-col
      self ili9488-8-col-offset @ 1- +to end-col
      self ili9488-8-row-offset @ +to start-row
      self ili9488-8-row-offset @ 1- +to end-row
      0 0 { W^ col-values W^ row-values }
      start-col 8 rshift col-values c!
      start-col col-values 1 + c!
      end-col 8 rshift col-values 2 + c!
      end-col col-values 3 + c!
      start-row 8 rshift row-values c!
      start-row row-values 1 + c!
      end-row 8 rshift row-values 2 + c!
      end-row row-values 3 + c!
      REG_CASET col-values 4 self cmd-args>ili9488-8
      REG_RASET row-values 4 self cmd-args>ili9488-8
    ; define ili9488-8-window!

    \ Update a rectangular space on the ILI9488 device
    :noname { start-col end-col start-row end-row self -- }
      start-col end-col start-row end-row self ili9488-8-window!
      low self ili9488-8-dc-pin @ pin!
      low self ili9488-8-cs-pin @ pin!
      REG_RAMWR { W^ cmd }
      cmd 1 self >ili9488-8
      high self ili9488-8-dc-pin @ pin!
      end-row start-row ?do
        self start-col i end-col start-col - dup 1 lshift [:
          { self start-col row cols line-buf }
          start-col row self pixel-addr line-buf cols convert-8-to-16
          line-buf cols 1 lshift self >ili9488-8
        ;] with-aligned-allot
      loop
      high self ili9488-8-cs-pin @ pin!
    ; define update-area

    \ Set the backlight (this may be a no-op)
    :noname { backlight self -- }
    ; define backlight!

    \ Update the ILI9488 device
    :noname { self -- }
      self dirty? if 
        self ili9488-8-dirty-start-col @
        self ili9488-8-dirty-end-col @
        self ili9488-8-dirty-start-row @
        self ili9488-8-dirty-end-row @
        self update-area
        self clear-dirty
      then
    ; define update-display

  end-implement
  
end-module