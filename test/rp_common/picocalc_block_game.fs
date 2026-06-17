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

\ The controls are:
\ 
\ Up: Move up
\ Down: Move down
\ Right: Move right
\ Left: Move left
\ Space: Select
\ S: Take a screenshot
\ Q: Give up in shame

begin-module block-game
  
  picocalc-term import
  picocalc-keys import
  picocalc-screenshot import
  pixmap8 import
  font import
  st7365p-8-common import
  tinymt32 import
  
  $B4 constant LEFT_ARROW
  $B5 constant UP_ARROW
  $B6 constant DOWN_ARROW
  $B7 constant RIGHT_ARROW

  0 value score
  
  255 255 255 rgb8 constant info-color
  127 127 127 rgb8 constant info-shadow-color
  16 constant info-x
  16 constant info-y
  
  tinymt32-size buffer: my-tinymt32
  
  : init-random ( -- )
    rng::random my-tinymt32 tinymt32-init
    my-tinymt32 tinymt32-prepare-example
  ;
  
  initializer init-random
  
  : random { val -- val' }
    val s>f my-tinymt32 tinymt32-generate-uint32 0 f*
    round-zero val 1- min
  ;
  
  create select-colors
  127 127 127 rgb8 c,
  255 0 0 rgb8 c,
  255 255 0 rgb8 c,
  0 255 0 rgb8 c,
  0 255 255 rgb8 c,
  0 0 255 rgb8 c,
  255 0 255 rgb8 c,
  cell align,
  
  create unselect-colors
  0 0 0 rgb8 c,
  191 0 0 rgb8 c,
  191 191 0 rgb8 c,
  0 191 0 rgb8 c,
  0 191 191 rgb8 c,
  0 0 191 rgb8 c,
  191 0 191 rgb8 c,
  
  6 constant colors
  16 constant game-width
  16 constant game-height
  8 constant game-init-height
  game-width game-height * cell align buffer: game-blocks
  0 value game-falling-block
  0 value game-falling-block-x
  0,0 2value game-falling-block-y
  1560 constant block-fall-delay
  1,5 game-height s>f f* 2constant block-fall-speed
  0 value last-block-fall-tick
  1250 constant select-move-delay
  0 value last-select-move-tick
  7500 constant select-delay
  0 value last-select-tick
  0 value last-tick
  game-width 2 / value select-x
  game-height 1- value select-y
  
  : game-block-addr { x y -- addr }
    game-width y * x + game-blocks +
  ;
  
  : game-block@ ( x y -- color )
    game-block-addr c@
  ;
  
  : game-block! ( color x y -- )
    game-block-addr c!
  ;
  
  : random-color ( -- color ) colors random 1+ ;
  
  : init-game ( -- )
    game-blocks game-width game-height * 0 fill
    game-height game-init-height ?do
      game-width 0 ?do
        random-color i j game-block!
      loop
    loop
    systick::systick-counter { current-tick }
    0 to game-falling-block
    0 to game-falling-block-x
    0,0 to game-falling-block-y
    current-tick to last-block-fall-tick
    current-tick select-move-delay - to last-select-move-tick
    current-tick select-delay - to last-select-tick
    current-tick to last-tick
    game-width 2 / to select-x
    game-height 1- to select-y
  ;
  
  : string> { addr bytes buf-addr -- buf-addr' }
    addr buf-addr bytes move bytes buf-addr +
  ;
  
  : n> ( n buf-addr -- buf-addr' )
    base @ { saved-base }
    [: 10 base ! swap format-integer + ;] try
    saved-base base ! ?raise
  ;

  : draw-info ( display -- )
    256 [: { display buf }
      buf { buf' }
      s" Score: " buf' string> to buf'
      score buf' n> to buf'
      info-shadow-color buf buf' buf - info-x 1+ info-y 1+
      display term-font@ draw-string-to-pixmap8
      info-color buf buf' buf - info-x info-y display
      term-font@ draw-string-to-pixmap8
    ;] with-allot
  ;
  
  term-pixels-dim@
  game-height / constant block-height
  game-width / constant block-width
  
  : draw-block { color x D: y display -- }
    color x block-width * y block-height s>f f* round-zero
    block-width block-height display draw-rect-const
  ;
  
  : draw-static-block { color x y display -- }
    x select-x = y select-y = and if
      color select-colors + c@
    else
      color unselect-colors + c@
    then
    x y s>f display draw-block
  ;
  
  : draw-game ( -- )
    [: { display }
      display clear-pixmap
      game-height 0 ?do
        game-width 0 ?do
          i j game-block@ i j display draw-static-block
        loop
      loop
      game-falling-block if
        game-falling-block unselect-colors + c@
        game-falling-block-x
        game-falling-block-y display draw-block
      then
      display draw-info
      display update-display
    ;] with-term-display
  ;
  
  : empty-keys ( -- ) begin key? while key drop repeat ;

  : game-over ( -- )
    [: dup clear-pixmap update-display ;] with-term-display
    page
    ." *** GAME OVER ***" cr cr ." Your score: " score . cr
    false raw-keys-enabled!
    clear-keymap
    1000 ms
    empty-keys
  ;
  
  : handle-screenshot ( -- )
    [:
      screenshot-fs@ { fs }
      fs if
        screenshot-path@ fs ['] take-screenshot try-and-display-error 0<> if
          drop 2drop
        then
      then
    ;] console::with-serial-error-output
    systick::systick-counter to last-tick
  ;
  
  : find-highest-block { x -- }
    0 game-height 1- ?do
      x i game-block@ 0= if i 1+ unloop exit then
    -1 +loop
    0
  ;
        
  
  : update-game { current-tick -- game-over? }
    game-falling-block if
      game-falling-block-y
      current-tick last-tick - s>f 10000,0 f/
      block-fall-speed f* d+ { D: y' }
      game-falling-block-x find-highest-block { y }   
      y' round-zero 1+ y >= if
        game-falling-block
        game-falling-block-x y 1- game-block!
        0 to game-falling-block
        current-tick to last-block-fall-tick
        false
      else
        y' to game-falling-block-y
        false
      then
    else
      last-block-fall-tick block-fall-delay +
      current-tick <= if
        random-color to game-falling-block
        game-width random to game-falling-block-x
        0,0 to game-falling-block-y
        game-falling-block-x 0 game-block@ 0<>
        dup not if
          game-falling-block-x 1 game-block@ if
            game-falling-block
            game-falling-block-x 0 game-block!
            0 to game-falling-block
          then
        then
      else
        false
      then
    then
    current-tick to last-tick
  ;
  
  : handle-select-move { current-tick -- }
    last-select-move-tick select-move-delay +
    current-tick - 0<= if
      false { update }
      LEFT_ARROW keymap-pressed@ if
        select-x 0> if
          -1 +to select-x true to update
        then
      then
      RIGHT_ARROW keymap-pressed@ if
        select-x game-width 1- < if
          1 +to select-x true to update
        then
      then
      UP_ARROW keymap-pressed@ if
        select-y 0> if
          -1 +to select-y true to update
        then
      then
      DOWN_ARROW keymap-pressed@ if
        select-y game-height 1- < if
          1 +to select-y true to update
        then
      then
      LEFT_ARROW reset-key
      RIGHT_ARROW reset-key
      UP_ARROW reset-key
      DOWN_ARROW reset-key
      update if
        current-tick to last-select-move-tick
      else
        current-tick select-move-delay -
        to last-select-move-tick
      then
    then
  ;
  
  : 2c! { x y addr -- } x addr c! y addr 1+ c! ;
  
  : 2c@ { addr -- x y } addr c@ addr 1+ c@ ;
  
  : push ( x y -- ) ram-here 2 ram-allot 2c! ;
  
  : pop ( -- x y ) ram-here 2 - 2c@ -2 ram-allot ;
  
  : select-blocks ( color x y -- )
    ram-here { addr }
    addr [: { color x y addr }
      0 { count }
      x y push
      begin ram-here addr <> while
        pop { x y }
        x y game-block@ color = if
          1 +to count
          0 x y game-block!
          x 0> if x 1- y push then
          x game-width 1- < if x 1+ y push then
          y 0> if x y 1- push then
          y game-height 1- < if x y 1+ push then
        then
      repeat
      count dup * +to score
    ;] try
    addr ram-here!
    ?raise
  ;
  
  : compact-blocks ( -- )
    game-width 0 ?do
      1 game-height 1- ?do
        j { x }
        x i game-block@ 0= if
          i 1- { y' }
          begin
            y' 0>= if
              x y' game-block@ { color }
              color if
                color x i game-block! 0 x y' game-block! true
              else
                -1 +to y' false
              then
            else
              true
            then
          until
        then 
      -1 +loop
    loop
  ;
  
  : handle-select { current-tick -- }
    last-select-tick select-delay + current-tick - 0<= if
      false { update }
      bl keymap-released@ if
        select-x select-y game-block@ { color }
        color if
          color select-x select-y select-blocks
          compact-blocks
        then
        true to update
      then
      bl reset-key
      update if
        current-tick to last-select-tick
      else
        current-tick select-delay - to last-select-tick
      then
    then
  ;
  
  : play-game ( -- )
    page
    true raw-keys-enabled!
    [: dup clear-pixmap update-display ;] with-term-display
    clear-keymap
    init-game
    begin
      update-keymap
      systick::systick-counter { current-tick }    
      current-tick handle-select-move
      current-tick handle-select
      current-tick update-game
      draw-game
      [char] s keymap-released@ if handle-screenshot then
      [char] q keymap-released@ or
      [char] s reset-key
      [char] q reset-key
    until
    game-over
  ;
  
end-module