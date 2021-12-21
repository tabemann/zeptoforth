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

begin-module hash-module

  begin-module hash-internal-module
    
    begin-structure hash-header-size

      \ The hash entry count
      field: hash-entry-count

      \ The hash used entry count
      field: hash-used-entry-count

      \ The size of a hash key in bytes
      field: hash-key-size

      \ The size of hash value in bytes
      field: hash-value-size

      \ The hash function
      field: hash-hash-xt

      \ The equals function
      field: hash-equals-xt
      
    end-structure

    \ Get the size of a hash entry for a given hash
    : hash-entry-size ( hash -- entry-size )
      dup hash-key-size @ swap hash-value-size @ + cell align cell +
    ;

    \ Get the address of hash entry
    : hash-entry ( index hash -- addr )
      dup hash-entry-size rot * + hash-header-size +
    ;

    \ Get the hash key of a key
    : key-hash ( key-addr hash -- hash-key )
      hash-hash-xt @ execute $FFFFFFFF umod 1+
    ;

    \ Get the initial index of a hash key
    : hash-init-index ( hash-key hash -- index )
      hash-entry-count @ umod
    ;

    \ Find a key in a hash table and return the index or, if not found, 0
    : find-key ( hash-key key-addr hash -- index|0 )
      rot >r r@ over hash-init-index dup begin
	dup 3 pick hash-entry dup @ r@ = if
	  cell+ 4 pick 4 pick hash-equals-xt @ execute if
	    nip nip nip rdrop exit
	  else
	    1+ 2 pick hash-entry-count @ umod
	  then
	else
	  drop 1+ 2 pick hash-entry-count @ umod
	then
      2dup = until
      2drop 2drop rdrop 0
    ;

    \ Get the next available index for a key in a hash table - note that this
    \ does *not* check for the full condition
    : find-available ( hash-key hash -- index )
      tuck hash-init-index begin
	2dup swap hash-entry @ 0= if
	  nip exit
	else
	  1+ over hash-entry-count @ umod
	then
      again
    ;
    
  end-module> import

  \ Hash index is out of range
  : x-hash-index-out-of-range ( -- ) space ." hash index is out of range" cr ;
  
  \ Hash is full exception
  : x-hash-full ( -- ) space ." hash is full" cr ;

  \ Destination hash does not match key size, value size, hash function, or
  \ equals function of the source hash exception
  : x-dest-hash-not-match ( -- )
    space ." destination hash does not match source hash" cr
  ;

  \ Destination hash is not sufficiently large to contain all the entries from
  \ the source hash exception
  : x-dest-hash-too-small ( -- )
    space ." destination hash is too small to copy source hash" cr
  ;
  
  \ Get the size of a hash for a given entry count, key size in bytes, and
  \ value size in bytes
  : hash-size ( count key-size value-size -- size )
    + cell align cell + * hash-header-size +
  ;

  \ Initialize a hash for a given hash function, equals function, entry count,
  \ key size in bytes, value size in bytes, at a specified address with the
  \ amount of space returned by hash-size available
  : init-hash ( hash-xt equals-xt count key-size value-size addr -- )
    tuck hash-value-size !
    tuck hash-key-size !
    tuck hash-entry-count !
    tuck hash-equals-xt !
    tuck hash-hash-xt !
    0 over hash-used-entry-count !
    0 begin 2dup swap hash-entry-count @ < while
      2dup swap hash-entry 0 swap ! 1+
    repeat
    2drop
  ;

  \ Find a key's value in a hash or return 0 if not found
  : find-hash ( key-addr hash -- value-addr|0 )
    dup >r 2dup key-hash -rot find-key ?dup if
      r@ hash-entry r> hash-key-size @ cell+ +
    else
      rdrop 0
    then
  ;

  \ Insert a value with a key into a hash
  : insert-hash ( value-addr key-addr hash -- )
    dup hash-used-entry-count @ over hash-entry-count @ < averts x-hash-full
    1 over hash-used-entry-count +!
    2dup key-hash
    >r 2dup r@ -rot find-key ?dup if
      rdrop swap >r r@ hash-entry cell+
    else
      r> 2dup swap find-available
      rot >r r@ hash-entry
      tuck ! cell+
    then
    tuck r@ hash-key-size @ move r@ hash-key-size @ +
    r> hash-value-size @ move
  ;

  \ Remove a value with a key from a hash
  : remove-hash ( key-addr hash -- )
    dup >r 2dup key-hash -rot find-key ?dup if
      r@ hash-entry 0 swap !
      -1 r> hash-used-entry-count +!
    then
  ;

  \ Get the first index in a hash, or return -1 if no entry is found
  : first-hash ( hash -- index|0 )
    >r 0 begin
      dup r@ hash-entry @ 0<> if rdrop exit then
    1+ r@ hash-entry-count @ = until
    rdrop drop -1
  ;

  \ Get the next index in a hash, or return -1 if no entry is found
  : next-hash ( index hash -- index'|-1 )
    over 0 >= averts x-hash-index-out-of-range
    2dup hash-entry-count @ < averts x-hash-index-out-of-range
    >r 1+ begin
      dup r@ hash-entry @ 0<> if rdrop exit then
    1+ r@ hash-entry-count @ = until
    rdrop drop -1
  ;

  \ Get the key and value at an index in a hash, or return 0 0 if no entry is
  \ found
  : at-hash ( index hash -- key-addr value-addr | 0 0 )
    over 0 >= averts x-hash-index-out-of-range
    2dup hash-entry-count @ < averts x-hash-index-out-of-range
    dup >r hash-entry dup @ if
      cell+ dup r> hash-value-size @ +
    else
      rdrop 0 0
    then
  ;

  \ Copy one hash into another hash
  : copy-hash ( dest-hash src-hash -- )
    2dup hash-value-size @ swap hash-value-size @ = averts x-dest-hash-not-match
    2dup hash-key-size @ swap hash-key-size @ = averts x-dest-hash-not-match
    2dup hash-equals-xt @ swap hash-equals-xt @ = averts x-dest-hash-not-match
    2dup hash-hash-xt @ swap hash-hash-xt @ = averts x-dest-hash-not-match
    2dup hash-used-entry-count @
    swap dup hash-entry-count @ swap hash-used-entry-count @ - <=
    averts x-dest-hash-too-small
    >r 0 begin
      dup r@ hash-entry cell+ dup r@ hash-key-size @ + swap 3 pick insert-hash
      1+ r@ hash-entry-count @ umod
    0= until
    rdrop 2drop drop
  ;
  
  \ Get hash entry count
  : hash-entry-count ( hash -- entry-count ) hash-entry-count @ ;

  \ Get hash used entry count
  : hash-used-entry-count ( hash -- used-entry-count ) hash-used-entry-count @ ;

  \ Get hash key size in bytes
  : hash-key-size ( hash -- key-size ) hash-key-size @ ;

  \ Get hash value size in bytes
  : hash-value-size ( hash -- value-size ) hash-value-size @ ;

  \ Get hash hash function
  : hash-hash-xt ( hash -- hash-xt ) hash-hash-xt @ ;

  \ Get hash equals function
  : hash-equals-xt ( hash -- equals-xt ) hash-equals-xt @ ;
  
end-module

\ Reboot


\ reboot