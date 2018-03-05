\ core definitions

\ <<<board>>>
compiletoflash
( core start: ) here hex.

PC0 constant LED1
PC1 constant LED2
PC2 constant LED3
PC3 constant LED4
PC4 constant LED5
PC5 constant LED6
PC6 constant LED7
PC7 constant LED8

: led-init ( -- )
  8 0 do OMODE-OD  i LED1 +  dup ios!  io-mode! loop ;

PB5 constant SPEAKER

: speaker-init ( -- )
  OMODE-PP SPEAKER io-mode! ;
: beep ( -- )
  1000 0 do pb5 iox! 500 us loop ;

PA0 constant KEY-UP
PE2 constant KEY-LEFT
PE3 constant KEY-DOWN
PE4 constant KEY-RIGHT

: key-init ( -- )
  IMODE-PULL KEY-UP     dup ioc!  io-mode!  \ has external pull-up
  IMODE-PULL KEY-LEFT   dup ios!  io-mode!
  IMODE-PULL KEY-DOWN   dup ios!  io-mode!
  IMODE-PULL KEY-RIGHT  dup ios!  io-mode! ;

: key-demo ( -- )
  key-init led-init
  begin
    KEY-UP    io@ LED1 io!
    KEY-LEFT  io@ LED2 io!
    KEY-DOWN  io@ LED3 io!
    KEY-RIGHT io@ LED4 io!
  key? until
  LED1 ios! ;

\ these re-definitions will switch the flash memory driver to SPI2
: +spi +spi2 inline ;
: -spi -spi2 inline ;
: >spi >spi2 inline ;
: spi> spi2> inline ;

: xspi2-init ( -- )  \ configure all the chip select pins
  OMODE-PP PG13  dup ios!  io-mode!  \ spi flash
  OMODE-PP PG14  dup ios!  io-mode!  \ sd card
  spi2-init ;

include ../flib/spi/smem.fs

: smem-demo ( -- )
  PG13 ssel2 ! ." ID: " smem-id hex. ." , " smem-size . ." kB " ;

: fsmc-pins ( -- )
  8 bit RCC-AHBENR bis!  \ enable FSMC clock
  OMODE-AF-PP OMODE-FAST + dup PD0 %1111111100110011 io-modes!
                           dup PE0 %1111111111000011 io-modes!
                           dup PF0 %1111000000111111 io-modes!
                               PG0 %0001010000111111 io-modes! ;

\ these two drivers use the FSMC for memory-mapped access
include sram.fs
include tft.fs

\ now the graphics code can be added
include ../flib/mecrisp/graphics.fs
\ include ../flib/any/digits.fs

\ some more utility code
include ../flib/mecrisp/quotation.fs
include ../flib/mecrisp/multi.fs
include ../flib/any/timed.fs

: init init led-init speaker-init key-init i2c-init ( smem-init )
            fsmc-pins sram-init tft-init ;

cornerstone <<<core>>>
hello

\ PG13 ssel2 !
\ spi2-init
\ smem-id .
\ smem-size .
