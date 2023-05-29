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

  sema import
  lock import

  \ ioctl type
  0 constant ioctl-get
  2 constant ioctl-set

  \ ioctl state type
  0 constant ioctl-state-pending
  1 constant ioctl-state-sent
  2 constant ioctl-state-done
  
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
    ioctl-state-size member cyw43-ioctl-data

    \ ioctl semaphore
    sema-size member cyw43-ioctl-sema

    \ ioctl lock
    lock-size member cyw43-ioctl-lock

    \ Test whether ioctl is pending
    method cyw43-ioctl-pending? ( self -- pending? )
    
    \ Wake an ioctl
    method wake-cyw43-ioctl ( self -- )

    \ Wait for a ioctl state
    method wait-cyw43-ioctl-state ( xt state self -- )
    
    \ Wait on a pending ioctl
    method wait-pending-cyw43-ioctl ( self -- )

    \ Wait for a complete ioctl
    method wait-complete-cyw43-ioctl ( self -- )

    \ Cancel an ioctl
    method cancel-cyw43-ioctl ( self -- )

    \ Execute an ioctl
    method do-cyw43-ioctl ( kind cmd iface addr bytes self -- )
    
    \ Mark an ioctl as done
    method cyw43-ioctl-done ( addr bytes self -- )
    
  end-class

  <cyw43-ioctl> begin-implement

    \ Constructor
    :noname { self -- }
      self <object>->new
      ioctl-state-done self cyw43-ioctl-data ioctl-state-type !
      0 self cyw43-ioctl-data dioctl-resp-len !
      1 0 self cyw43-ioctl-sema init-sema
      self cyw43-ioctl-lock init-lock
    ; define new
    
    \ Test whether ioctl is pending
    :noname { self -- pending? }
      self cyw43-ioctl-data ioctl-state-type @ ioctl-state-pending =
    ; define cyw43-ioctl-pending?

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

    \ Wait on a pending ioctl
    :noname { self -- }
      self [: { self }
        self cyw43-ioctl-data ioctl-state-type @ ioctl-state-pending = if
          self cyw43-ioctl-data pioctl-buf-addr @ { addr }
          self cyw43-ioctl-data pioctl-buf-size @ { bytes }
          ioctl-state-sent self cyw43-ioctl-data ioctl-state-type !
          addr self cyw43-ioctl-data sioctl-buf-addr !
          bytes self cyw43-ioctl-data sioctl-buf-size !
        then
      ;] ioctl-state-pending self wait-cyw43-ioctl-state
    ; define wait-pending-cyw43-ioctl

    \ Wait for a complete ioctl
    :noname { self -- }
      [: ;] ioctl-state-done self wait-cyw43-ioctl-state
    ; define wait-complete-cyw43-ioctl

    \ Cancel an ioctl
    :noname ( self -- )
      dup { self }
      [: { self }
        ioctl-state-done self cyw43-ioctl-data ioctl-state-type !
        0 self cyw43-ioctl-data dioctl-resp-len !
      ;] over cyw43-ioctl-lock with-lock
      self wake-cyw43-ioctl
    ; define cancel-cyw43-ioctl
    
    \ Execute an ioctl
    :noname ( kind cmd iface addr bytes self -- )
      dup { self }
      [: { kind cmd iface addr bytes self }
        ioctl-state-pending self cyw43-ioctl-data ioctl-state-type !
        bytes self cyw43-ioctl-data pioctl-buf-size !
        addr self cyw43-ioctl-data pioctl-buf-addr !
        iface self cyw43-ioctl-data pioctl-iface !
        cmd self cyw43-ioctl-data pioctl-cmd !
        kind self cyw43-ioctl-data pioctl-kind !
      ;] over cyw43-ioctl-lock with-lock
      self wait-cyw43-ioctl-complete
    ; define do-cyw43-ioctl

    \ Mark an ioctl as done
    :noname ( addr bytes self -- )
      dup { self }
      [: { addr bytes self }
        self cyw43-ioctl-data ioctl-state-type @ ioctl-state-sent = if
          addr self cyw43-ioctl sioctl-buf-addr @
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
