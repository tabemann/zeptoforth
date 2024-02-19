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

begin-module fixed32

  armv6m import

  begin-module fixed32-internal

    \ Convert an unsigned 32.32 number to an unsigned 16.16 number
    : 32.32>16.16 ( D: x -- x' )
      [inlined]
      code[
      16 r6 r0 lsls_,_,#_
      r6 1 r7 ldm
      16 r6 r6 lsrs_,_,#_
      r0 r6 orrs_,_
      ]code
    ;

    \ Convert an unsigned 16.16 number to an unsigned 32.32 number
    : 16.16>32.32 ( x -- D: x' )
      [inlined]
      code[
      r6 r0 movs_,_
      16 r6 r6 lsls_,_,#_
      4 r7 subs_,#_
      0 r7 r6 str_,[_,#_]
      16 r0 r6 lsrs_,_,#_
      ]code
    ;
    
  end-module> import

  \ Multiply two S15.16 fixed-point numbers
  : f32* { x y -- z }
    y 0< if
      x 0< if
        x negate y negate um* 32.32>16.16
      else
        x y negate um* 32.32>16.16 negate
      then
    else
      x 0< if
        x negate y um* 32.32>16.16 negate
      else
        x y um* 32.32>16.16
      then
    then
  ;

  \ Devide an S15.16 fixed-point number by another
  : f32/ { x y -- x }
    y 0< if
      x 0< if
        x negate 16.16>32.32
        y negate 16.16>32.32
        ud/mod 2nip 32.32>16.16
      else
        x 16.16>32.32
        y negate 16.16>32.32
        ud/mod 2nip 32.32>16.16 negate
      then
    else
      x 0< if
        x negate 16.16>32.32
        y 16.16>32.32
        ud/mod 2nip 32.32>16.16 negate
      else
        x 16.16>32.32
        y 16.16>32.32
        ud/mod 2nip 32.32>16.16
      then
    then
  ;
  
  \ Divide an S15.16 fixed-point number by another
  : f32/ ( x y -- z )
    swap s>d 16 2lshift rot s>d d/ drop
  ;

  \ Convert an S15.16 fixed-point number to a 16-bit integer
  : f32>s ( x -- y ) 16 arshift ;

  \ Convert a 16-bit integer to an S15.16 fixed-point number
  : s>f32 ( x -- y ) 16 lshift ;

  \ Convert an S31.32 fixed-point number to an S15.16 fixed-point number
  : f64>f32 ( D: x -- y ) 16 2arshift d>s ;

  \ Convert an S15.16 fixed-point number to an S31.32 fixed-point number
  : f32>f64 ( x -- D: y ) s>d 16 2lshift ;
  
  \ Calculate the modulus of two S15.16 fixed-point numbers
  : f32mod { x y -- z }
    x x y f32/ f32>s dup 0< if 1+ then s>f32 y f32* -
  ;

  \ Get the ceiling of an S15.16 fixed-point number
  : f32ceil ( f32 -- n ) dup $FFFF and if 16 arshift 1+ else 16 arshift then ;

  \ Get the floor of an S15.16 fixed-point number
  : f32floor ( f32 -- n ) 16 arshift ;

  \ Round an S15.16 fixed-point number to the nearest integer with half
  \ rounding up
  : f32round-half-up ( f32 -- n )
    dup $FFFF and 15 rshift if 16 arshift 1+ else 16 arshift then
  ;

  \ Round an S15.16 fixed-point number to the nearest integer with half
  \ rounding down
  : f32round-half-down ( f32 -- n )
    dup $FFFF and dup [ 15 bit ] literal <> if
      15 rshift if 16 arshift 1+ else 16 arshift then
    else
      drop 16 arshift
    then
  ;

  \ Round a S15.16 fixed-point number to the nearest integer with half rounding
  \ towards zero
  : f32round-half-zero ( f32 -- n )
    dup 16 arshift 0>= if f32round-half-down else f32round-half-up then
  ;
  
  \ Round a S15.16 fixed-point number to the nearest integer with half rounding
  \ away from zero
  : f32round-half-away-zero ( f32 -- n )
    dup 16 arshift 0>= if f32round-half-up else f32round-half-down then
  ;
  
  \ Round a S15.16 fixed-point number to the nearest integer with half rounding
  \ towards even
  : f32round-half-even ( f32 -- n )
    dup 16 arshift 1 and if f32round-half-up else f32round-half-down then
  ;
  
  \ Round a S15.16 fixed-point number to the nearest integer with half rounding
  \ towards even
  : f32round-half-odd ( f32 -- n )
    dup 16 arshift 1 and if f32round-half-down else f32round-half-up then
  ;
  
  \ Round a S15.16 fixed-point number towards zero
  : f32round-zero ( f32 -- n )
    dup 16 arshift 0>= if
      16 arshift
    else
      dup $FFFF and if 16 arshift 1+ else 16 arshift then
    then
  ;
  
  \ Round a S15.16 fixed-point number away from zero
  : f32round-away-zero ( f32 -- n )
    dup $FFFF and if 16 arshift dup 0>= if 1+ then else 16 arshift then
  ;

  \ Pi as a S15.16 fixed-point number
  205887 constant f32pi

  continue-module fixed32-internal
  
    \ Get whether two S15.16 fixed-point numbers are close enough for a square root
    : f32sqrt-close-enough ( x y -- flag )
      2dup - rot abs rot abs max f32/ abs 2 <
    ;
    
    \ Get a better guess for a square root of an S15.16 fixed-point number
    : f32sqrt-better-guess ( x y -- z )
      dup rot rot f32/ + 2 /
    ;

  end-module

  \ Get the square root of an S15.16 fixed-point number
  : f32sqrt ( x -- y )
    64 >r
    dup 2 /
    begin
      r@ 0> if
        2dup f32/ over f32sqrt-close-enough if
          rdrop nip true
        else
          r> 1- >r 2dup f32sqrt-better-guess nip false
        then
      else
        rdrop nip true
      then
    until
  ;
  
  \ Exponentiate an S15.16 fixed-point number by an integer
  : f32i** ( f32 exponent -- f32' )
    dup 0> if
      [ 1 s>f32 ] literal begin
        over 1 and if
          2 pick f32*
        then
        swap 1 rshift dup 0= if
          drop nip true
        else
          rot dup f32* swap rot false
        then
      until
    else
      0= if
        drop [ 1 s>f32 ] literal
      else
        drop 0
      then
    then
  ;

  \ Get the (e^x)-1 of an S15.16 fixed-point number
  : f32expm1 ( f32 -- f32' )
    >r 0 [ 1 s>f32 ] literal 1 begin
      swap r@ f32* over / dup abs 2 < >r rot over +
      swap rot 1 + r>
    until
    rdrop 2drop
  ;
  
  \ Get the e^x of an S15.16 fixed-point number
  : f32exp ( f32 -- f32' ) f32expm1 [ 1 s>f32 ] literal + ;
  
  \ Get the ln(x+1) of an S15.16 fixed-point number
  : f32lnp1 ( f32 -- f32' )
    [ 1 s>f32 ] literal + >r 0 begin
      dup f32exp
      dup r@ swap -
      r@ rot +
      f32/
      2 *
      over +
      dup rot -
      abs 2 <
    until
    rdrop
  ;
  
  \ Get the ln(x) of an S15.16 fixed-point number
  : f32ln ( f32 -- f32' ) [ 1 s>f32 ] literal - f32lnp1 ;
  
  \ Get the sine of an S15.16 fixed-point number
  : f32sin ( f32 -- f32' )
    [ f32pi 2 * ] literal f32mod
    dup dup >r 1 begin
      swap r@ f32* r@ f32*
      over 2 * /
      over 2 *
      1 + /
      dup abs 2 < >r
      rot
      over
      3 pick 1 and if - else + then
      swap
      rot
      1 +
      r>
    until
    rdrop 2drop
  ;
  
  \ Get the cosine of an S15.16 fixed-point number
  : f32cos ( f32 -- f32' ) [ f32pi 2 / ] literal swap - f32sin ;
  
  \ Get the tangent of an S15.16 fixed-point number
  : f32tan ( f32 -- f32' ) dup f32sin swap f32cos f32/ ;
  
  \ Get the arctangent of an S15.16 fixed-point number
  : f32atan ( f32 -- f32' )
    [ 1 s>f32 ] literal 1 40 do
      over i * 2 f32i** swap f32/ i 2 * 1 - s>f32 +
    -1 +loop
    f32/
  ;
  
  \ Get the angle of an x and an y S15.16 fixed-point numbers
  : f32atan2 ( f32x f32y -- f32angle )
    dup 0> if
      f32/ f32atan
    else
      2dup 0< swap 0>= and if
        f32/ f32atan f32pi +
      else
        2dup 0< swap 0< and if
          f32/ f32atan f32pi -
        else
          2dup 0= swap 0> and if
            drop [ f32pi 2 / ] literal
          else
            2dup 0= swap 0< and if
              drop [ f32pi -2 / ] literal
            else
              drop 0
            then
          then
        then
      then
    then
  ;
  
  \ Get the arcsine of an S15.16 fixed-point number
  : f32asin ( f32 -- f32' )
    dup 2 f32i** [ 1 s>f32 ] literal <  if
      [ 1 s>f32 ] literal over 2 f32i** - f32sqrt f32/ f32atan
    else
      dup 0> if
        drop [ f32pi 2 / ] literal
      else
        drop [ f32pi -2 / ] literal
      then
    then
  ;
  
  \ Get the arccosine of an S15.16 fixed-point number
  : f32acos ( f32 -- f32' )
    f32asin negate [ f32pi 2 / ] literal +
  ;
  
  \ Exponentiate two S15.16 fixed-point numbers
  : f32** ( f32b f32x -- f32b^f32x )
    over 0= >r dup 0= r> and triggers x-domain-error
    dup $FFFF and 0= if
      dup f32>s 0>= if
        f32>s f32i**
      else
        f32>s negate f32i** [ 1 s>f32 ] literal swap f32/
      then
    else
      over 0>= averts x-domain-error swap f32ln f32* f32exp
    then
  ;

  \ Get the hyperbolic sine of an S15.16 fixed-point number
  : f32sinh ( f32 -- f32' )
    f32expm1 dup dup [ 1 s>f32 ] literal + f32/ + 2 /
  ;
  
  \ Get the hyperbolic cosine of an S15.16 fixed-point number
  : f32cosh ( f32 -- f32' )
    f32expm1 dup dup [ 1 s>f32 ] literal + f32/ - 2 / [ 1 s>f32 ] literal +
  ;

  \ Get the hyperbolic tangent of an S15.16 fixed-point number
  : f32tanh ( f32 -- f32' )
    dup f32sinh swap f32cosh f32/
  ;

  \ Get the hyperbolic arcsine of an S15.16 fixed-point number
  : f32asinh ( f32 -- f32' )
    dup 2 f32i** [ 1 s>f32 ] literal + f32sqrt + f32ln
  ;

  \ Get the hyperbolic arccosine of an S15.16 fixed-point number
  : f32acosh ( f32 -- f32' )
    dup 2 f32i** [ 1 s>f32 ] literal - f32sqrt + f32ln
  ;

  \ Get the hyperbolic arctangent of an S15.16 fixed-point number
  : f32atanh ( f32 -- f32' )
    dup [ 1 s>f32 ] literal + swap negate [ 1 s>f32 ] literal + f32/ f32ln
    2 /
  ;

  continue-module fixed32-internal

    \ Handle the portion of a s15.16 fixed-point single-cell numeric literal to
    \ the right of the decimal point
    : handle-f32-fraction { c-addr bytes int base -- f32 -1 | 0 }
      bytes internal::max-fraction-chars base 2 - cells + @ min { max-places }
      0 0 { places accum }
      begin bytes 0> places max-places < and while
        c-addr c@ dup [char] _ <> if
          base parse-digit if
            accum base * swap + to accum
            1 +to places
          else
            drop 0 exit
          then
        else
          drop
        then
        1 +to c-addr
        -1 +to bytes
      repeat
      0 accum 0 base places fi** f/ int s>f d+ f64>f32 true
    ;

    \ Handle an unsigned fixed32 number
    : handle-f32 ( c-addr bytes base -- f32 -1 | 0 )
      >r 2dup [char] ; internal::char-count 1 = if
        over c@ dup [char] . <> over [char] , <> and swap [char] ; <> and if
          2dup + 1- c@ dup [char] . <> over [char] , <> and swap [char] ; <>
          and if
            0 begin over 0<> while
              2 pick c@ r@ parse-digit if
                swap r@ * + rot 1+ rot 1- rot
              else
                drop
                2 pick c@ dup [char] _ = if
                  drop rot 1+ rot 1- rot
                else
                  dup [char] . = over [char] , = or swap [char] ; = or if
                    rot 1+ rot 1- rot r> handle-f32-fraction exit
                  else
                    drop 2drop rdrop false exit
                  then
                then
              then
            repeat
          else
            rdrop 2drop false
          then
        else
          rdrop 2drop false
        then
      else
        rdrop 2drop false
      then
    ;
    
  end-module

  continue-module internal
    
    \ Handle parsing numbers, extending it to S15.16 fixed-point numbers
    : do-handle-number { addr bytes -- flag }
      addr bytes do-handle-number not if
        addr bytes
        parse-base >r
        dup 0<> if
          over c@ [char] - = if
            1- swap 1+ swap
            dup 0<> if
              r> handle-f32
            else
              rdrop 2drop false
            then
            dup if
              state @ if
                swap negate lit,
              else
                swap negate swap
              then
            then
          else
            r> handle-f32
            dup if
              state @ if
                swap lit,
              then
            then
          then
        else
          rdrop 2drop false
        then
      else
        true
      then
    ;
    
  end-module

  \ Parse a 32-bit fixed-point number
  : parse-f32 ( c-addr bytes -- f32 success? )
    2dup [char] . internal::char-count >r
    2dup [char] , internal::char-count >r
    2dup [char] ; internal::char-count r> r> + + 2 u< if
      parse-base >r
      dup 0<> if
        over c@ [char] - = if
          1- swap 1+ swap
          dup 0<> if r> handle-f32 else rdrop 2drop false then
          if negate true else 0 false then
        else
          r> handle-f32 if true else 0 false then
        then
      else
        rdrop 2drop 0 false
      then
    else
      2drop 0 false
    then
  ;

  continue-module fixed32-internal

    \ Fraction size lookup table
    create fraction-size-table
    16 , 10 , 8 , 7 , 7 , 6 , 6 , 6 , 5 , 5 , 5 , 5 , 5 , 4 , 4 ,
    4 , 4 , 4 , 4 , 4 , 4 , 4 , 4 , 4 , 4 , 4 , 3 , 3 , 3 , 3 , 3 ,
    3 , 3 , 3 , 3 ,
    
    \ Get current fraction size
    : fraction-size ( -- u ) base @ 2 - cells fraction-size-table + @ ;

    \ Format digits to the right of the decimal point
    : format-f32-fraction ( u c-addr bytes -- c-addr bytes )
      >r >r 16 lshift fraction-size swap r> r>
      2dup swap >r >r + 0 >r >r dup 0<> if
        begin
          r> r> dup swap >r swap >r 2 pick <> swap dup 0<> rot and if
            base @ um* dup 10 < if
              [char] 0 +
            else
              10 - [char] A +
            then
            r@ c! r> 1+ r> 1+ >r >r false
          else
            true
          then
        until
        drop rdrop r> r> + r> swap
      else
        drop [char] 0 r@ c! rdrop r> r> + 1+ r> swap
      then
      rot drop
    ;

    \ Format digits to the right of the decimal point truncated to a given
    \ number of places
    : format-f32-fraction-truncate ( places u c-addr bytes -- c-addr bytes )
      2swap 16 lshift swap fraction-size min swap 2swap
      2dup swap >r >r + 0 >r >r dup 0<> if
        begin
          r> r> dup swap >r swap >r 2 pick <> swap dup 0<> rot and if
            base @ um* dup 10 < if
              [char] 0 +
            else
              10 - [char] A +
            then
            r@ c! r> 1+ r> 1+ >r >r false
          else
            true
          then
        until
        drop rdrop r> r> + r> swap
      else
        drop [char] 0 r@ c! rdrop r> r> + 1+ r> swap
      then
      rot drop
    ;

    \ Add a decimal point
    : add-decimal ( c-addr bytes -- c-addr bytes )
      2dup + [char] ; swap c! 1+
    ;
    
  end-module

  \ Format an S15.16 number
  : format-f32 ( c-addr f32 -- c-addr bytes )
    dup 0< if
      negate dup 16 arshift 0 <# #s -1 sign #> add-decimal format-f32-fraction
    else
      dup 16 arshift 0 <# #s #> add-decimal format-f32-fraction
    then
    dup >r rot dup >r swap move r> r>
  ;

  \ Format a truncated S15,16 number
  : format-f32-truncate ( c-addr f32 places -- c-addr bytes )
    swap
    over 0> if
      dup 0< if
        negate dup 16 arshift 0 <# #s -1 sign #> add-decimal
        format-f32-fraction-truncate
      else
        dup 16 arshift 0 <# #s #> add-decimal format-f32-fraction-truncate
      then
    else
      -rot drop swap
      dup 0< if
        16 arshift <# #s -1 sign #>
      else
        16 arshift <# #s #>
      then
    then
    dup >r rot dup >r swap move r> r>
  ;
  
  \ Type an s15.16 fixed-point number without a following space
  : (f32.) ( f32 -- )
    ram-here swap format-f32 dup >r dup ram-allot type r> negate ram-allot
  ;

  \ Type a truncated s15.16 fixed-point number without a following space
  : (f32.n) ( f32 places -- )
    >r ram-here swap r> format-f32-truncate dup >r dup ram-allot type r>
    negate ram-allot
  ;

  \ Type an s15.16 fixed-point number with a following space
  : f32. ( f32 -- ) (f32.) space ;

  \ Type a truncated s15.16 fixed-point number with a following space
  : f32.n ( f32 places -- ) (f32.n) space ;
  
end-module

: init-f32 ( -- )
  ['] internal::do-handle-number handle-number-hook !
;

initializer init-f32