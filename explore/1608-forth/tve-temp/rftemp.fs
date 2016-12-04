\ application setup and main loop
\ assumes that the BME280 and TSL4531 sensors are connected to PB6..PB7
\ assumes OLED on i2c

0 constant debug  \ 0 = send RF packets, 1 = display on serial port
1 constant rate  \ seconds between readings

\ include ../flib/spi/rf69.fs
include ../tlib/oled.fs
include ../tlib/numprint.fs

: show-readings ( vprev vcc tint lux humi pres temp -- ) \ print readings on console
  hwid hex. ." = "
  dup .centi ." °C, "
  1 pick . ." Pa, "
  2 pick .centi ." %RH, "
  3 pick . ." lux, "
  4 pick . ." °C, "
  5 pick .milli ." => "
  6 pick .milli ." V "
  ;

: show-oled ( temp -- temp ) \ show the temperature on the OLED and return it again
  dup shownum2.2 
  \ 100 ms
  \ 5 pick shownum1.3
  ;

: roll \ little test snippet that rolls through numbers on the OLED at full speed
  1234
  begin
    dup shownum2.2
    1111 + 10000 mod
  key? until ;

: send-packet ( vprev vcc tint lux humi pres temp -- )
  2 <pkt  hwid u+>  n+> 6 0 do u+> loop  pkt>rf ;

: low-power-sleep
  rf-sleep
  -adc \ only-msi
  rate 0 do stop1s loop
  hsi-on +adc ;

: init-hw
  2.1MHz  1000 systick-hz  +lptim +i2c +adc
  ." a"

  OMODE-PP PA0 io-mode!
  OMODE-PP PA1 io-mode!
  PA1 ioc! 1 ms
  PA1 ios! 1 ms
  PA1 ioc!

  915750 rf69.freq ! 6 rf69.group ! 62 rf69.nodeid !
  rf69-init 16 rf-power
  ." b"

  bme-init drop bme-calib
  ." c"
  tsl-init drop
  ." d"
  lcd-init ." e" show-logo
  ." f"

  adc-vcc                      ( vprev )
  ;

: iter
  adc-vcc adc-temp             ( vprev vcc tint )
  tsl-data  bme-data bme-calc  ( vprev vcc tint lux humi pres temp )

  ." A"
  show-oled
  ." X"
  show-readings cr 1 ms
  send-packet
  1 ms

  adc-vcc

\     debug if
\       show-readings cr 1 ms
\     else
\       send-packet
\     then

  ;

: main
  ." ... starting rftemp..." cr
  init-hw
  ." ..." cr
  begin
    \ low-power-sleep

    iter
    200 ms

  key? until ;
