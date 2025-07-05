\ Copyright (c) 2025 Travis Bemann
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

compile-to-flash

begin-module voltage

  begin-module voltage-internal

    \ POWMAN base address
    $4010_0000 constant POWMAN_BASE

    \ POWMAN password
    $5AFE_0000 constant POWMAN_PASSWD

    \ Voltage regulator control register address
    POWMAN_BASE $04 + constant POWMAN_VREG_CTRL

    \ Not reset voltage regular to default setting
    15 bit constant POWMAN_VREG_CTRL_RST_N

    \ Unlock the voltage regulator after startup (it cannot be relocked)
    13 bit constant POWMAN_VREG_CTRL_UNLOCK

    \ Disable the voltage limit
    8 bit constant POWMAN_VREG_CTRL_DISABLE_VOLTAGE_LIMIT

    \ Voltage regulator settings register address
    POWMAN_BASE $0C + constant POWMAN_VREG
    
    \ Voltage regulator update in progress
    15 bit constant POWMAN_VREG_UPDATE_IN_PROGRESS

    \ Voltage regulator output voltage select LSB
    4 constant POWMAN_VREG_VSEL_LSB
    
    \ Voltage regulator output voltage select mask
    $1F POWMAN_VREG_VSEL_LSB lshift constant POWMAN_VREG_VSEL_MASK

    \ Wait for the voltage regular to be ready
    : wait-ready ( -- )
      begin POWMAN_VREG_UPDATE_IN_PROGRESS POWMAN_VREG bit@ not until
    ;

    \ Voltage table
    create voltage-table
    550 h,
    600 h,
    650 h,
    700 h,
    750 h,
    800 h,
    850 h,
    900 h,
    950 h,
    1000 h,
    1050 h,
    1100 h,
    1150 h,
    1200 h,
    1250 h,
    1300 h,
    1350 h,
    1400 h,
    1500 h,
    1600 h,
    1650 h,
    1700 h,
    1800 h,
    1900 h,
    2000 h,
    2350 h,
    2500 h,
    2650 h,
    2800 h,
    3000 h,
    3150 h,
    3300 h,

    \ Convert an index to a voltage
    : convert-to-voltage { index -- mvolts }
      index 1 lshift voltage-table + h@
    ;

    \ Convert a voltage to an index
    : convert-from-voltage { mvolts -- index }
      mvolts 550 max 3300 min to mvolts
      $1F begin
        dup 1 lshift voltage-table + h@ mvolts <= if exit then 1-
      again
    ;
    
  end-module> import

  \ Unsupported voltage exception
  : x-unsupported-voltage ( -- ) ." unsupported voltage" cr ;

  \ Get the voltage in mV
  : voltage@ { -- mvolts }
    POWMAN_VREG @ POWMAN_VREG_VSEL_MASK and POWMAN_VREG_VSEL_LSB rshift
    convert-to-voltage
  ;
  
  \ Set the voltage regulator -- voltage is specified in mV
  : set-voltage { mvolts -- }
    mvolts 550 >= mvolts 1300 <= and averts x-unsupported-voltage
    wait-ready
    [ POWMAN_PASSWD POWMAN_VREG_CTRL_UNLOCK or ] literal POWMAN_VREG_CTRL bis!
    wait-ready
    mvolts convert-from-voltage POWMAN_VREG_VSEL_LSB lshift
    POWMAN_PASSWD or POWMAN_VREG !
    wait-ready
  ;

  \ Set the voltage regulator with disabled voltage limit -- voltage is
  \ specified in mV. WARNING: This may brick your RP2040 or RP2350.
  : set-voltage-unlimited { mvolts -- }
    mvolts 550 >= mvolts 3300 <= and averts x-unsupported-voltage
    wait-ready
    [ POWMAN_PASSWD POWMAN_VREG_CTRL_UNLOCK or ] literal POWMAN_VREG_CTRL bis!
    [ POWMAN_PASSWD POWMAN_VREG_CTRL_DISABLE_VOLTAGE_LIMIT or ] literal
    POWMAN_VREG_CTRL bis!
    wait-ready
    mvolts convert-from-voltage POWMAN_VREG_VSEL_LSB lshift
    POWMAN_PASSWD or POWMAN_VREG !
    wait-ready
  ;

end-module

reboot
