\ Copyright (c) 2025 Travis Bemann
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

begin-module int-overload

  interrupt import
  closure import
  lock import
  armv6m import

  \ No interrupt adapter available exception
  : x-no-int-adapter ( -- ) ." no interrupt adapter available for vector" cr ;
  
  begin-module int-overload-internal
    
    \ The first interrupt adapter
    0 value first-adapter

    \ The first interrupt adapter lock
    lock-size buffer: adapter-lock
    
  end-module> import

  \ The overloaded interrupt structure
  begin-structure int-overload-size

    continue-module int-overload-internal

      \ The interrupt handler
      dup constant .overload-handler field: overload-handler

      \ The next overload
      dup constant .next-overload field: next-overload

    end-module

  end-structure
  
  \ The interrupt adapter structure
  begin-structure int-adapter-size

    continue-module int-overload-internal
    
      \ The interrupt handler for the interrupt adapter
      closure-size +field int-adapter-handler

      \ The vector index for the interrupt adapter
      field: int-adapter-vector-index
      
      \ The first overloading interrupt handler registered with the adapter
      dup constant .first-overload field: first-overload

      \ The next interrupt adapter
      field: next-adapter

      \ The base interrupt handler
      dup constant .base-handler field: base-handler

      \ Is there an interrupt adapter already registered
      field: already-registered?
      
    end-module
      
  end-structure
  
  continue-module int-overload-internal

    \ Handle an overloaded interrupt
    : handle-interrupt ( adapter -- )
      code[
      4 dp subs_,#_
      0 dp tos str_,[_,#_]
      .first-overload tos tos ldr_,[_,#_]
      mark<
      0 tos cmp_,#_
      ne bc>
      tos 1 dp ldm
      .base-handler tos tos ldr_,[_,#_]
      0 tos cmp_,#_
      eq bc>
      1 tos adds_,#_
      tos blx_
      >mark
      tos 1 dp ldm
      pc 1 pop
      >mark
      mark<
      .overload-handler tos r0 ldr_,[_,#_]
      1 r0 adds_,#_
      r0 blx_
      .next-overload tos tos ldr_,[_,#_]
      0 tos cmp_,#_
      ne bc<
      tos 1 dp ldm
      .base-handler tos tos ldr_,[_,#_]
      0 tos cmp_,#_
      eq bc>
      1 tos adds_,#_
      tos blx_
      >mark
      tos 1 dp ldm
      ]code
    ;
    
    \ The vector! handler
    : handle-vector! { xt vector-index -- }
      first-adapter begin ?dup while
        dup int-adapter-vector-index @ vector-index = if
          xt swap base-handler ! exit
        then
        next-adapter @
      repeat
    ;
    
    \ Initialize the vector!-hook
    : init-vector!-hook
      ['] handle-vector! vector!-hook !
      adapter-lock init-lock
    ;

    initializer init-vector!-hook
    
  end-module

  \ Register an interrupt overload
  : register-int-overload ( xt vector-index addr -- )
    over dup 0> swap vector-count < or averts x-invalid-vector
    [: { xt vector-index addr }
      xt addr overload-handler !
      0 addr next-overload !
      first-adapter begin
        dup if
          dup int-adapter-vector-index @ vector-index = if
            true
          else
            next-adapter @ false
          then
        else
          true
        then
      until
      dup averts x-no-int-adapter
      dup first-overload @ ?dup if
        begin dup next-overload @ ?dup while nip repeat
        addr swap next-overload !
      else
        addr swap first-overload !
      then
    ;] adapter-lock with-lock
  ;

  \ Unregister an interrupt overload
  : unregister-int-overload ( addr -- )
    [: { addr }
      first-adapter begin ?dup while
        dup first-overload @ addr = if
          addr next-overload @ swap first-overload ! exit
        then
        dup first-overload @ begin ?dup while
          dup next-overload @ addr = if
            addr next-overload @ swap next-overload ! drop exit
          then
          next-overload @
        repeat
        next-adapter @
      repeat
    ;] adapter-lock with-lock
  ;
  
  \ Register an interrupt adapter
  : register-int-adapter ( vector-index addr -- )
    over dup 0> swap vector-count < or averts x-invalid-vector
    [: { vector-index addr }
      first-adapter begin ?dup while
        dup int-adapter-vector-index @ vector-index = if
          true addr already-registered? ! exit
        then
        next-adapter @
      repeat
      false addr already-registered? !
      first-adapter addr next-adapter !
      vector-index addr int-adapter-vector-index !
      0 addr first-overload !
      addr to first-adapter
      addr addr int-adapter-handler ['] handle-interrupt bind
      [ cpu-count 1 > ] [if] internal::hold-core [then]
      disable-int
      addr next-adapter @ 0= vector-index vector!-bitmap@ and if
        vector-index vector@ addr
      else
        0
      then
      addr base-handler !
      addr int-adapter-handler vector-index force-vector!
      true vector-index vector!-hook-bitmap!
      enable-int
      [ cpu-count 1 > ] [if] internal::release-core [then]
    ;] adapter-lock with-lock
  ;

  \ Unregister an interrupt adapter
  : unregister-int-adapter ( addr -- )
    [: { addr }
      addr already-registered? @ if exit then
      addr base-handler @ ?dup if else [: ;] then
      addr int-adapter-vector-index @ force-vector!
      first-adapter addr = if
        addr next-adapter @ to first-adapter
        exit
      then
      first-adapter begin ?dup while
        dup next-adapter @ addr = if
          addr next-adapter @ swap next-adapter !
          exit
        then
        next-adapter @
      repeat
    ;] adapter-lock with-lock
  ;

end-module

reboot
