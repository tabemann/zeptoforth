\ Copyright (c) 2023-2026 Travis Bemann
\ Copyright (c) 2026 Ken Mitton
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

\ Drawn from:
\ * https://www.displayfuture.com/Display/datasheet/controller/ST7796s.pdf
\ * https://github.com/adafruit/Adafruit-ST7735-Library/blob/master/Adafruit_ST7796S.cpp

begin-module st7796s-8-common

  oo import
  pixmap8 import
  pixmap8-internal import
  pin import
  armv6m import

  begin-module st7796s-8-common-internal

    \ ST7796S MADCTL bits
    $80 constant MADCTL_MY
    $40 constant MADCTL_MX
    $20 constant MADCTL_MV
    $10 constant MADCTL_ML
    $08 constant MADCTL_BGR
    $00 constant MADCTL_RGB

    \ ST7796S registers
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

    $10 constant REG_SLPIN  \  ///< Enter Sleep Mode
    $11 constant REG_SLPOUT  \ ///< Sleep Out
    $12 constant REG_PTLON  \  ///< Partial Mode ON
    $13 constant REG_NORON  \  ///< Normal Display Mode ON

    $0A constant REG_RDMODE  \     ///< Read Display Power Mode
    $0B constant REG_RDMADCTL  \   ///< Read Display MADCTL
    $0C constant REG_RDPIXFMT  \   ///< Read Display Pixel Format
    $0D constant REG_RDIMGFMT  \   ///< Read Display Image Format
    $0F constant REG_RDSELFDIAG  \ ///< Read Display Self-Diagnostic Result

    $20 constant REG_INVOFF  \   ///< Display Inversion OFF
    $21 constant REG_INVON  \    ///< Display Inversion ON
    $26 constant REG_GAMMASET  \ ///< Gamma Set
    $28 constant REG_DISPOFF  \  ///< Display OFF
    $29 constant REG_DISPON  \   ///< Display ON

    $2A constant REG_CASET  \ ///< Column Address Set
    $2B constant REG_PASET  \ ///< Page Address Set
    $2C constant REG_RAMWR  \ ///< Memory Write
    $2E constant REG_RAMRD  \ ///< Memory Read

    $30 constant REG_PTLAR  \    ///< Partial Area
    $33 constant REG_VSCRDEF  \  ///< Vertical Scrolling Definition
    $36 constant REG_MADCTL  \   ///< Memory Access Control
    $37 constant REG_VSCRSADD  \ ///< Vertical Scrolling Start Address
    $3A constant REG_PIXFMT  \   ///< COLMOD: Pixel Format Set

    $B1 constant REG_FRMCTR1
    \ ///< Frame Rate Control (In Normal Mode/Full Colors)
    $B2 constant REG_FRMCTR2  \ ///< Frame Rate Control (In Idle Mode/8 colors)
    $B3 constant REG_FRMCTR3
    \ ///< Frame Rate control (In Partial Mode/Full Colors)
    $B4 constant REG_INVCTR  \  ///< Display Inversion Control
    $B6 constant REG_DFUNCTR  \ ///< Display Function Control

    $C0 constant REG_PWCTR1  \ ///< Power Control 1
    $C1 constant REG_PWCTR2  \ ///< Power Control 2
    $C2 constant REG_PWCTR3  \ ///< Power Control 3
    $C3 constant REG_PWCTR4  \ ///< Power Control 4
    $C4 constant REG_PWCTR5  \ ///< Power Control 5
    $C5 constant REG_VMCTR1  \ ///< VCOM Control 1
    $C7 constant REG_VMCTR2  \ ///< VCOM Control 2

    $DA constant REG_RDID1  \ ///< Read ID 1
    $DB constant REG_RDID2  \ ///< Read ID 2
    $DC constant REG_RDID3  \ ///< Read ID 3
    $DD constant REG_RDID4  \ ///< Read ID 4

    $E0 constant REG_GMCTRP1  \ ///< Positive Gamma Correction
    $E1 constant REG_GMCTRN1  \ ///< Negative Gamma Correction
    \ $FC constant REG_PWCTR6



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
  
  <pixmap8> begin-class <st7796s-8-common>

    continue-module st7796s-8-common-internal
      
      \ Column offset
      cell member st7796s-8-col-offset

      \ Row offset
      cell member st7796s-8-row-offset

      \ DC pin
      cell member st7796s-8-dc-pin

      \ CS pin
      cell member st7796s-8-cs-pin

      \ Backlight pin
      cell member st7796s-8-backlight-pin

      \ Dirty rectangle start column
      cell member st7796s-8-dirty-start-col

      \ Dirty rectangle end column
      cell member st7796s-8-dirty-end-col

      \ Dirty rectangle start row
      cell member st7796s-8-dirty-start-row

      \ Dirty rectangle end row
      cell member st7796s-8-dirty-end-row

      \ Initialize the ST7796S-8
      method init-st7796s-8 ( self -- )

      \ Write blocking data
      method >st7796s-8 ( addr count self -- )

      \ Send a command to the ST7796S-8
      method cmd>st7796s-8 ( cmd self -- )

      \ Send a command with arguments to the ST7796S-8
      method cmd-args>st7796s-8 ( cmd addr count self -- )

      \ Send a byte of data to the ST7796S-8
      method data>st7796s-8 ( data self -- )

      \ Set the ST7796S-8 window
      method st7796s-8-window! ( start-col end-col start-row end-row self -- )

      \ Update a rectangular space on the ST7796S device
      method update-area ( start-col end-col start-row end-row self -- )
      
    end-module

    \ Set the backlight (this may be a no-op)
    method backlight! ( backlight self -- )

    \ Update the ST7796S-8 device
    method update-display ( self -- )
    
  end-class

  <st7796s-8-common> begin-implement

    \ Constructor
    :noname { dc cs backlight buf cols rows self -- }
      buf cols rows self <pixmap8>->new
      backlight self st7796s-8-backlight-pin !
      dc self st7796s-8-dc-pin !
      cs self st7796s-8-cs-pin !
      0 self st7796s-8-col-offset !
      0 self st7796s-8-row-offset !
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
    
    \ Initialize the ST7796S-8
    :noname { self -- }
      REG_SWRESET self cmd>st7796s-8

      150 ms

      REG_SLPOUT self cmd>st7796s-8

      500 ms

      $EF s\" \x03\x80\x02" self cmd-args>st7796s-8
      $EF s\" \x03\x80\x02" self cmd-args>st7796s-8
      $CF s\" \x00\xC1\x30" self cmd-args>st7796s-8
      $ED s\" \x64\x03\x12\x81" self cmd-args>st7796s-8
      $E8 s\" \x85\x00\x78" self cmd-args>st7796s-8
      $CB s\" \x39\x2C\x00\x34\x02" self cmd-args>st7796s-8
      $F7 s\" \x20" self cmd-args>st7796s-8
      $EA s\" \x00\x00" self cmd-args>st7796s-8
      REG_PWCTR1   s\" \x23" self cmd-args>st7796s-8             \ Power control VRH[5:0]
      REG_PWCTR2   s\" \x10" self cmd-args>st7796s-8             \ Power control SAP[2:0];BT[3:0]
      REG_VMCTR1   s\" \x3e\x28" self cmd-args>st7796s-8       \ VCM control
      REG_VMCTR2   s\" \x86" self cmd-args>st7796s-8             \ VCM control2

      self dim@ { cols rows }
      0 { W^ madctl }
      
      rows 320 = cols 480 = and if
        \ Rotation 0
        0 self st7796s-8-col-offset !
        0 self st7796s-8-row-offset !
        [ MADCTL_MX MADCTL_RGB or ]
        literal madctl !
      else  \ Portrait
        \ Rotation 1
        0 self st7796s-8-col-offset !
        0 self st7796s-8-row-offset !
        [ MADCTL_MV MADCTL_RGB or ]
        literal madctl !
      then

      \ Rotation 2: (MADCTL_MY | MADCTL_BGR)
      \ Rotation 3: (MADCTL_MX | MADCTL_MY | MADCTL_MV | MADCTL_BGR);

      REG_MADCTL madctl 1 self cmd-args>st7796s-8             \ Memory Access Control

      REG_VSCRSADD s\" \x00" self cmd-args>st7796s-8             \ Vertical scroll zero
      REG_PIXFMT   s\" \x55" self cmd-args>st7796s-8
      REG_FRMCTR1  s\" \x00\x18" self cmd-args>st7796s-8
      REG_DFUNCTR  s\" \x80\x02\x3B" self cmd-args>st7796s-8 \ Display Function Control
      $F2 s\" \x00" self cmd-args>st7796s-8                         \ 3Gamma Function Disable
      REG_GAMMASET  s\" \x01" self cmd-args>st7796s-8             \ Gamma curve selected
      REG_GMCTRP1  s\" \x0F\x31\x2B\x0C\x0E\x08\x4E\xF1\x37\x07\x10\x03\x0E\x09\x00" self cmd-args>st7796s-8 \ Set Gamma
      REG_GMCTRN1  s\" \x00\x0E\x14\x03\x11\x07\x31\xC1\x48\x08\x0F\x0C\x31\x36\x0F" self cmd-args>st7796s-8 \ Set Gamma
      REG_SLPOUT  self cmd>st7796s-8 \ Exit Sleep
      120 ms
      REG_DISPON  self cmd>st7796s-8 \ Display on
      120 ms
      \ $00         \ NOP, End of list

      self dim@ { cols rows }
      0 cols 0 rows self st7796s-8-window!

      \ This may be a no-op
      true self backlight!
      
    ; define init-st7796s-8

    \ Send a command to the ST7796S-8
    :noname { W^ cmd self -- }
      low self st7796s-8-dc-pin @ pin!
      low self st7796s-8-cs-pin @ pin!
      cmd 1 self >st7796s-8
      high self st7796s-8-cs-pin @ pin!
    ; define cmd>st7796s-8

    \ Send 8 bits of data to the ST7796S-8
    :noname { W^ data self -- }
      high self st7796s-8-dc-pin @ pin!
      low self st7796s-8-cs-pin @ pin!
      data 1 self >st7796s-8
      high self st7796s-8-cs-pin @ pin!
    ; define data>st7796s-8

    \ Send a command with arguments to the ST7796S-8
    :noname { W^ cmd addr count self -- }
      low self st7796s-8-dc-pin @ pin!
      low self st7796s-8-cs-pin @ pin!
      cmd 1 self >st7796s-8
      high self st7796s-8-dc-pin @ pin!
      addr count self >st7796s-8
      high self st7796s-8-cs-pin @ pin!
    ; define cmd-args>st7796s-8

    \ Set the entire display to be dirty
    :noname { self -- }
      0 self st7796s-8-dirty-start-col !
      self pixmap-cols @ self st7796s-8-dirty-end-col !
      0 self st7796s-8-dirty-start-row !
      self pixmap-rows @ self st7796s-8-dirty-end-row !
    ; define set-dirty

    \ Clear dirty rectangle
    :noname { self -- }
      0 self st7796s-8-dirty-start-col !
      0 self st7796s-8-dirty-end-col !
      0 self st7796s-8-dirty-start-row !
      0 self st7796s-8-dirty-end-row !
    ; define clear-dirty
    
    \ Get whether an ST7796S-8 device is dirty
    :noname { self -- dirty? }
      self st7796s-8-dirty-start-col @ self st7796s-8-dirty-end-col @ <>
      self st7796s-8-dirty-start-row @ self st7796s-8-dirty-end-row @ <> and
    ; define dirty?
  
    \ Dirty a pixel on an ST7796S-8 device
    :noname { col row self -- }
      self dirty? if
        row self st7796s-8-dirty-start-row @ min
        self st7796s-8-dirty-start-row !
        row 1+ self st7796s-8-dirty-end-row @ max
        self st7796s-8-dirty-end-row !
        col self st7796s-8-dirty-start-col @ min
        self st7796s-8-dirty-start-col !
        col 1+ self st7796s-8-dirty-end-col @ max
        self st7796s-8-dirty-end-col !
      else
        row self st7796s-8-dirty-start-row !
        row 1+ self st7796s-8-dirty-end-row !
        col self st7796s-8-dirty-start-col !
        col 1+ self st7796s-8-dirty-end-col !
      then
    ; define dirty-pixel

    \ Dirty an area on an ST7796S-8 device
    :noname { start-col end-col start-row end-row self -- }
      start-col end-col < start-row end-row < and if
        start-col start-row self dirty-pixel
        end-col 1- end-row 1- self dirty-pixel
      then
    ; define dirty-area
    
    \ Set the ST7796S-8 window
    :noname { start-col end-col start-row end-row self -- }
      self st7796s-8-col-offset @ +to start-col
      self st7796s-8-col-offset @ 1- +to end-col
      self st7796s-8-row-offset @ +to start-row
      self st7796s-8-row-offset @ 1- +to end-row
      0 0 { W^ col-values W^ row-values }
      start-col 8 rshift col-values c!
      start-col col-values 1 + c!
      end-col 8 rshift col-values 2 + c!
      end-col col-values 3 + c!
      start-row 8 rshift row-values c!
      start-row row-values 1 + c!
      end-row 8 rshift row-values 2 + c!
      end-row row-values 3 + c!
      REG_CASET col-values 4 self cmd-args>st7796s-8
      REG_RASET row-values 4 self cmd-args>st7796s-8
    ; define st7796s-8-window!

    \ Update a rectangular space on the ST7796S device
    :noname { start-col end-col start-row end-row self -- }
      start-col end-col start-row end-row self st7796s-8-window!
      low self st7796s-8-dc-pin @ pin!
      low self st7796s-8-cs-pin @ pin!
      REG_RAMWR { W^ cmd }
      cmd 1 self >st7796s-8
      high self st7796s-8-dc-pin @ pin!
      end-row start-row ?do
        self start-col i end-col start-col - dup 1 lshift [:
          { self start-col row cols line-buf }
          start-col row self pixel-addr line-buf cols convert-8-to-16
          line-buf cols 1 lshift self >st7796s-8
        ;] with-aligned-allot
      loop
      high self st7796s-8-cs-pin @ pin!
    ; define update-area

    \ Set the backlight (this may be a no-op)
    :noname { backlight self -- }
      backlight self st7796s-8-backlight-pin @ pin!
    ; define backlight!

    \ Update the ST7796S device
    :noname { self -- }
      self dirty? if 
        self st7796s-8-dirty-start-col @
        self st7796s-8-dirty-end-col @
        self st7796s-8-dirty-start-row @
        self st7796s-8-dirty-end-row @
        self update-area
        self clear-dirty
      then
    ; define update-display

  end-implement
  
end-module
