\ Copyright (c) 2021-2022 Travis Bemann
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

\ Compile to flash
compile-to-flash

compress-flash

begin-module temp

  \ Data size is larger than temporary buffer size exception
  : x-data-too-big ( -- ) space ." data too big" cr ;

  begin-module temp-internal

    \ Temporary buffer structure
    begin-structure temp-size

      \ Temporary buffer data size
      field: temp-data-size

      \ Temporary buffer index
      field: temp-index
      
    end-structure

  end-module> import

  commit-flash
  
  \ Initialize a temporary buffer of the given data size
  : init-temp ( data-bytes addr -- ) tuck temp-data-size ! 0 swap temp-index ! ;
  
  \ Allocate a temporary block of data
  : allocate-temp ( bytes temp -- addr )
    2dup temp-data-size @ > triggers x-data-too-big
    2dup dup temp-data-size @ swap temp-index @ - > if
      0 over temp-index !
    then
    dup temp-index @ over + temp-size + -rot temp-index +!
  ;

  \ Get the size of a temporary buffer with a given data size
  : temp-size ( data-bytes -- bytes ) [inlined] temp-size + ;

end-module

end-compress-flash

\ Reboot
reboot
