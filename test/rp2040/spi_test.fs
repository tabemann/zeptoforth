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

continue-module forth

  spi import
  pin import
  task import

  \ Initialize the test
  : init-test ( -- )
    0 6 spi-pin \ SPI0 SCK
    0 7 spi-pin \ SPI0 TX
    1 8 spi-pin \ SPI1 RX
    1 9 spi-pin \ SPI1 CSn
    1 10 spi-pin \ SPI1 SCK
    1 11 spi-pin \ SPI1 TX
    0 16 spi-pin \ SPI0 RX
    0 17 spi-pin \ SPI0 CSn
    0 master-spi
    1 slave-spi
    2 0 do
      1000000 i spi-baud!
\      i ti-ss-spi
\      i natl-microwire-spi
      false false i motorola-spi
      16 i spi-data-size!
      i enable-spi
    loop
    100 ms

    0 [: 256 begin dup 1 >spi 1 spi> ." *" . 1+ again ;] 256 128 512 spawn run

    0 [: 10 0 do i 0 >spi 0 spi> ." +" . loop ;]
    256 128 512 spawn run
  ;
  
end-module
