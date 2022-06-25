\ Copyright (c) 2022 Travis Bemann
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

\  cell buffer: notify-buffer
\  variable response-task

  \ Initialize the test
  : init-test ( -- )
    1 ^ spi-internal :: init-spi
    3 ^ spi-internal :: init-spi
    1 4 xa spi-pin \ SPI1_NSS
    1 5 xa spi-pin \ SPI1_SCK
    1 6 xa spi-pin \ SPI1_MISO
    1 7 xa spi-pin \ SPI1_MOSI
    3 15 xa spi-pin \ SPI3_NSS
    3 3 xb spi-pin \ SPI3_SCK
    3 4 xb spi-pin \ SPI3_MISO
    3 5 xb spi-pin \ SPI3_MOSI
    1 master-spi
    3 slave-spi
    1000000 1 spi-baud!
    1 enable-spi-nss
    16 1 spi-data-size!
    true true 1 motorola-spi
    1 msb-first-spi
    1 disable-spi-ssm
    16 3 spi-data-size!
    true true 3 motorola-spi
    3 msb-first-spi
    3 enable-spi
    1 enable-spi
    
    100 ms

    0 [: 256 begin dup 3 >spi 3 spi> ." *" . 1+ again ;] 256 128 512 spawn run

    0 [: 10 0 do i 1 >spi 1 spi> ." +" . loop ;] 256 128 512 spawn run
  ;
  
end-module
