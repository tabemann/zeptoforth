\ Copyright (c) 2023-2025 Travis Bemann
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

begin-module raytracing

  oo import
  pixmap8 import
  st7365p-8-common import
  float32 import
  picocalc-term import

  \ Columns
  320 constant my-cols

  \ Rows
  320 constant my-rows

  \ Three-element constant
  : 3constant ( a b c "name" -- )
    : rot lit, swap lit, lit, postpone ;
  ;

  \ Background color
  0.5e0 0e0 0e0 3constant background-color

  \ Shadow color
  0e0 0e0 0e0 3constant shadow-color
  
  \ Eye coordinate
  0e0 0e0 3.20e0 3constant eye-coord
  
  \ Light coordinate
  -50e0 value light-x
  50e0 value light-y
  25e0 value light-z
  : light-coord light-x light-y light-z ;
\  -50e0 50e0 0e0 3constant light-coord

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
  : vect+ { ax ay az bx by bz -- cx cy cz } ax bx v+ ay by v+ az bz v+ ;

  \ Subtract two vectors
  : vect- { ax ay az bx by bz -- cx cy cz } ax bx v- ay by v- az bz v- ;

  \ Multiply a vector by a scalar
  : vect* { ax ay az n -- bx by bz } ax n v* ay n v* az n v* ;
  
  \ Get the dot product of two vectors
  : dot { ax ay az bx by bz -- dot } ax bx v* ay by v* az bz v* v+ v+ ;

  \ Get the cross product of two vectors
  : cross { ax ay az bx by bz -- cx cy cz }
    ay bz v* az by v* v- az bx v* ax bz v* v- ax by v* ay bx v* v-
  ;
  
  \ Get the magnitude of a vector
  : mag { x y z -- magnitude } x x v* y y v* z z v* v+ v+ vsqrt ;

  \ Get the unit vector of a vector
  : unit { x y z -- ux uy uz }
    x y z mag { dist } x dist v/ y dist v/ z dist v/
  ;

  \ Get the distance between two points
  : dist ( ax ay az bx by bz -- distance ) vect- mag ;

  \ Get the minimum root of an ax^2 + bx + c equation
  : minroot { a b c -- x found? }
    a v0= if
      c vnegate b v/ true
    else
      b b v* 4e0 a v* c v* v- { disc }
      disc v0>= if
        disc vsqrt { disc' }
        2e0 a v* { 2a }
        b vnegate disc' v+ b vnegate disc' v- vmin 2a v/ true
      else
        0e0 false
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
      1e0 \ vx vy vz vx vy vz dot
      osx osy osz vx vy vz dot 2e0 v*
      osx osy osz osx osy osz dot abs self sphere-radius @ dup v* v-
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
      hx hy hz lx ly lz -1e0 vect* 0 find-entity nip nip not if
        nx ny nz lx ly lz dot 0e0 vmax 1e0 vmin { lambert }
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
      r a v* self mirror-sphere-r !
      g a v* self mirror-sphere-g !
      b a v* self mirror-sphere-b !
      a self mirror-sphere-a !
    ; define new
    
    \ Get point color
    :noname { hx hy hz vx vy vz self -- r g b }
      self sphere-x @ self sphere-y @ self sphere-z @ hx hy hz vect- unit
      { nx ny nz }
      vx vy vz nx ny nz nx ny nz vx vy vz dot vect* 2e0 vect* vect-
      unit { rx ry rz }
      hx hy hz rx ry rz self find-entity if { entity distance }
        rx ry rz distance vect* hx hy hz vect+ rx ry rz entity point-color
      else
        2drop background-color
      then
      { r g b }
      hx hy hz light-coord vect- unit { lx ly lz }
      hx hy hz lx ly lz -1e0 vect* 0 find-entity nip nip not if
        nx ny nz lx ly lz dot 0e0 vmax 1e0 min { lambert }
        self mirror-sphere-r @
        self mirror-sphere-g @
        self mirror-sphere-b @ lambert vect*
      else
        shadow-color
      then
      { mr mg mb }
      self mirror-sphere-a @ { a } 1e0 a v- { a' }
      r a' v* mr v+ g a' v* mg v+ b a' v* mb v+
    ; define point-color
    
  end-implement

  \ Plane class
  <entity> begin-class <plane>
    cell member plane-x
    cell member plane-y
    cell member plane-z
    cell member plane-nx
    cell member plane-ny
    cell member plane-nz
  end-class

  \ Implement plane class
  <plane> begin-implement

    \ Constructor
    :noname { x y z nx ny nz self -- }
      self <entity>->new
      nx ny nz unit to nz to ny to nx
      x self plane-x !
      y self plane-y !
      z self plane-z !
      nx self plane-nx !
      ny self plane-ny !
      nz self plane-nz !
    ; define new
    
    \ Is there a hit?
    :noname { sx sy sz vx vy vz self -- distance hit? }
      self plane-nx @ self plane-ny @ self plane-nz @ { nx ny nz }
      nx ny nz vx vy vz dot { denom }
      denom v0<> if
        self plane-x @ self plane-y @ self plane-z @ sx sy sz vect- nx ny nz dot
        denom v/ { n }
        n v0>= n 16384e0 v< and if n true else 0e0 false then
      else
        0 false
      then
    ; define point-hit?

  end-implement

  \ Tiled plane
  <plane> begin-class <tiled-plane>
    cell member plane-ax
    cell member plane-ay
    cell member plane-az
    cell member plane-bx
    cell member plane-by
    cell member plane-bz
    cell member plane-r0
    cell member plane-g0
    cell member plane-b0
    cell member plane-r1
    cell member plane-g1
    cell member plane-b1
    cell member plane-tile-size
  end-class

  \ Implement tiled plane
  <tiled-plane> begin-implement

    \ Constructor
    :noname { x y z nx ny nz ax ay az r0 g0 b0 r1 g1 b1 tile-size self -- }
      x y z nx ny nz self <plane>->new
      ax ay az unit to az to ay to ax
      ax self plane-ax !
      ay self plane-ay !
      az self plane-az !
      self plane-nx @ self plane-ny @ self plane-nz @ ax ay az cross
      self plane-bz ! self plane-by ! self plane-bx !
      r0 self plane-r0 !
      g0 self plane-g0 !
      b0 self plane-b0 !
      r1 self plane-r1 !
      g1 self plane-g1 !
      b1 self plane-b1 !
      tile-size self plane-tile-size !
    ; define new

    \ Get point color
    :noname { hx hy hz vx vy vz self -- r g b }
      light-coord { lx' ly' lz' }
      lx' hx v- dup v*
      ly' hy v- dup v* v+
      lz' hz v- dup v* v+ vsqrt 180e0 v>= if
        shadow-color exit
      then
      hx hy hz self plane-x @ self plane-y @ self plane-z @ vect- { cx cy cz }
      self plane-ax @ self plane-ay @ self plane-az @ cx cy cz dot { dx }
      self plane-bx @ self plane-by @ self plane-bz @ cx cy cz dot { dy }
      dx self plane-tile-size @ v/ v>f64 floor { dx' }
      dy self plane-tile-size @ v/ v>f64 floor { dy' }
      dx' 2 mod 0= dy' 2 mod 0= xor if
        self plane-r0 @ self plane-g0 @ self plane-b0 @
      else
        self plane-r1 @ self plane-g1 @ self plane-b1 @
      then
      { r g b }
      hx hy hz lx' ly' lz'  vect- unit { lx ly lz }
      hx hy hz lx ly lz -1e0 vect* 0 find-entity nip nip not if
        self plane-nx @ self plane-ny @ self plane-nz @
        lx ly lz dot 0e0 vmax 1e0 vmin { lambert }
        r g b lambert vect*
      else
        shadow-color
      then
    ; define point-color
    
  end-implement
  
  <colored-sphere> class-size buffer: my-sphere-0
  <colored-sphere> class-size buffer: my-sphere-1
  <colored-sphere> class-size buffer: my-sphere-2
  <colored-sphere> class-size buffer: my-sphere-3
  <mirror-sphere> class-size buffer: my-sphere-4
  <tiled-plane> class-size buffer: my-plane

  create my-entities
  my-sphere-0 ,
  my-sphere-1 ,
  my-sphere-2 ,
  my-sphere-3 ,
  my-sphere-4 ,
  my-plane ,
  6 constant entity-count
  
  \ Find the closest, if any, entity
  :noname { sx sy sz vx vy vz exclude -- entity distance hit? }
    0 0 false { found-entity distance hit? }
    entity-count 0 ?do
      my-entities i cells + @ { entity }
      entity exclude <> if
        sx sy sz vx vy vz entity point-hit? if { entity-distance }
          hit? not entity-distance distance v< or entity-distance v0> and if
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
    x [ my-cols 2 / ] literal - n>v 100e0 v/
    y negate [ my-rows 2 / ] literal + n>v 100e0 v/
  ;

  \ Color dithering table
  create color-table
  0.8e0 , 1e0 , 1.2e0 , 1e0 , 0.8e0 , 1e0 , 1.2e0 , 1e0 ,
  1e0 , 1.1e0 , 1e0 , 0.9e0 , 1e0 , 1.1e0 , 1e0 , 0.9e0 ,
  1.2e0 , 1e0 , 0.8e0 , 1e0 , 1.2e0 , 1e0 , 0.8e0 , 1e0 ,
  1e0 , 0.9e0 , 1e0 , 1.1e0 , 1e0 , 0.9e0 , 1e0 , 1.1e0 ,
  0.8e0 , 1e0 , 1.2e0 , 1e0 , 0.8e0 , 1e0 , 1.2e0 , 1e0 ,
  1e0 , 1.1e0 , 1e0 , 0.9e0 , 1e0 , 1.1e0 , 1e0 , 0.9e0 ,
  1.2e0 , 1e0 , 0.8e0 , 1e0 , 1.2e0 , 1e0 , 0.8e0 , 1e0 ,
  1e0 , 0.9e0 , 1e0 , 1.1e0 , 1e0 , 0.9e0 , 1e0 , 1.1e0 ,
  
  \ Convert a color
  : convert-color { r g b x y -- color }
    x 3 and y 3 and 8 * + cells color-table + @ { factor }
    r 255e0 v* factor v* vround-zero v>n 0 max 255 min
    g 255e0 v* factor v* vround-zero v>n 0 max 255 min
    b 255e0 v* factor v* vround-zero v>n 0 max 255 min rgb8
  ;
  
  \ Draw world
  : draw-world ( -- )
    page
    [: { display }
      display clear-pixmap
      display update-display
    ;] with-term-display
    1e0 2.5e0 2.5e0 -6.4e0 0e0 1e0 0e0 <colored-sphere> my-sphere-0 init-object
    1.5e0 0e0 -1e0 -4.8e0 1e0 0e0 0e0 <colored-sphere> my-sphere-1 init-object
    2e0 -2.5e0 3e0 -10e0 0e0 0e0 1e0 <colored-sphere> my-sphere-2 init-object
    0.75e0 -2e0 1e0 -3.6e0 0e0 1e0 1e0 <colored-sphere> my-sphere-3 init-object
    1.5e0 0e0 2.5e0 -7.5e0 1e0 0e0 1e0 0.25e0 <mirror-sphere> my-sphere-4 init-object
    0e0 -20e0 0e0 0e0 -1e0 0e0 1e0 0e0 0e0 0.75e0 0e0 0e0 0e0 0.75e0 0e0 10e0
    <tiled-plane> my-plane init-object
    my-rows 0 ?do
      i [: { y display }
        my-cols 0 ?do
          i y convert-coord 0e0 eye-coord vect- unit { vx vy vz }
          eye-coord vx vy vz send-ray i y convert-color
          i y display draw-pixel-const
        loop
        display update-display
      ;] with-term-display
    loop
    begin key? until
    key drop
  ;
  
end-module
