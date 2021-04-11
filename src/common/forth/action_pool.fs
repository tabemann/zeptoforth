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

begin-module-once action-pool-module

  import schedule-module

  begin-import-module action-pool-internal-module

    \ Action pool structure
    begin-structure action-pool-size
      \ Action pool count
      field: action-pool-count

      \ Action pool schedule
      field: action-pool-schedule
    end-structure

    \ Get a action in a action pool
    : action@ ( index action-pool -- action )
      action-pool-size + swap action-size * +
    ;

  end-module
  
  \ No actions are available
  : x-no-action-available ( -- ) space ." no action is available" cr ;
  
  \ Initialize an action from a action pool
  : init-from-action-pool ( xt action-pool -- action )
    begin-critical
    dup action-pool-count @ 0 ?do
      i over action@ action-disposed? if
	i over action@ dup >r swap action-pool-schedule @
	init-action r> end-critical unloop exit
      then
    loop
    end-critical ['] x-no-action-available ?raise
  ;

  \ Get action pool free count
  : action-pool-free ( action-pool -- count )
    begin-critical
    0 swap dup action-pool-count @ 0 ?do
      i over action@ action-disposed? if swap 1+ swap then
    loop
    drop end-critical
  ;
  
  \ Initialize a action pool
  : init-action-pool ( schedule count addr -- )
    tuck action-pool-count !
    tuck action-pool-schedule !
    dup action-pool-count @ 0 ?do
      i over action@ action-size $FF fill \ This marks the action as disposed
    loop
    drop
  ;

  \ Get the size of a action pool of a given size
  : action-pool-size ( count -- bytes ) action-size * action-pool-size + ;

end-module

