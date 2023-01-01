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
    1 12 xe spi-pin \ SPI1_NSS
    1 13 xe spi-pin \ SPI1_SCK
    1 14 xe spi-pin \ SPI1_MISO
    1 15 xe spi-pin \ SPI1_MOSI
    1 master-spi
    281250 1 spi-baud!
    1 enable-spi-nss
    16 1 spi-data-size!
\    1 ti-ss-spi
    true false 1 motorola-spi
    1 msb-first-spi
    1 disable-spi-ssm
    1 enable-spi
    
    100 ms

    0 [: 355 256 do i 1 >spi 1 spi> . 1 ms loop ;] 256 128 512 spawn run
  ;
  
end-module
