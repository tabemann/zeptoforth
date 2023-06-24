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

begin-module cyw43-structs

  begin-structure shared-mem-data-size
    field: smd-flags
    field: smd-trap-addr
    field: smd-assert-exp-addr
    field: smd-assert-file-addr
    field: smd-assert-line
    field: smd-console-addr
    field: smd-msgtrace-addr
    field: smd-fwid
  end-structure

  begin-structure shared-mem-log-size
    field: sml-buf
    field: sml-buf-size
    field: sml-idx
    field: sml-out-idx
  end-structure

  begin-structure sdpcm-header-size
    hfield: sdpcmh-len
    hfield: sdpcmh-len-inv
    \ Rx/Tx sequence number
    cfield: sdpcmh-sequence
    \ 4 MSB Channel number, 4 LSB arbitrary flag
    cfield: sdpcmh-channel-and-flags
    \ Length of next data frame, reserved for Tx
    cfield: sdpcmh-next-length
    \ Data offset
    cfield: sdpcmh-header-length
    \ Flow control bits, reserved for Tx
    cfield: sdpcmh-wireless-flow-control
    \ Maximum Sequence number allowed by firmware for Tx
    cfield: sdpcmh-bus-data-credit
    \ Reserved
    2 +field sdpcmh-reserved
  end-structure

  begin-structure cdc-header-size
    field: cdch-cmd
    field: cdch-len
    hfield: cdch-flags
    hfield: cdch-id
    field: cdch-status
  end-structure

  2 constant BDC_VERSION
  4 constant BDC_VERSION_SHIFT
  
  begin-structure bdc-header-size
    cfield: bdch-flags
    \ 802.1d Priority (low 3 bits)
    cfield: bdch-priority
    cfield: bdch-flags2
    \ Offset from end of BDC header to packet data, in 4-uint8-t words. Leaves room for optional headers.
    cfield: bdch-data-offset
  end-structure

  begin-structure ethernet-header-size
    6 +field ethh-destination-mac
    6 +field ethh-source-mac
    hfield: ethh-ether-type
  end-structure

  begin-structure event-header-size
    hfield: evth-subtype
    hfield: evth-length
    cfield: evth-version
    3 +field evth-oui
    hfield: evth-user-subtype
  end-structure

  begin-structure event-message-size
    \ version   
    hfield: emsg-version
    \ see flags below
    hfield: emsg-flags
    \ Message (see below)
    field: emsg-event-type
    \ Status code (see below)
    field: emsg-status
    \ Reason code (if applicable)
    field: emsg-reason
    \ WLC-E-AUTH
    field: emsg-auth-type
    \ data buf
    field: emsg-datalen
    \ Station address (if applicable)
    6 +field emsg-addr
    \ name of the incoming packet interface
    16 +field emsg-ifname
    \ destination OS i/f index
    cfield: emsg-ifidx
    \ source bsscfg index
    cfield: emsg-bsscfgidx
  end-structure

  begin-structure event-packet-size
    ethernet-header-size +field evtp-eth
    event-header-size +field evtp-hdr
    event-message-size +field evtp-msg
  end-structure

  begin-structure download-header-size
    hfield: dh-flag
    hfield: dh-dload-type
    field: dh-len
    field: dh-crc
  end-structure

  $0001 constant DOWNLOAD_FLAG_NO_CRC
  $0002 constant DOWNLOAD_FLAG_BEGIN
  $0004 constant DOWNLOAD_FLAG_END
  $1000 constant DOWNLOAD_FLAG_HANDLER_VER

  \ Country Locale Matrix (CLM)
  2 constant DOWNLOAD_TYPE_CLM

  begin-structure country-info-size
    4 +field ci-country-abbrev
    field: ci-rev
    4 +field ci-country-code
  end-structure

  begin-structure ssid-info-size
    field: si-len
    32 +field si-ssid
  end-structure

  begin-structure passphrase-info-size
    hfield: pi-len
    hfield: pi-flags
    64 +field pi-passphrase
  end-structure

  begin-structure ssid-info-with-index-size
    field: siwi-index
    ssid-info-size +field siwi-ssid-info
  end-structure

  begin-structure event-mask-size
    field: emsk-iface
    24 +field emsk-events
  end-structure

  \ Parameters for a wifi scan

  begin-structure scan-params-size
    field: sp-version
    hfield: sp-action
    hfield: sp-sync-id
    field: sp-ssid-len
    32 +field sp-ssid
    6 +field sp-bssid
    cfield: sp-bss-type
    cfield: sp-scan-type
    field: sp-nprobes
    field: sp-active-time
    field: sp-passive-time
    field: sp-home-time
    field: sp-channel-num
    1 2 * +field sp-channel-list
  end-structure

  \ Wifi Scan Results Header, followed by `bss-count` `Bss-info`
  begin-structure scan-results-size
    field: sr-buflen
    field: sr-version
    hfield: sr-sync-id
    hfield: sr-bss-count
  end-structure

  \ Wifi Scan Result
  begin-structure bss-info-size
    field: bi-version
    field: bi-length
    6 +field bi-bssid
    hfield: bi-beacon-period
    hfield: bi-capability
    cfield: bi-ssid-len
    32 +field bi-ssid
    \ there will be more stuff here
  end-structure
  
end-module
