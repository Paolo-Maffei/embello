\ application setup and main loop
\ fan control


: show-readings ( vprev vcc tint txpow temp -- ) \ print readings on console
  hwid hex. ." = "
  dup cC>F 4 1 f.n.m ." °F, "
  1 pick . ." dBm, "
  2 pick c>f .n ." °F, "
  3 pick .milli ." => "
  4 pick .milli ." V "
  ;

: <pkt ( -- ) pkt.buf pkt.ptr ! ;  \ start collecting values for the packet
: pkt> ( format -- c-addr len )    \ encode the collected values as RF packet
  \ ." PKT> " pkt.ptr @  begin  4 - dup @ . dup pkt.buf u<= until  drop dup . cr
  <v
    pkt.ptr @  begin  4 - dup @ >var  dup pkt.buf u<= until  drop
    hold
  v> ;

: send-packet ( vprev vcc tint txpow temp -- )
  <pkt  hwid 6 0 do >+pkt loop  2 pkt>
  $80 rf-send \ request ack
  v-cellar ;



: init-hw
  4 rate !     \ seconds between readings
  lptim-init i2c-init adc-init

  OMODE-PP PA0 io-mode! \ for debugging
  OMODE-PP PA1 io-mode! \ for debugging

  912500000 rf.freq ! 6 rf.group ! 3 rf.nodeid !
  rf-init $0F rf-power \ rf. cr

  mcp9808-init drop

  v-cellar-init
  ;

: high-power-sleep ( n -- ) 100 * ms ;

0 variable last-status

: time-for-status?
  millis last-status @ - rate-now @ 1000 * u>
  ;

: send-status
  Vcellar @                    ( vprev )
  v-cellar-init
  adc-vcc adc-temp             ( vprev vcc tint )
  rf@power 18 -                ( vprev vcc tint txpow )
  mcp9808-data
  ( vprev vcc tint txpow temp )

  rx-connected? if show-readings cr then
  send-packet
  get-ack
  millis last-status !
  empty-stack
  v-cellar
  ;

: check-recv
  rf-recv ?dup if
    ." recv " dup . cr
  then
  ;

: main
  ." ... starting fanctrl ..." cr
  ['] high-power-sleep *lps !
  init-hw
  send-status
  begin
    time-for-status? if
      send-status
    else
      check-recv
    then
    1 low-power-sleep
  key? until ;
