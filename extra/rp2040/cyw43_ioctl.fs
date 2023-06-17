\ Copyright (c) 2023 Travis Bemann
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

begin-module cyw43-ioctl

  oo import
  cyw43-consts import
  cyw43-structs import
  sema import
  lock import

  \ ioctl type
  0 constant ioctl-get
  2 constant ioctl-set

  \ ioctl state type
  0 constant ioctl-state-pending
  1 constant ioctl-state-sent
  2 constant ioctl-state-done
  3 constant ioctl-state-ready
  
  \ ioctl state header
  begin-structure ioctl-state-header-size

    \ ioctl state type
    field: ioctl-state-type

  end-structure

  \ Pending ioctl
  begin-structure pending-ioctl-state-size

    \ Header
    ioctl-state-header-size +field pioctl-header

    \ Pending ioctl buffer address
    field: pioctl-buf-addr

    \ Pending ioctl buffer size
    field: pioctl-buf-size

    \ Pending ioctl type
    field: pioctl-kind

    \ Pending ioctl command
    field: pioctl-cmd

    \ Pending ioctl interface
    field: pioctl-iface

  end-structure

  \ Sent ioctl
  begin-structure sent-ioctl-state-size

    \ Header
    ioctl-state-header-size +field sioctl-header

    \ Sent ioctl buffer address
    field: sioctl-buf-addr

    \ Sent ioctl bufer size
    field: sioctl-buf-size

  end-structure

  \ Done ioctl
  begin-structure done-ioctl-state-size

    \ Header
    ioctl-state-header-size +field dioctl-header

    \ Done ioctl response length
    field: dioctl-resp-len
    
  end-structure

  \ Overall ioctl-state-size
  pending-ioctl-state-size sent-ioctl-state-size max done-ioctl-state-size max
  constant ioctl-state-size

  \ CYW43 ioctl class
  <object> begin-class <cyw43-ioctl>

    \ ioctl data
    ioctl-state-size cell align member cyw43-ioctl-data

    \ ioctl semaphore
    sema-size member cyw43-ioctl-sema

    \ ioctl lock
    lock-size member cyw43-ioctl-lock

    \ Test whether ioctl is pending
    method cyw43-ioctl-pending? ( self -- pending? )

    \ Test whether ioctl is done
    method cyw43-ioctl-done? ( self -- done? )
    
    \ Wake an ioctl
    method wake-cyw43-ioctl ( self -- )

    \ Wait for a ioctl state
    method wait-cyw43-ioctl-state ( xt state self -- )
    
    \ Mark a pending ioctl as having been sent
    method mark-cyw43-ioctl-sent ( self -- )

    \ Wait for a complete ioctl
    method wait-cyw43-ioctl ( self -- bytes )

    \ Cancel an ioctl
    method cancel-cyw43-ioctl ( self -- )

    \ Execute an ioctl
    method do-cyw43-ioctl ( addr bytes kind cmd iface self -- )

    \ Poll sending an ioctl
    method poll-cyw43-ioctl ( addr bytes kind cmd iface self -- success? )

    \ Blocking ioctl operation
    method block-cyw43-ioctl ( addr bytes kind cmd iface self -- actual-bytes )
    
    \ Mark an ioctl as done
    method cyw43-ioctl-done ( self -- )
    
  end-class

  <cyw43-ioctl> begin-implement

    \ Constructor
    :noname { self -- }
      self <object>->new
      ioctl-state-ready self cyw43-ioctl-data ioctl-state-type !
      1 0 self cyw43-ioctl-sema init-sema
      self cyw43-ioctl-lock init-lock
    ; define new

    \ Test whether ioctl is pending
    :noname { self -- pending? }
      self cyw43-ioctl-data ioctl-state-type @ ioctl-state-pending =
    ; define cyw43-ioctl-pending?

    \ Test whether ioctl is done
    :noname { self -- pending? }
      self cyw43-ioctl-data ioctl-state-type @ ioctl-state-done =
    ; define cyw43-ioctl-done?

    \ Wake an ioctl
    :noname { self -- }
      self cyw43-ioctl-sema give
    ; define wake-cyw43-ioctl

    \ Wait for a ioctl state
    :noname { xt state self -- }
      begin
        xt state self [: { xt state self }
          self cyw43-ioctl-data ioctl-state-type @ state = if
            xt execute false
          else
            true
          then
        ;] self cyw43-ioctl-lock with-lock
      while
        self cyw43-ioctl-sema take
      repeat      
    ; define wait-cyw43-ioctl-state

    \ Mark a pending ioctl as having been sent
    :noname { self -- }
      self cyw43-ioctl-data ioctl-state-type @ ioctl-state-pending = if
        self cyw43-ioctl-data pioctl-buf-addr @ { addr }
        self cyw43-ioctl-data pioctl-buf-size @ { bytes }
        ioctl-state-sent self cyw43-ioctl-data ioctl-state-type !
        addr self cyw43-ioctl-data sioctl-buf-addr !
        bytes self cyw43-ioctl-data sioctl-buf-size !
      then
    ; define mark-cyw43-ioctl-sent

    \ Wait for a complete ioctl
    :noname { self -- addr actual-bytes }
      [: { self }
        self cyw43-ioctl-data dioctl-resp-len @
        ioctl-state-ready self cyw43-ioctl-data ioctl-state-type !
      ;] ioctl-state-done over wait-cyw43-ioctl-state
    ; define wait-cyw43-ioctl

    \ Cancel an ioctl
    :noname ( self -- )
      dup { self }
      [: { self }
        ioctl-state-done self cyw43-ioctl-data ioctl-state-type !
        0 self cyw43-ioctl-data dioctl-resp-len !
      ;] over cyw43-ioctl-lock with-lock
      self wake-cyw43-ioctl
    ; define cancel-cyw43-ioctl
    
    \ Send an ioctl
    :noname { addr bytes kind cmd iface self -- }
      ioctl-state-pending self cyw43-ioctl-data ioctl-state-type !
      iface self cyw43-ioctl-data pioctl-iface !
      cmd self cyw43-ioctl-data pioctl-cmd !
      kind self cyw43-ioctl-data pioctl-kind !
      bytes self cyw43-ioctl-data pioctl-buf-size !
      addr self cyw43-ioctl-data pioctl-buf-addr !
    ; define do-cyw43-ioctl

    \ Polling sending an ioctl
    :noname ( addr bytes kind cmd iface self -- success? )
      [: dup { self }
        self cyw43-ioctl-data ioctl-state-type @ ioctl-state-ready = if
          do-cyw43-ioctl true
        else
          drop false
        then
      ;] over cyw43-ioctl-lock with-lock
    ; define poll-cyw43-ioctl
    
    \ Blocking ioctl operation
    :noname { addr bytes kind cmd iface self -- actual-bytes }
      begin
        kind cmd iface addr bytes self poll-cyw43-ioctl
        dup not if pause then
      until
      self wait-cyw43-ioctl
    ; define block-cyw43-ioctl

    \ Mark an ioctl as done
    :noname ( addr bytes self -- )
      dup { self }
      [: { addr bytes self }
        self cyw43-ioctl-data ioctl-state-type @ ioctl-state-sent = if
          addr self cyw43-ioctl sioctl-buf-addr @ dup { buf-addr }
          bytes self cyw43-ioctl sioctl-buf-size @ min dup { actual-bytes } move
          ioctl-state-done self cyw43-ioctl-data ioctl-state-type !
          actual-bytes self cyw43-ioctl-data dioctl-resp-len !
        else
          cr ." ioctl response but no pending ioctl"
        then
      ;] over cyw43-ioctl-lock with-lock
      self wake-cyw43-ioctl
    ; define cyw43-ioctl-done
    
  end-implement
  
end-module
