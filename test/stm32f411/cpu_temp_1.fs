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
\ SOFTWARE.

begin-module uart-adc-test
  
  task import
  uart import
  pin import
  adc import
  lambda import
  
  begin-module uart-adc-test-internal
  
    \ Configure USART1 to use pins PB6 and PB7 at 115200 baud
    : config-uart ( -- )
      1 6 xb uart-pin
      1 7 xb uart-pin
      115200 1 uart-baud!
    ;
    
    \ Configure the temperature sensor regardless of platform
    : config-temp ( -- )
      [ s" enable-tsvref" find ] [if]
        enable-tsvref
        480 temp-adc-chan default-adc adc-sampling-time!
      [else]
        [ s" enable-vsense" find ] [if]
          enable-vsense
          640 temp-adc-chan default-adc adc-sampling-time!
        [else]
          [ s" adc-sampling-time!" find ] [if]
            480 temp-adc-chan default-adc adc-sampling-time!
          [then]
        [then]
      [then]
    ;
    
    \ Write a string to USART1
    : type-uart ( c-addr u -- )
      0 [: over + c@ 1 >uart ;] qcount drop
    ;
    
    \ Write a newline to USART1
    : cr-uart ( -- )
      s\" \x0D\x0A" type-uart
    ;
    
    \ Write a number to USART1
    : uart. ( n -- )
      0 <# #s #> type-uart
    ;
    
    \ Send the CPU temperature over the UART each second
    : send-temp-over-uart ( -- )
      [: temp-adc-chan default-adc adc@ uart. cr-uart 1000 ms 1 uart>? ;] quntil
      1 uart> drop
    ;
  
  end-module> import
  
  \ Start a task which sends the current CPU temperature over the
  \ UART each second
  : init-test ( -- )
    config-uart
    config-temp
    0 ['] send-temp-over-uart 320 128 512 spawn run
  ;
  
end-module
