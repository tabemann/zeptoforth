\ Copyright (c) 2020-2021 Travis Bemann
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

begin-module forth-wordlist

  import internal-module
  import systick-module

  \ Start line time
  variable start-line-time

  \ Last line time
  variable last-line-time

  \ Line count
  variable line-count

  \ Maximum line time difference
  2500 constant maximum-last-line-diff

  \ Line interval
  25 constant line-interval

  \ Handle line
  : handle-line ( -- )
    systick-counter
    dup last-line-time @ - maximum-last-line-diff <= if
      last-line-time !
      1 line-count +!
      line-count @ line-interval = if
	space ." lines/s:"
	0 line-interval 0 last-line-time @ start-line-time @ - 10000,0 f/ f/ f.
	last-line-time @ start-line-time ! 0 line-count !
      then
    else
      dup last-line-time ! start-line-time ! 0 line-count !
    then
    do-prompt
  ;

  \ Enable input speed
  : enable-input-speed ( -- )
    systick-counter dup start-line-time ! last-line-time ! 0 line-count !
    ['] handle-line prompt-hook !
  ;

  \ Disable input speed
  : disable-input-speed ( -- ) ['] do-prompt prompt-hook ! ;

end-module
