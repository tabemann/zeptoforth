begin-module snowflake
  oo import
  bitmap import
  ssd1306 import
  font import
  simple-font import
  ssd1306-print import

  : >s { x y -- } x c, y c, [char] * c, ;
  : >l { x y -- } x c, y c, [char] _ c, ;
  : >f { x y -- } x c, y c, [char] / c, ;
  : >b { x y -- } x c, y c, $5C c, ;
  : >d { x y -- } x y >b x 1+ y >f ;
  : >u { x y -- } x y >f x 1+ y >b ;

  : draw-snowflake ( -- )
    here { start }
    3 0 >s 10 0 >s 3 7 >s 10 7 >s
    3 1 >l 10 1 >l
    6 2 do 4 i >l 9 i >l loop
    0 3 >l 2 3 >l 11 3 >l 13 3 >l
    4 1 >d 8 1 >d 5 2 >d 7 2 >d 5 3 >d 7 3 >d
    5 4 >u 7 4 >u 5 5 >u 7 5 >u 4 6 >u 8 6 >u
    1 3 >b 3 3 >b 10 3 >f 12 3 >f
    1 4 >f 3 4 >f 10 4 >b 12 4 >b $FF c,
    start begin dup c@ $FF <> while
      dup c@ over 1+ c@ goto-ssd1306 dup 2 + c@ emit-ssd1306 3 +
    repeat drop
    start ram-here!
    0 0 goto-ssd1306
  ;
end-module
