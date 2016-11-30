\ application setup and main loop
\ assumes that the BME280 and TSL4531 sensors are connected to PB6..PB7
\ assumes OLED on i2c

0 constant debug  \ 0 = send RF packets, 1 = display on serial port
1 constant rate  \ seconds between readings

\ include ../flib/spi/rf69.fs
include ../tlib/oled.fs

: show-oled ( temp -- temp )
  dup shownum2.2 
  100 ms
  5 pick shownum1.3
  ;

: roll
  1234
  begin
    dup shownum2.2
    1111 + 10000 mod
  key? until ;

: send-packet ( vprev vcc tint lux humi pres temp -- )
  2 <pkt  hwid u+>  u14+> 6 0 do u+> loop  pkt>rf ;

: low-power-sleep
  rf-sleep
  -adc \ only-msi
  rate 0 do stop1s loop
  hsi-on +adc ;

: init-hw
  2.1MHz  1000 systick-hz  +lptim +i2c +adc

  915750 rf69.freq ! 6 rf69.group ! 62 rf69.nodeid !
  rf69-init 16 rf-power

  bme-init drop bme-calib
  tsl-init drop
  lcd-init show-logo

  adc-vcc                      ( vprev )
  ;

: iter
  adc-vcc adc-temp             ( vprev vcc tint )
  tsl-data  bme-data bme-calc  ( vprev vcc tint lux humi pres temp )

  led-on
  show-oled
  show-readings cr 1 ms
  send-packet
  1 ms
  led-off 

  adc-vcc

\     debug if
\       show-readings cr 1 ms
\     else
\       send-packet
\     then

  ;

: main
  init-hw
  begin
    \ low-power-sleep

    iter
    200 ms

  key? until ;
