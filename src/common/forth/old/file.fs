\ Copyright (c) 2022 Travis Bemann
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

begin-module file

  oo import
  temp import
  lock import

  \ Maximum file object size
  128 constant max-file-object-size

  \ File object pool size
  max-file-object-size 2 * constant file-object-pool-size

  \ File object pool
  file-object-pool-size buffer: file-object-pool

  \ File lock
  lock-size buffer: file-lock

  \ Entity not found exception
  : x-entity-not-found ( -- ) ." entity not found" cr ;

  \ Entity already exists exception
  : x-entity-already-exists ( -- ) ." entity already exists" cr ;

  \ Root directory exception
  : x-root-directory ( -- ) ." root directory" cr ;

  \ Read only exception
  : x-read-only ( -- ) ." read only" cr ;

  \ File is closed exception
  : x-file-closed ( -- ) ." file closed" cr ;

  \ Base filesystem class
  <object> begin-class <fs>
    \ Get the class of the base directory of a filesystem
    method base-dir-class@ ( fs -- class )

    \ Initialize the memory for a base directory of a filesystem
    method init-base-dir ( addr fs -- )
    
    \ Allot a base directory and pass it to an execution token
    method with-base-dir ( xt dir -- )    
  end-class

  \ Implement base filesystem class
  <fs> begin-implement
    :noname ( xt fs -- )
      dup base-dir-class@ class-size [:
        tuck swap init-base-dir
        swap execute
      ;] with-aligned-allot
    ; define with-base-dir
    ' abstract-method define base-dir-class@
    ' abstract-method define init-base-dir
    ' abstract-method define with-base-dir
  end-implement
  
  \ Base filesystem entity class
  <object> begin-class <entity>
    \ Is file?
    method entity-file? ( entity -- flag )

    \ Is directory?
    method entity-dir? ( entity -- flag )

    \ Is read only?
    method entity-read-only? ( entity -- flag )

    \ Is fixed sized?
    method entity-fixed-sized? ( entity -- flag )
    
    \ Remove an entity
    method remove-entity ( entity -- )

    \ Rename an entity
    method rename-entity ( c-addr u entity -- )
  end-class

  \ Implement base filesystem entity class
  <entity> begin-implement
    :noname drop false ; define entity-file?
    :noname drop false ; define entity-dir?
    :noname drop true ; define entity-read-only?
    :noname drop true ; define entity-fixed-sized?
    ' abstract-method define remove-entity
    ' abstract-method define rename-entity
  end-implement
  
  \ Base directory class
  <entity> begin-class <dir>
    \ Get the class of an entity
    method entity-class@ ( c-addr u dir -- class )

    \ Initialize the memory for an entity
    method init-entity ( c-addr u addr dir -- )
    
    \ Allot an entity and pass it to an execution token
    method with-entity ( c-addr u xt dir -- )    

    \ Get the class of a directory
    method dir-class@ ( dir -- class )
    
    \ Create a directory
    method create-dir ( c-addr u addr dir -- )

    \ Get the class of a file
    method file-class@ ( dir -- class )
    
    \ Create an ordinary file
    method create-file ( c-addr u addr dir -- )

    \ Get the name of an entity at a given index if one exists
    method entity-name@ ( index dir -- c-addr u | 0 0 )
  end-class

  \ Implement base directory class
  <dir> begin-implement
    :noname ( c-addr u xt dir -- )
      swap >r 3dup entity-class@ class-size r> swap [: ( c-addr u dir xt buf )
        swap >r dup >r swap init-entity
        r> r> execute
      ;] with-aligned-allot
    ; define with-entity
    ' abstract-method define entity-class@
    ' abstract-method define init-entity
    ' abstract-method define dir-class@
    ' abstract-method define create-dir
    ' abstract-method define file-class@
    ' abstract-method define create-file
    ' abstract-method define entity-name@
  end-implement

  \ Seek from the beginning of a file
  0 constant seek-set

  \ Seek from the current position in a file
  1 constant seek-cur

  \ Seek from the end of a file
  2 constant seek-end
  
  \ Base file class
  <entity> begin-class <file>
    \ Read from a file
    method read-file ( c-addr u file -- u )

    \ Write to a file
    method write-file ( c-addr u file -- u )

    \ Seek in a file
    method seek-file ( offset whence file -- )

    \ Get the current offset in a file
    method tell-file ( file -- offset )

    \ Flush a file
    method flush-file ( file -- )

    \ Close a file
    method close-file ( file -- )
  end-class

  \ Implement base file class
  <file> begin-implement
    \ Read from a file ( c-addr u file -- u )
    ' abstract-method define read-file

    \ Write to a file ( c-addr u file -- u )
    ' abstract-method define write-file

    \ Seek in a file ( offset whence file -- )
    ' abstract-method define seek-file

    \ Get the current offset in a file ( file -- offset )
    ' abstract-method define tell-file

    \ Flush a file ( file -- )
    ' abstract-method define flush-file

    \ Close a file ( file -- )
    ' abstract-method define close-file
  end-implement

  \ File context class
  <object> begin-class <file-context>
    \ Root filesystem
    cell member root-fs

    \ Working directory
    2 cells member working-dir

    \ Get the class of an entity at a given path
    method find-entity-class@ ( c-addr u context -- class )
    
    \ Initialize an entity at a given path
    method find-init-entity ( c-addr u addr context -- )
    
    \ Execute an execution token with an entity
    method find-with-entity ( c-addr u xt context -- )
    
    \ Execute an execution token with the root directory
    method with-root-dir ( xt context -- )
    
    \ Execute an execution token with a parsed absolute path
    method parse-abs-path ( c-addr u xt context -- )

    \ Execute an execution token with a parsed relative path
    method parse-rel-path ( c-addr u xt context -- )

    \ Execute an execution token with a parsed path
    method parse-path ( c-addr u xt context -- )
  end-class

  \ Get the path element count
  : path-element-count ( c-addr u -- count )
    1 begin over while 2 pick c@ [char] / = if 1+ then rot 1+ rot 1- rot repeat
    nip nip
  ;

  \ Execute an execution token with a double-cell array
  : with-2array ( u xt -- ) swap 2 * cells swap with-aligned-allot ;

  \ Find the end of the current directory part
  : find-end-path-part ( c-addr u -- u )
    0 begin
      over 0> if
        1+ swap 1- swap rot 1+ -rot 2 pick 1- c@ [char] / =
      else
        true
      then
    until
    nip nip
  ;

  \ Strip trailing slash off a directory
  : strip-end-slash ( c-addr u -- c-addr u )
    over >r
    dup 0> if dup 1- rot + c@ [char] / = if 1- then else 2drop 0 then
    r> swap
  ;

  \ Process a path component
  : process-path-part ( c-addr u index -- result )
    over 0= if
      dup 0= if 1 else 0 then nip nip nip
    else
      -rot 2dup s" ." equal-strings? if
        2drop
      else
	s" .." equal-strings? if dup 0<> averts x-entity-not-found 1- else 1+ then
      then
    then
  ;
  
  \ Populate a path component into an array
  : populate-path-part ( c-addr u index array -- c-addr u index )
    3 pick 3 pick find-end-path-part ( c-addr u index array u' )
    swap 2 pick 2 * cells + ( c-addr u index u' dest )
    4 pick 2 pick strip-end-slash rot 2! ( c-addr u index u' )
    3 pick over strip-end-slash 3 roll process-path-part ( c-addr u u' index' )
    -rot tuck - ( c-addr index' u' u'' )
    3 roll rot + ( index' u'' c-addr' )
    -rot swap ( c-addr' u'' index' ) 
  ;
  
  \ Execute an execution token with a compressed path
  : with-compressed-path ( c-addr u xt -- )
    rot 1+ rot 1- rot 2 pick 2 pick path-element-count dup [: ( c-addr u xt elements array )
      0 ( c-addr u xt elements array index )
      swap 5 pick 5 pick 2swap 4 pick ( c-addr u xt elements c-addr u index array elements )
      begin dup 0> if >r dup >r populate-path-part r> r> 1- false else true then until 
      ( c-addr u xt elements c-addr u index array elements )
      drop swap 2>r 2drop drop nip nip 2r> rot execute
    ;] with-2array
  ;
  
  \ Get the last element in a double-cell array
  : 2array-last ( elements count -- xd ) 1- 2 * cells + 2@ ;

  \ Look up a file path and call an execution token with the found element and the
  \ found containing directory
  : with-lookup-path ( c-addr u dir xt -- )
    >r -rot r> -rot ( dir xt c-addr u )
    [:
      [: ( dir xt elements count ) 2swap swap 2swap ( xt dir elements count )
        dup 0> if ( xt dir elements count )
          2dup 2>r 1- [: ( xt dir c-addr u )
            2 pick entity-dir? averts x-entity-not-found
            dup 0<> if ( xt dir c-addr u )
              3dup rot entity-class@ class-size file-object-pool allocate-temp
              ( xt dir c-addr u dir' )
              dup >r 3 roll init-entity r> ( xt dir' )
            else
              2drop
            then
          ;] 2iter ( xt dir' )
          2r> 2array-last 2swap swap ( c-addr u dir' xt ) execute
        else
          ['] x-entity-not-found ?raise
        then
      ;] with-compressed-path
    ;] file-lock with-lock
  ;

  \ Call an execution token with a concatenated path
  : with-concat-path ( c-addr u c-addr u xt -- )
    >r 2swap strip-end-slash 2swap strip-end-slash r>
    3 pick 2 pick 1+ + [: ( c-addr0 u0 c-addr1 u1 xt buf )
      5 pick 5 pick 2 pick swap move ( c-addr0 u0 c-addr1 u1 xt buf )
      [char] / 5 pick 2 pick + c! ( c-addr0 u0 c-addr1 u1 xt buf )
      3 pick 3 pick 2 pick ( c-addr0 u0 c-addr1 u1 xt buf c-addr1 u1 buf )
      7 pick 1+ + swap move ( c-addr0 u0 c-addr1 u1 xt buf )
      rot 1+ ( c-addr0 u0 c-addr1 xt buf bytes ) 3 roll drop ( c-addr0 u0 xt buf bytes )
      3 roll + ( c-addr0 xt buf bytes' ) 3 roll drop ( xt buf bytes' )
      rot execute
    ;] with-allot
  ;

  \ Implement the file context class
  <file-context> begin-implement
    :noname ( working-dir root-fs context -- )
      dup [ <object> ] -> new
      tuck root-fs !
      working-dir 2!
    ; define new
    :noname ( c-addr u context -- class )
      -rot 2 pick [: ( context c-addr' u' dir )
        over 0> if
          entity-class@ nip
        else
          2drop drop root-fs @ base-dir-class@
        then
      ;] swap parse-path
    ; define find-entity-class@
    :noname ( c-addr u addr context -- )
      2swap 2 pick [: ( addr context c-addr' u' dir )
        over 0> if
          4 roll swap init-entity drop
        else
          2drop drop root-fs @ init-base-dir
        then
      ;] swap parse-path
    ; define find-init-entity
    :noname ( c-addr u xt context -- )
      2swap 2 pick [: ( xt context c-addr' u' dir )
        over 0> if
          4 roll swap with-entity drop
        else
          2drop drop root-fs @ with-base-dir
        then
      ;] swap parse-path
    ; define find-with-entity
    :noname ( xt context -- )
      root-fs @ with-base-dir
    ; define with-root-dir
    :noname ( c-addr u xt context -- )
      [: ( c-addr u xt root-dir )
        2swap rot ( xt c-addr u root-dir )
        [: ( xt c-addr' u' dir )
          3 roll execute
        ;] with-lookup-path
      ;] swap with-root-dir
    ; define parse-abs-path
    :noname ( c-addr u xt context -- )
      2swap 2 pick working-dir 2@ 2swap ( xt context c-addr0 u0 c-addr1 u1 ) [:
        ( xt context c-addr' u' -- ) 2swap parse-abs-path
      ;] with-concat-path
    ; define parse-rel-path
    :noname ( c-addr u xt context -- )
      2 pick 0> averts x-entity-not-found
      3 pick c@ [char] / = if parse-abs-path else parse-rel-path then
    ; define parse-path
  end-implement

  \ Initialize files
  : init-files ( -- )
    file-object-pool-size file-object-pool init-temp
    file-lock init-lock
  ;
  
end-module
