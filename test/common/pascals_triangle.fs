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

defined? rows not [if] 8 value rows [then]

begin
  rows 2 * cells [: { buf }
    1 buf !
    cr 1 .
    rows 1 ?do
      i 1 and 0<> { odd }
      odd if buf rows cells + else buf then { new-row }
      odd if buf else buf rows cells + then { old-row }
      1 new-row !
      1 new-row i cells + !
      i 1 ?do
        old-row i 1- cells + @ old-row i cells + @ + new-row i cells + !
      loop
      cr
      i 1+ 0 ?do
        new-row i cells + @ .
      loop
    loop
  ;] with-aligned-allot
end