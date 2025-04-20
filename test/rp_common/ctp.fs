\ Copyright (c) 2025 Paul Koning
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

\ This is an implementation of the Ethernet Configuration Test Protocol
\ (loopback protocol), intended as a sample frame handler.  For the protocol
\ specification, seen the DEC/Intel/Xerox Ethernet specification, version 2.0
\ (November 1982), DEC document number AA-K759B-TK, chapter 8.

\ To use this, create an instance of one of the "simple" drivers, for example
\ simple-enc28j60-net-ipv4.  Start that, then call run-single-ctp-server in
\ this module, passing it that driver as argument.  Note that you can only do
\ this with one driver; to enable ctp with multiple drivers you need to
\ allocate and initialize an instance of <ctp> for each one, then call
\ start-ctp to start it.
\
\ By default the Loop Assistance Multicast Address is enabled.  Edit the value
\ of CTP_MC_ENABLE below if you do not want that.

begin-module ethernet-ctp

  oo import
  net import
  net-ipv4 import
  net-misc import
  net-ipv4-internal import
  net-consts import
  simple-net-ipv4 import
  frame-process import
  frame-interface import

  false constant DEBUG
  true constant CTP_MC_ENABLE

  begin-module ctp-internal

    \ Protocol type (in network order)
    $9000 rev16 constant ctp-proto

    \ CTP opcodes
    1 constant ctp-reply
    2 constant ctp-forward
    
  end-module> import

  \ All CTP responders multicast address
  $cf0000000000. 2constant ctp-multicast

  \ Frame handler for the CTP server
  <frame-handler> begin-class <ctp>

    continue-module ctp-internal

      \ The network driver
      cell member ctp-frame-driver

      \ My MAC address
      2 cells member ctp-src-addr

    end-module

    \ Start the CTP server
    method start-ctp ( self -- )
    
  end-class

  <ctp> begin-implement

    \ Construct a <ctp> instance
    :noname { driver self -- }
      self <frame-handler>->new
      driver self ctp-frame-driver !
      driver device-frame-interface@ mac-addr@ self ctp-src-addr 2!
      [ DEBUG ] [if]
	self ctp-src-addr 2@ ." my mac addr " mac. cr
      [then]
    ; define new

    \ Handle a frame.
    :noname { addr bytes self -- }
      \ todo: check dest addr
      [ DEBUG ] [if]
	addr bytes over + dump
      [then]
      addr ethh-ether-type h@ ctp-proto = if
	addr ethernet-header-size + { rec-ctp }
	rec-ctp h@ { skip }
	skip 1 and skip bytes 26 - > or if
	  \ bad skip count, ignore the frame
	  [ DEBUG ] [if]
	    ." bad skip count" skip . cr
	  [then]
	  exit
	then
	rec-ctp skip + 2 + h@
	[ DEBUG ] [if]
	  ." loop opcode " dup h.2 cr
	[then]
	ctp-forward = if
	  \ forward the frame, set dest address
	  rec-ctp skip + 4 + mac@
	  [ DEBUG ] [if]
	    ." forward to " 2dup mac. cr
	  [then]
	  over 1 and if
	    \ Forwarding to multicast, not valid, ignore the frame
	    [ DEBUG ] [if]
	      ." forward to multicast " 2dup mac. cr
	    [then]
	    2drop exit
	  then
	  addr ethh-destination-mac mac!
	  self ctp-src-addr 2@ addr ethh-source-mac mac!
	  skip 8 + rec-ctp h!
	  \ Now send it -- omit crc
	  [ DEBUG ] [if]
	    ." sending" addr bytes over + dump
	  [then]
	  addr bytes 4 - self ctp-frame-driver @ net-interface@ out-frame-interface @ put-tx-frame
	then
      then
    ; define handle-frame

    \ Start the server
    :noname { self -- }
      \ Connect the frame handler
      self self ctp-frame-driver @ net-frame-process@ add-frame-handler
      [ CTP_MC_ENABLE ] [if]
	\ Enable the loopback assistance multicast address
	ctp-multicast self ctp-frame-driver @ device-frame-interface@ add-multicast-filter
      [then]
    ; define start-ctp
    
  end-implement

  <ctp> class-size buffer: ctp-frame-handler
  
  : run-single-ctp-server ( driver -- )
    <ctp> ctp-frame-handler init-object
    ctp-frame-handler start-ctp
  ;
  
end-module
