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

: key-demo
  key-init led-init
  begin
    KEY-UP    io@ LED1 io!
    KEY-LEFT  io@ LED2 io!
    KEY-DOWN  io@ LED3 io!
    KEY-RIGHT io@ LED4 io!
  key? until ;

include ../flib/mecrisp/quotation.fs
include ../flib/mecrisp/multi.fs
include ../flib/any/timed.fs

cornerstone <<<core>>>
hello
