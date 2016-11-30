\ tve-central node
\ receives packets via rf69
\ prints packets on serial (usb)
\ displays packet on oled

\ include ../flib/spi/rf69.fs
include ../tlib/oled.fs

: led-on LED ioc! ;
: led-off LED ios! ;

lcd-init show-logo
10 ms

915750 rf69.freq ! 6 rf69.group ! \ 62 rf69.nodeid !
rf69-init 16 rf-power

: rf69.rssi-db ( u -- u ) \ convert rssi register value to dB
  2 / negate
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

: .n ( n -- ) \ print signed integer without following space
  s>d <# #S #> type
  ;

: .centi ( n -- ) \ print signed int divided by 100 with 2 decimals without following space
  s>d 100,0 f/ \ convert to fixed point
  tuck dabs 0 <# #s \ print whole part
  [char] . hold<
  rot f# f# drop \ print decimal digits
  rot sign
  #> type
  ;

: .milli ( n -- ) \ print signed int divided by 100 with 2 decimals without following space
  s>d 1000,0 f/ \ convert to fixed point
  tuck dabs 0 <# #s \ print whole part
  [char] . hold<
  rot f# f# f# drop \ print decimal digits
  rot sign
  #> type
  ;

: u.n ( u n -- ) \ print u using n characters (leading space)
  <# 0 rot ( n f )
  begin \ output digits
    rot 1- -rot \ decrement n
    # d0= \ output digit, stop when zero
  until
  begin \ output leading spaces
    rot 1- 0 > while
    bl hold
  repeat
  #> type
  ;

: f.n ( f64 n m -- ) \ prints fixed-point double f64 using n int digits and m decimal digits
  3 pick rot ( f64whole n )
  u.n



  ;

: rf69-info ( -- )  \ display reception parameters as hex string
  rf69.freq @ .milli ." khz "
  rf69.group @ .n ." g "
  rf.rssi @ rf69.rssi-db .n ." dBm "
  rf.lna @ rf69.lna-db .n ." dB "
  rf.afc @ rf69.fdev-hz .n ." Hz "
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

." ready!" cr 10 ms
\ rf69-listen
