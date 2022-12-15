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

compile-to-flash

begin-module closure

  armv6m import
  internal import

  \ Calculate a closure size
  : find-closure-size ( -- bytes )
    compiling-to-flash? { to-flash? }
    compile-to-ram
    here { here-saved }
    $FEDCBA98 lit, $FEDCBA99 lit,
    undefer-lit
    [ armv6m-instr import ]
    tos r0 movs_,_
    tos 1 dp ldm    
    r0 bx_
    [ armv6m-instr unimport ]
    thumb-2? not if consts, then
    here here-saved - 4 align
    here-saved ram-here!
    to-flash? if compile-to-flash then
  ;

  \ Closure size
  find-closure-size constant closure-size
  
  \ Calculate a two-argument closure size
  : find-2closure-size ( -- bytes )
    compiling-to-flash? { to-flash? }
    compile-to-ram
    here { here-saved }
    $FEDCBA98 lit, $FEDCBA99 lit, $FEDCBA9A lit,
    undefer-lit
    [ armv6m-instr import ]
    tos r0 movs_,_
    tos 1 dp ldm    
    r0 bx_
    [ armv6m-instr unimport ]
    thumb-2? not if consts, then
    here here-saved - 4 align
    here-saved ram-here!
    to-flash? if compile-to-flash then
  ;

  \ Closure size
  find-2closure-size constant 2closure-size

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

end-module

reboot
