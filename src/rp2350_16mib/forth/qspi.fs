\ Copyright (c) 2022-2025 Travis Bemann
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

\ Compile this to flash
compile-to-flash

begin-module qspi

  internal import

  begin-module qspi-internal
  
    \ Quad SPI map base
    $10000000 constant QUADSPI_Map_Base
    
    \ Hidden QSPI size
    $200000 constant QUADSPI_Hidden_Size

    \ Total QSPI size
    $1000000 constant QUADSPI_Size

  end-module> import
    
  \ Internal words which are exported through this module
  ' mass-qspi! export mass-qspi! \ ( data-addr bytes addr -- )
  ' cqspi! export cqspi! \ ( c addr -- )
  ' hqspi! export hqspi! \ ( h addr -- )
  ' qspi!  export qspi!  \ ( x addr -- )
  ' 2qspi! export 2qspi! \ ( d addr -- )
  ' erase-qspi-sector export erase-qspi-sector \ ( addr -- )

  \ Get whether QSPI is initialized
  : qspi-inited? ( -- flag ) true ;

  \ Get whether mapping QSPI is enabled
  : map-qspi-enabled? ( -- flag ) true ;

  \ Get the base usable Quad SPI address
  : qspi-base ( -- addr ) QUADSPI_Map_Base QUADSPI_Hidden_Size + ;

  \ Get the usable Quad SPI flash size
  : qspi-size ( -- bytes ) QUADSPI_Size QUADSPI_Hidden_Size - ;

  \ Bulk erase QSPI
  : erase-qspi-bulk ( -- )
    qspi-base begin dup [ qspi-base qspi-size + ] literal < while
      dup erase-qspi-sector $10000 +
    repeat
    drop
  ;
    
end-module

\ Reboot
reboot