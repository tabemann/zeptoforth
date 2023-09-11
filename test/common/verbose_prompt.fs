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

begin-module verbose-prompt-internal
  
  : type-my-base ( -- )
    base @ dup { saved-base } case
      #10 of ." #" endof
      #16 of ." $" endof
      #8 of ." /" endof
      #2 of ." %" endof
      dup #10 base ! (.) saved-base base !
    endcase
  ;
  
  : my-prompt ( ? -- ? )
    ." ok<"
    type-my-base
    compiling-to-flash? if ." , flash>" else ." , RAM>" then
    .s
    cr
  ;
  
end-module> import

\ Set a verbose prompt
: verbose-prompt ( -- ) ['] my-prompt prompt-hook ! ;

\ Set a short prompt
: short-prompt ( -- ) ['] internal::do-prompt prompt-hook ! ;
