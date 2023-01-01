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
    2 4 xb spi-pin \ SPI2_NSS
    2 1 xi spi-pin \ SPI2_SCK
    2 14 xb spi-pin \ SPI2_MISO
    2 15 xb spi-pin \ SPI2_MOSI
    5 6 xf spi-pin \ SPI5_NSS
    5 7 xf spi-pin \ SPI5_SCK
    5 8 xf spi-pin \ SPI5_MISO
    5 9 xf spi-pin \ SPI5_MOSI
    2 master-spi
    5 slave-spi
    1000000 2 spi-baud!
    2 enable-spi-nss
    16 2 spi-data-size!
    true true 2 motorola-spi
    2 msb-first-spi
    2 disable-spi-ssm
    16 5 spi-data-size!
    true true 5 motorola-spi
    5 msb-first-spi
    5 enable-spi
    2 enable-spi
    
    100 ms

    0 [: 256 begin dup 5 >spi 5 spi> ." *" . 1+ again ;] 256 128 512 spawn run

    0 [: 10 0 do i 2 >spi 2 spi> ." +" . loop ;] 256 128 512 spawn run
  ;
  
end-module
