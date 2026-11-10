\ Copyright (c) 2026 Travis Bemann
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

compile-to-flash

begin-module literal-internal

  \ Saved handle-number-hook
  variable saved-handle-number-hook

  \ Conditionally compile a literal
  : lit,? ( x -- x | ) state @ if lit, then ;

  \ Parse a string literal
  : parse-string-literal { addr bytes -- success? }
    addr 1+ source drop - >parse ! postpone s\" true
  ;

  \ Parse a tick literal
  : parse-tick-literal { addr bytes -- success? }
    addr 1+ c@ [char] ` = if
      addr 2 + bytes 2 - find ?dup if lit,? true else false then
    else
      addr 1+ bytes 1- find ?dup if >xt lit,? true else false then
    then
  ;

  \ Parse a 2-byte UTF-8 Unicode character
  : parse-utf-8-2 { addr bytes -- c success? }
    bytes 2 = if
      addr c@ $E0 bic 6 lshift
      addr 1+ c@ dup utf-8-continue? if
        $C0 bic or dup $80 u< if drop 0 false else true then
      else
        2drop 0 false
      then
    else
      0 false
    then
  ;

  \ Parse a 3-byte UTF-8 Unicode character
  : parse-utf-8-3 { addr bytes -- c success? }
    bytes 3 = if
      addr c@ $F0 bic 12 lshift
      addr 1+ c@ dup utf-8-continue? if
        $C0 bic 6 lshift or
        addr 2 + c@ dup utf-8-continue? if
          $C0 bic or dup $0800 u< if drop 0 false else true then
        else
          2drop 0 false
        then
      else
        2drop 0 false
      then
    else
      0 false
    then
  ;

  \ Parse a 4-byte UTF-8 Unicode character
  : parse-utf-8-4 { addr bytes -- c success? }
    bytes 4 = if
      addr c@ $F0 bic 18 lshift
      addr 1+ c@ dup utf-8-continue? if
        $C0 bic 12 lshift or
        addr 2 + c@ dup utf-8-continue? if
          $C0 bic 6 lshift or
          addr 3 + c@ dup utf-8-continue? if
            $C0 bic or
            dup $010000 u< if drop 0 false exit then
            dup $10FFFF u> if drop 0 false else true then
          else
            2drop 0 false
          then
        else
          2drop 0 false
        then
      else
        2drop 0 false
      then
    else
      0 false
    then
  ;
  
  \ Parse a UTF-8 Unicode character
  : parse-utf-8 { addr bytes -- c success? }
    addr c@ utf-8-start? if
      addr c@ $E0 and $C0 = if addr bytes parse-utf-8-2 exit then
      addr c@ $F0 and $E0 = if addr bytes parse-utf-8-3 exit then
      addr c@ $F0 and $F0 = if addr bytes parse-utf-8-4 exit then
      0 false
    else
      0 false
    then
  ;
  
  \ Parse a character literal
  : parse-char-literal { addr bytes -- success? }
    addr bytes 1- + c@ [char] ' = if -1 +to bytes then
    1 +to addr -1 +to bytes
    bytes 0> if
      addr c@ utf-8? if
        addr bytes parse-utf-8 if lit,? true else drop false then
      else
        bytes 1 = if addr c@ lit,? true else false then
      then
    else
      false
    then
  ;
  
  \ Parse a literal
  : parse-literal { addr bytes -- }
    addr bytes saved-handle-number-hook @ execute if true exit then
    bytes 1 > if
      addr c@ [char] " = if addr bytes parse-string-literal exit then
      addr c@ [char] ` = if addr bytes parse-tick-literal exit then
      addr c@ [char] ' = if addr bytes parse-char-literal exit then
    then
    false
  ;
  
  \ Initialize
  : do-init ( -- )
    handle-number-hook @ saved-handle-number-hook !
    ['] parse-literal handle-number-hook !
  ;
  initializer do-init
  
end-module> import

\ Invalid character literal exception
: x-invalid-char-literal ( -- ) ." invalid char literal" cr ;

\ Redefine CHAR to support Unicode
: char ( "char" -- c )
  token dup averts x-token-expected { addr bytes }
  bytes 0> averts x-invalid-char-literal
  addr c@ utf-8? if
    addr bytes parse-utf-8 averts x-invalid-char-literal
  else
    bytes 1 = averts x-invalid-char-literal
    addr c@
  then
;

\ Redefine [CHAR] to support Unicode
: [char] ( runtime: -- c compile-time: "char" -- )
  [immediate] [compile-only] char lit,
;

reboot

