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

begin-module closure

  armv6m import
  internal import

  begin-module closure-internal
    
    \ Calculate a closure size
    : find-closure-size ( -- bytes )
      compiling-to-flash? { to-flash? }
      compile-to-ram
      here { here-saved }
      4 align,
      here { here-start }
      0 h,
      $FEDCBA98 lit, $FEDCBA99 lit,
      undefer-lit
      [ armv6m-instr import ]
      tos r0 movs_,_
      tos 1 dp ldm    
      r0 bx_
      [ armv6m-instr unimport ]
      thumb-2? not if consts, then
      here here-start - 4 align
      here-saved ram-here!
      to-flash? if compile-to-flash then
    ;

    \ Calculate a two-argument closure size
    : find-2closure-size ( -- bytes )
      compiling-to-flash? { to-flash? }
      compile-to-ram
      here { here-saved }
      4 align,
      here { here-start }
      0 h,
      $FEDCBA98 lit, $FEDCBA99 lit, $FEDCBA9A lit,
      undefer-lit
      [ armv6m-instr import ]
      tos r0 movs_,_
      tos 1 dp ldm    
      r0 bx_
      [ armv6m-instr unimport ]
      thumb-2? not if consts, then
      here here-start - 4 align
      here-saved ram-here!
      to-flash? if compile-to-flash then
    ;

  end-module> import

  \ Single-cell closure size
  find-closure-size constant closure-size
  
  \ Double-cell closure size
  find-2closure-size constant 2closure-size
  
  \ Multi-cell closure size
  : nclosure-size ( count -- bytes ) 2 + cells closure-size + ;
  
  \ Bind a closure
  : bind { data addr xt -- }
    compiling-to-flash? { to-flash? }
    compile-to-ram
    here { here-saved }
    addr ram-here!
    data lit, xt 1 or lit,
    undefer-lit
    [ armv6m-instr import ]
    tos r0 movs_,_
    tos 1 dp ldm    
    r0 bx_
    [ armv6m-instr unimport ]
    thumb-2? not if consts, then
    here-saved ram-here!
    to-flash? if compile-to-flash then
  ;

  \ Bind a two-argument closure
  : 2bind { data1 data0 addr xt -- }
    compiling-to-flash? { to-flash? }
    compile-to-ram
    here { here-saved }
    addr ram-here!
    data1 lit, data0 lit, xt 1 or lit,
    undefer-lit
    [ armv6m-instr import ]
    tos r0 movs_,_
    tos 1 dp ldm    
    r0 bx_
    [ armv6m-instr unimport ]
    thumb-2? not if consts, then
    here-saved ram-here!
    to-flash? if compile-to-flash then
  ;

  \ Bind a multi-argument closure
  : nbind { count addr xt -- }
    addr closure-size + { data-addr }
    xt data-addr !
    count data-addr cell+ !
    count 0> if
      2 count 1+ ?do
        i cells data-addr + !
      -1 +loop
    then
    data-addr addr [: { data-addr }
      data-addr cell+ @ 2 + 2 ?do
        i cells data-addr + @
      loop
      data-addr @ execute
    ;] bind
  ;

  \ Create a one-argument closure in a set scope in the dictionary
  : with-closure ( data bound-xt scope-xt -- )
    closure-size [: swap >r >r
      r@ swap bind r> r> execute
    ;] with-aligned-allot
  ;

  \ Create a two-argument closure in a set scope in the dictionary
  : with-2closure ( data1 data0 bound-xt scope-xt -- )
    2closure-size [: swap >r >r
      r@ swap 2bind r> r> execute
    ;] with-aligned-allot
  ;

  \ Create a multi-argument closure in a set scope in the dictionary
  : with-nclosure ( datan ... data0 count bound-xt scope-xt -- )
    2 pick nclosure-size [: swap >r >r
      r@ swap nbind r> r> execute
    ;] with-aligned-allot
  ;

end-module

reboot
