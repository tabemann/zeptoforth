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

\ Compile to flash
compile-to-flash

compress-flash

continue-module forth-module

  temp-module import
  internal-module import
  esc-string-module import

  \ Our temporary buffer size
  256 constant temp-str-size

  commit-flash
  
  \ Our temporary buffer
  temp-str-size temp-size buffer: temp-str

  commit-flash
  
  \ Interpret or compile a counted string
  : c" ( -- )
    [immediate]
    state @ if
      postpone c"
    else
      advance-once
      compiling-to-flash? dup if
	compile-to-ram
      then
      [char] " parse-to-char
      dup 1+ temp-str ['] allocate-temp critical
      2dup c!
      rot over 1+ 3 roll move
      swap if
	compile-to-flash
      then
    then
  ;

  \ Interpret or compile a string
  : s" ( -- )
    [immediate]
    state @ if
      postpone s"
    else
      advance-once
      compiling-to-flash? dup if
	compile-to-ram
      then
      [char] " parse-to-char
      dup temp-str ['] allocate-temp critical
      rot over 3 pick move
      swap rot if
	compile-to-flash
      then
    then
  ;

  \ Interpret or compile an escaped counted string
  : c\" ( -- )
    [immediate]
    state @ if
      postpone c\"
    else
      [:
	advance-once
	here [char] " parse-esc-string
	here over - dup 1+ temp-str ['] allocate-temp critical
	2dup c!
	2 pick over 1+ 3 roll move
	swap ram-here!
	1 advance-bytes
      ;] with-ram
    then
  ;

  \ Interpret or compile an escaped string
  : s\" ( -- )
    [immediate]
    state @ if
      postpone s\"
    else
      [:
	advance-once
	here [char] " parse-esc-string
	here over - dup temp-str ['] allocate-temp critical
	2 pick over 3 pick move
	rot ram-here! swap
	1 advance-bytes
      ;] with-ram
    then
  ;

  \ Initialize the temporary buffer
  : init ( -- ) init temp-str-size temp-str init-temp ;

end-module

end-compress-flash

\ Reboot
reboot
