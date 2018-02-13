\ board definitions

\ eraseflash
compiletoflash
( board start: ) here dup hex.

\ emulate c, which is not available in hardware on some chips.
\ copied from Mecrisp's common/charcomma.txt
0 variable c,collection

: c, ( c -- )  \ emulate c, with h,
  c,collection @ ?dup if $FF and swap 8 lshift or h,
                         0 c,collection !
                      else $100 or c,collection ! then ;

: calign ( -- )  \ must be called to flush after odd number of c, calls
  c,collection @ if 0 c, then ;

: jtag-deinit ( -- )  \ disable JTAG on PB3 PB4 PA15
  25 bit AFIO-MAPR bis! ;
: swd-deinit ( -- )  \ disable JTAG as well as PA13 and PA14
  AFIO-MAPR @ %111 24 lshift bic 26 bit or AFIO-MAPR ! ;

: list ( -- )  \ list all words in dictionary, short form
  cr dictionarystart begin
      dup 6 + ctype space
        dictionarynext until drop ;

include ../flib/mecrisp/calltrace.fs
include ../flib/mecrisp/cond.fs

\ deal with a missing definition, needed by adc.fs, possibly others
[ifndef] RCC-AHBENR  RCC $14 + constant RCC-AHBENR  [then]

include ../flib/mecrisp/hexdump.fs
include ../flib/stm32f1/clock.fs
include ../flib/stm32f1/io.fs
include ../flib/pkg/pins64.fs
include ../flib/stm32f1/spi.fs
include ../flib/any/i2c-bb.fs
include ../flib/stm32f1/timer.fs
include ../flib/stm32f1/pwm.fs
include ../flib/stm32f1/adc.fs
include ../flib/stm32f1/rtc.fs

0 constant OLED.LARGE  \ display size: 0 = 128x32, 1 = 128x64 (default)

: hello ( -- ) flash-kb . ." KB <g6u> " hwid hex.
  $10000 compiletoflash here -  flashvar-here compiletoram here -
  ." ram/flash: " . . ." free " ;

: init ( -- )  \ board initialisation
  init  \ this is essential to start up USB comms!
  ['] ct-irq irq-fault !  \ show call trace in unhandled exceptions
  jtag-deinit  \ disable JTAG, we only need SWD
  1000 systick-hz
\ hello ." ok." cr
;

cornerstone <<<board>>>
hello
