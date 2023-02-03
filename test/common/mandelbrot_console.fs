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
  : mandelbrot-test ( -- )
    height 0 ?do
      cr
      width 0 ?do
        i s>f width s>f f/ x-multiplier f* x-offset d+ { x0-lo x0-hi }
        j s>f height s>f f/ y-multiplier f* y-offset d+ { y0-lo y0-hi }
        0,0 0,0 { x-lo x-hi y-lo y-hi }
        0 { iteration }
        begin
          x-lo x-hi 2dup f* y-lo y-hi 2dup f* d+ 4,0 d<=
          iteration max-iteration < and
        while
          x-lo x-hi 2dup f* y-lo y-hi 2dup f* d- x0-lo x0-hi d+
          { xtemp-lo xtemp-hi }
          x-lo x-hi y-lo y-hi f* 2,0 f* y0-lo y0-hi d+ to y-hi to y-lo
          xtemp-lo xtemp-hi to x-hi to x-lo
          1 +to iteration
        repeat
        color iteration + c@ emit
      loop
    loop
  ;
  
end-module
