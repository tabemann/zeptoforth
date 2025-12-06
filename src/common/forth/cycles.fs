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

begin-module cycles

  begin-module cycles-internal
  
    $E0001000 constant DWT_CONTROL
    $E0001004 constant DWT_CYCCNT
    $E0001FB0 constant DWT_LAR
    $E000EDFC constant SCB_DEMCR
    
  end-module> import
  
  \ Initialize the cycle counter at 0
  : init-cycles ( -- )
    $C5ACCE55 DWT_LAR !
    $01000000 SCB_DEMCR !
    0 DWT_CYCCNT !
    1 DWT_CONTROL !
  ;
  
  \ Get the current cycle count
  : cycle-counter ( -- cycles ) DWT_CYCCNT @ ;

  \ Wait a given number of cycles (note that this is a spinwait)
  : wait-cycles { cycles -- }
    cycle-counter { start-cycle }
    begin cycle-counter start-cycle - cycles > until
  ;
  
end-module

reboot
