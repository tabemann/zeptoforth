\ Copyright (c) 2025 tmsgthb (GitHub)
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

begin-module mqtt

  \ debug flag
  false constant debug?

  begin-module mqtt-internal

    : x-out-of-range ." out of range" ;

    \ MQTT header flags
    %00010000 constant cmd-connect
    %00110000 constant cmd-publish-message
    %00000010 constant cmd-ack-deliver
    %11100000 constant cmd-disconnect

    %00100000 constant header-con-ack
    %01000000 constant header-pub-ack

    \ MQTT connect flag constants
    %10000000 constant user-flag
    %01000000 constant password-flag
    %00000010 constant clean-session-flag

    begin-structure ack-packet-size
      cfield: aps-header
      cfield: aps-msglen
      cfield: aps-ack-flag
      cfield: aps-retval
    end-structure

    \ Copy byte to buffer
    : byte> ( byte c-addr -- ) { byte c-addr -- }
      byte c-addr + c!
    ;

    \ Copy bytes to buffer 
    : bytes> ( b0 .. bn n c-addr -- ) { n c-addr -- }
      n 0 ?do c-addr i + c! loop
      c-addr n reverse
    ; 

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

    \ Encode 1 byte from Variable Byte Integer
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

    \ Sixfold dup
    : 6dup ( d1 d2 d3 -- d1 d2 d3 d1 d2 d3 )
      3 0 ?do 5 pick 5 pick loop
    ;

    \ Compute packet length
    : con-packet-size ( D: client-id D: mqtt-user D: mqtt-password -- size )
      swap drop + swap drop + swap drop
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
      \ return value
      c-addr dup index swap -
    ;

    \ Compute publish packet length
    : pub-packet-size ( D: mqtt-topic D: mqtt-message -- size )
      swap drop + swap drop
      dup 4 + 127 > if 5 + else 4 + then
    ;

    \ Construct publish packet
    : pub-packet-init 
      { D: mqtt-topic D: mqtt-message mqtt-message-id size c-addr -- c-addr size }
      c-addr size 0 fill
      \ header
      cmd-publish-message cmd-ack-deliver or c-addr c!
      c-addr 1 + { index }
      \ msg len
      size encode-vbi dup { temp-size } index bytes>
      index temp-size + to index
      \ topic length
      mqtt-topic dup >hi-low 2 index bytes>
      index 2 + to index
      dup to temp-size
      \ topic
      index swap move
      index temp-size + to index
      \ message id
      mqtt-message-id >hi-low 2 index bytes>
      index 2 + to index
      \ message
      mqtt-message dup to temp-size index swap move
      index temp-size + to index
      \ return value 
      c-addr dup index swap -
    ;

    \ Connect acknowledge?
    : con-ack? { c-addr n -- f }
      \ c-addr @ $20020000 =
      c-addr aps-header c@ $20 = 
      c-addr aps-msglen c@ $02 = and 
      c-addr aps-ack-flag c@ $00 = and
      c-addr aps-retval c@ $00 = and
    ;

    \ Publish acknowledge?
    : pub-ack? { c-addr n message-id -- f }
      c-addr aps-header c@ $40 = \ averts x-header-pub-ack
      c-addr aps-msglen c@ $02 = and \ averts x-header-pub-ack
      c-addr aps-ack-flag c@ c-addr aps-retval c@ hi-low> 
      message-id = 
      and
    ;

  end-module> import

  sema import
  oo import
  net-consts import
  net import
  endpoint-process import

  \ Communication state constants
  0 constant none
  1 constant send-connect-req 
  2 constant recv-connect-ack
  3 constant recv-publish-ack
  4 constant close-connection

  <endpoint-handler> begin-class <mqtt-client>
    \ Interface
    cell member iface
    \ Allocated endpoint
    cell member mqtt-endpoint
    \ IP of mqtt server
    cell member mqtt-server-ip
    \ Port of mqtt server
    cell member mqtt-server-port
    \ Mqtt client id 
    9 cell align member mqtt-client-id
    \ Address of mqtt user 
    cell member mqtt-user-addr
    \ Size of mqtt user
    cell member mqtt-user-size
    \ Address of mqtt password
    cell member mqtt-password-addr
    \ Size of mqtt password
    cell member mqtt-password-size
    \ Address of mqtt topic 
    cell member mqtt-topic-addr
    \ Size of mqtt topic
    cell member mqtt-topic-size
    \ Address of mqtt message
    cell member mqtt-message-addr
    \ Size of mqtt message
    cell member mqtt-message-size
    \ Communication state
    cell member com-state
    \ Semaphore to enable publishing
    sema-size cell align member pub-sema
    \ Flag to change pub-sema semaphore
    cell member sema-give?
    \ Publication done handler's token
    cell member pub-done-handler

    \ Debug prologoue
    method class-> ( self -- )

    \ Init mqtt client - set iface, server ip and port
    method init-mqtt-client ( iface dest-addr dest-port self -- )
    
    \ Set user name, passsword and generate client id
    method credentials! ( D: user-str D: password-str self -- )

    \ Connect to mqtt server and allocate endpoint
    method publish ( D: topic D: message message-id self -- )

    \ Set publication done handler
    method pub-done-handler! ( xt-handler self -- ) 

  end-class

  <mqtt-client> begin-implement

    \ Debug prologue
    :noname { self -- }
      cr ." <mqtt-client> -> " 
    ; define class->

    \ Init mqtt client - set iface, server ip and port  
    :noname { {iface} dest-addr dest-port self -- }
      [ debug? ] [if] self class-> ." init-mqtt-client method" [then]

      {iface} self iface !
      dest-addr self mqtt-server-ip !
      dest-port self mqtt-server-port !
      none self com-state !
      1 1 self pub-sema init-sema
      0 self pub-done-handler !
    ; define init-mqtt-client

    \ Set user name, password and generate client id 
    :noname { D: user-str D: password-str self -- }
      user-str self mqtt-user-size ! self mqtt-user-addr !
      password-str self mqtt-password-size ! self mqtt-password-addr !
      self rng::random 20 [: { buffer }  
        base @ { my-base } hex 
        buffer swap format-unsigned rot mqtt-client-id string!
        my-base base !
      ;] with-allot
    ; define credentials!

    \ MQTT endpoint handler
    :noname { endpoint self }
      [ debug? ] [if] self class-> ." handle-endpoint / state: " 
        endpoint endpoint-tcp-state@ . 
      [then]

      endpoint endpoint-tcp-state@ TCP_ESTABLISHED = if
        self com-state @ case
          send-connect-req of 
            \ Send connect packet
            [ debug? ] [if] self class-> ." handle-endpoint / send-connect-req" 
            [then]

            endpoint self iface @
            self mqtt-client-id string@ 
            self mqtt-user-addr @ self mqtt-user-size @
            self mqtt-password-addr @ self mqtt-password-size @ 6dup 
            con-packet-size dup 2 + [: { buffer }
              buffer con-packet-init
              2swap
              send-tcp-endpoint
            ;] with-allot
            
            recv-connect-ack self com-state !
            
          endof
          recv-connect-ack of 
            \ Receive connect ack and send publish packet
            [ debug? ] [if] self class-> ." handle-endpoint / recv-connect-ack" 
            [then] 
            
            endpoint endpoint-rx-data@ 10 ms con-ack?
            if cr ." MQTT LOGIN OK" cr
              endpoint self iface @
              self mqtt-topic-addr @ self mqtt-topic-size @ 
              self mqtt-message-addr @ self mqtt-message-size @ 4dup 
              pub-packet-size 1 swap dup 2 + [: { buffer }
                buffer pub-packet-init
                2swap
                send-tcp-endpoint 
              ;] with-allot           
              recv-publish-ack self com-state !
            else cr ." MQTT LOGIN FAILED" cr
              close-connection self com-state !
            then
          endof        
          recv-publish-ack of 
            \ Receive publish ack and send disconnect request
            [ debug? ] [if] self class-> ." handle-endpoint / recv-publish-ack" 
            [then]
            
            endpoint endpoint-rx-data@ 10 ms 1 pub-ack?
            if 
              cr ." MESSAGE PUBLISHED" cr
              \ call handler
              self pub-done-handler @ ?execute

              endpoint self iface @ 2 [: { buffer  }
                \ header
                cmd-disconnect buffer c!
                \ msg len
                0 buffer 1+ c!
                buffer 2 
                2swap
                send-tcp-endpoint
              ;] with-allot

            else 
              cr ." MESSAGE NOT PUBLISHED" cr
            then
            close-connection self com-state !
          endof
        endcase
      then
      endpoint endpoint-tcp-state@ TCP_CLOSE_WAIT = if
        cr ." CLOSING CONNECTION" cr
        self com-state @ close-connection = if
          [ debug? ] [if] self class-> ." handle-endpoint / close-tcp-endpoint" 
          [then]
          endpoint self iface @ close-tcp-endpoint 
          true self sema-give? !
        then
      then
      endpoint endpoint-tcp-state@ TCP_LAST_ACK = if
        cr ." WAITING FOR LAST ACK" cr
      then
      endpoint endpoint-tcp-state@ TCP_CLOSED = if
        cr ." CONNECTION CLOSED" cr
        \ none self com-state !
        true self sema-give? !
      then
      endpoint self iface @ endpoint-done
      
      \ Semaphore manupulation
      self sema-give? @ if
        [ debug? ] [if] self class-> ." handle-enpoint / semaphore manupulation"
        [then]
        none self com-state !
        self pub-sema give
        false self sema-give? !
      then

    ; define handle-endpoint

    \ Connect to mqtt server and allocate endpoint
    :noname { D: topic D: message message-id self -- }

      [ debug? ] [if] self class-> ." publish method " [then]
     
      \ Block publishing
      self pub-sema take
      false self sema-give? !
      
      message self mqtt-message-size ! self mqtt-message-addr !
      [ debug? ] [if] self class-> ." publish / message:" space 
        self mqtt-message-addr @ self mqtt-message-size @ type 
      [then]

      topic self mqtt-topic-size ! self mqtt-topic-addr !
      [ debug? ] [if] self class-> ." publish / topic:" space
        self mqtt-topic-addr @ self mqtt-topic-size @  type 
      [then]
   
      [ debug? ] [if] self class-> ." publish / allocate endpoint " [then]
     
      EPHEMERAL_PORT self mqtt-server-ip @ 
      self mqtt-server-port @
      self iface @ 
      allocate-tcp-connect-ipv4-endpoint if
        cr ." CONNECTED" 
        send-connect-req self com-state ! 
      else 
        cr ." NOT CONNECTED" 
      then
                 
    ; define publish

    \ Set publication done handler
    :noname { xt-handler self -- }      
      [ debug? ] [if]  self class-> ." pub-done-handler " [then]
      xt-handler self pub-done-handler !
    ; define pub-done-handler!

  end-implement

end-module
