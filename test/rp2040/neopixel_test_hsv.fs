\ Copyright (c) 2023 Travis Bemann
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
\ SOFTWARE

begin-module neopixel-test

  neopixel import
  pin import
  pio import
  pio-registers import
  
  \ The number of Neopixels
  1 constant neopixel-count
  
  neopixel-count neopixel-size buffer: my-neopixel
  
  \ Neopixel power pin
  11 constant neopixel-power
  
  \ Neopixel data pin
  12 constant neopixel-data
  
  \ Neopixel PIO
  PIO0 constant neopixel-pio
  
  \ Neopixel state machine
  0 constant neopixel-sm

  neopixel-power output-pin
  high neopixel-power pin!
  
  neopixel-sm neopixel-pio neopixel-count neopixel-data my-neopixel init-neopixel
  
  \ Red color weight
  1,0 2constant red-weight
  
  \ Green color weight
  1,0 2,0 f/ 2constant green-weight
  
  \ Blue color weight
  3,0 5,0 f/ 2constant blue-weight
  
  : correct-color { r g b -- r' g' b' }
    r s>f red-weight f* f>s g s>f green-weight f* f>s b s>f blue-weight f* f>s 
  ;

  : correct-brightness { r g b -- r' g' b' }
    255,0 r s>f red-weight f/ 2 fi**
    g s>f green-weight f/ 2 fi** d+
    b s>f blue-weight f/ 2 fi** d+ sqrt f/ { D: adjust }
    r s>f adjust f* f>s
    g s>f adjust f* f>s
    b s>f adjust f* f>s
  ;
  
  : run-neopixel-test ( -- )
    0,0 { D: angle }
    begin key? not while
      angle 2,0 pi f* f/ 6,0 f* { D: h' }
      1,0 h' 2,0 fmod 1,0 d- dabs d- 255,0 f* f>s { x }
      0,0 h' d<= h' 1,0 d< and if
        255 x 0
      then
      1,0 h' d<= h' 2,0 d< and if
        x 255 0
      then
      2,0 h' d<= h' 3,0 d< and if
        0 255 x
      then
      3,0 h' d<= h' 4,0 d< and if
        0 x 255
      then
      4,0 h' d<= h' 5,0 d< and if
        x 0 255
      then
      5,0 h' d<= h' 6,0 d< and if
        255 0 x
      then
      correct-color correct-brightness 0 my-neopixel neopixel!
      angle pi 0,0625 f* d+ 2dup pi 2,0 f* d>= if pi 2,0 f* d- then to angle
      my-neopixel update-neopixel
      50 ms
    repeat
    key drop
  ;
  
end-module
