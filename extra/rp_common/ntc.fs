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

\ This is the common driver for the NTC thermistors.

begin-module ntc

  adc import
 
  begin-structure ntc-size
    begin-module ntc-internal
      \ ADC peripheral 
      field: ntc-adc
      \ Channel of ADC
      field: ntc-chan
      \ Pin for ADC
      field: ntc-pin
      \ A value
      2field: a-val
      \ B value
      2field: b-val
      \ C value
      2field: c-val
      \ Input voltage
      2field: vin
      \ Output voltage
      2field: vout
      \ Resistence of R0
      2field: r0
      \ Resistance of thermistor
      2field: rt
      \ Temperature in Kelvin
      2field: kelvin
      \ Temperature in Celsius
      2field: celsius
    end-module> import
  end-structure
  
  continue-module ntc-internal
  
    ansi-term import
    
    \ Colorize message 
    : dbg ( c-addr size -- )
      green color-effect!
      cr type
      none color-effect! 
    ;

    \ Colorize message and stack 
    : cr.s ( c-addr size -- )
      green color-effect!
      cr type bl emit .s 
      none color-effect!
    ;

    \ Enable/disbale debug messages
    false constant debug?
       
    \ t0 constant, 25ºC in ºK  = 25 + 273.15
    298,15 2constant t0-const

    \ Constant to convert analog value to digital  
    4096,0 2constant adc-const 

    \ Initialize ADC
    : ntc-init ( ntc -- )
      [ debug? ] [if] s" -> ntc-init" dbg [then]
      dup >r      
      ntc-adc @ r> ntc-pin @ adc-pin
      [ debug? ] [if] s" <- ntc-init" cr.s [then]
    ;
     
    \ Read value from ADC
    : ntc-adc@ ( ntc -- )
      [ debug? ] [if] s" -> ntc-adc@" dbg [then]
      dup >r
      ntc-adc @ enable-adc
      r@ ntc-chan @ r@ ntc-adc @ adc@
      s>f 
      r@ vin 2@ adc-const f/ f*
      r@ vout 2!
      r> ntc-adc @ disable-adc
      [ debug? ] [if] s" <- ntc-adc@" cr.s [then]
    ;
    
    \ Calculate rt from vout
    : calc-r ( ntc -- )
      [ debug? ] [if] s" -> calc-r" dbg [then]
      dup >r vout 2@ r@ r0 2@ f*
      r@ vin 2@ r@ vout 2@ d-
      f/ r> rt 2!
      [ debug? ] [if] s" <- calc-r" cr.s [then]
    ;
    
    \ Check a-val and c-val
    : temp-k? ( ntc -- )
      [ debug? ] [if] s" -> temp-k?" dbg [then]
      dup a-val 2@ d0>
      swap c-val 2@ d0>
      and
      [ debug? ] [if] s" <- temp-k? " cr.s [then]
    ;
    
    \ Calculate temperature with Steinhart-Hart equation
    \ 1 / (A + (B * math.log(Rt)) + C * math.log(Rt)^3)
    : temp-k, ( ntc -- ) 
      [ debug? ] [if] s" -> temp-k," dbg [then]
      dup >r
      a-val 2@
      r@ rt 2@ ln r@ b-val 2@ f*
      r@ rt 2@ ln 3,0 f** r@ c-val 2@ f*
      d+ d+
      1,0 2swap f/
      r> kelvin 2!
    
      [ debug? ] [if] s" <- temp-k," cr.s [then]
    ;
    
    \ Calculate temperature with b-val
    \ 1 / (1/T0 + 1/B * ln(Rt/R0))
    : temp-k ( ntc -- ) 
      [ debug? ] [if] s" -> temp-k" dbg [then]
      >r
      1,0 t0-const f/
      1,0 r@ b-val 2@ f/
      r@ rt 2@ r@ r0 2@ f/ ln 
      f* d+
      1,0 2swap f/
      r> kelvin 2!
      [ debug? ] [if] s" <- temp-k" cr.s [then]
    ;
  
    \ Convert Kelvin to Celsius
    : temp-c ( ntc -- )
      [ debug? ] [if] s" -> temp-c" dbg [then]
      dup >r
      kelvin 2@
      273,15 d-
      r> celsius 2!
      [ debug? ] [if] s" <- temp-c" cr.s [then]
    ;
    
  end-module> import
  
  \ Set up ADC
  : setup-adc ( adc chan pin ntc -- )
    [ debug? ] [if] s" -> setup-adc" dbg [then]
    \ pin
    dup >r ntc-pin !
    \ chan
    r@ ntc-chan ! 
    \ adc
    r> ntc-adc !
    [ debug? ] [if] s" <- setup-adc" cr.s [then]
  ;
  
  \ Set up thermistor's parameters ( values of a, b, c)
  \ These values are in S31.32 format
  : setup-abc ( D: a-val D: b-val D: c-val ntc -- )
    [ debug? ] [if] s" -> setup-abc" dbg [then]
    \ c-val
    dup >r c-val 2!
    \ b-val
    r@ b-val 2!
    \ a-val
    r> a-val 2!
    [ debug? ] [if] s" <- setup-abc" cr.s [then]
  ;
  
  \ Set up input voltage and resistance, these values are in S31.32 format
  : setup-therm ( D: vin D: r0 ntc -- )
    [ debug? ] [if] s" -> setup-therm" dbg [then]
    dup >r r0 2!
    0,0 r@ vout 2!
    0,0 r@ rt 2!  
    r> vin 2!
    [ debug? ] [if] s" <- setup-therm" cr.s [then]
  ;
    
  \ Measure temperature
  : measure-ntc ( ntc -- )  
    [ debug? ] [if] s" -> measure-ntc" dbg [then]
    dup >r
    ntc-init
    r@ ntc-adc@
    r@ calc-r
    r@ temp-k? if r@ temp-k, else r@ temp-k then
    r> temp-c
    [ debug? ] [if] s" <- measure-ntc" cr.s [then]
  ;
  
  \ Put temperatures to stack, these values are in S31.32
  : temp@ ( ntc -- D: kelvin D: celsius ) { ntc-local -- }
    [ debug? ] [if] s" -> temp@" dbg [then]
    ntc-local kelvin 2@
    ntc-local celsius 2@
    [ debug? ] [if] s" <- temp@" cr.s [then]
  ;

  \ Dump ntc structure
  : dump-ntc ( ntc -- )
    [ debug? ] [if] s" -> dump-ntc" dbg [then]
    dup ntc-adc @ cr ." ntc-adc: " .
    dup ntc-chan @ cr ." chan: " .
    dup ntc-pin @ cr ." pin: " .
    dup a-val 2@ cr ." a-val: " f.
    dup b-val 2@ cr ." b-val: " f.
    dup c-val 2@ cr ." c-val: " f.
    dup vin 2@ cr ." vin: " f.
    dup vout 2@ cr ." vout: " f.
    dup r0 2@ cr ." r0: " f.
    rt 2@ cr ." rt: " f.
    [ debug? ] [if] s" <- dump-ntc" cr.s [then]
  ;
  
 
end-module
