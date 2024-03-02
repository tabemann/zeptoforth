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

begin-module mandelbrot

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
  
  \ Mandelbrot test
  : draw { D: xa D: xb D: ya D: yb -- }
    xb xa d- { D: x-mult }
    yb ya d- { D: y-mult }
    height 0 ?do
      cr
      width 0 ?do
        i s>f width s>f f/ x-mult f* xa d+ { D: x0 }
        height j - s>f height s>f f/ y-mult f* ya d+ { D: y0 }
        0,0 0,0 { D: x D: y }
        0 { iteration }
        begin
          x 2dup f* y 2dup f* d+ 4,0 d<= iteration max-iteration < and
        while
          x 2dup f* y 2dup f* d- x0 d+ { D: xtemp }
          x y f* 2,0 f* y0 d+ to y
          xtemp to x
          1 +to iteration
        repeat
        color 1+ iteration + c@ emit
      loop
    loop
  ;

  \ Draw a mandelbrot set
  : test ( -- ) -2,00 0,47 -1,12 1,12 draw ;
  
end-module
