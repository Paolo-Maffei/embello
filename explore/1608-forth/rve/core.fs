\ core definitions

\ <<<board>>>
compiletoflash
( core start: ) here hex.

include ../flib/mecrisp/quotation.fs
include ../flib/mecrisp/multi.fs
include ../flib/any/timed.fs

PC11 variable ssel  \ cs
PC12 constant SCLK  \ clk
PC8  constant MISO  \ do
PD2  constant MOSI  \ di
include ../flib/any/spi-bb.fs

: sd-slow ;
include ../flib/spi/sdcard.fs

PB9  constant LED

: led-init  omode-pp LED io-mode!  LED ios! ;
: led-on  LED ioc! ;
: led-off  LED ios! ;

cornerstone <<<core>>>
hello
