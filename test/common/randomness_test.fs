\ Copyright (c) 2021 Travis Bemann
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

continue-module forth-module

  rng-module import

  \ Number of bits in a random number
  32 constant bit-count

  \ Initialize statistics
  : init-rng-stats ( stats-addr -- ) bit-count cells 0 fill ;

  \ Print statistics
  : rng-stats. ( stats-addr count -- )
    bit-count 0 ?do
      cr ." Bit" i . ." :" over i cells + @ over 0 -rot 0 swap f/ f.
    loop
    0 bit-count 0 ?do
      2 pick i cells + @ +
    loop
    0 swap 0 bit-count f/ rot 0 swap f/ cr ." Mean:" f. drop
  ;

  \ Generate statistics
  : rng-stats ( count -- )
    ram-here bit-count 2 * cells ram-allot dup init-rng-stats
    over 0 ?do
      random
      bit-count 0 ?do
	dup i rshift 1 and if
	  over 1 swap i cells + +!
	then
      loop
      drop
    loop
    tuck swap rng-stats. ram-here!
  ;

end-module
