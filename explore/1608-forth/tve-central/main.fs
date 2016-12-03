\ tve-central node
\ receives packets via rf69
\ prints packets on serial (usb)
\ displays packet on oled

\ include ../flib/spi/rf69.fs
include ../tlib/oled.fs
include ../tlib/numprint.fs
include ../flib/any/varint.fs

: fault-anykey ( -- ) unhandled key drop ;

: led-on LED ioc! ;
: led-off LED ios! ;

lcd-init show-logo
10 ms

: readings>uart ( vprev vcc tint lux humi pres temp -- ) \ print readings on console
  var-init
  var> if . then
  var> if hex. then
  var-s> if .centi ." °C, " then
  var> if . ." Pa, " then
  var> if .centi ." %RH, " then
  var> if . ." lux, " then
  var> if . ." °C, " then
  var> if .milli ." => " then
  var> if .milli ." V " then
  ;

: readings>oled ( temp -- temp ) \ show the temperature on the OLED and return it again
  clear
  var-init
  var>
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

: rf69-info ( -- )  \ display reception parameters as hex string
  rf69.freq @ .milli ." khz "
  rf69.group @ .n ." g "
  rf.rssi @ rf69.rssi-db 0 1 f.n.m ." dBm "
  rf.lna @ rf69.lna-db .n ." dB "
  rf.afc @ rf69.fdev-hz 0 swap 5 0 f.n.m ." Hz "
  dup .n ." bytes"
  ;

\ print a string to graphics
: string>gr ( addr u -- )
  0 ?do
    dup c@ ascii>bitpattern drawcharacterbitmap 1+
  loop  drop ;

: rf69>oled ( -- )  \ display reception parameters on oled
  0 font-x ! 8 font-y !
  \ rf69.freq @ >milli string>gr s" khz " string>gr
  rf69.group @ >n string>gr s" g " string>gr
  rf.rssi @ rf69.rssi-db 0 1 f>n.m string>gr s" dBm " string>gr
  rf.lna @ rf69.lna-db >n string>gr s" dB " string>gr
  \ 0 font-x ! 18 font-y !
  \ rf.afc @ rf69.fdev-hz 5 0 s>n string>gr s" Hz " string>gr
  .v
  ;

: rf69-listen ( -- )  \ init RFM69 and report incoming packets until key press
  rf69-init cr
  0 rf.last !
  RF:M_FS rf!mode
  begin
    rf-recv ?dup if
      ." RF69 " rf69-info
      dup 0 do
        rf.buf i + c@ h.2
        i 1 = if 2- h.2 space then
      loop  cr
    then
  key? until ;

: rf69-listenv ( -- )  \ init RFM69 and report incoming packets until key press
  rf69-init cr
  0 rf.last !
  RF:M_FS rf!mode
  begin
    rf-recv ?dup if
      ." RF69 " rf69-info
      clear rf69>oled display
      ( len ) dup 0 do
        rf.buf i + c@ h.2
        i 1 = if dup 2- h.2 space then
      loop  cr
      ( len ) 5 spaces rf.buf 2+ swap 2-
      readings>uart cr
      \ show-readings-oled
    then
  key? until ;

: l ( -- )  \ init RFM69 and report incoming packets until key press
  rf69-init cr
  0 rf.last !
  RF:M_FS rf!mode
  begin
    rf-recv ?dup if
      ." RF69 " rf69-info
      clear rf69>oled display
\      ( len ) dup 0 do
\        rf.buf i + c@ h.2
\        i 1 = if dup 2- h.2 space then
\      loop  cr
\      drop
      .v
      \ ( len ) 5 spaces rf.buf 2+ swap 2-
      \ readings>uart cr
      \ show-readings-oled
    then
  key? until ;

." ready!" cr 10 ms
\ rf69-listen


