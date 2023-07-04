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

compile-to-flash

begin-module oo

  internal import
  armv6m import

  begin-module oo-internal
    
    \ The method structure
    begin-structure method-header-size
      
      \ The previous method
      field: prev-method

      \ The method index
      field: method-index

      \ The method word
      field: method-word
      
    end-structure
    
    \ The class header structure
    begin-structure class-header-size

      \ The method table of a class
      field: class-methods
      
      \ The superclass of a class
      field: class-superclass

      \ The method list of a class
      field: class-method-list
      
      \ The size of the members of a class
      field: members-size
      
      \ The number of methods of a class
      field: method-count
      
    end-structure

    \ A method of a class
    : class-method ( index class -- addr )
      class-methods @ swap 1+ cells +
    ;

    \ Find a method index by name
    : method-by-name ( c-addr u class -- index|-1 )
      class-method-list @ begin ?dup while
	dup method-word @ word-name 2over rot count
	equal-case-strings? if
	  nip nip method-index @ exit
	then
	prev-method @
      repeat
      2drop -1
    ;
    
    \ Get a method list length
    : method-list-len ( list -- len )
      0 swap begin ?dup while swap 1+ swap prev-method @ repeat
    ;
    
    \ Base new method
    : new ( object -- ) drop ;

    \ Base destroy method
    : destroy ( object -- ) drop ;

  end-module> import

  \ Method not found in class exception
  : x-method-not-in-class ( -- ) ." method not in class" cr ;
  
  \ Method not implemented exception
  : x-method-not-implemented ( -- ) ." method not implemented" cr ;

  \ Method not implemented yet exception
  : x-method-not-implemented-yet ( -- ) ." method not implemented yet" cr ;

  \ Abstract method
  : abstract-method ( -- ) ['] x-method-not-implemented ?raise ;
  
  \ The object class's method list
  create new-method 0 , 0 , s" new" find ,
  create destroy-method new-method , 1 , s" destroy" find ,

  \ The object class's method table
  create object-method-table cell allot ' new 1 or , ' destroy 1 or ,
    
  \ Define the object class
  create <object> object-method-table , here cell - , destroy-method , 0 , 2 ,
  here 5 cells - object-method-table current!
  
  \ This is the entry point to the new method
  : new ( object -- ) dup @ 4 + @ 1 bic execute ;

  \ This is the entry point to the destroy method
  : destroy ( object -- ) dup @ 8 + @ 1 bic execute ;

  \ Begin the declaration of a class
  : begin-class
    ( super-class "name" -- class-header member-offset list method-count )
    { super-class }
    create here cell allot super-class , 3 cells allot
    super-class members-size @
    super-class class-method-list @
    super-class method-count @
    compiling-to-flash? if flash-block-align, then
  ;

  \ Finish the declaration of a class
  : end-class ( class-header member-offset list method-count -- )
    { class-header member-offset method-list #methods }
    member-offset class-header members-size current!
    #methods class-header method-count current!
    cell align,
    compiling-to-flash? if flash-block-align, then
    here #methods 1+ cells allot { methods }
    compiling-to-flash? if flash-block-align, then
    methods class-header class-methods current!
    class-header methods current!
    method-list class-header class-method-list current!
  ;

  \ Begin the implementation of a class
  : begin-implement ( class -- class-header )
    compiling-to-flash? not if
      dup class-methods @ cell+ over class-method-list @ method-index @ 1+ cells
      0 fill
    then
  ;

  \ End the implementation of a class
  : end-implement { class-header -- }
    class-header class-method-list @ { current-method }
    begin current-method while
      current-method method-index @ { index }
      index class-header class-method flash-buffer@
      dup 0= compiling-to-flash? not and swap -1 = or if
        index class-header class-superclass @ method-count @ < if
          index class-header class-superclass @ class-method @
          index class-header class-method current!
        else
          ['] abstract-method 1 or index class-header class-method current!
        then
      then
      current-method prev-method @ to current-method
    repeat
  ;

  \ Declare a member of a class
  : member ( member-offset list method-count size "name" -- member-offset list method-count )
    { member-offset method-list #methods size }
    : inlined member-offset cell+ lit, postpone + postpone ;
    member-offset size + method-list #methods
  ;
  
  \ Declare a method of a class
  : method ( list method-count "name" -- list method-count )
    { method-list #methods }
    #methods 31 < if
      :
        inlined
        [ armv6m-instr import ]
        0 r6 r0 ldr_,[_,#_]
        #methods 1+ cells r0 r0 ldr_,[_,#_]
        r0 blx_
        [ armv6m-instr unimport ]
      postpone ;
    else
      :
        [ armv6m-instr import ]
        0 r6 r0 ldr_,[_,#_]
        #methods 1+ cells r1 literal,
        r1 r0 r0 ldr_,[_,#_]
        r0 blx_
        [ armv6m-instr unimport ]
      postpone ;
    then
    cell align, here method-header-size allot { method-header }
    method-list method-header prev-method current!
    #methods method-header method-index current!
    latest method-header method-word current!
    compiling-to-flash? if flash-block-align, then
    method-header #methods 1+
  ;

  \ Define a method of a class
  : define ( class-header xt "name" -- class-header )
    1 or
    token
    dup 0= triggers x-token-expected
    3 pick method-by-name dup -1 <> averts x-method-not-in-class
    2 pick class-method current!
  ;

  \ Get the size of an object of a class
  : class-size ( class -- bytes ) members-size @ cell+ ;

  \ Get the class of an object
  : object-class ( object -- class ) @ @ ;

  \ Initialize an instance of a class
  : init-object ( ? class addr -- ) tuck swap @ swap ! new ;

  \ Allot an object and initialize it
  : with-object ( ? class xt -- )
    over class-size [:
      swap >r dup >r init-object
      r> r> over >r execute
      r> destroy
    ;] with-aligned-allot
  ;

  continue-module oo-internal

    \ The old find hook
    variable old-find-hook
    
    \ Find the arrow in a name
    : find-arrow ( c-addr u -- u'|-1 )
      swap 1+ swap 1-
      1 begin over 2 > while
        2 pick c@ [char] - = if
          2 pick 1+ c@ [char] > = if
            nip nip exit
          then
        then
        rot 1+ rot 1- rot 1+
      repeat
      2drop drop -1
    ;
    
    \ Execute or compile a particular word in a provided module
    : do-find-with-arrow ( name-addr name-bytes -- word|0 )
      2dup find-arrow dup -1 <> if
        2 pick over old-find-hook @ execute ?dup if
          >r 2 + tuck - -rot + swap r> >xt execute >r
          r@ method-by-name dup -1 <> if
            r> class-method flash-buffer@ dup 0<> over -1 <> and if
              1 bic find-by-xt
            else
              drop ['] x-method-not-implemented-yet ?raise
            then
          else
            rdrop drop 0
          then
        else
          2drop drop 0
        then
      else
        drop old-find-hook @ execute
      then
    ;
  
  end-module
  
end-module

\ Initialize
: init ( -- )
  init
  find-hook @ oo::oo-internal::old-find-hook !
  ['] oo::oo-internal::do-find-with-arrow find-hook !
;

reboot
