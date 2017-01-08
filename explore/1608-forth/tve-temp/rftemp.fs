\ application setup and main loop
\ assumes that the BME280 and TSL4531 sensors are connected to PB6..PB7
\ assumes OLED on i2c

0 constant debug  \ 0 = send RF packets, 1 = display on serial port
1 constant rate   \ seconds between readings
0 variable OLED   \ whether OLED is present

\ include ../flib/spi/rf69.fs
\ include ../tlib/oled.fs
\ include ../tlib/numprint.fs

: c>f ( n -- n ) \ convert celsius to farenheit
  9 * 5 / 32 +
  ;

: cC>F ( n -- f ) \ convert hundredths of degrees celsius to farenheit
  0 swap 0,018 f* 0 32 d+ 
  ;

: show-readings ( vprev vcc tint lux humi pres temp -- ) \ print readings on console
  hwid hex. ." = "
  dup cC>F 4 1 f.n.m ." °F, "
  1 pick . ." Pa, "
  2 pick .centi ." %RH, "
  3 pick . ." lux, "
  4 pick c>f .n ." °F, "
  5 pick .milli ." => "
  6 pick .milli ." V "
  ;

: show-oled ( temp -- temp ) \ show the temperature on the OLED and return it again
  dup cC>F 10,0 f* shownum3.1 drop
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
  2 <pkt  hwid 8 0 do n+> loop  pkt>rf ;

: low-power-sleep
  rf69-sleep
  -adc \ only-msi
  rate 0 do stop1s loop
  hsi-on adc-init ;

: init-hw
  2.1MHz 1000 systick-hz
  lptim-init i2c-init adc-init

  OMODE-PP PA0 io-mode!
  OMODE-PP PA1 io-mode!

  915750 rf69.freq ! 6 rf69.group ! 62 rf69.nodeid !
  rf69-init 16 rf69-power

  bme-init drop bme-calib
  tsl-init drop
  lcd? OLED !
  OLED if lcd-init show-logo then

  adc-vcc                      ( vprev )
  ;

: iter
  adc-vcc adc-temp             ( vprev vcc tint )
  tsl-data  bme-data bme-calc  ( vprev vcc tint lux humi pres temp )

  OLED if show-oled then

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
  begin
    low-power-sleep
    iter
  key? until ;
