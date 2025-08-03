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

begin-module snake
  
  oo import
  picocalc-term import
  pixmap8 import
  st7365p-8-common import
  tinymt32 import
  
  tinymt32-size buffer: prng
  
  : init-prng ( -- )
    rng::random prng tinymt32-init
    prng tinymt32-prepare-example
  ;
  
  initializer init-prng
  
  : random { x -- u }
    x s>f prng tinymt32-generate-uint32 0 f* round-half-zero
    x min
  ;

  8 constant cell-width
  8 constant cell-height
  
  term-pixels-dim@
  cell-height / constant world-height
  cell-width / constant world-width
  
  world-width world-height * constant max-snake-len
  
  <object> begin-class <food>
  
    cell member food-count
    world-width world-height * 8 align 8 / cell align
    member food-bits
    
    method food-count@ ( food -- count )
    method create-food ( x y food -- )
    method eat-food ( x y food -- )
    method food-at? ( x y food -- at? )
    
  end-class
  
  <food> begin-implement
    
    :noname { self -- }
      self <object>->new
      self food-bits [ world-width world-height * 8 align 8 / ]
      literal 0 fill
      0 self food-count !
    ; define new
    
    :noname ( self -- count ) food-count @ ; define food-count@
    
    :noname { x y self -- }
      x y self food-at? not if
        1 self food-count +!
        y world-width * x + { index }
        index 7 and bit index 3 rshift self food-bits + cbis!
      then
    ; define create-food
    
    :noname { x y self -- }
      x y self food-at? if
        -1 self food-count +!
        y world-width * x + { index }
        index 7 and bit index 3 rshift self food-bits + cbic!
      then
    ; define eat-food
    
    :noname { x y self -- at? }
      y world-width * x + { index }
      index 7 and bit index 3 rshift self food-bits + cbit@
    ; define food-at?
    
  end-implement
  
  0 constant none
  1 constant up
  2 constant down
  3 constant left
  4 constant right
  
  : adjust-coord ( x y dir -- x' y' )
    case
      up of 1- endof
      down of 1+ endof
      left of swap 1- swap endof
      right of swap 1+ swap endof
    endcase
  ;
  
  : in-bounds? { x y -- in-bounds? }
    x 0>= y 0>= and x world-width < and y world-height < and
  ;
  
  <object> begin-class <snake>
  
    cell member snake-dir
    cell member snake-head-index
    cell member snake-tail-index
    max-snake-len cell align member snake-x
    max-snake-len cell align member snake-y
    
    method extend-snake ( dir snake -- alive? )
    method push-snake-head ( x y snake -- )
    method drop-snake-tail ( snake -- )
    method snake-head@ ( snake -- x y )
    method snake-at? ( x y snake -- at? )
    method snake-len@ ( snake -- len )
    
  end-class
  
  <snake> begin-implement
    
    :noname { x y self -- }
      self <object>->new
      none self snake-dir !
      0 self snake-head-index !
      0 self snake-tail-index !
      x self snake-x c!
      y self snake-y c!
    ; define new
    
    :noname { dir self -- alive? }
      dir none = if self snake-dir @ to dir then
      dir none <> if
        dir self snake-dir !
        self snake-head@ dir adjust-coord { x y }
        x y in-bounds? if
          x y self snake-at? not if
            x y self push-snake-head true
          else
            false
          then
        else
          false
        then
      else
        true
      then
    ; define extend-snake
 
    :noname { x y self -- }
      self snake-head-index @ 1+ max-snake-len umod
      dup { index } self snake-head-index !
      x self snake-x index + c!
      y self snake-y index + c!
    ; define push-snake-head
    
    :noname { self -- }
      self snake-tail-index @ { index }
      index self snake-head-index @ <> if
        index 1+ max-snake-len umod self snake-tail-index !
      then
    ; define drop-snake-tail
    
    :noname { self -- x y }
      self snake-head-index @ { index }
      self snake-x index + c@
      self snake-y index + c@
    ; define snake-head@
    
    :noname { x y self -- at? }
      self snake-tail-index @ { index }
      self snake-head-index @ { head-index }
      begin
        self snake-x index + c@ x =
        self snake-y index + c@ y = and if true exit then
        index head-index =
        index 1+ max-snake-len umod to index
      until
      false
    ; define snake-at?
    
    :noname { self -- len }
      self snake-head-index @ { head-index }
      self snake-tail-index @ { tail-index }
      head-index
      tail-index head-index > if max-snake-len + then
      tail-index - 1+
    ; define snake-len@
    
  end-implement
  
  4 constant init-food-count
  100 constant food-chance \ Actually the reciprocal
  4 constant min-snake-len
  0 255 0 rgb8 constant body-color
  255 255 0 rgb8 constant head-color
  255 0 0 rgb8 constant food-color
  
  <object> begin-class <world>
    
    <snake> class-size member the-snake
    <food> class-size member the-food
    
    method cycle-world ( dir world -- continue? )
    method draw-world ( world -- )
    method create-random-food ( world -- )
    
  end-class
  
  <world> begin-implement
  
    :noname { self -- }
      self <object>->new
      world-width 1- random world-height 1- random
      <snake> self the-snake init-object
      <food> self the-food init-object
      init-food-count 0 ?do self create-random-food loop
    ; define new
    
    :noname { dir self -- continue? }
      dir self the-snake extend-snake if
        self the-snake snake-head@ { x y }
        x y self the-food food-at? not if
          self the-snake snake-len@ min-snake-len > if
            self the-snake drop-snake-tail
          then
        else
          x y self the-food eat-food
        then
        food-chance random 0=
        self the-food food-count@ 0= or if
          self create-random-food
        then
        true
      else
        false
      then
    ; define cycle-world
    
    :noname ( self -- )
      [: { self display -- }
        display clear-pixmap
        world-height 0 ?do
          world-width 0 ?do
            0 { color }
            i j self the-snake snake-at? if
              self the-snake snake-head@ j = swap i = and if
                head-color
              else
                body-color
              then
              to color
            else
              i j self the-food food-at? if
                food-color to color
              then
            then
            color ?dup if
              i cell-width * j cell-height *
              cell-width cell-height
              display draw-rect-const
            then
          loop
        loop
        display update-display
      ;] with-term-display
    ; define draw-world
    
    :noname { self -- }
      begin
        world-width 1- random { x }
        world-height 1- random { y }
        x y self the-snake snake-at? not
        x y self the-food food-at? not and dup if
          x y self the-food create-food
        then
      until
    ; define create-random-food
    
  end-implement
  
  $1B constant escape
  
  : empty-keys ( -- ) begin key? while key drop repeat ;
  
  : handle-key ( -- dir exit? )
    key? if
      key case
        escape of
          key? if
            key [char] [ = if
              key? if
                key case
                  [char] A of up endof
                  [char] B of down endof
                  [char] C of right endof
                  [char] D of left endof
                  empty-keys none swap
                endcase
              then
            else
              empty-keys none
            then
          then
          false
        endof
        [char] q of none true endof
        none false rot
      endcase
    else
      none false
    then
  ;
  
  1875 constant snake-delay-ticks
  
  : play-snake ( -- )
    <world> [: { the-world }
      page
      [: dup clear-pixmap update-display ;] with-term-display
      the-world draw-world
      begin
        handle-key not if
          systick::systick-counter { start-systick }
          the-world cycle-world { alive? }
          the-world draw-world
          start-systick snake-delay-ticks
          task::current-task task::delay
          alive? not
        else
          drop true
        then
      until
      ." *** GAME OVER ***" cr
      empty-keys
    ;] with-object
  ;

end-module
