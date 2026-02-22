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
\ utils/codeload3.sh -B 115200 -p <tty device> serial extra/rp_common/pico_w_net_ipv4_all.fs
\
\ Afterwards, if you had not already installed the CYW43439 firmware, driver,
\ and zeptoIP, make sure to reboot zeptoforth, either by executing:
\
\ reboot
\
\ or by entering control-C at a terminal or Reboot with zeptocom.js.
\
\ Then ensure that extra/rp_common/net_tools/pico_w_ipv4_base.fs and either
\ extra/rp2040/net/net_ntp_ipv4.fs (if using an RP2040) or
\ extra/rp2350/net/net_ntp_ipv4.fs (if using an RP2350) from the base
\ directory of zeptoforth are loaded into RAM and configured per the directions
\ therein.
\
\ Once that is done, execute:
\
\ pico-w-net-ntp::start-client
\ 
\ This will start an NTP client pointed at pool.ntp.org and regularly report
\ the time.
\ 
\ To display NTP time info, then execute:
\
\ pico-w-net-ntp::display-ntp-times
\ 
\ This will display the current time once every second once a time has been
\ acquired through NTP.

begin-module pico-w-net-ntp
  
  oo import
  net import
  net-ipv4 import
  simple-cyw43-net-ipv4 import
  endpoint-process import
  ntp-ipv4 import
  rtc import

  <ntp-ipv4> class-size buffer: my-ntp

  \ Initialize NTP
  : start-client ( -- )
    pico-w-net::my-interface @ <ntp-ipv4> my-ntp init-object
    my-ntp pico-w-net::my-cyw43-net net-endpoint-process@ add-endpoint-handler
    s" pool.ntp.org" ntp-port my-ntp init-ntp
  ;

  \ Display the current time repeatedly
  : display-ntp-times ( -- )
    begin key? not while
      my-ntp time-set? if
        cr my-ntp current-time@ f.
        date-time-size [: { date-time }
          date-time date-time@ date-time date-time.
        ;] with-aligned-allot
      then
      1000 ms
    repeat
    key drop
  ;

end-module