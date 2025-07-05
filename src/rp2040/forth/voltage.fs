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

    \ VREG and chip reset base address
    $40064000 constant VREG_AND_CHIP_RESET_BASE

    \ VREG control register
    VREG_AND_CHIP_RESET_BASE $00 + constant VREG

    \ Voltage regulator output voltage select LSB
    4 constant VREG_VSEL_LSB
    
    \ Voltage regulator output voltage select mask
    $F VREG_VSEL_LSB lshift constant VREG_VSEL_MASK

    \ Voltage regulator enabled
    0 bit constant VREG_EN
    
  end-module> import

  \ Unsupported voltage exception
  : x-unsupported-voltage ( -- ) ." unsupported voltage" cr ;

  \ Get the voltage in mV
  : voltage@ { -- mvolts }
    VREG @ VREG_VSEL_MASK and VREG_VSEL_LSB rshift 50 * 550 +
  ;
  
  \ Set the voltage regulator -- voltage is specified in mV
  : set-voltage { mvolts -- }
    mvolts 850 >= mvolts 1300 <= and averts x-unsupported-voltage
    mvolts 550 - 50 / VREG_VSEL_LSB lshift VREG_EN or VREG !
  ;

  \ Set the voltage regulator with disabled voltage limit -- this exists for
  \ compatibility with the RP2350
  : set-voltage-unlimited ( mvolts -- ) set-voltage ;
  
end-module

reboot
