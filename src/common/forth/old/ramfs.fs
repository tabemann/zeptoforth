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

begin-module ramfs

  oo import
  file import

  \ RamFS filesystem file proxy class
  <file> begin-class <ram-file-proxy>
    cell member ram-file-orig
    cell member ram-file-offset
  end-class

  \ RamFS filesystem directory proxy class
  <dir> begin-class <ram-dir-proxy>
    cell member ram-dir-orig
  end-class

  \ RamFS filesystem mount point proxy class
  <dir> begin-class <ram-mount-proxy>
    cell member ram-mount-orig
  end-class

  \ RamFS filesystem real entity base class
  <object> begin-class <ram-real-entity>
    cell member next-ram-entity
    2 cells member ram-entity-name
  end-class

  \ Implement RamFS filesystem real entity base class
  <ram-real-entity> begin-implement
    :noname ( next-ram-entity c-addr u entity -- )
      dup <object>->new
      dup >r ram-entity-name 2! r>
      next-ram-entity !
    ; define new
  end-implement
  
  \ RamFS filesystem real file class
  <ram-real-entity> begin-class <ram-real-file>
    cell member ram-data-addr
    cell member ram-data-size
  end-class

  \ Implement RamFS filesystem real file class
  <ram-real-file> begin-implement
    :noname ( data-addr data-bytes next-ram-entity c-addr u entity -- )
      dup >r <ram-real-entity>->new
      r@ ram-data-size !
      r> ram-data-addr !
    ; define new
  end-implement
  
  \ RamFS filesystem directory class
  <ram-real-entity> begin-class <ram-real-dir>
    \ The first member of the directory
    cell member first-real-dir-member
    
    \ Get the class of an entity
    method real-entity-class@ ( c-addr u dir -- class )

    \ Initialize the memory for an entity
    method real-init-entity ( c-addr u addr dir -- )

    \ Get the class of a directory
    method real-dir-class@ ( dir -- class )
    
    \ Create a directory
    method real-create-dir ( c-addr u addr dir -- )

    \ Get the class of a file
    method real-file-class@ ( dir -- class )
    
    \ Create an ordinary file
    method real-create-file ( c-addr u addr dir -- )

    \ Get the name of an entity at a given index if one exists
    method real-entity-name@ ( index dir -- c-addr u | 0 0 )

    \ Look up an entity by name
    method lookup-entity ( c-addr u dir -- real-entity )
  end-class

  \ RamFS filesystem directory class
  <ram-real-entity> begin-class <ram-real-mount>
    \ The mounted filesystem
    cell member mounted-fs
    
    \ Get the class of an entity
    method mount-entity-class@ ( c-addr u dir -- class )

    \ Initialize the memory for an entity
    method mount-init-entity ( c-addr u addr dir -- )

    \ Get the class of a directory
    method mount-dir-class@ ( dir -- class )
    
    \ Create a directory
    method mount-create-dir ( c-addr u addr dir -- )

    \ Get the class of a file
    method mount-file-class@ ( dir -- class )
    
    \ Create an ordinary file
    method mount-create-file ( c-addr u addr dir -- )

    \ Get the name of an entity at a given index if one exists
    method mount-entity-name@ ( index dir -- c-addr u | 0 0 )

    \ Execute an xt against the mounted filesystem
    method execute-mount ( xt dir -- )
  end-class

  \ Implement RamFS filesystem directory class
  <ram-real-dir> begin-implement
    :noname ( first-real-dir-member next-ram-entity c-addr u entity -- )
      dup >r <ram-real-entity>->new
      r> first-real-dir-member !
    ; define new
    :noname ( c-addr u dir -- class )
      lookup-entity object-class case
	<ram-real-dir> of <ram-dir-proxy> endof
	<ram-real-file> of <ram-file-proxy> endof
	<ram-real-mount> of <ram-mount-proxy> endof
      endcase
    ; define real-entity-class@
    :noname ( c-addr u addr dir -- )
      swap >r lookup-entity dup object-class case
	<ram-real-dir> of <ram-dir-proxy> r> init-object endof
	<ram-real-file> of <ram-file-proxy> r> init-object endof
	<ram-real-mount> of <ram-mount-proxy> r> init-object endof
      endcase
    ; define real-init-entity
    :noname drop <ram-dir-proxy> ; define real-dir-class@
    :noname ['] x-read-only ?raise ; define real-create-dir
    :noname drop <ram-file-proxy> ; define real-file-class@
    :noname ['] x-read-only ?raise ; define real-create-file
    :noname ( index dir -- c-addr u | 0 0 )
      first-real-dir-member @ begin
        dup if
          over 0> if next-ram-entity @ swap 1- swap false else true then
        else
          true
        then
      until
      dup 0= if 2drop 0 0 else nip ram-entity-name 2@ then
    ; define real-entity-name@
    :noname ( c-addr u dir -- )
      first-real-dir-member @ begin
	dup averts x-entity-not-found
	3dup ram-entity-name 2@ equal-strings? dup not if
	  swap next-ram-entity @ swap
	then
      until
      nip nip 
    ; define lookup-entity
  end-implement

  \ Implement RamFS filesystem directory class
  <ram-real-mount> begin-implement
    :noname ( mounted-fs next-ram-entity c-addr u entity -- )
      dup >r <ram-real-entity>->new r>
      mounted-fs !
    ; define new
    :noname ( c-addr u dir -- class ) [: entity-class@ ;] swap execute-mount ;
    define mount-entity-class@
    :noname ( c-addr u addr dir -- ) [: init-entity ;] swap execute-mount ;
    define mount-init-entity
    :noname ( dir -- ) [: dir-class@ ;] swap execute-mount ;
    define mount-dir-class@
    :noname ( c-addr u addr dir -- ) [: create-dir ;] swap execute-mount ;
    define mount-create-dir
    :noname ( dir -- ) [: file-class@ ;] swap execute-mount ;
    define mount-file-class@
    :noname ( c-addr u addr dir -- ) [: create-file ;] swap execute-mount ;
    define mount-create-file
    :noname ( index dir -- c-addr u | 0 0 ) [: entity-name@ ;] swap execute-mount ;
    define mount-entity-name@
    :noname ( xt dir -- )
      mounted-fs @ with-base-dir
\      dup mounted-fs @ base-dir-class@ class-size [:
\	tuck swap mounted-fs @ init-base-dir swap execute
\      ;] with-aligned-allot
    ; define execute-mount
  end-implement

  \ Implement RamFS filesystem file proxy class
  <ram-file-proxy> begin-implement
    :noname ( ram-file-orig proxy -- )
      dup <file>->new
      tuck ram-file-orig !
      0 swap ram-file-offset !
    ; define new
    :noname drop true ; define entity-file?
    :noname drop false ; define entity-read-only?
    :noname ( c-addr u file -- u )
      dup >r ram-file-orig @ ram-data-size @ r@ ram-file-offset @ -
      min tuck r@ ram-file-orig @ ram-data-addr @ r@ ram-file-offset @ + -rot move
      dup r> ram-file-offset +!
    ; define read-file
    :noname ( c-addr u file -- u )
      dup >r ram-file-orig @ ram-data-size @ r@ ram-file-offset @ -
      min tuck r@ ram-file-orig @ ram-data-addr @ r@ ram-file-offset @ + swap move
      dup r> ram-file-offset +!
    ; define write-file
    :noname ( offset whence file -- )
      >r case
	seek-cur of r@ ram-file-offset @ + endof
	seek-end of r@ ram-file-orig @ ram-data-size @ + endof
      endcase
      0 max r@ ram-file-orig @ ram-data-size @ min r> ram-file-offset !
    ; define seek-file
    :noname ( file -- offset ) ram-file-offset @ ; define tell-file
    :noname ( file -- ) drop ; define flush-file
    :noname ( file -- ) drop ; define close-file
  end-implement
  
  \ Implement RamFS filesystem directory proxy class
  <ram-dir-proxy> begin-implement
    :noname ( ram-dir-orig proxy -- )
      dup <dir>->new
      ram-dir-orig !
    ; define new
    :noname drop true ; define entity-dir?
    :noname ram-dir-orig @ real-entity-class@ ; define entity-class@
    :noname ram-dir-orig @ real-init-entity ; define init-entity
    :noname ram-dir-orig @ real-dir-class@ ; define dir-class@
    :noname ram-dir-orig @ real-create-dir ; define create-dir
    :noname ram-dir-orig @ real-file-class@ ; define file-class@
    :noname ram-dir-orig @ real-create-file ; define create-file
    :noname ram-dir-orig @ real-entity-name@ ; define entity-name@
  end-implement

  \ Implement RamFS filesystem mount point proxy class
  <ram-mount-proxy> begin-implement
    :noname ( ram-mount-orig proxy -- )
      dup <dir>->new
      ram-mount-orig !
    ; define new
    :noname drop true ; define entity-dir?
    :noname ram-mount-orig @ mount-entity-class@ ; define entity-class@
    :noname ram-mount-orig @ mount-init-entity ; define init-entity
    :noname ram-mount-orig @ mount-dir-class@ ; define dir-class@
    :noname ram-mount-orig @ mount-create-dir ; define create-dir
    :noname ram-mount-orig @ mount-file-class@ ; define file-class@
    :noname ram-mount-orig @ mount-create-file ; define create-file
    :noname ram-mount-orig @ mount-entity-name@ ; define entity-name@
  end-implement
  
  \ RamFS filesystem class
  <fs> begin-class <ram-fs>
    cell member ram-root-dir
  end-class

  \ Implement RamFS filesystem class
  <ram-fs> begin-implement
    :noname ( first-real-dir-member fs -- )
      dup <fs>->new
      ram-root-dir !
    ; define new
    :noname ( fs -- class ) drop <ram-dir-proxy> ; define base-dir-class@
    :noname ( addr fs -- ) ram-root-dir @ <ram-dir-proxy> rot init-object ;
    define init-base-dir
    :noname ( xt fs -- )
      <ram-dir-proxy> class-size [:
        >r ram-root-dir @ <ram-dir-proxy> r@ init-object
        r> swap execute
      ;] with-aligned-allot
    ; define with-base-dir
  end-implement

end-module