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

begin-module lindenmayer

  turtle import

  begin-module lindenmayer-internal
    
    \ Token buffer
    256 buffer: token-buf
    
    \ The current level
    variable level
    
    \ The current color
    2variable color

    \ Saved HERE
    variable saved-here
    
    \ Convert 0,0 ... 1,0 rgb to 0 ... 255 rgb
    : rgb { D: r D: g D: b -- r g b }
      r 255,0 f* round-zero g 255,0 f* round-zero b 255,0 f* round-zero
    ;
    
    \ Convert a fully-saturated 0,0 ... 1,0 color to 0 ... 255 rgb
    : convert-color { D: f -- r g b }
      f 6,0 f* to f
      f 1,0 d< if 1,0 f 0,0 rgb exit then
      f 2,0 d< if 2,0 f d- 1,0 0,0 rgb exit then
      f 3,0 d< if 0,0 1,0 f 2,0 d- rgb exit then
      f 4,0 d< if 0,0 4,0 f d- 1,0 rgb exit then
      f 5,0 d< if f 4,0 d- 0,0 1,0 rgb exit then
      1,0 0,0 6,0 f d- rgb
    ;
    
    \ Initialize variables
    : init-lindenmayer ( -- )
      token-buf 256 0 fill 0 level ! 0,0 color 2! 0 saved-here !
    ;
    
    initializer init-lindenmayer

    \ Saved x/y and heading frame
    begin-structure frame-size
      2field: frame-x
      2field: frame-y
      field: frame-heading
    end-structure
    
    \ Get a token and put it in the token buffer as a counted string
    : store-token ( "name" -- )
      token dup averts x-token-expected
      255 min dup token-buf c!
      token-buf 1+ swap move
    ;

    \ Drop keys
    : drop-keys ( -- ) begin key? while key drop repeat ;

  end-module> import

  \ Step being defined exception
  : x-step-being-defined ( -- ) ." step being defined" cr ;

  \ Step not being defined exception
  : x-step-not-being-defined ( -- ) ." step not being defined" cr ;
  
  \ Declare a turn
  : turn ( degrees "name" -- )
    { angle } : angle postpone literal postpone left postpone ;
  ;
  
  \ Declare a step
  : step ( "name" -- ) defer ;

  \ Save the current x/y and heading
  : [[ ( -- )
    ram-here { frame }
    frame-size ram-allot
    fgetxy frame frame-y 2! frame frame-x 2!
    getheading frame frame-heading !
  ;

  \ Restore the previously saved x/y and heading
  : ]] ( -- )
    penup
    ram-here frame-size - { frame }
    frame frame-x 2@ frame frame-y 2@ fsetxy
    frame frame-heading @ setheading
    [ frame-size negate ] literal ram-allot
    pendown
  ;

  begin-module step-internal

    \ End a step
    : ;step ( xt branch... -- )
      [immediate] [compile-only]
      token-buf count nip averts x-step-not-being-defined
      postpone level postpone @ postpone 1+ postpone level postpone !
      postpone then
      postpone ; token-buf count find dup averts x-unknown-word >xt defer!
      token-buf 256 0 fill
      step-internal unimport
    ;

  end-module
  
  \ Define a forward step
  : :forward-step ( pixels "name" -- xt branch... )
    { pixels }
    token-buf count nip 0= averts x-step-being-defined
    store-token
    :noname
    postpone key? postpone if postpone exit postpone then
    postpone level postpone @ postpone 0= postpone if
    pixels postpone literal postpone forward postpone else
    postpone level postpone @ postpone 1- postpone level postpone !
    step-internal import
  ;

  \ Define a fixed-color forward step
  : :rgb-forward-step ( r g b pixels "name" -- xt branch... )
    { r g b pixels }
    token-buf count nip 0= averts x-step-being-defined
    store-token
    :noname
    postpone key? postpone if postpone exit postpone then
    postpone level postpone @ postpone 0= postpone if
    r postpone literal g postpone literal b postpone literal
    postpone setpencolor
    pixels postpone literal postpone forward postpone else
    postpone level postpone @ postpone 1- postpone level postpone !
    step-internal import
  ;

  \ Define a rotating-color forward step
  : :rcolor-forward-step ( color-incr pixels "name" -- xt branch... )
    { D: color-incr pixels }
    token-buf count nip 0= averts x-step-being-defined
    store-token
    :noname
    postpone key? postpone if postpone exit postpone then
    postpone level postpone @ postpone 0= postpone if
    postpone color postpone 2@ postpone convert-color postpone setpencolor
    postpone color postpone 2@ color-incr swap postpone literal postpone literal
    postpone d+ 1,0 swap postpone literal postpone literal postpone fmod
    postpone color postpone 2!
    pixels postpone literal postpone forward postpone else
    postpone level postpone @ postpone 1- postpone level postpone !
    step-internal import
  ;

  \ Define a null step
  : :step ( "name" -- xt branch... )
    token-buf count nip 0= averts x-step-being-defined
    store-token
    :noname
    postpone key? postpone if postpone exit postpone then
    postpone level postpone @ postpone if
    postpone level postpone @ postpone 1- postpone level postpone !
    step-internal import
  ;

  begin-module axiom-internal

    \ End an axiom
    : ;axiom ( -- ) 
      [immediate] [compile-only]
      postpone showturtle
      postpone saved-here postpone @ postpone ram-here!
      postpone drop-keys
      postpone ;
    ;
    
  end-module

  \ Define an axiom
  : :axiom ( level D: init-color r g b D: x D: y heading "name" -- )
    { level' D: init-color r g b D: x D: y heading }
    :
    postpone ram-here cell postpone literal postpone ram-align,
    postpone saved-here postpone !
    init-color swap postpone literal postpone literal postpone color postpone 2!
    r postpone literal g postpone literal b postpone literal
    postpone setpencolor
    postpone page postpone clear postpone penup
    x swap postpone literal postpone literal
    y swap postpone literal postpone literal postpone fsetxy
    heading postpone literal postpone setheading
    postpone pendown postpone hideturtle
    level' postpone literal postpone level postpone !
    axiom-internal import
  ;
  
end-module
