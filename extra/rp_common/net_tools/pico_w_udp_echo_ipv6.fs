\ Copyright (c) 2023-2026 Travis Bemann
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

\ Note that prior to running this code, one must have uploaded the CYW43439
\ firmware, CYW43439 driver, and zeptoIP to one's Raspberry Pi Pico W (this
\ does not work on a Raspberry Pi Pico) as follows:
\ 
\ Execute from the base directory of zeptoforth:
\ 
\ utils/load_cyw43_fw.sh <tty device> <43439A0.bin path> <43439A0_clm.bin path>
\ 
\ 43439A0.bin and 43439A0_clm.bin can be gotten from:
\ https://github.com/tabemann/cyw43-firmware/tree/master/cyw43439-firmware
\ 
\ Then execute from the base directory of zeptoforth:
\ 
\ utils/codeload3.sh -B 115200 -p <tty device> serial extra/rp_common/pico_w_net_ipv6_all.fs
\
\ Afterwards, if you had not already installed the CYW43439 firmware, driver,
\ and zeptoIP, make sure to reboot zeptoforth, either by executing:
\
\ reboot
\
\ or by entering control-C at a terminal or Reboot with zeptocom.js.
\ 
\ Then ensure that extra/rp_common/net_tools/pico_w_ipv6_base.fs is loaded into
\ RAM and configured per the directions therein.
\
\ Once that is done, execute:
\
\ pico-w-net-udp-echo::start-server
\
\ This will establish a UDP server on port 4444 at the IPv6 address acquired via
\ DHCP that is reported on the console.
\
\ The recommended means of communicating with this REPL is:
\
\ nc -u <the reported IPv6 address> 4444
\
\ Each line transmitted by netcat will be echoed back to the user via UDP.

begin-module pico-w-net-udp-echo
  
  oo import
  net-misc import
  frame-process import
  net-consts import
  net-config import
  net import
  net-ipv6 import
  endpoint-process import
  simple-cyw43-net-ipv6 import
  pico-w-cyw43-net-ipv6 import

  begin-module pico-w-net-udp-echo-internal
    
    \ Our port
    4444 constant my-port
    
    <endpoint-handler> begin-class <udp-echo-handler>
      
    end-class

    <udp-echo-handler> begin-implement
      
      \ Handle a endpoint packet
      :noname { endpoint self -- }
        endpoint udp-endpoint?
        endpoint endpoint-local-port@ my-port = and if
          endpoint endpoint-ipv6-remote@ { src-0 src-1 src-2 src-3 src-port }
          endpoint endpoint-rx-data@ { addr bytes }
          addr bytes my-port src-0 src-1 src-2 src-3 src-port bytes [:
            { addr bytes buf }
            addr buf bytes move true
          ;] pico-w-net::my-interface @ send-ipv6-udp-packet drop
          pico-w-net::my-cyw43-net toggle-pico-w-led
          endpoint pico-w-net::my-interface @ endpoint-done
        then
      ; define handle-endpoint
      
    end-implement
    
    <udp-echo-handler> class-size buffer: my-udp-echo-handler

  end-module> import
  
  \ Initialize the test
  : start-server ( -- )
    <udp-echo-handler> my-udp-echo-handler init-object
    my-udp-echo-handler
    pico-w-net::my-cyw43-net net-endpoint-process@ add-endpoint-handler
    my-port pico-w-net::my-interface @ allocate-udp-listen-endpoint if
      drop
    else
      [: ." unable to allocate UDP listen endpoint" cr ;] ?raise
    then
  ;

end-module