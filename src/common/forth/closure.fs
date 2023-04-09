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
      [:
        here [:
          cell align,
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
          here here-start - cell align
        ;] with-here
      ;] with-ram
    ;

    \ Calculate a double-cell closure size
    : find-dclosure-size ( -- bytes )
      [:
        here [:
          cell align,
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
          here here-start - cell align
        ;] with-here
      ;] with-ram
    ;

    \ Calculate a reference closure size
    : find-refclosure-size ( -- bytes )
      [:
        here [:
          cell align,
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
          cell align,
          0 ,
          here here-start - cell align
        ;] with-here
      ;] with-ram
    ;
    
    \ Calculate a double-cell reference closure size
    : find-2refclosure-size ( -- bytes )
      [:
        here [:
          cell align,
          here { here-start }
          $FEDCBA98 lit, $FEDCBA9A lit,
          undefer-lit
          [ armv6m-instr import ]
          tos r0 movs_,_
          0 dp tos ldr_,[_,#_]
          cell tos adds_,#_
          r0 bx_
          [ armv6m-instr unimport ]
          thumb-2? not if consts, then
          cell align,
          0. 2,
          here here-start - cell align
        ;] with-here
      ;] with-ram
    ;

    \ Calculate a double-cell reference closure size
    : find-drefclosure-size ( -- bytes )
      [:
        here [:
          cell align,
          here { here-start }
          0 h,
          $FEDCBA98 lit, $FEDCBA9A lit,
          undefer-lit
          [ armv6m-instr import ]
          tos r0 movs_,_
          tos 1 dp ldm    
          r0 bx_
          [ armv6m-instr unimport ]
          thumb-2? not if consts, then
          cell align,
          0. 2,
          here here-start - cell align
        ;] with-here
      ;] with-ram
    ;

  end-module> import

  \ Single-cell closure size
  find-closure-size constant closure-size
  
  \ Double-cell closure size
  find-dclosure-size constant dclosure-size
  
  \ Multi-cell closure size
  : nclosure-size ( count -- bytes ) 2 + cells closure-size + ;

  \ Multi-double cell closure size
  : ndclosure-size ( count -- bytes ) 2 * 2 + cells closure-size + ;

  \ Single-cell reference closure size
  find-refclosure-size constant refclosure-size

  \ Double-argument (not double-cell) reference closure size
  find-2refclosure-size constant 2refclosure-size

  \ Double-cell reference closure size
  find-drefclosure-size constant drefclosure-size

  \ Multi-cell reference closure size
  : nrefclosure-size ( count -- bytes ) nclosure-size ;

  \ Multi-double-cell reference closure size
  : ndrefclosure-size ( count -- bytes ) 2 * 2 + cells closure-size + ;
  
  \ Bind a closure
  : bind ( x addr xt -- )
    [:
      swap [: { data xt }
        data lit, xt 1 or lit,
        undefer-lit
        [ armv6m-instr import ]
        tos r0 movs_,_
        tos 1 dp ldm    
        r0 bx_
        [ armv6m-instr unimport ]
        thumb-2? not if consts, then
      ;] with-here
    ;] with-ram
  ;

  \ Bind a double-cell closure
  : dbind ( d addr xt -- )
    [:
      swap [: { data1 data0 xt }
        data1 lit, data0 lit, xt 1 or lit,
        undefer-lit
        [ armv6m-instr import ]
        tos r0 movs_,_
        tos 1 dp ldm    
        r0 bx_
        [ armv6m-instr unimport ]
        thumb-2? not if consts, then
      ;] with-here
    ;] with-ram
  ;

  \ Bind a multi-single-cell closure
  : nbind { count addr xt -- }
    addr closure-size + { data-addr }
    xt data-addr !
    count data-addr cell+ !
    0 { index }
    begin index count < while
      index 2 + cells data-addr + !
      1 +to index
    repeat
    data-addr addr [: >r
      r@ cell+ @
      begin dup while
        1- dup 2 + cells r@ + @ swap
      repeat
      drop
      r> @ execute
    ;] bind
  ;
  
  \ Bind a multi-double-cell closure
  : ndbind { count addr xt -- }
    addr closure-size + { data-addr }
    xt data-addr !
    count data-addr cell+ !
    0 { index }
    begin index count < while
      index 1 lshift 2 + cells data-addr + 2!
      1 +to index
    repeat
    data-addr addr [: >r
      r@ cell+ @
      begin dup while
        1- dup 1 lshift 2 + cells r@ + 2@ rot
      repeat
      drop
      r> @ execute
    ;] bind
  ;
  
  \ Bind a reference closure
  : refbind ( x addr xt -- )
    [:
      over [: { data addr xt }
        addr refclosure-size cell - + lit, xt 1 or lit,
        undefer-lit
        [ armv6m-instr import ]
        tos r0 movs_,_
        tos 1 dp ldm    
        r0 bx_
        [ armv6m-instr unimport ]
        thumb-2? not if consts, then
        cell align,
        data ,
      ;] with-here
    ;] with-ram
  ;

  \ Bind a double-cell reference closure; this closure will push one address
  : drefbind ( d addr xt -- )
    [:
      over [: { D: data addr xt }
        addr drefclosure-size 2 cells - + lit, xt 1 or lit,
        undefer-lit
        [ armv6m-instr import ]
        tos r0 movs_,_
        tos 1 dp ldm    
        r0 bx_
        [ armv6m-instr unimport ]
        thumb-2? not if consts, then
        cell align,
        data 2,
      ;] with-here
    ;] with-ram
  ;

  \ Bind a multi-single-cell reference closure
  : nrefbind { count addr xt -- }
    addr closure-size + { data-addr }
    xt data-addr !
    count data-addr cell+ !
    0 { index }
    begin index count < while
      index 2 + cells data-addr + !
      1 +to index
    repeat
    data-addr addr [: >r
      r@ cell+ @
      begin dup while
        1- dup 2 + cells r@ + swap
      repeat
      drop
      r> @ execute
    ;] bind
  ;

  \ Bind a multi-double-cell reference closure
  : ndrefbind { count addr xt -- }
    addr closure-size + { data-addr }
    xt data-addr !
    count data-addr cell+ !
    0 { index }
    begin index count < while
      index 1 lshift 2 + cells data-addr + 2!
      1 +to index
    repeat
    data-addr addr [: >r
      r@ cell+ @
      begin dup while
        1- dup 1 lshift 2 + cells r@ + swap
      repeat
      drop
      r> @ execute
    ;] bind
  ;

  \ Create a single-cell closure in a set scope in the dictionary
  : with-closure ( x bound-xt scope-xt -- )
    closure-size [: swap >r >r
      r@ swap bind r> r> execute
    ;] with-aligned-allot
  ;

  \ Create a double-cell closure in a set scope in the dictionary
  : with-dclosure ( x1 x0 bound-xt scope-xt -- )
    dclosure-size [: swap >r >r
      r@ swap dbind r> r> execute
    ;] with-aligned-allot
  ;

  \ Create a multi-single-cell closure in a set scope in the dictionary
  : with-nclosure ( xn ... x0 count bound-xt scope-xt -- )
    2 pick nclosure-size [: swap >r >r
      r@ swap nbind r> r> execute
    ;] with-aligned-allot
  ;

  \ Create a multi-double-cell closure in a set scope in the dictionary
  : with-ndclosure ( dn ... d0 count bound-xt scope-xt -- )
    2 pick ndclosure-size [: swap >r >r
      r@ swap ndbind r> r> execute
    ;] with-aligned-allot
  ;

  \ Create a single-cell reference closure in a set scope in the dictionary
  : with-refclosure ( x bound-xt scope-xt -- )
    refclosure-size [: swap >r >r
      r@ swap refbind r> r> execute
    ;] with-aligned-allot
  ;

  \ Create a double-cell reference closure in a set scope in the dictionary;
  \ this closure will push one address
  : with-drefclosure ( d bound-xt scope-xt -- )
    drefclosure-size [: swap >r >r
      r@ swap drefbind r> r> execute
    ;] with-aligned-allot
  ;

  \ Create a multi-single-cell reference closure in a set scope in the
  \ dictionary; this closure will push one address per single-cell value
  : with-nrefclosure ( xn ... x0 count bound-xt scope-xt -- )
    2 pick nrefclosure-size [: swap >r >r
      r@ swap nrefbind r> r> execute
    ;] with-aligned-allot
  ;
  
  \ Create a multi-double-cell reference closure in a set scope in the
  \ dictionary; this closure will push one address per double-cell value
  : with-ndrefclosure ( dn ... d0 count bound-xt scope-xt -- )
    2 pick ndrefclosure-size [: swap >r >r
      r@ swap ndrefbind r> r> execute
    ;] with-aligned-allot
  ;

end-module

reboot
