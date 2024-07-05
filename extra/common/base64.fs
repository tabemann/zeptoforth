\ Copyright (c) 2024 Travis Bemann
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

begin-module base64

  begin-module base64-internal

    \ The Base64 lookup table
    : base64-lookup ( -- addr )
      s" ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/" drop
    ;

    \ Encode 3 bytes
    : encode-3 { in-addr out-addr -- }
      in-addr c@ 16 lshift in-addr 1 + c@ 8 lshift or in-addr 2 + c@ or { data }
      data 18 rshift base64-lookup + c@ out-addr c!
      data 12 rshift $3F and base64-lookup + c@ out-addr 1 + c!
      data 6 rshift $3F and base64-lookup + c@ out-addr 2 + c!
      data $3F and base64-lookup + c@ out-addr 3 + c!
    ;

    \ Encode 2 bytes
    : encode-2 { in-addr out-addr -- }
      in-addr c@ 16 lshift in-addr 1 + c@ 8 lshift or { data }
      data 18 rshift base64-lookup + c@ out-addr c!
      data 12 rshift $3F and base64-lookup + c@ out-addr 1 + c!
      data 6 rshift $3F and base64-lookup + c@ out-addr 2 + c!
      [char] = out-addr 3 + c!
    ;

    \ Encode 1 byte
    : encode-1 { in-addr out-addr -- }
      in-addr c@ 16 lshift in-addr 1 + c@ 8 lshift or { data }
      data 18 rshift base64-lookup + c@ out-addr c!
      data 12 rshift $3F and base64-lookup + c@ out-addr 1 + c!
      [char] = out-addr 2 + c!
      [char] = out-addr 3 + c!
    ;

    \ Convert a Base64 character to a 6-bit value
    : base64-char>6-bit { c -- 6-bits valid? }
      c [char] A >= c [char] Z <= and if
        c [char] A - true
      else
        c [char] a >= c [char] z <= and if
          c [char] a - 26 + true
        else
          c [char] 0 >= c [char] 9 <= and if
            c [char] 0 - 52 + true
          else
            c [char] + = if
              62 true
            else
              c [char] / = if
                63 true
              else
                c [char] = = if
                  0 true
                else
                  0 false
                then
              then
            then
          then
        then
      then
    ;

    \ Parse 4 Base64 bytes
    : parse-base64 { in-addr -- valid? }
      in-addr c@ base64-char>6-bit not if drop false exit then 18 lshift
      in-addr 1 + c@ base64-char>6-bit not if 2drop false exit then 12 lshift or
      in-addr 2 + c@ base64-char>6-bit not if 2drop false exit then 6 lshift or
      in-addr 3 + c@ base64-char>6-bit not if 2drop false exit then or true
    ;

    \ Decode as 3 bytes
    : decode-3 { in-addr out-addr -- valid? }
      in-addr parse-base64 not if drop false exit then { data }
      data 16 rshift out-addr c!
      data 8 rshift out-addr 1 + c!
      data out-addr 2 + c!
      true
    ;

    \ Decode as 2 bytes
    : decode-2 { in-addr out-addr -- valid? }
      in-addr parse-base64 not if drop false exit then { data }
      data 16 rshift out-addr c!
      data 8 rshift out-addr 1 + c!
      true
    ;

    \ Decode as 1 bytes
    : decode-1 { in-addr out-addr -- valid? }
      in-addr parse-base64 not if drop false exit then { data }
      data 16 rshift out-addr c!
      true
    ;

  end-module> import

  \ Encode data as Base64
  : encode-base64 { in-addr in-len out-addr out-len -- total-len }
    0 { total-len }
    begin in-len 0> out-len 4 >= and while
      in-len 3 >= if
        in-addr out-addr encode-3
        -3 +to in-len
        3 +to in-addr
      else
        in-len 2 = if
          in-addr out-addr encode-2
        else
          in-addr out-addr encode-1
        then
        0 to in-len
      then
      4 +to out-addr
      -4 +to out-len
      4 +to total-len
    repeat
    total-len
  ;

  \ Decode Base64 data
  : decode-base64 { in-addr in-len out-addr out-len -- total-len valid? }
    0 { total-len }
    begin out-len 0> in-len 4 >= and while
      out-len 3 >= if
        in-addr out-addr decode-3 not if 0 false exit then
        -3 +to out-len
        3 +to out-addr
        3 +to total-len
      else
        out-len +to total-len
        out-len 2 = if
          in-addr out-addr decode-2 not if 0 false exit then
        else
          in-addr out-addr decode-1 not if 0 false exit then
        then
        0 to out-len
      then
      4 +to in-addr
      -4 +to in-len
    repeat
    total-len
    true
  ;
  
end-module
