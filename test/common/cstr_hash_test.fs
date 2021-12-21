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

  hash-module import
  cstr-hash-module import

  \ Hash entry count
  32 constant entry-count

  \ The counted string hash
  entry-count cell cstr-hash-size buffer: my-hash

  \ Find a value in a counted string hash
  : find-cstr-hash ( key-cstr hash -- value|-1 )
    find-cstr-hash ?dup if @ else -1 then
  ;

  \ Insert a value in a counted string hash
  : insert-cstr-hash ( value key-cstr hash -- )
    rot cell [: tuck ! -rot insert-cstr-hash ;] with-aligned-allot
  ;
  
  \ Initialize the test
  : init-test ( -- ) 
  
    \ Initialize the counted string hash
    entry-count cell my-hash init-cstr-hash

    \ Insert some values into the hash
    10 c" foo" my-hash insert-cstr-hash
    20 c" bar" my-hash insert-cstr-hash
    30 c" baz" my-hash insert-cstr-hash

    \ Get some values from the hash
    c" foo" my-hash find-cstr-hash cr ." foo:" .
    c" bar" my-hash find-cstr-hash cr ." bar:" .
    c" baz" my-hash find-cstr-hash cr ." baz:" .

  ;
  
end-module