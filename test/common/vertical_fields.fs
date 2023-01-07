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

begin-module field-test

  6 constant field-count
  field-count cells aligned-buffer: field-array
  
  : init-fields ( -- )
    c" FOO" field-array 0 cells + !
    c" BAR" field-array 1 cells + !
    c" BAZ" field-array 2 cells + !
    c" FOOBAR" field-array 3 cells + !
    c" FOOBAZ" field-array 4 cells + !
    c" QUUX" field-array 5 cells + !
  ;
  
  : max-field-length { array array-count -- length }
    0 array array-count cells + array ?do
      i @ c@ max
    cell +loop
  ;
  
  : max-index-length { max-index -- }
    max-index s>d <# #s #> nip
  ;
  
  : emit-field-char { row max-length index array -- }
    array index cells + @ { field }
    field c@ { field-length }
    max-length row - field-length > if
      space
    else
      field-length max-length row - - { char-index }
      field 1+ char-index + c@ emit
    then
  ;
  
  : emit-index-char { index-length row index -- }
    index s>d <# #s #> { index-addr index-bytes }
    index-length row - index-bytes > if
      space
    else
      index-bytes index-length row - - { char-index }
      index-addr char-index + c@ emit
    then
  ;
  
  : print-fields { array array-count -- }
    cr
    array array-count max-field-length { max-length }
    max-length 0 ?do
      0 array-count 1- ?do
        j max-length i array emit-field-char space
      -1 +loop
      cr
    loop
    cr
    array-count max-index-length { index-length }
    index-length 0 ?do
      0 array-count 1- ?do
        index-length j i emit-index-char space
      -1 +loop
    loop
  ;
  
  : run-test ( -- )
    init-fields
    field-array field-count print-fields
  ;
  
end-module