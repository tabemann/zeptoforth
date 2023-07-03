\ Copyright (c) 2023 Travis Bemann
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

begin-module i2c-scanner

  pin import
  i2c import
  
  : test-i2c-addr { addr pin0 pin1 i2c-periph -- }
    i2c-periph i2c::clear-i2c
    i2c-periph pin0 i2c::i2c-pin
    i2c-periph pin1 i2c::i2c-pin
    i2c-periph i2c::master-i2c
    addr $80 u< if i2c-periph 7-bit-i2c-addr else i2c-periph 10-bit-i2c-addr then
    addr i2c-periph ['] i2c::i2c-target-addr! try 0= if
      i2c-periph i2c::enable-i2c
      0. { D^ buf } buf 1 i2c-periph ['] i2c::>i2c-stop try
      i2c-periph i2c::disable-i2c
      addr h.4 ." : "
      ?dup if
        execute 2drop drop
      else
        drop ." FOUND" cr
      then
    else
      2drop
    then
  ;
  
  : scan-i2c { pin0 pin1 i2c-periph -- }
    cr $400 $000 ?do i pin0 pin1 i2c-periph test-i2c-addr loop 
  ;
  
end-module