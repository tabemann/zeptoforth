\ Copyright (c) 2021 Travis Bemann
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

\ Set up the wordlist order
forth-wordlist 1 set-order
forth-wordlist set-current

\ Test to see if this has already been compiled
defined? esc-string-wordlist not [if]

  \ Compile this to flash
  compile-to-flash

  \ Set up the wordlist
  wordlist constant esc-string-wordlist
  internal-wordlist forth-wordlist esc-string-wordlist 3 set-order
  esc-string-wordlist set-current

  \ Character constants
  $07 constant alert
  $08 constant backspace
  $1B constant escape
  $0C constant form-feed
  $0D constant return
  $0A constant line-feed
  $09 constant horizontal-tab
  $0B constant vertical-tab

  \ Get an input byte, and return whether a byte was successfully gotten
  : get-byte ( -- b success )
    >in @ input# @ < if
      input >in @ + b@
      1 >in +!
      true
    else
      0 false
    then
  ;

  \ Advance n bytes
  : advance-bytes ( bytes -- )
    >in @ + input# @ min >in !
  ;

  \ Revert n bytes
  : revert-bytes ( bytes -- ) negate >in +! ;
  
  \ Are bytes left
  : bytes-left? ( -- ) >in @ input# @ < ;
  
  \ Get the octal length
  : octal-len ( -- bytes )
    3 begin dup 0> while
      get-byte if
	dup [char] 0 < swap [char] 9 > or if
	  3 swap - dup 1+ revert-bytes exit
	else
	  1-
	then
      else
	drop 3 swap - dup revert-bytes exit
      then
    repeat
    drop 3 dup revert-bytes
  ;

  \ Get the hexadecimal length
  : hex-len ( max-bytes -- bytes )
    dup >r begin dup 0> while
      get-byte if
	dup [char] 0 < over [char] 9 > or
	over [char] A < 2 pick [char] F > or and
	over [char] a < rot [char] f > or and if
	  r> swap - dup 1+ revert-bytes exit
	else
	  1-
	then
      else
	drop r> swap - dup revert-bytes exit
      then
    repeat
    drop r> dup revert-bytes
  ;

  \ Parse an octal escape
  : escape-octal ( -- )
    octal-len
    dup >r base @ >r 10 base !
    >in @ input + swap parse-unsigned if
      r> base !
      dup 256 u< if
	b, r> advance-bytes
      else
	rdrop
      then
    else
      r> base ! rdrop
    then
  ;

  \ Parse a hexadecimal escape
  : escape-hex ( -- )
    2 hex-len
    dup >r base @ >r 16 base !
    >in @ input + swap parse-unsigned if
      r> base ! b, r> advance-bytes
    else
      r> base ! rdrop
    then
  ;

  \ Parse an escape
  : parse-escape ( -- )
    get-byte if
      dup [char] 0 < over [char] 9 > or if
	case
	  [char] a of alert b, endof
	  [char] A of alert b, endof
	  [char] b of backspace b, endof
	  [char] B of backspace b, endof
	  [char] e of escape b, endof
	  [char] E of escape b, endof
	  [char] f of form-feed b, endof
	  [char] F of form-feed b, endof
	  [char] m of return b, line-feed b, endof
	  [char] M of return b, line-feed b, endof
	  [char] n of line-feed b, endof
	  [char] N of line-feed b, endof
	  [char] q of [char] " b, endof
	  [char] Q of [char] " b, endof
	  [char] r of return b, endof
	  [char] R of return b, endof
	  [char] t of horizontal-tab b, endof
	  [char] T of horizontal-tab b, endof
	  [char] v of vertical-tab b, endof
	  [char] V of vertical-tab b, endof
	  [char] x of escape-hex endof
	  [char] X of escape-hex endof
	  dup b,
	endcase
      else
	1 revert-bytes escape-octal
      then
    else
      drop
    then
  ;

  \ Parse an escaped string
  : parse-esc-string ( end-byte -- )
    >r begin
      get-byte if
	dup [char] \ = if
	  drop parse-escape false
	else
	  dup r@ = if
	    drop true
	  else
	    b, false
	  then
	then
      else
	drop true
      then
    until
    rdrop
  ;

  \ Actually compile an escaped counted string
  : compile-esc-cstring ( end-byte -- )
    undefer-lit
    reserve-branch swap
    here dup 1+
    compiling-to-flash? if flash-here! else ram-here! then swap
    here swap parse-esc-string
    here swap -
    over bcurrent!
    swap here swap branch-back!
    lit,
    1 advance-bytes
  ;

  \ Change wordlists
  forth-wordlist set-current

  \ Compile an escaped counted string
  : c\" ( -- )
    [immediate]
    [compile-only]
    skip-to-token
    [char] " compile-esc-cstring
  ;

  \ Compile an escaped string
  : s\" ( -- )
    [immediate]
    [compile-only]
    skip-to-token
    [char] " compile-esc-cstring
    postpone count
  ;
  
  \ Compile typing an escaped string
  : .\" ( -- )
    [immediate]
    [compile-only]
    skip-to-token
    [char] " compile-esc-cstring
    postpone count
    postpone type
  ;

  \ Immediately type an escaped string
  : .\( ( -- )
    [immediate]
    skip-to-token
    compiling-to-flash? dup if
      compile-to-ram
    then
    here [char] ) parse-esc-string
    dup dup here swap - type
    swap if compile-to-flash flash-here! else ram-here! then
    1 advance-bytes
  ;
  
[then]

\ Warm reboot
warm
