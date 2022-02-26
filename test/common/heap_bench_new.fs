\ Copyright (c) 2022 Travis Bemann
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

begin-module heap-bench-new

  heap-bench import
  new-heap import
  
  \ Block size
  16 constant block-size

  \ Block count
  1024 constant block-count

  \ Block size limit
  128 block-size * constant block-size-limit
  
  \ Block count limit
  128 constant block-count-limit

  \ Run benchmark for the new heap allocator and output a fixed point number of
  \ seconds per cycle
  : run-bench-new ( seed cycles -- seconds-per-cycle )
    swap block-size block-count heap-size [:
      dup block-size block-count heap-size 0 fill
      block-count-limit bench-size [:
	>r >r
	['] allocate ['] free ['] resize ['] diagnose-heap
	[: block-size block-count rot init-heap ;]
	r> block-size-limit block-count-limit r@ init-bench
	r> run-bench
      ;] with-aligned-allot
    ;] with-aligned-allot
  ;
  
end-module