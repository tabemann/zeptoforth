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

: byte-sieve-inner { size flags -- count }
  1 { iter }
  0 { count }
  begin iter 10 <= while
    0 { index }
    begin index size <= while
      true index flags + c!
      1 +to index
    repeat
    0 to index
    begin index size <= while
      index flags + c@ if
        index index + 3 + { prime }
        index prime + { k }
        begin k size <= while
          false k flags + c!
          prime +to k
        repeat
        1 +to count
      then
      1 +to index
    repeat
    1 +to iter
  repeat
  count
;
    
: byte-sieve ( -- )
  8190 dup 1+ [: { size flags }
    timer::us-counter { D: time }
    size flags byte-sieve-inner { count }
    timer::us-counter { D: end-time }
    cr count . ."  primes"
    cr end-time time d- drop s>f 1_000_000,0 f/ ." Time: " f. ." s"
  ;] with-allot
;
