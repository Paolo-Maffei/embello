\ core libraries

<<<board>>>
cr
compiletoflash
( core start: ) here dup hex.

\ PA5  constant LED  \ don't use, shared with SCLK
PC13 constant BUTTON

 \ SX1272 radio
 PB6  constant NSS
 PA7  constant MOSI
 PA6  constant MISO
 PA5  constant SCLK
 PA0  constant RST
 PA10 constant DIO0
 PB3  constant DIO1
 PB5  constant DIO2
 PB4  constant DIO3

\ Grove UART
 PA3  constant GRX
 PA2  constant GTX
\ Grove I2C
 PB8  constant SCL
 PB9  constant SDA
\ Grove ANA A1
 PA1  constant GA1
 PA4  constant GA2
\ Grove ANA A3
 PB0  constant GA3
 PC1  constant GA4
\ Grove DIO D6
 PB10 constant GD6
 PA8  constant GD7
\ Grove DIO D6
 PA9  constant GD8
 PC7  constant GD9

include ../flib/any/i2c-bb.fs
include ../flib/stm32l0/spi.fs

include ../flib/mecrisp/quotation.fs
include ../flib/mecrisp/multi.fs
include ../flib/any/timed.fs

( core end, size: ) here dup hex. swap - .
cornerstone <<<core>>>
compiletoram
