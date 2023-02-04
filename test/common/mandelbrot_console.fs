begin-module mandelbrot

  \ Displayed X size
  80 constant width

  \ Displayed Y size
  40 constant height

  \ X scale offset
  -2,00 2constant x-offset

  \ Y scale offset
  -1,12 2constant y-offset

  \ X scale multiplier
  0,47 -2,00 d- 2constant x-multiplier

  \ Y scale multiplier
  1,12 -1,12 d- 2constant y-multiplier

  \ The color array
  : color
    c\"  .`'-~+^\":;Il!i><tfjrxnuvczXYUJCLQ0OZmwqpdbkhao*#MW&8%B@$ "
  ;

  \ The maximum number of iterations
  color c@ constant max-iteration
  
  \ Mandelbrot test
  : draw { D: xa D: xb D: ya D: yb -- }
    xb xa d- { D: x-mult }
    yb ya d- { D: y-mult }
    height 0 ?do
      cr
      width 0 ?do
        i s>f width s>f f/ x-mult f* xa d+ { D: x0 }
        j s>f height s>f f/ y-mult f* ya d+ { D: y0 }
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
        color iteration + c@ emit
      loop
    loop
  ;

  \ Draw a mandelbot set
  : test ( -- ) -2,00 0,47 -1,12 1,12 draw ;
  
end-module
