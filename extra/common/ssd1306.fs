\ Copyright (c) 2022-2023 Travis Bemann
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

begin-module ssd1306

  oo import
  i2c import
  bitmap import
  bitmap-internal import

  \ An I2C SSD1306 device
  <bitmap> begin-class <ssd1306>
    
    begin-module ssd1306-internal

      \ I2C device
      cell member ssd1306-device

      \ I2C address
      cell member ssd1306-addr

      \ I2C pin 0 (SCK or SDA, does not matter)
      cell member ssd1306-pin0

      \ I2C pin 1 (SCK or SDA, does not matter)
      cell member ssd1306-pin1
      
      \ Command base register
      cell member ssd1306-cmd-base

      \ Dirty rectangle start column
      cell member ssd1306-dirty-start-col

      \ Dirty rectangle end column
      cell member ssd1306-dirty-end-col

      \ Dirty rectangle start row
      cell member ssd1306-dirty-start-row

      \ Dirty rectangle end row
      cell member ssd1306-dirty-end-row
      
    end-module> import

    \ Set the display contrast ( $FF highest, $00 lowest)
    method display-contrast! ( contrast ssd1306 -- )
    
    \ Update the SSD1306 device
    method update-display ( ssd1306 -- )
    
  end-class

  \ Default I2C address
  $3C constant SSD1306_I2C_ADDR

  continue-module ssd1306-internal
    
    \ SSD1306 Commands - see Datasheet

    \ Indicates following bytes are commands
    $00 constant SSD1306_CMD_START

    \ Indicates following bytes are data
    $40 constant SSD1306_DATA_START

    \ Fundamental Command Table (p. 28)

    \ Double-byte command to set contrast (1-256)
    $81 constant SSD1306_SETCONTRAST

    \ Set entire display on
    $A5 constant SSD1306_ENTIREDISPLAY_ON

    \ Use RAM contents for display
    $A4 constant SSD1306_ENTIREDISPLAY_OFF

    \ Invert RAM contents to display
    $A7 constant SSD1306_SETINVERT_ON

    \ Normal display
    $A6 constant SSD1306_SETINVERT_OFF

    \ Display OFF (sleep mode)
    $AE constant SSD1306_SETDISPLAY_OFF

    \ Display ON (normal mode)
    $AF constant SSD1306_SETDISPLAY_ON

    \ Scrolling Command Table (pp. 28-30)

    \ Configure right horizontal scroll
    $26 constant SSD1306_SCROLL_SETUP_H_RIGHT

    \ Configure left horizontal scroll
    $27 constant SSD1306_SCROLL_SETUP_H_LEFT

    \ Configure right & vertical scroll
    $29 constant SSD1306_SCROLL_SETUP_HV_RIGHT

    \ Configure left & vertical scroll
    $2A constant SSD1306_SCROLL_SETUP_HV_LEFT

    \ Configure vertical scroll area
    $A3 constant SSD1306_SCROLL_SETUP_V

    \ Stop scrolling
    $2E constant SSD1306_SCROLL_DEACTIVATE

    \ Addressing Setting Command Table (pp. 30-31)

    \ Start scrolling
    $2F constant SSD1306_SCROLL_ACTIVATE

    \ Set lower 4 bits of column start address by ORing 4 LSBs
    $00 constant SSD1306_PAGE_COLSTART_LOW

    \ Set upper 4 bits of column start address by ORing 4 LSBs
    $10 constant SSD1306_PAGE_COLSTART_HIGH

    \ Set page start address by ORing 4 LSBs
    $B0 constant SSD1306_PAGE_PAGESTART

    \ Set addressing mode (horizontal, vertical, or page)
    $20 constant SSD1306_SETADDRESSMODE

    \ Send 2 more bytes to set start and end columns for hor/vert modes
    $21 constant SSD1306_SETCOLRANGE

    \ Send 2 more bytes to set start and end pages
    $22 constant SSD1306_SETPAGERANGE

    \ Hardware Configuration Commands (p. 31)

    \ Set RAM display start line by ORing 6 LSBs
    $40 constant SSD1306_SETSTARTLINE

    \ Set column address 0 to display column 0
    $A0 constant SSD1306_COLSCAN_ASCENDING

    \ Set column address 127 to display column 127
    $A1 constant SSD1306_COLSCAN_DESCENDING

    \ Set size of multiplexer based on display height (31 for 32 rows)
    $A8 constant SSD1306_SETMULTIPLEX

    \ Set COM 0 to display row 0
    $C0 constant SSD1306_COMSCAN_ASCENDING

    \ Set COM N-1 to display row 0
    $C8 constant SSD1306_COMSCAN_DESCENDING

    \ Set display vertical shift
    $D3 constant SSD1306_VERTICALOFFSET

    \ Set COM pin hardware configuration
    $DA constant SSD1306_SETCOMPINS

    \ Timing and Driving Scheme Settings Commands (p. 32)

    \ Set display clock divide ratio and frequency
    $D5 constant SSD1306_SETDISPLAYCLOCKDIV

    \ Set pre-charge period
    $D9 constant SSD1306_SETPRECHARGE

    \ Set V_COMH voltage level
    $DB constant SSD1306_SETVCOMLEVEL

    \ No operation
    $E3 constant SSD1306_NOP

    \ Charge Pump Commands (p. 62)

    \ Enable / disable charge pump
    $8D constant SSD1306_SETCHARGEPUMP
    
    \ Initialize I2C for an SSD1306 device
    : init-i2c { self -- }
      self ssd1306-device @ { device }
      device self ssd1306-pin0 @ i2c-pin
      device self ssd1306-pin1 @ i2c-pin
      device master-i2c
      device 7-bit-i2c-addr
      self ssd1306-addr @ device i2c-target-addr!
      device enable-i2c
    ;

    \ Begin constructing a command to send to the SSD1306
    : begin-cmd { self -- }
      ram-here self ssd1306-cmd-base !
    ;
    
    \ Write a byte to the current command being constructed
    : >cmd ( c -- ) cram, ;
    
    \ Send the current command that has been constructed
    : send-cmd { self -- }
      self [: dup { self }
        self ssd1306-cmd-base @ dup ram-here swap -
        self ssd1306-device @ >i2c-stop drop
      ;] try nip self ssd1306-cmd-base @ ram-here! ?raise
    ;

    \ Initialize an SSD1306 display
    : init-display { self -- }
      self begin-cmd
      \ Begin a command
      SSD1306_CMD_START >cmd
      \ Turn off display
      SSD1306_SETDISPLAY_OFF >cmd
      \ Set the clock to Fosc = 8, divide ratio = 1
      SSD1306_SETDISPLAYCLOCKDIV >cmd $80 >cmd
      \ Set the display multiplexer to the number of rows - 1
      SSD1306_SETMULTIPLEX >cmd self bitmap-rows @ 1 - >cmd
      \ Set the vertical offset to 0
      SSD1306_VERTICALOFFSET >cmd 0 >cmd
      \ RAM start line 0
      SSD1306_SETSTARTLINE $00 or >cmd
      \ Set the charge pump on
      SSD1306_SETCHARGEPUMP >cmd $14 >cmd
      \ Set the addressing mode to horizontal mode
      SSD1306_SETADDRESSMODE >cmd $00 >cmd
      \ Set flip columns
      SSD1306_COLSCAN_DESCENDING >cmd
      \ Set to not flip pages
      SSD1306_COMSCAN_DESCENDING >cmd
      \ Set COM pins to alternative COM pin mode
      SSD1306_SETCOMPINS >cmd $12 >cmd
      \ Set contrast to minimal
      SSD1306_SETCONTRAST >cmd $01 >cmd
      \ Set precharge period to phase1 = 15, phase2 = 1
      SSD1306_SETPRECHARGE >cmd $F1 >cmd
      \ Set VCOMH deselect level to (0, 2, 3)
      SSD1306_SETVCOMLEVEL >cmd $40 >cmd
      \ Set u se RAM contents for display
      SSD1306_ENTIREDISPLAY_OFF >cmd
      \ Set no inversion
      SSD1306_SETINVERT_OFF >cmd
      \ Set no scrolling
      SSD1306_SCROLL_DEACTIVATE >cmd
      \ Turn on display in normal mode
      SSD1306_SETDISPLAY_ON >cmd
      self send-cmd
    ;

    \ Send a row of data to the SSD1305 device
    : send-row { start-col end-col page self -- }
      self begin-cmd SSD1306_DATA_START >cmd
      page self page-addr dup end-col + swap start-col + ?do i c@ >cmd loop
      self send-cmd
    ;
    
    \ Send an area of data to the SSD1306 device
    : send-area { start-col end-col start-page end-page self -- }
      end-page start-page ?do start-col end-col i self send-row loop
    ;
    
    \ Update a rectangular space on the SSD1306 device
    : update-area { start-col end-col start-row end-row self -- }
      start-row 3 rshift { start-page }
      end-row 8 align 3 rshift { end-page }
      start-page end-page < if
        self begin-cmd
        SSD1306_CMD_START >cmd
        SSD1306_SETPAGERANGE >cmd start-page >cmd end-page 1- >cmd
        SSD1306_SETCOLRANGE >cmd start-col >cmd end-col 1- >cmd    
        self send-cmd
        start-col end-col start-page end-page self send-area
      then
    ;
    
  end-module

  <ssd1306> begin-implement
  
    \ Initialize an SSD1306 device
    :noname { pin0 pin1 buf cols rows i2c-addr i2c-device self -- }
      buf cols rows self <bitmap>->new
      i2c-device self ssd1306-device !
      i2c-addr self ssd1306-addr !
      pin1 self ssd1306-pin1 !
      pin0 self ssd1306-pin0 !
      self init-i2c
      self init-display
    ; define new

    \ Set the entire display to be dirty
    :noname { self -- }
      0 self ssd1306-dirty-start-col !
      self bitmap-cols @ self ssd1306-dirty-end-col !
      0 self ssd1306-dirty-start-row !
      self bitmap-rows @ self ssd1306-dirty-end-row !
    ; define set-dirty

    \ Clear dirty rectangle
    :noname { self -- }
      0 self ssd1306-dirty-start-col !
      0 self ssd1306-dirty-end-col !
      0 self ssd1306-dirty-start-row !
      0 self ssd1306-dirty-end-row !
    ; define clear-dirty
    
    \ Get whether an SSD1306 device is dirty
    :noname { self -- dirty? }
      self ssd1306-dirty-start-col @ self ssd1306-dirty-end-col @ <>
      self ssd1306-dirty-start-row @ self ssd1306-dirty-end-row @ <> and
    ; define dirty?
  
    \ Dirty a pixel on an SSD1306 device
    :noname { col row self -- }
      self dirty? if
        row self ssd1306-dirty-start-row @ min self ssd1306-dirty-start-row !
        row 1+ self ssd1306-dirty-end-row @ max self ssd1306-dirty-end-row !
        col self ssd1306-dirty-start-col @ min self ssd1306-dirty-start-col !
        col 1+ self ssd1306-dirty-end-col @ max self ssd1306-dirty-end-col !
      else
        row self ssd1306-dirty-start-row !
        row 1+ self ssd1306-dirty-end-row !
        col self ssd1306-dirty-start-col !
        col 1+ self ssd1306-dirty-end-col !
      then
    ; define dirty-pixel

    \ Dirty an area on an SSD1306 device
    :noname { start-col end-col start-row end-row self -- }
      start-col end-col < start-row end-row < and if
        start-col start-row self dirty-pixel
        end-col 1- end-row 1- self dirty-pixel
      then
    ; define dirty-area

    \ Change the SSD1306 device contrast
    :noname { contrast self -- }
      self begin-cmd
      SSD1306_CMD_START >cmd
      SSD1306_SETCONTRAST >cmd contrast $FF and >cmd
      self send-cmd
    ; define display-contrast!

    \ Update the SSD1306 device
    :noname { self -- }
      self dirty? if
        self ssd1306-dirty-start-col @
        self ssd1306-dirty-end-col @
        self ssd1306-dirty-start-row @
        self ssd1306-dirty-end-row @
        self update-area
        self clear-dirty
      then
    ; define update-display

  end-implement
  
end-module