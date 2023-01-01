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

\ Begin compressing flash
compress-flash

begin-module action-pool

  action import

  \ No actions are available
  : x-no-action-available ( -- ) ." no action is available" cr ;
  
  begin-module action-pool-internal

    \ Action pool structure
    begin-structure action-pool-size

      \ Action pool count
      field: action-pool-count
      
    end-structure

    commit-flash
    
    \ Get a action in a action pool
    : action@ ( index action-pool -- action )
      action-pool-size + swap action-size * +
    ;

    \ Find a free action in an action pool
    : find-free-action ( action-pool -- action )
      dup action-pool-count @ 0 ?do
	i over action@ dup in-schedule? not if nip unloop exit else drop then
      loop
      drop ['] x-no-action-available ?raise
    ;

  end-module> import
  
  commit-flash
    
  \ Get action pool free count
  : action-pool-free ( action-pool -- count )
    0 swap dup action-pool-count @ 0 ?do
      i over action@ in-schedule? not if swap 1+ swap then
    loop
    drop
  ;
  
  \ Initialize a action pool
  : init-action-pool ( count addr -- )
    tuck action-pool-count !
    dup action-pool-count @ 0 ?do
      0 [: ;] i 3 pick action@ init-action
    loop
    drop
  ;

  \ Initialize an action from a action pool
  : add-action-from-pool ( schedule data xt action-pool -- action )
    find-free-action dup >r init-action r@ add-action r>
  ;

  \ Get the size of a action pool of a given size
  : action-pool-size ( count -- bytes ) action-size * action-pool-size + ;

end-module

end-compress-flash
  
