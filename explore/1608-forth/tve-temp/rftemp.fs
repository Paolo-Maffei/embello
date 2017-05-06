\ application setup and main loop
\ assumes that the BME280 and TSL4531 sensors are connected to PB6..PB7

300 constant rate     \ seconds between readings
\ 20 constant rate     \ seconds between readings
  1 variable rate-now \ current rate depending on ACK success
  1 variable missed   \ number of consecutive ACKs missed

15 constant margin \ target SNR margin in dB 8dB demod + log10(2*RxBw) + 5dB margin

\ include ../flib/spi/rf69.fs
\ include ../tlib/oled.fs
\ include ../tlib/numprint.fs
\ include ../flib/i2c/tsl4531.fs

: bme-reset ( -- ) \ software reset of the bme280
  BME.ADDR i2c-addr
  $E0 >i2c $B6 
  0 i2c-xfer ;

: bme-init-sleep ( -- nak ) \ init the bme280 into sleep mode
  i2c-init bme-reset
  BME.ADDR i2c-addr
  $F2 >i2c %1 >i2c         \ 1x oversampling of humidity
  $F4 >i2c %100101 >i2c    \ forced mode (takes 1 meas), 1x oversampling of tmp and pressure
  $F5 >i2c %10100000 >i2c  \ filter off, 1sec interval (unused)
  0 i2c-xfer ;

: bme-sleep ( -- ) \ force bme280 to sleep
  BME.ADDR i2c-addr
  $F4 >i2c %100100 >i2c 0 i2c-xfer drop ;

: bme-convert ( -- ms ) \ perform a one-shot forced reading, return ms to sleep
  BME.ADDR i2c-addr
  $F4 >i2c %100101 >i2c    \ forced mode (takes 1 meas), 1x oversampling of tmp and pressure
  0 i2c-xfer drop
  10 ;

: highz-gpio
\ this s(h)aves another 0.6 µA ...
\ IMODE-ADC PA0  io-mode!   \ debug
\ IMODE-ADC PA1  io-mode!   \ debug
  IMODE-ADC PA2  io-mode!
  IMODE-ADC PA3  io-mode!
\ IMODE-ADC PA4  io-mode!   \ SSEL
\ IMODE-ADC PA5  io-mode!   \ SCLK
\ IMODE-ADC PA6  io-mode!   \ MISO
\ IMODE-ADC PA7  io-mode!   \ MOSI
  IMODE-ADC PA8  io-mode!
\ IMODE-ADC PA9  io-mode!   \ uart TX
\ IMODE-ADC PA10 io-mode!   \ uart RX
  IMODE-ADC PA11 io-mode!
  IMODE-ADC PA12 io-mode!
  IMODE-ADC PA13 io-mode!
  IMODE-ADC PA14 io-mode!
\ IMODE-ADC PA15 io-mode!   \ LED
  IMODE-ADC PB0  io-mode!
  IMODE-ADC PB1  io-mode!
  IMODE-ADC PB3  io-mode!
  IMODE-ADC PB4  io-mode!
  IMODE-ADC PB5  io-mode!
\ IMODE-ADC PB6  io-mode!   \ SCL
\ IMODE-ADC PB7  io-mode!   \ SDA
  IMODE-ADC PC14 io-mode!
  IMODE-ADC PC15 io-mode!
;

: low-power-sleep ( n -- ) \ sleeps for n * 100ms
  rf-sleep bme-sleep highz-gpio
  -adc only-msi
  0 do stop100ms loop
  hsi-on adc-init ;

1 variable Vcellar \ lowest VCC measured
: v-cellar adc-vcc Vcellar @ min Vcellar ! ;
: v-cellar-init adc-vcc Vcellar ! ;

: n-flash ( n -- ) \ flash LED n times very briefly (100ms)
  0 ?do
    LED ioc! 1 low-power-sleep
    LED ios! 1 low-power-sleep
  loop ;

: empty-stack ( any -- ) \ checks that the stack is empty, if not flash LED and empty it
  depth ?dup if
    rx-connected? if
      ." *** " . ." left on stack: " .v
      depth
    else
      10 n-flash
    then
    0 do drop loop
  then ;

\ : rx-connected? 0 ;

: c>f ( n -- n ) \ convert celsius to farenheit
  9 * 5 / 32 + ;

: cC>F ( n -- f ) \ convert hundredths of degrees celsius to farenheit fixed-point
  0 swap 0,018 f* 0 32 d+ 
  ;

: show-readings ( vprev vcc tint txpow lux humi pres temp -- ) \ print readings on console
  hwid hex. ." = "
  dup cC>F 4 1 f.n.m ." °F, "
  1 pick . ." Pa, "
  2 pick .centi ." %RH, "
  3 pick . ." lux, "
  4 pick . ." dBm, "
  5 pick c>f .n ." °F, "
  6 pick .milli ." => "
  7 pick .milli ." V "
  ;

: rf>uart ( len -- len )  \ print reception parameters
  rf.freq @ .n ." Hz "
  rf.rssi @ 2/ negate .n ." dBm "
  rf.afc @ 16 lshift 16 arshift 61 * .n ." Hz "
  dup .n ." b "
  ;

: <pkt ( -- ) pkt.buf pkt.ptr ! ;  \ start collecting values for the packet
: pkt> ( format -- c-addr len )    \ encode the collected values as RF packet
  \ ." PKT> " pkt.ptr @  begin  4 - dup @ . dup pkt.buf u<= until  drop dup . cr
  <v
    pkt.ptr @  begin  4 - dup @ >var  dup pkt.buf u<= until  drop
    hold
  v> ;

: send-packet ( vprev vcc tint txpow lux humi pres temp -- )
  <pkt  hwid 9 0 do >+pkt loop  2 pkt>
  PA0 ios!
  $80 rf-send \ request ack
  v-cellar ;

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
: rate!normal rate rate-now ! ;           \ set normal rate
: rate!fast rate 3 rshift 1+ rate-now ! ; \ set fast rate (8x)
: rate!slow rate 2 lshift rate-now ! ;    \ set slow rate (0.25x)

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
  PA0 ioc!
  v-cellar rf-sleep
  ?dup if process-ack
  else missed @ ?dup if
    8 > if no-ack-slow else no-ack-continue then
  else no-ack-repeat
  then then
  PA0 ios! ;

: init-hw
  2.1MHz 1000 systick-hz
  lptim-init i2c-init adc-init

  OMODE-PP PA0 io-mode! \ for debugging
  OMODE-PP PA1 io-mode! \ for debugging

  912500000 rf.freq ! 6 rf.group ! \ 61 rf.nodeid !
  rf-init $0F rf-power \ rf. cr

  bme-init drop bme-calib
  tsl-init drop tsl-sleep

  v-cellar-init
  ;

: iter
  Vcellar @                    ( vprev )
  v-cellar-init
  adc-vcc adc-temp             ( vprev vcc tint )
  rf@power 18 -                ( vprev vcc tint txpow )
  PA0 ios!
  tsl-convert 100 / 1+ low-power-sleep tsl-data
  bme-convert 100 / 1+ low-power-sleep bme-data bme-calc
  ( vprev vcc tint txpow lux humi pres temp )

  rx-connected? if show-readings cr 1 ms then
  PA0 ioc!
  send-packet
  PA0 ios!
  get-ack
  empty-stack
  v-cellar
  ;

: lpt \ low power test
  ." ... low power test..." cr
  init-hw LED ios!
  rf-sleep
  begin
    PA0 ios! 10 ms PA0 ioc!
    10 low-power-sleep
  again
  ;

: main
  ." ... starting rftemp..." cr
  init-hw LED ios!
  PA1 ios!
  begin
    iter
    PA1 ioc!
    rate-now @ 10 * low-power-sleep
    OMODE-PP PA0 io-mode! \ for debugging
    OMODE-PP PA1 io-mode! \ for debugging
    PA1 ios!
  key? until ;
