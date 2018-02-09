\ various

include tm1638.fs

PC11 variable ssel  \ cs
PC12 constant SCLK  \ clk
PC8  constant MISO  \ do
PD2  constant MOSI  \ di
include ../flib/any/spi-bb.fs

: sd-slow ;
include ../flib/spi/sdcard.fs

PB9  constant LED
PD15 constant BTN1
PD14 constant BTN2
PD13 constant BTN3
PD12 constant BTN4

: led-init  omode-pp LED io-mode!  LED ios! ;
: led-on  LED ioc! ;
: led-off  LED ios! ;

: btn-init ( -- )  \ set up button pull-ups
  IMODE-PULL BTN1 io-mode!  BTN1 ios!
  IMODE-PULL BTN2 io-mode!  BTN2 ios!
  IMODE-PULL BTN3 io-mode!  BTN3 ios!
  IMODE-PULL BTN4 io-mode!  BTN4 ios!  ;

: btn@ ( -- u )  \ bits 0..3 correspond to buttons 1..4 being pressed
  BTN1 io@ 1 and
  BTN2 io@ 2 and or
  BTN3 io@ 4 and or
  BTN4 io@ 8 and or
  $F xor ;

lcd-init show-logo
led-init
btn-init
tm1638-init
tm1638-send
sd-mount.
