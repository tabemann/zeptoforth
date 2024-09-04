\ Copyright (c) 2024 Travis Bemann
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

\ Find a prescale for a baud rate
: find-spi-prescale ( baud sysclk -- prescale )
  { sysclk }
  s>d { D: baud }
  2. begin 2dup 254. d<= while
    2dup 2. d+ 256. d* baud d* sysclk s>d d> if
      d>s exit
    else
      2. d+
    then
  repeat
  d>s
;

\ Find a postdiv for a baud rate and prescale
: find-spi-postdiv ( baud prescale sysclk -- postdiv )
  { sysclk }
  256 begin dup 1 > while
    2dup 1- * sysclk swap / 3 pick > if
      nip nip exit
    else
      1-
    then
  repeat
  nip nip
;

\ Calculate an actual SPI clock
: spi-clock { baud sysclk -- real-baud }
  baud sysclk find-spi-prescale { prescale }
  baud prescale sysclk find-spi-postdiv { postdiv }
  sysclk prescale / postdiv /
;

\ SPI clock test
: spi-clock-test ( -- )
  false { prev-met }
  75_000_000 2_000_000 ?do
    i 125_000_000 spi-clock { clock-125-MHz }
    i 150_000_000 spi-clock { clock-150-MHz }
    clock-125-MHz clock-150-MHz > if
      prev-met not if
        cr i . clock-125-MHz . clock-150-MHz .
      else
        i 1+ 125_000_000 spi-clock i 1+ 150_000_000 spi-clock <= if
          cr i . clock-125-MHz . clock-150-MHz .
        then
      then
      true to prev-met
    else
      false to prev-met
    then
  10000 +loop
;