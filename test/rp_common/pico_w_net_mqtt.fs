\ mqtt demo
begin-module mqtt-demo

  oo import
  simple-cyw43-net import
  pico-w-cyw43-net import
  cyw43-control import
  net import
  endpoint-process import
  net-consts import
  mqtt import

  0 constant pio-addr
  0 constant sm-index
  pio::PIO0 constant pio-instance

  <pico-w-cyw43-net> class-size buffer: my-cyw43-net
  \ 0 value my-cyw43-control
  \ 0 value my-interface
  variable my-cyw43-control
  variable my-interface
  0 value my-outgoing-endpoint

  <mqtt-client> class-size buffer: mqtt-client

  : net-init
    cr ." Network stack initializing..."
    pio-addr sm-index pio-instance <pico-w-cyw43-net> my-cyw43-net init-object
    my-cyw43-net cyw43-control@ my-cyw43-control !
    my-cyw43-net net-interface@ my-interface !
    my-cyw43-net init-cyw43-net 
    
    <mqtt-client> mqtt-client init-object
    mqtt-client my-cyw43-net net-endpoint-process@ add-endpoint-handler
    \ cyw43-consts::PM_AGGRESSIVE my-cyw43-control cyw43-power-management!
  ;

  : connect-wpa2-ap { D: ssid D: pass -- }
    cr ." Connecting to ap..." 
    begin ssid pass my-cyw43-control @ join-cyw43-wpa2 nip until
    my-cyw43-control @ disable-all-cyw43-events
    my-cyw43-control @ clear-cyw43-events    
    cr ." Run net process"
    my-cyw43-net run-net-process
  ;

  : show-info
    cr ." Discovering IPv4 address..."
    my-interface @ discover-ipv4-addr
    my-interface @ intf-ipv4-addr@
    cr ." IPv4 address: " net-misc::ipv4.
    my-interface @ intf-ipv4-netmask@
    cr ." IPv4 netmask: " net-misc::ipv4.
    my-interface @ gateway-ipv4-addr@
    cr ." Gateway IPv4 address: " net-misc::ipv4.
    my-interface @ dns-server-ipv4-addr@
    cr ." DNS server IPv4 address: " net-misc::ipv4.
    my-cyw43-net toggle-pico-w-led
  ;

  \ load stored passwords
  \ 0 load
  \ mqtt test environment
  \ mosquitto server runs on 192.168.1.10:1883 
  \ and defined user/password: muser/mpassword
  \
  \ example: 
  \ s" mysid" s" my-pass" init-demo
  \ 192 168 1 10 net-misc::make-ipv4-addr init-mqtt-demo
  \ run-demo
  : init-demo { D: my-sid D: my-pass }
    net-init my-sid my-pass connect-wpa2-ap show-info
  ;

  : init-mqtt-demo { mqtt-server-ip }
    my-interface @ mqtt-server-ip 1883 mqtt-client init-mqtt-client
  ;

  : run-demo
    s" muser" s" mpassword" mqtt-client credentials!
    cr ." Publishing message started..." 
    s" /mychannel/mytopic" s" hello zeptoforth" 1 mqtt-client publish
  ;

  : run-demo-again { D: topic D: msg -- }
    cr ." Publishing new message started with old credentials"
    topic msg 1 mqtt-client publish
  ;

  : run-demo-fail
    s" badusername" s" badpassword" mqtt-client credentials!
    s" /mychannel/mytopic" s" fail message" 1 mqtt-client publish
  ;

end-module
