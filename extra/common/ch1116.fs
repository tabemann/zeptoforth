\ Copyright (c) 2022-2025 Travis Bemann
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

begin-module ch1116

  oo import
  i2c import
  bitmap import
  bitmap-internal import

  \ An I2C CH1116 device
  <bitmap> begin-class <ch1116>
    
    begin-module ch1116-internal

      \ I2C device
      cell member ch1116-device

      \ I2C address
      cell member ch1116-addr

      \ I2C pin 0 (SCK or SDA, does not matter)
      cell member ch1116-pin0

      \ I2C pin 1 (SCK or SDA, does not matter)
      cell member ch1116-pin1
      
      \ Command base register
      cell member ch1116-cmd-base

      \ Dirty rectangle start column
      cell member ch1116-dirty-start-col

      \ Dirty rectangle end column
      cell member ch1116-dirty-end-col

      \ Dirty rectangle start row
      cell member ch1116-dirty-start-row

      \ Dirty rectangle end row
      cell member ch1116-dirty-end-row
      
    end-module> import

    \ Set the display contrast ( $FF highest, $00 lowest)
    method display-contrast! ( contrast ch1116 -- )
    
    \ Update the CH1116 device
    method update-display ( ch1116 -- )
    
  end-class

  \ Default I2C address
  $3C constant CH1116_I2C_ADDR

  continue-module ch1116-internal
    
    \ CH1116 Commands - see Datasheet

    \ Indicates following bytes are commands
    $00 constant CH1116_CMD_START

    \ Indicates following bytes are data
    $40 constant CH1116_DATA_START

    \ Fundamental Command Table (p. 28)

    \ Double-byte command to set contrast (1-256)
    $81 constant CH1116_SETCONTRAST

    \ Set entire display on
    $A5 constant CH1116_ENTIREDISPLAY_ON

    \ Use RAM contents for display
    $A4 constant CH1116_ENTIREDISPLAY_OFF

    \ Invert RAM contents to display
    $A7 constant CH1116_SETINVERT_ON

    \ Normal display
    $A6 constant CH1116_SETINVERT_OFF

    \ Display OFF (sleep mode)
    $AE constant CH1116_SETDISPLAY_OFF

    \ Display ON (normal mode)
    $AF constant CH1116_SETDISPLAY_ON

    \ Scrolling Command Table (pp. 28-30)

    \ Configure right horizontal scroll
    $26 constant CH1116_SCROLL_SETUP_H_RIGHT

    \ Configure left horizontal scroll
    $27 constant CH1116_SCROLL_SETUP_H_LEFT

    \ Configure right & vertical scroll
    $29 constant CH1116_SCROLL_SETUP_HV_RIGHT

    \ Configure left & vertical scroll
    $2A constant CH1116_SCROLL_SETUP_HV_LEFT

    \ Configure vertical scroll area
    $A3 constant CH1116_SCROLL_SETUP_V

    \ Stop scrolling
    $2E constant CH1116_SCROLL_DEACTIVATE

    \ Addressing Setting Command Table (pp. 30-31)

    \ Start scrolling
    $2F constant CH1116_SCROLL_ACTIVATE

    \ Set lower 4 bits of column start address by ORing 4 LSBs
    $00 constant CH1116_PAGE_COLSTART_LOW

    \ Set upper 4 bits of column start address by ORing 4 LSBs
    $10 constant CH1116_PAGE_COLSTART_HIGH

    \ Set page start address by ORing 4 LSBs
    $B0 constant CH1116_PAGE_PAGESTART
    \ Hardware Configuration Commands (p. 31)

    \ Set RAM display start line by ORing 6 LSBs
    $40 constant CH1116_SETSTARTLINE

    \ Set column address 0 to display column 0
    $A0 constant CH1116_COLSCAN_ASCENDING

    \ Set column address 127 to display column 127
    $A1 constant CH1116_COLSCAN_DESCENDING

    \ Set size of multiplexer based on display height (31 for 32 rows)
    $A8 constant CH1116_SETMULTIPLEX

    \ Set COM 0 to display row 0
    $C0 constant CH1116_COMSCAN_ASCENDING

    \ Set COM N-1 to display row 0
    $C8 constant CH1116_COMSCAN_DESCENDING

    \ Set display vertical shift
    $D3 constant CH1116_VERTICALOFFSET

    \ Set COM pin hardware configuration
    $DA constant CH1116_SETCOMPINS

    \ Timing and Driving Scheme Settings Commands (p. 32)

    \ Set display clock divide ratio and frequency
    $D5 constant CH1116_SETDISPLAYCLOCKDIV

    \ Set pre-charge period
    $D9 constant CH1116_SETPRECHARGE

    \ Set V_COMH voltage level
    $DB constant CH1116_SETVCOMLEVEL

    \ No operation
    $E3 constant CH1116_NOP

    \ Charge Pump Commands (p. 62)

    \ Enable / disable charge pump
    $8D constant CH1116_SETCHARGEPUMP
    
    \ Initialize I2C for an CH1116 device
    : init-i2c { self -- }
      self ch1116-device @ { device }
      device self ch1116-pin0 @ i2c-pin
      device self ch1116-pin1 @ i2c-pin
      device master-i2c
      device 7-bit-i2c-addr
      self ch1116-addr @ device i2c-target-addr!
      device enable-i2c
    ;

    \ Begin constructing a command to send to the CH1116
    : begin-cmd { self -- }
      ram-here self ch1116-cmd-base !
    ;
    
    \ Write a byte to the current command being constructed
    : >cmd ( c -- ) cram, ;
    
    \ Send the current command that has been constructed
    : send-cmd { self -- }
      self [: dup { self }
        self ch1116-cmd-base @ dup ram-here swap -
        self ch1116-device @ >i2c-stop drop
      ;] try nip self ch1116-cmd-base @ ram-here! ?raise
    ;

    \ Initialize an CH1116 display
    : init-display { self -- }
      self begin-cmd
      \ Begin a command
      CH1116_CMD_START >cmd
      \ Turn off display
      CH1116_SETDISPLAY_OFF >cmd
      \ Set the clock to Fosc = 8, divide ratio = 1
      CH1116_SETDISPLAYCLOCKDIV >cmd $80 >cmd
      \ Set the display multiplexer to the number of rows - 1
      CH1116_SETMULTIPLEX >cmd self bitmap-rows @ 1 - >cmd
      \ Set the vertical offset to 0
      CH1116_VERTICALOFFSET >cmd 0 >cmd
      \ RAM start line 0
      CH1116_SETSTARTLINE $00 or >cmd
      \ Set the charge pump on
      CH1116_SETCHARGEPUMP >cmd $14 >cmd
      \ Set flip columns
      CH1116_COLSCAN_DESCENDING >cmd
      \ Set to not flip pages
      CH1116_COMSCAN_DESCENDING >cmd
      \ Set COM pins mode
      CH1116_SETCOMPINS >cmd self bitmap-rows @ 32 > if $12 else $02 then >cmd
      \ Set contrast to minimal
      CH1116_SETCONTRAST >cmd $01 >cmd
      \ Set precharge period to phase1 = 15, phase2 = 1
      CH1116_SETPRECHARGE >cmd $F1 >cmd
      \ Set VCOMH deselect level to (0, 2, 3)
      CH1116_SETVCOMLEVEL >cmd $40 >cmd
      \ Set u se RAM contents for display
      CH1116_ENTIREDISPLAY_OFF >cmd
      \ Set no inversion
      CH1116_SETINVERT_OFF >cmd
      \ Set no scrolling
      CH1116_SCROLL_DEACTIVATE >cmd
      \ Turn on display in normal mode
      CH1116_SETDISPLAY_ON >cmd
      self send-cmd
    ;

    \ Send a row of data to the SSD1305 device
    : send-row { start-col end-col page self -- }
      self begin-cmd CH1116_DATA_START >cmd
      page self page-addr dup end-col + swap start-col + ?do i c@ >cmd loop
      self send-cmd
    ;
    
    \ Send an area of data to the CH1116 device
    : send-area { start-col end-col start-page end-page self -- }
      end-page start-page ?do start-col end-col i self send-row loop
    ;
    
    \ Update a rectangular space on the CH1116 device
    : update-area { start-col end-col start-row end-row self -- }
      start-row 3 rshift { start-page }
      end-row 8 align 3 rshift { end-page }
      end-page start-page ?do
        self begin-cmd
        CH1116_CMD_START >cmd
        CH1116_PAGE_COLSTART_LOW start-col $F and or >cmd
        CH1116_PAGE_COLSTART_HIGH start-col 4 rshift $F and or >cmd
        CH1116_PAGE_PAGESTART i $F and or >cmd
        self send-cmd
        start-col end-col i i 1+ self send-area
      loop
    ;
    
  end-module

  <ch1116> begin-implement
  
    \ Initialize an CH1116 device
    :noname { pin0 pin1 buf cols rows i2c-addr i2c-device self -- }
      buf cols rows self <bitmap>->new
      i2c-device self ch1116-device !
      i2c-addr self ch1116-addr !
      pin1 self ch1116-pin1 !
      pin0 self ch1116-pin0 !
      self init-i2c
      self init-display
    ; define new

    \ Set the entire display to be dirty
    :noname { self -- }
      0 self ch1116-dirty-start-col !
      self bitmap-cols @ self ch1116-dirty-end-col !
      0 self ch1116-dirty-start-row !
      self bitmap-rows @ self ch1116-dirty-end-row !
    ; define set-dirty

    \ Clear dirty rectangle
    :noname { self -- }
      0 self ch1116-dirty-start-col !
      0 self ch1116-dirty-end-col !
      0 self ch1116-dirty-start-row !
      0 self ch1116-dirty-end-row !
    ; define clear-dirty
    
    \ Get whether an CH1116 device is dirty
    :noname { self -- dirty? }
      self ch1116-dirty-start-col @ self ch1116-dirty-end-col @ <>
      self ch1116-dirty-start-row @ self ch1116-dirty-end-row @ <> and
    ; define dirty?
  
    \ Dirty a pixel on an CH1116 device
    :noname { col row self -- }
      self dirty? if
        row self ch1116-dirty-start-row @ min self ch1116-dirty-start-row !
        row 1+ self ch1116-dirty-end-row @ max self ch1116-dirty-end-row !
        col self ch1116-dirty-start-col @ min self ch1116-dirty-start-col !
        col 1+ self ch1116-dirty-end-col @ max self ch1116-dirty-end-col !
      else
        row self ch1116-dirty-start-row !
        row 1+ self ch1116-dirty-end-row !
        col self ch1116-dirty-start-col !
        col 1+ self ch1116-dirty-end-col !
      then
    ; define dirty-pixel

    \ Dirty an area on an CH1116 device
    :noname { start-col end-col start-row end-row self -- }
      start-col end-col < start-row end-row < and if
        start-col start-row self dirty-pixel
        end-col 1- end-row 1- self dirty-pixel
      then
    ; define dirty-area

    \ Change the CH1116 device contrast
    :noname { contrast self -- }
      self begin-cmd
      CH1116_CMD_START >cmd
      CH1116_SETCONTRAST >cmd contrast $FF and >cmd
      self send-cmd
    ; define display-contrast!

    \ Update the CH1116 device
    :noname { self -- }
      self dirty? if
        self ch1116-dirty-start-col @
        self ch1116-dirty-end-col @
        self ch1116-dirty-start-row @
        self ch1116-dirty-end-row @
        self update-area
        self clear-dirty
      then
    ; define update-display

  end-implement
  
end-module