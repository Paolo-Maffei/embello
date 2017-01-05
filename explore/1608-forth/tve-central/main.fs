\ tve-central node
\ receives packets via rf69
\ prints packets on serial (usb)
\ displays packet on oled

\ include ../flib/spi/rf69.fs
\ include ../tlib/oled.fs
\ include ../tlib/numprint.fs
\ include ../flib/any/varint.fs

: fault-anykey ( -- ) unhandled ." continue?" key drop ;
' fault-anykey irq-fault !
' fault-anykey irq-collection !

: led-on LED ioc! ;
: led-off LED ios! ;

: h>n ( u n -- ) \ convert hex to n digit string
  base @ hex -rot ( base u n )
  0 swap ( base u 0 n )
  <# 0 do # loop #> ( base c-addr len )
  rot base !
  ;

lcd-init show-logo
10 ms

\ print a string to graphics
: >gr ( addr u -- )
  0 ?do
    dup c@ ascii>bitpattern drawcharacterbitmap 1+
  loop  drop ;

: c>f ( n -- n ) \ convert celsius to farenheit
  9 * 5 / 32 +
  ;

: cC>F ( n -- c-addr len ) \ convert hundredths of degrees celsius to farenheit
  0 swap 0,018 f* 0 32 d+ 
  ;

: readings>uart ( buf-addr buf-len -- ) \ print readings on console
  var-init
  var> if ." #" . then
  var> if hex. ." :  " then
  var-s> if cC>F 4 1 f.n.m ." °F " then
  var> if 0 u.n ." Pa " then
  var> if .centi ." %RH " then
  var> if . ." lux " then
  var> if c>f .n ." °F " then
  var> if .milli ." =>" then
  var> if .milli ." V " then
  ;

: readings>oled ( buf-addr buf-len -- ) \ show readings on oled
  var-init
  64 font-x ! 8 font-y !
  \ pkt format, source node
  var> if drop then \ s" #" >gr >n >gr then
  var> if s"  " >gr 8 h>n >gr then
  \ temp, %rh
  0 font-x ! 40 font-y !
  var-s> if cC>F 4 1 f>n.m >gr s" F " >gr then
  var> if drop then \ 0 u.n ." Pa " then
  var> if >centi >gr s" %" >gr then
  0 font-x ! 50 font-y !
  var> if >n >gr s" lux " >gr then 
  var> if drop then \ . ." °C " then
  var> if drop then \ .milli ." =>" then
  var> if >milli >gr s" V" >gr then
  ;

915750 rf69.freq ! 6 rf69.group ! \ 62 rf69.nodeid !
rf69-init 16 rf-power

: rf69.rssi-db ( u -- f ) \ convert rssi register value to dB
  0 swap d2/ dnegate
  ;

: rf69.lna-db ( u -- u ) \ convert lna register value to dB
  case 
  1 of 0 endof
  2 of -6 endof
  3 of -12 endof
  4 of -24 endof
  5 of -36 endof
  6 of -48 endof
  0
  endcase
  ;

: rf69.fdev-hz ( u -- u ) \ convert freq deviation (ln a or afc) to Hz
  16 lshift 16 arshift 61 * \ 16-bit to 32-bit sign-extension
  ;

: rf69>uart ( len -- len )  \ print reception parameters
  rf69.freq @ .milli ." khz "
  ." g" rf69.group @ .
  rf.rssi @ rf69.rssi-db 0 1 f.n.m ." dBm "
  rf.lna @ rf69.lna-db .n ." dB "
  rf.afc @ rf69.fdev-hz 0 swap 5 0 f.n.m ." Hz "
  dup .n ." b "
  ;

: rf69>oled ( len -- len )  \ display reception parameters on oled
  0 font-x ! 8 font-y !
  \ group : source node -> dest node, length
  rf69.group @ >n >gr s" :" >gr
  rf.buf 1+ c@ $3F and >n >gr s" >" >gr
  rf.buf c@ $3F and >n >gr
  s"  L" >gr dup >n >gr
  \ dBm, fDev
  0 font-x ! 18 font-y !
  rf.rssi @ rf69.rssi-db 5 1 f>n.m >gr s" dBm " >gr
  rf.afc @ rf69.fdev-hz 0 swap 5 0 f>n.m >gr s" Hz " >gr
  ;

: rf69-listen ( -- )  \ init RFM69 and report incoming packets until key press
  rf69-init cr
  0 rf.last !
  RF:M_FS rf!mode
  begin
    rf-recv ?dup if
      ." RF69 " rf69>uart
      clear rf69>oled display
      ( len ) dup 0 do
        rf.buf i + c@ h.2
        i 1 = if dup 2- h.2 space then
      loop  cr
      ( len ) 5 spaces rf.buf 2+ swap 2-
      2dup readings>uart cr
      readings>oled display
    then
  key? until ;

." ready!" cr 10 ms
\ rf69-listen


