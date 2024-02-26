\ Copyright (c) 2024 Travis Bemann
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

begin-module dynamic

  task import
  internal import
  
  \ Dynamic variable not set exception
  : x-dyn-variable-not-set ( -- ) ." dynamic variable not set" cr ;
  
  begin-module dynamic-internal
    
    \ Single cell dynamic variable stack
    user single-dyn-stack

    \ Double cell dynamic variable stack
    user double-dyn-stack

    \ Saved task initialization hook
    variable saved-task-init-hook
    
    \ RAM current dynamic id
    -1 value current-ram-dynamic-id

    \ Flash current dynamid id
    0 value current-flash-dynamic-id
    
    \ Specify current flash dynamically-scoped variable ID
    : set-current-flash-dynamic-id ( id -- )
      get-current
      swap
      internal set-current
      s" *DYNAMIC*" constant-with-name
      set-current
    ;

    \ Get current flash dynamically-scoped variable ID
    : get-current-flash-dynamic-id ( -- id )
      s" *DYNAMIC*" flash-latest find-all-dict dup if
        >xt execute
      else
        drop 0
      then
    ;

    \ Initialize dynamic variables
    : init-dynamic ( -- )
      get-current-flash-dynamic-id to current-flash-dynamic-id
      0 single-dyn-stack !
      0 double-dyn-stack !
      task-init-hook @ saved-task-init-hook !
      [: { new-task -- }
        saved-task-init-hook @ ?dup if new-task swap execute then
        0 new-task ['] single-dyn-stack for-task!
        0 new-task ['] double-dyn-stack for-task!
      ;] task-init-hook !
    ;
    
  end-module> import

  \ Define a dynamic variable
  : dyn ( "name" -- )
    token dup 0<> averts x-token-expected
    compiling-to-flash? if
      current-flash-dynamic-id 1+ dup 1 and 0= if 1+ then
      dup to current-flash-dynamic-id dup set-current-flash-dynamic-id
    else
      current-ram-dynamic-id 1- dup 1 and 0= if 1- then
      dup to current-ram-dynamic-id
    then
    -rot constant-with-name
  ;

  \ Define a double-cell dynamic variable
  : 2dyn ( "name" -- )
    token dup 0<> averts x-token-expected
    compiling-to-flash? if
      current-flash-dynamic-id 1+ dup 1 and 0<> if 1+ then
      dup to current-flash-dynamic-id dup set-current-flash-dynamic-id
    else
      current-ram-dynamic-id 1- dup 1 and 0<> if 1- then
      dup to current-ram-dynamic-id
    then
    -rot constant-with-name
  ;

  \ Set a dynamic variable within a set scope
  : dyn! ( xt x|xd dyn-variable -- )
    dup 1 and if
      here [ 3 cells ] literal allot
      single-dyn-stack @ over !
      tuck cell+ !
      tuck [ 2 cells ] literal + !
      single-dyn-stack !
      try
      here [ -3 cells ] literal + @ single-dyn-stack !
      [ -3 cells ] literal allot
    else
      here [ 4 cells ] literal allot
      double-dyn-stack @ over !
      tuck cell+ !
      rot rot 2 pick [ 2 cells ] literal + 2!
      double-dyn-stack !
      try
      here [ -4 cells ] literal + @ double-dyn-stack !
      [ -4 cells ] literal allot
    then
    ?raise
  ;

  \ Set a dynamic variable without a scope
  : dyn-no-scope! ( x|xd dyn-variable -- )
    dup 1 and if
      here [ 3 cells ] literal allot
      single-dyn-stack @ over !
      tuck cell+ !
      tuck [ 2 cells ] literal + !
      single-dyn-stack !
    else
      here [ 4 cells ] literal allot
      double-dyn-stack @ over !
      tuck cell+ !
      rot rot 2 pick [ 2 cells ] literal + 2!
      double-dyn-stack !
    then
  ;

  \ Get a dynamic variable
  : dyn@ ( dyn-variable - x|xd )
    dup 1 and if
      single-dyn-stack @ begin ?dup while
        dup cell+ @ 2 pick = if [ 2 cells ] literal + nip @ exit then
        @
      repeat
    else
      double-dyn-stack @ begin ?dup while
        dup cell+ @ 2 pick = if [ 2 cells ] literal + nip 2@ exit then
        @
      repeat
    then
    ['] x-dyn-variable-not-set ?raise
  ;
  
end-module

initializer dynamic::dynamic-internal::init-dynamic

reboot
