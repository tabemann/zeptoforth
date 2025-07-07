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

begin-module term-test

  : csi ( -- ) .\" \x1B[" ;
  : dec. ( n -- )
    base @ { saved-base }
    [: 10 base ! (.) ;] try
    saved-base base !
    ?raise
  ;

  : cursor-up ( u -- ) csi 0 max dup 1 > if dec. else drop then ." A" ;
  : cursor-down ( u -- ) csi 0 max dup 1 > if dec. else drop then ." B" ;
  : cursor-forward ( u -- ) csi 0 max dup 1 > if dec. else drop then ." C" ;
  : cursor-back ( u -- ) csi 0 max dup 1 > if dec. else drop then ." D" ;
  : cursor-next-line ( u -- ) csi 0 max dup 1 > if dec. else drop then ." E" ;
  : cursor-prev-line ( u -- ) csi 0 max dup 1 > if dec. else drop then ." F" ;
  : horiz-abs ( u -- ) csi 0 max dup 1 > if dec. else drop then ." G" ;
  : erase-down ( -- ) csi ." J" ;
  : erase-up ( -- ) csi ." 1J" ;
  : erase-screen ( -- ) csi ." 2J" ;
  : erase-right ( -- ) csi ." K" ;
  : erase-left ( -- ) csi ." 1K" ;
  : erase-line ( -- ) csi ." 2K ";
  : scroll-up ( u -- ) csi 0 max dup 1 > if dec. else drop then ." S" ;
  : scroll-down ( u -- ) csi 0 max dup 1 > if dec. else drop then ." T" ;

end-module