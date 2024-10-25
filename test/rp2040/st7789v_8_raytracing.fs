\ Copyright (c) 2023-2024 Travis Bemann
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

begin-module st7789v-raytracing

  oo import
  pixmap8 import
  pixmap8-utils import
  st7789v-8 import
  fixed32 import
  oo import

  \ Columns
  320 constant my-cols

  \ Rows
  240 constant my-rows

  \ Pio device
  pio::PIO0 constant my-pio

  \ State machine
  0 constant my-sm

  \ Pins
  14 constant lcd-d0
  13 constant lcd-rd-sck
  12 constant lcd-wr-sck
  11 constant lcd-dc
  10 constant lcd-cs
  2 constant lcd-bl

  \ Buffer
  my-cols my-rows pixmap8-buf-size buffer: my-buffer

  \ Display
  <st7789v-8> class-size buffer: my-display

  \ Initialize the test:
  : init-test
    lcd-d0 lcd-wr-sck lcd-rd-sck lcd-dc lcd-cs lcd-bl
    my-buffer false my-cols my-rows my-sm my-pio
    <st7789v-8> my-display init-object
    my-display clear-pixmap
    my-display update-display
  ;

  initializer init-test

  \ Three-element constant
  : 3constant ( a b c "name" -- )
    : rot lit, swap lit, lit, postpone ;
  ;

  \ Background color
  0;5 0;0 0;0 3constant background-color

  \ Shadow color
  0;0 0;0 0;0 3constant shadow-color
  
  \ Eye coordinate
  0;0 0;0 3;20 3constant eye-coord
  
  \ Light coordinate
  -50;0 value light-x
  50;0 value light-y
  25;0 value light-z
  : light-coord light-x light-y light-z ;
\  -50;0 50;0 0;0 3constant light-coord

  \ Find an entity along a line
  defer find-entity ( sx sy sz vx vy vz exclude -- entity distance hit? )
  
  \ Entity class
  <object> begin-class <entity>

    \ Is there a hit?
    method point-hit? ( sx sy sz vx vy vz self -- distance hit? )

    \ Get point color
    method point-color ( hx hy hz vx vy vz self -- r g b )
    
  end-class

  \ Implement entity class
  <entity> begin-implement
  end-implement

  \ Sphere class
  <entity> begin-class <sphere>
    cell member sphere-radius
    cell member sphere-x
    cell member sphere-y
    cell member sphere-z
  end-class

  \ Add two vectors
  : vect+ { ax ay az bx by bz -- cx cy cz } ax bx + ay by + az bz + ;

  \ Subtract two vectors
  : vect- { ax ay az bx by bz -- cx cy cz } ax bx - ay by - az bz - ;

  \ Multiply a vector by a scalar
  : vect* { ax ay az n -- bx by bz } ax n f32* ay n f32* az n f32* ;
  
  \ Get the dot product of two vectors
  : dot { ax ay az bx by bz -- dot } ax bx f32* ay by f32* az bz f32* + + ;

  \ Get the cross product of two vectors
  : cross { ax ay az bx by bz -- cx cy cz }
    ay bz f32* az by f32* - az bx f32* ax bz f32* - ax by f32* ay bx f32* -
  ;
  
  \ Get the magnitude of a vector
  : mag { x y z -- magnitude } x x f32* y y f32* z z f32* + + f32sqrt ;

  \ Get the unit vector of a vector
  : unit { x y z -- ux uy uz }
    x y z mag { dist } x dist f32/ y dist f32/ z dist f32/
  ;

  \ Get the distance between two points
  : dist ( ax ay az bx by bz -- distance ) vect- mag ;

  \ Get the minimum root of an ax^2 + bx + c equation
  : minroot { a b c -- x found? }
    a 0= if
      c negate b f32/ true
    else
      b b f32* 4;0 a f32* c f32* - { disc }
      disc 0>= if
        disc f32sqrt { disc' }
        2;0 a f32* { 2a }
        b negate disc' + b negate disc' - min 2a f32/ true
      else
        0 false
      then
    then
  ;
  
  \ Implement sphere class
  <sphere> begin-implement

    \ Constructor
    :noname { radius x y z self -- }
      self <entity>->new
      radius self sphere-radius !
      x self sphere-x !
      y self sphere-y !
      z self sphere-z !
    ; define new

    \ Is there a hit?
    :noname { sx sy sz vx vy vz self -- distance hit? }
      sx sy sz self sphere-x @ self sphere-y @ self sphere-z @ vect-
      { osx osy osz }
      1;0 \ vx vy vz vx vy vz dot
      osx osy osz vx vy vz dot 2;0 f32*
      osx osy osz osx osy osz dot abs self sphere-radius @ dup f32* -
      minroot
    ; define point-hit?

  end-implement

  \ The colored sphere class
  <sphere> begin-class <colored-sphere>
    cell member sphere-r
    cell member sphere-g
    cell member sphere-b
  end-class

  \ Implement the colored sphere class
  <colored-sphere> begin-implement

    \ Constructor
    :noname { radius x y z r g b self -- }
      radius x y z self <sphere>->new
      r self sphere-r !
      g self sphere-g !
      b self sphere-b !
    ; define new

    \ Get point color
    :noname { hx hy hz vx vy vz self -- r g b }
      self sphere-x @ self sphere-y @ self sphere-z @ hx hy hz vect- unit
      { nx ny nz }
      hx hy hz light-coord vect- unit { lx ly lz }
      hx hy hz lx ly lz -1;0 vect* 0 find-entity nip nip not if
        nx ny nz lx ly lz dot 0;0 max 1;0 min { lambert }
        self sphere-r @ self sphere-g @ self sphere-b @ lambert vect*
      else
        shadow-color
      then
    ; define point-color
    
  end-implement

  \ The mirror sphere class
  <sphere> begin-class <mirror-sphere>
    cell member mirror-sphere-r
    cell member mirror-sphere-g
    cell member mirror-sphere-b
    cell member mirror-sphere-a
  end-class

  \ Implement the mirror sphere class
  <mirror-sphere> begin-implement
    
    \ Constructor
    :noname { radius x y z r g b a self -- }
      radius x y z self <sphere>->new
      r a f32* self mirror-sphere-r !
      g a f32* self mirror-sphere-g !
      b a f32* self mirror-sphere-b !
      a self mirror-sphere-a !
    ; define new
    
    \ Get point color
    :noname { hx hy hz vx vy vz self -- r g b }
      self sphere-x @ self sphere-y @ self sphere-z @ hx hy hz vect- unit
      { nx ny nz }
      vx vy vz nx ny nz nx ny nz vx vy vz dot vect* 2;0 vect* vect-
      unit { rx ry rz }
      hx hy hz rx ry rz self find-entity if { entity distance }
        rx ry rz distance vect* hx hy hz vect+ rx ry rz entity point-color
      else
        2drop background-color
      then
      { r g b }
      hx hy hz light-coord vect- unit { lx ly lz }
      hx hy hz lx ly lz -1;0 vect* 0 find-entity nip nip not if
        nx ny nz lx ly lz dot 0;0 max 1;0 min { lambert }
        self mirror-sphere-r @
        self mirror-sphere-g @
        self mirror-sphere-b @ lambert vect*
      else
        shadow-color
      then
      { mr mg mb }
      self mirror-sphere-a @ { a } 1;0 a - { a' }
      r a' f32* mr + g a' f32* mg + b a' f32* mb +
    ; define point-color
    
  end-implement
  
  <colored-sphere> class-size buffer: my-sphere-0
  <colored-sphere> class-size buffer: my-sphere-1
  <colored-sphere> class-size buffer: my-sphere-2
  <colored-sphere> class-size buffer: my-sphere-3
  <mirror-sphere> class-size buffer: my-sphere-4

  create my-entities
  my-sphere-0 ,
  my-sphere-1 ,
  my-sphere-2 ,
  my-sphere-3 ,
  my-sphere-4 ,
  5 constant entity-count
  
  \ Find the closest, if any, entity
  :noname { sx sy sz vx vy vz exclude -- entity distance hit? }
    0 0 false { found-entity distance hit? }
    entity-count 0 ?do
      my-entities i cells + @ { entity }
      entity exclude <> if
        sx sy sz vx vy vz entity point-hit? if { entity-distance }
          hit? not entity-distance distance < or entity-distance 0> and if
            entity to found-entity
            entity-distance to distance
            true to hit?
          then
        else
          drop
        then
      then
    loop
    found-entity distance hit?
  ; is find-entity
  
  \ Send a ray
  : send-ray { sx sy sz vx vy vz -- r g b }
    sx sy sz vx vy vz 0 find-entity if { entity distance }
      vx vy vz distance vect* sx sy sz vect+ vx vy vz entity point-color
    else
      drop drop background-color
    then
  ;

  \ Convert a coordinate
  : convert-coord { x y -- x' y' }
    x [ my-cols 2 / ] literal - s>f32 100;0 f32/
    y negate [ my-rows 2 / ] literal + s>f32 100;0 f32/
  ;

  \ Color dithering table
  create color-table
  0;9 , 0;95 , 1;0 , 1;05 , 1;1 , 1;05 , 1;0 , 0;95 ,

  \ Convert a color
  : convert-color { r g b x y -- color }
    x 2 and y 2 and + cells color-table + @ { factor }
    r 255;0 f32* factor f32* f32round-zero 0 max 255 min
    g 255;0 f32* factor f32* f32round-zero 0 max 255 min
    b 255;0 f32* factor f32* f32round-zero 0 max 255 min rgb8
  ;
  
  \ Draw world
  : draw-world ( -- )
    1;0 2;5 2;5 -6;4 0;0 1;0 0;0 <colored-sphere> my-sphere-0 init-object
    1;5 0;0 -1;0 -4;8 1;0 0;0 0;0 <colored-sphere> my-sphere-1 init-object
    2;0 -2;5 3;0 -10;0 0;0 0;0 1;0 <colored-sphere> my-sphere-2 init-object
    0;75 -2;0 1;0 -3;6 0;0 1;0 1;0 <colored-sphere> my-sphere-3 init-object
    1;5 0;0 2;5 -7;5 1;0 0;0 1;0 0;25 <mirror-sphere> my-sphere-4 init-object
    my-rows 0 ?do
      my-cols 0 ?do
        i j convert-coord 0;0 eye-coord vect- unit { vx vy vz }
        eye-coord vx vy vz send-ray i j convert-color
        i j my-display draw-pixel-const
      loop
      my-display update-display
    loop
  ;
  
end-module
