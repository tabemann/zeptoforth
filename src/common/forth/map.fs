\ Copyright (c) 2021-2023 Travis Bemann
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

begin-module map

  begin-module map-internal
    
    begin-structure map-header-size

      \ The map entry count
      field: map-entry-count

      \ The map used entry count
      field: map-used-entry-count

      \ The size of a map key in bytes
      field: map-key-size

      \ The size of map value in bytes
      field: map-value-size

      \ The hash function
      field: map-hash-xt

      \ The equals function
      field: map-equals-xt
      
      \ The remove handler
      field: map-remove-xt

    end-structure

    \ Get the size of a map entry for a given map
    : map-entry-size ( map -- entry-size )
      dup map-key-size @ swap map-value-size @ + cell align cell +
    ;

    \ Get the address of map entry
    : map-entry ( index map -- addr )
      dup map-entry-size rot * + map-header-size +
    ;

    \ Get the hash key of a key
    : key-hash ( key-addr map -- hash-key )
      map-hash-xt @ execute $FFFFFFFF umod 1+
    ;

    \ Get the initial index of a hash key
    : map-init-index ( hash-key map -- index )
      map-entry-count @ umod
    ;

    \ Find a key in a map and return the index or, if not found, -1
    : find-key ( hash-key key-addr map -- index | -1 )
      rot >r r@ over map-init-index dup begin
        dup 3 pick map-entry dup @ ?dup if
          r@ = if
            cell+ 4 pick 4 pick map-equals-xt @ execute if
              nip nip nip rdrop exit
            else
              1+ 2 pick map-entry-count @ umod
            then
          else
            drop 1+ 2 pick map-entry-count @ umod
          then
        else
          drop 2drop 2drop rdrop -1 exit
        then
      2dup = until
      2drop 2drop rdrop -1
    ;

    \ Get the next available index for a key in a map - note that this
    \ does *not* check for the full condition
    : find-available ( hash-key map -- index )
      tuck map-init-index begin
	2dup swap map-entry @ dup 0= swap -1 = or if
	  nip exit
	else
	  1+ over map-entry-count @ umod
	then
      again
    ;
    
  end-module> import

  \ Map index is out of range
  : x-map-index-out-of-range ( -- ) ." map index is out of range" cr ;
  
  \ Map is full exception
  : x-map-full ( -- ) ." map is full" cr ;

  \ Destination map does not match key size, value size, hash function, or
  \ equals function of the source map exception
  : x-dest-map-not-match ( -- )
    ." destination map does not match source map" cr
  ;

  \ Index contains no entry
  : x-map-index-no-entry ( -- ) ." map index has no entry" cr ;

  \ Get the size of a map for a given entry count, key size in bytes, and
  \ value size in bytes
  : map-size ( count key-size value-size -- size )
    + cell align cell + * map-header-size +
  ;

  \ Initialize a map for a given hash function, equals function, remove
  \ handler, entry count, key size in bytes, value size in bytes, at a
  \ specified address with the amount of space returned by map-size available
  \
  \ hash-xt: ( key-addr -- hash )
  \ equals-xt: ( key-addr key-addr -- equals? )
  \ remove-xt: ( value-addr key-addr -- )
  : init-map ( hash-xt equals-xt remove-xt count key-size value-size addr -- )
    tuck map-value-size !
    tuck map-key-size !
    tuck map-entry-count !
    tuck map-remove-xt !
    tuck map-equals-xt !
    tuck map-hash-xt !
    0 over map-used-entry-count !
    0 begin 2dup swap map-entry-count @ < while
      2dup swap map-entry 0 swap ! 1+
    repeat
    2drop
  ;

  \ Find a key's value in a map or return 0 if not found
  : find-map ( key-addr map -- value-addr|0 )
    dup >r 2dup key-hash -rot find-key dup -1 <> if
      r@ map-entry r> map-key-size @ cell+ +
    else
      drop rdrop 0
    then
  ;

  \ Insert a value with a key into a map
  : insert-map ( value-addr key-addr map -- )
    2dup key-hash
    >r 2dup r@ -rot find-key dup -1 <> if
      rdrop swap >r r@ map-entry cell+
      r@ map-remove-xt @ if
	dup dup r@ map-key-size @ + swap r@ map-remove-xt @ execute
      then
    else
      drop
      dup map-used-entry-count @ over map-entry-count @ < averts x-map-full
      r> 2dup swap find-available
      rot >r r@ map-entry
      tuck ! cell+
      1 r@ map-used-entry-count +!
    then
    tuck r@ map-key-size @ move r@ map-key-size @ +
    r> map-value-size @ move
  ;

  \ Remove a value with a key from a map
  : remove-map ( key-addr map -- )
    dup >r 2dup key-hash -rot find-key dup -1 <> if
      r@ map-entry -1 over !
      -1 r@ map-used-entry-count +!
      r@ map-remove-xt @ if
	cell+ dup r@ map-key-size @ + swap r> map-remove-xt @ execute
      else
	rdrop
      then
    else
      drop rdrop
    then
  ;

  \ Get the first index in a map, or return -1 if no entry is found
  : first-map ( map -- index|-1 )
    >r 0 begin
      dup r@ map-entry @ dup 0<> swap -1 <> and if rdrop exit then
    1+ dup r@ map-entry-count @ = until
    rdrop drop -1
  ;

  \ Get the next index in a map, or return -1 if no entry is found
  : next-map ( index map -- index'|-1 )
    over 0 >= averts x-map-index-out-of-range
    2dup map-entry-count @ < averts x-map-index-out-of-range
    >r 1+ begin
      dup r@ map-entry @ dup 0<> swap -1 <> and if rdrop exit then
    1+ dup r@ map-entry-count @ = until
    rdrop drop -1
  ;

  \ Get the key and value at an index in a map
  : at-map ( index map -- value-addr key-addr )
    over 0 >= averts x-map-index-out-of-range
    2dup map-entry-count @ < averts x-map-index-out-of-range
    dup >r map-entry dup @ dup 0<> swap -1 <> and averts x-map-index-no-entry
    cell+ dup r> map-value-size @ + swap
  ;

  \ Clear a map
  : clear-map ( map -- )
    >r 0 r@ map-used-entry-count !
    0 begin dup r@ map-entry-count @ < while
      dup r@ map-entry dup @ if
	0 over !
	r@ map-remove-xt @ if
	  cell+ dup r@ map-key-size @ + swap r@ map-remove-xt @ execute
	else
	  drop
	then
      else
	drop
      then
      1+
    repeat
    drop rdrop
  ;

  \ Copy one map into another map
  : copy-map ( src-map dest-map -- )
    2dup map-value-size @ swap map-value-size @ = averts x-dest-map-not-match
    2dup map-key-size @ swap map-key-size @ = averts x-dest-map-not-match
    2dup map-equals-xt @ swap map-equals-xt @ = averts x-dest-map-not-match
    2dup map-hash-xt @ swap map-hash-xt @ = averts x-dest-map-not-match
    >r r@ first-map
    begin dup -1 <> while
      dup r@ at-map 3 pick insert-map
      r@ next-map
    repeat
    2drop rdrop
  ;
  
  \ Get map entry count
  : map-entry-count ( map -- entry-count ) map-entry-count @ ;

  \ Get map used entry count
  : map-used-entry-count ( map -- used-entry-count ) map-used-entry-count @ ;

  \ Get map key size in bytes
  : map-key-size ( map -- key-size ) map-key-size @ ;

  \ Get map value size in bytes
  : map-value-size ( map -- value-size ) map-value-size @ ;

  \ Get map hash function
  : map-hash-xt ( map -- hash-xt ) map-hash-xt @ ;

  \ Get map equals function
  : map-equals-xt ( map -- equals-xt ) map-equals-xt @ ;

  \ Get map remove function
  : map-remove-xt ( map -- remove-xt ) map-remove-xt @ ;
  
end-module

\ Reboot
reboot