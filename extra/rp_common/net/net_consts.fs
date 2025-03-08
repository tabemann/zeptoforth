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

begin-module net-consts

  \ IPv4 address size
  4 constant ipv4-addr-size
  
  \ IPv6 address size
  16 constant ipv6-addr-size

  \ MAC address size
  6 constant mac-addr-size
  
  \ TCP states
  0 constant TCP_CLOSED
  1 constant TCP_LISTEN
  2 constant TCP_SYN_SENT
  3 constant TCP_SYN_RECEIVED
  4 constant TCP_ESTABLISHED
  5 constant TCP_FIN_WAIT_1
  6 constant TCP_FIN_WAIT_2
  7 constant TCP_CLOSING
  8 constant TCP_CLOSE_WAIT
  9 constant TCP_LAST_ACK
  10 constant TCP_TIME_WAIT

  \ Ephemeral ports
  49152 constant MIN_EPHEMERAL_PORT
  65534 constant MAX_EPHEMERAL_PORT
  -1 constant EPHEMERAL_PORT

end-module
