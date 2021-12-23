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

\ Compile this to flash
compile-to-flash

begin-module int-map-module

  map-module import

  begin-module int-map-internal-module

    \ Hash function for integers
    : int-map-hash ( addr -- hash ) @ ;

    \ Equals function for integers
    : int-map-equals ( addr addr -- equals? ) @ swap @ = ;
    
  end-module> import
  
  \ Get the size of a integer map for a given entry count and value size
  \ in bytes
  : int-map-size ( count value-size -- size ) cell swap map-size ;

  \ Initialize a integer map with the specified remove handler, entry count
  \ and value size in bytes at the specified address
  \
  \ remove-xt: ( value-addr key-addr -- )
  : init-int-map ( remove-xt count value-size addr -- )
    2>r 2>r ['] int-map-hash ['] int-map-equals 2r> cell 2r> init-map
  ;

  \ Find a key's value in a integer map or return 0 if not found
  : find-int-map ( key-int map -- value-addr|0 )
    cell [: rot over ! swap find-map ;] with-aligned-allot
  ;

  \ Insert a value with a key into a integer map
  : insert-int-map ( value-addr key-int map -- )
    cell [: rot over ! swap insert-map ;] with-aligned-allot
  ;

  \ Remove a value with a key from a integer map
  : remove-int-map ( key-int map -- )
    cell [: rot over ! swap remove-map ;] with-aligned-allot
  ;

  \ Get the key and value at an index in a integer map
  : at-int-map ( index map -- value-addr key-int ) at-map @ ;

end-module

\ Reboot
reboot
