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

continue-module forth

  heap import

  \ My block count
  64 constant my-block-count

  \ My block size
  16 constant my-block-size

  \ My heap size
  my-block-size my-block-count heap-size constant my-heap-size

  \ My heap
  my-heap-size buffer: my-heap

  \ My blocks
  variable block-0
  variable block-1
  variable block-2
  variable block-3

  \ Initialize the test
  : init-test ( -- )
    my-block-size my-block-count my-heap init-heap
    16 my-heap allocate block-0 !
    cr ." allocated block-0 (16 bytes): " block-0 @ h.8
    32 my-heap allocate block-1 !
    cr ." allocated block-1 (32 bytes): " block-1 @ h.8
    64 my-heap allocate block-2 !
    cr ." allocated block-2 (64 bytes): " block-2 @ h.8
    block-1 @ my-heap free
    cr ." freed block-1"
    32 my-heap allocate block-1 !
    cr ." allocated block-1 (32 bytes): " block-1 @ h.8
    64 block-1 @ my-heap resize block-1 !
    cr ." resized block-1 (64 bytes): " block-1 @ h.8
    16 block-0 @ my-heap resize block-0 !
    cr ." resized block-0 (16 bytes): " block-0 @ h.8
    32 block-0 @ my-heap resize block-0 !
    cr ." resized block-0 (32 bytes): " block-0 @ h.8
    64 block-0 @ my-heap resize block-0 !
    cr ." resized block-0 (64 bytes): " block-0 @ h.8
    128 block-0 @ my-heap resize block-0 !
    cr ." resized block-0 (128 bytes): " block-0 @ h.8
  ;
  
end-module