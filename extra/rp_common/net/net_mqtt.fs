\ mqtt module
begin-module mqtt

  oo import 

  \ debug flag
  true constant debug?

  begin-module mqtt-internal

    : x-out-of-range ." out of range" ;
    : x-header ." packet header exception" ;
    : x-alloc-endpoint ." unable to allocate TCP endpoint" ;

    \ 800 constant buffer-max-size

    \ header flags
    %00010000 constant cmd-connect
    %00110000 constant cmd-publish-message
    %00000010 constant cmd-ack-deliver
    %11100000 constant cmd-disconnect

    %00100000 constant header-con-ack
    %01000000 constant header-pub-ack

    \ connect packet base size
    \ 16 constant con-packet-base-size
    \ publish packet base size
    \ 4 constant pub-packet-base-size

    \ connect flag constants
    %10000000 constant user-flag
    %01000000 constant password-flag
    %00000010 constant clean-session-flag

    \ offset of optional header and/or payload
    \ 2 value offset-ohp
    \ 0 value offset-client-id
    \ 0 value offset-mqtt-user
    \ 0 value offset-mqtt-password
    \ 0 value offset-mqtt-topic
    \ 0 value offset-mqtt-message
    \ 0 value mqtt-packet-size

    \ Copy byte to buffer
    : byte> ( byte c-addr -- ) { byte c-addr -- }
      byte c-addr + c!
    ;

    \ Copy bytes to buffer 
    : bytes> ( b0 .. bn n c-addr -- ) { n c-addr -- }
      n 0 ?do c-addr i + c! loop
      c-addr n reverse
    ; 

    \ Copy string to buffer and return new index
    \ : string>  ( c-addr n dest-addr -- ) { c-addr n dest-addr -- }
    \   swap move
    \ ;

    \ Convert n to 2 byte, result on the stack
    : >hi-low ( n -- hi-byte low-byte )
      dup $FFFF > triggers x-out-of-range dup 0 < triggers x-out-of-range
      dup $FF00 and 8 rshift
      swap $00FF and
    ;

    \ Create number from hi-byte and low-byte, result on the stack
    : hi-low> ( hi-byte low-byte - u )
      swap 8 lshift or
    ;

    \ Encode 1 byte from Variable Bytye Integer
    : mod/or ( n -- mod n/128 )
      dup $80 mod swap $80 / dup 0 > if swap $80 or swap then
    ; 

    \ Encode Variable Byte Integer ( mqtt packet length )
    : encode-vbi ( lenght -- b0 bn n ) 
      0 { size }
      dup $FFFFFF7F u> triggers x-out-of-range
      begin size 1+ to size mod/or dup 0 = until
      drop size
    ; 

    \ Compute packet length
    : con-packet-size ( D: client-id D: mqtt-user D: mqtt-password -- D: client-id D: mqtt-user D: mqtt-passsword size )
      dup 3 pick 6 pick + +  
      dup 16 + 127 > if 17 + else 16 + then
    ;

    \ Construct connection packet
    : con-packet-init 
      { D: client-id D: mqtt-user D: mqtt-password size c-addr -- c-addr size }
      c-addr size 0 fill
      \ header
      cmd-connect c-addr c!
      c-addr 1 + { index }
      \ msg len
      size encode-vbi dup { temp-size } index bytes>
      index temp-size + to index      
      \ protocol name, length version
      $00 $04 [char] M [char] Q [char] T [char] T $04
      \ connect flag
      user-flag password-flag or clean-session-flag or
      \ keepalive
      $00 $3c 10 index bytes>
      index 10 + to index
      \ client-id length
      client-id dup >hi-low 2 index bytes>
      index 2 + to index
      dup to temp-size
      \ client-id
      index swap move
      index temp-size + to index
      \ user length
      mqtt-user dup >hi-low 2 index bytes>
      index 2 + to index
      dup to temp-size
      \ user
      index swap move 
      index temp-size + to index 
      \ password length
      mqtt-password dup >hi-low 2 index bytes>
      index 2 + to index
      dup to temp-size
      \ password
      index swap move
      index temp-size + to index
      \ return value to stack
      c-addr dup index swap -
    ;

    \ Compute publish packet length
    : pub-packet-size ( D: mqtt-topic D: mqtt-message -- D: mqtt-topic D: mqtt-message size )
      dup 3 pick +
      dup 5 + 127 > if 6 + else 5 + then
    ;

    \ TODO
    : pub-packet-init
      { D: mqtt-topic D: mqtt-message mqtt-message-id size c-addr -- c-addr size }
      c-addr size 0 fill
      \ header
      \ msg len
      \ topic length
      \ topic
      \ message id
      \ mnessage
      c-addr size
    ;

    \ TODO
    \ Connect acknowledge?
    : con-ack? ( c-addr n -- f) 
        true
    ;

    \ TODO
    \ Publish acknowledge?
    : pub-ack? ( c-addr n -- f )
        \ todo
        true
    ;

  end-module> import


  net-consts import
  net import
  endpoint-process import
  simple-cyw43-net import


  \ communication state constants
  0 constant none
  1 constant send-connect-req 
  2 constant recv-connect-ack
  \ 3 constant send-publish-req
  4 constant recv-publish-ack
  \ 5 constant send-disconnect-req
  6 constant recv-disconnect-ack
  7 constant close-connection

  <endpoint-handler> begin-class <mqtt-client>
    \ members
    cell member iface
    cell member cyw43-net
    cell member mqtt-endpoint
    cell member mqtt-server-ip
    cell member mqtt-server-port
    cell member mqtt-client-id
    cell member mqtt-user
    cell member mqtt-password
    cell member mqtt-topic-addr
    cell member mqtt-topic-size
    cell member mqtt-message-addr
    cell member mqtt-message-size
    cell member com-state
    \ size of client-id + user + password
    \ cell member size-cup

    \ methods
    \ debug
    method class->

    \ init base settings
    method init-mqtt-client

    \ method con-packet-size
    \ method pub-packet-size

    \ setters and gettesr
    method credentials!
    method mqtt-client-id@
    method mqtt-user@
    method mqtt-password@
    \ util
    method publish
    \ packets
    \ method con-packet@
    \ method pub-packet@
    \ method discon-packet@
  end-class

  <mqtt-client> begin-implement

    :noname { self -- }
      cr ." <mqtt-client> -> " 
    ; define class->

    \ init mqtt client - set iface, server ip and port  
    :noname { {iface} dest-addr dest-port self -- }
      \ debug
      [ debug? ] [if] self class-> ." init-mqtt-client" [then]

      {iface} self iface !
      dest-addr self mqtt-server-ip !
      dest-port self mqtt-server-port !
      none self com-state !
    ; define init-mqtt-client

    \ Set user name, password and generate client id 
    :noname { D: user-str D: password-str self -- }
      user-str cfix self mqtt-user !
      password-str cfix self mqtt-password !
      self rng::random 20 [: { buffer }  
        base @ { my-base } hex 
        buffer swap format-unsigned cfix swap mqtt-client-id !
        my-base base !
      ;] with-allot
      \ user-size password-size + 8 + self size-cup !
    ; define credentials!

    \ Get client-id
    :noname { self -- client-id-addr client-id-size }
      self mqtt-client-id @ count
    ; define mqtt-client-id@

    \ Get user name
    :noname { self -- user-addr user-size }
      self mqtt-user @ count
    ; define mqtt-user@

    \ Get password
    :noname { self -- password-addr password-size }
      self mqtt-password @ count
    ; define mqtt-password@

    \ Generate connect packet 
    \ :noname ( -- c-addr n ) { self }
    \  self client-id@ self user@ self password@ self packet do-con-packet
    \ ; define con-packet@

    \ Generate publish packet
    \ :noname ( topic-addr topic-size message-addr message-size message-id -- c-addr n ) 
    \   { self } 
    \   [ debug? ] [if] self class-> ." pub-packet@ stack: " .s [then]
    \   self packet do-pub-packet
    \ ; define pub-packet@

    \ Generate disconnect request packet
    \ :noname ( -- c-addr n ) { self }
    \  [ debug? ] [if] self class-> ." discon-packet@" [then]
    \   self packet do-discon-packet
    \ ; define discon-packet@

    \ Compute connect packet size, and prepare stack
    \ :noname { self -- D: mqtt-password D: mqtt-user D: mqtt-client-id packet-size }
    \   [ debug? ] [if] self class-> ." con-packet-size" [then]
    \   \ prepare stack
    \   self mqtt-password@ self mqtt-user@ self mqtt-client-id@

    \  \ client-id offset
    \  dup offset-ohp + 10 + to offset-client-id
    \  \ user offset
    \  2 pick offset-client-id + 2 + to offset-mqtt-user
    \  \ pasword offset
    \  4 pick offset-mqtt-user + 2 + to offset-mqtt-password
    \
    \  self size-cup @ 4 + offset-client-id +
    \  [ debug? ] [if] self class-> ." con-packet-size end " 
    \    cr ." stack: " .s
    \  [then]
    \
    \ ; define con-packet-size

    \ Compute publish packet size, offsets, and puts topic and message to stack
    \ :noname { D: mqtt-topic D: mqtt-message self -- D: mqtt-topic D: mqtt-message packet-size }
    \  mqtt-message dup
    \  mqtt-topic rot 2dup drop dup >r +
    \
    \  dup 1 + > 127 if 2 to offset-ohp then
    \  offset-ohp to offset-mqtt-topic
    \  offset-mqtt-topic 2 + r> + to offset-mqtt-message
    \  dup to mqtt-packet-size
    \ ; define pub-packet-size 

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
            [ debug? ] [if] self class-> ." handle-endpoint send-connect-req" 
            [then]

            endpoint self iface @
            self mqtt-client-id@ self mqtt-user@ self mqtt-password@
            con-packet-size dup 2 + cr ." stack20: " .s
            [: { buffer }
              \ [ debug? ] [if] cr ." mqtt-packet-size: " mqtt-packet-size . [then]
              cr ." stack21: " .s 
              buffer con-packet-init

              2swap
              \ endpoint self iface @ send-tcp-endpoint
              cr ." stack22: " .s
              send-tcp-endpoint
            ;] with-allot
            
            \ endpoint self iface @ send-tcp-endpoint 
            recv-connect-ack self com-state !
            
          endof
          recv-connect-ack of 
            \ Receive connect ack and send publish packet
            [ debug? ] [if] self class-> ." handle-endpoint recv-connect-ack" [then] 
            
            endpoint endpoint-rx-data@ con-ack?
           
            endpoint self iface @
            self mqtt-topic-addr @ self mqtt-topic-size @ 
            self mqtt-message-addr @ self mqtt-message-size @ 
            self pub-packet-size dup [: { buffer }
              \ header
              \ cmd-publish-message packet c!
              \ msg len
              \ mqtt-packet-size encode-vbi packet 1+ bytes>
              \ topic length
              \ dup >hi-low packet offset-mqtt-topic + 2 bytes>
              \ topic
              \ packet offset-mqtt-topic + 2 + swap move                
              \ message length
              \ dup >hi-low packet offset-mqtt-message + 2 bytes>
              \ message
              \ packet offset-mqtt-message + 2 + swap move
              
              \ endpoint self iface @ send-tcp-endpoint
              send-tcp-endpoint 

            ;] with-allot
           
            recv-publish-ack self com-state !
          
          endof        
          recv-publish-ack of 
            \ Receive publish ack and send disconnect request
            [ debug? ] [if] self class-> ." handle-endpoint recv-publish-ack" [then]
            
            endpoint endpoint-rx-data@ pub-ack?
            if cr ." PUBLISHED" else ." NOT PUBLISHED" then

            endpoint self iface @
            2 [: { packet }
                \ header
                cmd-disconnect packet c!
                \ msg len
                0 packet 1+ c!
                packet 2 send-tcp-endpoint
            ;] with-allot
            \ self discon-packet@ endpoint self iface @ send-tcp-endpoint
            
            close-connection self com-state !
          endof
        endcase
      then
      endpoint endpoint-tcp-state@ TCP_CLOSE_WAIT = if
        cr ." CLOSING CONNECTION" cr
        self com-state @ close-connection = if
          [ debug? ] [if] self class-> ." handle-endpoint close-tcp-endpoint" [then]
          endpoint self iface @ close-tcp-endpoint 
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
 
      message self mqtt-message-size ! self mqtt-message-addr !
      [ debug? ] [if] self class-> ." publish message:" space 
        self mqtt-message-addr @ self mqtt-message-size @ type 
      [then]

      topic self mqtt-topic-size ! self mqtt-topic-addr !
      [ debug? ] [if] self class-> ." publish topic:" space
        self mqtt-topic-addr @ self mqtt-topic-size @  type 
      [then]
   
      [ debug? ] [if] self class-> ." publish allocate endpoint " [then]
      
      EPHEMERAL_PORT self mqtt-server-ip @ self mqtt-server-port @
      self iface @ allocate-tcp-connect-ipv4-endpoint 
      if cr ." CONNECTED" else cr ." NOT CONNECTED" then

    ; define publish

  end-implement


  \ test
  <mqtt-client> class-size buffer: mqtt-client
  <mqtt-client> mqtt-client init-object

  : test-1
    s" client-id_1-2" s" muser" s" mpassword" mqtt-internal::con-packet-size
    \ mqtt-client con-packet-size
  ;

  : test-2
    s" client-id_1-2" s" muser" s" mpassword" mqtt-internal::con-packet-size
    dup 2 + [: { buffer }
      cr ." stack1: " .s
      buffer mqtt-internal::con-packet-init

      cr ." stack2: " .s
      over + dump
    ;] with-allot
  ;


end-module
