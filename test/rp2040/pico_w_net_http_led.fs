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

begin-module pico-w-net-http-server

  oo import
  cyw43-control import
  net-misc import
  frame-process import
  net-consts import
  net-config import
  net import
  endpoint-process import
  alarm import
  simple-cyw43-net import
  pico-w-cyw43-net import

  <pico-w-cyw43-net> class-size buffer: my-cyw43-net
  variable my-cyw43-control
  variable my-interface

  0 constant pio-addr
  0 constant sm-index
  pio::PIO0 constant pio-instance
  
  \ Our close alarm
  alarm-size buffer: my-close-alarm

  \ Our closing state
  variable closing?

  \ Server port
  80 constant server-port
  
  \ The HTTP input buffer size
  1024 constant http-buffer-size

  \ The HTTP buffer
  http-buffer-size buffer: http-buffer

  \ The HTTP buffer offset
  variable http-buffer-offset

  \ Are we on the first HTTP header
  variable first-http-header?

  \ HTTP method types
  0 constant method-not-set
  1 constant method-get
  2 constant method-post
  3 constant method-other
  
  \ HTTP method
  variable http-method

  \ The HTTP destination types
  0 constant dest-invalid
  1 constant dest-home
  2 constant dest-led-on
  3 constant dest-led-off
  4 constant dest-toggle-led

  \ HTTP destination
  variable http-dest

  \ THe HTTP protocol types
  0 constant protocol-unsupported
  1 constant protocol-http1.0
  2 constant protocol-http1.1

  \ HTTP protocol
  variable http-protocol

  \ HTTP corrupt
  variable http-corrupt?

  \ Output buffer size
  1024 constant out-buffer-size

  \ Output buffer
  out-buffer-size buffer: out-buffer

  \ Output buffer offset
  variable out-buffer-offset

  \ Find HTTP header newline
  : find-http-newline ( -- offset | -1 )
    http-buffer-offset @ 0 ?do
      http-buffer i + c@ $0D = if
        i 1+ http-buffer-offset @ < if
          http-buffer i 1+ + c@ $0A = if
            i unloop exit
          then
        else
          -1 unloop exit
        then
      then
    loop
    -1
  ;

  \ Get the token end from a header
  : find-token-end { offset length -- offset' }
    length offset ?do
      http-buffer i + c@ bl = if i unloop exit then
    loop
    length
  ;

  \ Get the next token from a header
  : find-next-token { offset length -- offset' | -1 }
    offset length < if
      length offset ?do
        http-buffer i + c@ bl <> if i unloop exit then
      loop
    then
    -1
  ;

  \ Remove text after ?
  : remove-extra { offset length -- offset' }
    length offset ?do
      http-buffer i + c@ [char] ? = if i unloop exit then
    loop
    offset
  ;

  \ Parse HTTP method
  : parse-http-method { D: header-method -- }
    header-method s" GET" equal-strings? if
      method-get
    else
      header-method s" POST" equal-strings? if
        method-post
      else
        method-other
      then
    then
    http-method !
  ;

  \ Parse HTTP destination
  : parse-http-dest { D: dest -- }
    dest s" /" equal-strings? if
      dest-home
    else
      dest s" /led-on" equal-strings? if
        dest-led-on
      else
        dest s" /led-off" equal-strings? if
          dest-led-off
        else
          dest s" /toggle-led" equal-strings? if
            dest-toggle-led
          else
            dest-invalid
          then
        then
      then
    then
    http-dest !
  ;

  \ Parse HTTP protocol
  : parse-http-protocol { D: protocol -- }
    protocol s" HTTP/1.0" equal-strings? if
      protocol-http1.0
    else
      protocol s" HTTP/1.1" equal-strings? if
        protocol-http1.1
      else
        protocol 5 min s" HTTP/" equal-strings? not http-corrupt? !
        protocol-unsupported
      then
    then
    http-protocol !
  ;

  \ Initialize state
  : init-http-state ( -- )
    false closing? !
    false http-corrupt? !
    true first-http-header? !
    method-other http-method !
    dest-invalid http-dest !
    protocol-unsupported http-protocol !
    0 http-buffer-offset !
    0 out-buffer-offset !
  ;
  
  \ Process the first HTTP header
  : http-first-header { length -- }
    false first-http-header? !
    false http-corrupt? !
    0 { offset }
    offset length find-token-end { end-offset }
    http-buffer offset + end-offset offset - parse-http-method
    end-offset length find-next-token to offset
    offset -1 <> if
      offset length find-token-end to end-offset
      end-offset length remove-extra { path-end-offset }
      http-buffer offset + path-end-offset offset - parse-http-dest
      end-offset length find-next-token to offset
      offset -1 <> if
        offset length find-token-end to end-offset
        http-buffer offset + end-offset offset - parse-http-protocol
        end-offset length find-next-token -1 <> http-corrupt? !
      else
        true http-corrupt? !
      then
    else
      dest-invalid http-dest !
    then
  ;

  \ Parse HTTP headers
  : parse-http-headers ( endpoint -- done? )
    begin
      find-http-newline dup -1 <> if { offset }
        first-http-header? @ if
          offset http-first-header http-corrupt? @ if true true else false then
        else
          offset 0= if true true else false then
        then
        offset 2 + negate http-buffer-offset +!
        http-buffer offset 2 + + http-buffer http-buffer-offset @ move
      else
        drop false true
      then
    until
  ;

  \ Send output
  : send-output { endpoint -- }
    out-buffer out-buffer-offset @ endpoint my-interface @ send-tcp-endpoint
    0 out-buffer-offset !
  ;

  \ Add text to the output buffer
  : add-output { addr bytes endpoint -- }
    begin bytes 0> while
      out-buffer-size out-buffer-offset @ - bytes min { bytes-to-send }
      addr out-buffer out-buffer-offset @ + bytes-to-send move
      bytes-to-send out-buffer-offset +!
      out-buffer-offset @ out-buffer-size = if endpoint send-output then
      bytes-to-send negate +to bytes
      bytes-to-send +to addr
    repeat
  ;

  \ Serve an HTTP protocol indicator
  : serve-protocol { endpoint -- }
    http-protocol @ case
      protocol-http1.0 of s" HTTP/1.0 " endof
      protocol-http1.1 of s" HTTP/1.1 " endof
      protocol-unsupported of s" HTTP/1.1 " endof
    endcase
    endpoint add-output
  ;

  \ Serve header fields
  : serve-header-fields { endpoint -- }
    s\" Content-Type: text/html\r\n" endpoint add-output
    s\" Connection: close\r\n\r\n" endpoint add-output
  ;

  \ Serve a corrupt HTTP response
  : serve-corrupt { endpoint -- }
    endpoint serve-protocol
    s\" 400 Bad Request\r\n" endpoint add-output
    endpoint serve-header-fields
    s" <html><head><title>400 Bad Request</title></head>"
    endpoint add-output
    s" <body><h1>400 Bad Request</h1></body>" endpoint add-output
    s" </html>" endpoint add-output

  ;

  \ Serve an invalid destination
  : serve-invalid { endpoint -- }
    endpoint serve-protocol
    s\" 404 Not Found\r\n" endpoint add-output
    endpoint serve-header-fields
    s" <html><head><title>404 Not Found</title></head>"
    endpoint add-output
    s" <body><h1>404 Not Found</h1></body>" endpoint add-output
    s" </html>" endpoint add-output
  ;

  \ Serve an unsupported method
  : serve-method-not-allowed { endpoint -- }
    endpoint serve-protocol
    s\" 405 Method Not Allowed\r\n" endpoint add-output
    endpoint serve-header-fields
    s" <html><head><title>405 Method Not Allowed</title></head>"
    endpoint add-output
    s" <body><h1>405 Method Not Allowed</h1></body>" endpoint add-output
    s" </html>" endpoint add-output
  ;
  
  \ Serve an OK destination
  : serve-ok { endpoint -- }
    endpoint serve-protocol
    s\" 200 OK\r\n" endpoint add-output
    endpoint serve-header-fields
  ;

  \ Generate a button with a label
  : serve-button { D: label D: path endpoint -- }
    s\" <form method=\"post\" action=\"" endpoint add-output
    path endpoint add-output
    s\" \"><button type=\"submit\">" endpoint add-output
    label endpoint add-output
    s" </button></form>" endpoint add-output
  ;

  \ Serve HTML content
  : serve-home-content { endpoint -- }
    s" <html><head><title>Control the LED</title></head>" endpoint add-output
    s" <body>" endpoint add-output
    s" LED On" s" led-on" endpoint serve-button
    s" LED Off" s" led-off" endpoint serve-button
    s" Toggle LED" s" toggle-led" endpoint serve-button
    s" </body></html>" endpoint add-output
  ;

  \ Serve the home page
  : serve-home { endpoint -- }
    endpoint serve-ok
    endpoint serve-home-content
  ;

  \ Serve the LED on page
  : serve-led-on { endpoint -- }
    http-method @ method-post = if
      true my-cyw43-net pico-w-led!
      endpoint serve-home
    else
      endpoint serve-method-not-allowed
    then
  ;

  \ Serve the LED off page
  : serve-led-off { endpoint -- }
    http-method @ method-post = if
      false my-cyw43-net pico-w-led!
      endpoint serve-home
    else
      endpoint serve-method-not-allowed
    then
  ;

  \ Serve the toggle LED page
  : serve-toggle-led { endpoint -- }
    http-method @ method-post = if
      my-cyw43-net toggle-pico-w-led
      endpoint serve-home
    else
      endpoint serve-method-not-allowed
    then
  ;

  \ Add data to the HTTP buffer
  : add-http-buffer { in-buffer in-size -- }
\    in-buffer in-size type
    in-size http-buffer-size http-buffer-offset @ - min { accept-size }
    in-buffer http-buffer http-buffer-offset @ + accept-size move
    accept-size http-buffer-offset +!
  ;

  \ Do a connection close
  : actually-close ( endpoint -- )
    cr ." CLOSING....!"
    my-interface @ close-tcp-endpoint
    cr ." CLOSED!"
    init-http-state
    server-port my-interface @ allocate-tcp-listen-endpoint 2drop
  ;

  \ Schedule a connection close
  : schedule-close ( endpoint -- )
    closing? @ not if
      true closing? !
      1000000 0 rot [: drop actually-close ;]
      my-close-alarm set-alarm-delay-default
    else
      drop
    then
  ;

  \ Force a connection close
  : force-close ( endpoint -- )
    my-close-alarm unset-alarm
    0 0 rot [: drop actually-close ;]
    my-close-alarm set-alarm-delay-default
  ;

  <endpoint-handler> begin-class <tcp-echo-handler>
    
  end-class

  <tcp-echo-handler> begin-implement
    
    \ Handle an HTTP request
    :noname { endpoint self -- }
      endpoint endpoint-tcp-state@ { state }
      state TCP_SYN_RECEIVED = if
        init-http-state
      else
        state TCP_ESTABLISHED = state TCP_CLOSE_WAIT = or if
          endpoint schedule-close
          endpoint endpoint-rx-data@ add-http-buffer
          parse-http-headers if
            http-corrupt? @ if
              endpoint serve-corrupt
            else
              http-dest @ case
                dest-invalid of endpoint serve-invalid endof
                dest-home of endpoint serve-home endof
                dest-led-on of endpoint serve-led-on endof
                dest-led-off of endpoint serve-led-off endof
                dest-toggle-led of endpoint serve-toggle-led endof
              endcase
            then
            endpoint send-output
            endpoint force-close
          then
        else
        then
      then
      endpoint my-interface @ endpoint-done
    ; define handle-endpoint
  
  end-implement
  
  <tcp-echo-handler> class-size buffer: my-tcp-echo-handler
  
  \ Initialize the test
  : init-test { D: ssid D: pass -- }
    pio-addr sm-index pio-instance <pico-w-cyw43-net> my-cyw43-net init-object
    my-cyw43-net cyw43-control@ my-cyw43-control !
    my-cyw43-net net-interface@ my-interface !
    my-cyw43-net init-cyw43-net
    <tcp-echo-handler> my-tcp-echo-handler init-object
    my-tcp-echo-handler
    my-cyw43-net net-endpoint-process@ add-endpoint-handler
    cyw43-consts::PM_AGGRESSIVE my-cyw43-control @ cyw43-power-management!
    begin ssid pass my-cyw43-control @ join-cyw43-wpa2 nip until
    my-cyw43-net run-net-process
    cr ." Discovering IPv4 address..."
    my-interface @ discover-ipv4-addr
    my-interface @ intf-ipv4-addr@ cr ." IPv4 address: " ipv4.
    my-interface @ intf-ipv4-netmask@ cr ." IPv4 netmask: " ipv4.
    my-interface @ gateway-ipv4-addr@ cr ." Gateway IPv4 address: " ipv4.
    my-interface @ dns-server-ipv4-addr@ cr ." DNS server IPv4 address: " ipv4.
    my-cyw43-net toggle-pico-w-led
    init-http-state
  ;

  \ Run the test
  : run-test ( -- )
    init-http-state
    server-port my-interface @ allocate-tcp-listen-endpoint 2drop
  ;

end-module