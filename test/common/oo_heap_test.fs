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

begin-module oo-heap-test

  oo import
  heap import
  
  4 cells constant block-size
  256 constant block-count
  
  block-size block-count heap-size buffer: my-heap
  
  variable last-object
  
  <object> begin-class <heap-object>
    cell member index
    cell member prev-object
  end-class
  
  <heap-object> begin-implement
    :noname
      dup <object>->new
      last-object @ swap
      2dup prev-object !
      over [:
        swap index @ 1+ over index !
      ;] [:
        nip 0 over index !
      ;] qifelse
      cr ." Initializing object "
      dup index @ .
      last-object !
    ; define new
    :noname
      dup <object>->destroy
      cr ." Destroying object "
      dup index @ .
      dup prev-object @ last-object !
      my-heap free
    ; define destroy
  end-implement
  
  : allocate-object ( ? class -- object )
    dup class-size my-heap allocate
    dup >r init-object r>
  ;
  
  : free-all-objects ( -- )
    [: last-object @ ;] [: last-object @ destroy ;] qwhile
  ;
  
  : fill-heap-with-objects ( -- )
    [:
      [: <heap-object> allocate-object drop ;] qagain
    ;] try dup ['] x-allocate-failed = [: drop 0 ;] qif ?raise
  ;
  
  : init-test ( -- )
    block-size block-count my-heap init-heap
    0 last-object !
    fill-heap-with-objects
    free-all-objects
    fill-heap-with-objects
    free-all-objects
  ;

end-module