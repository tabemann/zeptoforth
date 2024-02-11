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

begin-module julia

  \ Displayed X size
  80 constant width

  \ Displayed Y size
  40 constant height

  \ The color array
  : color
    c\"  .`'-~+^\":;Il!i><tfjrxnuvczXYUJCLQ0OZmwqpdbkhao*#MW&8%B@$ "
  ;

  \ The maximum number of iterations
  color c@ 1- constant max-iteration

  \ Initialize the test
  \ Julia test
  : draw { D: cx D: cy D: r -- }
    r 2,0 f* { D: 2r }
    r r f* { D: r2** }
    height 0 ?do
      cr
      width 0 ?do
        i s>f width s>f f/ 0,5 d- 2r f* { D: zx }
        height j - s>f height s>f f/ 0,5 d- 2r f* { D: zy }
        0 { iteration }
        begin
          zx 2dup f* zy 2dup f* d+ r2** d< iteration max-iteration < and
        while
          zx 2dup f* zy 2dup f* d- { D: xtemp }
          zx zy f* 2,0 f* cy d+ to zy
          xtemp cx d+ to zx
          1 +to iteration
        repeat
        color 1+ iteration + c@ emit
      loop
    loop
  ;

  \ Draw a julia set
  : test ( -- ) -0,4 0,6 1,0 julia::draw ;
  
end-module
