\ Copyright (c) 2020 Travis Bemann
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

\ Set up the wordlist
forth-wordlist internal-wordlist systick-wordlist 3 set-order
forth-wordlist set-current

\ Start line time
variable start-line-time

\ Last line time
variable last-line-time

\ Starting word count
variable start-word-count

\ Current word rate
variable current-word-rate

\ Maximum line time difference
2500 constant maximum-last-line-diff

\ Line interval
25 constant line-interval

\ Get word count for a dictionary
: dict-word-count ( dict -- )
  0 swap begin dup while swap 1+ swap next-word @ repeat drop
;

\ Get word count
: word-count ( -- u )
  ram-latest dict-word-count flash-latest dict-word-count +
;

\ Handle line
: handle-line ( -- )
  systick-counter
  dup last-line-time @ - maximum-last-line-diff <= if
    last-line-time !
    0 word-count start-word-count @ -
    0 last-line-time @ start-line-time @ - 10000,0 f/ f/ f.
  else
    dup last-line-time ! start-line-time ! word-count start-word-count !
  then
  do-prompt
;

\ Enable word speed
: enable-word-speed ( -- )
  systick-counter dup start-line-time ! last-line-time !
  word-count start-word-count !
  ['] handle-line prompt-hook !
;

\ Disable word speed
: disable-word-speed ( -- ) ['] do-prompt prompt-hook ! ;