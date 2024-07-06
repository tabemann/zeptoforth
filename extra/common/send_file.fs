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

begin-module send-file

  oo import
  fat32 import
  crc32 import
  base64 import
  
  begin-module send-file-internal

    \ The size of a received packet body
    512 constant packet-body-size

    \ Send packets done
    0 constant send-packet-done

    \ Sent the next packet
    1 constant send-packet-next

    \ Resent the current packet
    2 constant send-packet-resend
    
    \ Successfully received packet
    3 constant recv-packet-ack

    \ Unsuccessfully received packet
    4 constant recv-packet-nak

    \ Receive packets failed
    5 constant recv-packet-fail

    \ Send/receive Base64-encoded packet buffer size - do not change this
    700 constant base64-packet-buffer-size

    \ Send/receive Base64-encoded packet buffer
    base64-packet-buffer-size buffer: base64-packet-buffer

    \ Send/receive non-Base64-encoded packet buffer size - do not change this
    packet-body-size 12 + constant packet-buffer-size
    
    \ Send/receive non-Base64-encoded packet buffer
    packet-buffer-size aligned-buffer: packet-buffer

    \ Packet body received length
    0 value packet-body-len
    
    \ Packet body buffer
    packet-body-size buffer: packet-body

    \ The file being written to
    <fat32-file> class-size aligned-buffer: my-file

    \ Receive data into a buffer
    : recv-data ( -- )
      4 cells 0 do
        key base64-packet-buffer i + c!
      loop
    ;
    
    \ Receive a packet
    : recv-packet ( -- valid? )
      recv-data
      base64-packet-buffer 4 cells
      packet-buffer packet-buffer-size
      decode-base64 nip
    ;

    \ Send a packet
    : send-packet ( resend? -- )
      if send-packet-resend else send-packet-next then packet-buffer !
      packet-body-len packet-buffer cell + !
      packet-buffer 2 cells + packet-body-size 0 fill
      packet-body packet-buffer 2 cells + packet-body-len move
      packet-buffer 2 cells packet-body-size + generate-crc32
      packet-buffer 2 cells packet-body-size + + !
      packet-buffer packet-buffer-size
      base64-packet-buffer base64-packet-buffer-size encode-base64
      base64-packet-buffer swap type
    ;

    \ Send a done packet
    : send-done-packet ( -- )
      send-packet-done packet-buffer !
      0 packet-buffer cell + !
      packet-buffer 2 cells + packet-body-size 0 fill
      packet-buffer 2 cells packet-body-size + generate-crc32
      packet-buffer 2 cells packet-body-size + + !
      packet-buffer packet-buffer-size
      base64-packet-buffer base64-packet-buffer-size encode-base64
      base64-packet-buffer swap type
    ;

    \ Handle a packet
    : handle-packet ( -- error? valid? )
      recv-packet
      packet-buffer 2 cells generate-crc32 packet-buffer 2 cells + @ = and if
        packet-buffer @ case
          recv-packet-ack of false true endof
          recv-packet-nak of false false endof
          recv-packet-fail of true false endof
          false false rot
        endcase
      else
        false false
      then
    ;

    \ Transfer data
    : transfer-data ( -- valid? )
      false { done? }
      handle-packet 2drop
      begin done? not while
        packet-body packet-body-size my-file read-file to packet-body-len
        packet-body-len 0> if
          false { resend? }
          begin
            resend? send-packet
            handle-packet not to resend? to done?
            resend? not done? or
          until
        else
          true to done?
        then
      repeat
      begin
        send-done-packet
        handle-packet or
      until
    ;

  end-module> import

  \ Send a file
  : send-file ( path-addr path-u -- )
    [: { path-addr path-u dir }
      path-addr path-u my-file dir open-file
    ;] fat32-tools::current-fs@ with-root-path
    transfer-data
  ;

end-module
