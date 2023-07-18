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

begin-module cyw43-nvram

  begin-module cyw43-nvram-internal

    \ Append an NVRAM null-terminated string
    : nvram," ( length "string" -- length' )
      advance-once
      [char] " internal::parse-to-char
      rot over + 1+ -rot
      begin ?dup while 1- swap dup c@ c, 1+ swap repeat drop 0 c,
    ;

    \ End string sequence
    : end-nvram ( length -- length' ) 0 c, 0 c, 2 + 4 align 4 align, ;
    
  end-module> import

  \ The NVRAM
  create nvram 0
  nvram," NVRAMRev=$Rev$"
  nvram," manfid=0x2d0"
  nvram," prodid=0x0727"
  nvram," vendid=0x14e4"
  nvram," devid=0x43e2"
  nvram," boardtype=0x0887"
  nvram," boardrev=0x1100"
  nvram," boardnum=22"
  nvram," macaddr=00:A0:50:b5:59:5e"
  nvram," sromrev=11"
  nvram," boardflags=0x00404001"
  nvram," boardflags3=0x04000000"
  nvram," xtalfreq=37400"
  nvram," nocrc=1"
  nvram," ag0=255"
  nvram," aa2g=1"
  nvram," ccode=ALL"
  nvram," pa0itssit=0x20"
  nvram," extpagain2g=0"
  nvram," pa2ga0=-168,6649,-778"
  nvram," AvVmid_c0=0x0,0xc8"
  nvram," cckpwroffset0=5"
  nvram," maxp2ga0=84"
  nvram," txpwrbckof=6"
  nvram," cckbw202gpo=0"
  nvram," legofdmbw202gpo=0x66111111"
  nvram," mcsbw202gpo=0x77711111"
  nvram," propbw202gpo=0xdd"
  nvram," ofdmdigfilttype=18"
  nvram," ofdmdigfilttypebe=18"
  nvram," papdmode=1"
  nvram," papdvalidtest=1"
  nvram," pacalidx2g=45"
  nvram," papdepsoffset=-30"
  nvram," papdendidx=58"
  nvram," ltecxmux=0"
  nvram," ltecxpadnum=0x0102"
  nvram," ltecxfnsel=0x44"
  nvram," ltecxgcigpio=0x01"
  nvram," il0macaddr=00:90:4c:c5:12:38"
  nvram," wl0id=0x431b"
  nvram," deadman_to=0xffffffff"
  nvram," muxenab=0x100"
  nvram," spurconfig=0x3"
  nvram," glitch_based_crsmin=1"
  nvram," btc_mode=1"
  end-nvram
  constant nvram-bytes
  
end-module
