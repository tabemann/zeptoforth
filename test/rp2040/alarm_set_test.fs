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

begin-module alarm-set-test
  
  : run-test ( -- )
    cr ." testing timer alarm0"
    0 timer::alarm-set? if cr ." timer alarm0 already set" then
    timer::us-counter-lsb 1_000_000 +
    [: 0 timer::clear-alarm-int ;] 0 timer::set-alarm
    0 timer::alarm-set? not if cr ." timer alarm0 not set" then
    0 timer::clear-alarm
    0 timer::alarm-set? if cr ." timer alarm0 still set" then
    
    cr ." testing timer alarm1"
    1 timer::alarm-set? if cr ." timer alarm1 already set" then
    timer::us-counter-lsb 1_000_000 +
    [: 1 timer::clear-alarm-int ;] 1 timer::set-alarm
    1 timer::alarm-set? not if cr ." timer alarm1 not set" then
    1 timer::clear-alarm
    1 timer::alarm-set? if cr ." timer alarm1 still set" then
    
    cr ." testing timer alarm2"
    2 timer::alarm-set? if cr ." timer alarm2 already set" then
    timer::us-counter-lsb 1_000_000 +
    [: 2 timer::clear-alarm-int ;] 2 timer::set-alarm
    2 timer::alarm-set? not if cr ." timer alarm2 not set" then
    2 timer::clear-alarm
    2 timer::alarm-set? if cr ." timer alarm2 still set" then

    cr ." testing timer alarm3"
    3 timer::alarm-set? if cr ." timer alarm3 already set" then
    timer::us-counter-lsb 1_000_000 +
    [: 3 timer::clear-alarm-int ;] 3 timer::set-alarm
    3 timer::alarm-set? not if cr ." timer alarm3 not set" then
    3 timer::clear-alarm
    3 timer::alarm-set? if cr ." timer alarm3 still set" then
  ;

end-module
