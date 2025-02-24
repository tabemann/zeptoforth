\ mqtt module
begin-module mqtt

  oo import

  \ debug flag
  true constant debug?

  begin-module mqtt-internal
    : x-out-of-range ." out of range" ;
    : x-header ." packet header exception" ;
  
    800 constant buffer-max-size

    \ header flags
    %00010000 constant cmd-connect
    %00110000 constant cmd-publish-message
    %00000010 constant cmd-ack-deliver
    %11100000 constant cmd-disconnect

    %00100000 constant header-con-ack
    %01000000 constant header-pub-ack

    \ connect packet base size
    16 constant con-packet-base-size
    \ publish packet base size
    4 constant pub-packet-base-size

    \ connect flag constants
    %10000000 constant user-flag
    %01000000 constant password-flag
    %00000010 constant clean-session-flag

    <object> begin-class <mqtt-packet>
      \ members
      \ final buffer
      buffer-max-size cell align member buffer
      cell member packet-size

      \ methods
      \ debug
      method class->
      \ constructor
      method new
      \ utils
      method byte>
      method bytes>
      method string>
      method >hi-low 
      method hi-low>
      method mod/or
      method encode-vbi
      method header?
      method dump-packet
      \ builders
      method do-con-packet
      method do-pub-packet
      method do-discon-packet
      \ checkers
      method con-ack?
      method pub-ack?

    end-class


    <mqtt-packet> begin-implement

      :noname { self }
        cr ." <mqtt-packet> -> "
      ; define class->
    
      \ new method
      :noname { self }
        [ debug? ] [if] self class-> ." new" [then]
        self buffer buffer-max-size 0 fill
      ; define new

      \ Copy byte to buffer and return new index
      :noname ( byte index -- new-index ) { byte index self }
        byte self buffer index + c!
        index 1+
      ; define byte>

      \ Copy bytes to buffer and return new index
      :noname ( b0 .. bn n index -- new-index ) { n index self }
        n 0 do self buffer i index + + c! loop
        self buffer index + n reverse
        n index + 
      ; define bytes>

      \ Copy string to buffer and return new index
      :noname ( c-addr n index -- new-index ) { c-addr n index self }
        c-addr self buffer index + n move
        n index + 
      ; define string>

      \ Convert n to 2 byte, result on the stack
      :noname ( n -- hi-byte low-byte ) { self }
        dup $FFFF > triggers x-out-of-range dup 0 < triggers x-out-of-range
        dup $FF00 and 8 rshift
        swap $00FF and
      ; define >hi-low

      \ Create number from hi-byte and low-byte, result on the stack
      :noname ( hi-byte low-byte - u ) { self }
        swap 8 lshift or
      ; define hi-low>

      \ Encode 1 byte from Variable Bytye Integer
      :noname ( n -- mod n/128 ) { self }
        dup $80 mod swap $80 / dup 0 > if swap $80 or swap then
      ; define mod/or

      \ Encode Variable Byte Integer ( mqtt packet length )
      :noname ( lenght -- b0 bn n ) { self } 0 { size }
        dup $FFFFFF7F u> triggers x-out-of-range
        begin size 1+ to size self mod/or dup 0 = until
        drop size
      ; define encode-vbi

      \ Check header
      :noname ( byte header -- f ) { self }
        2dup and = swap drop
      ; define header?

      \ MQTT Connect packet
      \ header / $10 - 1 byte
      \ msg len - 1-4 byte
      \ protocol name length / $0004 - 2 byte
      \ protocol name  / MQTT - 4 byte
      \ protocol version / $04 - 1 byte
      \ connect flag - 1 byte
      \ keep alive / $003c - 2 byte
      \ client id length - 2 byte
      \ client id - variable length
      \ user name length - 2 byte
      \ user name - variable length
      \ password length - 2 byte
      \ password - variable lenght
      :noname ( client-id-addr client-id-size user-addr user-size password-addr password-size  -- c-addr n ) 
        { client-id-addr client-id-size user-addr user-size password-addr password-size self }
        [ debug? ] [if] self class-> ." do-con-packet client-id: " 
          client-id-addr client-id-size type 
          space ." user: " user-addr user-size type space 
          ." password: " password-addr password-size type 
        [then] 
        0 { index }
        self buffer buffer-max-size 0 fill
        cmd-connect index self byte> to index
        con-packet-base-size client-id-size + user-size + password-size + 
        self encode-vbi index self bytes> to index
        $00 $04 2 index self bytes> to index
        s" MQTT" index self string> to index
        $04 index self byte> to index
        user-flag password-flag or clean-session-flag or index self byte> to index
        $00 $3c 2 index self bytes> to index
        client-id-size self >hi-low 2 index self bytes> to index
        client-id-addr client-id-size index self string> to index
        user-size self >hi-low 2 index self bytes> to index
        user-addr user-size index self string> to index
        password-size self >hi-low 2 index self bytes> to index
        password-addr password-size index self string> to index
        self buffer index dup self packet-size !

        [ debug? ] [if] self class-> ." do-con-packet buffer-addr: " self buffer .
          space ." packet-size: " index . 
        [then]

      ; define do-con-packet

      \ MQTT Publish message packet
      \ header / $30 - 1 byte
      \ msg len - 1-4 byte
      \ topic length - 2 byte
      \ topic - variable length
      \ message id - 2 byte
      \ message - variable length
      :noname ( topic-addr topic-size message-addr message-size message-id -- c-addr n ) 
        { topic-addr topic-size message-addr message-size message-id self } 
        [ debug? ] [if] self class-> ." do-pub-packet topic: " 
          topic-addr topic-size type space ." message id: " message-id . 
          space ." message: " message-addr message-size type
        [then]
        0 { index }
        self buffer buffer-max-size 0 fill
        cmd-publish-message cmd-ack-deliver or index self byte> to index
        pub-packet-base-size topic-size + message-size + 
        self encode-vbi index self bytes> to index
        topic-size self >hi-low 2 index self bytes> to index
        topic-addr topic-size index self string> to index
        message-id self >hi-low 2 index self bytes> to index
        message-addr message-size index self string> to index
        self buffer index dup self packet-size !
      ; define do-pub-packet

      \ MQTT Disconenct request packet
      \ header / $e0 - 1 byte
      \ msg len: / $0 - 1 byte
      :noname ( -- ) { self }
        [ debug? ] [if] self class-> ." do-discon-packet" [then]
        0 { index }
        self buffer buffer-max-size 0 fill
        cmd-disconnect 0 2 index self bytes> to index
        self buffer index dup self packet-size !
      ; define do-discon-packet

      \ MQTT Connect ack packet
      \ header - 1 byte
      \ msg len - 1 byte
      \ return code - 1 byte
      :noname ( c-addr n -- f ) { c-addr n self }
        [ debug? ] [if] self class-> ." con-ack? c-addr: " 
          c-addr . space ." n: " n . 
        [then]
        c-addr c@ header-con-ack self header? averts x-header
        c-addr 1+ c@ 2 = averts x-header
        c-addr 2 + c@ 0 =
        [ debug? ] [if] self class-> ." con-ack? result:" space dup . [then]
      ; define con-ack?

      \ MQTT Publish ack packet
      \ header / $40 - 1 byte
      \ msg len / $02 - 1 byte
      \ message-id - 2 byte
      :noname ( c-addr n -- f ) { c-addr n message-id self }
        [ debug? ] [if] self class-> ." pub-ack? c-addr: " c-addr . space 
          ." n: " n . space ." message-id: " message-id . 
        [then]
        c-addr c@ header-pub-ack self header? averts x-header
        c-addr 2 + c@ c-addr 3 + c@ self hi-low> 
        message-id =
      ; define pub-ack?

      \ Dump packet
      :noname ( -- ) { self }
        [ debug? ] [if] self class-> ." dump packet with size" space 
          self packet-size @ . 
        [then]
        self buffer dup self packet-size @ + dump 
      ; define dump-packet

    end-implement

  end-module> import

  net-consts import
  net import
  endpoint-process import
  simple-cyw43-net import

  \ debug flag
  \ true constant debug?

  \ communication state constants
  0 constant none
  1 constant send-connect-req 
  2 constant recv-connect-ack
  \ 3 constant send-publish-req
  4 constant recv-publish-ack
  \ 5 constant send-disconnect-req
  6 constant recv-disconnect-ack
  7 constant close-connection

  : x-alloc-endpoint ." unable to allocate TCP endpoint" ;

  <endpoint-handler> begin-class <mqtt-client>
    \ members
    cell member iface
    cell member cyw43-net
    cell member mqtt-endpoint
    cell member mqtt-server-ip
    cell member mqtt-server-port
    cell member client-id
    cell member user
    cell member password
    cell member topic-addr
    cell member topic-size
    cell member message-addr
    cell member message-size
    <mqtt-packet> class-size cell align member packet
    cell member com-state 
    
    \ methods
    \ debug
    method class->
    \ constructor
    method new
    \ setters and gettesr
    method credentials!
    method client-id@
    method user@
    method password@
    \ util
    method publish
    \ packets
    method con-packet@
    method pub-packet@
    method discon-packet@
  end-class

  <mqtt-client> begin-implement

    :noname ( -- ) { self }
      cr ." <mqtt-client> -> " 
    ; define class->

    \ Constructor - set mqtt ip and port  
    :noname ( dest-addr dest-port -- ) { {iface} dest-addr dest-port self }
      \ debug
      [ debug? ] [if] self class-> ." new" [then]

      \ my-cyw43-net self cyw43-net !
      {iface} self iface !
      dest-addr self mqtt-server-ip !
      dest-port self mqtt-server-port !
      <mqtt-packet> self packet init-object
      self packet new
      none self com-state !
    
      \ set up handler
      \ [ debug? ] [if] self class-> ." my-cyw43-net: " self cyw43-net @ . [then]
    ; define new

    \ Set user name and password, generate client id 
    :noname ( user-addr user-size password-addr password-size ) 
      { user-addr user-size password-addr password-size self }
      user-addr user-size cfix self user !
      password-addr password-size cfix self password !
      self rng::random 20 [: { buffer }  
        base @ { my-base } hex 
        buffer swap format-unsigned cfix swap client-id !
        my-base base !
      ;] with-allot
    ; define credentials!

    \ Get client-id
    :noname ( -- client-id-addr client-id-size ) { self }
      self client-id @ count
    ; define client-id@

    \ Get user name
    :noname ( -- user-addr user-size ) { self }
      self user @ count
    ; define user@

    \ Get password
    :noname ( -- password-addr password-size ) { self }
      self password @ count
    ; define password@

    \ Generate connect packet 
    :noname ( -- c-addr n ) { self }
      self client-id@ self user@ self password@ self packet do-con-packet
    ; define con-packet@

    \ Generate publish packet
    :noname ( topic-addr topic-size message-addr message-size message-id -- c-addr n ) 
      { self } 
      [ debug? ] [if] self class-> ." pub-packet@ stack: " .s [then]
      self packet do-pub-packet
    ; define pub-packet@

    \ Generate disconnect request packet
    :noname ( -- c-addr n ) { self }
      [ debug? ] [if] self class-> ." discon-packet@" [then]
      self packet do-discon-packet
    ; define discon-packet@

    \ MQTT endpoint handler
    :noname { endpoint self }
      \ my-cyw43-net toggle-pico-w-led
      [ debug? ] [if] self class-> ." handle-endpoint state: " 
        endpoint endpoint-tcp-state@ . 
      [then]

      endpoint endpoint-tcp-state@ TCP_ESTABLISHED = if
        self com-state @ case
          send-connect-req of 
            \ Send connect packet
            [ debug? ] [if] self class-> ." handle-endpoint send-connect-req" [then]
            
            self con-packet@ 
            
            [ debug? ] [if] 2dup swap 
              self class-> ." handle-endpoint send-connect-req buffer-addr" 
              space . ." packet-size:" space . 
            [then]

            endpoint self iface @ send-tcp-endpoint 
            recv-connect-ack self com-state !
            \ endpoint self iface @ endpoint-done
          endof
          recv-connect-ack of 
            \ Receive connect ack and send publish packet
            [ debug? ] [if] self class-> ." handle-endpoint recv-connect-ack" [then] 
            
            endpoint endpoint-rx-data@ 
            self packet con-ack?
            
            self topic-addr @ self topic-size @ 
            self message-addr @ self message-size @ 1 self pub-packet@ 
            endpoint self iface @ send-tcp-endpoint
            recv-publish-ack self com-state !
            endpoint self iface @ endpoint-done
          endof        
          recv-publish-ack of 
            \ Receive publish ack and send disconnect request
            [ debug? ] [if] self class-> ." handle-endpoint recv-publish-ack" [then]
            endpoint endpoint-rx-data@ 1
            self packet pub-ack?
            if cr ." PUBLISHED" else ." NOT PUBLISHED" then

            self discon-packet@ endpoint self iface @ send-tcp-endpoint
            
            close-connection self com-state !
            endpoint self iface @ endpoint-done
          endof
        endcase
      then
      endpoint endpoint-tcp-state@ TCP_CLOSE_WAIT = if
        cr ." CLOSING CONNECTION" cr
        self com-state @ close-connection = if
          [ debug? ] [if] self class-> ." handle-endpoint close-tcp-endpoint" [then]
          endpoint self iface @ close-tcp-endpoint 
          \ self iface @ 500 0 endpoint [:
          \  drop close-tcp-endpoint
          \ ;] my-close-alarm set-alarm-delay-default
        then
      then
      endpoint endpoint-tcp-state@ TCP_LAST_ACK = if
        cr ." WAITING FOR LAST ACK" cr
      then
      endpoint endpoint-tcp-state@ TCP_CLOSED = if
        cr ." CONNECTION CLOSED" cr
      then
      endpoint self iface @ endpoint-done

    ; define handle-endpoint

    \ Publish message
    :noname ( D:topic D: message --  )
      { D: topic D: message message-id self }

      [ debug? ] [if] self class-> ." publish" [then]
      
      send-connect-req self com-state !
 
      message self message-size ! self message-addr !
      [ debug? ] [if] self class-> ." publish message:" space 
        self message-addr @ self message-size @ type 
      [then]

      topic self topic-size ! self topic-addr !
      [ debug? ] [if] self class-> ." publish topic:" space
        self topic-addr @ self topic-size @  type 
      [then]
   
      [ debug? ] [if] self class-> ." publish allocate endpoint " [then]
      
      EPHEMERAL_PORT self mqtt-server-ip @ self mqtt-server-port @
      self iface @ allocate-tcp-connect-ipv4-endpoint 
      if cr ." CONNECTED" else cr ." NOT CONNECTED" then

    ; define publish

  end-implement

end-module
