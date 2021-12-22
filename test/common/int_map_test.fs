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

continue-module forth-module

  map-module import
  int-map-module import

  \ Map entry count
  32 constant entry-count

  \ The integer map
  entry-count cell int-map-size buffer: my-map

  \ The duplicate integer map
  entry-count cell int-map-size buffer: my-copy-map

  \ Find a value in a integer map
  : find-int-map ( key-int map -- value|-1 )
    find-int-map ?dup if @ else -1 then
  ;

  \ Insert a value in a integer map
  : insert-int-map ( value key-int map -- )
    rot cell [: tuck ! -rot insert-int-map ;] with-aligned-allot
  ;

  \ Get the key and value at an index in an integer map, or return -1 -1
  \ if no entry is found
  : at-int-map ( index map -- value-int key-int )
    at-int-map swap @ swap
  ;

  \ Handle item removal
  : handle-remove ( value-addr key-addr -- )
    cr ." Removed:" @ . @ .
  ;
  
  \ Initialize the test
  : init-test ( -- ) 
  
    \ Initialize the integer map
    ['] handle-remove entry-count cell my-map init-int-map

    \ Initialize the duplicate integer map
    ['] handle-remove entry-count cell my-copy-map init-int-map

    \ Insert some values into the map
    10 0 my-map insert-int-map
    20 1 my-map insert-int-map
    30 2 my-map insert-int-map

    \ Get some values from the map
    0 my-map find-int-map cr ." 0:" .
    1 my-map find-int-map cr ." 1:" .
    2 my-map find-int-map cr ." 2:" .

    \ Insert some different values into the map
    100 0 my-map insert-int-map
    200 1 my-map insert-int-map
    300 2 my-map insert-int-map

    \ Get some values from the map
    0 my-map find-int-map cr ." 0:" .
    1 my-map find-int-map cr ." 1:" .
    2 my-map find-int-map cr ." 2:" .

    \ Iterate through the entries in the map
    my-map first-map
    begin dup -1 <> while
      dup my-map at-int-map cr ." Iterated:" . .
      my-map next-map
    repeat
    drop

    \ Copy the map
    my-copy-map my-map copy-map

    \ Iterate through the entries in the duplicate map
    my-copy-map first-map
    begin dup -1 <> while
      dup my-copy-map at-int-map cr ." Iterated copy:" . .
      my-copy-map next-map
    repeat
    drop

    \ Remove some values from the map
    0 my-map remove-int-map
    1 my-map remove-int-map
    2 my-map remove-int-map
    
    \ Get some values from the map
    0 my-map find-int-map cr ." 0:" .
    1 my-map find-int-map cr ." 1:" .
    2 my-map find-int-map cr ." 2:" .

    \ Insert values until an exception occurs
    [:
      0 begin
	10 + dup cr ." Inserting" . 0 over my-map insert-int-map
      again
    ;] try drop

    \ Clear the map
    my-map clear-map

    \ Insert values until an exception occurs again
    [:
      0 begin
	10 + dup cr ." Inserting" . 0 over my-map insert-int-map
      again
    ;] try drop
    
  ;
  
end-module
