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

begin-module st7796s-text-common

  oo import
  bitmap import
  font import
  text8 import
  text8-internal import
  pin import
  armv6m import

  begin-module st7796s-text-common-internal

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
    
  end-module> import
  
  <text8> begin-class <st7796s-text-common>

    continue-module st7796s-text-common-internal

      \ The font
      cell member st7796s-text-font

      \ The physical number of columns
      cell member st7796s-text-phys-cols
      
      \ The physical number of rows
      cell member st7796s-text-phys-rows

      \ Column offset
      cell member st7796s-text-col-offset

      \ Row offset
      cell member st7796s-text-row-offset

      \ DC pin
      cell member st7796s-text-dc-pin

      \ CS pin
      cell member st7796s-text-cs-pin

      \ Backlight pin
      cell member st7796s-text-backlight-pin

      \ Dirty rectangle start column
      cell member st7796s-text-dirty-start-col

      \ Dirty rectangle end column
      cell member st7796s-text-dirty-end-col

      \ Dirty rectangle start row
      cell member st7796s-text-dirty-start-row

      \ Dirty rectangle end row
      cell member st7796s-text-dirty-end-row

      \ Initialize the ST7796S-text
      method init-st7796s-text ( self -- )

      \ Write blocking data
      method >st7796s-text ( addr count self -- )

      \ Send a command to the ST7796S-text
      method cmd>st7796s-text ( cmd self -- )

      \ Send a command with arguments to the ST7796S-text
      method cmd-args>st7796s-text ( cmd addr count self -- )

      \ Send a byte of data to the ST7796S-text
      method data>st7796s-text ( data self -- )

      \ Set the ST7796S-text window
      method st7796s-text-window! ( start-col end-col start-row end-row self -- )

      \ Update a rectangular space on the ST7796S device
      method update-area ( start-col end-col start-row end-row self -- )

      \ Populate a row
      method populate-row ( start-col row cols line-buf self -- )

    end-module

    \ Get the character dimensions
    method char-dim@ ( self -- cols rows )

    \ Set the backlight (this may be a no-op)
    method backlight! ( backlight self -- )

    \ Update the ST7796S-text device
    method update-display ( self -- )

    \ Clear the ST7796S-text device
    method clear-display ( self -- )
    
  end-class

  <st7796s-text-common> begin-implement

    \ Constructor
    :noname { dc cs backlight the-font buf cols rows phys-cols phys-rows self -- }
      buf cols rows self <text8>->new
      phys-cols self st7796s-text-phys-cols !
      phys-rows self st7796s-text-phys-rows !
      the-font self st7796s-text-font !
      backlight self st7796s-text-backlight-pin !
      dc self st7796s-text-dc-pin !
      cs self st7796s-text-cs-pin !
      0 self st7796s-text-col-offset !
      0 self st7796s-text-row-offset !
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
    
    \ Initialize the ST7796S-text
    :noname { self -- }
      REG_SWRESET self cmd>st7796s-text

      150 ms

      REG_SLPOUT self cmd>st7796s-text

      500 ms

      $EF s\" \x03\x80\x02" self cmd-args>st7796s-text
      $EF s\" \x03\x80\x02" self cmd-args>st7796s-text
      $CF s\" \x00\xC1\x30" self cmd-args>st7796s-text
      $ED s\" \x64\x03\x12\x81" self cmd-args>st7796s-text
      $E8 s\" \x85\x00\x78" self cmd-args>st7796s-text
      $CB s\" \x39\x2C\x00\x34\x02" self cmd-args>st7796s-text
      $F7 s\" \x20" self cmd-args>st7796s-text
      $EA s\" \x00\x00" self cmd-args>st7796s-text
      REG_PWCTR1   s\" \x23" self cmd-args>st7796s-text             \ Power control VRH[5:0]
      REG_PWCTR2   s\" \x10" self cmd-args>st7796s-text             \ Power control SAP[2:0];BT[3:0]
      REG_VMCTR1   s\" \x3e\x28" self cmd-args>st7796s-text       \ VCM control
      REG_VMCTR2   s\" \x86" self cmd-args>st7796s-text             \ VCM control2

      self st7796s-text-phys-cols @ { cols }
      self st7796s-text-phys-rows @ { rows }
      
      0 { W^ madctl }
      
      rows 320 = cols 480 = and if
        \ Rotation 0
        0 self st7796s-text-col-offset !
        0 self st7796s-text-row-offset !
        [ MADCTL_MX MADCTL_RGB or ]
        literal madctl !
      else  \ Portrait
        \ Rotation 1
        0 self st7796s-text-col-offset !
        0 self st7796s-text-row-offset !
        [ MADCTL_MV MADCTL_RGB or ]
        literal madctl !
      then

      \ Rotation 2: (MADCTL_MY | MADCTL_BGR)
      \ Rotation 3: (MADCTL_MX | MADCTL_MY | MADCTL_MV | MADCTL_BGR);

      REG_MADCTL madctl 1 self cmd-args>st7796s-text             \ Memory Access Control

      REG_VSCRSADD s\" \x00" self cmd-args>st7796s-text             \ Vertical scroll zero
      REG_PIXFMT   s\" \x55" self cmd-args>st7796s-text
      REG_FRMCTR1  s\" \x00\x18" self cmd-args>st7796s-text
      REG_DFUNCTR  s\" \x80\x02\x3B" self cmd-args>st7796s-text \ Display Function Control
      $F2 s\" \x00" self cmd-args>st7796s-text                         \ 3Gamma Function Disable
      REG_GAMMASET  s\" \x01" self cmd-args>st7796s-text             \ Gamma curve selected
      REG_GMCTRP1  s\" \x0F\x31\x2B\x0C\x0E\x08\x4E\xF1\x37\x07\x10\x03\x0E\x09\x00" self cmd-args>st7796s-text \ Set Gamma
      REG_GMCTRN1  s\" \x00\x0E\x14\x03\x11\x07\x31\xC1\x48\x08\x0F\x0C\x31\x36\x0F" self cmd-args>st7796s-text \ Set Gamma
      REG_SLPOUT  self cmd>st7796s-text \ Exit Sleep
      120 ms
      REG_DISPON  self cmd>st7796s-text \ Display on
      120 ms
      \ $00         \ NOP, End of list

      true self backlight!
    ; define init-st7796s-text

    \ Send a command to the ST7796S-text
    :noname { W^ cmd self -- }
      low self st7796s-text-dc-pin @ pin!
      low self st7796s-text-cs-pin @ pin!
      cmd 1 self >st7796s-text
      high self st7796s-text-cs-pin @ pin!
    ; define cmd>st7796s-text

    \ Send a command with arguments to the ST7796S-text
    :noname { W^ cmd addr count self -- }
      low self st7796s-text-dc-pin @ pin!
      low self st7796s-text-cs-pin @ pin!
      cmd 1 self >st7796s-text
      high self st7796s-text-dc-pin @ pin!
      addr count self >st7796s-text
      high self st7796s-text-cs-pin @ pin!
    ; define cmd-args>st7796s-text

    \ Set the entire display to be dirty
    :noname { self -- }
      self dim@ { cols rows }
      0 self st7796s-text-dirty-start-col !
      cols self st7796s-text-dirty-end-col !
      0 self st7796s-text-dirty-start-row !
      rows self st7796s-text-dirty-end-row !
    ; define set-dirty

    \ Clear dirty rectangle
    :noname { self -- }
      0 self st7796s-text-dirty-start-col !
      0 self st7796s-text-dirty-end-col !
      0 self st7796s-text-dirty-start-row !
      0 self st7796s-text-dirty-end-row !
    ; define clear-dirty
    
    \ Get whether an ST7796S-text device is dirty
    :noname { self -- dirty? }
      self st7796s-text-dirty-start-col @ self st7796s-text-dirty-end-col @ <>
      self st7796s-text-dirty-start-row @ self st7796s-text-dirty-end-row @ <> and
    ; define dirty?
  
    \ Dirty a character on an ST7796S-text device
    :noname { col row self -- }
      self dirty? if
        row self st7796s-text-dirty-start-row @ min
        self st7796s-text-dirty-start-row !
        row 1+ self st7796s-text-dirty-end-row @ max
        self st7796s-text-dirty-end-row !
        col self st7796s-text-dirty-start-col @ min
        self st7796s-text-dirty-start-col !
        col 1+ self st7796s-text-dirty-end-col @ max
        self st7796s-text-dirty-end-col !
      else
        row self st7796s-text-dirty-start-row !
        row 1+ self st7796s-text-dirty-end-row !
        col self st7796s-text-dirty-start-col !
        col 1+ self st7796s-text-dirty-end-col !
      then
    ; define dirty-char

    \ Dirty an area on an ST7796S-text device
    :noname { start-col end-col start-row end-row self -- }
      start-col end-col < start-row end-row < and if
        start-col start-row self dirty-char
        end-col 1- end-row 1- self dirty-char
      then
    ; define dirty-area
    
    \ Set the ST7796S-text window
    :noname { start-col end-col start-row end-row self -- }
      self st7796s-text-col-offset @ +to start-col
      self st7796s-text-col-offset @ 1- +to end-col
      self st7796s-text-row-offset @ +to start-row
      self st7796s-text-row-offset @ 1- +to end-row
      0 0 { W^ col-values W^ row-values }
      start-col 8 rshift col-values c!
      start-col col-values 1 + c!
      end-col 8 rshift col-values 2 + c!
      end-col col-values 3 + c!
      start-row 8 rshift row-values c!
      start-row row-values 1 + c!
      end-row 8 rshift row-values 2 + c!
      end-row row-values 3 + c!
      REG_CASET col-values 4 self cmd-args>st7796s-text
      REG_RASET row-values 4 self cmd-args>st7796s-text

    ; define st7796s-text-window!

    \ Update a rectangular space on the ST7796S device
    :noname { start-col end-col start-row end-row self -- }
      self char-dim@ { char-cols char-rows }
      start-col char-cols * to start-col
      end-col char-cols * to end-col
      start-row char-rows * to start-row
      end-row char-rows * to end-row
      start-col end-col start-row end-row self st7796s-text-window!
      low self st7796s-text-dc-pin @ pin!
      low self st7796s-text-cs-pin @ pin!
      REG_RAMWR { W^ cmd }
      cmd 1 self >st7796s-text
      high self st7796s-text-dc-pin @ pin!
      end-row start-row ?do
        self start-col i end-col start-col - dup 1 lshift [:
          { self start-col row cols line-buf }
          start-col row cols line-buf self populate-row
          line-buf cols 1 lshift self >st7796s-text
        ;] with-aligned-allot
      loop
      high self st7796s-text-cs-pin @ pin!
    ; define update-area
    
    \ Populate a row
    :noname { start-col row cols line-buf self -- }
      self st7796s-text-font @ { the-font }
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

    \ Clear the ST7796S-text device
    :noname { self -- }
      self st7796s-text-phys-cols @ { cols }
      self st7796s-text-phys-rows @ { rows }
      0 cols 0 rows self st7796s-text-window!
      low self st7796s-text-dc-pin @ pin!
      low self st7796s-text-cs-pin @ pin!
      REG_RAMWR { W^ cmd }
      cmd 1 self >st7796s-text
      high self st7796s-text-dc-pin @ pin!
      self rows cols dup 1 lshift [: { self rows cols line-buf }
        line-buf cols 1 lshift 0 fill
        rows 0 ?do line-buf cols 1 lshift self >st7796s-text loop
      ;] with-allot
      high self st7796s-text-cs-pin @ pin!
    ; define clear-display

    \ Get the character dimensions
    :noname { self -- cols rows }
      self st7796s-text-font @ { the-font }
      the-font char-cols @ the-font char-rows @
    ; define char-dim@

    \ Set the backlight (this may be a no-op)
    :noname { backlight self -- }
      backlight self st7796s-text-backlight-pin @ pin!
    ; define backlight!

    \ Update the ST7796S device
    :noname { self -- }
      self dirty? if 
        self st7796s-text-dirty-start-col @
        self st7796s-text-dirty-end-col @
        self st7796s-text-dirty-start-row @
        self st7796s-text-dirty-end-row @
        self update-area
        self clear-dirty
      then
    ; define update-display

  end-implement
  
end-module
