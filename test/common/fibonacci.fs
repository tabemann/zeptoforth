\ Copyright (c) 2021-2023 Travis Bemann
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

continue-module forth

  systick import
  
  \ Calculate a single fibonacci sequence with the specified number of
  \ iterations, returning the values
  : fibonacci ( iterations -- ud )
    0 0 rot 1 0 rot 1 ?do 2dup 2>r d+ 2r> 2swap loop 2swap 2drop
  ;

  \ Test the timing of multiple computations of the fibonacci sequence
  : fibonacci-time ( iterations times -- f )
    systick-counter
    over 0 ?do 2 pick fibonacci 2drop loop
    0 swap systick-counter 0 swap 2swap d- rot 0 swap f/ rot drop
    10000,0 f/
  ;
  
end-module
