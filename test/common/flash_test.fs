\ Copyright (c) 2020-2022 Travis Bemann
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

\ Compile to RAM
compile-to-ram

\ Set wordlist order to include internal words
0 1 2 set-order

\ Output a hexadecimal nibble
: h.1 ( b -- )
  $F and dup 10 < if [char] 0 + else 10 - [char] A + then emit
;

\ Output a hexadecimal 8 bit value, padded with zeros
: h.2 ( b -- )
  $FF and dup 4 rshift h.1 h.1
;

\ Output a hexadecimal 16 bit value, padded with zeros
: h.4 ( h -- )
  $FFFF and dup 8 rshift h.2 h.2
;

\ Output a hexadecimal 32 bit value, padded with zeros
: h.8 ( x -- )
  dup 16 rshift h.4 h.4
;

\ Begin a ?do loop
: ?do ( end start -- ) ( R: -- leave start end ) ( compile: -- leave* loop )
  [immediate]
  [compile-only]
  undefer-lit
  6 push,
  reserve-literal
  postpone >r
  postpone 2dup
  postpone <>
  postpone if
  postpone >r
  postpone >r
  postpone else
  postpone 2drop
  postpone exit
  postpone then
  here
;

\ End a do loop
: loop ( R: leave current end -- leave current end | )
  [immediate]
  [compile-only]
  postpone r>
  postpone r>
  1 lit, postpone +
  postpone 2dup
  postpone =
  postpone swap
  postpone >r
  postpone swap
  postpone >r
  undefer-lit
  0 6 0 lsl-imm,
  6 pull,
  0 0 cmp-imm,
  0branch,
  postpone rdrop
  postpone rdrop
  postpone rdrop
  here 1+ 6 rot literal!
;

\ Get the loop index
: i ( R: current end -- current end ) ( -- current )
  [immediate]
  [compile-only]
  postpone r>
  postpone r>
  postpone dup
  postpone >r
  postpone swap
  postpone >r
;

\ Verify flash by writing and then reading a given byte up to a given address
: verify-flash ( b addr -- )
  dup flash-here u> if
    flash-here >r
    dup begin dup flash-here u> while
      flash-here $FFFF and 0= if
	cr ." Writing: " flash-here h.8
      then
      2 pick bflash,
    repeat
    drop r> ?do
      i $FFFF and 0= if
	cr ." Verifying: " i h.8
      then
      i c@ over <> if
	cr ." Error at: " i h.8 space ." found: " i c@ h.2
      then
    loop
    drop
  else
    2drop
  then
;