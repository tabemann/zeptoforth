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

begin-module net-config

  \ Debugging print hook
  ' console::with-serial-output value debug-hook

  \ Are we debugging?
  false constant debug?
  
  \ DHCP logging
  false value dhcp-log?
  
  \ The MTU size
  1500 constant mtu-size

  \ The maximum fragment buffer size
  1500 constant fragment-buf-size

  \ The maximum IP endpoint in size
  3000 constant max-endpoint-in-size

  \ The maximum fragment unit count
  fragment-buf-size 8 align 8 / constant max-fragment-units

  \ Maximum endpoint count
  4 constant max-endpoints

  \ Maximum number of addresses to map
  16 constant max-addresses

  \ Maximum number of DNS addresses to cache
  16 constant max-dns-cache

  \ DNS address cache name heap block size
  16 constant dns-cache-heap-block-size

  \ DNS address cache name heap block count
  64 constant dns-cache-heap-block-count

  \ Maximum number of packets to send at a time
  16 constant max-out-packets

  \ Maximum number of packets to receive at a time
  16 constant max-in-packets

  \ Minimum number of bytes to send if more bytes are available
  16 value small-send-bytes

  \ The maximum retransmit count
  1 value max-retransmits

  \ The initial timeout time
  1500 value init-timeout

  \ The timeout multiplication factor
  2 value timeout-multiplier

  \ Send check interval
  250 value send-check-interval
  
  \ DNS port
  53 constant dns-port

  \ DNS source port
  65535 constant dns-src-port

  \ DHCP client port
  68 constant dhcp-client-port

  \ DHCP server port
  67 constant dhcp-server-port

  \ Parse DNS name depth
  16 constant parse-dns-name-depth

  \ Refresh interval
  500 value refresh-interval

  \ SYN_SENT initial refresh timeout
  1000 value syn-sent-init-refresh-timeout

  \ SYN_SENT refresh timeout multiplication factor
  2 value syn-sent-refresh-timeout-multiplier

  \ SYN_SENT maximum refresh count
  3 value syn-sent-max-refreshes

  \ SYN_RECEIVED initial refresh timeout
  1000 value syn-ack-sent-init-refresh-timeout

  \ SYN_RECEIVED refresh timeout multiplication factor
  2 value syn-ack-sent-refresh-timeout-multiplier

  \ SYN_RECEIVED maximum refresh count
  3 value syn-ack-sent-max-refreshes

  \ ESTABLISHED initial refresh timeout
  500 value established-init-refresh-timeout

  \ ESTABLISHED refresh timeout multplication factor
  2 value established-refresh-timeout-multiplier

  \ ESTABLISHED maximum refresh count
  3 value established-max-refreshes

  \ ESTABLISHED no missing packets refresh timeout
  5000 value established-no-missing-refresh-timeout

  \ FIN_WAIT_1 initial refresh timeout
  1000 value fin-wait-1-init-refresh-timeout

  \ FIN_WAIT_1 refresh timeout multiplication factor
  2 value fin-wait-1-refresh-timeout-multiplier

  \ FIN_WAIT_1 maximum refresh count
  3 value fin-wait-1-max-refreshes

  \ LAST_ACK initial refresh timeout
  1000 value last-ack-init-refresh-timeout

  \ LAST_ACK refresh timeout multiplication factor
  2 value last-ack-refresh-timeout-multiplier

  \ LAST_ACK maximum refresh count
  3 value last-ack-max-refreshes
  
  \ Close timeout
  1000 value close-timeout

  \ TIME_WAIT count
  8 constant time-wait-count

  \ TIME_WAIT timeout
  50000 value time-wait-timeout

  \ TIME_WAIT interval timeout
  10000 value time-wait-interval-timeout
  
  \ IPv4 discover attempts
  50000 value dhcp-discover-timeout

  \ Default DHCP renewal time
  86400 10000 * 2 / value default-dhcp-renew-interval

  \ Initial DHCP renewal divisor
  2 value dhcp-renew-init-divisor
  
  \ DHCP renewal retry divisor
  60 value dhcp-renew-retry-divisor

  \ DHCP rebind multiplier
  7 value dhcp-rebind-multiplier
  
  \ DHCP rebind divisor
  8 value dhcp-rebind-divisor

  \ DHCP rebind interval
  10 10000 * value dhcp-rebind-retry-interval

  \ Default DHCP ARP interval
  10000 value dhcp-arp-interval

  \ DHCP ARP attempt count
  5 value dhcp-arp-attempt-count

  \ DHCPDECLINE delay
  100000 value dhcpdecline-delay

  \ MAC address resolution interval in ticks (100 us intervals)
  50000 value mac-addr-resolve-interval

  \ DNS resolution interval in ticks (100 us intervals)
  50000 value dns-resolve-interval

  \ Maximum MAC address resolution attempts
  5 value max-mac-addr-resolve-attempts

  \ Maximum DNS resolution attempts
  5 value max-dns-resolve-attempts
  
end-module
