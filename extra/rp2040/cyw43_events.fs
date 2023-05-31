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

begin-module cyw43-events

  oo import
  cyw43-structs import
  
  $FF constant EVENT_Unknown
  \ indicates status of set SSID
  0 constant EVENT_SET_SSID
  \ differentiates join IBSS from found (START) IBSS
  1 constant EVENT_JOIN
  \ STA founded an IBSS or AP started a BSS
  2 constant EVENT_START
  \ 802.11 AUTH request
  3 constant EVENT_AUTH
  \ 802.11 AUTH indication
  4 constant EVENT_AUTH_IND
  \ 802.11 DEAUTH request
  5 constant EVENT_DEAUTH
  \ 802.11 DEAUTH indication
  6 constant EVENT_DEAUTH_IND
  \ 802.11 ASSOC request
  7 constant EVENT_ASSOC
  \ 802.11 ASSOC indication
  8 constant EVENT_ASSOC_IND
  \ 802.11 REASSOC request
  9 constant EVENT_REASSOC
  \ 802.11 REASSOC indication
  10 constant EVENT_REASSOC_IND
  \ 802.11 DISASSOC request
  11 constant EVENT_DISASSOC
  \ 802.11 DISASSOC indication
  12 constant EVENT_DISASSOC_IND
  \ 802.11h Quiet period started
  13 constant EVENT_QUIET_START
  \ 802.11h Quiet period ended
  14 constant EVENT_QUIET_END
  \ BEACONS received/lost indication
  15 constant EVENT_BEACON_RX
  \ generic link indication
  16 constant EVENT_LINK
  \ TKIP MIC error occurred
  17 constant EVENT_MIC_ERROR
  \ NDIS style link indication
  18 constant EVENT_NDIS_LINK
  \ roam attempt occurred: indicate status & reason
  19 constant EVENT_ROAM
  \ change in dot11FailedCount (txfail)
  20 constant EVENT_TXFAIL
  \ WPA2 pmkid cache indication
  21 constant EVENT_PMKID_CACHE
  \ current AP's TSF value went backward
  22 constant EVENT_RETROGRADE_TSF
  \ AP was pruned from join list for reason
  23 constant EVENT_PRUNE
  \ report AutoAuth table entry match for join attempt
  24 constant EVENT_AUTOAUTH
  \ Event encapsulating an EAPOL message
  25 constant EVENT_EAPOL_MSG
  \ Scan results are ready or scan was aborted
  26 constant EVENT_SCAN_COMPLETE
  \ indicate to host addts fail/success
  27 constant EVENT_ADDTS_IND
  \ indicate to host delts fail/success
  28 constant EVENT_DELTS_IND
  \ indicate to host of beacon transmit
  29 constant EVENT_BCNSENT_IND
  \ Send the received beacon up to the host
  30 constant EVENT_BCNRX_MSG
  \ indicate to host loss of beacon
  31 constant EVENT_BCNLOST_MSG
  \ before attempting to roam
  32 constant EVENT_ROAM_PREP
  \ PFN network found event
  33 constant EVENT_PFN_NET_FOUND
  \ PFN network lost event
  34 constant EVENT_PFN_NET_LOST
  35 constant EVENT_RESET_COMPLETE
  36 constant EVENT_JOIN_START
  37 constant EVENT_ROAM_START
  38 constant EVENT_ASSOC_START
  39 constant EVENT_IBSS_ASSOC
  40 constant EVENT_RADIO
  \ PSM microcode watchdog fired
  41 constant EVENT_PSM_WATCHDOG
  \ CCX association start
  42 constant EVENT_CCX_ASSOC_START
  \ CCX association abort
  43 constant EVENT_CCX_ASSOC_ABORT
  \ probe request received
  44 constant EVENT_PROBREQ_MSG
  45 constant EVENT_SCAN_CONFIRM_IND
  \ WPA Handshake
  46 constant EVENT_PSK_SUP
  47 constant EVENT_COUNTRY_CODE_CHANGED
  \ WMMAC excedded medium time
  48 constant EVENT_EXCEEDED_MEDIUM_TIME
  \ WEP ICV error occurred
  49 constant EVENT_ICV_ERROR
  \ Unsupported unicast encrypted frame
  50 constant EVENT_UNICAST_DECODE_ERROR
  \ Unsupported multicast encrypted frame
  51 constant EVENT_MULTICAST_DECODE_ERROR
  52 constant EVENT_TRACE
  \ BT-AMP HCI event
  53 constant EVENT_BTA_HCI_EVENT
  \ I/F change (for wlan host notification)
  54 constant EVENT_IF
  \ P2P Discovery listen state expires
  55 constant EVENT_P2P_DISC_LISTEN_COMPLETE
  \ indicate RSSI change based on configured levels
  56 constant EVENT_RSSI
  \ PFN best network batching event
  57 constant EVENT_PFN_BEST_BATCHING
  58 constant EVENT_EXTLOG_MSG
  \ Action frame reception
  59 constant EVENT_ACTION_FRAME
  \ Action frame Tx complete
  60 constant EVENT_ACTION_FRAME_COMPLETE
  \ assoc request received
  61 constant EVENT_PRE_ASSOC_IND
  \ re-assoc request received
  62 constant EVENT_PRE_REASSOC_IND
  \ channel adopted (xxx: obsoleted)
  63 constant EVENT_CHANNEL_ADOPTED
  \ AP started
  64 constant EVENT_AP_STARTED
  \ AP stopped due to DFS
  65 constant EVENT_DFS_AP_STOP
  \ AP resumed due to DFS
  66 constant EVENT_DFS_AP_RESUME
  \ WAI stations event
  67 constant EVENT_WAI_STA_EVENT
  \ event encapsulating an WAI message
  68 constant EVENT_WAI_MSG
  \ escan result event
  69 constant EVENT_ESCAN_RESULT
  \ action frame off channel complete
  70 constant EVENT_ACTION_FRAME_OFF_CHAN_COMPLETE
  \ probe response received
  71 constant EVENT_PROBRESP_MSG
  \ P2P Probe request received
  72 constant EVENT_P2P_PROBREQ_MSG
  73 constant EVENT_DCS_REQUEST
  \ credits for D11 FIFOs. [AC0,AC1,AC2,AC3,BC_MC,ATIM]
  74 constant EVENT_FIFO_CREDIT_MAP
  \ Received action frame event WITH wl_event_rx_frame_data_t header
  75 constant EVENT_ACTION_FRAME_RX
  \ Wake Event timer fired, used for wake WLAN test mode
  76 constant EVENT_WAKE_EVENT
  \ Radio measurement complete
  77 constant EVENT_RM_COMPLETE
  \ Synchronize TSF with the host
  78 constant EVENT_HTSFSYNC
  \ request an overlay IOCTL/iovar from the host
  79 constant EVENT_OVERLAY_REQ
  80 constant EVENT_CSA_COMPLETE_IND
  \ excess PM Wake Event to inform host
  81 constant EVENT_EXCESS_PM_WAKE_EVENT
  \ no PFN networks around
  82 constant EVENT_PFN_SCAN_NONE
  \ last found PFN network gets lost
  83 constant EVENT_PFN_SCAN_ALLGONE
  84 constant EVENT_GTK_PLUMBED
  \ 802.11 ASSOC indication for NDIS only
  85 constant EVENT_ASSOC_IND_NDIS
  \ 802.11 REASSOC indication for NDIS only
  86 constant EVENT_REASSOC_IND_NDIS
  87 constant EVENT_ASSOC_REQ_IE
  88 constant EVENT_ASSOC_RESP_IE
  \ association recreated on resume
  89 constant EVENT_ASSOC_RECREATED
  \ rx action frame event for NDIS only
  90 constant EVENT_ACTION_FRAME_RX_NDIS
  \ authentication request received
  91 constant EVENT_AUTH_REQ
  \ fast assoc recreation failed
  93 constant EVENT_SPEEDY_RECREATE_FAIL
  \ port-specific event and payload (e.g. NDIS)
  94 constant EVENT_NATIVE
  \ event for tx pkt delay suddently jump
  95 constant EVENT_PKTDELAY_IND
  \ AWDL AW period starts
  96 constant EVENT_AWDL_AW
  \ AWDL Master/Slave/NE master role event
  97 constant EVENT_AWDL_ROLE
  \ Generic AWDL event
  98 constant EVENT_AWDL_EVENT
  \ NIC AF txstatus
  99 constant EVENT_NIC_AF_TXS
  \ NAN event
  100 constant EVENT_NAN
  101 constant EVENT_BEACON_FRAME_RX
  \ desired service found
  102 constant EVENT_SERVICE_FOUND
  \ GAS fragment received
  103 constant EVENT_GAS_FRAGMENT_RX
  \ GAS sessions all complete
  104 constant EVENT_GAS_COMPLETE
  \ New device found by p2p offload
  105 constant EVENT_P2PO_ADD_DEVICE
  \ device has been removed by p2p offload
  106 constant EVENT_P2PO_DEL_DEVICE
  \ WNM event to notify STA enter sleep mode
  107 constant EVENT_WNM_STA_SLEEP
  \ Indication of MAC tx failures (exhaustion of 802.11 retries) exceeding threshold(s)
  108 constant EVENT_TXFAIL_THRESH
  \ Proximity Detection event
  109 constant EVENT_PROXD
  \ AWDL RX Probe response
  111 constant EVENT_AWDL_RX_PRB_RESP
  \ AWDL RX Action Frames
  112 constant EVENT_AWDL_RX_ACT_FRAME
  \ AWDL Wowl nulls
  113 constant EVENT_AWDL_WOWL_NULLPKT
  \ AWDL Phycal status
  114 constant EVENT_AWDL_PHYCAL_STATUS
  \ AWDL OOB AF status
  115 constant EVENT_AWDL_OOB_AF_STATUS
  \ Interleaved Scan status
  116 constant EVENT_AWDL_SCAN_STATUS
  \ AWDL AW Start
  117 constant EVENT_AWDL_AW_START
  \ AWDL AW End
  118 constant EVENT_AWDL_AW_END
  \ AWDL AW Extensions
  119 constant EVENT_AWDL_AW_EXT
  120 constant EVENT_AWDL_PEER_CACHE_CONTROL
  121 constant EVENT_CSA_START_IND
  122 constant EVENT_CSA_DONE_IND
  123 constant EVENT_CSA_FAILURE_IND
  \ CCA based channel quality report
  124 constant EVENT_CCA_CHAN_QUAL
  \ to report change in BSSID while roaming
  125 constant EVENT_BSSID
  \ tx error indication
  126 constant EVENT_TX_STAT_ERROR
  \ credit check for BCMC supported
  127 constant EVENT_BCMC_CREDIT_SUPPORT
  \ psta primary interface indication
  128 constant EVENT_PSTA_PRIMARY_INTF_IND
  \ Handover Request Initiated
  130 constant EVENT_BT_WIFI_HANDOVER_REQ
  \ Southpaw TxInhibit notification
  131 constant EVENT_SPW_TXINHIBIT
  \ FBT Authentication Request Indication
  132 constant EVENT_FBT_AUTH_REQ_IND
  \ Enhancement addition for RSSI
  133 constant EVENT_RSSI_LQM
  \ Full probe/beacon (IEs etc) results
  134 constant EVENT_PFN_GSCAN_FULL_RESULT
  \ Significant change in rssi of bssids being tracked
  135 constant EVENT_PFN_SWC
  \ a STA been authroized for traffic
  136 constant EVENT_AUTHORIZED
  \ probe req with wl_event_rx_frame_data_t header
  137 constant EVENT_PROBREQ_MSG_RX
  \ PFN completed scan of network list
  138 constant EVENT_PFN_SCAN_COMPLETE
  \ RMC Event
  139 constant EVENT_RMC_EVENT
  \ DPSTA interface indication
  140 constant EVENT_DPSTA_INTF_IND
  \ RRM Event
  141 constant EVENT_RRM
  \ ULP entry event
  146 constant EVENT_ULP
  \ TCP Keep Alive Offload Event
  151 constant EVENT_TKO
  \ authentication request received
  187 constant EVENT_EXT_AUTH_REQ
  \ authentication request received
  188 constant EVENT_EXT_AUTH_FRAME_RX
  \ mgmt frame Tx complete
  189 constant EVENT_MGMT_FRAME_TXSTATUS
  \ highest val + 1 for range checking
  190 constant EVENT_LAST

  \ The event mask bytes
  EVENT_LAST 32 align 8 / constant event-mask-bytes
  
  \ Event message type
  begin-structure event-message-size
    field: evt-event-type
    field: evt-status
    bss-info-size +field evt-payload
  end-structure
  
  \ Out of range event exception
  : x-invalid-event ( -- ) cr ." invalid event" ;

  \ Validate an event
  : validate-event ( event -- ) EVENT_LAST u< averts x-invalid-event ;
  
  \ Event mask class
  <object> begin-class <cyw43-event-mask>
    
    \ The event mask bits
    event-mask-bytes member cyw43-event-bits

    \ Enable an event
    method enable-cyw43-event ( event self -- )

    \ Enable multiple events
    method enable-cyw43-events ( event-addr event-count self -- )

    \ Disable an event
    method disable-cyw43-event ( event self -- )

    \ Disable multiple events
    method disable-cyw43-events ( event-addr event-count self -- )

    \ Is an event enabled?
    method cyw43-event-enabled? ( event self -- enabled? )
    
  end-class

  \ Implement the event mask class
  <cyw43-event-mask> begin-implement

    \ The constructor
    :noname { self -- }
      self <object>->new
      self cyw43-event-bits event-mask-bytes 0 fill
    ; define new

    \ Enable an event
    :noname { event self -- }
      event validate-event
      event 7 and self cyw43-event-bits event 3 rshift + cbis!
    ; define enable-cyw43-event

    \ Enable multiple events
    :noname { event-addr event-count self -- }
      event-addr dup event-count + swap ?do i c@ validate-event loop
      event-addr dup event-count + swap ?do i c@ self enable-cyw43-event loop
    ; define enable-cyw43-events

    \ Disable an event
    :noname { event self -- }
      event validate-event
      event 7 and self cyw43-event-bits event 3 rshift + cbic!
    ; define disable-cyw43-event

    \ Disable multiple events
    :noname { event-addr event-count self -- }
      event-addr dup event-count + swap ?do i c@ validate-event loop
      event-addr dup event-count + swap ?do i c@ self disable-cyw43-event loop
    ; define disable-cyw43-events

    \ Is an event enabled
    :noname { event self -- enabled? }
      event validate-event
      event 7 and self cyw43-event-bits event 3 rshift + cbit@
    ; define cyw43-event-enabled?
    
  end-implement
  
end-module
