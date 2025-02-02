\ Copyright (c) 2023-2025 Travis Bemann
\ Copyright (c) 2024-2025 Serialcomms (GitHub)
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

\ ring buffer sizes must be power-of-two values - see below

\  7 bit constant = 128 bytes
\  8 bit constant = 256 bytes
\  9 bit constant = 512 bytes
\ 10 bit constant = 1024 bytes
\ 11 bit constant = 2048 bytes
\ 12 bit constant = 4096 bytes

\ tx and rx buffer sizes can be different if required
\ adjust values locally for required usage scenarios.

compile-to-flash

begin-module usb-cdc-buffers

\ Transmit ring-buffer size - must be power-of-two value
9 bit constant tx-buffer-size

\ Receive ring-buffer size  - must be power-of-two value
8 bit constant rx-buffer-size

\ RAM variable for tx buffer read-index
variable tx-read-index

\ RAM variable for rx buffer read-index
variable rx-read-index

\ RAM variable for tx buffer write-index
variable tx-write-index

\ RAM variable for rx buffer write-index
variable rx-write-index

\ tx buffer size mask
tx-buffer-size 1 - constant tx-buffer-mask

\ rx buffer size mask
rx-buffer-size 1 - constant rx-buffer-mask

\ tx buffer to Host
tx-buffer-size buffer: tx-buffer

\ rx buffer to Pico
rx-buffer-size buffer: rx-buffer

\ Initialise tx ring buffer
: init-tx-ring ( -- )
  0 tx-read-index !
  0 tx-write-index !
  tx-buffer tx-buffer-size 0 fill
;

\ Initialise rx ring buffer
: init-rx-ring ( -- )
  0 rx-read-index !
  0 rx-write-index !
  rx-buffer rx-buffer-size 0 fill
;

\ Usable tx ring buffer zize
: tx-size ( -- u )
  tx-buffer-size 1 -
;

\ Usable rx ring buffer zize
: rx-size ( -- u )
  rx-buffer-size 1 -
;

\ Number of bytes used in tx ring buffer
: tx-used ( -- u )
  tx-read-index @ { read-index }
  tx-write-index @ { write-index }
  read-index write-index <= if
   write-index read-index -
  else
   tx-buffer-size read-index - write-index +
  then
;

\ Number of bytes used in rx ring buffer
: rx-used ( -- u )
  rx-read-index @ { read-index }
  rx-write-index @ { write-index }
  read-index write-index <= if
   write-index read-index -
  else
   rx-buffer-size read-index - write-index +
  then
;

\ Number of free bytes available in tx buffer
: tx-free ( -- bytes )
  tx-buffer-size 1 - tx-used -
;

\ Number of free bytes available in rx buffer
: rx-free ( -- bytes )
  rx-buffer-size 1 - rx-used -
;

\ Transmit ring buffer full flag
: tx-full? ( -- f )
  tx-write-index @ 1 + tx-buffer-mask and tx-read-index @ =
;

\ Receive ring buffer full flag
: rx-full? ( -- f )
  rx-write-index @ 1 + rx-buffer-mask and rx-read-index @ =
;

\ Transmit ring buffer empty flag
: tx-empty? ( -- f )
  tx-write-index @ tx-read-index @ =
;

\ Receive ring buffer empty flag
: rx-empty? ( -- f )
  rx-write-index @ rx-read-index @ =
;

\ USB read byte from tx ring buffer
: read-tx ( -- c )
  tx-empty? if
    0
  else
    tx-read-index @ tx-buffer + c@
    tx-read-index @ 1 + tx-buffer-mask and tx-read-index !
  then
;

\ Console read byte from rx ring buffer
: read-rx ( -- c )
  rx-empty? if
    0
  else
    rx-read-index @ rx-buffer + c@
    rx-read-index @ 1 + rx-buffer-mask and rx-read-index !
  then
;

\ Console write byte to tx ring buffer
: write-tx ( c -- )
  tx-full? if
    drop
  else
    tx-write-index @ tx-buffer + c!
    tx-write-index @ 1 + tx-buffer-mask and tx-write-index !
  then
;

\ USB write byte to rx ring buffer
: write-rx ( c -- )
  rx-full? if
    drop
  else
    rx-write-index @ rx-buffer + c!
    rx-write-index @ 1 + rx-buffer-mask and rx-write-index !
  then
;

end-module

compile-to-ram
