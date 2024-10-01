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

: sieve-inner { count buffer }
  0 { index }
  begin index count < while
    index 7 and bit index 3 rshift buffer + cbit@ if
      index 1+ { test-index }
      begin test-index count < while
        test-index 7 and bit { test-bit }
        test-index 3 rshift buffer + { test-addr }
        test-bit test-addr cbit@ if
          test-index 2 + index 2 + umod 0= if
            test-bit test-addr cbic!
          then
        then
        1 +to test-index
      repeat
    then
    1 +to index
  repeat
;

: sieve. { count buffer }
  0 { index }
  begin index count < while
    index 7 and bit index 3 rshift buffer + cbit@ if
      index 2 + .
    then
    1 +to index
  repeat
;

: sieve ( n -- )
  2 - dup 8 align 8 / dup [: { count byte-count buffer }
    buffer byte-count $FF fill
    timer::us-counter { D: time }
    count buffer sieve-inner
    timer::us-counter { D: end-time }
    count buffer sieve.
    cr end-time time d- drop s>f 1_000_000,0 f/ ." Time: " f. ." s"
  ;] with-allot
;
