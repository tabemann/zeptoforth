\ Copyright (c) 2026 Travis Bemann
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

begin-module flood

  closure import

  begin-module flood-internal

    \ Write frame
    : frame! ( x1 x2 y dy -- )
      ram-here 8 - { addr } addr 6 + h! addr 4 + h! addr 2 + h! addr h!
    ;

    \ Read a signed 16-bit value
    : sh@ ( addr -- n ) h@ 16 lshift 16 arshift ;
    
    \ Read frame
    : frame@ ( -- x1 x2 y dy )
      ram-here 8 - { addr } addr sh@ addr 2 + sh@ addr 4 + sh@ addr 6 + sh@
    ;

    \ Push frame
    : >frame ( x1 x2 y dy -- ) 8 ram-allot frame! ;

    \ Drop frame
    : drop-frame ( -- ) -8 ram-allot ;

    \ Pop frame
    : frame> ( -- x1 x2 y dy ) frame@ drop-frame ;

    \ Clamp a value to the range [-32768, 32767]
    : sh-clamp ( n -- nh ) -32768 max 32767 min ;

    \ Force two values to be in lower-higher order
    : lo-hi { n1 n2 -- n-lo n-hi } n1 n2 < if n1 n2 else n2 n1 then ;

    \ Debug test
    : =debug= type ." : " depth . ;
    
  end-module> import
  
  \ Draw a flood fill
  \
  \ Note that all coordinates must be in the range [-32768, 32767]
  \
  \ inside-xt is an operation to test a pixel, with the following signature:
  \ ( x y -- inside? )
  \ 
  \ set-xt is an operation to set a pixel, with the following signature:
  \ ( x y -- )
  : draw-flood
    ( x-min y-min cols rows x-seed y-seed max-depth inside-xt set-xt -- )
    2 pick 2 < if 2drop drop 2drop 2drop 2drop exit then
    ram-here { saved-here }
    [: { x-min y-min cols rows x y max-depth inside-xt set-xt }
      cols 0= rows 0= or if exit then
      x-min cols + 1- { x-max }
      y-min rows + 1- { y-max }
      x-min sh-clamp to x-min y-min sh-clamp to y-min
      x-max sh-clamp to x-max y-max sh-clamp to y-max
      x x-min < x x-max > or y y-min < or y y-max > or if exit then
      x y inside-xt execute not if exit then
      1 { depth }
      x x y 1 >frame
      y 1- y-min >= if x x y 1- -1 >frame 1 +to depth then
      begin depth 0> while
        -1 +to depth
        frame> { dy } to y { x1 x2 } x1 to x
        x y inside-xt execute if
          begin x x-min > if x 1- y inside-xt execute else false then while
            -1 +to x x y set-xt execute
          repeat
          x x1 < depth max-depth < and if
            y dy - { y' }
            y' y-min >= y' y-max <= and if
              x x1 1- y' dy negate >frame 1 +to depth
            then
          then
        then
        begin x1 x2 <= while
          begin x1 x-max <= if x1 y inside-xt execute else false then while
            x1 y set-xt execute 1 +to x1
          repeat
          x1 x > depth max-depth < and if
            y dy + { y' }
            y' y-min >= y' y-max <= and if
              x x1 1- y' dy >frame 1 +to depth
            then
          then
          x1 1- x2 > depth max-depth < and if
            x2 1+ x-max min { x2' }
            x1 1- x-min max { x1' }
            y dy - { y' }
            y' y-min >= y' y-max <= and if
              x2' x1' y' dy negate >frame 1 +to depth
            then
          then
          1 +to x1
          begin x1 x2 <= x1 x-max < and x1 y inside-xt execute not and while
            1 +to x1
          repeat
          x1 to x
        repeat
      repeat
    ;] try
    saved-here ram-here!
    ?raise
  ;

  defined? bitmap [if]

    bitmap import

    \ Draw a bitmap flood fill
    : draw-bitmap-flood { set? x y max-depth dest -- }
      0 0 dest dim@ x y max-depth set? dest
      dest bitmap-cols @ dest bitmap-internal::bitmap-buf @
      set? if
        [: ( x y cols buf )
          >r >r dup 7 and bit swap 3 rshift r> * r> + rot + bit@ 0=
        ;]
      else
        [: ( x y cols buf )
          >r >r dup 7 and bit swap 3 rshift r> * r> + rot + bit@
        ;]
      then
      [: -rot { set? dest }
        dest bitmap-cols @ dest bitmap-internal::bitmap-buf @
        set? if
          [: ( x y cols buf )
            >r >r dup 7 and bit swap 3 rshift r> * r> + rot + bis!
          ;]
        else
          [: ( x y cols buf )
            >r >r dup 7 and bit swap 3 rshift r> * r> + rot + bic!
          ;]
        then
        ['] draw-flood with-dclosure
      ;] with-dclosure
      dest bitmap-internal::set-dirty
    ;

    bitmap unimport
    
  [then]

  defined? pixmap8 [if]

    pixmap8 import

    \ Draw a pixmap8 flood fill
    : draw-pixmap8-flood { color x y max-depth dest -- }
      0 0 dest dim@ x y max-depth color dest
      dest pixmap-cols @ dest pixmap8-internal::pixmap-buf @ color
      3 [: ( x y cols buf color )
        >r >r * r> + + c@ r> <>
      ;]
      [: -rot { color dest }
        dest pixmap-cols @ dest pixmap8-internal::pixmap-buf @ color
        3 [: ( x y cols buf color )
          >r >r * r> + + r> swap c!
        ;]
        ['] draw-flood with-nclosure
      ;] with-nclosure
      dest pixmap8-internal::set-dirty
    ;

    \ Draw a pixmap8 flood fill inside a color
    : draw-pixmap8-flood-inside { inside-color set-color x y max-depth dest -- }
      0 0 dest dim@ x y max-depth set-color dest
      dest pixmap-cols @ dest pixmap8-internal::pixmap-buf @ inside-color
      3 [: ( x y cols buf color )
        >r >r * r> + + c@ r> =
      ;]
      [: -rot { set-color dest }
        dest pixmap-cols @ dest pixmap8-internal::pixmap-buf @ set-color
        3 [: ( x y cols buf color )
          >r >r * r> + + r> swap c!
        ;]
        ['] draw-flood with-nclosure
      ;] with-nclosure
      dest pixmap8-internal::set-dirty
    ;

    pixmap8 unimport
    
  [then]

  defined? pixmap16 [if]

    pixmap16 import

    \ Draw a pixmap16 flood fill
    : draw-pixmap16-flood { color x y max-depth dest -- }
      0 0 dest dim@ x y max-depth color dest
      dest pixmap-cols @ 1 lshift dest pixmap16-internal::pixmap-buf @ color
      3 [: ( x y cols2* buf color )
        >r >r * r> + + h@ r> <>
      ;]
      [: -rot { color dest }
        dest pixmap-cols @ 1 lshift dest pixmap16-internal::pixmap-buf @ color
        3 [: ( x y cols2* buf color )
          >r >r * r> + + r> swap h!
        ;]
        ['] draw-flood with-nclosure
      ;] with-nclosure
      dest pixmap16-internal::set-dirty
    ;

    \ Draw a pixmap16 flood fill inside a color
    : draw-pixmap16-flood-inside { inside-color set-color x y max-depth dest -- }
      0 0 dest dim@ x y max-depth set-color dest
      dest pixmap-cols @ 1 lshift dest pixmap16-internal::pixmap-buf @
      inside-color
      3 [: ( x y cols2* buf color )
        >r >r * r> + + h@ r> =
      ;]
      [: -rot { set-color dest }
        dest pixmap-cols @ 1 lshift dest pixmap16-internal::pixmap-buf @
        set-color
        3 [: ( x y cols2* buf color )
          >r >r * r> + + r> swap h!
        ;]
        ['] draw-flood with-nclosure
      ;] with-nclosure
      dest pixmap16-internal::set-dirty
    ;

    pixmap16 unimport
    
  [then]

end-module
