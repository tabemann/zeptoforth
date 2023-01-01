\ Copyright (c) 2022-2023 Travis Bemann
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

  tinymt32 import
  heap import

  \ Block size
  16 constant block-size

  \ Block count
  1024 constant block-count

  begin-structure heap-exhaust-size

    \ The pseudorandom number generator
    tinymt32-size +field heap-exhaust-prng

    \ The heap
    block-size block-count heap-size +field heap-exhaust-heap

  end-structure

  \ Exhaust a heap using a seed for randomization
  : run-heap-exhaust ( seed -- )
    heap-exhaust-size [:
      >r block-size block-count r@ heap-exhaust-heap init-heap
      r@ heap-exhaust-prng tinymt32-prepare-example
      r@ heap-exhaust-prng tinymt32-init
      begin
	r@ heap-exhaust-prng tinymt32-generate-uint32
	8 cells umod 3 cells max 16 align cell -
	r@ heap-exhaust-heap allocate drop
      again
    ;] with-aligned-allot
  ;

end-module