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
  
  : run-neopixel-test ( -- )
    0,0 { D: angle }
    begin key? not while
      angle cos 255,0 f* nip 0 max 255 min abs { r }
      angle pi 2,0 3,0 f/ f* d+ cos 255,0 f* nip 0 max 255 min abs { g }
      angle pi 2,0 3,0 f/ f* d- cos 255,0 f* nip 0 max 255 min abs { b }
      r g b 0 my-neopixel neopixel!
      angle pi 0,0625 f* d+ 2dup pi 2,0 f* d>= if pi 2,0 f* d- then to angle
      my-neopixel update-neopixel
      50 ms
    repeat
    key drop
  ;
  
end-module
