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

  \ Does an integer literal contain an extra base prefix
  : has-base-prefix? { addr bytes -- }
    bytes 0> if
      addr c@ dup [char] $ = if drop true exit then
      dup [char] # = if drop true exit then
      dup [char] / = if drop true exit then
      [char] % = if drop true exit then
      false
    else
      false
    then
  ;
  
  \ Is an integer literal double-cell
  : is-double? { addr bytes -- }
    0 { dot-count }
    addr bytes + addr ?do i c@ [char] . = if 1 +to dot-count then loop
    dot-count 1 = if bytes 1 > else false then
  ;

  \ Is an integer literal S31.32 fixed-point
  : is-s31.32? { addr bytes -- }
    0 { comma-count }
    addr bytes + addr ?do i c@ [char] , = if 1 +to comma-count then loop
    comma-count 1 = if bytes 1 > else false then
  ;

  \ Parse an 0x-hex literal
  : parse-0x-literal { addr bytes -- success? }
    2 +to addr -2 +to bytes
    addr bytes has-base-prefix? if false exit then
    addr bytes is-double? if
      base @ { saved-base }
      addr bytes [:
        hex parse-double if state @ if swap lit, lit, then true else 2drop false then
      ;] try
      saved-base base !
      ?raise
    else
      addr bytes is-s31.32? if
        base @ { saved-base }
        addr bytes [:
          hex parse-fixed if state @ if swap lit, lit, then true else 2drop false then
        ;] try
        saved-base base !
        ?raise
      else
        base @ { saved-base }
        addr bytes [:
          hex parse-integer if lit,? true else drop false then
        ;] try
        saved-base base !
        ?raise
      then
    then
  ;

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

  \ Parse a 2-byte Unicode character
  : parse-unicode?-2 { addr bytes -- c success? }
    bytes 2 = if
      addr c@ $E0 bic 6 lshift
      addr 1+ c@ dup $C0 and $80 = if
        $C0 bic or true
      else
        2drop 0 false
      then
    else
      0 false
    then
  ;

  \ Parse a 3-byte Unicode character
  : parse-unicode?-3 { addr bytes -- c success? }
    bytes 3 = if
      addr c@ $F0 bic 12 lshift
      addr 1+ c@ dup $C0 and $80 = if
        $C0 bic 6 lshift or
        addr 2 + c@ dup $C0 and $80 = if
          $C0 bic or true
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

  \ Parse a 4-byte Unicode character
  : parse-unicode?-4 { addr bytes -- c success? }
    bytes 4 = if
      addr c@ $F0 bic 18 lshift
      addr 1+ c@ dup $C0 and $80 = if
        $C0 bic 12 lshift or
        addr 2 + c@ dup $C0 and $80 = if
          $C0 bic 6 lshift or
          addr 3 + c@ dup $C0 and $80 = if
            $C0 bic or dup $10FFFF u<=
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
  
  \ Parse a Unicode character
  : parse-unicode? { addr bytes -- c success? }
    addr c@ unicode-start? if
      addr c@ $E0 and $C0 = if addr bytes parse-unicode?-2 exit then
      addr c@ $F0 and $E0 = if addr bytes parse-unicode?-3 exit then
      addr c@ $F0 and $F0 = if addr bytes parse-unicode?-4 exit then
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
      addr c@ unicode? if
        addr bytes parse-unicode? if lit,? true else drop false then
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
      addr c@ [char] 0 = addr 1+ c@ [char] x = and if
        addr bytes parse-0x-literal exit
      then
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
  addr c@ unicode? if
    addr bytes parse-unicode? averts x-invalid-char-literal
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

