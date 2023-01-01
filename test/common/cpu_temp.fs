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

begin-module temp-test

  task import
  adc import
  
  begin-module temp-test-internal
  
    \ Enable the temperature sensor regardless of platform
    : enable-temp ( -- )
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
    
  end-module> import
    
  \ Display the temperature every second
  : display-temp ( -- )
    enable-temp
    0 [: [: temp-adc-chan default-adc adc@ . 1000 ms ;] qagain ;] 256 128 512 spawn run
  ;

end-module