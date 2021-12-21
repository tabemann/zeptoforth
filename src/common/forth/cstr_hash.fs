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
\ compile-to-flash

begin-module cstr-hash-module

  hash-module import

  begin-module cstr-hash-internal-module

    \ Hash function for counted strings
    : cstr-hash-hash ( c-addr -- hash )
      dup >r c@ dup begin ?dup while
	swap dup 7 lshift swap 25 rshift or r@ c@ xor r> 1+ >r swap 1-
      repeat
      rdrop
    ;

    \ Equals function for counted strings
    : cstr-hash-equals ( c-addr c-addr -- equals? )
      @ count rot @ count equal-strings?
    ;
    
  end-module> import
  
  \ Get the size of a counted string hash for a given entry count and data size
  \ in bytes
  : cstr-hash-size ( count value-size -- size ) cell swap hash-size ;

  \ Initialize a counted string hash
  : init-cstr-hash ( count value-size addr -- )
    2>r >r ['] cstr-hash-hash ['] cstr-hash-equals r> cell 2r> init-hash
  ;

  \ Find a key's value in a counted string hash or return 0 if not found
  : find-cstr-hash ( key-cstr hash -- value-addr|0 )
    cell [: rot over ! swap find-hash ;] with-aligned-allot
  ;

  \ Insert a value with a key into a counted string hash
  : insert-cstr-hash ( value-addr key-cstr hash -- )
    cell [: rot over ! swap insert-hash ;] with-aligned-allot
  ;

  \ Remove a value with a key from a counted string hash
  : remove-cstr-hash ( key-cstr hash -- )
    cell [: rot over ! swap remove-hash ;] with-aligned-allot
  ;

  \ Get the key and value at an index in a counted string hash, or return 0 0
  \ if no entry is found
  : at-cstr-hash ( index hash -- key-cstr value-addr | 0 0 )
    at-hash ?dup if swap @ swap else 0 then
  ;

end-module

\ Reboot
\ reboot
