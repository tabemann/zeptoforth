\ Copyright (c) 2020-2021 Travis Bemann
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

begin-module pool-module

  task-module import

  begin-module pool-internal-module

    \ Pool header structure
    begin-structure pool-size

      \ First free block header
      field: pool-first-free

      \ Last free block header
      field: pool-last-free

      \ Block size
      field: pool-block-size

      \ Free block count
      field: pool-free-count

      \ Total block-count
      field: pool-total-count

    end-structure

    \ Pool block header structure
    begin-structure pool-header-size

      \ Next free block header
      field: pool-next-free

    end-structure

  end-module> import

  commit-flash

  \ Initialize a pool header
  : init-pool ( block-size addr -- )
    0 over pool-first-free !
    0 over pool-last-free !
    0 over pool-free-count !
    0 over pool-total-count !
    swap pool-header-size max swap pool-block-size !
  ;

  \ Free memory to a pool
  : pool-free ( addr pool -- )
    1 over pool-free-count +!
    over 0 swap pool-next-free !
    dup pool-first-free @ 0= if
      2dup pool-first-free !
    else
      2dup pool-last-free @ pool-next-free !
    then
    pool-last-free !
  ;

  \ Add memory to a pool
  : pool-add ( addr bytes pool -- )
    begin
      dup pool-block-size @ 2 pick u<=
    while
      2 pick over pool-free
      1 over pool-total-count +!
      dup pool-block-size @ 3 roll +
      over pool-block-size @ 3 roll swap -
      rot
    repeat
    2drop drop
  ;

  \ Allocate memory from a pool, or return 0 if no space is available
  : pool-allocate ( pool -- addr | 0 )
    dup pool-first-free ?dup if
      dup pool-next-free @
      2 pick pool-first-free !
      over pool-first-free @ 0= if
	0 2 pick pool-last-free !
      then
      -1 rot pool-free-count +!
    else
      drop 0
    then
  ;

  \ Get the pool block size
  : pool-block-size ( pool -- bytes ) pool-block-size @ ;

  \ Get the pool free count
  : pool-free-count ( pool -- u ) pool-free-count @ ;

  \ Get the pool total count
  : pool-total-count ( pool -- u ) pool-total-count @ ;

  \ Export pool-size
  export pool-size

end-module

end-compress-flash

\ Reboot
reboot
