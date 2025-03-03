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

begin-module turtle-random-tree

  turtle import
  tinymt32 import

  tinymt32-size buffer: rng
  2variable angle-vary
  2variable size-vary

  : vary-random { n D: vary -- n' }
    n s>f { D: nd }
    rng tinymt32-generate-uint32 { random }
    random s>d 32 2lshift 33 2arshift vary f* nd f* nd d+ round-zero
  ;

  defer do-tree ( size level -- )
  :noname { level }
    level 0> if
      dup 1 > if
        size-vary 2@ vary-random { size' }
        60 angle-vary 2@ vary-random { angle0' }
        60 angle-vary 2@ vary-random { angle1' }
        size' forward
        angle0' left
        size' 2 * 3 / level 1- do-tree
        angle0' angle1' + right
        size' 2 * 3 / level 1- do-tree
        angle1' left
        penup size' negate forward pendown
      else
        dup forward penup negate forward pendown
      then
    else
      drop
    then
  ; is do-tree

  : random-tree { size D: angle-vary' D: size-vary' level seed -- }
    seed rng tinymt32-init
    angle-vary' angle-vary 2!
    size-vary' size-vary 2!
    rng tinymt32-prepare-example
    size level do-tree
  ;

end-module
