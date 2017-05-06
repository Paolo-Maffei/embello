\ misc utilities that come in handy when coding the RF rx/tx loop

\ ===== misc stuff

: n-flash ( n -- ) \ flash LED n times very briefly (100ms)
  0 ?do
    LED ioc! 1 low-power-sleep
    LED ios! 1 low-power-sleep
  loop ;

: empty-stack ( any -- ) \ checks that the stack is empty, if not flash LED and empty it
  depth if
    rx-connected? if
      ." *** " depth . ." left on stack: " .v
    else
      10 n-flash
    then
    depth 100 u< if
      depth 0 do drop loop
    then
  then ;

: rf>uart ( len -- len )  \ print reception parameters
  rf.freq @ .n ." Hz "
  rf.rssi @ 2/ negate .n ." dBm "
  rf.afc @ 16 lshift 16 arshift 61 * .n ." Hz "
  dup .n ." b "
  ;

\ ===== RF ack processing and power adjustment

 15 constant margin   \ target SNR margin in dB [8dB demod + log10(2*RxBw) + 5dB margin]
  1 variable rate-now \ current rate depending on ACK success
  0 variable missed   \ number of consecutive ACKs missed
300 variable rate     \ seconds between readings

0 variable Vcellar                           \ lowest VCC measured
: v-cellar adc-vcc Vcellar @ min Vcellar ! ; \ measure and update
: v-cellar-init adc-vcc Vcellar ! ;          \ measure and init

: rf@power ( -- n ) RF:PA rf@ $1F and ;

: rf-adj-power ( snr )
  rx-connected? if ." SNR " dup .  then
  rf@power swap
  margin - 0 > if 2-  0 max  \ SNR > margin
             else 2+ 31 min  \ SNR <= margin
  then
  rx-connected? if ."  POW " dup . cr then
  rf-power ;

: rf-up-power ( -- ) \ increase power one step due to missed ACK
  rf@power 1+ 31 min
  rx-connected? if ." POW++ " dup . cr then
  rf-power ;

: rf-toggle-power ( -- ) \ toggle max/med power so we don't overrun RX by sticking to max
  rf@power $10 xor $F or
  rx-connected? if ." POW<> " dup . cr then
  rf-power ;

: missed++ missed dup @ 1+ swap ! ;
: rate!normal rate @ rate-now ! ;           \ set normal rate
: rate!fast rate @ 3 rshift 1+ rate-now ! ; \ set fast rate (8x)
: rate!slow rate @ 2 lshift rate-now ! ;    \ set slow rate (0.25x)

: no-ack-repeat ( -- ) \ first missed ACK, quickly repeat
  rf-up-power rate!fast missed++ ;
: no-ack-continue ( -- ) \ missed a few ACKs, raise power
  rf-up-power rate!normal missed++ ;
: no-ack-slow ( -- ) \ missed a pile of ACKs, toggle high/med power and go slow
  rf-toggle-power rate!slow missed++ ;

: process-ack ( n -- )
    rx-connected? if              \ print info if connected
      ." RF69 " rf>uart
      dup 0 do rf.buf i + c@ h.2 loop cr
    then
    if rf.buf c@ ?dup if          \ fetch SNR
      rf-adj-power                \ adjust power
    then then
    rf-correct                    \ correct frequency
    rate!normal
    0 missed !
    LED ioc! 1 low-power-sleep LED ios!     \ brief LED blink
    ;

: get-ack ( -- ) \ wait a bit to receive an ACK, adjust rate-now accordingly
  40 rf-ack?
  v-cellar rf-sleep
  ?dup if process-ack
  else missed @ ?dup if
    8 > if no-ack-slow else no-ack-continue then
  else no-ack-repeat
  then then ;
