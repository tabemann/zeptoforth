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
  cstr-map-module import

  \ Map entry count
  32 constant entry-count

  \ The counted string map
  entry-count cell cstr-map-size buffer: my-map

  \ Find a value in a counted string map
  : find-cstr-map ( key-cstr map -- value|-1 )
    find-cstr-map ?dup if @ else -1 then
  ;

  \ Insert a value in a counted string map
  : insert-cstr-map ( value key-cstr map -- )
    rot cell [: tuck ! -rot insert-cstr-map ;] with-aligned-allot
  ;

  \ Handle item removal
  : handle-remove ( value-addr key-addr -- )
    cr ." Removed: " @ count type @ .
  ;
  
  \ Initialize the test
  : init-test ( -- ) 
  
    \ Initialize the counted string map
    ['] handle-remove entry-count cell my-map init-cstr-map

    \ Insert some values into the map
    10 c" foo" my-map insert-cstr-map
    20 c" bar" my-map insert-cstr-map
    30 c" baz" my-map insert-cstr-map

    \ Get some values from the map
    c" foo" my-map find-cstr-map cr ." foo:" .
    c" bar" my-map find-cstr-map cr ." bar:" .
    c" baz" my-map find-cstr-map cr ." baz:" .

    \ Insert some different values into the map
    100 c" foo" my-map insert-cstr-map
    200 c" bar" my-map insert-cstr-map
    300 c" baz" my-map insert-cstr-map

    \ Get some values from the map
    c" foo" my-map find-cstr-map cr ." foo:" .
    c" bar" my-map find-cstr-map cr ." bar:" .
    c" baz" my-map find-cstr-map cr ." baz:" .

    \ Remove some values from the map
    c" foo" my-map remove-cstr-map
    c" bar" my-map remove-cstr-map
    c" baz" my-map remove-cstr-map
    
    \ Get some values from the map
    c" foo" my-map find-cstr-map cr ." foo:" .
    c" bar" my-map find-cstr-map cr ." bar:" .
    c" baz" my-map find-cstr-map cr ." baz:" .

  ;
  
end-module