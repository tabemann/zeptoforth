\ Copyright (c) 2025 tmsgthb (GitHub)
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

\ This is a demo of using ntc module with various thermistors.
\
\ Circuit diagram:
\
\  Pi Pico 3V3 -------+
\   (3V3 - Pin36)     |
\                     R0 - Resistor, R0 = RT at 25ºC
\                     |
\  Pi Pico ADC0 ------+
\   (GP26 - Pin31)    |
\                     RT - Thermistor
\                     |
\  Pi Pico GND -------+
\   (GND - Pin28)
\
\ With continuous use, the thermistor can self-heat, 
\ leading to an inaccurate reading at the AD input.

begin-module ntc-demo

  ntc import

  continue-module ntc-internal
    ntc-size buffer: my-ntc
  end-module

  begin-module ntc-demo-internal

    ntc-internal import
    \ Check calculation, result must be 25 ºC
    : calc-check ( D: B-val D: R0 -- ) { D: B-val D: R0 -- }
      0,0 B-val 0,0 my-ntc setup-abc
      3,3 R0 my-ntc setup-therm
      \ Force set value of rt (without reading value from ADC)
      R0 my-ntc rt 2!
      my-ntc ntc::ntc-internal::temp-k
      my-ntc ntc::ntc-internal::temp-c
      my-ntc temp@
      cr ." Thermistor: " R0 f.
      cr ." Celsius: " 2dup f. 
      bl emit swap drop 25 = if ." :) " else ." :( " then
      cr ." Kelvin: " f.
      cr ." ---------------"
    ;
  
    \ Calculation, R0 = 470Ω, B-value = 3560
    : calc-470 ( -- )
      3560,0 470,0 calc-check
    ;
  
    \ Calculation, R0 = 1kΩ, B-value = 3528 
    : calc-1k ( -- )
      3102,0 1000,0 calc-check
    ;
  
    \ Calculation, R0 = 10kΩ, B-value = 3435 
    : calc-10k (  -- )
      3435,0 10000,0 calc-check
    ;
  
    \ Calculation, R0 = 100kΩ, B-value = 3950 
    : calc-100k
      3950,0 100000,0 calc-check
    ;
  
    \ Test without measuring
    : calc-demo
      calc-470 calc-1k calc-10k calc-100k
    ;
  
    \ Demo with measuring
    : measure-demo ( adc chan pin D: B-val D: R0 -- ) 
      { adc chan pin D: B-val D: R0 -- }
      adc 0 pin my-ntc setup-adc
      0,0 B-val 0,0 my-ntc setup-abc
      3,3 R0 my-ntc setup-therm

      my-ntc ntc@ my-ntc temp@
     
      cr ." Thermistor " R0 f. ." Ω at 25 ºC demo" 
      cr ." Celsius: " f.
      cr ." Kelvin: " f.
    ;

  end-module> import

  \ Demo - measuring test using thermistor NTCLE100E3471JB0 
  \ ADC = 0, chanel = 0, pin = 26, R0 = 470, B-value = 3560
  : demo-470
    0 0 26 3560,0 470,0 measure-demo     
  ;

  \ Demo - measuring test using thermistor NTCLE100E3102JB0A 
  \ ADC = 0, chanel = 0, pin = 26, R0 = 1000, B-value = 3528
  : demo-1k
    0 0 26 3528,0 1000,0 measure-demo 
  ;

  \ Demo - measuring test using thermistor NRMR104F3435B2F
  \ ADC = 0, chanel = 0, pin = 26, R0 = 10000, B-value = 3435
  : demo-10k
    0 0 26 3435,0 10000,0 measure-demo
  ;

  \ Demo - measuring test using thermistor NRMR105F3950B1F
  \ ADC = 0, chanel = 0, pin = 26, R0 = 100000, B-value = 3950
  : demo-100k
    0 0 26 3950,0 100510,0 measure-demo
  ;
  
  \ Current demo 
  : demo ( -- )    
    demo-1k
  ;
 
end-module


