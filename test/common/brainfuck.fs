\ Copyright (c) 2022-2023 Travis Bemann
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

begin-module brainfuck

  internal import

  begin-module brainfuck-internal

    \ Compute the sum of multiple data increments and decrements
    : find-data-change ( initial -- final )
      begin
	source >parse @ > if
	  >parse @ + c@
	  case
	    [char] + of 1+ false 1 >parse +! endof
	    [char] - of 1- false 1 >parse +! endof
	    [char] [ of true endof
	    [char] ] of true endof
	    [char] > of true endof
	    [char] < of true endof
	    [char] . of true endof
	    [char] , of true endof
	    [char] ; of true endof
	    false 1 >parse +! swap
	  endcase
	else
	  drop prompt-hook @ ?execute refill false
	then
      until
    ;

    \ Compute the sum of multiple address increments and decrements
    : find-addr-change ( initial -- final )
      begin
	source >parse @ > if
	  >parse @ + c@
	  case
	    [char] > of 1+ false 1 >parse +! endof
	    [char] < of 1- false 1 >parse +! endof
	    [char] [ of true endof
	    [char] ] of true endof
	    [char] + of true endof
	    [char] - of true endof
	    [char] . of true endof
	    [char] , of true endof
	    [char] ; of true endof
	    false 1 >parse +! swap
	  endcase
	else
	  drop prompt-hook @ ?execute refill false
	then
      until
    ;
    
  end-module> import

  \ Begin a word written in brainfuck; note that the word generated has the
  \ signature ( addr -- addr' ) and operates in terms of bytes.
  : :bf ( "name" "code" -- )
    token
    dup 0= triggers x-token-expected
    start-compile
    begin
      source >parse @ > if
	>parse @ + c@ 1 >parse +!
	case
	  [char] [ of
	    postpone begin postpone dup postpone c@ postpone while false endof
	  [char] ] of postpone repeat false endof
	  [char] + of
	    1 find-data-change lit, postpone over postpone c+! false endof
	  [char] - of
	    -1 find-data-change lit, postpone over postpone c+! false endof
	  [char] > of
	    1 find-addr-change lit, postpone + false endof
	  [char] < of
	    -1 find-addr-change lit, postpone + false endof
	  [char] . of postpone dup postpone c@ postpone emit false endof
	  [char] , of postpone key postpone over postpone c! false endof
	  [char] ; of true endof
	  false swap
	endcase
      else
	drop prompt-hook @ ?execute refill false
      then
    until
    visible
    end-compile,
  ;

  \ Begin a word written in brainfuck; note that the word generated has the
  \ signature ( addr -- addr' ) and operates in terms of cells.
  : :bf-cell ( "name" "code" -- )
    token
    dup 0= triggers x-token-expected
    start-compile
    begin
      source >parse @ > if
	>parse @ + c@ 1 >parse +!
	case
	  [char] [ of
	    postpone begin postpone dup postpone @ postpone while false endof
	  [char] ] of postpone repeat false endof
	  [char] + of
	    1 find-data-change lit, postpone over postpone +! false endof
	  [char] - of
	    -1 find-data-change lit, postpone over postpone +! false endof
	  [char] > of
	    1 find-addr-change cells lit, postpone + false endof
	  [char] < of
	    -1 find-addr-change cells lit, postpone + false endof
	  [char] . of
	    postpone dup postpone @ $FF lit, postpone and postpone emit
	    false endof
	  [char] , of postpone key postpone over postpone ! false endof
	  [char] ; of true endof
	  false swap
	endcase
      else
	drop prompt-hook @ ?execute refill false
      then
    until
    visible
    end-compile,
  ;

end-module

