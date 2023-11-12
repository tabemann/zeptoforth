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
  
  \ CYW43 ioctl class
  <object> begin-class <cyw43-ioctl>

    \ ioctl state
    cell member cyw43-ioctl-state

    \ ioctl tx buffer address
    cell member cyw43-ioctl-tx-buf

    \ ioctl tx size
    cell member cyw43-ioctl-tx-size

    \ ioctl rx buffer address
    cell member cyw43-ioctl-rx-buf
    
    \ Ioctl rx size
    cell member cyw43-ioctl-rx-size

    \ Pending ioctl type
    cell member cyw43-ioctl-kind

    \ Pending ioctl command
    cell member cyw43-ioctl-cmd

    \ Pending ioctl interface
    cell member cyw43-ioctl-iface

    \ ioctl ready semaphore
    sema-size member cyw43-ioctl-ready-sema

    \ ioctl done semaphore
    sema-size member cyw43-ioctl-done-sema

    \ ioctl lock
    lock-size member cyw43-ioctl-lock

    \ Test whether ioctl is pending
    method cyw43-ioctl-pending? ( self -- pending? )

    \ Test whether ioctl is done
    method cyw43-ioctl-done? ( self -- done? )
    
    \ Mark a pending ioctl as having been sent
    method mark-cyw43-ioctl-sent ( self -- )

    \ Wait for a complete ioctl
    method wait-cyw43-ioctl ( self -- bytes )

    \ Cancel an ioctl
    method cancel-cyw43-ioctl ( self -- )

    \ Execute an ioctl
    method do-cyw43-ioctl
    ( tx-addr tx-bytes rx-addr rx-bytes kind cmd iface self -- )

    \ Blocking ioctl operation
    method block-cyw43-ioctl
    ( tx-addr tx-bytes rx-addr rx-bytes kind cmd iface self -- actual-bytes )
    
    \ Mark an ioctl as done
    method cyw43-ioctl-done ( self -- )
    
  end-class

  <cyw43-ioctl> begin-implement

    \ Constructor
    :noname { self -- }
      self <object>->new
      ioctl-state-ready self cyw43-ioctl-state !
      1 0 self cyw43-ioctl-ready-sema init-sema
      1 0 self cyw43-ioctl-done-sema init-sema
      self cyw43-ioctl-ready-sema give
      self cyw43-ioctl-lock init-lock
    ; define new

    \ Test whether ioctl is pending
    :noname { self -- pending? }
      self cyw43-ioctl-state @ ioctl-state-pending =
    ; define cyw43-ioctl-pending?

    \ Test whether ioctl is done
    :noname { self -- pending? }
      self cyw43-ioctl-state @ ioctl-state-done =
    ; define cyw43-ioctl-done?

    \ Mark a pending ioctl as having been sent
    :noname { self -- }
      self cyw43-ioctl-state @ ioctl-state-pending = if
        ioctl-state-sent self cyw43-ioctl-state !
      then
    ; define mark-cyw43-ioctl-sent

    \ Send an ioctl
    :noname ( tx-addr tx-bytes rx-addr rx-bytes kind cmd iface self -- )
      { self }
      self cyw43-ioctl-iface !
      self cyw43-ioctl-cmd !
      self cyw43-ioctl-kind !
      self cyw43-ioctl-rx-size !
      self cyw43-ioctl-rx-buf !
      self cyw43-ioctl-tx-size !
      self cyw43-ioctl-tx-buf !
      ioctl-state-pending self cyw43-ioctl-state !
    ; define do-cyw43-ioctl

    \ Blocking ioctl operation
    :noname
      ( tx-addr tx-bytes rx-addr rx-bytes kind cmd iface self -- actual-bytes )
      dup { self }
      self cyw43-ioctl-ready-sema take
      do-cyw43-ioctl
      self cyw43-ioctl-done-sema take
      self cyw43-ioctl-rx-size @
      ioctl-state-ready self cyw43-ioctl-state !
      self cyw43-ioctl-ready-sema give
    ; define block-cyw43-ioctl

    \ Mark an ioctl as done
    :noname { addr bytes self -- }
      self cyw43-ioctl-state @ ioctl-state-sent = if
        addr self cyw43-ioctl-rx-buf @
        bytes self cyw43-ioctl-rx-size @ min dup { actual-bytes } move
        actual-bytes self cyw43-ioctl-rx-size !
        ioctl-state-done self cyw43-ioctl-state !
        self cyw43-ioctl-done-sema give
      else
        cr ." ioctl response but no pending ioctl"
      then
    ; define cyw43-ioctl-done
    
  end-implement
  
end-module
