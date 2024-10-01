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

begin-module heap-exhaust-test

  heap import

  65536 constant my-base-heap-size
  16 12 + constant my-block-size

  : get-heap-size ( -- bytes )
    my-block-size my-base-heap-size my-block-size / heap-size
  ;

  : run-test ( -- )
    get-heap-size [: { my-heap }
      my-block-size my-base-heap-size my-block-size / my-heap init-heap
      0 { counter }
      begin
        0 { last-data }
        1024 1 ?do
          ." cycle " counter .
          ." bytes " i .
          ." allocating "
          i my-heap allocate { data }
          last-data 0<> if
            ." freeing "
            last-data my-heap free
            ." freed "
          then
          data to last-data
        loop
      again
    ;] with-aligned-allot
  ;

end-module
