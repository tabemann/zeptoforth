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

compile-to-flash

: rp2040? ( -- flag )
  chip $7270 = swap 2040 = and
;

: rp2350? ( -- flag )
  chip $7270 = swap 2350 = and
;

\ Set the search order to both FORTH and INTERNAL
0 1 2 set-order

\ Select INTERNAL module for new words
1 set-current

\ Base of SYSINFO address space
$40000000 constant SYSINFO_BASE

\ CHIP_ID register address
SYSINFO_BASE $0 + constant SYSINFO_CHIP_ID

\ CHIP_ID revision register LSB
28 constant SYSINFO_CHIP_ID_REVISION_LSB

\ Select FORTH module for new words
0 set-current

\ Get chip revision
: chip-revision ( -- revision )
  SYSINFO_CHIP_ID @ SYSINFO_CHIP_ID_REVISION_LSB rshift
;

reboot
