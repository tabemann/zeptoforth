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

begin-module recv-file

  oo import
  systick import
  fat32 import
  crc32 import
  base64 import
  
  begin-module recv-file-internal

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
      base64-packet-buffer-size 0 do
        key base64-packet-buffer i + c!
      loop
    ;
    
    \ Receive a packet
    : recv-packet ( -- valid? )
      recv-data
      base64-packet-buffer base64-packet-buffer-size
      packet-buffer packet-buffer-size
      decode-base64 nip
    ;

    \ Send a packet
    : send-packet ( message -- )
      packet-buffer !
      0 packet-buffer cell+ !
      packet-buffer 2 cells generate-crc32 packet-buffer 2 cells + !
      packet-buffer 3 cells
      base64-packet-buffer base64-packet-buffer-size
      encode-base64
      base64-packet-buffer swap type
    ;

    \ Handle a packet
    : handle-packet ( -- done? valid? )
      false true { done? valid? }
      recv-packet
      packet-buffer packet-buffer-size cell - generate-crc32
      packet-buffer packet-buffer-size cell - + @ = and if
        packet-buffer cell+ @ { new-packet-body-len }
        new-packet-body-len packet-body-size u> if
          recv-packet-fail send-packet true false exit
          true to done?
          false to valid?
        else
          packet-buffer @ case
            send-packet-done of
              packet-body packet-body-len my-file write-file drop
              true to done?
            endof
            send-packet-next of
              packet-body packet-body-len my-file write-file drop
              new-packet-body-len to packet-body-len
              packet-buffer 2 cells + packet-body packet-body-len move
            endof
            send-packet-resend of
              new-packet-body-len to packet-body-len
              packet-buffer 2 cells + packet-body packet-body-len move
            endof
            false to valid?
          endcase
        then
      else
        false to valid?
      then
      done? valid?
    ;

    \ Transfer data
    : transfer-data ( -- valid? )
      0 to packet-body-len
      false { done? }
      begin done? not while
        ['] handle-packet try { exception }
        exception 0= if
          swap to done?
          if
            recv-packet-ack send-packet
            done? if my-file file-fs@ flush then
          else
            recv-packet-nak send-packet
          then
        else
          recv-packet-fail send-packet
          exception ?raise
        then
      repeat
    ;

    \ Open or create a file
    : open-or-create-file ( path-addr path-u -- )
      [: { path-addr path-u dir }
        path-addr path-u dir exists? if
          path-addr path-u my-file dir open-file
        else
          path-addr path-u my-file dir create-file
        then
      ;] fat32-tools::current-fs@ with-root-path
    ;

  end-module> import

  \ Receive a file
  : recv-file ( path-addr path-u -- )
    open-or-create-file transfer-data my-file truncate-file
  ;

end-module
