\ Copyright (c) 2025 Travis Bemann
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

begin-module int-map-fill-test

  int-map import

  16 constant my-count
  cell constant my-value-size

  my-count 1+ my-value-size int-map-size buffer: my-map

  : generate-key ( n -- key ) 3 * ;
  
  : dump-map ( -- )
    my-count 0 ?do cr i generate-key dup . my-map find-int-map . loop
  ;
  
  : populate-map ( -- )
    my-count 0 ?do 
      i { W^ addr }
      i generate-key { key }
      cr ." Populating map for " key .
      addr key my-map insert-int-map
      dump-map
    loop
  ;

  : empty-map ( -- )
    0 my-count 1- ?do
      i generate-key { key }
      cr ." Clearing map for " key .
      key my-map remove-int-map
      dump-map
    -1 +loop
  ;

  : run-test ( -- )
    0 my-count 1+ my-value-size my-map init-int-map
    populate-map
    empty-map
    populate-map
    empty-map
  ;
  
end-module
